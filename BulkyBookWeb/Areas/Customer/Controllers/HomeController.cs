﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area(nameof(Customer))]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(products);
        }

        public async Task<IActionResult> Details(int? productId)
        {
            if (productId is null || _unitOfWork.Product is null)
            {
                return NotFound();
            }
            ShoppingCart shoppingCart = new()
            {
                Count = 1,
                ProductId = productId.Value,
                Product = await _unitOfWork.Product.FirstOrDefaultAsync(p => p.Id == productId, includeProperties: "Category,CoverType"),
            };

            if (shoppingCart is null)
            {
                return NotFound();
            }
            return View(shoppingCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart shoppingCartFromDb = await _unitOfWork.ShoppingCart.FirstOrDefaultAsync(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId
                );
            if (shoppingCartFromDb is null)
            {
                await _unitOfWork.ShoppingCart.AddAsync(shoppingCart);
                await _unitOfWork.SaveAsync();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
            }
            else
            {
                _unitOfWork.ShoppingCart.IncrementCount(shoppingCartFromDb, shoppingCart.Count);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}