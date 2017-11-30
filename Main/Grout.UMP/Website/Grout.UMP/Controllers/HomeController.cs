using System.Web.Mvc;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult PermissionDenied()
        {
            return View();
        }

        public JsonResult GetAllActiveGroupsAndUsers()
        {
            return Json(new { data = new SearchManager().GetAllActiveGroupsAndUsers() });
        }        
    }
}