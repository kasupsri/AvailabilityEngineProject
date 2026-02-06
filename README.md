# Calendar Availability

API and Web UI for finding the earliest available meeting slot across multiple attendees. Implements the take-home exercise with a simple union-all-busy strategy and a design that allows swapping to a K-way merge strategy later.

## Design

**Two approaches considered**

- **Approach A (Union-all-busy + scan):** Collect all attendees’ busy intervals in the window, sort by start, merge overlaps into one “group busy” union, then scan for the first gap ≥ duration. Complexity: O(M log M) where M = total busy intervals.
- **Approach B (K-way merge with min-heap):** Each attendee’s list is already sorted; use a min-heap to merge K sorted lists, build merged blocks on the fly, scan for gaps. Complexity: O(M log K) where K = attendee count.

**Which one chosen and why**

Chose **Approach A** for the initial implementation: simplest to implement and reason about under the timebox, and correctness is easy to verify. The design keeps the “scan for earliest gap” logic behind an `IBusyBlockStream` abstraction so the strategy is swappable.

**What you deliberately did not build**

No authentication (per exercise). No persistence requirement in the spec (project uses SQLite for convenience; could be removed or swapped). No recurring events, no timezone conversion beyond UTC, no partial-day views or calendar metadata, no conflict resolution beyond busy intervals, minimal UI polish.

**What change or scale would make you revisit this design**

Revisit when: (1) very large M (many busy intervals per query) or many concurrent availability queries, where the global sort cost of Approach A dominates; (2) need for early-exit or streaming semantics. Then implement **Approach B** (`KWayMergeBusyStream`) and register it in DI in place of `UnionAllBusyStream`; the scanner and API stay unchanged.

## Schema

- **Persons**: `PersonId` (PK), `Name` (optional). One row per person/attendee.
- **PersonBusyIntervals**: `Id`, `PersonId` (FK → Persons, cascade delete), `StartUtc`, `EndUtc`. Many intervals per person; explicit relationship for referential integrity.
- **Later (meeting rooms):** The current model is person-centric only. Meeting room reservation can be added later (e.g. a separate `Room` + `RoomBusyIntervals` or similar) and availability can then consider both attendees and room availability; no change to the existing person model required.

## Decisions

- **PUT /api/calendars/{personId}/busy** replaces existing busy intervals for that person.
- **Unknown attendees** in GET /api/availability are treated as having no busy intervals (fully available).
- **Touching intervals** (e.g. `[1,2)` and `[2,3)`) are not merged during normalization; they remain separate.
- **Times** are UTC ISO-8601; invalid or non-UTC input is rejected or normalized as documented.
- **GET /api/availability** never returns a slot that starts before the current time (UTC); the effective window start is `max(windowStart, now)`.
- **Future scale:** Replace `IBusyBlockStream` with a `KWayMergeBusyStream` implementation in DI; the scanner and API stay unchanged.

## Run locally

1. **API** (from repo root):
   ```bash
   dotnet run --project Presentation/AvailabilityEngineProject.API
   ```
   Default URL: http://localhost:5251 (or see `Configurations/ApiConfiguration.json`). Create the DB with:
   ```bash
   dotnet run --project Presentation/AvailabilityEngineProject.DbPrimer.Console -- --connection-string "Data Source=./Data/app.db"
   ```

2. **Web** (from repo root):
   ```bash
   cd Presentation/AvailabilityEngineProject.Web && npm install && npm run dev
   ```
   Set `VITE_API_URL=http://localhost:5251` if the API runs on a different port.

## Docker

From repo root:

```bash
docker compose --profile init up -d dbprimer   # optional: init DB
docker compose up -d
```

- API: http://localhost/api (via nginx)
- Web: http://localhost/
- Swagger: http://localhost/swagger

## Production deployment (Proxmox)

Deploy using pre-built images and an existing Cloudflare Tunnel (cloudflared), same pattern as UserDirectory:

1. **Build images** on a build machine (from repo root): `docker compose build`. Optionally tag: `docker tag availabilityengine-api:latest availabilityengine-api:1.0.0` (and similarly for web). Save and copy to the VM: `docker save availabilityengine-api:latest availabilityengine-web:latest | gzip > images.tar.gz`, then scp to the VM and `docker load < images.tar.gz`.
2. **On the Proxmox VM:** Copy the repo (or relevant files) to `/opt/availabilityengine` so that `docker-compose.yml` and `deploy/docker-compose.production.yml` are in place, plus `nginx/` config. Do not copy `data/` or dev `.env` from your machine.
3. **One-time setup:** Run `sudo deploy/setup-vm.sh` to install Docker and create `/opt/availabilityengine` directories.
4. **Configure:** Create `/opt/availabilityengine/.env.production` from `deploy/.env.production.example` (set `VITE_API_URL` to your public URL, e.g. `https://availability.yourdomain.com/api`).
5. **Deploy:** From `/opt/availabilityengine`, run `sudo deploy/deploy.sh`. It uses the pre-loaded images (no build on server), starts Compose, and enables the systemd service.
6. **Cloudflared:** Configure your existing Cloudflare Tunnel to route your chosen hostname to this VM’s port 80 (e.g. `http://localhost:80` or the VM’s LAN IP). The app does not install cloudflared; use your existing tunnel.

## API

- **PUT /api/calendars/{personId}/busy** — Body: `{ "busy": [{ "start": "<ISO-8601>", "end": "<ISO-8601>" }] }`. Returns normalized busy intervals.
- **GET /api/availability** — Query: `attendees`, `windowStart`, `windowEnd`, `durationMinutes`. Returns `{ "found": true, "start", "end" }` or `{ "found": false }`. The returned slot always starts **at or after the current time (UTC)**; if the window start is in the past, the search effectively starts at "now".

No authentication. SQLite persistence.
