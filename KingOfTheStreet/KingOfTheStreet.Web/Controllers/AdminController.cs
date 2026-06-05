using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KingOfTheStreet.Web.Controllers
{
    [Authorize(Roles = DbSeeder.AdminRole)]
    public class AdminController : Controller
    {
        private readonly IStatisticsService _stats;
        private readonly ITeamService _teams;
        private readonly IBracketService _bracket;
        private readonly IAdminService _admin;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            IStatisticsService stats,
            ITeamService teams,
            IBracketService bracket,
            IAdminService admin,
            UserManager<ApplicationUser> userManager)
        {
            _stats = stats;
            _teams = teams;
            _bracket = bracket;
            _admin = admin;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Stats"] = await _stats.GetDashboardStatsAsync();
            ViewData["PendingTeams"] = (await _teams.GetAllAsync())
                .Where(t => !t.IsApproved).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTeam(int id)
        {
            await _admin.ApproveTeamAsync(id);
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateBracket(int tournamentId)
        {
            await _bracket.GenerateBracketAsync(tournamentId);
            return RedirectToAction("Schedule", "Match", new { tournamentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimulateSeason()
        {
            await _admin.SimulateSeasonAsync();
            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult ManageUsers()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
    }
}
