using System.Collections.Generic;
using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Capqwebsite.Controllers
{
    public class DataEntryController : Controller
    {
        AgricultureDBContext DBContext = new AgricultureDBContext();
        private readonly IWebHostEnvironment _hostingEnvironment;
        public DataEntryController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]

        public IActionResult Index(int ID)
        {
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                if (ID != 0)
                {

                    try
                    {
                        ViewBag.TitleAr = "الاسم"; 
                        ViewBag.TitleEn = "";
                        ViewBag.descAr = "التفاصيل";
                        ViewBag.descEn = "التفاصيل";
                        ViewBag.filepath = "اختيار ملف";
                        ViewBag.linkURL = "الرابط";
                        ViewBag.Date = "التاريخ";

                        switch (ID)
                        {
                           
                            case 12 ://مكاتبنا
                                ViewBag.descEn = "الهاتف";
                                ViewBag.descAr = "العنوان";
                                ViewBag.TitleEn = "البريد";
                                break;
                            case 7://الاخبار
                                ViewBag.TitleAr = "العنوان";
                                break;
                            default:
                                break;
                        }

                        string TypeAr = DBContext.Websitetypes.Where(a => a.ID == ID).ToList().FirstOrDefault()?.TypeAr;
                        var list = DBContext.WebsiteTypeDetails.Where(a => a.WebsitetypeID == ID && (a.IsActive == true || a.IsActive == null)).ToList();
                        ViewBag.TypeAr = TypeAr;
                        ViewBag.ID = ID;
                        return View(list);
                    }
                    catch (Exception ex)
                    {
                        //////////////////////////////errorr looogggg/////////////////////////////////////////
                        var log = new A__plant_Error_Save
                        {
                            PageName = "DataEntryController",
                            ErrorMessage = ex.Message,
                            FunctionName = "Index",
                            Date = DateTime.Now, // "02:30:45 PM"
                            User_Ip = "rehabSaveErrorr",
                            IsWeb = true
                        };
                        DBContext.Add(log);
                        DBContext.SaveChanges();
                        ////////////////////////////////////////////////////
                        return Redirect("/Login/Index");
                    }
                    
                }
                else { return View(); }
            }
            else {
                //return View();
                return Redirect("/Login/Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Index(WebsiteTypeDetail form, IFormFile imageFile)
        {       // ✅ تحقق من الـ Model State
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Model State غير صحيح:");
            }
                try
            {
            

                int ID = int.Parse(GetSequencing("WebsiteTypeDetail_SEQ", "int"));
                form.ID = ID;

                // Handle file upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Get the original filename (e.g., "myphoto.jpg")
                    string originalFileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                    string fileExtension = Path.GetExtension(imageFile.FileName); // .jpg, .png, etc.

                    // Add a timestamp or GUID to avoid duplicates
                    //string uniqueFileName = $"{originalFileName}_{DateTime.Now.Ticks}{fileExtension}";
                    string uniqueFileName = $"{originalFileName}{fileExtension}";
                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "img");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    // Save to wwwroot/img/
                    string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "img", uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Store the relative path (e.g., "~/img/myphoto_123456789.jpg")
                    form.filepath = $"/img/{uniqueFileName}";


                }
                //////////////////pdf//////////////////
                //if (pdfFile != null && pdfFile.Length > 0)
                //{
                //    // Get original filename
                //    string originalFileName = Path.GetFileNameWithoutExtension(pdfFile.FileName);
                //    string fileExtension = Path.GetExtension(pdfFile.FileName); // .pdf

                //    // Keep the same name exactly (زي ما انت عامل في الصور)
                //    string uniqueFileName = $"{originalFileName}{fileExtension}";

                //    // Folder wwwroot/pdf
                //    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "pdf");

                //    if (!Directory.Exists(directoryPath))
                //    {
                //        Directory.CreateDirectory(directoryPath);
                //    }

                //    // Full path
                //    string filePath = Path.Combine(directoryPath, uniqueFileName);

                //    // Save file
                //    using (var fileStream = new FileStream(filePath, FileMode.Create))
                //    {
                //        await pdfFile.CopyToAsync(fileStream);
                //    }

                //    // Save relative path to DB (نفس النهج)
                //    form.filepathpdf = $"/pdf/{uniqueFileName}";
                //}
                /////////////////////////////////////
                form.User_Creation_Date = DateTime.Now;
                DBContext.WebsiteTypeDetails.Add(form);

                DBContext.SaveChanges();
                TempData["WarningMessage"] = "Saved successfully, but please review the data!";
                // Rest of your code...

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //////////////////////////////errorr looogggg/////////////////////////////////////////
                var log = new A__plant_Error_Save
                {
                    Id = long.Parse(GetSequencing("A__plant_Error_Save_SEQ", "long")),
                    PageName = "DataEntryController",
                    ErrorMessage = ex.Message,
                    FunctionName = "Insert",
                    Date = DateTime.Now, // "02:30:45 PM"
                    User_Ip = "rehabSaveErrorr",
                    IsWeb = true
                };
                DBContext.Add(log);
                DBContext.SaveChanges();
                ////////////////////////////////////////////////////
                return Redirect("/Login/Index");
            }
        }
        //[HttpPost]
        //public async Task<IActionResult> Index(WebsiteTypeDetail form, IFormFile imageFile)
        //{

        //    if (form.EditMode = true)
        //    {
        //        if (imageFile != null && imageFile.Length > 0)
        //        {
        //            // Get the original filename (e.g., "myphoto.jpg")
        //            string originalFileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
        //            string fileExtension = Path.GetExtension(imageFile.FileName); // .jpg, .png, etc.

        //            // Add a timestamp or GUID to avoid duplicates
        //            //string uniqueFileName = $"{originalFileName}_{DateTime.Now.Ticks}{fileExtension}";
        //            string uniqueFileName = $"{originalFileName}{fileExtension}";
        //            var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "img");
        //            if (!Directory.Exists(directoryPath))
        //            {
        //                Directory.CreateDirectory(directoryPath);
        //            }
        //            // Save to wwwroot/img/
        //            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "img", uniqueFileName);
        //            using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await imageFile.CopyToAsync(fileStream);
        //            }

        //            // Store the relative path (e.g., "~/img/myphoto_123456789.jpg")
        //            form.filepath = $"/img/{uniqueFileName}";


        //        }
        //        DBContext.WebsiteTypeDetails.Update(form);
        //        DBContext.SaveChanges();
        //    }
        //    else
        //    {
        //        int ID = int.Parse(GetSequencing("WebsiteTypeDetail_SEQ", "int"));
        //        form.ID = ID;

        //        // Handle file upload
        //        if (imageFile != null && imageFile.Length > 0)
        //        {
        //            // Get the original filename (e.g., "myphoto.jpg")
        //            string originalFileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
        //            string fileExtension = Path.GetExtension(imageFile.FileName); // .jpg, .png, etc.

        //            // Add a timestamp or GUID to avoid duplicates
        //            //string uniqueFileName = $"{originalFileName}_{DateTime.Now.Ticks}{fileExtension}";
        //            string uniqueFileName = $"{originalFileName}{fileExtension}";
        //            var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "img");
        //            if (!Directory.Exists(directoryPath))
        //            {
        //                Directory.CreateDirectory(directoryPath);
        //            }
        //            // Save to wwwroot/img/
        //            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "img", uniqueFileName);
        //            using (var fileStream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await imageFile.CopyToAsync(fileStream);
        //            }

        //            // Store the relative path (e.g., "~/img/myphoto_123456789.jpg")
        //            form.filepath = $"/img/{uniqueFileName}";


        //        }
        //        DBContext.WebsiteTypeDetails.Add(form);
        //        DBContext.SaveChanges();


        //        // Rest of your code...
        //    }
        //    return RedirectToAction("Index");
        //}

        public string GetSequencing(string seqName, string type)
        {
            var dqlType = type switch
            {
                "byte" => System.Data.SqlDbType.TinyInt,
                "short" => System.Data.SqlDbType.SmallInt,
                "long" => System.Data.SqlDbType.BigInt,
                _ => System.Data.SqlDbType.Int
            };

            var p = new SqlParameter("@result", dqlType);
            p.Direction = System.Data.ParameterDirection.Output;

            using var dbContext = new AgricultureDBContext();
            // execute the SQL query that sets the parameter to the next value for the sequence
            dbContext.Database.ExecuteSqlRaw("set @result = next value for dbo." + seqName, p);

            // get the parameter value from your code
            var nextVal = p.Value;

            return nextVal.ToString();
        }


        //////////////////////////////////////Edit//////////////////////////////////////////////////
        [HttpGet]
        public IActionResult Edit(int id,int IdType)
        {
            try
            {

          
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                if (id != null)
            {
                var itemToEdit = DBContext.WebsiteTypeDetails.FirstOrDefault(a => a.ID == id);
                if (itemToEdit != null)
                {
                    string TypeAr = DBContext.Websitetypes.Where(a => a.ID == itemToEdit.WebsitetypeID).FirstOrDefault()?.TypeAr;
                    ViewBag.TypeAr = TypeAr;
                    ViewBag.ID = itemToEdit.WebsitetypeID;
                    ViewBag.IdType = IdType;
                    ViewBag.EditMode = true;
                    return View(itemToEdit); // Pass single item for editing
                }
                return View();
            }
            return View();
            }
            else
            {
                //return View();
                return Redirect("/Login/Index");
            }
            }
            catch (Exception ex)
            {
                //////////////////////////////errorr looogggg/////////////////////////////////////////
                var log = new A__plant_Error_Save
                {
                    PageName = "LoginController",
                    ErrorMessage = ex.Message,
                    FunctionName = "EditGet",
                    Date = DateTime.Now, // "02:30:45 PM"
                    User_Ip = "rehabSaveErrorr",
                    IsWeb = true
                };
                DBContext.Add(log);
                DBContext.SaveChanges();
                ////////////////////////////////////////////////////
                return Redirect("/Login/Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(WebsiteTypeDetail form, IFormFile imageFile)
        {



            try
            {

            
                // Handle file upload
                if (imageFile != null && imageFile.Length > 0)
            {
                // Get the original filename (e.g., "myphoto.jpg")
                string originalFileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                string fileExtension = Path.GetExtension(imageFile.FileName); // .jpg, .png, etc.

                // Add a timestamp or GUID to avoid duplicates
                //string uniqueFileName = $"{originalFileName}_{DateTime.Now.Ticks}{fileExtension}";
                string uniqueFileName = $"{originalFileName}{fileExtension}";
                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "img");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                // Save to wwwroot/img/
                string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "img", uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Store the relative path (e.g., "~/img/myphoto_123456789.jpg")
                form.filepath = $"/img/{uniqueFileName}";

            }
                form.User_Updation_Date= DateTime.Now; 
                DBContext.WebsiteTypeDetails.Update(form);
            DBContext.SaveChanges();
            var returnedId = form.WebsitetypeID;
            return RedirectToAction("Index", new { ID = returnedId });
            }
            catch (Exception ex)
            {
                //////////////////////////////errorr looogggg/////////////////////////////////////////
                var log = new A__plant_Error_Save
                {
                    Id = long.Parse(GetSequencing("A__plant_Error_Save_SEQ", "long")),

                    PageName = "LoginController",
                    ErrorMessage = ex.Message,
                    FunctionName = "Update",
                    Date = DateTime.Now, // "02:30:45 PM"
                    User_Ip = "rehabSaveErrorr",
                    IsWeb = true
                };
                AgricultureDBContext dBContext = new AgricultureDBContext();
                dBContext.Add(log);
                dBContext.SaveChanges();
                return Redirect("/Login/Index");
            }
        }

        public IActionResult Delete(int id)
        {
            var deletdRow = DBContext.WebsiteTypeDetails.Where(a => a.ID == id && (a.IsActive == true || a.IsActive == null)).ToList().FirstOrDefault();
            var returnedId = deletdRow.WebsitetypeID;
            try
            {
                //int x = 0;
                //int y = 5 / x;
                deletdRow.IsActive = false;
                DBContext.SaveChanges();
                
                return RedirectToAction("Index", new { ID = returnedId });
            }
            catch (Exception ex)
            {
                var log = new A__plant_Error_Save
                {
                    Id = long.Parse(GetSequencing("A__plant_Error_Save_SEQ", "long")),

                    PageName = "LoginController",
                    ErrorMessage = ex.Message,
                    FunctionName = "delete",
                    Date = DateTime.Now, // "02:30:45 PM"
                    User_Ip = "rehabSaveErrorr",
                    IsWeb = true
                };
                AgricultureDBContext dBContext = new AgricultureDBContext();
                dBContext.Add(log);
                dBContext.SaveChanges();



                return RedirectToAction("Index", new { ID = returnedId });

            }

        }
    }

}
