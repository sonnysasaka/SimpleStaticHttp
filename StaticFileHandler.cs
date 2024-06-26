using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace SimpleStaticHttp
{
    /// <summary>
    /// Represents a handler for serving static files and directory listings.
    /// </summary>
    public class StaticFileHandler
    {
        /// <summary>
        /// Gets or sets the base directory from which files are served.
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Gets or sets the base URL path.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Type of the content type detector.
        /// </summary>
        public delegate string GetContentTypeDelegate(string path);

        /// <summary>
        /// Gets or sets the content type detector.
        /// </summary>
        public GetContentTypeDelegate ContentTypeDetector { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticFileHandler"/> class.
        /// </summary>
        /// <param name="baseDirectory">The base directory to serve files from.</param>
        /// <param name="basePath">The URL base path to match.</param>
        public StaticFileHandler(string baseDirectory = ".", string basePath = "/")
        {
            BaseDirectory = baseDirectory;
            BasePath = basePath;
        }

        /// <summary>
        /// Handles the incoming HTTP request and serves the appropriate file or directory listing.
        /// </summary>
        /// <param name="context">The context of the HTTP request.</param>
        public void HandleContext(HttpListenerContext context)
        {
            if (!context.Request.Url.AbsolutePath.StartsWith(BasePath))
            {
                // Base URL path does not match, should not be routed here.
                NotFound(context);
                return;
            }

            // File path to serve is relative to `BasePath`.
            string relativePath = Uri.UnescapeDataString(context.Request.Url.AbsolutePath)
                .Substring(BasePath.Length).TrimStart('/');
            string filePath = Path.Combine(
                BaseDirectory,
                relativePath.Replace('/', Path.DirectorySeparatorChar));

            try
            {
                if (Directory.Exists(filePath))
                {
                    ServeDirectoryListing(context, filePath);
                    return;
                }

                if (File.Exists(filePath))
                {
                    ServeStaticFile(context, filePath);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Exception when serving file/directory: " + ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                SafeCloseOutputStream(context);
                return;
            }

            // Not a file nor a directory, return 404.
            NotFound(context);
        }

        private void NotFound(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            SafeCloseOutputStream(context);
        }

        private void ServeStaticFile(HttpListenerContext context, string filename)
        {
            using (FileStream fs = File.OpenRead(filename))
            {
                context.Response.ContentType = GetContentType(filename);
                context.Response.ContentLength64 = fs.Length;
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                // A file may be very large, so stream the file content directly to the response stream
                // without loading all to memory.
                fs.CopyTo(context.Response.OutputStream);
            }

            SafeCloseOutputStream(context);
        }

        private void ServeDirectoryListing(HttpListenerContext context, string path)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            using (XmlWriter writer = XmlWriter.Create(
                context.Response.OutputStream,
                new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("html");

                writer.WriteStartElement("head");
                writer.WriteElementString("title", "Directory Listing - " + path);
                writer.WriteEndElement(); // End head

                writer.WriteStartElement("body");
                writer.WriteElementString("h2", "Directory Listing - " + path);

                writer.WriteStartElement("ul");

                foreach (string directory in Directory.GetDirectories(path))
                {
                    string dirName = Path.GetFileName(directory);
                    writer.WriteStartElement("li");
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("href", Path.Combine(context.Request.Url.AbsolutePath, dirName));
                    writer.WriteString(dirName + "/");
                    writer.WriteEndElement(); // End a
                    writer.WriteEndElement(); // End li
                }

                foreach (string file in Directory.GetFiles(path))
                {
                    string fileName = Path.GetFileName(file);
                    writer.WriteStartElement("li");
                    writer.WriteStartElement("a");
                    writer.WriteAttributeString("href", Path.Combine(context.Request.Url.AbsolutePath, fileName));
                    writer.WriteString(fileName);
                    writer.WriteEndElement(); // End a
                    writer.WriteEndElement(); // End li
                }

                writer.WriteEndElement(); // End ul
                writer.WriteEndElement(); // End body
                writer.WriteEndElement(); // End html
                writer.WriteEndDocument();
            }

            SafeCloseOutputStream(context);
        }


        private string GetContentType(string path)
        {
            // If set, use the user-defined content type detection function.
            if (ContentTypeDetector != null)
            {
                return ContentTypeDetector(path);
            }

            // Default MIME type association.
            return MimeTypes.GetContentType(path);
        }

        private void SafeCloseOutputStream(HttpListenerContext context)
        {
            try
            {
                context.Response.OutputStream.Close();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Ignoring exception when closing stream: " + ex.Message);
            }
        }
    }
}
