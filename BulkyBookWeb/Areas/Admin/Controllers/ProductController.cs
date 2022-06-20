using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
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
            Product product = new();

            IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            IEnumerable<SelectListItem> coverTypeList = _unitOfWork.CoverType.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });

            if (id is null || _unitOfWork.Product is null)
            {
                // create product
                ViewBag.CategoryList = categoryList;
                ViewData["coverTypeList"] = coverTypeList;
                return View(product);
            }
            else
            {
                // update product
            }

            return View(product);
        }

        // POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(coverType);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Cover type updated successfully";
                return RedirectToAction("Index");
            }
            return View(coverType);
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
