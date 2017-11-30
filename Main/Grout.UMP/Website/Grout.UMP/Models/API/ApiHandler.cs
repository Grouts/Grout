using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Grout.Base.Encryption;
using System.Web.Security;
using Grout.Base.DataClasses;
using Grout.Base.Logger;
using System.Reflection;

namespace Grout.UMP.Models
{
    public class ApiHandler
    {
        readonly JavaScriptSerializer jserializer = new JavaScriptSerializer();

        public string ApiProcessor(string url, Dictionary<string, object> headers, Dictionary<string, object> inputItems)
        {
            string target;
            var queryString = String.Empty;

            try
            {
                if (inputItems != null)
                {
                    foreach (var inputItem in inputItems)
                    {
                        if (string.IsNullOrEmpty(queryString))
                        {
                            queryString = "?" + inputItem.Key + "=" + inputItem.Value;
                        }
                        else
                        {
                            queryString = queryString + "&" + inputItem.Key + "=" + inputItem.Value;
                        }
                    }
                }

                var requestModeHeader = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(new TokenCryptography().Encrypt("WebServer",
                        HttpContext.Current.Request.UserHostAddress)));

                if (headers == null)
                {
                    headers = new Dictionary<string, object> { { "RequestMode", requestModeHeader } };
                }
                else if (headers.ContainsKey("RequestMode") == false)
                {
                    headers.Add("RequestMode", requestModeHeader);
                }

                var baseUrl = new UriBuilder(HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port);

                var requestUrl = (baseUrl.ToString().TrimEnd('/') + url + queryString);
                LogExtension.LogInfo("API Request URL - " + requestUrl, MethodBase.GetCurrentMethod());

                var request = (HttpWebRequest)WebRequest.Create(requestUrl);

                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value.ToString());
                }
                var response = (HttpWebResponse)request.GetResponse();
                var streamReader = new StreamReader(response.GetResponseStream(), true);
                try
                {
                    target = streamReader.ReadToEnd();
                }
                finally
                {
                    streamReader.Close();
                    LogExtension.LogInfo("API Request successful", MethodBase.GetCurrentMethod(), " Url - " + url + " QueryString - " + queryString + " RequestUrl - " + requestUrl);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogInfo("API Request error", MethodBase.GetCurrentMethod());
                LogExtension.LogError("Error in APi Request", ex, MethodBase.GetCurrentMethod(), " Url - " + url + " QueryString - " + queryString);
                try
                {
                    var webException = ex as WebException;
                    var response = webException.Response as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        FormsAuthentication.SignOut();
                        target =
                            jserializer.Serialize(new ApiResponse
                            {
                                Data = new ApiData { Message = response.StatusCode.ToString() },
                                ApiStatus = false
                            });
                    }
                    else
                    {
                        target =
                       jserializer.Serialize(new ApiResponse
                       {
                           Data = new ApiData { Message = ex.Message },
                           ApiStatus = false
                       });
                    }
                }
                catch (Exception)
                {
                    target =
                        jserializer.Serialize(new ApiResponse
                        {
                            Data = new ApiData { Message = ex.Message },
                            ApiStatus = false
                        });
                }
            }
            return target;
        }

    }
}