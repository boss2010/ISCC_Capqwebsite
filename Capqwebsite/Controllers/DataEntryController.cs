using System.Collections.Generic;
using EF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
                        string TypeAr = DBContext.Websitetypes.Where(a => a.ID == ID).ToList().FirstOrDefault()?.TypeAr;
                        var list = DBContext.WebsiteTypeDetails.Where(a => a.WebsitetypeID == ID && (a.IsActive == true || a.IsActive == null)).ToList();
                        ViewBag.TypeAr = TypeAr;
                        ViewBag.ID = ID;
                        return View(list);
                    }
                    catch (Exception)
                    {

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
        {
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
                DBContext.WebsiteTypeDetails.Add(form);
                DBContext.SaveChanges();


                // Rest of your code...
            
            return RedirectToAction("Index");
            }
            catch (Exception)
            {

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
        public IActionResult Edit(int id)
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
            catch (Exception)
            {

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
            DBContext.WebsiteTypeDetails.Update(form);
            DBContext.SaveChanges();
            var returnedId = form.WebsitetypeID;
            return RedirectToAction("Index", new { ID = returnedId });
            }
            catch (Exception)
            {

                return Redirect("/Login/Index");
            }
        }

        public IActionResult Delete(int id)
        {

            var deletdRow = DBContext.WebsiteTypeDetails.Where(a => a.ID == id && (a.IsActive == true || a.IsActive == null)).ToList().FirstOrDefault();
            deletdRow.IsActive = false;
            DBContext.SaveChanges();
            var returnedId = deletdRow.WebsitetypeID;
            return RedirectToAction("Index", new { ID = returnedId });


        }
    }

}
