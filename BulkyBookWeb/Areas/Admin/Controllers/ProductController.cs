using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var coverTypes = _unitOfWork.CoverType.GetAll();
            return coverTypes != null ?
                    View(coverTypes) :
                    Problem("Entity 'coverTypes' is null.");

        }

        // GET - UPSERT
        public async Task<IActionResult> Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                }),
            };

            if (id is null || _unitOfWork.Product is null)
            {
                // create product
                return View(productVM);
            }
            else
            {
                // update product
            }

            return View(productVM);
        }

        // POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM productVM, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                //_unitOfWork.CoverType.Update(coverType);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Cover type updated successfully";
                return RedirectToAction("Index");
            }
            return View(productVM);
        }

        // GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || _unitOfWork.CoverType is null)
            {
                return NotFound();
            }
            var coverType = await _unitOfWork.CoverType.FirstOrDefaultAsync(c => c.Id == id);
            if (coverType == null)
            {
                return NotFound();
            }
            return View(coverType);
        }

        // POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (_unitOfWork.CoverType is null)
            {
                return Problem("Entity '_unitOfWork.CoverType' is null.");
            }
            var coverType = await _unitOfWork.CoverType.FirstOrDefaultAsync(c => c.Id == id);
            if (coverType is null)
            {
                return NotFound();
            }

            _unitOfWork.CoverType.Remove(coverType);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
