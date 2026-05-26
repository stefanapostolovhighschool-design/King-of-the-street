# King Of The Street 🏀

A 3v3 street-basketball tournament management system built with **ASP.NET Core MVC (.NET 8)**.
Manage tournaments, teams and players, generate brackets, and run a **possession-based match
simulation engine** that produces realistic box scores, MVPs and leaderboards.

This project was built as a final-defense project for the *Applied Programmer* qualification.

---

## ✨ Features

- **MVC architecture** with a clean separation into four projects (Web / Data / Services / Tests).
- **Entity Framework Core 8** (code-first) with SQL Server LocalDB and seeded demo data.
- **ASP.NET Core Identity** with roles (`Administrator`, `User`), registration capturing first/last name.
- **Full CRUD** for Tournaments, Teams and Players with server-side validation (DataAnnotations).
- **Possession-based simulation engine** — each player has nine attributes (shooting %s, defense,
  rebounding, passing, speed, stamina). Matches are simulated possession-by-possession, weighted by
  player stats, and are **deterministic when seeded** (which makes them unit-testable).
- **Team rating** = average overall rating of the team's top five players.
- **Brackets** — shuffle + pair approved teams, handle byes, advance winners round-by-round to a champion.
- **Admin tools** — approve teams, generate brackets, simulate a single match / whole tournament / full season.
- **Leaderboards** — top scorers, rebounders, assist leaders, best defenders, plus MVP voting.
- **Web API + AJAX** — live scores polling, dynamic bracket loading, live team/player search, MVP voting
  without a page refresh.
- **Modern sports UI** inspired by NBA / Sofascore / Flashscore (orange/black theme, cards, progress bars).
- **Custom error pages** (400 / 401 / 403 / 404 / 500).
- **Unit tests** (NUnit + Moq + EF Core InMemory) covering the service and simulation layers.
- **Future AI hook** — `ILineupAdvisor` interface is reserved for a later AI recommendation layer
  (lineup suggestions, substitutions, winner prediction).

---

## 🧱 Solution structure

```
KingOfTheStreet.sln
├── KingOfTheStreet.Data        # EF Core models, DbContext, seeder
├── KingOfTheStreet.Services    # Interfaces + implementations + simulation engine
├── KingOfTheStreet.Web         # MVC controllers, Razor views, Web API, Identity, wwwroot
└── KingOfTheStreet.Tests       # NUnit tests (services + simulator)
```

---

## 🔧 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server **LocalDB** (ships with Visual Studio; or adjust the connection string for full SQL Server)

---

## 🚀 Getting started

```bash
# 1. Restore + build
dotnet restore
dotnet build

# 2. (Optional but recommended) create the EF migration
#    The app will auto-create the schema if no migration exists, but a real
#    migration is the proper workflow and is required by the grading rubric.
dotnet tool install --global dotnet-ef        # if you don't have the EF CLI
cd KingOfTheStreet.Web
dotnet ef migrations add InitialCreate --project ../KingOfTheStreet.Data --startup-project .
dotnet ef database update --project ../KingOfTheStreet.Data --startup-project .

# 3. Run the web app
dotnet run --project KingOfTheStreet.Web
```

Then open the URL printed in the console (e.g. `https://localhost:5001`).

On first run the database is created and seeded automatically with courts, teams, players and tournaments.

---

## 👤 Demo accounts

| Role  | Email                          | Password    |
|-------|--------------------------------|-------------|
| Admin | admin@kingofthestreet.com      | Admin123!   |
| User  | captain@kingofthestreet.com    | Captain123! |

The admin can approve teams, generate brackets and run simulations from the **Admin → Dashboard** page.

---

## 🧪 Running the tests

```bash
dotnet test
```

To produce a coverage report:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

The tests cover tournament/team/player/match/bracket/statistics/MVP-vote services and the
deterministic match simulator.

---

## 🔌 Connection string

`KingOfTheStreet.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=KingOfTheStreetDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Change this if you target a different SQL Server instance.

---

## 🌐 Web API endpoints

| Method | Route                                | Description                       |
|--------|--------------------------------------|-----------------------------------|
| GET    | `/api/matches/live/{tournamentId}`   | Live + finished matches (polled)  |
| GET    | `/api/matches/bracket/{tournamentId}`| Bracket data for dynamic render   |
| POST   | `/api/matches/result`                | Enter a result (admin)            |
| POST   | `/api/votes/mvp`                      | Cast an MVP vote (authenticated)  |

---

## 📝 Notes on the simulation

The engine (`KingOfTheStreet.Services/Simulation/MatchSimulator.cs`) plays street rules: first to
**21**, win by **2**, hard cap **29**. Each possession picks a shooter and defender weighted by their
attributes, models steals, blocks, two-point (behind the arc) vs one-point (inside) shots, assists,
rebounds and the occasional trip to the free-throw line. Supplying a `seed` makes a game fully
reproducible, which is what the unit tests rely on.
