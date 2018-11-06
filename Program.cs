﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Kestrel
{
    class Context
    {
        public IFeatureCollection features;

        public Context(IFeatureCollection features)
        {
            this.features = features;
        }
    }

    class Application : IHttpApplication<Context>
    {
        private WebSocketMiddleware wsMiddleware;

        public Application() {
            var wsOptions = new WebSocketOptions();
            wsMiddleware = new WebSocketMiddleware(continueRequest, new OptionsWrapper<WebSocketOptions>(wsOptions));
        }

        public Context CreateContext(IFeatureCollection contextFeatures)
        {
            return new Context(contextFeatures);
        }

        public void DisposeContext(Context context, Exception exception)
        {
        }

        public async Task ProcessRequestAsync(Context context)
        {
            HttpContext httpContext = new DefaultHttpContext(context.features);
            await wsMiddleware.Invoke(httpContext);
        }

        private async Task continueRequest(HttpContext httpContext) {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
                var message = Encoding.ASCII.GetBytes("hello world");
                await socket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true,
                    CancellationToken.None);

                await socket.ReceiveAsync(new byte[4096], CancellationToken.None);
            }
            else
            {
                httpContext.Response.Headers.Add("Content-Type", new StringValues("text/plain"));
                await httpContext.Response.Body.WriteAsync(Encoding.ASCII.GetBytes("hello world"));
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var serverOptions = new KestrelServerOptions();
            serverOptions.ListenAnyIP(8080);

            var uvOptions = new LibuvTransportOptions();

            var loggerFactory = new NullLoggerFactory();
            var lifetimeLogger = new NullLogger<ApplicationLifetime>();
            var applicationLifetime = new ApplicationLifetime(lifetimeLogger);

            var transportFactory = new LibuvTransportFactory(
                new OptionsWrapper<LibuvTransportOptions>(uvOptions), applicationLifetime, loggerFactory);

            using (var server = new KestrelServer(new OptionsWrapper<KestrelServerOptions>(serverOptions),
                transportFactory, loggerFactory))
            {
                await server.StartAsync(new Application(), CancellationToken.None);
                Console.ReadLine();
            }
        }
    }
}