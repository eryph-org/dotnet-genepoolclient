﻿using Azure.Core.Pipeline;
using Azure.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

using Eryph.GenePool.Client.Internal;

namespace Eryph.GenePool.Client.RestClients
{
    internal class UploadClient
    {
        private readonly HttpPipeline _pipeline;

        /// <summary> The ClientDiagnostics is used to provide tracing support for the client library. </summary>
        internal ClientDiagnostics ClientDiagnostics { get; }

        /// <summary> Initializes a new instance of GenesRestClient. </summary>
        /// <param name="clientDiagnostics"> The handler for diagnostic messaging in the client. </param>
        /// <param name="pipeline"> The HTTP pipeline for sending and receiving REST requests and responses. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="clientDiagnostics"/> or <paramref name="pipeline"/> is null. </exception>
        public UploadClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline)
        {
            ClientDiagnostics = clientDiagnostics ?? throw new ArgumentNullException(nameof(clientDiagnostics));
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }


        internal HttpMessage CreateUploadRequest(Uri uri, Stream content)
        {
            var message = _pipeline.CreateMessage();
            var request = message.Request;
            request.Method = RequestMethod.Put;
            request.Uri.Reset(uri);
            request.Content = RequestContent.Create(content);
            return message;
        }

        /// <summary> Creates a organization. </summary>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public async Task UploadPartAsync(Uri uri, Stream content, CancellationToken cancellationToken = default)
        {

            using var message = CreateUploadRequest(uri, content);
            await _pipeline.SendAsync(message, cancellationToken).ConfigureAwait(false);
        }


        /// <summary> Creates a organization. </summary>
        /// <param name="body"> The CreateOrganizationBody to use. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public void UploadPart(Uri uri, Stream content, CancellationToken cancellationToken = default)
        {

            using var message = CreateUploadRequest(uri, content);
            
            _pipeline.Send(message, cancellationToken);

        }

    }
}
