using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend.Controllers;

[Route("[controller]/[action]")]
public class RoleController : Controller {
    private UserManager<IdentityUser> _userManager;
    private RoleManager<IdentityRole> _roleManager;

    public RoleController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index() {
        var roleExist = await _roleManager.RoleExistsAsync("Admin");
        if (!roleExist)
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }


        var result=new List<RoleViewModel>();
        var role = "Admin";
        foreach (var user in await _userManager.Users.ToListAsync()){
            foreach(var rol in (await _userManager.GetRolesAsync(user))){Console.WriteLine(rol);};
            var rolesUser = new RoleViewModel{
                user=user,
                isAdmin=await _userManager.IsInRoleAsync(user, role)
            };
            result.Add(rolesUser);
        }
        return View(result);
    }

    public async Task<RedirectToActionResult> ToggleAdmin(string id){
        var roleExist = await _roleManager.RoleExistsAsync("Admin");
        if (!roleExist)
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }


        var user = await _userManager.FindByIdAsync(id);
        var role = "Admin";
        if (await _userManager.IsInRoleAsync(user, role)){
            await _userManager.RemoveFromRoleAsync(user, role);
        }else {
            await _userManager.AddToRoleAsync(user, role);
        }
        return RedirectToAction(nameof(Index));

    }


}

