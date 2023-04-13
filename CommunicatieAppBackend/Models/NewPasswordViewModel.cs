using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CommunicatieAppBackend.Models;
public class NewPasswordViewModel{
    public String userId {get;set;}
    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public String NewPassword {get;set;}

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    [Required(ErrorMessage = "Confirmation Password is required.")]
    public String NewPasswordConfirm {get;set;}
    public String code {get;set;}
}