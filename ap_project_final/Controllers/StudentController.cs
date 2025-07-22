using Microsoft.AspNetCore.Mvc;

namespace ap_project_final.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
