using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area(nameof(Customer))]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.PriceBasedOnQuantity = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.CartTotal += cart.Count * cart.PriceBasedOnQuantity;
            }

            return View(ShoppingCartVM);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCart.FirstOrDefaultAsync(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCart.FirstOrDefaultAsync(u => u.Id == cartId);

            if (cart.Count <= 1)
            {
                // if there is last product and someone remove it, remove product from cart
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                // only decrement by 1
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }

            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _unitOfWork.ShoppingCart.FirstOrDefaultAsync(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(int quantity, double price, double price50, double price100)
        {
            switch (quantity)
            {
                case > 100: return price100;
                case > 50: return price50;
                default: return price;
            }
        }
    }
}
