using CommunicatieAppBackend.Models;

namespace CommunicatieAppBackend.Models;
public class NieuwsberichtViewModel
    {
        public Nieuwsbericht nieuwsbericht { get; set; }
        public IFormFile Foto { get; set; }
    }
