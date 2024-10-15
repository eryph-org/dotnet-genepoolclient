// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using Azure.Core;

namespace Eryph.GenePool.Client.Internal;

internal class RawRequestUriBuilder: RequestUriBuilder
{
    private const string SchemeSeparator = "://";
    private const char HostSeparator = '/';
    private const char PortSeparator = ':';
    private static readonly char[] HostOrPort = [HostSeparator, PortSeparator];
    private const char QueryBeginSeparator = '?';
    private const char QueryContinueSeparator = '&';
    private const char QueryValueSeparator = '=';

    private RawWritingPosition? _position;

    private static void GetQueryParts(ReadOnlySpan<char> queryUnparsed, out ReadOnlySpan<char> name, out ReadOnlySpan<char> value)
    {
        var separatorIndex = queryUnparsed.IndexOf(QueryValueSeparator);
        if (separatorIndex == -1)
        {
            name = queryUnparsed;
            value = ReadOnlySpan<char>.Empty;
        }
        else
        {
            name = queryUnparsed[..separatorIndex];
            value = queryUnparsed[(separatorIndex + 1)..];
        }
    }

    public void AppendRaw(string value, bool escape)
    {
        AppendRaw(value.AsSpan(), escape);
    }

    private void AppendRaw(ReadOnlySpan<char> value, bool escape)
    {
        if (_position == null)
        {
            if (HasQuery)
            {
                _position = RawWritingPosition.Query;
            }
            else if (HasPath)
            {
                _position = RawWritingPosition.Path;
            }
            else if (!string.IsNullOrEmpty(Host))
            {
                _position = RawWritingPosition.Host;
            }
            else
            {
                _position = RawWritingPosition.Scheme;
            }
        }

        while (!value.IsEmpty)
        {
            switch (_position)
            {
                case RawWritingPosition.Scheme:
                {
                    var separator = value.IndexOf(SchemeSeparator.AsSpan(), StringComparison.InvariantCultureIgnoreCase);
                    if (separator == -1)
                    {
                        Scheme += value.ToString();
                        value = ReadOnlySpan<char>.Empty;
                    }
                    else
                    {
                        Scheme += value[..separator].ToString();
                        // TODO: Find a better way to map schemes to default ports
                        Port = string.Equals(Scheme, "https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
                        value = value[(separator + SchemeSeparator.Length)..];
                        _position = RawWritingPosition.Host;
                    }

                    break;
                }
                case RawWritingPosition.Host:
                {
                    var separator = value.IndexOfAny(HostOrPort);
                    if (separator == -1)
                    {
                        if (!HasPath)
                        {
                            Host += value.ToString();
                            value = ReadOnlySpan<char>.Empty;
                        }
                        else
                        {
                            // All Host information must be written before Path information
                            // If Path already has information, we transition to writing Path
                            _position = RawWritingPosition.Path;
                        }
                    }
                    else
                    {
                        Host += value[..separator].ToString();
                        _position = value[separator] == HostSeparator ? RawWritingPosition.Path : RawWritingPosition.Port;
                        value = value[(separator + 1)..];
                    }

                    break;
                }
                case RawWritingPosition.Port:
                {
                    var separator = value.IndexOf(HostSeparator);
                    if (separator == -1)
                    {
#if NETCOREAPP2_1_OR_GREATER
                        Port = int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
#else
                        Port = int.Parse(value.ToString(), CultureInfo.InvariantCulture);
#endif
                        value = ReadOnlySpan<char>.Empty;
                    }
                    else
                    {
#if NETCOREAPP2_1_OR_GREATER
                        Port = int.Parse(value.Slice(0, separator), NumberStyles.Integer, CultureInfo.InvariantCulture);
#else
                        Port = int.Parse(value[..separator].ToString(), CultureInfo.InvariantCulture);
#endif
                        value = value[(separator + 1)..];
                    }
                    // Port cannot be split (like Host), so always transition to Path when Port is parsed
                    _position = RawWritingPosition.Path;
                    break;
                }
                case RawWritingPosition.Path:
                {
                    var separator = value.IndexOf(QueryBeginSeparator);
                    if (separator == -1)
                    {
                        AppendPath(value, escape);
                        value = ReadOnlySpan<char>.Empty;
                    }
                    else
                    {
                        AppendPath(value[..separator], escape);
                        value = value[(separator + 1)..];
                        _position = RawWritingPosition.Query;
                    }

                    break;
                }
                case RawWritingPosition.Query:
                {
                    var separator = value.IndexOf(QueryContinueSeparator);
                    if (separator == 0)
                    {
                        value = value[1..];
                    }
                    else if (separator == -1)
                    {
                        GetQueryParts(value, out var queryName, out var queryValue);
                        AppendQuery(queryName, queryValue, escape);
                        value = ReadOnlySpan<char>.Empty;
                    }
                    else
                    {
                        GetQueryParts(value[..separator], out var queryName, out var queryValue);
                        AppendQuery(queryName, queryValue, escape);
                        value = value[(separator + 1)..];
                    }

                    break;
                }
            }
        }
    }

    private enum RawWritingPosition
    {
        Scheme,
        Host,
        Port,
        Path,
        Query
    }

    public void AppendRawNextLink(string nextLink, bool escape)
    {
        // If it is an absolute link, we use the nextLink as the entire url
        if (nextLink.StartsWith(Uri.UriSchemeHttp, StringComparison.InvariantCultureIgnoreCase))
        {
            Reset(new Uri(nextLink));
            return;
        }

        AppendRaw(nextLink, escape);
    }
}