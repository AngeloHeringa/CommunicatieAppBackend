using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend.Controllers;
public class NieuwsberichtController : Controller
{
    private readonly AppDbContext _context;

    public NieuwsberichtController(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index(String searchString)
    {
        if (searchString != null)
        {
            return View(_context.nieuwsberichten.Where(x => x.Titel.Contains(searchString) || searchString == null).ToList());
        }
        return View(await _context.nieuwsberichten.ToListAsync());
    }

    // GET: nieuwsberichten/Details/5
    // [Route("Details")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.nieuwsberichten == null)
        {
            return NotFound();
        }

        var Nieuwsbericht = await _context.nieuwsberichten
            .FirstOrDefaultAsync(m => m.NieuwsberichtId == id);
        if (Nieuwsbericht == null)
        {
            return NotFound();
        }

        return View(Nieuwsbericht);
    }

    // GET: nieuwsberichten/Create
    // [Route("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: nieuwsberichten/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Route("Create")]
    public async Task<IActionResult> Create([Bind("Titel,Inhoud,Datum,Image")] Nieuwsbericht Nieuwsbericht)
    {
        Nieuwsbericht.NieuwsberichtId=await _context.nieuwsberichten.MaxAsync(it=>it.NieuwsberichtId)+1;
        Console.WriteLine("creating "+Nieuwsbericht.NieuwsberichtId+"..");
        
        if (ModelState.IsValid)
        {
            _context.Add(Nieuwsbericht);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        } else Console.WriteLine("creating failed");
        return View(Nieuwsbericht);
    }

    // GET: nieuwsberichten/Edit/5
    // [Route("Nieuwsbericht/Edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.nieuwsberichten == null)
        {
            return NotFound();
        }

        var Nieuwsbericht = await _context.nieuwsberichten.FindAsync(id);
        if (Nieuwsbericht == null)
        {
            return NotFound();
        }
        return View(Nieuwsbericht);
    }

    // POST: nieuwsberichten/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]  
    // [Route("Nieuwsbericht/Edit")]  
    public async Task<IActionResult> Edit(int id, [Bind("NieuwsberichtId,Titel,Inhoud,Datum,Image")] Nieuwsbericht Nieuwsbericht)
    {
        if (id != Nieuwsbericht.NieuwsberichtId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(Nieuwsbericht);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NieuwsberichtExists(Nieuwsbericht.NieuwsberichtId))
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
        return View(Nieuwsbericht);
    }

    // GET: nieuwsberichten/Delete/5
    // [Route("Delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.nieuwsberichten == null)
        {
            return NotFound();
        }

        var Nieuwsbericht = await _context.nieuwsberichten
            .FirstOrDefaultAsync(m => m.NieuwsberichtId == id);
        if (Nieuwsbericht == null)
        {
            return NotFound();
        }

        return View(Nieuwsbericht);
    }

    // POST: nieuwsberichten/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    // [Route("DeleteConfirm")]
    public async Task<IActionResult> Delete(int id)
    {
        if (_context.nieuwsberichten == null)
        {
            return Problem("Entity set 'PocketSkillsDbContext.nieuwsberichten'  is null.");
        }
        var Nieuwsbericht = await _context.nieuwsberichten.SingleOrDefaultAsync(it=>it.NieuwsberichtId==id);
        if (Nieuwsbericht != null)
        {
            Console.WriteLine("removing "+Nieuwsbericht.NieuwsberichtId+" "+Nieuwsbericht.Titel+" "+Nieuwsbericht.Inhoud+" "+Nieuwsbericht.Datum);
            _context.nieuwsberichten.Remove(Nieuwsbericht);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool NieuwsberichtExists(int id)
    {
        return _context.nieuwsberichten.Any(e => e.NieuwsberichtId == id);
    }

    [HttpGet]
    [Route("Nieuwsbericht/Get")]
    public async Task<GetNieuwsberichtDTO> getAll(){
        return new GetNieuwsberichtDTO
        {
            Nieuwsberichten = await _context.nieuwsberichten.ToListAsync()
        };
    }
}