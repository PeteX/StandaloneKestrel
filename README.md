Standalone Kestrel
==================

This project demonstrates how to run Kestrel without ASP.NET.  It creates a web endpoint on http://localhost:8080 which responds to all requests with "hello world".

It also demonstrates how to add the websocket middleware.  If you connect to ws://localhost:8080 (with wscat, for example) it will again respond with "hello world".  This time it will be sent as a textual websocket message.

I wrote this code because I was curious to see if it could be done, but it may be interesting for someone.  It uses significantly less memory than ASP.NET.  Startup time is a bit lower too, about 180ms on my system.  On a Linux system, you can measure it like this:

```
dotnet publish -c Release
time ./time-startup
```

---

This project was originally developed for .NET Core 2.1, but has now been updated for .NET 6.  The update allowed one of the classes to be removed, simplifying the project significantly.  Unfortunately, the Kestrel package is no longer published on its own, so it now has to depend on `Microsoft.NET.Sdk.Web` rather than `Microsoft.NET.Sdk`.
