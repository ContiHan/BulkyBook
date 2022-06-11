using BulkyBook.Data;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BulkyBook.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = sortOrder == "Name" ? "NameDesc" : "Name";
            ViewData["OrderSortParm"] = sortOrder == "Order" ? "OrderDesc" : "Order";
            ViewData["CurrentFilter"] = searchString;
            var categories = from c in _dbContext.Categories
                             select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                categories = categories
                    .Where(s => s.Name != null && s.Name.Contains(searchString) || s.DisplayOrder.ToString().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "Name":
                    categories = categories.OrderBy(c => c.Name);
                    break;
                case "NameDesc":
                    categories = categories.OrderByDescending(c => c.Name);
                    break;
                case "Order":
                    categories = categories.OrderBy(c => c.DisplayOrder);
                    break;
                case "OrderDesc":
                    categories = categories.OrderByDescending(c => c.DisplayOrder);
                    break;
                default:
                    categories = categories.OrderBy(c => c.Id);
                    break;
            }

            return View(await categories.Take(10).AsNoTracking().ToListAsync());
        }

        // GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        // POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category objectCategory)
        {
            if (objectCategory.Name == objectCategory.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _dbContext.Add(objectCategory);
                await _dbContext.SaveChangesAsync();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(objectCategory);
        }

        // GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null || _dbContext.Categories is null)
            {
                return NotFound();
            }
            var categoryFromDb = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (categoryFromDb is null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category objectCategory)
        {
            if (objectCategory.Name == objectCategory.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _dbContext.Update(objectCategory);
                await _dbContext.SaveChangesAsync();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(objectCategory);
        }

        // GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || _dbContext.Categories is null)
            {
                return NotFound();
            }
            var categoryFromDb = await _dbContext.Categories.FindAsync(id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (_dbContext.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var categoryFromDb = await _dbContext.Categories.FindAsync(id);
            if (categoryFromDb is null)
            {
                return NotFound();
            }

            _dbContext.Categories.Remove(categoryFromDb);
            await _dbContext.SaveChangesAsync();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> FillTable()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var categories = new List<Category>();
            if (categories.Count == 0)
            {
                for (int i = 0; i < 10_000; i++)
                {
                    var category = new Category
                    {
                        Name = $"Generated {i}",
                        DisplayOrder = 66
                    };
                    categories.Add(category);
                }
            }

            await _dbContext.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();

            stopWatch.Stop();
            TempData["info"] = $"Fill table took {stopWatch.Elapsed.TotalSeconds:N3} seconds";
            return RedirectToAction("Index");
        }

        // Unefficient way how to wipe table
        //public async Task<IActionResult> WipeTable()
        //{
        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();

        //    if (_dbContext.Categories is not null)
        //    {
        //        var categories = _dbContext.Categories.ToList();
        //        _dbContext.RemoveRange(categories);
        //        await _dbContext.SaveChangesAsync();
        //    }

        //    stopWatch.Stop();
        //    TempData["info"] = $"Wipe table took {stopWatch.Elapsed.TotalSeconds:N2} seconds";
        //    return RedirectToAction("Index");
        //}

        public async Task<IActionResult> WipeTableOptimized()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Categories]");

            stopWatch.Stop();
            TempData["info"] = $"Wipe table q? took {stopWatch.Elapsed.TotalSeconds:N3} seconds";
            return RedirectToAction("Index");
        }

        // Only fool will use it :D this method will remove the whole DB with all tables and recreate db with tables, but the migration history table not
        //public async Task<IActionResult> RecreateDb()
        //{
        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    await _dbContext.Database.EnsureDeletedAsync();
        //    await _dbContext.Database.EnsureCreatedAsync();
        //    stopWatch.Stop();
        //    TempData["info"] = $"Recreate DB took {stopWatch.Elapsed.TotalSeconds} seconds";
        //    return RedirectToAction("Index");
        //}
    }
}
