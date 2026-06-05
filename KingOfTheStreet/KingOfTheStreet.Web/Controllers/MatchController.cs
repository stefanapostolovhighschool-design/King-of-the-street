using KingOfTheStreet.Data;
using KingOfTheStreet.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KingOfTheStreet.Web.Controllers
{
    public class MatchController : Controller
    {
        private readonly IMatchService _service;
        public MatchController(IMatchService service) => _service = service;

        public async Task<IActionResult> Schedule(int tournamentId)
        {
            ViewData["TournamentId"] = tournamentId;
            return View(await _service.GetScheduleAsync(tournamentId));
        }

        public async Task<IActionResult> Details(int id)
        {
            var match = await _service.GetByIdAsync(id);
            if (match == null) return NotFound();
            return View(match);
        }

        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> EnterResult(int id)
        {
            var match = await _service.GetByIdAsync(id);
            if (match == null) return NotFound();
            return View(match);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> EnterResult(int id, int scoreA, int scoreB)
        {
            await _service.EnterResultAsync(id, scoreA, scoreB);
            var match = await _service.GetByIdAsync(id);
            return RedirectToAction(nameof(Schedule), new { tournamentId = match.TournamentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = DbSeeder.AdminRole)]
        public async Task<IActionResult> Simulate(int id)
        {
            await _service.SimulateMatchAsync(id);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
