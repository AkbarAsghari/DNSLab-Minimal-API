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
                    return httpContext.GetPublicIPBrowser();
                }
                else
                {
                    return httpContext.GetPublicIP();
                }
            });

            app.MapGet("/json", (HttpContext httpContext) =>
            {
                httpContext.GetPublicIPJson();
            });

            app.Run();
        }
    }

    static class ContextExtensions
    {
        public static string GetPublicIP(this HttpContext context)
        {
            string ip = context.Connection.RemoteIpAddress!.ToString();

            if (context.Connection.RemoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ||
                context.Connection.RemoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                ip = "127.0.0.1";

            return ip;
        }

        public static string GetPublicIPBrowser(this HttpContext context)
        {
            return $"Your public ip is [ {context.GetPublicIP()} ] \r\r" +
                $"---------------Json-------------- \r\r" +
                $"url : ip.dnslab.link/json\r\r" +
                $"result\r\r" +
                $"{context.GetPublicIPJson()}\r\r" +
                $"---------------Hex---------------  \r\r" +
                $"url : ip.dnslab.link/hex\r\r" +
                $"result [ {context.GetPublicIPHex()} ]\r\r" +
                $"---------------About-------------  \r\r" +
                $"Made with ❤️ by DNSLab.link";
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
            return Regex.IsMatch(context.Request.Headers.UserAgent, @"Edge\/\d+") ||
                Regex.IsMatch(context.Request.Headers.UserAgent, @"Chrome\/\d+");
        }
    }
}