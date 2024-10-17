// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eryph.GenePool.Client.Internal;

internal class HttpMessageSanitizer(
    string[] allowedQueryParameters,
    string[] allowedHeaders,
    string redactedPlaceholder = "REDACTED")
{
    private const string LogAllValue = "*";
    private readonly bool _logAllHeaders = allowedHeaders.Contains(LogAllValue);
    private readonly bool _logFullQueries = allowedQueryParameters.Contains(LogAllValue);
    private readonly HashSet<string> _allowedHeaders = new HashSet<string>(allowedHeaders, StringComparer.InvariantCultureIgnoreCase);

    internal static HttpMessageSanitizer Default = new HttpMessageSanitizer([], []);

    public string SanitizeHeader(string name, string value)
    {
        if (_logAllHeaders || _allowedHeaders.Contains(name))
        {
            return value;
        }

        return redactedPlaceholder;
    }

    public string SanitizeUrl(string url)
    {
        if (_logFullQueries)
        {
            return url;
        }

#if NET5_0_OR_GREATER
            int indexOfQuerySeparator = url.IndexOf('?', StringComparison.Ordinal);
#else
        var indexOfQuerySeparator = url.IndexOf('?');
#endif

        if (indexOfQuerySeparator == -1)
        {
            return url;
        }

        var stringBuilder = new StringBuilder(url.Length);
        stringBuilder.Append(url, 0, indexOfQuerySeparator);

        var query = url[indexOfQuerySeparator..];

        var queryIndex = 1;
        stringBuilder.Append('?');

        do
        {
            var endOfParameterValue = query.IndexOf('&', queryIndex);
            var endOfParameterName = query.IndexOf('=', queryIndex);
            var noValue = false;

            // Check if we have parameter without value
            if ((endOfParameterValue == -1 && endOfParameterName == -1) ||
                (endOfParameterValue != -1 && (endOfParameterName == -1 || endOfParameterName > endOfParameterValue)))
            {
                endOfParameterName = endOfParameterValue;
                noValue = true;
            }

            if (endOfParameterName == -1)
            {
                endOfParameterName = query.Length;
            }

            if (endOfParameterValue == -1)
            {
                endOfParameterValue = query.Length;
            }
            else
            {
                // include the separator
                endOfParameterValue++;
            }

            var parameterName = query.AsSpan(queryIndex, endOfParameterName - queryIndex);

            var isAllowed = false;
            foreach (var name in allowedQueryParameters)
            {
                if (!parameterName.Equals(name.AsSpan(), StringComparison.OrdinalIgnoreCase)) continue;
                isAllowed = true;
                break;
            }

            var valueLength = endOfParameterValue - queryIndex;
            var nameLength = endOfParameterName - queryIndex;

            if (isAllowed)
            {
                stringBuilder.Append(query, queryIndex, valueLength);
            }
            else
            {
                if (noValue)
                {
                    stringBuilder.Append(query, queryIndex, valueLength);
                }
                else
                {
                    stringBuilder.Append(query, queryIndex, nameLength);
                    stringBuilder.Append('=');
                    stringBuilder.Append(redactedPlaceholder);
                    if (query[endOfParameterValue - 1] == '&')
                    {
                        stringBuilder.Append('&');
                    }
                }
            }

            queryIndex += valueLength;
        } while (queryIndex < query.Length);

        return stringBuilder.ToString();
    }
}