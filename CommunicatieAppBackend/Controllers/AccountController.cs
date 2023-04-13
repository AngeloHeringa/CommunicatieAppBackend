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
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using CommunicatieAppBackend.Models;

namespace CommunicatieAppBackend.Controllers;
[Route("[controller]/[action]")]
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

        return View(await _userManager.Users.Where(it=>it.LockoutEnabled==true&&it.EmailConfirmed==true).ToListAsync());
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
        toApprove.LockoutEnabled=false;

        await _emailSender.SendEmailAsync(toApprove.Email, "Aanmelding geaccepteerd",
            $"Uw aanmelding voor een account is geaccepteerd door de beheerder van de app. U kunt nu terug naar de app gaan om in te loggen.");

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

        await _emailSender.SendEmailAsync(toDeny.Email, "Aanmelding afgewezen",
            $"Uw aanmelding voor een account is afgewezen door de beheerder van de app.");

        await _userManager.DeleteAsync(toDeny);

        return RedirectToAction(nameof(Index));
    }

    // public IActionResult Manage(){
    //     return View();
    // }

    // public IActionResult ApproveRequests(){
    //     return View();
    // }

    public async Task<IActionResult> ConfirmEmail(string userid, string code)
    {            
        var token= code;
        _logger.Log(LogLevel.Debug,code);
        var decodedTokenString = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        IdentityUser user = await _userManager.FindByIdAsync(userid);
        if(user != null)
        {
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
            
            if (result.Succeeded)
            {        
                return Redirect("/Account/Thankyou?status=confirm");
            }
            else
            {                    
                return Redirect("/Account/Thankyou?status=" + result.Errors.ToArray()[0].Description);                    
            }
        }
        else
        {
            return Redirect("/Account/Thankyou?status=Invalid User");
        }

    } 
    

    [HttpPost]
    [AllowAnonymous]
    public async Task<bool> NewPassword([FromBody]ChangePasswordDTO dto)
    {            

        IdentityUser user = await _userManager.FindByEmailAsync(dto.email);
        if(user != null)
        {
            String code = await _userManager.GeneratePasswordResetTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(new UrlActionContext{
                Action = nameof(NewPasswordForm),
                Controller = "Account",
                Values= new { userId = user.Id, code = code, password = user.PasswordHash, returnUrl = Url.Content("~/") },
                Protocol= Request.Scheme});

            await _emailSender.SendEmailAsync(user.Email, "Change your password",
                $"Please change your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            
            return true;
        }
        return false;
    } 

    
    public async Task<IActionResult> NewPasswordForm(string userid, string code)
    {            
        IdentityUser user = await _userManager.FindByIdAsync(userid);
        if(user != null)
        {
            NewPasswordViewModel vm = new NewPasswordViewModel{userId=user.Id, code=code};
            return View(vm);
        }

        return NotFound();
    } 

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> NewPasswordConfirm(NewPasswordViewModel vm)
    {
        if(!ModelState.IsValid){
            return View(vm);
        }
        var token= vm.code;
        _logger.Log(LogLevel.Debug,token);
        var decodedTokenString = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        IdentityUser user = await _userManager.FindByIdAsync(vm.userId);
        if(user != null)
        {
            IdentityResult result = await _userManager.ResetPasswordAsync(user, decodedTokenString, vm.NewPassword);
            
            if (result.Succeeded)
            {        
                return Redirect("/Account/Thankyou?status=confirmPass");
            }
            else
            {                    
                return Redirect("/Account/Thankyou?status=" + result.Errors.ToArray()[0].Description);                    
            }
        }
        else
        {
            return Redirect("/Account/Thankyou?status=Invalid User");
        }

    } 



    [AllowAnonymous]
    public IActionResult Thankyou(string status){
        return View(nameof(Thankyou),status);
    }



    [HttpPost, ActionName("Register")]
    [AllowAnonymous]
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
            EmailConfirmed = false,
            LockoutEnd = DateTime.Now.AddYears(99)
        };

        var result = await _userManager.CreateAsync(user, model.Password);

// var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email }, Request.Scheme);
        // await _emailSender.SendEmailAsync(user.Email, "Confirmation email link", "Beste gebruiker, \nKlik op de link om uw account te bevestigen: "+confirmationLink);
        var userId = await _userManager.GetUserIdAsync(user);
        var returnUrl = Url.Content("~/");
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        _logger.Log(LogLevel.Debug,code);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Action(new UrlActionContext{
            Action = nameof(ConfirmEmail),
            Controller = "Account",
            Values= new { userId = userId, code = code, returnUrl = returnUrl },
            Protocol= Request.Scheme});

        await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        if (result.Succeeded)
        {
            return Ok("Nice");
        }
        _logger.Log(LogLevel.Information, $"Registering failed for: {model.Username}: {result.Errors}");
        return BadRequest();
    }
    
    [HttpPost]
    [AllowAnonymous]
    [ActionName("Login")]
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
    [ActionName("data")]
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

