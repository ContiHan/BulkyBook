using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
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
                EntireShoppingCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cartItem in ShoppingCartVM.EntireShoppingCart)
            {
                cartItem.PriceBasedOnQuantity = GetPriceBasedOnQuantity(cartItem.Count, cartItem.Product.Price, cartItem.Product.Price50, cartItem.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += cartItem.Count * cartItem.PriceBasedOnQuantity;
            }

            return View(ShoppingCartVM);
        }

        public async Task<IActionResult> Summary()
        {
            var claim = GetUserIdentity();

            ShoppingCartVM = new()
            {
                EntireShoppingCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: nameof(Product)),
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

            foreach (var cartItem in ShoppingCartVM.EntireShoppingCart)
            {
                cartItem.PriceBasedOnQuantity = GetPriceBasedOnQuantity(cartItem.Count, cartItem.Product.Price, cartItem.Product.Price50, cartItem.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += cartItem.Count * cartItem.PriceBasedOnQuantity;
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName(nameof(Summary))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPOST()
        {
            var claim = GetUserIdentity();
            ShoppingCartVM.EntireShoppingCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: nameof(Product));

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            foreach (var cartItem in ShoppingCartVM.EntireShoppingCart)
            {
                cartItem.PriceBasedOnQuantity = GetPriceBasedOnQuantity(cartItem.Count, cartItem.Product.Price, cartItem.Product.Price50, cartItem.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += cartItem.Count * cartItem.PriceBasedOnQuantity;
            }

            await _unitOfWork.OrderHeader.AddAsync(ShoppingCartVM.OrderHeader);
            await _unitOfWork.SaveAsync();

            foreach (var cartItem in ShoppingCartVM.EntireShoppingCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cartItem.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cartItem.PriceBasedOnQuantity,
                    Count = cartItem.Count
                };
                await _unitOfWork.OrderDetail.AddAsync(orderDetail);
            }
            await _unitOfWork.SaveAsync();


            // STRIPE SETTINGS
            // START
            var domain = "https://localhost:44306/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };

            foreach (var cartItem in ShoppingCartVM.EntireShoppingCart)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(cartItem.PriceBasedOnQuantity * 100),
                        Currency = "czk",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = cartItem.Product.Title,
                        },

                    },
                    Quantity = cartItem.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            await _unitOfWork.SaveAsync();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            // STRIPE SETTINGS
            // END


            //_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.EntireShoppingCart);
            //await _unitOfWork.SaveAsync();

            //return RedirectToAction(nameof(Index), "Home");
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(u => u.Id == id);
            var service = new SessionService();
            Session session = await service.GetAsync(orderHeader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                await _unitOfWork.SaveAsync();
            }
            var entireShoppingCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(entireShoppingCart);
            await _unitOfWork.SaveAsync();

            return View(id);
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
                // if there is last product and someone remove it, remove product from cartItem
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
