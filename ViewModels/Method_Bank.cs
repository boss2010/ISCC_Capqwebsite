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
    public class Method_Bank
    {
        public static Out_SessionDTO Create_Session(decimal amount, string order, string CancelURL, string SuccessUrl, int? PortBank, string hostName)
        {

            try
            {


                Out_SessionDTO out_SessionDTO = new Out_SessionDTO();
                String Order_No = order; //"2" +Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 9)+ Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 3);
                CancelURL = CancelURL + Order_No;
                var url = "";
                if (PortBank == 80 || PortBank == 443 || hostName == "www.site.capq.gov.eg")
                {
                    url = "https://nbe.gateway.mastercard.com/api/rest/version/61/merchant/AGRICULTURE/session";

                }
                else
                {
                    url = "https://test-nbe.gateway.mastercard.com/api/rest/version/61/merchant/TESTAGRICULTURE/session";

                }


                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";

                httpRequest.Accept = "application/json";
                if (PortBank == 80 || PortBank == 443 || hostName == "www.site.capq.gov.eg")
                {
                    httpRequest.Headers["Authorization"] = "Basic TWVyY2hhbnQuQUdSSUNVTFRVUkU6ZWVkZTUzOGY2N2RlZDE5OTBkYmYwMTllYzM3Mzk0ODk=";

                }
                else
                {

                    httpRequest.Headers["Authorization"] = "Basic bWVyY2hhbnQuVEVTVEFHUklDVUxUVVJFOjU1ZWMyMmNjOTMyNTA3NzA3MGJiMTVkYzc3NWEwNTAz";
                }


                httpRequest.ContentType = "application/json";
                var data = @"{'apiOperation': 'CREATE_CHECKOUT_SESSION', 'interaction': { 'operation': 'PURCHASE', 'returnUrl': '" + SuccessUrl + "&&Order_No=" + Order_No + "',  'cancelUrl': '" + CancelURL + "' },'order': {  'currency': 'EGP', 'id': '" + Order_No + "',  'amount': '" + amount + "'  } }";


                data = data.Replace("'", "\"");
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var responseData = JsonConvert.DeserializeObject<SessionDTO>(result);
                    var Session_value = responseData.session.id;

                    out_SessionDTO.Session_Id = Session_value;


                }
                out_SessionDTO.Order_No = Order_No;
                out_SessionDTO.amount = amount;

                return out_SessionDTO;
            }
            catch (Exception ex)
            {
                Out_SessionDTO out_SessionDTO2 = new Out_SessionDTO();

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
                out_SessionDTO2.ErrorMess = ErrorMessage;
                return out_SessionDTO2;
            }



        }

        public static Out_SessionDTO Create_SessionFor_Inspection(decimal amount, string order, string CancelURL, string SuccessUrl, int? PortBank, string hostName)
        {

            try
            {

                Out_SessionDTO out_SessionDTO = new Out_SessionDTO();
                String Order_No = order; 

                CancelURL = CancelURL + Order_No;
                var url = "";
                var httpRequest = (HttpWebRequest)WebRequest.Create("https://test-nbe.gateway.mastercard.com/api/rest/version/61/merchant/TESTAGRICULTURE/session");


               

                ///////////////////////////////////////////
              
                    if (PortBank == 80 || PortBank == 443 || hostName == "www.site.capq.gov.eg")
                    {
                        url = "https://nbe.gateway.mastercard.com/api/rest/version/61/merchant/AGRICULTURE2/session";

                    }
                    else
                    {
                        url = "https://test-nbe.gateway.mastercard.com/api/rest/version/61/merchant/TESTAGRICULTURE/session";

                    }

                    httpRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpRequest.Method = "POST";



                    httpRequest.Accept = "application/json";
                    if (PortBank == 80 || PortBank == 443 || hostName == "www.site.capq.gov.eg")
                    {
                        httpRequest.Headers["Authorization"] = "Basic bWVyY2hhbnQuQUdSSUNVTFRVUkUyOjRiMDQwZTVkOTIwZDA5MDc5ZjdkMTllZWQxZmRmM2Jh";
                    }
                    else
                    {

                        httpRequest.Headers["Authorization"] = "Basic bWVyY2hhbnQuVEVTVEFHUklDVUxUVVJFOjU1ZWMyMmNjOTMyNTA3NzA3MGJiMTVkYzc3NWEwNTAz";
                    }
              

                httpRequest.ContentType = "application/json";
                var data = @"{'apiOperation': 'CREATE_CHECKOUT_SESSION', 'interaction': { 'operation': 'PURCHASE', 'returnUrl': '" + SuccessUrl + "&&Order_No=" + Order_No + "',  'cancelUrl': '" + CancelURL + "' },'order': {  'currency': 'EGP', 'id': '" + Order_No + "',  'amount': '" + amount + "'  } }";


                data = data.Replace("'", "\"");
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var responseData = JsonConvert.DeserializeObject<SessionDTO>(result);
                    var Session_value = responseData.session.id;

                    out_SessionDTO.Session_Id = Session_value;


                }
                out_SessionDTO.Order_No = Order_No;
                out_SessionDTO.amount = amount;

                return out_SessionDTO;
            }
            catch (Exception ex)
            {
                Out_SessionDTO out_SessionDTO2 = new Out_SessionDTO();

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
                out_SessionDTO2.ErrorMess = ErrorMessage;
                return out_SessionDTO2;
            }



        }

        public static String DomainName(int? port)
        {

            if (port == 80 || port == 443)
            {
                return "https://www.site.capq.gov.eg/";
            }
            else if (port == 8071)
            {
                return "http://41.33.237.90:8071/";

            }
            else if (port == 5205)
            {
                return "http://localhost:5205/";
            }
            else if (port == 8054)
            {
                return "http://10.7.7.242:8054/";
            }

            return "";




        }
    }
}
