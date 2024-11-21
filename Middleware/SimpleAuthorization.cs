using System.Text.RegularExpressions;
using System.Text;
using System.Net.Http.Headers;
using System.Net;

namespace Blog.Middleware;

public class SimpleAuthorization
{
    RequestDelegate next;
    public SimpleAuthorization( RequestDelegate next) => this.next = next;

    public async Task Invoke(HttpContext context)
    {
        if(!context.Request.Path.StartsWithSegments("/admin"))
        {
          await next(context);
          return;
        }

        if(!context.Request.Headers.ContainsKey("Authorization"))
        {
            Headers(context);
            return;
        }

        var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
        var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
        
        var userName = credentials[0];
        var password = credentials[1];

        if(!(userName == "a" && password == "1234"))
        {
            context.Response.StatusCode = 401;
            return;
        }

        await next(context);
    }

    private string[] Decode(string? str)
    {
        if (str == null)
            return Array.Empty<string>();

        string nuevo = Regex.Replace(str.Trim(), @"/Basic\s+/i", "");
        var decoded = Convert.FromBase64String(nuevo);

        return Encoding.UTF8.GetString(decoded).Split(':');
    }
    private void Headers(HttpContext context)
    {
        context.Response.Headers["WWW-Authenticate"] = "Basic";
        context.Response.StatusCode = 401;
    }
}