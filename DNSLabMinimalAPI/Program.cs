using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
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

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            var app = builder.Build();

            app.UseResponseCompression();

            app.MapGet("/hex", (HttpContext httpContext) => httpContext.GetPublicIPHex());
            app.MapGet("/hex", (HttpContext httpContext) => httpContext.GetPublicIPHex());
            app.MapGet("/json", (HttpContext httpContext) => httpContext.GetPublicIPJson());
            app.MapGet("/ip", (HttpContext httpContext) => httpContext.GetPublicIP());

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
                $"<!DOCTYPE html>" +
                $"<html lang='en'>" +
                $"<head>" +
                $"<meta charset='utf-8'>" +
                $"<meta name='viewport' content='width=device-width, initial-scale=1'>" +
                $"<meta name='description' content='Find out what your public IPv4 address and other tools about IP address'>" +
                $"<title>Public IP Address</title>" +
                "<style>tr:nth-child(even) {background-color:#e2e2e2;}</style>" +
                $"</head>" +
                $"<body style='font-family: monospace;'>" +
                $"<div>" +
                $"  <h1>IP.DNSLab.link - what is my IP address? </h1>" +
                $"  <p>IP Address : {context.GetPublicIP()}</p>" +
                $"  <h2>Simple cURL API</h2>" +
                $"  <hr>" +
                $"  <table>" +
                $"  <tr><th>Command</th><th>Result</th></tr>" +
                $"  <tr><td>$curl ip.dnslab.link</td><td>{context.GetPublicIP()}</td></tr>" +
                $"  <tr><td>$curl ip.dnslab.link/ip</td><td>{context.GetPublicIP()}</td></tr>" +
                $"  <tr><td>$curl ip.dnslab.link/hex</td><td>{context.GetPublicIPHex()}</td></tr>" +
                $"  <tr><td>$curl ip.dnslab.link/json</td><td>{context.GetPublicIPJson()}</td></tr>" +
                $"  </table>" +
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