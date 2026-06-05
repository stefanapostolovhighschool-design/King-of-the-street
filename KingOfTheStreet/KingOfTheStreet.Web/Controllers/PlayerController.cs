using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KingOfTheStreet.Web.Controllers
{
    public class PlayerController : Controller
    {
        private readonly IPlayerService _service;
        private readonly ITeamService _teams;

        public PlayerController(IPlayerService service, ITeamService teams)
        {
            _service = service;
            _teams = teams;
        }

        public async Task<IActionResult> Index(string search)
        {
            ViewData["Search"] = search;
            var players = string.IsNullOrWhiteSpace(search)
                ? await _service.GetAllAsync()
                : await _service.SearchAsync(search);
            return View(players);
        }

        public async Task<IActionResult> Details(int id)
        {
            var player = await _service.GetByIdAsync(id);
            if (player == null) return NotFound();
            return View(player);
        }

        // JSON endpoint for AJAX live search.
        [HttpGet]
        public async Task<IActionResult> SearchJson(string term)
        {
            var players = await _service.SearchAsync(term);
            return Json(players.Select(p => new
            {
                id = p.Id,
                name = p.FullName,
                position = p.Position,
                team = p.Team?.Name ?? "Free Agent",
                rating = System.Math.Round(p.OverallRating, 1)
            }));
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            await PopulateTeamsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Player player)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTeamsAsync();
                return View(player);
            }
            await _service.CreateAsync(player);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var player = await _service.GetByIdAsync(id);
            if (player == null) return NotFound();
            await PopulateTeamsAsync();
            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(Player player)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTeamsAsync();
                return View(player);
            }
            await _service.UpdateAsync(player);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var player = await _service.GetByIdAsync(id);
            if (player == null) return NotFound();
            return View(player);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateTeamsAsync()
        {
            var teams = await _teams.GetAllAsync();
            ViewBag.Teams = new SelectList(teams, "Id", "Name");
        }
    }
}
