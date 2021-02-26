using Grpc.Net.Client;
using Microsoft.AspNetCore.SignalR;
using Streamer;
using StreamServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StreamServer.Hubs
{
    public class StreamingHub : Hub
    {
        HttpClientHandler httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        public async Task SendImage(int index, int x, int y, int w, int h, bool stop)
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:55555", new GrpcChannelOptions { HttpHandler = httpHandler }))
            {
                var request = new StreamRequest();
                request.X = x;
                request.Y = y;
                request.W = w;
                request.H = h;
                var client = new Greeter.GreeterClient(channel);
                CancellationTokenSource cts = new CancellationTokenSource();
                var streamServer = client.StreamingServer(request, cancellationToken: cts.Token);
                while (await streamServer.ResponseStream.MoveNext(cts.Token))
                {
                    var base64Data = Convert.ToBase64String(streamServer.ResponseStream.Current.Image.ToByteArray());
                    var imgData = "data:image/gif;base64," + base64Data;
                    await Clients.Caller.SendAsync("ReceiveImage", imgData);
                }
            }
        }
    }
}
