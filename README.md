# SimpleStaticHttp

A simple library for handling static files and directory browsing with
[System.Net.HttpListener](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener).

> :information_source: This library is meant to help development and not optimized for production.

## Usage

Sample usage with C#:

```
using System.Net;
using SimpleStaticHttp;

class ExampleStaticServer
{
    public static void Main()
    {
        StaticFileHandler staticHandler = new StaticFileHandler(
                baseDirectory: ".", // local base dir to serve
                basePath: "/static"); // base URL path which the file/dir is relative to

        // This library has a default minimal mime type detection.
        // You can bring your own mime type detector, e.g. using another library:
        // staticHandler.ContentTypeDetector = HeyRed.Mime.MimeTypesMap.GetMimeType;
        // Or your custom logic, e.g. return everything as text/plain:
        // staticHandler.ContentTypeDetector = (path) => "text/plain";

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        System.Console.WriteLine("Static file server running at port 8080");

        while (true)
        {
            staticHandler.HandleContext(listener.GetContext());
        }
    }
}
```

Example with PowerShell script:

```
# Adjust dll location
Add-Type -Path "bin\Release\net40\SimpleStaticHttp.dll"

$staticHandler = New-Object SimpleStaticHttp.StaticFileHandler("C:\Path\To\Dir")

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:8080/")
$listener.Start()

Write-Host "Static file server running"

while ($true) {
    $context = $listener.GetContext()
    $staticHandler.HandleContext($context)
}
```

## Building

To build this library:

```
dotnet pack
```

## .NET tool wrapper

A .NET command line tool wrapper is available for basic use of this library:

```
$ dotnet tool install -g SimpleStaticHttpServer
$ simple-static-http-server # Serves current dir at http://localhost:8080
```
