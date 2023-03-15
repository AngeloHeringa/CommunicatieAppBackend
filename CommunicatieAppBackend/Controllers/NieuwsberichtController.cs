using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Hubs;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend.Controllers;
public class NieuwsberichtController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    private IHubContext<NotificationHub> HubContext{ get; set; }


    public NieuwsberichtController(AppDbContext context, IWebHostEnvironment environment, IHubContext<NotificationHub> hubContext)
    {
        HubContext = hubContext;
        _context = context;
        _environment = environment;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(String searchString)
    {
        if (searchString != null)
        {
            return View(_context.nieuwsberichten.Where(x => x.Titel.Contains(searchString) || searchString == null).Include(m=>m.Locatie).ToList());
        }
        return View(await _context.nieuwsberichten.Include(m=>m.Locatie).ToListAsync());
    }

    // GET: nieuwsberichten/Details/5
    // [Route("Details")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.nieuwsberichten == null)
        {
            return NotFound();
        }

        var Nieuwsbericht = await _context.nieuwsberichten.Include(it=>it.Locatie)
            .FirstOrDefaultAsync(m => m.NieuwsberichtId == id);
        if (Nieuwsbericht == null)
        {
            return NotFound();
        }

        return View(Nieuwsbericht);
    }

    // GET: nieuwsberichten/Create
    // [Route("Create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        return View(new NieuwsberichtViewModel{
            Locaties= await _context.Locaties.ToListAsync()
        });
    }

    // POST: nieuwsberichten/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    // [Route("Create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(NieuwsberichtViewModel model)
    {
        if (model.Foto == null || model.Foto.Length == 0)
        {
            return Content("File not selected");
        }
        var path = Path.Combine(_environment.WebRootPath, "Image/Nieuws", model.Foto.FileName);
        Console.WriteLine(path);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            await model.Foto.CopyToAsync(stream);
            stream.Close();
        }

        model.nieuwsbericht.Image = model.Foto.FileName;

        if (model != null)
        {
            Console.WriteLine(model.nieuwsbericht.Image);
            var nieuwsbericht = new Nieuwsbericht
            {
                Titel = model.nieuwsbericht.Titel,
                Inhoud = model.nieuwsbericht.Inhoud,
                Datum = model.nieuwsbericht.Datum,
                Image = model.nieuwsbericht.Image,
                LocatieId = model.nieuwsbericht.LocatieId
                // ImageLocation = path,
            };
            _context.Add(nieuwsbericht);
            await _context.SaveChangesAsync();
            await HubContext.Clients.All.SendAsync("ReceiveNotification",nieuwsbericht.Titel,nieuwsbericht.Inhoud, nieuwsbericht.Image);
        }
        return RedirectToAction(nameof(Index));

        // Nieuwsbericht.NieuwsberichtId=await _context.nieuwsberichten.MaxAsync(it=>it.NieuwsberichtId)+1;
        // Console.WriteLine("creating "+Nieuwsbericht.NieuwsberichtId+"..");
        
        // if (ModelState.IsValid)
        // {
        //     _context.Add(Nieuwsbericht);
        //     await _context.SaveChangesAsync();
        //     return RedirectToAction(nameof(Index));
        // } else Console.WriteLine("creating failed");
        // return View(Nieuwsbericht);
    }

    // GET: nieuwsberichten/Edit/5
    // [Route("Nieuwsbericht/Edit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.nieuwsberichten == null)
        {
            return NotFound();
        }

        var Nieuwsbericht = await _context.nieuwsberichten.Include(it=>it.Locatie).FirstOrDefaultAsync(it=>it.NieuwsberichtId==id);
        if (Nieuwsbericht == null)
        {
            return NotFound();
        }
        var path = Path.Combine(_environment.WebRootPath, "Image/Nieuws", Nieuwsbericht.Image);
        using (var stream = System.IO.File.OpenRead(path))
        {
        IEnumerable<Locatie> locaties = await _context.Locaties.ToListAsync();
        return View(new NieuwsberichtViewModel{
            nieuwsbericht = Nieuwsbericht,
            Locaties = locaties,
            Foto = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name)),
            });
        }

    }

    // POST: nieuwsberichten/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]  
    // [Route("Nieuwsbericht/Edit")]  
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, NieuwsberichtViewModel model)
    {
        // Console.WriteLine(nieuwsberichtViewModel.Locaties.First());
        if (id != model.nieuwsbericht.NieuwsberichtId)
        {
            return NotFound();
        }
        if (model.Foto != null && model.Foto.Length > 0)
        {
            var path = Path.Combine(_environment.WebRootPath, "Image/Nieuws", model.Foto.FileName);
            Console.WriteLine(path);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await model.Foto.CopyToAsync(stream);
                stream.Close();
            }

            model.nieuwsbericht.Image = model.Foto.FileName;
        }
        model.nieuwsbericht.Locatie=await _context.Locaties.SingleOrDefaultAsync(it=>it.Id==model.nieuwsbericht.LocatieId);

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(model.nieuwsbericht);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NieuwsberichtExists(model.nieuwsbericht.NieuwsberichtId))
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
        var errors = ModelState.Where(x => x.Value.Errors.Any())
                .Select(x => new { x.Key, x.Value.Errors });
        foreach (var err in errors){
            Console.WriteLine(err);
            foreach (var item in err.Errors)
            {
                Console.WriteLine(item.ErrorMessage);

            }
        }
        IEnumerable<Locatie> locaties = await _context.Locaties.ToListAsync();
        model.Locaties=locaties;
        foreach (var loc in locaties)
        {
            Console.WriteLine(loc.name);
        }

        // return RedirectToAction(nameof(Index));

        return View(model);
    }

    // GET: nieuwsberichten/Delete/5
    // [Route("Delete")]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
    private bool NieuwsberichtExists(int id)
    {
        return _context.nieuwsberichten.Any(e => e.NieuwsberichtId == id);
    }

    [HttpGet]
    [Route("Nieuwsbericht/Get")]
    public async Task<GetNieuwsberichtDTO> getAll(){
        return new GetNieuwsberichtDTO
        {
            Nieuwsberichten = await _context.nieuwsberichten.Include(m=>m.Locatie).ToListAsync()
        };
    }

    [HttpGet]
    [Route("Nieuwsbericht/GetByLocatie/{id}")]
    public async Task<GetNieuwsberichtDTO> getByLocatie(String id){
        return new GetNieuwsberichtDTO
        {
            Nieuwsberichten = await _context.nieuwsberichten.Include(m=>m.Locatie).Where(n=>n.Locatie.name==id).ToListAsync()
        };
    }
}