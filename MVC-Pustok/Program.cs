using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC_Pustok.Data;
using MVC_Pustok.Models;
using MVC_Pustok.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
}).AddDefaultTokenProviders().AddEntityFrameworkStores<AppDbContext>();


builder.Services.AddScoped<LayoutService>();

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Events.OnRedirectToLogin = opt.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.Value.ToLower().StartsWith("/admin"))
        {
            var uri = new Uri(context.RedirectUri);
            context.Response.Redirect("/admin/account/login" + uri.Query);
        }
        return Task.CompletedTask;
    };

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
           name: "areas",
           pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
         );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
