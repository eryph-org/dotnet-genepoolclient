using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Eryph.GenePool.Client;

internal static class AbstractAcquireTokenParameterBuilderExtensions
{
    public static async ValueTask<AuthenticationResult> ExecuteAsync<T>(this AbstractAcquireTokenParameterBuilder<T> builder, bool async, CancellationToken cancellationToken)
        where T : AbstractAcquireTokenParameterBuilder<T>
    {
        var result = async
            ? await builder.ExecuteAsync(cancellationToken).ConfigureAwait(false)
            : builder.ExecuteAsync(cancellationToken).GetAwaiter().GetResult();
        return result;
    }
}