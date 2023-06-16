using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DNSLabMinimalAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            app.MapGet("/hex", (HttpContext httpContext) => httpContext.GetPublicIPHex());

            app.MapGet("", (HttpContext httpContext) =>
            {
                if (httpContext.IsBrowser())
                {
                    httpContext.Response.ContentType = "text/html";
                    httpContext.Response.Headers.Add("Server", "dnslab.link");
                    return httpContext.GetPublicIPBrowser();
                }
                else
                {
                    return httpContext.GetPublicIP();
                }
            });

            app.MapGet("/json", (HttpContext httpContext) =>
            {
               return httpContext.GetPublicIPJson();
            });

            app.Run();
        }
    }

    static class ContextExtensions
    {
        public static string GetPublicIP(this HttpContext context)
        {
            string ip = context.Connection.RemoteIpAddress!.ToString();

            if (context.Connection.RemoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                ip = "127.0.0.1";

            return ip;
        }

        public static string GetPublicIPBrowser(this HttpContext context)
        {
            return 
                $"<html lang='en'>" +
                $"<head>" +
                $"<meta charset='utf-8'>" +
                $"<meta name='viewport' content='width=device-width, initial-scale=1'>" +
                $"<title>Public IP Address</title>" +
                $"</head>" +
                $"<body style='font-family: monospace;'>" +
                $"<div>" +
                $"  <h1>IP.DNSLab.link - what is my IP address? </h1>" +
                $"  <p>IP Address : {context.GetPublicIP()}</p>" +
                $"  <h2>Simple cURL API</h2>" +
                $"  <hr>" +
                $"  <p>$curl ip.dnslab.link ----> {context.GetPublicIP()}" +
                $"  <p>$curl ip.dnslab.link/hex ----> {context.GetPublicIPHex()}" +
                $"  <p>$curl ip.dnslab.link/json ----> {context.GetPublicIPJson()}" +
                $"  <h3>About</h3>" +
                $"  <hr>" +
                $"  <p>Made with \t&#10084; by <a href='https://dnslab.link'>DNSLab.link</a></p>" +
                $"  <p>Repository <a href='https://github.com/AkbarAsghari/DNSLab-Minimal-API'>DNSLab-Minimal-API</a></p>" +
                $"</div>" +
                $"</body>" +
                $"</html>";
        }

        public static string GetPublicIPHex(this HttpContext context)
        {
            return String.Concat(context.GetPublicIP().Split('.').Select(x => int.Parse(x).ToString("X2")));
        }

        public static string GetPublicIPJson(this HttpContext context)
        {
            return JsonSerializer.Serialize(new
            {
                IPv4 = new
                {
                    Address = context.GetPublicIP(),
                    Hex = context.GetPublicIPHex()
                }
            }, options: new JsonSerializerOptions { WriteIndented = true });
        }

        public static bool IsBrowser(this HttpContext context)
        {
            string agent = context.Request.Headers.UserAgent.ToString().ToLower();
            return Regex.IsMatch(agent.ToString().ToLower(), @"edge\/\d+") ||
                Regex.IsMatch(agent.ToString().ToLower(), @"chrome\/\d+") ||
                Regex.IsMatch(agent.ToString().ToLower(), @"firefox\/\d+") ||
                Regex.IsMatch(agent.ToString().ToLower(), @"safari\/\d+");
        }
    }
}