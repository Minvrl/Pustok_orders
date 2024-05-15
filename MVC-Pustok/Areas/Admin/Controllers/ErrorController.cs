using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MVC_Pustok.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin,super_admin")]
    [Area("admin")]
    public class ErrorController : Controller
    {
        public IActionResult NotFound()
        {
            return View();
        }
    }
}
