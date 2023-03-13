
namespace CommunicatieAppBackend.Models;
public class NieuwsberichtViewModel
    {
        public Nieuwsbericht nieuwsbericht { get; set; }
        public IFormFile? Foto { get; set; }
        public IEnumerable<Locatie> Locaties {get; set;}
    }
    