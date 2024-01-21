using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Identity.Client;

namespace Eryph.GenePool.Client;

/// <summary>
/// Account information relating to an authentication request.
/// </summary>
/// <seealso cref="TokenCachePersistenceOptions"/>.
[PublicAPI]
public class AuthenticationRecord
{
    internal const string? CurrentVersion = "1.0";
    private const string AuthorityPropertyName = "authority";
    private const string HomeAccountIdPropertyName = "homeAccountId";
    private const string TenantIdPropertyName = "tenantId";
    private const string ClientIdPropertyName = "clientId";
    private const string VersionPropertyName = "version";

    private static readonly JsonEncodedText AuthorityPropertyNameBytes = JsonEncodedText.Encode(AuthorityPropertyName);
    private static readonly JsonEncodedText HomeAccountIdPropertyNameBytes = JsonEncodedText.Encode(HomeAccountIdPropertyName);
    private static readonly JsonEncodedText TenantIdPropertyNameBytes = JsonEncodedText.Encode(TenantIdPropertyName);
    private static readonly JsonEncodedText ClientIdPropertyNameBytes = JsonEncodedText.Encode(ClientIdPropertyName);
    private static readonly JsonEncodedText VersionPropertyNameBytes = JsonEncodedText.Encode(VersionPropertyName);

    internal AuthenticationRecord()
    {
        AccountId = new AccountId("", "", "");
    }

    internal AuthenticationRecord(AuthenticationResult authResult, string? clientId)
    {
        Authority = authResult.Account.Environment;
        AccountId = authResult.Account.HomeAccountId;
        TenantId = authResult.TenantId;
        ClientId = clientId;
    }

    internal AuthenticationRecord(string? authority, string? homeAccountId, string? tenantId, string? clientId)
    {
        Authority = authority;
        AccountId = BuildAccountIdFromString(homeAccountId ?? "");
        TenantId = tenantId;
        ClientId = clientId;
    }

    /// <summary>
    /// The authority host used to authenticate the account.
    /// </summary>
    public string? Authority { get; private set; }

    /// <summary>
    /// A unique identifier of the account.
    /// </summary>
    public string? HomeAccountId => AccountId.Identifier;

    /// <summary>
    /// The tenant the account should authenticate in.
    /// </summary>
    public string? TenantId { get; private set; }

    /// <summary>
    /// The client id of the application which performed the original authentication
    /// </summary>
    public string? ClientId { get; private set; }

    internal AccountId AccountId { get; private set; }
    internal string? Version { get; private set; } = CurrentVersion;

    /// <summary>
    /// Serializes the <see cref="AuthenticationRecord"/> to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> which the serialized <see cref="AuthenticationRecord"/> will be written to.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    public void Serialize(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        SerializeAsync(stream, false, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Serializes the <see cref="AuthenticationRecord"/> to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which the serialized <see cref="AuthenticationRecord"/> will be written.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    public async Task SerializeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        await SerializeAsync(stream, true, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deserializes the <see cref="AuthenticationRecord"/> from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> from which the serialized <see cref="AuthenticationRecord"/> will be read.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    public static AuthenticationRecord Deserialize(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        return DeserializeAsync(stream, false, cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Deserializes the <see cref="AuthenticationRecord"/> from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> from which the serialized <see cref="AuthenticationRecord"/> will be read.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
    public static async Task<AuthenticationRecord> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        return await DeserializeAsync(stream, true, cancellationToken).ConfigureAwait(false);
    }

    private async Task SerializeAsync(Stream stream, bool async, CancellationToken cancellationToken)
    {
        await using var json = new Utf8JsonWriter(stream);
        json.WriteStartObject();


        json.WriteString(AuthorityPropertyNameBytes, Authority);

        json.WriteString(HomeAccountIdPropertyNameBytes, HomeAccountId);

        json.WriteString(TenantIdPropertyNameBytes, TenantId);

        json.WriteString(ClientIdPropertyNameBytes, ClientId);

        json.WriteString(VersionPropertyNameBytes, Version);

        json.WriteEndObject();

        if (async)
        {
            await json.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await json.FlushAsync(cancellationToken);
        }
    }

    private static async Task<AuthenticationRecord> DeserializeAsync(Stream stream, bool async, CancellationToken cancellationToken)
    {
        var authProfile = new AuthenticationRecord();

        using var doc = async ? await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false) : JsonDocument.Parse(stream);

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            switch (prop.Name)
            {
                case AuthorityPropertyName:
                    authProfile.Authority = prop.Value.GetString();
                    break;
                case HomeAccountIdPropertyName:
                    authProfile.AccountId = BuildAccountIdFromString(prop.Value.GetString()?? "");
                    break;
                case TenantIdPropertyName:
                    authProfile.TenantId = prop.Value.GetString();
                    break;
                case ClientIdPropertyName:
                    authProfile.ClientId = prop.Value.GetString();
                    break;
                case VersionPropertyName:
                    authProfile.Version = prop.Value.GetString();
                    if (authProfile.Version != CurrentVersion)
                    {
                        throw new InvalidOperationException($"Attempted to deserialize an {nameof(AuthenticationRecord)} with a version that is not the current version. Expected: '{CurrentVersion}', Actual: '{authProfile.Version}'");
                    }
                    break;
            }
        }

        return authProfile;
    }

    private static AccountId BuildAccountIdFromString(string homeAccountId)
    {
        //For the Microsoft identity platform (formerly named Azure AD v2.0), the identifier is the concatenation of
        // Microsoft.Identity.Client.AccountId.ObjectId and Microsoft.Identity.Client.AccountId.TenantId separated by a dot.
        var homeAccountSegments = homeAccountId.Split('.');
        var accountId = homeAccountSegments.Length == 2 
            ? new AccountId(homeAccountId, homeAccountSegments[0], homeAccountSegments[1]) 
            : new AccountId(homeAccountId);
        return accountId;
    }
}