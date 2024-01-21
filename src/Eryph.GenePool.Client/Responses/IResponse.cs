using System.Net;
using JetBrains.Annotations;

namespace Eryph.GenePool.Client.Responses
{
    [PublicAPI]
    public interface IResponse
    {
        HttpStatusCode StatusCode { get;  }
        ResponseType ResponseType { get;  }
    }
}
