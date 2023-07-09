using Microsoft.AspNetCore.Mvc;

namespace Ae.Nuntium.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
