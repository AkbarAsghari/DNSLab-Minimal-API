namespace DNSLabMinimalAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.MapGet("", (HttpContext httpContext) =>
            {
               return httpContext.Connection.RemoteIpAddress!.ToString();
            });

            app.Run();
        }
    }
}