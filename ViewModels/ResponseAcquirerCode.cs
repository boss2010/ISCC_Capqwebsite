using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class ResponseAcquirerCode
    {
        public static string Response(string order, int BankPort, string BankUrL)
        {

            try
            {
                
                string Code;
                var url = "";
                if (BankUrL == "www.capq.gov.eg" || BankPort == 80 || BankPort == 443)
                {
                    url = "https://nbe.gateway.mastercard.com/api/rest/version/64/merchant/AGRICULTURE/order/" + order + "";

                }
                else
                {
                    url = "https://test-nbe.gateway.mastercard.com/api/rest/version/64/merchant/TESTAGRICULTURE/order/" + order + "";

                }




                var httpRequest = (HttpWebRequest)WebRequest.Create(url);

                httpRequest.Accept = "application/json";
                if (BankUrL == "www.capq.gov.eg" || BankPort == 80 || BankPort == 443)
                {
                    httpRequest.Headers["Authorization"] = "Basic TWVyY2hhbnQuQUdSSUNVTFRVUkU6ZWVkZTUzOGY2N2RlZDE5OTBkYmYwMTllYzM3Mzk0ODk=";

                }
                else
                {
                    httpRequest.Headers["Authorization"] = "Basic bWVyY2hhbnQuVEVTVEFHUklDVUxUVVJFOjU1ZWMyMmNjOTMyNTA3NzA3MGJiMTVkYzc3NWEwNTAz";

                }





                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    ResponseDTO.Root myDeserializedClass = JsonConvert.DeserializeObject<ResponseDTO.Root>(result);
                    //  var x = JsonConvert.DeserializeObject(result);//<IEnumerable<object>>(result);
                    // SerializeObject(result);
                    Code = myDeserializedClass.transaction[myDeserializedClass.transaction.Count - 1].response.acquirerCode;
                }




                ///////////////////////////////

                return Code;
            }
            catch (WebException ex)
            {

                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();

                String ErrorMessage = "";



                ErrorMessage += "Source :" + ex.Source;
                ErrorMessage += "Data :" + ex.Data.ToString();

                ErrorMessage += "GetBaseException :" + ex.GetBaseException().ToString();
                ErrorMessage += "HelpLink :" + ex.HelpLink;
                ErrorMessage += "HResult :" + ex.HResult.ToString();
                ErrorMessage += "Message :" + ex.Message;
                ErrorMessage += "LineNumber :" + line.ToString();
                ErrorMessage += "TargetSite :" + ex.TargetSite.ToString();
                ErrorMessage += "InnerException :" + ex.InnerException;
                using (WebResponse webResponse = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)webResponse;
                    if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        return "NF";
                    }
               
                }

                return ErrorMessage;


            }



        }
        public static string Response_Inspection(string order, int BankPort, string BankUrL)
        {

            try
            {

                string Code;
                var url = "";
                var httpRequest = (HttpWebRequest)WebRequest.Create("https://nbe.gateway.mastercard.com/api/rest/version/64/merchant/AGRICULTURE/order");

               
          

                    if (BankUrL == "www.capq.gov.eg" || BankPort == 80 || BankPort == 443)
                    {
                        url = "https://nbe.gateway.mastercard.com/api/rest/version/64/merchant/AGRICULTURE2/order/" + order + "";

                    }
                    else
                    {
                        url = "https://test-nbe.gateway.mastercard.com/api/rest/version/64/merchant/TESTAGRICULTURE/order/" + order + "";

                    }


                    httpRequest = (HttpWebRequest)WebRequest.Create(url);


                    httpRequest.Accept = "application/json";
                    if (BankPort == 80 || BankPort == 443 || BankUrL == "www.capq.gov.eg")
                    {

                        httpRequest.Headers["Authorization"] = "Basic bWVyY2hhbnQuQUdSSUNVTFRVUkUyOjRiMDQwZTVkOTIwZDA5MDc5ZjdkMTllZWQxZmRmM2Jh";
                    }
                    else
                    {

                        httpRequest.Headers["Authorization"] = "Basic bWVyY2hhbnQuVEVTVEFHUklDVUxUVVJFOjU1ZWMyMmNjOTMyNTA3NzA3MGJiMTVkYzc3NWEwNTAz";
                    }



                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    ResponseDTO.Root myDeserializedClass = JsonConvert.DeserializeObject<ResponseDTO.Root>(result);
                    //  var x = JsonConvert.DeserializeObject(result);//<IEnumerable<object>>(result);
                    // SerializeObject(result);
                    Code = myDeserializedClass.transaction[myDeserializedClass.transaction.Count - 1].response.acquirerCode;
                }




                ///////////////////////////////

                return Code;
            }
            catch (WebException ex)
            {

                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();

                String ErrorMessage = "";



                ErrorMessage += "Source :" + ex.Source;
                ErrorMessage += "Data :" + ex.Data.ToString();

                ErrorMessage += "GetBaseException :" + ex.GetBaseException().ToString();
                ErrorMessage += "HelpLink :" + ex.HelpLink;
                ErrorMessage += "HResult :" + ex.HResult.ToString();
                ErrorMessage += "Message :" + ex.Message;
                ErrorMessage += "LineNumber :" + line.ToString();
                ErrorMessage += "TargetSite :" + ex.TargetSite.ToString();
                ErrorMessage += "InnerException :" + ex.InnerException;
                using (WebResponse webResponse = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)webResponse;
                    if (httpResponse.StatusCode == HttpStatusCode.BadRequest)
                    {
                        return "NF";
                    }
                    //using (Stream data = webResponse.GetResponseStream())
                    //using (var reader = new StreamReader(data))
                    //{
                    //    string x = reader.ReadToEnd();

                    //}
                }

                return ErrorMessage;


            }



        }
    }
}
