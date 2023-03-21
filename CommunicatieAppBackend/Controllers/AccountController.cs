using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommunicatieAppBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Authentication;
using NuGet.Protocol;

namespace CommunicatieAppBackend.Controllers;
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    
    public AccountController(ILogger<AccountController> logger, IConfiguration configuration, UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _logger = logger;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // [HttpPost]
    // [AllowAnonymous]
    // [Route("Register")]
    // public async Task<IActionResult> RegisterUser([FromBody] RegisterDTO model)
    // {
    //     // TODO Probably do some basic filtering here in regard to the input?
    //     _logger.Log(LogLevel.Information, $"Registering user: {model.Username}.");
    //     var userExists = await _userManager.FindByEmailAsync(model.Email);
    //     if (userExists != null)
    //     {
    //         return BadRequest("User exists");
    //     }

    //     var user = new IdentityUser
    //     {
    //         UserName = model.Username,
    //         Email = model.Email,
    //         FirstName = model.Firstname,
    //         LastName = model.Lastname,
            
    //     };
        
    //     var result = await _userManager.CreateAsync(user, model.Password);

    //     if (result.Succeeded)
    //     {
    //         return Ok("Nice");            
    //     }
    //     _logger.Log(LogLevel.Information, $"Registering failed for: {model.Username}: {result.Errors}");
    //     return BadRequest();
    // }
    
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