using System.ComponentModel.DataAnnotations;

namespace CommunicatieAppBackend.Models;

public class Nieuwsbericht{
    [Key]
    public int NieuwsberichtId { get; set; }
    public String Titel { get; set; }
    public String Inhoud { get; set; }
    public String? Image { get; set; }    
    public DateTime Datum { get; set; }
}