using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        //public IActionResult Index()
        //{
        //    IEnumerable<Category> objectCategoryList = _dbContext.Categories;
        //    return View(objectCategoryList);
        //}

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
                    .Where(s => s.Name.Contains(searchString) || s.DisplayOrder.ToString().Contains(searchString));
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

            return View(await categories.AsNoTracking().ToListAsync());
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
            var categoryFromDb = await _dbContext.Categories.FindAsync(id);
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
    }
}
