using KingOfTheStreet.Data;
using KingOfTheStreet.Data.Models;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KingOfTheStreet.Web.Controllers
{
    public class TournamentController : Controller
    {
        private readonly ITournamentService _service;
        public TournamentController(ITournamentService service) => _service = service;

        // Guests can view.
        public async Task<IActionResult> Index()
            => View(await _service.GetAllAsync());

        public async Task<IActionResult> Details(int id)
        {
            var tournament = await _service.GetDetailsAsync(id);
            if (tournament == null) return NotFound();
            return View(tournament);
        }

        [Authorize(Roles = DbSeeder.AdminRole)]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Create(Tournament tournament)
        {
            if (!ModelState.IsValid) return View(tournament);
            await _service.CreateAsync(tournament);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Edit(int id)
        {
            var tournament = await _service.GetByIdAsync(id);
            if (tournament == null) return NotFound();
            return View(tournament);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Edit(Tournament tournament)
        {
            if (!ModelState.IsValid) return View(tournament);
            await _service.UpdateAsync(tournament);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var tournament = await _service.GetByIdAsync(id);
            if (tournament == null) return NotFound();
            return View(tournament);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Register(int tournamentId, int teamId)
        {
            await _service.RegisterTeamAsync(tournamentId, teamId);
            return RedirectToAction(nameof(Details), new { id = tournamentId });
        }
    }
}
