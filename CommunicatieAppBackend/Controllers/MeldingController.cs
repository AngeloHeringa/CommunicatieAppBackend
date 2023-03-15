using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Hubs;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace CommunicatieAppBackend.Controllers;
public class MeldingController : Controller
{
    private readonly AppDbContext _context;
    private IHubContext<NotificationHub> HubContext{ get; set; }

    public MeldingController(AppDbContext context, IHubContext<NotificationHub> hubcontext)
    {
        HubContext = hubcontext;
        _context = context;
    }
    
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(String searchString)
    {
        if (searchString != null)
        {
            return View(_context.meldingen.Where(x => x.Titel.Contains(searchString) || searchString == null).Include(m=>m.Locatie).ToList());
        }
        return View(await _context.meldingen.Include(m=>m.Locatie).ToListAsync());
    }

    // GET: meldingen/Details/5
    // [Route("Details")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.meldingen == null)
        {
            return NotFound();
        }

        var melding = await _context.meldingen.Include(it=>it.Locatie)
            .FirstOrDefaultAsync(m => m.MeldingId == id);
        if (melding == null)
        {
            return NotFound();
        }

        return View(melding);
    }

    // GET: meldingen/Create
    // [Route("Create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        return View(new MeldingViewModel{
            Locaties= await _context.Locaties.ToListAsync()
        });
    }

    // POST: meldingen/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Route("Create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(MeldingViewModel model)
    {
        model.melding.MeldingId=await _context.meldingen.MaxAsync(it=>it.MeldingId)+1;
        // Console.WriteLine("creating "+model.melding.MeldingId+"..");
        
        if (model!=null)
        {
            var melding = new Melding{
                MeldingId = model.melding.MeldingId,
                Titel = model.melding.Titel,
                Inhoud = model.melding.Inhoud,
                Datum = model.melding.Datum,
                LocatieId = model.melding.LocatieId,
                Locatie = model.melding.Locatie
            };
            _context.Add(melding);
            await _context.SaveChangesAsync();
            await HubContext.Clients.All.SendAsync("ReceiveNotification",melding.Titel,melding.Inhoud);
            return RedirectToAction(nameof(Index));
        } else Console.WriteLine("creating failed");
        return RedirectToAction(nameof(Index));
    }

    // GET: meldingen/Edit/5
    // [Route("Melding/Edit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.meldingen == null)
        {
            return NotFound();
        }

        var melding = await _context.meldingen.FindAsync(id);
        if (melding == null)
        {
            return NotFound();
        }
        return View(new MeldingViewModel{
            Locaties= await _context.Locaties.ToListAsync(),
            melding=melding
        });

    }

    // POST: meldingen/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]  
    // [Route("Melding/Edit")]  
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id, MeldingViewModel mvm)
    {
        Console.WriteLine(id+" "+mvm.Locaties.ToJson()+" "+mvm.melding.Titel);

        if (id != mvm.melding.MeldingId)
        {
            return NotFound();
        }

    
        try
        {
            _context.Update(mvm.melding);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!meldingExists(mvm.melding.MeldingId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
        
    }

    // GET: meldingen/Delete/5
    // [Route("Delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.meldingen == null)
        {
            return NotFound();
        }

        var melding = await _context.meldingen.Include(it=>it.Locatie)
            .FirstOrDefaultAsync(m => m.MeldingId == id);
        if (melding == null)
        {
            return NotFound();
        }

        return View(melding);
    }

    // POST: meldingen/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    // [Route("DeleteConfirm")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (_context.meldingen == null)
        {
            return Problem("Entity set 'PocketSkillsDbContext.meldingen'  is null.");
        }
        var melding = await _context.meldingen.SingleOrDefaultAsync(it=>it.MeldingId==id);
        if (melding != null)
        {
            Console.WriteLine("removing "+melding.MeldingId+" "+melding.Titel+" "+melding.Inhoud+" "+melding.Datum);
            _context.meldingen.Remove(melding);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    private bool meldingExists(int id)
    {
        return _context.meldingen.Any(e => e.MeldingId == id);
    }

    [HttpGet]
    [Route("Melding/Get")]
    public async Task<GetMeldingDTO> getAll(){
        return new GetMeldingDTO
        {
            Meldingen = await _context.meldingen.Include(m=>m.Locatie).ToListAsync()
        };
    }

    [HttpGet]
    [Route("Melding/GetByLocatie/{Id}")]
    public async Task<GetMeldingDTO> getByLocatie(String Id){
        return new GetMeldingDTO
        {
            Meldingen = await _context.meldingen.Include(m=>m.Locatie).Where(n=>n.Locatie.name==Id).ToListAsync()
        };
    }
}
 