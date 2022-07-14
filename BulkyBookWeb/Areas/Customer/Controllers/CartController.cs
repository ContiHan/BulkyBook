using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
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

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claim = GetUserIdentity();

            ShoppingCartVM = new()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.PriceBasedOnQuantity = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Count * cart.PriceBasedOnQuantity;
            }

            return View(ShoppingCartVM);
        }

        public async Task<IActionResult> Summary()
        {
            var claim = GetUserIdentity();

            ShoppingCartVM = new()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: nameof(Product)),
                OrderHeader = new()
                {
                    ApplicationUser = await _unitOfWork.ApplicationUser.FirstOrDefaultAsync(u => u.Id == claim.Value)
                }
            };

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.PriceBasedOnQuantity = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Count * cart.PriceBasedOnQuantity;
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName(nameof(Summary))]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST()
        {
            var claim = GetUserIdentity();
            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: nameof(Product));

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.PriceBasedOnQuantity = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Count * cart.PriceBasedOnQuantity;
            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                var ProductId = cart.ProductId;
                var OrderId = ShoppingCartVM.OrderHeader.Id;
                var Price = cart.PriceBasedOnQuantity;
                var Count = cart.Count;

                OrderDetail orderDetail = new()
                {
                    //Count = Count,
                   // Price = Price,
                   // OrderId = OrderId,
                    //ProductId = ProductId,
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index), nameof(HomeController));
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

        private Claim GetUserIdentity()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return claim;
        }
    }
}
