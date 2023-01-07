﻿using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Kestrel;

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
    private readonly WebSocketMiddleware wsMiddleware;

    public Application(ILoggerFactory loggerFactory)
    {
        var wsOptions = new WebSocketOptions();
        wsMiddleware = new WebSocketMiddleware(ContinueRequest, new OptionsWrapper<WebSocketOptions>(wsOptions),
            loggerFactory);
    }

    public Context CreateContext(IFeatureCollection contextFeatures)
    {
        return new Context(contextFeatures);
    }

    public void DisposeContext(Context context, Exception? exception)
    {
    }

    public async Task ProcessRequestAsync(Context context)
    {
        HttpContext httpContext = new DefaultHttpContext(context.features);
        await wsMiddleware.Invoke(httpContext);
    }

    private async Task ContinueRequest(HttpContext httpContext)
    {
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
            await httpContext.Response.Body.WriteAsync(Encoding.ASCII.GetBytes("hello world\n"));
        }
    }
}

class AppServices : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(ILoggerFactory))
            return new NullLoggerFactory();

        return null;
    }
}

class Program
{
    static async Task Main()
    {
        KestrelServerOptions serverOptions = new();
        if (File.Exists("localhost.pfx"))
        {
            HttpsConnectionAdapterOptions httpsOptions = new()
            {
                ServerCertificate = new X509Certificate2("localhost.pfx")
            };

            serverOptions.ListenAnyIP(8080, options =>
            {
                options.KestrelServerOptions.ApplicationServices = new AppServices();
                options.UseHttps(httpsOptions);
            });
        }
        else
        {
            serverOptions.ListenAnyIP(8080);
        }

        var transportOptions = new SocketTransportOptions();
        var loggerFactory = new NullLoggerFactory();

        var transportFactory = new SocketTransportFactory(
            new OptionsWrapper<SocketTransportOptions>(transportOptions), loggerFactory);

        using var server = new KestrelServer(
            new OptionsWrapper<KestrelServerOptions>(serverOptions), transportFactory, loggerFactory);

        await server.StartAsync(new Application(loggerFactory), CancellationToken.None);
        Console.WriteLine("Server started");
        await Task.Delay(Timeout.Infinite);
    }
}
