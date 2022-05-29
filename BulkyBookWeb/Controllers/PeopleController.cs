using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Newtonsoft.Json;

namespace BulkyBookWeb.Controllers
{
    public class PeopleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PeopleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: People
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["FirstNameSort"] = sortOrder == "FirstName" ? "FirstNameDesc" : "FirstName";
            ViewData["LastNameSort"] = sortOrder == "LastName" ? "LastNameDesc" : "LastName";
            ViewData["DateSort"] = sortOrder == "Date" ? "DateDesc" : "Date";
            ViewData["CurrentFilter"] = searchString;
            var people = from person in _context.People
                         select person;

            if (!String.IsNullOrEmpty(searchString))
            {
                people = people
                    .Where(s => s.Jmeno.Contains(searchString) || s.Prijmeni.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "FirstName":
                    people = people.OrderBy(c => c.Jmeno);
                    break;
                case "FirstNameDesc":
                    people = people.OrderByDescending(c => c.Jmeno);
                    break;
                case "LastName":
                    people = people.OrderBy(c => c.Prijmeni);
                    break;
                case "LastNameDesc":
                    people = people.OrderByDescending(c => c.Prijmeni);
                    break;
                case "Date":
                    people = people.OrderBy(c => c.Date);
                    break;
                case "DateDesc":
                    people = people.OrderByDescending(c => c.Date);
                    break;
                default:
                    people = people.OrderBy(c => c.Id);
                    break;
            }

            return _context.People != null ?
                        View(await people.AsNoTracking().ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.People'  is null.");
        }

        // GET: People/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.People == null)
            {
                return NotFound();
            }

            var person = await _context.People
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: People/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.People == null)
            {
                return NotFound();
            }

            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: People/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Jmeno,Prijmeni,Date")] Person person)
        {
            if (id != person.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.Id))
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
            return View(person);
        }

        // GET: People/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.People == null)
            {
                return NotFound();
            }

            var person = await _context.People
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.People == null)
            {
                return Problem("Entity set 'ApplicationDbContext.People'  is null.");
            }
            var person = await _context.People.FindAsync(id);
            if (person != null)
            {
                _context.People.Remove(person);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetUpToDateData()
        {
            string json;
            using (HttpClient client = new())
            {
                json = client.GetStringAsync("https://xevos.store/wp-content/jmena.json").Result;
            }

            var people = JsonConvert.DeserializeObject<List<Person>>(json);

            if (_context.People is not null && _context.People.Any())
            {
                var oldRecords = _context.People.ToList();
                _context.RemoveRange(oldRecords);
                await _context.SaveChangesAsync();
            }

            if (people is not null)
            {
                await _context.AddRangeAsync(people);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
            return (_context.People?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
