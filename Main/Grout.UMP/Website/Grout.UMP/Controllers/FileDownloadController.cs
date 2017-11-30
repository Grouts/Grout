using System;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Grout.UMP.Controllers
{
	public class FileDownloadController : Controller
	{
		public FileResult ContentRedirection()
		{
		    var url = HttpContext.Request.Url.AbsolutePath.Replace("/", "\\");

			var contentType = "image/png";

			var fileExtension = HttpContext.Request.CurrentExecutionFilePathExtension;
			if (fileExtension.ToLower() == ".csv")
			{
				contentType = "text/csv";
			}

		    return new FilePathResult(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
		                              "\\Grout\\" + WebConfigurationManager.AppSettings["ApplicationName"] + "\\" + url, contentType);
		}
	}
}