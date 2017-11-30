using System;
using System.Collections.Generic;

namespace Grout.UMP.Models
{
    public static class SearchDropDown
    {
        public static string GetDropDownOptions(List<SearchObject> dataList)
        {
            var htmlContent = String.Empty;
            foreach (var data in dataList)
            {
                if (data.Attribute == null)
                {
                    htmlContent += String.Format("<option value='{0}'>{1}</option>", data.Key, data.Value);
                }
                else
                {
                    htmlContent += String.Format("<option {2} value='{0}' data-property='{2}'>{1}</option>", data.Key, data.Value, data.Attribute);
                }
            }
            return htmlContent;
        }
        public static string GetFilterDropDownOptions(List<FilterSearchObject> dataList)
        {
            var htmlContent = String.Empty;
            foreach (var data in dataList)
            {
                if (data.Attribute == null)
                {
                    htmlContent += String.Format("<option value='{0}'>{1}</option>", data.Key, data.Value);
                }
                else
                {
                    htmlContent += String.Format("<option {2} value='{0}' data-property='{2}'>{1}</option>", data.Key, data.Value, data.Attribute);
                }
            }
            return htmlContent;
        }
    }
}