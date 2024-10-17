using Azure.Core;
using Azure.Core.Pipeline;

namespace Eryph.GenePool.Client;

internal class HardwareIdHeaderPolicy(string hardwareId)
    : HttpPipelineSynchronousPolicy
{
    public override void OnSendingRequest(HttpMessage message)
    {
        message.Request.Headers.SetValue("X-Eryph-Hardware-Id", hardwareId);
    }
}