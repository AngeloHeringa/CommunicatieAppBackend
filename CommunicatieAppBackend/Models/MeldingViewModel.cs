
namespace CommunicatieAppBackend.Models;
public class MeldingViewModel
    {
        public Melding melding { get; set; }
        public IEnumerable<Locatie> Locaties {get; set;}
    }
