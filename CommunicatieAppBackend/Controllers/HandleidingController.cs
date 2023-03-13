using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend.Controllers;

public class HandleidingController : Controller{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public HandleidingController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<ViewResult> Index() => View(await _context.Handleidingen.ToListAsync());

        public async Task<IActionResult> Create()
    {
        return View(new HandleidingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HandleidingViewModel model)
    {
        if (model.Document == null || model.Document.Length == 0)
        {
            return Content("File not selected");
        }
        var path = Path.Combine(_environment.WebRootPath, "Document/Handleiding", model.Document.FileName);
        Console.WriteLine(path);

        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            await model.Document.CopyToAsync(stream);
            stream.Close();
        }

        model.Handleiding.Document = model.Document.FileName;

        if (model != null)
        {
            Console.WriteLine(model.Handleiding.Document);
            var handleiding = new Handleiding
            {
                Title = model.Handleiding.Title,
                Details = model.Handleiding.Details,
                Document = model.Handleiding.Document,
                id = model.Handleiding.id
            };
            _context.Add(handleiding);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));

    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Handleidingen == null)
        {
            return NotFound();
        }

        var Handleiding = await _context.Handleidingen.FirstOrDefaultAsync(it=>it.id==id);
        if (Handleiding == null)
        {
            return NotFound();
        }
        var path = Path.Combine(_environment.WebRootPath, "Document/Handleiding", Handleiding.Document);
        using (var stream = System.IO.File.OpenRead(path))
        {
        IEnumerable<Locatie> locaties = await _context.Locaties.ToListAsync();
        return View(new HandleidingViewModel{
            Handleiding = Handleiding,
            Document = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name)),
            });
        }

    }

    // POST: handleidingen/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]  
    // [Route("Handleiding/Edit")]  
    public async Task<IActionResult> Edit(int id, HandleidingViewModel model)
    {
        // Console.WriteLine(handleidingViewModel.Locaties.First());
        if (id != model.Handleiding.id)
        {
            return NotFound();
        }
        if (model.Document != null && model.Document.Length > 0)
        {
            var path = Path.Combine(_environment.WebRootPath, "Document/Handleiding", model.Document.FileName);
            Console.WriteLine(path);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await model.Document.CopyToAsync(stream);
                stream.Close();
            }

            model.Handleiding.Document = model.Document.FileName;
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(model.Handleiding);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HandleidingExists(model.Handleiding.id))
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
        return View(model);
    }

    private bool HandleidingExists(int id)
    {
        return _context.Handleidingen.Any(e => e.id == id);
    }


    // GET: handleidingen/Delete/5
    // [Route("Delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Handleidingen == null)
        {
            return NotFound();
        }

        var Handleiding = await _context.Handleidingen
            .FirstOrDefaultAsync(m => m.id == id);
        if (Handleiding == null)
        {
            return NotFound();
        }

        return View(Handleiding);
    }
        // POST: nieuwsberichten/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    // [Route("DeleteConfirm")]
    public async Task<IActionResult> Delete(int id)
    {
        if (_context.Handleidingen == null)
        {
            return Problem("Entity set 'PocketSkillsDbContext.nieuwsberichten'  is null.");
        }
        var handleiding = await _context.Handleidingen.SingleOrDefaultAsync(it=>it.id==id);
        if (handleiding != null)
        {
            Console.WriteLine("removing "+handleiding.id+" "+handleiding.Title+" "+handleiding.Details);
            _context.Handleidingen.Remove(handleiding);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    [Route("Handleiding/Get")]
    public async Task<GetHandleidingDTO> getAll(){
        return new GetHandleidingDTO
        {
            Handleidingen = await _context.Handleidingen.ToListAsync()
        };
    }

        public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Handleidingen == null)
        {
            return NotFound();
        }

        var handleiding = await _context.Handleidingen.FirstOrDefaultAsync(m => m.id == id);
        if (handleiding == null)
        {
            return NotFound();
        }

        return View(handleiding);
    }


} 