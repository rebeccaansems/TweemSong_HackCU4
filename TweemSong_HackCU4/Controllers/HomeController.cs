using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        [HttpPost]
        public IActionResult Results(ThemeModel form)
        {
            string username = form.TwitterHandle.Replace("@", string.Empty);
            ThemeSong theme = new ThemeSong(username);
            Track track = theme.GetThemeSong();

            if (track != null)
            {
                ResultsModel results = new ResultsModel()
                {
                    Username = username,
                    Track = track,
                    ListenUrl = "https://open.spotify.com/embed?uri=spotify:album:" + track.album.id + "&theme=white&view=coverart",
                    Sentiment = theme.GetSentiment()
                };
                return View(results);
            }
            else
            {
                ResultsModel results = new ResultsModel()
                {
                    Username = username
                };
                return View("Error", results);
            }
        }
    }
}
