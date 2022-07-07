using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
	[Area(nameof(Customer))]
	public class CartController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
