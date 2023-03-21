using CommunicatieAppBackend;
using CommunicatieAppBackend.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("CommunicatieAppBackendIdentityDbContextConnection") ?? throw new InvalidOperationException("Connection string 'CommunicatieAppBackendIdentityDbContextConnection' not found.");

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>().AddDefaultUI();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddMvcCore();
//Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Error/Index";
    options.LoginPath = "/Login/Login";
    options.Cookie.Name = "CookieAuthentication";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Events.OnRedirectToLogin = e =>
    {
        return Task.CompletedTask;
    };
});

//Jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
        ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:Key"))),
    };
});

    
// builder.Services.AddAuthentication(options =>
// {
//     // options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     // options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//     // options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultScheme = "JWT_OR_COOKIE";
//     options.DefaultChallengeScheme = "JWT_OR_COOKIE";

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
// }).AddCookie(options =>
// {
//     options.LoginPath = "/Identity/Account/Login";
//     options.ExpireTimeSpan = TimeSpan.FromDays(1);
// }).AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
//     {
//         // runs on each request
//         options.ForwardDefaultSelector = context =>
//         {
//             // filter by auth type
//             string authorization = context.Request.Headers[HeaderNames.Authorization];
//             if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ")){
//             	Console.WriteLine("use bearer token");
//                 return JwtBearerDefaults.AuthenticationScheme;    
//             }
//             Console.WriteLine("use cookie");
//             // otherwise always check for cookie auth
//             return CookieAuthenticationDefaults.AuthenticationScheme;
//         };
//     });
// ;

const string allowOrigins = "_allowOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowOrigins,
        policy =>
        {
            policy.AllowAnyHeader().AllowAnyOrigin();
        });
});

var app = builder.Build();

app.UseCors();

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


app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();


// app.MapAreaControllerRoute(
//     name: "default",
//     areaName:"Identity",
//     pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
// );

app.UseEndpoints(endpoints =>{
    endpoints.MapGet("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));

    endpoints.MapPost("/Identity/Account/Register", context => Task.Factory.StartNew(() => context.Response.Redirect("/Identity/Account/Login", true, true)));
    endpoints.MapControllers();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();
