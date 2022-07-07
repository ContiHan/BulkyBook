using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET - UPSERT
        public IActionResult Upsert(int? id)
        {
            var company = new Company();
            if (id is null || id == 0)
            {
                // create company
                return View(company);
            }
            else
            {
                // update company
                company = _unitOfWork.Company.FirstOrDefault(p => p.Id == id);
                return View(company);
            }
        }

        // POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    // create company
                    await _unitOfWork.Company.AddAsync(company);
                    TempData["success"] = "Company created successfully";
                }
                else
                {
                    // update company
                    _unitOfWork.Company.Update(company);
                    TempData["success"] = "Company updated successfully";
                }
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index");
            }
            return View(company);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = _unitOfWork.Company.GetAll();
            return Json(new { data = companies });
        }

        // POST - DELETE
        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (_unitOfWork.Company is null)
            {
                return Problem("Entity '_unitOfWork.Company' is null.");
            }

            var company = await _unitOfWork.Company.FirstOrDefaultAsync(c => c.Id == id);
            if (company is null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Remove(company);
            await _unitOfWork.SaveAsync();
            return Json(new { success = true, message = "Company deleted successfully" });
        }
        #endregion
    }
}
