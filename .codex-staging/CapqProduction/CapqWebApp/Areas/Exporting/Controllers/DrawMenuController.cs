using CapqApI;
using CapqWebDTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace CapqWebApp.Areas.Exporting.Controllers
{
    public class DrawMenuController : Controller
    {
        // GET: Exporting/DrawMenu
        public ActionResult Index()
        {
            return View();
        }



        public String DrawMenu()
        {
            try
            {
                var Area = (Request.UrlReferrer.Segments.Skip(1).Take(1).SingleOrDefault() ?? "Main").Trim('/');

              
                var Controller = (Request.UrlReferrer.Segments.Skip(2).Take(1).SingleOrDefault() ?? "Home").Trim('/');
                // Index is default action 

                var Action  = (Request.UrlReferrer.Segments.Skip(3).Take(1).SingleOrDefault() ?? "Index").Trim('/');
                


                Boolean DarwConstrainat = false;
                String Draw = "";
                int UserMenu = 0;
                if (Session["UserTypeMenu"] != null)
                {
                    UserMenu = int.Parse(Session["UserTypeMenu"].ToString());
                }

                if (Session["SendEmails"] != null)
                {
                    Draw += "  <li><a href='/Admin/PR_User_SendEmails/Index'> موظيفين لم  يتم أرسال كلمة المرور لهم </a></li>";
                    Draw += "  <li><a href='/Admin/PR_User_SendEmails/SelectAllTrue'> موظيفين تم أرسال كلمة المرور لهم</a></li>";
                    Draw += "  <li><a href='/Admin/PR_User_SendEmails/SelectAllFalse'> موظيفين بريدهم الألكتروني غير صحيح</a></li>";
                    Draw += "  <li><a href='/Exporting/ClientRefusedData/Index'> بيانات العملاء التى تم رفضها</a></li>";
                   

                }
                if (Session["AdminUserLogin"] != null)
                {
                    Draw += "  <li><a href='/Admin/AdminSystem/Index'> بيانات المستخدمين</a></li>";
                    Draw += "  <li><a href='/Admin/FeesDetails/Index'> بيانات الدفع الألكتروني</a></li>";
                    Draw += "  <li><a href='/Admin/FeesDetailsByOrderNumber/Index'> طلبات الدفع</a></li>";
                  
                    // Draw += "  <li><a href='/Admin/WebUser/Index'> بيانات المستخدمين</a></li>";
                }
                //if (Session["UserTypeMenu"] == null)

                //{

                if (UserMenu == 1)
                {
                    Draw += "  <li><a href='/Exporting/CompanyProfile/Index'> بيانات  الشـــركة</a></li>";

                    int port = Request.Url.Port;

                    //Draw += "  <li><a href='/Exporting/Farm/Index'>طلب تكويد مزرعة</a></li>";
                    //Draw += "  <li><a href='/Exporting/FarmInformation/Index'>بيانات المزارع السابقة</a></li>";
                    if (port == 49886 || port == 8071 || port == 8182 || port == 8054)
                    {


                       
                        Draw += "  <li><a href='/Exporting/AddCompanyActivities/Index'> انـشـطة الشـــركة</a></li>";
                    }

                    Draw += "  <li><a href='/NewExporting/Quick_Ex_CheckRequest/Index'>تسجيل طلب فحص صادر زراعة   17  </a></li>";
                    Draw += "  <li><a href='/NewExporting/GetAllExCheckRequestSpeed/Index'>  طلبات الفحص ( صادر زراعه 17 ) </a></li>";
                    Draw += "  <li><a href='/Exporting/Export_Constraint/Index'>  اشترطات التصدير</a></li>";
                    Draw += "  <li><a href='/NewExporting/Ex_CheckRequest/Index'>  تسجيل طلب الفحص صادر</a></li>";
                    Draw += "  <li><a href='/NewExporting/SelectAllCheckRequest/Index'>  طلبات الفحص ( صادر ) </a></li>";
                  
                    Draw += "  <li><a href='/NewExporting/SelectAll_PlantCeritificate/Index'> طلبات  الشهادات النباتية </a></li>";
                    if (port == 49886 || port == 8054 || port == 8071)
                    {
                        
                       
                        Draw += "  <li><a href='/Exporting/ExCheckReqForPalletsDistribution/Index'>طلب تقسيم طلب فحص مواد تعبئة خشبية</a></li>";
                      
                        Draw += "  <li><a href='/NewExporting/Ex_CheckRequest_Bales/Index'>  تسجيل طلب فحص تعبئة خشبية</a></li>";
                        Draw += "  <li><a href='/NewExporting/SelectAllPaletsExCheckRequest/Index'>    طلبات الفحص ( مواد تعبئة خشبية ) </a></li>";

                        
                    }


                    if (port == 49886 || port == 8071 || port == 8182 || port == 8054 || port == 80 || port == 443)
                    {
                        Draw += "  <li><a href='/Exporting/Farm/Index'>طلب تكويد مزرعة</a></li>";
                        Draw += "  <li><a href='/Exporting/FarmInformation/Index'>بيانات المزارع السابقة</a></li>";
                        Draw += "  <li><a href='/Exporting/FarmDataPalntFactoring/Index'>طلب تقسيم المزرعة</a></li>";
                        Draw += "  <li><a href='/Exporting/Station/Index'>طلب اعتماد منشأة حجرية </a></li>";
                        Draw += "  <li><a href='/Exporting/StationInformation/Index'> بيانات المنشأت الحجرية </a></li>";
                        //Draw += "  <li><a href='/Exporting/StationApproval/Index'>بيانات الأعتمادات الخاصة بكل محطة</a></li>";
                        Draw += "  <li><a href='/Exporting/StationRequirements/Index'>أشتراطات المنشأة الحجرية</a></li>";
                        Draw += "  <li><a href='/NewExporting/AddAccreditionStationsForComapny/Index'>طلب العمل في منشأة معتمدة</a></li>";
                        Draw += "  <li><a href='/NewExporting/GetAllStationAccredationforComapny/Index'>المنشأت المصرح العمل بها</a></li>";
                       
                    }


                    //Draw += "  <li><a href='/Exporting/FarmAccerdaition/Index'>طلب أعتماد مزراعة</a></li>";
                }
                if (UserMenu == 2)
                {
                    int port = Request.Url.Port;
                    Draw += "  <li><a href='/Exporting/OrganizationProfile/Index'> بيانات  الهيئة</a></li>";
                    //Draw += "  <li><a href='/Exporting/Farm/Index'>طلب تكويد مزرعة</a></li>";
                    //Draw += "  <li><a href='/Exporting/FarmInformation/Index'>بيانات المزارع السابقة</a></li>";
                    Draw += "  <li><a href='/Exporting/Export_Constraint/Index'>  اشترطات التصدير</a></li>";
                    Draw += "  <li><a href='/NewExporting/Ex_CheckRequest/Index'>   17 تسجيل طلب فحص صادر زراعة </a></li>";
                    Draw += "  <li><a href='/NewExporting/GetAllExCheckRequestSpeed/Index'>  طلبات الفحص ( صادر زراعه 17 ) </a></li>";
                    if (port == 49886 || port == 8054 || port == 8071 || port == 8182)
                    {
                      
                        Draw += "  <li><a href='/NewExporting/Ex_CheckRequest/Index'>  تسجيل طلب الفحص صادر</a></li>";
                        Draw += "  <li><a href='/NewExporting/SelectAllCheckRequest/Index'>  طلبات الفحص ( صادر ) </a></li>";
                    
                        Draw += "  <li><a href='/Exporting/ExCheckReqForPalletsDistribution/Index'>طلب تقسيم طلب فحص مواد تعبئة خشبية</a></li>";
                        Draw += "  <li><a href='/NewExporting/Ex_CheckRequest_Bales/Index'>  تسجيل طلب فحص تعبئة خشبية</a></li>";
                        Draw += "  <li><a href='/NewExporting/SelectAllPaletsExCheckRequest/Index'>    طلبات الفحص ( مواد تعبئة خشبية ) </a></li>";

                        Draw += "  <li><a href='/Exporting/Station/Index'>طلب اعتماد منشأة حجرية</a></li>";
                        Draw += "  <li><a href='/Exporting/StationInformation/Index'> بيانات المنشأت الحجرية </a></li>";
                        //Draw += "  <li><a href='/Exporting/StationApproval/Index'>بيانات الأعتمادات الخاصة بكل محطة</a></li>";
                        Draw += "  <li><a href='/NewExporting/AddAccreditionStationsForComapny/Index'>طلب العمل في منشأة معتمدة</a></li>";
                        Draw += "  <li><a href='/NewExporting/GetAllStationAccredationforComapny/Index'>المنشأت المصرح العمل بها</a></li>";
                        
                    }


                    if ( port == 80 || port == 443||port == 49886 || port == 8071 || port == 8182 || port == 8054)
                    {
                        Draw += "  <li><a href='/Exporting/Farm/Index'>طلب تكويد مزرعة</a></li>";
                        Draw += "  <li><a href='/Exporting/FarmInformation/Index'>بيانات المزارع السابقة</a></li>";
                        Draw += "  <li><a href='/Exporting/FarmDataPalntFactoring/Index'>طلب تقسيم المزرعة</a></li>";
                        //Draw += "  <li><a href='/Exporting/Station/Index'>طلب تكويد محطة</a></li>";
                        //Draw += "  <li><a href='/Exporting/StationInformation/Index'>بيانات المحطات السابقة</a></li>";

                        Draw += "  <li><a href='/Exporting/Station/Index'>طلب اعتماد منشأة حجرية</a></li>";
                        Draw += "  <li><a href='/Exporting/StationInformation/Index'> بيانات المنشأت الحجرية </a></li>";
                        //Draw += "  <li><a href='/Exporting/StationApproval/Index'>بيانات الأعتمادات الخاصة بكل محطة</a></li>";
                        Draw += "  <li><a href='/Exporting/StationRequirements/Index'>أشتراطات المنشأة الحجرية</a></li>";
                        Draw += "  <li><a href='/NewExporting/AddAccreditionStationsForComapny/Index'>طلب العمل في منشأة معتمدة</a></li>";
                        Draw += "  <li><a href='/NewExporting/GetAllStationAccredationforComapny/Index'>المنشأت المصرح العمل بها</a></li>";

                    }



                    //Draw += "  <li><a href='/Exporting/FarmAccerdaition/Index'>طلب أعتماد مزراعة</a></li>";
                }

                if (UserMenu == 5)
                {
                    Draw += "  <li><a href='/Exporting/PersonProfile/Index'> بيانات  الأساسية</a></li>";
                    Draw += "  <li><a href='/NewExporting/Ex_CheckRequest/Index'>   17 تسجيل طلب فحص صادر زراعة </a></li>";
                    Draw += "  <li><a href='/NewExporting/GetAllExCheckRequestSpeed/Index'>  طلبات الفحص ( صادر زراعه 17 ) </a></li>";
                    //Draw += "  <li><a href='/Exporting/Farm/Index'>طلب تكويد مزرعة</a></li>";
                    //Draw += "  <li><a href='/Exporting/FarmInformation/Index'>بيانات المزارع السابقة</a></li>";
                    //Draw += "  <li><a href='/Exporting/ExportRequest/Index'>طلب فحص</a></li>";
                    //Draw += "  <li><a href='/Exporting/ExportsData/Index'>طلبات الفحص الحالية - صادر</a></li>";
                    Draw += "  <li><a href='/Exporting/Export_Constraint/Index'>  اشترطات التصدير</a></li>";
                    Draw += "  <li><a href='/NewExporting/Ex_CheckRequest/Index'>  تسجيل طلب الفحص صادر</a></li>";
                    Draw += "  <li><a href='/NewExporting/SelectAllCheckRequest/Index'>  طلبات الفحص ( صادر ) </a></li>";
               
                    Draw += "  <li><a href='/NewExporting/SelectAll_PlantCeritificate/Index'> طلبات  الشهادات النباتية </a></li>";

                    int port = Request.Url.Port;
                    if (port == 49886 || port == 8071 || port == 8182 || port == 8054)
                    {
                        
                        Draw += "  <li><a href='/Exporting/ExCheckReqForPalletsDistribution/Index'>طلب تقسيم طلب فحص مواد تعبئة خشبية</a></li>";
                        Draw += "  <li><a href='/NewExporting/Ex_CheckRequest_Bales/Index'>  تسجيل طلب فحص تعبئة خشبية</a></li>";
                        Draw += "  <li><a href='/NewExporting/SelectAllPaletsExCheckRequest/Index'>    طلبات الفحص ( مواد تعبئة خشبية ) </a></li>";
                        //Draw += "  <li><a href='/Exporting/StationApproval/Index'>بيانات الأعتمادات الخاصة بكل محطة</a></li>";

                    }
                    if (port == 80 || port == 443 || port == 49886 || port == 8071 || port == 8182 || port == 8054)
                    {
                        Draw += "  <li><a href='/Exporting/Farm/Index'>طلب تكويد مزرعة</a></li>";
                        Draw += "  <li><a href='/Exporting/FarmInformation/Index'>بيانات المزارع السابقة</a></li>";
                        Draw += "  <li><a href='/Exporting/FarmDataPalntFactoring/Index'>طلب تقسيم المزرعة</a></li>";
                        Draw += "  <li><a href='/Exporting/Station/Index'>طلب اعتماد منشأة حجرية</a></li>";
                        Draw += "  <li><a href='/Exporting/StationInformation/Index'> بيانات المنشأت الحجرية </a></li>";
                        Draw += "  <li><a href='/Exporting/StationRequirements/Index'>أشتراطات المنشأة الحجرية</a></li>";
                        Draw += "  <li><a href='/NewExporting/AddAccreditionStationsForComapny/Index'>طلب العمل في منشأة معتمدة</a></li>";
                        Draw += "  <li><a href='/NewExporting/GetAllStationAccredationforComapny/Index'>المنشأت المصرح العمل بها</a></li>";
                       
                        //Draw += "  <li><a href='/Exporting/Station/Index'>طلب تكويد محطة</a></li>";
                        //Draw += "  <li><a href='/Exporting/StationInformation/Index'>بيانات المحطات السابقة</a></li>";

                    }



                    //Draw += "  <li><a href='/Exporting/FarmAccerdaition/Index'>طلب أعتماد مزراعة</a></li>";
                }


                if (UserMenu == 1)
                {
                    //Draw += "  <li><a href='/Exporting/CompanyAccerdition/Index'>طلب أعتماد شركة</a></li>";
                    //Draw += "  <li><a href='/Exporting/EditProfile/Index'>تغيير بيانات المستخدم</a></li>";
                    //Draw += "  <li><a href='/Exporting/Farm/Index'>تسجيل مزرعة</a></li>";
                }

                if (UserMenu == 4)
                {
                    //Draw += "  <li><a href='/Exporting/FarmAccerdaition/Index'>طلب أعتماد مزراعة</a></li>";
                    Draw += "  <li><a href='/Exporting/EditProfile/Index'>تغيير بيانات المستخدم</a></li>";
                }

                if (UserMenu == 3)
                {
                    //Draw += "  <li><a href='/Exporting/StationAccerdaition/Index'>طلب أعتماد محطة</a></li>";
                    //Draw += "  <li><a href='/Exporting/EditProfile/Index'>تغيير بيانات المستخدم</a></li>";
                    //Draw += "  <li><a href='/Exporting/StationDetails/Index'>تغيير البيانات الأساسية </a></li>";
                    //Draw += "  <li><a href='/Exporting/Farm/Index'>تسجيل مزرعة</a></li>";
                }
                if (UserMenu == 6)
                {
                    Draw += "  <li><a href='/Admin/File/Index'>شهادة الصحة النباتية </a></li>";
                    Draw += "  <li><a href='/Admin/Countrypassword/Index'>كلمة السر للدول</a></li>";
                    Draw += "  <li><a href='/Admin/CountryPasswordDisplay/Index'>عرض كلمة السر للدول</a></li>";
                    Draw += "  <li><a href='/Admin/Country/Index'>اضافة دول</a></li>";
                    Draw += "  <li><a href='/Admin/Users/Index'>المستخدمين</a></li>";
                    Draw += "  <li><a href='/Admin/UersDetails/Index'>بيانات الشهادات</a></li>";
                    Draw += "  <li><a href='/Exporting/EditProfile/Index'>تغيير بيانات المستخدم</a></li>";

                }
                if (UserMenu == 7)
                {
                    Draw += "  <li><a href='/Admin/Test/Index'>الخرائط</a></li>";
                    Draw += "  <li><a href='/Admin/Map/Index'>بيانات المزراع</a></li>";
                    //Draw += "  <li><a href='/Exporting/EditProfile/Index'>تغيير بيانات المستخدم</a></li>";
                    Session["UserMap"] = 1;

                }

                if (UserMenu == 1006)
                {
                    //Draw += "  <li><a href='/Admin/File/Index'>شهادة الصحة النباتية</a></li>";



                }
                if (UserMenu == 2020)
                {
                    int port = Request.Url.Port;
                    if (port == 8071 || port == 49886 || port == 8054)
                    {
                        //Draw += "  <li><a href='/Admin/PaymentOnline/Index'>دفع رسوم طلبات أعتماد المزرعة</a></li>";
                        Draw += "  <li><a href='/Admin/Farm_CommittePaymentOnline/Index'>دفع رسوم لجان أعتماد المزرعة</a></li>";
                        Draw += "  <li><a href='/Admin/PermissionRequestPayment/Index'>دفع رسوم اذن استيراد</a></li>";
                        Draw += "  <li><a href='/Admin/StationAccreditationPayment/Index'>دفع رسوم الأعتمادات الخاصة بكل منشأة حجرية</a></li>";

                        //Draw += "  <li><a href='/Admin/FarmRequestAllPaid/Index'>طباعة  رسوم طلبات أعتماد المزارع   المدفوعة مسبقا </a></li>";
                        Draw += "  <li><a href='/Admin/FarmCommitteAllPaid/Index'>طباعة رسوم لجان أعتماد المزارع المدفوعة مسبقا </a></li>";
                        Draw += "  <li><a href='/Admin/PermissionRequestAllPaid/Index'>طباعة رسوم اذن استيراد المدفوعة مسبقا</a></li>";
                        Draw += "  <li><a href='/Admin/GetAllTreatmentCommitteNotPaied/Index'>دفع لجان المعالجات لطلب الفحص الوارد</a></li>";
                        //Draw += "  <li><a href='/Admin/Im_PermissionRequest_EndDate/Index'>تقديم طلب تجديد أذون الأستيراد </a></li>";

                        //Draw += "  <li><a href='/Admin/PermissionRequestRenewPayment/Index'>دفع طلبات التجديد لأذون الأستيراد </a></li>";


                    }
                  //7-3-2022  if (port == 80 || port == 443)
                   // {
                    //    Draw += "  <li><a href='/Admin/PermissionRequestPayment/Index'>دفع رسوم اذن استيراد</a></li>";
                   //     Draw += "  <li><a href='/Admin/PermissionRequestAllPaid/Index'>طباعة رسوم اذن استيراد المدفوعة مسبقا</a></li>";
                  //  }



                }


                if (Session["IsSecurity"] != null && UserMenu == 30)
                {
                    int port = Request.Url.Port;

                    var SIP = APIConfiguration.GetApIUrl(port);
                    List<DrawMenuResult> list = new List<DrawMenuResult>();
                    HttpClient client = new HttpClient();

                    int iD = int.Parse(Session["IDSource"].ToString());
                    var result = client.GetAsync("" + SIP + "/api/DrawMenu?UserID=" + iD).Result;
                    list = result.Content.ReadAsAsync<List<DrawMenuResult>>().Result;

                    if(list.Count()>=0)
                    {
                        DarwConstrainat = true;
                        //Draw += "  <li><a href='/Exporting/Export_Constraint/Index'>اشترطات التصدير</a></li>";
                        //Draw += "  <li><a href='/Exporting/StationRequirements/Index'>أشتراطات المنشأة الحجرية</a></li>";
                        //Draw += "  <li><a href='/Importing/Import_Constraint/Index'> اشتراطات الأستيراد</a></li>";
                    }

                    foreach (var item in list)
                    {
                        if (item.PR_GroupId == 10)
                            Draw += "  <li><a href='/" + item.MenuURL + "'>" + item.MenuTitle + "</a></li>";
                        //Draw += "  <li><a href='/Exporting/OrganizationInformation/Index'>بيانات الهيئات</a></li>";
                        //Draw += "  <li><a href='/Exporting/PersonInformation/Index'>بيانات الأفراد</a></li>";

                    }




                }
                if (Session["IsSecurity"] != null && UserMenu == 12020)
                {
                    int port = Request.Url.Port;

                    var SIP = APIConfiguration.GetApIUrl(port);
                    List<DrawMenuResult> list = new List<DrawMenuResult>();
                    HttpClient client = new HttpClient();

                    int iD = int.Parse(Session["IDSource"].ToString());
                    var result = client.GetAsync("" + SIP + "/api/DrawMenu?UserID=" + iD).Result;
                    list = result.Content.ReadAsAsync<List<DrawMenuResult>>().Result;
                    if (list.Count() >= 0)
                    {
                        DarwConstrainat = true;
                        //Draw += "  <li><a href='/Exporting/Export_Constraint/Index'>اشترطات التصدير</a></li>";
                        //Draw += "  <li><a href='/Exporting/StationRequirements/Index'>أشتراطات المنشأة الحجرية</a></li>";
                        //Draw += "  <li><a href='/Importing/Import_Constraint/Index'> اشتراطات الأستيراد</a></li>";
                    }
                    if (Session["TestPayment"] != null || Session["TestVaildation"] !=null)
                    {
                        foreach (var item in list)
                        {
                            if (item.PR_GroupId == 10)
                                Draw += "  <li><a href='/" + item.MenuURL + "'>" + item.MenuTitle + "</a></li>";
                           

                        }
                      
                    }
                    
                        if (Session["TestPayment"] != null)
                    {
                       //7-3-2022 if (port == 80 || port == 443 || port == 8071)
                        //{
                        //    Draw += "  <li><a href='/Admin/PermissionRequestPayment/Index'>دفع رسوم اذن استيراد</a></li>";
                        //    Draw += "  <li><a href='/Admin/PermissionRequestAllPaid/Index'>طباعة رسوم اذن استيراد المدفوعة مسبقا</a></li>";
                        //    //  Draw += "  <li><a href='/Admin/Im_PermissionRequest_EndDate/Index'>تقديم طلب تجديد أذون الأستيراد </a></li>";
                        //    Draw += "  <li><a href='/Admin/PermissionRequestRenewPayment/Index'>دفع رسوم طلبات  تجديد أذون الأستيراد</a></li>";
                        //    Draw += "  <li><a href='/Admin/Im_CheckRequestPayment/Index'> دفع رسوم طلب الفحص الوراد</a></li>";
                        //    Draw += "  <li><a href='/Admin/Im_CheckRequestPaymentPrint/Index'>طباعة رسوم طلب الفحص الوراد </a></li>";
                        //}


                        if (port == 80 || port == 443 || port == 49886 || port == 8071 || port == 8182 || port == 8054)
                        {
                         
                            //27-2-2022 
                            // Draw += "  <li><a href='/Admin/Im_CheckRequestPayment/Index'> دفع رسوم طلب الفحص الوراد</a></li>";
                            // Draw += "  <li><a href='/Admin/Im_CheckRequestPaymentPrint/Index'>طباعة رسوم طلب الفحص الوراد </a></li>";
                            // Draw += "  <li><a href='/Admin/Ex_CheckRequestPayment/Index'>  دفع رسوم طلب الفحص الصادر</a></li>";
                            // Draw += "  <li><a href='/Admin/Ex_CheckRequestPaymentPrint/Index'>طباعة رسوم طلب الفحص الصادر</a></li>";
                            // Draw += "  <li><a href='/Admin/CertificationPlantExportRequest/Index'> دفع رسوم شهادات الصحة النباتية</a></li>";
                            // Draw += "  <li><a href='/Admin/CertificationPlantExportRequest/SelectAllToPrintIt'>طباعة رسوم شهادات الصحة النباتية </a></li>";


                            // Draw += "  <li><a href='/Admin/PermissionRequestPayment/Index'>دفع رسوم اذن استيراد</a></li>";

                            //// Draw += "  <li><a href='/Admin/Im_PermissionRequest_EndDate/Index'>تقديم طلب تجديد أذون الأستيراد </a></li>";
                            // Draw += "  <li><a href='/Admin/PermissionRequestRenewPayment/Index'>دفع رسوم طلبات تجديد أذون الأستيراد  </a></li>";
                        }



                      
                    }




                    if (Session["TestVaildation"] != null)
                    {

                        foreach (var item in list)
                        {
                            //if (item.PR_GroupId == 10)
                            //    Draw += "  <li><a href='/" + item.MenuURL + "'>" + item.MenuTitle + "</a></li>";
                            //Draw += "  <li><a href='/Exporting/OrganizationInformation/Index'>بيانات الهيئات</a></li>";
                            //Draw += "  <li><a href='/Exporting/PersonInformation/Index'>بيانات الأفراد</a></li>";

                        }
                    }


                    if (Session["FramPaymentTrue"] != null)
                    {
                        Draw += "  <li><a href='/Admin/PaymentOnline/Index'>دفع رسوم طلبات أعتماد المزرعة</a></li>";


                        Draw += "  <li><a href='/Admin/FarmRequestAllPaid/Index'>طباعة  رسوم طلبات أعتماد المزارع   المدفوعة مسبقا </a></li>";

                        Draw += "  <li><a href='/Admin/Farm_CommittePaymentOnline/Index'>دفع رسوم لجان أعتماد المزرعة</a></li>";
                        Draw += "  <li><a href='/Admin/FarmCommitteAllPaid/Index'>طباعة رسوم لجان أعتماد المزارع المدفوعة مسبقا </a></li>";


                    }


                }
                ///
                if (Session["FramPaymentTrue"] != null && UserMenu == 120120)
                {
                    Draw += "  <li><a href='/Admin/PaymentOnline/Index'>دفع رسوم طلبات أعتماد المزرعة</a></li>";


                    Draw += "  <li><a href='/Admin/FarmRequestAllPaid/Index'>طباعة  رسوم طلبات أعتماد المزارع   المدفوعة مسبقا </a></li>";

                    Draw += "  <li><a href='/Admin/Farm_CommittePaymentOnline/Index'>دفع رسوم لجان أعتماد المزرعة</a></li>";
                    Draw += "  <li><a href='/Admin/FarmCommitteAllPaid/Index'>طباعة رسوم لجان أعتماد المزارع المدفوعة مسبقا </a></li>";




                }


                if (DarwConstrainat == true)
                {

                    Draw += "  <li><a href='/Exporting/Export_Constraint/Index'>اشترطات التصدير</a></li>";
                    Draw += "  <li><a href='/Exporting/StationRequirements/Index'>أشتراطات المنشأة الحجرية</a></li>";
                    Draw += "  <li><a href='/Importing/Import_Constraint/Index'> اشتراطات الأستيراد</a></li>";
                }

                // }




                return Draw;
            }
            catch (Exception ex)
            {
                DataClasses1DataContext data = new DataClasses1DataContext();
                string StoredName = "DrawMenu/DrawMenu";
                string ErrorName = ex.Message;
                data.Error_saving_App(ErrorName, StoredName, DateTime.Now, 1);

                return null;
            }

        }




        public String DrawMenu2()
        {
            int port = Request.Url.Port;
            try
            {
                String Draw = "";



                //if (port == 49886 || port == 8071 || port == 8182)
                //{

                //}
              
                if (Session["UserTypeMenu"] != null)

                {
                    int UserMenu = int.Parse(Session["UserTypeMenu"].ToString());
                    if (UserMenu == 1 || UserMenu == 2)
                    {
                       
                        
                        //if (port == 80)
                        //{
                        Draw += "  <li><a href='/Importing/Import_Constraint/Index'> اشتراطات الأستيراد</a></li>";
                        Draw += "  <li><a href='/Importing/ImportMain/Index'>تسجيل طلب أذن الأستيراد</a></li>";
                        Draw += "  <li><a href='/Importing/PermissionRequestDetails/Index'> طلبات أذون الأستيراد</a></li>";
                        Draw += "  <li><a href='/Importing/ArrivalNoticetype/Index'>تسجيل إشعار وصول</a></li>";
                        Draw += "  <li><a href='/Importing/ArrivalNoticeDetails/Index'> طلبات إشعار وصول</a></li>";
                        if (port == 80 || port == 443)
                        {
                            Draw += "  <li><a href='/Importing/ImCheckRequestWithPermissionNumber/Index'>تسجيل طلب فحص وارد</a></li>";
                        }
                        else if (port == 49886 || port == 8071 || port == 8182 || port == 8054)
                        {
                            Draw += "  <li><a href='/Importing/Im_CheckRequestMain/Index'>تسجيل طلب فحص وارد</a></li>";
                        }
                      
                        Draw += "  <li><a href='/Importing/SelectAllCheckRequest/Index'>طلبات فحص الوارد</a></li>";
                        
                        if (port == 49886 || port == 8071 || port == 8182 || port == 8054)
                        {
                            Draw += "  <li><a href='/Importing/ImportType/Index?Req=" + 13 + "'>تسجيل ترانزيت</a></li>";
                            Draw += "  <li><a href='/Importing/Import_Info/Index?Info=" + 13 + "'> طلبات الترانزيت</a></li>";
                            Draw += "  <li><a href='/Importing/ImportType/Index?Req=" + 14 + "'>تسجيل إذن الاستيراد لأغراض البحث العلمي</a></li>";
                            Draw += "  <li><a href='/Importing/Import_Info/Index?Info=" + 14 + "'> طلبات إذون الاستيراد لأغراض البحث العلمي</a></li>";
                            Draw += "  <li><a href='/Importing/ImportType/Index?Req=" + 15 + "'>تسجيل تصاريح خاصة </a></li>";
                            Draw += "  <li><a href='/Importing/Import_Info/Index?Info=" + 15 + "'> طلبات التصاريح خاصة</a></li>";

                            Draw += "  <li><a href='/Importing/All_Im_CustodyPlaceRequests/Index'>طلبات فحص الوارد تحت التحفظ</a></li>";
                            Draw += "  <li><a href='/Importing/All_Im_CheckRequestsByOperationType/Index?OperationId=" + 13 + "'>طلبات فحص الوارد ترانزيت</a></li>";
                            Draw += "  <li><a href='/Importing/All_Im_CheckRequestsByOperationType/Index?OperationId=" + 14 + "'>طلبات فحص الوارد رسائل علمية</a></li>";
                            Draw += "  <li><a href='/Importing/All_Im_CheckRequestsByOperationType/Index?OperationId=" + 15 + "'>طلبات فحص الوارد تصاريح خاصة</a></li>";

                        }

                        Draw += "  <li><a href='/Exporting/EditProfile/Index'>تغيير بيانات المستخدم</a></li>";




                    }
                    if (UserMenu == 5)
                    {
                       
                        Draw += "  <li><a href='/Importing/Import_Constraint/Index'> اشتراطات الأستيراد</a></li>";
                        Draw += "  <li><a href='/Importing/ImportMain/Index'>تسجيل طلب أذن الأستيراد</a></li>";
                        Draw += "  <li><a href='/Importing/PermissionRequestDetails/Index'> طلبات أذون الأستيراد</a></li>";
                        Draw += "  <li><a href='/Importing/ArrivalNoticetype/Index'>تسجيل إشعار وصول</a></li>";
                        Draw += "  <li><a href='/Importing/ArrivalNoticeDetails/Index'> طلبات إشعار وصول</a></li>";
                        if (port == 80 || port == 443)
                        {
                            Draw += "  <li><a href='/Importing/ImCheckRequestWithPermissionNumber/Index'>تسجيل طلب فحص وارد</a></li>";
                        }
                        else if (port == 49886 || port == 8071 || port == 8182 || port == 8054)
                        {
                            Draw += "  <li><a href='/Importing/Im_CheckRequestMain/Index'>تسجيل طلب فحص وارد</a></li>";
                        }

                        Draw += "  <li><a href='/Importing/SelectAllCheckRequest/Index'>طلبات فحص الوارد</a></li>"; 
                        Draw += "  <li><a href='/Exporting/EditProfile/Index'>تغيير بيانات المستخدم</a></li>";
                        if (port == 49886 || port == 8071 || port == 8182 || port == 8054)
                        {
                          
                            //if (port == 80 || port == 443)
                            //{
                            //    Draw += "  <li><a href='/Importing/ImCheckRequestWithPermissionNumber/Index'>تسجيل طلب فحص وارد</a></li>";
                            //}
                            //else if (port == 49886 || port == 8071 || port == 8182 || port == 8054)
                            //{
                             //   Draw += "  <li><a href='/Importing/Im_CheckRequestMain/Index'>تسجيل طلب فحص وارد</a></li>";
                            //}

                          


                            Draw += "  <li><a href='/Importing/ImportType/Index?Req=" + 13 + "'>تسجيل ترانزيت</a></li>";
                            Draw += "  <li><a href='/Importing/Import_Info/Index?Info=" + 13 + "'> طلبات الترانزيت</a></li>";
                            Draw += "  <li><a href='/Importing/ImportType/Index?Req=" + 14 + "'>تسجيل رسائل علمية</a></li>";
                            Draw += "  <li><a href='/Importing/Import_Info/Index?Info=" + 14 + "'> طلبات الرسائل علمية</a></li>";
                            Draw += "  <li><a href='/Importing/ImportType/Index?Req=" + 15 + "'>تسجيل تصاريح خاصة </a></li>";
                            Draw += "  <li><a href='/Importing/Import_Info/Index?Info=" + 15 + "'> طلبات التصاريح خاصة</a></li>";
                            Draw += "  <li><a href='/Importing/All_Im_CustodyPlaceRequests/Index'>طلبات فحص الوارد تحت التحفظ</a></li>";
                            Draw += "  <li><a href='/Importing/All_Im_CheckRequestsByOperationType/Index?OperationId=" + 13 + "'>طلبات فحص الوارد ترانزيت</a></li>";
                            Draw += "  <li><a href='/Importing/All_Im_CheckRequestsByOperationType/Index?OperationId=" + 14 + "'>طلبات فحص الوارد رسائل علمية</a></li>";
                            Draw += "  <li><a href='/Importing/All_Im_CheckRequestsByOperationType/Index?OperationId=" + 15 + "'>طلبات فحص الوارد تصاريح خاصة</a></li>";

                        }
                    }
                    if (port== 8054 || port == 49886 || port == 8071 || port == 8182 && UserMenu == 1 || UserMenu == 2)
                    {

                        Draw += "  <li><a href='/Importing/ImCheckRequestDistribute/Index'>تقسيم طلب الفحص الوارد</a></li>";
                    }


                    }




                return Draw;
            }
            catch (Exception ex)
            {
                DataClasses1DataContext data = new DataClasses1DataContext();
                string StoredName = "DrawMenu/DrawMenu2";
                string ErrorName = ex.Message;
                data.Error_saving_App(ErrorName, StoredName, DateTime.Now, 1);

                return null;
            }


        }
    }
}
