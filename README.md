# 🌍 Shake to Travel

A project that turns an **Arduino MKR IoT Carrier** into a playful travel
randomizer. You pick a continent on the device, **shake it** to land on a random
country, and save your "trip" to a backend. A **Blazor WebAssembly** web app
shows your travel log live.

The project ties together three worlds: **embedded hardware (Arduino)**,
a **.NET backend API**, and a **web frontend** — demonstrating a full end-to-end
flow from a physical device to a database to a browser.

---

## What it does

1. **Pick** a continent (Europe, Asia or Africa) on the Arduino's screen.
2. **Shake** the device — it requests a random country from an external
   countries API.
3. **Press 00** to save the country. The Arduino sends the country data to the
   backend as an HTTP `POST`.
4. The backend stores the country in a database.
5. The **web app** polls the backend and displays the latest destination,
   statistics, and a scrollable **travel log** — updating automatically.

```
┌─────────────┐   GET country    ┌──────────────────────┐
│   Arduino   │ ───────────────▶ │  External Countries  │
│ MKR IoT     │ ◀─────────────── │  API (REST)          │
│ Carrier     │                  └──────────────────────┘
│             │
│  (shake &   │   POST country   ┌──────────────────────┐
│   save)     │ ───────────────▶ │   MiniApi backend    │
└─────────────┘                  │   (.NET Web API)     │
                                 │   + database         │
                                 └──────────┬───────────┘
                                            │ GET countries (poll)
                                            ▼
                                 ┌──────────────────────┐
                                 │  ShakeToTravel.Web   │
                                 │  (Blazor WebAssembly)│
                                 └──────────────────────┘
```

---

## Project structure

```
H3-MiniApi/
├── Countries_00.ino          # Arduino sketch (device logic)
├── arduino_secrets.h         # Wi-Fi credentials for the Arduino (not committed)
│
├── MiniApi/                  # Backend REST API (.NET 10)
│   ├── Controllers/
│   │   ├── CountriesController.cs   # GET/POST endpoints for countries
│   │   └── HelloController.cs       # Simple health/test endpoint
│   ├── Data/
│   │   └── AppDbContext.cs          # EF Core database context
│   ├── Models/
│   │   └── Country.cs               # Country entity (shared shape with device)
│   ├── Migrations/                  # EF Core database migrations
│   └── Program.cs                   # App startup, DB & CORS configuration
│
└── ShakeToTravel.Web/        # Frontend web app (Blazor WebAssembly)
    ├── Pages/
    │   └── Home.razor               # Main dashboard (latest trip, stats, log)
    ├── Layout/
    │   └── MainLayout.razor
    ├── Models/
    │   └── Country.cs               # Country model used by the UI
    └── Program.cs                   # Client startup & HttpClient setup
```

---

## The three components

### 1. Arduino (`Countries_00.ino`)
Runs on the **MKR IoT Carrier**. It handles:
- A small **state machine**: pick continent → wait for shake → show country → save.
- **Shake detection** using the onboard accelerometer.
- Drawing menus and results on the round **display**, driven by the carrier buttons.
- Two network calls:
  - `GET` to an external **countries API** (over HTTPS) to fetch country details.
  - `POST` to the **MiniApi backend** (over local HTTP) to save the chosen country.

### 2. Backend — `MiniApi` (.NET 10 Web API)
A minimal REST API that stores and serves countries.
- **`Country`** model carries the fields sent from the device: name/choice,
  country code (`cca3`), capital, region, subregion, population, area,
  coordinates and flag URL — plus a server-side `Id` and `CreatedAt`.
- **`CountriesController`** exposes:
  - `POST /api/countries` — save a new country (validates input, rejects duplicates).
  - `GET /api/countries` — list saved countries, with optional `region` and
    `search` filtering.
  - `GET /api/countries/{id}` — fetch a single country.
- **`AppDbContext`** uses Entity Framework Core. The data provider is
  configurable: an **in-memory database** for quick testing, or **PostgreSQL**
  for a real database.
- **CORS** is enabled so the Blazor web app can call the API from the browser.

### 3. Frontend — `ShakeToTravel.Web` (Blazor WebAssembly)
A single-page dashboard that visualizes the travel log.
- **Latest destination** card with the country's flag, capital, region,
  population, area and a link to view it on a map.
- **Statistics** panel: total countries visited and counts per continent.
- **Travel log** list with per-continent filter pills.
- **Live updates**: the page periodically polls `GET /api/countries` and shows a
  highlight animation when a new country arrives — so shaking the Arduino makes
  the web app react within seconds.
- Styled with a soft gradient theme and **Font Awesome** icons.

---

## Technologies used

| Area      | Technology                                             |
|-----------|--------------------------------------------------------|
| Hardware  | Arduino MKR IoT Carrier (accelerometer, display, Wi-Fi)|
| Device SW | Arduino C++ (`WiFiNINA`, `ArduinoHttpClient`, `Arduino_JSON`) |
| Backend   | .NET 10, ASP.NET Core Web API, Entity Framework Core   |
| Database  | PostgreSQL (or in-memory for testing)                  |
| Frontend  | Blazor WebAssembly, Font Awesome                        |

---

## Data model

Each saved country includes:

| Field                   | Description                                 |
|-------------------------|---------------------------------------------|
| `Id`, `CreatedAt`       | Set by the backend when saved               |
| `ApiName`               | Which external API the data came from        |
| `Choice`                | The country name                            |
| `Cca3`                  | 3-letter country code (e.g. `DNK`)          |
| `Capital`               | Capital city                                |
| `Region` / `Subregion`  | Continent and sub-region                     |
| `Population`, `Area`    | Size figures                                |
| `Latitude`, `Longitude` | Coordinates (used for the map link)         |
| `FlagUrl`               | Link to the country's flag image            |

---
