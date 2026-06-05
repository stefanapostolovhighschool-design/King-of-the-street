using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KingOfTheStreet.Web.Controllers
{
    public class TeamController : Controller
    {
        private readonly ITeamService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamController(ITeamService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string search)
        {
            ViewData["Search"] = search;
            var teams = string.IsNullOrWhiteSpace(search)
                ? await _service.GetAllAsync()
                : await _service.SearchAsync(search);
            return View(teams);
        }

        public async Task<IActionResult> Details(int id)
        {
            var team = await _service.GetDetailsAsync(id);
            if (team == null) return NotFound();
            ViewData["TeamRating"] = _service.CalculateTeamRating(team);
            return View(team);
        }

        // JSON endpoint for AJAX live search.
        [HttpGet]
        public async Task<IActionResult> SearchJson(string term)
        {
            var teams = await _service.SearchAsync(term);
            return Json(teams.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                players = t.Players?.Count ?? 0,
                rating = _service.CalculateTeamRating(t)
            }));
        }

        [Authorize]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Team team)
        {
            if (!ModelState.IsValid) return View(team);
            team.CaptainId = _userManager.GetUserId(User);
            await _service.CreateAsync(team);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _service.GetByIdAsync(id);
            if (team == null) return NotFound();
            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(Team team)
        {
            if (!ModelState.IsValid) return View(team);
            await _service.UpdateAsync(team);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _service.GetByIdAsync(id);
            if (team == null) return NotFound();
            return View(team);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
