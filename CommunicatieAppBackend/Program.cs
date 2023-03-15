using CommunicatieAppBackend;
using CommunicatieAppBackend.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

// using CommunicatieAppBackend.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("CommunicatieAppBackendIdentityDbContextConnection") ?? throw new InvalidOperationException("Connection string 'CommunicatieAppBackendIdentityDbContextConnection' not found.");

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>().AddDefaultUI();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
 builder.Services.AddAuthentication();
//auth key for api
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
// }).AddJwtBearer(options =>
// {
//     options.SaveToken = true;
//     options.RequireHttpsMetadata = false;
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = false,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
//         ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
//         IssuerSigningKey =
//             new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:Key"))),
//     };
// });

// const string allowOrigins = "_allowOrigins";

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy(name: allowOrigins,
//         policy =>
//         {
//             policy.AllowAnyHeader().AllowAnyOrigin();
//         });
// });


var app = builder.Build();

app.MapHub<NotificationHub>("/Notification");

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


// app.MapAreaControllerRoute(
//     name: "default",
//     areaName:"Identity",
//     pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
// );

app.UseEndpoints(endpoints =>{
    endpoints.MapGet("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));

    endpoints.MapPost("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
