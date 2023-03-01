using CommunicatieAppBackend;
using CommunicatieAppBackend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend.Controllers;
public class LocatieController : Controller{
    private readonly AppDbContext _context;

    public LocatieController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("Locatie/Get")]
    public async Task<GetLocatiesDTO> getAll(){
        return new GetLocatiesDTO
        {
            locaties = await _context.Locaties.ToListAsync()
        };
    }


}