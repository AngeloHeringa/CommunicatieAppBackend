using Microsoft.AspNetCore.Identity;

namespace CommunicatieAppBackend.Models;
public class RoleViewModel{
    public IdentityUser user {get;set;}
    public Boolean isAdmin {get;set;}
}