using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend.Controllers;
public class MeldingController : Controller
{
    private readonly AppDbContext _context;

    public MeldingController(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index(String searchString)
    {
        if (searchString != null)
        {
            return View(_context.meldingen.Where(x => x.Titel.Contains(searchString) || searchString == null).ToList());
        }
        return View(await _context.meldingen.ToListAsync());
    }

    // GET: meldingen/Details/5
    // [Route("Details")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.meldingen == null)
        {
            return NotFound();
        }

        var melding = await _context.meldingen
            .FirstOrDefaultAsync(m => m.MeldingId == id);
        if (melding == null)
        {
            return NotFound();
        }

        return View(melding);
    }

    // GET: meldingen/Create
    // [Route("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: meldingen/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Route("Create")]
    public async Task<IActionResult> Create([Bind("Titel,Inhoud,Datum")] Melding melding)
    {
        melding.MeldingId=await _context.meldingen.MaxAsync(it=>it.MeldingId)+1;
        Console.WriteLine("creating "+melding.MeldingId+"..");
        
        if (ModelState.IsValid)
        {
            _context.Add(melding);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        } else Console.WriteLine("creating failed");
        return View(melding);
    }

    // GET: meldingen/Edit/5
    // [Route("Melding/Edit")]
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
        return View(melding);
    }

    // POST: meldingen/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]  
    // [Route("Melding/Edit")]  
    public async Task<IActionResult> Edit(int id, [Bind("MeldingId,Titel,Inhoud,Datum")] Melding melding)
    {
        if (id != melding.MeldingId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(melding);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!meldingExists(melding.MeldingId))
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
        return View(melding);
    }

    // GET: meldingen/Delete/5
    // [Route("Delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.meldingen == null)
        {
            return NotFound();
        }

        var melding = await _context.meldingen
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

    private bool meldingExists(int id)
    {
        return _context.meldingen.Any(e => e.MeldingId == id);
    }

    [HttpGet]
    [Route("Melding/Get")]
    public async Task<GetMeldingDTO> getAll(){
        return new GetMeldingDTO
        {
            Meldingen = await _context.meldingen.ToListAsync()
        };
    }
}