using System.Web.Mvc;

namespace Grout.UMP.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult HttpError404()
        {
            return View();
        }

        public ActionResult HttpError500()
        {
            return View();
        }
    }
}