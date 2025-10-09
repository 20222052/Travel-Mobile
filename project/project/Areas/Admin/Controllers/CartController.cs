using Microsoft.AspNetCore.Mvc;

namespace project.Areas.Admin.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
