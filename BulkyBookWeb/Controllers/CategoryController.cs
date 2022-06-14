using BulkyBook.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BulkyBook.Controllers
{
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

            if (!String.IsNullOrEmpty(searchString))
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

            return View(categories.Take(10).ToList());
        }

        // GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        // POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category objectCategory)
        {
            if (objectCategory.Name == objectCategory.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(objectCategory);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(objectCategory);
        }

        // GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id is null || _unitOfWork.Category is null)
            {
                return NotFound();
            }
            var categoryFromDb = _unitOfWork.Category.FirstOrDefault(c => c.Id == id);
            if (categoryFromDb is null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category objectCategory)
        {
            if (objectCategory.Name == objectCategory.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(objectCategory);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(objectCategory);
        }

        // GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id is null || _unitOfWork.Category is null)
            {
                return NotFound();
            }
            var categoryFromDb = _unitOfWork.Category.FirstOrDefault(c => c.Id == id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            if (_unitOfWork.Category == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var categoryFromDb = _unitOfWork.Category.FirstOrDefault(c => c.Id == id);
            if (categoryFromDb is null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(categoryFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }

        public IActionResult FillTable()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            if (!_unitOfWork.Category.GetAll().Any())
            {
                _unitOfWork.Category.Add(new Category { Name = "Science Fiction", DisplayOrder = 1 });
                _unitOfWork.Category.Add(new Category { Name = "Horror", DisplayOrder = 2 });
                _unitOfWork.Category.Add(new Category { Name = "Detective", DisplayOrder = 3 });
                _unitOfWork.Category.Add(new Category { Name = "Romance", DisplayOrder = 4 });
                _unitOfWork.Category.Add(new Category { Name = "Mystery", DisplayOrder = 5 });
                _unitOfWork.Category.Add(new Category { Name = "Hobby", DisplayOrder = 6 });
                _unitOfWork.Category.Add(new Category { Name = "Fantasy", DisplayOrder = 7 });
                _unitOfWork.Category.Add(new Category { Name = "Animals", DisplayOrder = 8 });
                _unitOfWork.Category.Add(new Category { Name = "Family", DisplayOrder = 9 });
                _unitOfWork.Category.Add(new Category { Name = "Story", DisplayOrder = 10 });
                _unitOfWork.Category.Add(new Category { Name = "Kids", DisplayOrder = 11 });
                _unitOfWork.Save();
            }

            stopWatch.Stop();
            TempData["info"] = $"Fill table took {stopWatch.Elapsed.TotalSeconds:N3} seconds";
            return RedirectToAction("Index");
        }

        public IActionResult Wipe()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _unitOfWork.Category.Wipe();

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
