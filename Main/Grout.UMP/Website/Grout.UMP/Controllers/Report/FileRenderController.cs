using System;
using System.IO;
using System.Net.Mime;
using System.Web.Mvc;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using Grout.UMP.ActionFilters;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    [Authorize]
    [ItemAccessActionFilter]
    public class FileRenderController : Controller
    {
        private readonly IItemManagement itemManagement = new ItemManagement();

        public ActionResult Index(string id, string version = null)
        {
            var itemId = id;

            if (version != null)
            {
                itemId += "/" + version;
            }

            var itemDetail = itemManagement.GetItemDetailsFromItemId(Guid.Parse(id));

            ViewBag.ItemName = itemDetail.Name;
            ViewBag.ItemId = itemId;
            return View("ReportViewer");
        }

        public ActionResult Download(string id, string version = null)
        {
            var itemId = Guid.Parse(id);
            var versionId = 0;
            if (version != null)
                Int32.TryParse(version, out versionId);

            var itemDetail = itemManagement.GetItemDetailsFromItemId(itemId);
            var itemLocation = itemManagement.GetItemLocation(itemId, itemDetail.ItemType, "", versionId);

            var temporaryDirectory =
                Path.Combine(GlobalAppSettings.GetItemsPath() + "Temporary_Files");

            if (Directory.Exists(temporaryDirectory) == false)
            {
                Directory.CreateDirectory(temporaryDirectory);
            }

            var destination =
                Path.Combine(temporaryDirectory + "\\Item_download_" + HttpContext.User.Identity.Name + "_" + itemId + "_" +
                             DateTime.UtcNow.ToString("ddMMyyyyhhmmss") + ".zip");

            if (System.IO.File.Exists(destination))
            {
                System.IO.File.Delete(destination);
            }

            if (itemDetail.ItemType == ItemType.Dashboard)
            {
                ZipManager.CreateZipFromDirectory(itemLocation, destination);
                try
                {
                    var fileBytes = System.IO.File.ReadAllBytes(destination);
                    return File(fileBytes, MediaTypeNames.Application.Octet, itemDetail.Name + ".sydx");
                }
                finally
                {
                    if (System.IO.File.Exists(destination))
                    {
                        System.IO.File.Delete(destination);
                    }
                }

            }
            else if (itemDetail.ItemType == ItemType.Report)
            {
                var fileBytes = System.IO.File.ReadAllBytes(itemLocation);
                return File(fileBytes, "application/octet-stream", itemDetail.Name + ".rdl");
            }
            else if (itemDetail.ItemType == ItemType.Datasource)
            {
                var fileBytes = System.IO.File.ReadAllBytes(itemLocation);
                return File(fileBytes, "application/octet-stream", itemDetail.Name + ".rds");
            }
            else
            {
                ZipManager.CreateZipFromFile(itemLocation, destination);
                try
                {
                    var fileBytes = System.IO.File.ReadAllBytes(destination);
                    return File(fileBytes, MediaTypeNames.Application.Octet, itemDetail.Name + ".zip");
                }
                finally
                {
                    if (System.IO.File.Exists(destination))
                    {
                        System.IO.File.Delete(destination);
                    }
                }
            }
        }

    }
}