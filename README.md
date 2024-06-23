Standalone Kestrel
==================

This project demonstrates how to run Kestrel without ASP.NET.  It creates a web endpoint on http://localhost:8080 which responds to all requests with "hello world".

It also demonstrates how to add the websocket middleware.  If you connect to ws://localhost:8080 (with wscat, for example) it will again respond with "hello world".  This time it will be sent as a textual websocket message.

Memory consumption is slightly better than ASP.NET, and startup time is a bit lower too, about 180ms on my system.  On a Linux system, you can measure the startup time like this:

```
dotnet publish -c Release
time ./time-startup
```

.NET 8
======

On .NET 7, the techniques used here were the only way to use Kestrel with native AOT compilation.  With .NET 8, the [minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview?view=aspnetcore-8.0) are supported with native AOT.  For most projects, this will be a better option.  If you still want to use Kestrel directly, this project may be useful.

As with .NET 7, you can build an AOT version if that is useful:

```
dotnet publish -r linux-x64 -c Release
```

(This has only been tested on Linux.  If you are not on Linux, you could try building an executable, but you will need to substitute a different runtime identifier in the above command.)

Unfortunately, with .NET 8, `UseHttps` has a dependency on the `KestrelMetrics` class, which is internal.  Instances of this class are created by reflection, and there is a `TrimmerRootDescriptor` in the `.csproj` file to ensure that all the necessary code is included in the final executable.  This is unsatisfactory because it depends on the internal design of Kestrel.  Also, the `dotnet publish` step displays trim warnings for this class even though they are supposed to be turned off; presumably this is a bug in the .NET SDK.  If you have a better solution for any of this, please let me know!

TLS
===

The server now supports TLS.  If you store a PKCS12 certificate in `localhost.pfx`, the server will listen for https connections on port 8080, rather than plain http.

For testing purposes (and *only* testing purposes) you can create a suitable self-signed certificate like this:

```
openssl req -x509 -newkey rsa:2048 -sha256 -keyout localhost.key -out localhost.crt -subj '/CN=test.com' -nodes
openssl pkcs12 -export -name localhost -out localhost.pfx -inkey localhost.key -in localhost.crt -passout pass:
```

To test the server, you will now need these commands:

```
curl --insecure https://localhost:8080
wscat --no-check -c wss://desktop.chown.org.uk:8080/ws
```
