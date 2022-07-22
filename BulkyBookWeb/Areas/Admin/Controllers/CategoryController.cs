using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = sortOrder == "Name" ? "NameDesc" : "Name";
            ViewData["OrderSortParm"] = sortOrder == "Order" ? "OrderDesc" : "Order";
            ViewData["CurrentFilter"] = searchString;
            var categories = _unitOfWork.Category.GetAll();

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories
                    .Where(s => s.Name != null && s.Name.ToLower().Contains(searchString.ToLower()) || s.DisplayOrder.ToString().ToLower().Contains(searchString.ToLower()));
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

            return categories != null ?
                    View(categories.Take(50)) :
                    Problem("Entity 'categories' is null.");

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
                await _unitOfWork.Category.AddAsync(objectCategory);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(objectCategory);
        }

        // GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null || _unitOfWork.Category is null)
            {
                return NotFound();
            }
            var category = await _unitOfWork.Category.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
            {
                return NotFound();
            }
            return View(category);
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
                _unitOfWork.Category.Update(objectCategory);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(objectCategory);
        }

        // GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || _unitOfWork.Category is null)
            {
                return NotFound();
            }
            var categoryFromDb = await _unitOfWork.Category.FirstOrDefaultAsync(c => c.Id == id);
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
            if (_unitOfWork.Category == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var categoryFromDb = await _unitOfWork.Category.FirstOrDefaultAsync(c => c.Id == id);
            if (categoryFromDb is null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(categoryFromDb);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> FillTable()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            if (!_unitOfWork.Category.GetAll().Any())
            {
                await _unitOfWork.Category.AddAsync(new Category { Name = "Science Fiction", DisplayOrder = 1 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Horror", DisplayOrder = 2 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Detective", DisplayOrder = 3 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Romance", DisplayOrder = 4 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Mystery", DisplayOrder = 5 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Hobby", DisplayOrder = 6 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Fantasy", DisplayOrder = 7 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Animals", DisplayOrder = 8 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Family", DisplayOrder = 9 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Story", DisplayOrder = 10 });
                await _unitOfWork.Category.AddAsync(new Category { Name = "Kids", DisplayOrder = 11 });

                //var generatedCategories = new List<Category>();
                //for (int i = 0; i < 1_000; i++)
                //{
                //    generatedCategories.Add(new Category { Name = $"Generated {i}", DisplayOrder = 66 });
                //}
                //await _unitOfWork.Category.AddRangeAsync(generatedCategories);

                await _unitOfWork.SaveAsync();
            }

            stopWatch.Stop();
            TempData["info"] = $"Fill table took {stopWatch.Elapsed.TotalSeconds:N3} seconds";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> WipeAsync()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await _unitOfWork.Category.WipeAsync();

            stopWatch.Stop();
            TempData["info"] = $"Wipe table took {stopWatch.Elapsed.TotalSeconds:N3} seconds";
            return RedirectToAction("Index");
        }

        //public IActionResult FillTable()
        //{
        //    var stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    var categories = new List<Category>();
        //    if (categories.Count == 0)
        //    {
        //        for (int i = 0; i < 10_000; i++)
        //        {
        //            var category = new Category
        //            {
        //                Name = $"Generated {i}",
        //                DisplayOrder = 66
        //            };
        //            categories.Add(category);
        //        }
        //    }

        //    _db.Add(categories);
        //    _db.Save();

        //    stopWatch.Stop();
        //    TempData["info"] = $"Fill table took {stopWatch.Elapsed.TotalSeconds:N3} seconds";
        //    return RedirectToAction("Index");
        //}

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
