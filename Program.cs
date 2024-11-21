using Blog.Middleware;
using Blog.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<RazorPagesOptions>( (opt) => opt.Conventions.AddPageRoute("/Home", "/home") );

builder.Services.AddScoped<Data>
( 
    (e) => new Data(
        Path.Combine(builder.Environment.ContentRootPath, "storage"),
        "blog.xml" )
);

var app = builder.Build();

app.UseRouting();
app.UseMiddleware<SimpleAuthorization>();

// Configure the HTTP request pipeline.
app.UseStaticFiles();

app.MapRazorPages();

//

app.Run();
