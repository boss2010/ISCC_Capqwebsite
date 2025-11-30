using Capqwebsite.Controllers;
using EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Capqwebsite.Controllers
{
    
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }
        //rehabLogin2
        [AllowAnonymous]
        [Route("/Login/Index")]
        
        public IActionResult Index()
        {
            return View();
        }

        //HttpContext.Session.SetString("UserSession", "Authenticated");
        [HttpPost]
            public IActionResult GoDataEntryMenu( string userName , string password)
        {
            try
            {
               
                if (userName == "admin" && password == "admin@123")
                {
                    ////////
                    CookieOptions option = new CookieOptions();

                    //Response.Cookies.Append("UserId", "1", option);
                    HttpContext.Session.SetString("UserSession", "Authenticated");
                    //string id = Request.Cookies.FirstOrDefault(c => c.Key == "UserId").Value;
                    return View();
                }
                else
                {

                    _logger.LogInformation("userName && password غير صحيح",
       DateTime.Now.ToLongTimeString());
                    ///////////////////////errorrloooooooooog////////////////////////////////////
                    var log = new A__plant_Error_Save
                    {
                        PageName= "LoginController",
                        ErrorMessage = "بيانات الدخول خاطئة",
                        FunctionName = "GoDataEntryMenu",
                        Date = DateTime.Now, // "02:30:45 PM"
                        User_Ip = "rehabSaveErrorr"
                    };
                    AgricultureDBContext dBContext = new AgricultureDBContext();
                    dBContext.Add(log);
                    dBContext.SaveChanges();
                    ///////////////////////////////////////////////////////////

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex) {
                _logger.LogInformation(ex.Message,"About page visited at {DT}",
             DateTime.Now.ToLongTimeString());
                Console.WriteLine("ERrrrrrrrrrrrrr____Rehab"+ex.Message);
                var log = new A__plant_Error_Save
                {
                      Id = long.Parse(GetSequencing("A__plant_Error_Save_SEQ", "long")),
                     
                    PageName = "LoginController",
                    ErrorMessage = ex.Message,
                    FunctionName = "GoDataEntryMenu",
                    Date = DateTime.Now, // "02:30:45 PM"
                    User_Ip="rehabSaveErrorr",
                   IsWeb=true
                };
                AgricultureDBContext dBContext = new AgricultureDBContext();
                dBContext.Add(log);
                dBContext.SaveChanges();
                return RedirectToAction("Index");

            }
            }
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
    }
}
