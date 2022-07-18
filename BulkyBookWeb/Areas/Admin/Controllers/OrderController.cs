using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = await _unitOfWork.OrderHeader.FirstOrDefaultAsync(u => u.Id == orderId, includeProperties: nameof(ApplicationUser)),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: nameof(Product))
            };
            return View(OrderVM);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.RoleAdmin) || User.IsInRole(SD.RoleEmployee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: nameof(ApplicationUser));
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: nameof(ApplicationUser));
            }

            switch (status)
            {
                case "inProcess":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusInProcess);
                    break;
                case "pending":
                    orderHeaders = orderHeaders.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusShipped);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
