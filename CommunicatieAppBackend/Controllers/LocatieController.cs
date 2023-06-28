using CommunicatieAppBackend;
using CommunicatieAppBackend.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend.Controllers;

[Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme)]
public class LocatieController : Controller{
    private readonly IAppDbContext _context;

    public LocatieController(IAppDbContext context)
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