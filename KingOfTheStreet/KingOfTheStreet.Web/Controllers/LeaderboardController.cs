using KingOfTheStreet.Services.Interfaces;
using KingOfTheStreet.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace KingOfTheStreet.Web.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly IStatisticsService _stats;
        public LeaderboardController(IStatisticsService stats) => _stats = stats;

        public async Task<IActionResult> Index()
        {
            var model = new LeaderboardViewModel
            {
                TopScorers = (await _stats.GetTopScorersAsync(10)).ToList(),
                TopRebounders = (await _stats.GetTopRebloundersAsync(10)).ToList(),
                TopAssists = (await _stats.GetTopAssistLeadersAsync(10)).ToList(),
                BestDefenders = (await _stats.GetBestDefendersAsync(10)).ToList()
            };
            return View(model);
        }
    }
}
