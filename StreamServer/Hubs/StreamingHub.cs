using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.SignalR;
using Streamer;
using StreamServer.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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
        static Subject<Streamer> StreamerSubject = new Subject<Streamer>();

        private static readonly Dictionary<int, bool> RoomHasSubscriber = new Dictionary<int, bool>();
        public async Task Subscribe(int index, int x, int y, int w, int h, bool stop)
        {
            var subscription = StreamerSubject.Subscribe(
                         async x =>
                         {
                             if (x.Index == index)
                             {
                                 var base64Data = Convert.ToBase64String(x.Image.ToByteArray());
                                 //var base64Data = x.Image.ToBase64();
                                 var imgData = "data:image/gif;base64," + base64Data;
                                 await Clients.Caller.SendAsync("ReceiveImage", imgData);
                             }
                         },
                         () => Console.WriteLine("Sequence Completed."));
            if (!RoomHasSubscriber.ContainsKey(index))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
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
                        var sb = new StringBuilder();
                        var streamServer = client.StreamingServer(request, cancellationToken: cts.Token);
                        RoomHasSubscriber[index] = true;
                        while (await streamServer.ResponseStream.MoveNext(cts.Token))
                        {
                            StreamerSubject.OnNext(new Streamer { Index = index, Image = streamServer.ResponseStream.Current.Image });
                        }
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            while (!RoomHasSubscriber.ContainsKey(index))
            {
                await Task.Delay(1000);
            }
            await Task.Delay(-1);
        }

        private static readonly Dictionary<int, ArrayPool<byte>> ImagePool = new Dictionary<int, ArrayPool<byte>>();
        public async Task SubscribePool(int index, int x, int y, int w, int h, bool stop)
        {
            var sb = new StringBuilder();
            var subscription = StreamerSubject.Subscribe(
                         async x =>
                         {
                             if (x.Index == index)
                             {
                                 //var base64Data = Convert.ToBase64String(x.Image.ToByteArray());
                                 //var base64Data = x.Image.ToBase64();
                                 //var length = x.Image.Length;
                                 //var arr = ImagePool[index].Rent(length);
                                 //x.Image.CopyTo(arr, 0);
                                 //var imgData = "data:image/gif;base64," + Convert.ToBase64String(arr);
                                 //await Clients.Caller.SendAsync("ReceiveImage", imgData);
                                 //ImagePool[index].Return(arr);
                                 await Clients.Caller.SendAsync("ReceiveImage", x.ImageString);
                             }
                         },
                         () => Console.WriteLine("Sequence Completed."));
            if (!RoomHasSubscriber.ContainsKey(index))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
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
                        RoomHasSubscriber[index] = true;
                        ImagePool[index] = ArrayPool<byte>.Create();
                        while (await streamServer.ResponseStream.MoveNext(cts.Token))
                        {
                            var img = streamServer.ResponseStream.Current.Image;
                            var length = img.Length;
                            var arr = ImagePool[index].Rent(length);
                            img.CopyTo(arr, 0);
                            var imgData = "data:image/gif;base64," + Convert.ToBase64String(arr);
                            ImagePool[index].Return(arr);
                            StreamerSubject.OnNext(new Streamer { Index = index, ImageString = imgData });
                        }
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            while (!RoomHasSubscriber.ContainsKey(index))
            {
                await Task.Delay(1000);
            }
            await Task.Delay(-1);
        }
        public async Task SubscribeAttempt_v1(int index, int x, int y, int w, int h, bool stop)
        {
            var sb = new StringBuilder();
            var subscription = StreamerSubject.Subscribe(
                         async x =>
                         {
                             if (x.Index == index)
                             {
                             //var base64Data = Convert.ToBase64String(x.Image.ToByteArray());
                             //var base64Data = x.Image.ToBase64();
                             //var length = x.Image.Length;
                             //var arr = ImagePool[index].Rent(length);
                             //x.Image.CopyTo(arr, 0);
                             //var imgData = "data:image/gif;base64," + Convert.ToBase64String(arr);
                             //await Clients.Caller.SendAsync("ReceiveImage", imgData);
                             //ImagePool[index].Return(arr);
                             await Clients.Caller.SendAsync("ReceiveImage", x.ImageString);
                             }
                         },
                         () => Console.WriteLine("Sequence Completed."));
            if (!RoomHasSubscriber.ContainsKey(index))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
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
                        RoomHasSubscriber[index] = true;
                        ImagePool[index] = ArrayPool<byte>.Create();
                        while (await streamServer.ResponseStream.MoveNext(cts.Token))
                        {
                            var img = streamServer.ResponseStream.Current.Image;
                            var length = img.Length;
                            var arr = ImagePool[index].Rent(length);
                            img.CopyTo(arr, 0);
                            var imgData = "data:image/gif;base64," + Convert.ToBase64String(arr);
                            ImagePool[index].Return(arr);
                            StreamerSubject.OnNext(new Streamer { Index = index, ImageString = imgData });
                        }
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            while (!RoomHasSubscriber.ContainsKey(index))
            {
                await Task.Delay(1000);
            }
            await Task.Delay(-1);
        }
        public async Task SubscribeAttempt_v2(int index, int x, int y, int w, int h, bool stop) 
        {
            var subscription = StreamerSubject.Subscribe(
                         async x =>
                         {
                             if (x.Index == index)
                             {
                                 var base64Data = Convert.ToBase64String(x.Image.ToByteArray());
                                 //var base64Data = x.Image.ToBase64(); // even worse than ToByteArray
                                 var length = x.Image.Length;
                                 var arr = ImagePool[index].Rent(length);
                                 x.Image.CopyTo(arr, 0);
                                 var imgData = "data:image/gif;base64," + Convert.ToBase64String(arr);
                                 await Clients.Caller.SendAsync("ReceiveImage", imgData);
                                 ImagePool[index].Return(arr);
                             }
                         },
                         () => Console.WriteLine("Sequence Completed."));
            if (!RoomHasSubscriber.ContainsKey(index))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
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
                        RoomHasSubscriber[index] = true;
                        ImagePool[index] = ArrayPool<byte>.Create();
                        while (await streamServer.ResponseStream.MoveNext(cts.Token))
                        {
                            StreamerSubject.OnNext(new Streamer { Index = index, Image = streamServer.ResponseStream.Current.Image});
                        }
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            while (!RoomHasSubscriber.ContainsKey(index))
            {
                await Task.Delay(1000);
            }
            await Task.Delay(-1);
        }
        public async Task SubscribeAttempt_v3(int index, int x, int y, int w, int h, bool stop)
        {
            var sb = new StringBuilder();
            var subscription = StreamerSubject.Subscribe(
                         async x =>
                         {
                             if (x.Index == index)
                             {
                                 var base64Data = Convert.ToBase64String(x.Image.ToByteArray());
                                 //var base64Data = x.Image.ToBase64();
                                 var length = x.Image.Length;
                                 var arr = ImagePool[index].Rent(length);
                                 x.Image.CopyTo(arr, 0);
                                 var imgData = "data:image/gif;base64," + Convert.ToBase64String(arr);
                                 await Clients.Caller.SendAsync("ReceiveImage", imgData);
                                 ImagePool[index].Return(arr);
                             }
                         },
                         () => Console.WriteLine("Sequence Completed."));
            if (!RoomHasSubscriber.ContainsKey(index))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () =>
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
                        RoomHasSubscriber[index] = true;
                        ImagePool[index] = ArrayPool<byte>.Create();
                        while (await streamServer.ResponseStream.MoveNext(cts.Token))
                        {
                            StreamerSubject.OnNext(new Streamer { Index = index, Image = streamServer.ResponseStream.Current.Image });
                        }
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            while (!RoomHasSubscriber.ContainsKey(index))
            {
                await Task.Delay(1000);
            }
            await Task.Delay(-1);
        }
    }

    public class Streamer
    {
        public int Index { get; set; }
        public ByteString Image { get; set; }
        public string ImageString { get; set; }
    }
}
