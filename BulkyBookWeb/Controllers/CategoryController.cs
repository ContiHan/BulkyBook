﻿using BulkyBookWeb.Data;
using BulkyBookWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public CategoryController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        //public IActionResult Index()
        //{
        //    IEnumerable<Category> objectCategoryList = dbContext.Categories;
        //    return View(objectCategoryList);
        //}

        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "NameDesc" : "";
            ViewData["OrderSortParm"] = sortOrder == "Order" ? "OrderDesc" : "Order";
            var categories = from c in dbContext.Categories
                             select c;

            switch (sortOrder)
            {
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
                    categories = categories.OrderBy(c => c.Name);
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
        public IActionResult Create(Category objectCategory)
        {
            if (objectCategory.Name == objectCategory.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                dbContext.Categories.Add(objectCategory);
                dbContext.SaveChanges();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(objectCategory);
        }

        // GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = dbContext.Categories.Find(id);
            if (categoryFromDb == null)
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
                dbContext.Categories.Update(objectCategory);
                dbContext.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(objectCategory);
        }

        // GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = dbContext.Categories.Find(id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST - DELET
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var categoryFromDb = dbContext.Categories.Find(id);
            if (id is null)
            {
                return NotFound();
            }

            dbContext.Categories.Remove(categoryFromDb);
            dbContext.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
