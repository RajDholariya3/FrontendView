using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register Services (Remove Duplicates)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<CommonVariable>();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<FrontendView.Helper.MailService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Configure CORS (Ensure it's properly placed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ Configure Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";  // Ensure this route exists
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ✅ Define Image Directory
var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Images");
Console.WriteLine($"Image Directory Path: {imageDirectory}");

// ✅ Ensure Image Directory Exists
if (!Directory.Exists(imageDirectory))
{
    Directory.CreateDirectory(imageDirectory);
}

// ✅ Middleware Order (Fixed)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ CORS should come **before** Routing
app.UseCors("AllowAll");

app.UseRouting();

// ✅ Ensure session is set **before** authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Serve Images from 'App_Data/Images'
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imageDirectory),
    RequestPath = "/images",
    ServeUnknownFileTypes = true, // Allows serving any file type
});

// ✅ Configure Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
