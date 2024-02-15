using System.Net;
using System.Text;
using System.Web;
using Fluid;
using WebSocketSharp;

namespace osu_progressCLI.Webserver.Server
{
    public class Webserver
    {
        public static readonly string DEFAULT_PATH = "public/";
        public static readonly string DEFAULT_CSS = "public/css/";
        public static readonly string DEFAULT_HTML = "public/html/";
        public static readonly string DEFAULT_IMAGES = "public/img/";
        public static readonly string DEFAULT_JS = "public/js/";
        public static readonly string DEFAULT_FLUID = "Webserver/Fluid/";
        public static readonly string DEFAULT_VIDEO = "public/video/";
        private static readonly FluidParser parser = new FluidParser();

        private static Webserver instance;
        private static HttpListener listener;
        private Api api;
        private RequestHandler helper;
        private SSEstream sse;

        private Webserver()
        {
            listener = new HttpListener();
            listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{Credentials.Instance.GetConfig().port}/");
            listener.IgnoreWriteExceptions = true;
            listener.Start();

            api = new Api();
            helper = new RequestHandler();
            sse = new SSEstream();
        }

        public static Webserver Instance()
        {
            if (instance == null)
            {
                instance = new Webserver();
            }
            return instance;
        }

        public async Task start()
        {
            await FluidRenderer.setup(parser);
            listener.Start();
            Task listenTask = Task.Run(async () =>
            {
                while (true)
                {
                    var context = await listener.GetContextAsync();
                    await HandleRequest(context);
                }
            });
            await listenTask;
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string path = request.Url.AbsolutePath;
            //Logger.Log(Logger.Severity.Debug, Logger.Framework.Server, path);

            if (
                path.EndsWith(".jpg")
                || path.EndsWith(".jpeg")
                || path.EndsWith(".png")
                || path.EndsWith(".gif")
                || path.EndsWith(".bmp")
                || path.EndsWith(".tiff")
                || path.EndsWith(".ico")
                || path.EndsWith(".webp")
                || path.EndsWith(".svg")
            )
            {
                serveimage(request, response, path);
            }

            if (path.EndsWith(".mp4"))
            {
                Servevid(request, response, Webserver.DEFAULT_VIDEO + path, "video/mp4");
                return;
            }

            if (
                path.EndsWith(".css")
                || path.EndsWith(".html")
                || path.EndsWith(".js")
                || path.EndsWith(".osr")
            )
            {
                if (path.EndsWith(".css"))
                {
                    ServeStaticFile(response, Webserver.DEFAULT_CSS + path, "text/css");
                    return;
                }

                if (path.EndsWith(".html"))
                {
                    ServeStaticFile(response, Webserver.DEFAULT_HTML + path, "text/html");
                    return;
                }

                if (path.EndsWith(".js"))
                {
                    ServeStaticFile(response, Webserver.DEFAULT_JS + path, "text/js");
                    return;
                }

                if (path.EndsWith(".osr"))
                {
                    ServeStaticFile(
                        response,
                        $"{Credentials.Instance.GetConfig().osufolder}/Data/r/{path}",
                        "application/osr"
                    );
                    return;
                }
            }

            if (path.EndsWith(".ogg") || path.EndsWith(".mp3") || path.EndsWith(".wav"))
            {
                serveaudio(request, response, path);
            }

            if (path.Equals("/stream"))
            {
                sse.Setup(request, response, parser);
                return;
            }

            if (path.StartsWith("/api/"))
            {
                api.Route(request, response, parser);
            }
            else if (path.StartsWith("/"))
            {
                helper.route(request, response, parser);
            }
            else
            {
                response.StatusCode = 404;
                response.OutputStream.Close();

                Logger.Log(
                    Logger.Severity.Warning,
                    Logger.Framework.Server,
                    $"Not found: {path}. (Can be Ignored if everything works fine!)"
                );
            }
        }

