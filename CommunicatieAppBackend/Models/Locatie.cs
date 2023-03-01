using System.ComponentModel.DataAnnotations;

namespace CommunicatieAppBackend.Models;
public class Locatie{
    [Key]
    public int Id {get;set;}
    public String name {get;set;}

}