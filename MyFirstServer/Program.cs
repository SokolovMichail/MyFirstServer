using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySimpleServer
{
	class Server
	{
		public static HttpListener Listener;
		public static string Url = "http://localhost:8000/";
		public static bool Runs = true;
		public static int requestCount = 0;
		public static string pageData = 
			"<!DOCTYPE>" +
			"<html>" +
			"  <head>" +
			"    <title>HttpListener Example</title>" +
			"  </head>" +
			"  <body>" +
			"    <p>Page Views: {0}</p>" +
			"  </body>" +
			"</html>";

		private static string _directory = "D:\\SomeTestDirectory";
		
		
		
		public static async Task HandleConns()
		{
			HttpListenerContext ctx = await Listener.GetContextAsync();

			// Peel out the requests and response objects
			HttpListenerRequest req = ctx.Request;
			HttpListenerResponse resp = ctx.Response;

			// If `shutdown` url requested w/ POST, then shutdown the server after serving the page
			if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
			{
				Console.WriteLine("Shutdown requested");
				Runs = false;
			}
			byte[] data = new byte[]{};
			int code = 404;
			if (req.QueryString["filename"] != "")
			{
				Console.WriteLine(req.QueryString["filename"]);
				var searchRes = Directory.GetFiles(_directory, req.QueryString["filename"], SearchOption.AllDirectories);
				if (searchRes.Length != 0)
				{
					code = 200;
					data = File.ReadAllBytes(searchRes[0]);
					resp.ContentType = "application/octet-stream";
					resp.ContentEncoding = Encoding.UTF8;
					resp.ContentLength64 = data.LongLength;
					resp.Headers["content-disposition"] = "attachment; filename="+req.QueryString["filename"];
				}
				else
				{
					resp.StatusCode = 404;
					resp.ContentType = "text/html";
				}
			}

			resp.StatusCode = code;
			// Write out to the response stream (asynchronously), then close it
			await resp.OutputStream.WriteAsync(data, 0, data.Length);
			//await resp.OutputStream.
			File.AppendAllLines("log.txt",new string[] {String.Format("{0};{1};{2};{3}",
				DateTime.Now,req.RemoteEndPoint.Address.ToString(),req.QueryString["filename"],code.ToString()) } );
			resp.Close();
		}
		
	    static void Main (string[] args)
        {
	        
	        Listener = new HttpListener();
	        Listener.Prefixes.Add(Url);
	        Listener.Start();
	        Console.WriteLine("Listening for connections on {0}", Url);
	        while (Runs)
	        {
		        Task listenTask = HandleConns();
		        listenTask.GetAwaiter().GetResult();

		        // Close the listener
		        
	        }
	        Listener.Close();
        }
    }
}