        public void WriteResponse(
            HttpListenerResponse response,
            string responseString,
            string contentType
        )
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentType = contentType;
                response.ContentLength64 = buffer.Length;

                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Server, e.Message);
            }
        }

        public void Redirect(
            HttpListenerResponse response,
            string redirectLocation,
            bool permanent = false
        )
        {
            try
            {
                int statusCode = permanent ? 301 : 302;

                response.StatusCode = statusCode;
                response.AddHeader("Location", redirectLocation);
                response.ContentType = "text/html";

                string html =
                    $"<!DOCTYPE html><html><head><meta http-equiv='refresh' content='0;url={redirectLocation}'></head><body></body></html>";

                byte[] buffer = Encoding.UTF8.GetBytes(html);
                response.ContentLength64 = buffer.Length;

                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Server, e.Message);
            }
        }

        public void ServeStaticFile(
            HttpListenerResponse response,
            string filePath,
            string contentType
        )
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                WriteResponse(response, content, contentType);
            }
            else
            {
                response.StatusCode = 404;
                string responseString = $"404 - Not Found: File not found at {filePath}";
                WriteResponse(response, responseString, "text/plain");
            }
        }

        public void Servevid(
            HttpListenerRequest request,
            HttpListenerResponse response,
            string filePath,
            string contentType
        )
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] videoBytes;

                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        // Check for a range header in the request

                        if (request.Headers["Range"] is not null)
                        {
                            string rangeHeader = request.Headers["Range"];

                            // Parse the range header
                            string[] rangeValues = rangeHeader.Split('=')[1].Split('-');
                            long startRange = long.Parse(rangeValues[0]);
                            long endRange = 0;
                            if (rangeValues[1].IsNullOrEmpty())
                            {
                                endRange = fs.Length - 1;
                            }
                            else
                            {
                                endRange =
                                    rangeValues.Length > 1
                                        ? long.Parse(rangeValues[1])
                                        : fs.Length - 1;
                            }

                            response.StatusCode = 206;
                            response.Headers.Add(
                                "Content-Range",
                                $"bytes {startRange}-{endRange}/{fs.Length}"
                            );

                            response.ContentLength64 = endRange - startRange + 1;

                            videoBytes = new byte[endRange - startRange + 1];
                            fs.Seek(startRange, SeekOrigin.Begin);
                            fs.Read(videoBytes, 0, videoBytes.Length);
                        }
                        else
                        {
                            response.ContentLength64 = fs.Length;

                            videoBytes = new byte[fs.Length];
                            fs.Read(videoBytes, 0, videoBytes.Length);
                        }
                    }

                    Stream output = response.OutputStream;

                    output.Write(videoBytes, 0, videoBytes.Length);
                    output.FlushAsync();
                    output.Close();
                }
                catch (Exception e) { }
            });
        }

        private void serveaudio(
            HttpListenerRequest request,
            HttpListenerResponse response,
            string path
        )
        {
            if (
                File.Exists(
                    $"{Credentials.Instance.GetConfig().songfolder}{HttpUtility.UrlDecode(path)}"
                )
            )
            {
                response.ContentType = GetContentType(HttpUtility.UrlDecode(path));

                using (
                    FileStream fs = File.OpenRead(
                        $"{Credentials.Instance.GetConfig().songfolder}{HttpUtility.UrlDecode(path)}"
                    )
                )
                {
                    fs.CopyTo(response.OutputStream);
                }
                response.OutputStream.Close();
            }
            return;
        }

        private void serveimage(
            HttpListenerRequest request,
            HttpListenerResponse response,
            string path
        )
        {
            string sanitizedname = Path.GetFileName(path);
            string decodedname = HttpUtility.UrlDecode(path);
            try
            {
                if (File.Exists(DEFAULT_IMAGES + sanitizedname))
                {
                    string contentType = GetContentType(
                        Path.GetExtension(DEFAULT_IMAGES + sanitizedname)
                    );

                    byte[] imageBytes = File.ReadAllBytes(DEFAULT_IMAGES + sanitizedname);
                    response.ContentType = contentType;
                    response.ContentLength64 = imageBytes.Length;
                    response.OutputStream.Write(imageBytes, 0, imageBytes.Length);
                    response.OutputStream.Close();
                }
                else if (File.Exists($"{Credentials.Instance.GetConfig().songfolder}{decodedname}"))
                {
                    string contentType = GetContentType(
                        Path.GetExtension(
                            $"{Credentials.Instance.GetConfig().songfolder}{decodedname}"
                        )
                    );

                    byte[] imageBytes = File.ReadAllBytes(
                        $"{Credentials.Instance.GetConfig().songfolder}{decodedname}"
                    );
                    response.ContentType = contentType;
                    response.ContentLength64 = imageBytes.Length;
                    response.OutputStream.Write(imageBytes, 0, imageBytes.Length);
                    response.OutputStream.Close();
                }
                else
                {
                    response.StatusCode = 404;
                    string responseString = $"404 - Not Found: Image not found at {path}";
                    WriteResponse(response, responseString, "text/plain");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Server, $"{ex.Message}");
            }
        }

        static string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".ico":
                    return "image/x-icon";
                case ".ogg":
                    return "audio/ogg";
                case ".mp3":
                    return "audio/mpeg";
                case ".wav":
                    return "audio/wav";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
