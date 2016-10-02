ARSoft.Tools.Net-Modified
-------------------------

Solution contains modified library ARSoft.Tools.Net version 2.2.6

### Main Change List

- Feature: Added possibility to use DNS Client with DNS Servers, 
  which are using other than standard port (53). 
  This is rare case but it might be desirable to use custom port, for example:

  - Service Discovery systems, like [Consul](https://www.consul.io/) 
    are using DNS protocol, but are not always hosted on the standard port. Consul by default is using port 8600.
    This feature might be helpful when setting the infrastructure for Microservices up.

  - During testing custom DNS Servers 

- Bug fix / Optimization: Caching, when TTL is zero (caching is off), than records is not added unnecessary to cache (kept in memory).



Original project ARSoft.Tools.Net read me content
-------------------------------------------------

ARSoft.Tools.Net - C#/.Net DNS client/server, SPF and SenderID Library

This repository is automatically generated from builds made available
on http://arsofttoolsnet.codeplex.com/ with the purpose of making it easier
to track changes in the sources, since the project doesn't make their repository
available.

Sources and release notes are copied from the codeplex pages unmodified.

The scripts to generate this repository are available at
https://github.com/nvivo/ARSoft.Tools.Net-Generator