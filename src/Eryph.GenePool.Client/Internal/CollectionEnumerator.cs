using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;

namespace Eryph.GenePool.Client.Internal;

internal abstract class CollectionEnumerator<T> where T : notnull
{
    public abstract ValueTask<Page<T>> GetNextPageAsync(
        string? continuationToken,
        int? pageSizeHint,
        bool async,
        CancellationToken cancellationToken);

    public Pageable<T> ToSyncCollection(CancellationToken cancellationToken)
    {
        return new GenepoolPageable(this, cancellationToken);
    }

    public AsyncPageable<T> ToAsyncCollection(CancellationToken cancellationToken)
    {
        return new GenepoolAsyncPageable(this, cancellationToken);
    }

    /// <summary>
    /// Abstract the pageable pattern for async iteration
    /// </summary>
    private sealed class GenepoolPageable(CollectionEnumerator<T> enumerator, CancellationToken cancellationToken)
        : Pageable<T>(cancellationToken)
    {
        /// <summary>
        /// Determine if the iteration can continue.
        /// </summary>
        /// <param name="continuationToken">
        /// The next continuation token provided with the last
        /// <see cref="Page{T}"/>.
        /// </param>
        /// <returns>
        /// True if the iteration can continue, false otherwise.
        /// </returns>
        private bool CanContinue(string? continuationToken) =>
            !string.IsNullOrEmpty(continuationToken);

        /// <summary>
        /// Enumerate the values a <see cref="Page{T}"/> at a time.  This may
        /// make multiple service requests.
        /// </summary>
        /// <param name="continuationToken">
        /// A continuation token indicating where to resume paging or null to
        /// begin paging from the beginning.
        /// </param>
        /// <param name="pageHintSize">
        /// The size of <see cref="Page{T}"/>s that should be requested (from
        /// service operations that support it).
        /// </param>
        /// <returns>
        /// An async sequence of <see cref="Page{T}"/>s.
        /// </returns>
        public override IEnumerable<Page<T>> AsPages(
            string? continuationToken = default,
            int? pageHintSize = default)
        {
            do
            {
                var page = enumerator.GetNextPageAsync(
                        continuationToken,
                        pageHintSize,
                        async: false,
                        cancellationToken: CancellationToken)
                    .EnsureCompleted();
                continuationToken = page.ContinuationToken;
                yield return page;
            } while (CanContinue(continuationToken));
        }

        /// <summary>
        /// Enumerate the values in the collection synchronously.  This may
        /// make multiple service requests.
        /// </summary>
        /// <returns>A sequence of values.</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            string? continuationToken = null;
            do
            {
                var page = enumerator.GetNextPageAsync(
                        continuationToken,
                        null,
                        async: false,
                        cancellationToken: CancellationToken)
                    .EnsureCompleted();
                continuationToken = page.ContinuationToken;
                foreach (var item in page.Values)
                {
                    yield return item;
                }
            } while (CanContinue(continuationToken));
        }
    }

    /// <summary>
    /// Abstract the pageable pattern for async iteration
    /// </summary>
    private sealed class GenepoolAsyncPageable(CollectionEnumerator<T> enumerator, CancellationToken cancellationToken)
        : AsyncPageable<T>(cancellationToken)
    {
        /// <summary>
        /// Determine if the iteration can continue.
        /// </summary>
        /// <param name="continuationToken">
        /// The next continuation token provided with the last
        /// <see cref="Page{T}"/>.
        /// </param>
        /// <returns>
        /// True if the iteration can continue, false otherwise.
        /// </returns>
        private static bool CanContinue(string? continuationToken) =>
            !string.IsNullOrEmpty(continuationToken);

        /// <summary>
        /// Enumerate the values a <see cref="Page{T}"/> at a time.  This may
        /// make multiple service requests.
        /// </summary>
        /// <param name="continuationToken">
        /// A continuation token indicating where to resume paging or null to
        /// begin paging from the beginning.
        /// </param>
        /// <param name="pageHintSize">
        /// The size of <see cref="Page{T}"/>s that should be requested (from
        /// service operations that support it).
        /// </param>
        /// <returns>
        /// An async sequence of <see cref="Page{T}"/>s.
        /// </returns>
        public override async IAsyncEnumerable<Page<T>> AsPages(
            string? continuationToken = default,
            int? pageHintSize = default)
        {
            do
            {
                var page = await enumerator.GetNextPageAsync(
                        continuationToken,
                        pageHintSize,
                        async: true,
                        cancellationToken: CancellationToken)
                    .ConfigureAwait(false);
                continuationToken = page.ContinuationToken;
                yield return page;
            } while (CanContinue(continuationToken));
        }

        /// <summary>
        /// Enumerate the values in the collection asynchronously.  This may
        /// make multiple service requests.
        /// </summary>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used for requests made while
        /// enumerating asynchronously.
        /// </param>
        /// <returns>An async sequence of values.</returns>
        public override async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            // This is the only method that takes its own CancellationToken, but
            // we'll still use the original CancellationToken if one wasn't passed.
            if (cancellationToken == default)
            {
                cancellationToken = CancellationToken;
            }

            string? continuationToken = null;
            do
            {
                var page = await enumerator.GetNextPageAsync(
                        continuationToken,
                        null,
                        async: true,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                continuationToken = page.ContinuationToken;
                foreach (var item in page.Values)
                {
                    yield return item;
                }
            } while (CanContinue(continuationToken));
        }
    }
}