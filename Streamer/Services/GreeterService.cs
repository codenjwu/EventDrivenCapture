using EventDrivenCapture;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Streamer
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;

        }
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
        AutoResetEvent resetEvent = new AutoResetEvent(false);
        public override async Task StreamingServer(StreamRequest request, IServerStreamWriter<StreamImages> responseStream, ServerCallContext context)
        {
            var capture = new Capture(new CaptureSetting(request.X, request.Y, request.W, request.H) { ID = request.Index });
            CaptureEventHandler CaptureEvent = new CaptureEventHandler(new EventDrivenCapture.Capture[1] { capture });

            capture.CapturedEventHandler += (sender, args) =>
            {
                var images = new StreamImages();
                var streamReader = new MemoryStream();
                capture.CapturedImage.Save(streamReader, ImageFormat.Bmp);
                images.Image = ByteString.CopyFrom(streamReader.ToArray());
                responseStream.WriteAsync(images).Wait(); // use await here will make event handler async void which actually will not wait for response write to be done, that may cause write to a uncomplete response stream
            };
            CaptureEvent.Start();

            resetEvent.WaitOne();
        }
    }
}
