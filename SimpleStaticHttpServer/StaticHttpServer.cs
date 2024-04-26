using CommandLine;
using System;
using System.Net;
using System.Threading.Tasks;
using SimpleStaticHttp;

namespace SimpleStaticHttp
{
    class Options
    {
        [Option('h', "host", Required = false, HelpText = "Adress/host to listen at.", Default = "localhost")]
        public string Host { get; set; }

        [Option('p', "port", Required = false, HelpText = "TCP port to listen at.", Default = 8080)]
        public int Port { get; set; }

        [Option("base-dir", Required = false, HelpText = "Root directory to serve.", Default = ".")]
        public string BaseDir { get; set; }
    }

    class StaticHttpServer
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    RunServer(o.Host, o.Port, o.BaseDir);
                });
        }

        private static void RunServer(string host, int port, string baseDir)
        {
            StaticFileHandler staticHandler = new StaticFileHandler(baseDir);

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{host}:{port}/");
            listener.Start();

            Console.WriteLine($"Serving directory {baseDir} at port {port}.");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                // Dispatch the serving to a task so we can serve the next request
                // without waiting for current download to finish.
                Task.Factory.StartNew(() => staticHandler.HandleContext(context));
            }
        }
    }
}
