using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CommunicatieAppBackend.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _userManager=userManager;
    }

    public async Task<IActionResult> IndexAsync()
    {
        var user = await _userManager.GetUserAsync(User).ConfigureAwait(false);
        ViewData["isLoggedIn"]=false;    

        if (user!=null){
            ViewData["username"]= user.UserName;
            ViewData["isLoggedIn"]=true;    
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
