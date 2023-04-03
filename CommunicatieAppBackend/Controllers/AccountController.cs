using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommunicatieAppBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SendGrid;
using SendGrid.Helpers.Mail;
using NuGet.Protocol;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using CommunicatieAppBackend.Services;

namespace CommunicatieAppBackend.Controllers;
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    private readonly IEmailSender _emailSender;
    
    public AccountController(ILogger<AccountController> logger, IConfiguration configuration, UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
    {
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender=emailSender;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() {

        return View(await _userManager.Users.Where(it=>it.LockoutEnabled==true).ToListAsync());
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(string? id){
        var toApprove = await _userManager.FindByIdAsync(id);

        return View(toApprove);
    }

    [HttpPost, ActionName("Approve")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveConfirm(string id){
        var toApprove = await _userManager.FindByIdAsync(id);
        toApprove.LockoutEnabled=true;

        await _userManager.UpdateAsync(toApprove);

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deny(string? id){
        var toDeny = await _userManager.FindByIdAsync(id);

        return View(toDeny);
    }

    [HttpPost, ActionName("Deny")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DenyConfirm(string id){
        var toDeny= await _userManager.FindByIdAsync(id);

        await _userManager.DeleteAsync(toDeny);

        return RedirectToAction(nameof(Index));
    }

    // public IActionResult Manage(){
    //     return View();
    // }

    // public IActionResult ApproveRequests(){
    //     return View();
    // }

    public string ConfirmEmail(){
        return "success?";
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Account/Register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO model)
    {
        // TODO Probably do some basic filtering here in regard to the input?
        _logger.Log(LogLevel.Information, $"Registering user: {model.Username}.");
        var userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
        {
            return BadRequest("User exists");
        }

        var user = new IdentityUser
        {
            UserName = model.Username,
            NormalizedUserName = model.Username!.ToUpper(),
            Email = model.Email,
            NormalizedEmail = model.Email!.ToUpper(),
            EmailConfirmed = false
        };

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email }, Request.Scheme);
        await _emailSender.SendEmailAsync(user.Email, "Confirmation email link", "Beste gebruiker, \nKlik op de link om uw account te bevestigen: "+confirmationLink);

        
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok("Nice");
        }
        _logger.Log(LogLevel.Information, $"Registering failed for: {model.Username}: {result.Errors}");
        return BadRequest();
    }
    
    [HttpPost]
    [AllowAnonymous]
    [Route("Account/Login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        System.Diagnostics.Trace.TraceInformation("Login post");
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user != null)
        {
            System.Diagnostics.Trace.TraceInformation("user exists");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (result.Succeeded)
            {
                System.Diagnostics.Trace.TraceInformation("Auth success");
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("UserId", user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };                

                foreach(var role in await _userManager.GetRolesAsync(user)) 
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                    Console.WriteLine("role found for user: "+role);
                }

                var authSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Key")));
                var token = new JwtSecurityToken(
                    _configuration.GetValue<string>("Jwt:Issuer"),
                    _configuration.GetValue<string>("Jwt:Audience"), 
                    claims.ToArray(), 
                    expires: DateTime.UtcNow.AddDays(7), 
                    signingCredentials: new SigningCredentials(
                        authSigningKey, SecurityAlgorithms.HmacSha256Signature)
                    );
                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), expiration = token.ValidTo });
            }
        }
        return BadRequest();
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme)]
    [Route("Account/Data")]
    public async Task<IActionResult> GetUserData()
    {
        Console.WriteLine(User.Identity.Name);
        var user = await _userManager.FindByNameAsync(User.Identity.Name);
        Console.WriteLine("userdata: "+user.ToJson());

        return Ok(new
        {
            UserId = user.Id,
            Email = user.Email,
            Username = user.UserName
        });
    }
}

