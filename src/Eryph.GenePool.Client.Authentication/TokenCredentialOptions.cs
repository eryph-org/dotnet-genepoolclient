namespace Eryph.GenePool.Client;

/// <summary>
/// Options to configure requests made to the OAUTH identity service.
/// </summary>
public class TokenCredentialOptions
{
    public string? AuthorityUri { get; set; }
    /// <summary>
    /// Constructs a new <see cref="TokenCredentialOptions"/> instance.
    /// </summary>
    public TokenCredentialOptions()
    {
    }

    /// <summary>
    /// Specifies the <see cref="TokenCachePersistenceOptions"/> to be used by the credential. If not options are specified, the token cache will not be persisted to disk.
    /// </summary>
    public TokenCachePersistenceOptions? TokenCachePersistenceOptions { get; set; }

}