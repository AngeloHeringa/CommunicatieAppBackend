using CommunicatieAppBackend.Models;

namespace CommunicatieAppBackend.DTOs;

public class GetMeldingDTO{
    public List<Melding> Meldingen { get; set; }
}