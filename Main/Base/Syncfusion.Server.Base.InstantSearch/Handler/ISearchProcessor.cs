using System.Web;

namespace Syncfusion.UMP.InstantSearch.Base
{
    public interface ISearchProcessor
    {
        string ProcessRequest(HttpContext ctxt);
    }
}