Standalone Kestrel
==================

This project demonstrates how to run Kestrel without ASP.NET.  It creates a web endpoint on http://localhost:8080 which responds to all requests with "hello world".

It also demonstrates how to add the websocket middleware.  If you connect to ws://localhost:8080 (with wscat, for example) it will again respond with "hello world".  This time it will be sent as a textual websocket message.

I wrote this code because I was curious to see if it could be done, but it may be interesting for someone.  It uses significantly less memory than ASP.NET.
