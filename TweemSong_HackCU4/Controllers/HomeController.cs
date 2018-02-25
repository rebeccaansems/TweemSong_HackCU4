using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TweemSong_HackCU4.Models;

namespace TweemSong_HackCU4.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }   

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/Theme/{username}")]
        public ActionResult Theme(string username)
        {
            ThemeSong theme = new ThemeSong(username);
            return Content(theme.GenerateThemeSong().artists.ToString());
        }

    }
}
