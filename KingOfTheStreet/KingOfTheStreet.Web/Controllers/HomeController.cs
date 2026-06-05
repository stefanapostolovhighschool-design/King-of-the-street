using KingOfTheStreet.Services.Interfaces;
using KingOfTheStreet.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace KingOfTheStreet.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITournamentService _tournaments;
        private readonly ITeamService _teams;
        private readonly IStatisticsService _stats;

        public HomeController(ITournamentService tournaments, ITeamService teams, IStatisticsService stats)
        {
            _tournaments = tournaments;
            _teams = teams;
            _stats = stats;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                UpcomingTournaments = (await _tournaments.GetUpcomingAsync(3)).ToList(),
                TopTeams = (await _teams.GetTopTeamsAsync(4)).ToList(),
                TopScorers = (await _stats.GetTopScorersAsync(5)).ToList(),
                Stats = await _stats.GetDashboardStatsAsync()
            };
            return View(model);
        }

        public IActionResult About() => View();

        public IActionResult Contact() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // Custom pages for 400/401/403/404/500 via UseStatusCodePagesWithReExecute.
        public IActionResult HttpStatus(int code)
        {
            ViewData["Code"] = code;
            ViewData["Message"] = code switch
            {
                400 => "Bad Request — the server couldn't understand your request.",
                401 => "Unauthorized — please sign in to continue.",
                403 => "Forbidden — you don't have permission to access this resource.",
                404 => "Not Found — this page took a shot and missed.",
                500 => "Internal Server Error — something went wrong on our court.",
                _ => "An unexpected error occurred."
            };
            Response.StatusCode = code;
            return View("HttpStatus");
        }
    }
}
