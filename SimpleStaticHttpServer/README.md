# SimpleStaticHttpServer

A simple tool to serve static files over HTTP using
[SimpleStaticHttp](https://www.nuget.org/packages/SimpleStaticHttp) library.

> :information_source: This utility is meant to help development and not optimized for production.

## Usage

To install:

```
$ dotnet tool install -g SimpleStaticHttpServer
```

To run:

```
$ simple-static-http-server # Serves current dir at http://localhost:8080
```

Use `--help` to see how to configure host, port, and base dir.

## Support for .NET framework 4.*

For .NET framework 4.*, this is not installable as dotnet tool but can be built:

```
$ dotnet build -f net40 -c Release
```
