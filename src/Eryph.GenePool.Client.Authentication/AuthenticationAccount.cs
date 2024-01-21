using Microsoft.Identity.Client;

namespace Eryph.GenePool.Client;

internal class AuthenticationAccount : IAccount
{
    private readonly AuthenticationRecord _profile;

    internal AuthenticationAccount(AuthenticationRecord profile)
    {
        _profile = profile;
    }

    public string Username => "";
    string? IAccount.Environment => _profile.Authority;

    AccountId IAccount.HomeAccountId => _profile.AccountId;

    public static explicit operator AuthenticationAccount(AuthenticationRecord profile) => new(profile);
    public static explicit operator AuthenticationRecord(AuthenticationAccount account) => account._profile;
}