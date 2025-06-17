using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Capqwebsite.Controllers
{
    public class LoginController : Controller
    {
        //rehabLogin
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
            if (userName == "admin" && password == "admin@123")
            {
                ////////
                CookieOptions option = new CookieOptions();

                //Response.Cookies.Append("UserId", "1", option);
                HttpContext.Session.SetString("UserSession", "Authenticated");
                //string id = Request.Cookies.FirstOrDefault(c => c.Key == "UserId").Value;
                return View();
            }
            else {
                return RedirectToAction("Index");
            }
        }
    }
}
