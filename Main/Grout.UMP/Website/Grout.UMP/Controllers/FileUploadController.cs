using System.Web.Mvc;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    public class FileUploadController : Controller
    {
        public void Upload()
        {
            new FileUpload().ProcessRequest(System.Web.HttpContext.Current);
        }
    }
}
