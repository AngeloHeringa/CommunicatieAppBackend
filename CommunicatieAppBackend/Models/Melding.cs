
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunicatieAppBackend.Models;

public class Melding{
    [Key]
    public int MeldingId { get; set; }
    public String Titel { get; set; }
    public String Inhoud{ get; set; }
    public DateTime Datum { get; set; }
    [ForeignKey("Locatie")]
    public int LocatieId {get;set;}
    public Locatie Locatie {get;set;}
}