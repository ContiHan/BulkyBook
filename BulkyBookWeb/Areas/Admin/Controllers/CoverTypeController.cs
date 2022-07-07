using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
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

        // GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        // POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.CoverType.AddAsync(coverType);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Cover type created successfully";
                return RedirectToAction("Index");
            }
            return View(coverType);
        }

        // GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null || _unitOfWork.CoverType is null)
            {
                return NotFound();
            }
            var coverType = await _unitOfWork.CoverType.FirstOrDefaultAsync(c => c.Id == id);
            if (coverType is null)
            {
                return NotFound();
            }
            return View(coverType);
        }

        // POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CoverType coverType)
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
