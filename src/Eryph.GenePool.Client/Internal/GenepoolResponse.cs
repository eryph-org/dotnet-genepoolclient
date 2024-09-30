using Azure;

namespace Eryph.GenePool.Client.Internal;

///<inheritdoc cref="Response"/>
internal class GenepoolResponse<T>(T value, Response rawResponse) : Response<T>
{
    ///<inheritdoc cref="Value"/>
    public override T Value => value;


    ///<inheritdoc cref="GetRawResponse"/>
    public override Response GetRawResponse()
    {
        return rawResponse;
    }
}