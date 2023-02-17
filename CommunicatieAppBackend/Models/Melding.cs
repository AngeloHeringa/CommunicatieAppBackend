
using System.ComponentModel.DataAnnotations;

namespace CommunicatieAppBackend.Models;

public class Melding{
    [Key]
    public int MeldingId { get; set; }
    public String Titel { get; set; }
    public String Inhoud{ get; set; }
    public DateTime Datum { get; set; }
}