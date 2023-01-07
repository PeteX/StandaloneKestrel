Standalone Kestrel
==================

This project demonstrates how to run Kestrel without ASP.NET.  It creates a web endpoint on http://localhost:8080 which responds to all requests with "hello world".

It also demonstrates how to add the websocket middleware.  If you connect to ws://localhost:8080 (with wscat, for example) it will again respond with "hello world".  This time it will be sent as a textual websocket message.

Memory consumption is slightly better than ASP.NET, and startup time is a bit lower too, about 180ms on my system.  On a Linux system, you can measure the startup time like this:

```
dotnet publish -c Release
time ./time-startup
```

.NET 7
======

I've been maintaining this package on and off since .NET Core 2.1.  For most of this time, it has just been an interesting hack, but now it has a potential real-world use.  .NET 7 supports [AOT compilation to a single executable](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/), but this is only for simple console applications, not for ASP.NET.  Standalone Kestrel, though, works in this mode.  You can build it like this:

```
dotnet publish -r linux-x64 -c Release
```

(This has only been tested on Linux.  If you are not on Linux, you could try building an executable, but you will need to substitute a different runtime identifier in the above command.)

You will see a few warnings during compilation, but as far as I can tell, the resulting executable works well.  The executable is about 46M with debug information, 15M stripped.  After the process has been invoked (using both HTTP GET and websockets) the resident memory is about 22M, far better than even the simplest ASP.NET application, and similar to Go.

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
