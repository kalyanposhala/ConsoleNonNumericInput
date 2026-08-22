# Kanta Maheswara Seva Sangam — Finance Management System

A digital replacement for the Sangam's paper ledger books: member
contributions, loans, flexible-principal repayments with interest-on-outstanding
calculation, common fund balance, and monthly/yearly reporting.

**Start here:** [`docs/business-rules-and-design.md`](docs/business-rules-and-design.md) —
the business rules extracted and verified from the physical ledger, the open
questions still pending admin confirmation, the database schema, and the
calculation rules with worked test cases. Read that before touching the code;
it explains *why* the schema looks the way it does.

## Status

Design phase. This repo currently holds a **non-functional project skeleton**
only — no business logic, no auth, no data screens. See the design doc's
"What exists in this branch right now" section for the exact current state
and the suggested next steps.

## Structure

```
backend/    ASP.NET Core 8 Web API (C#) — the authoritative source of truth
            for every financial calculation. PostgreSQL via EF Core.
frontend/   React + TypeScript (Vite) — dashboards, forms, reports. Never
            computes a balance or interest figure itself; only displays
            what the API returns.
docs/       Business rules, database schema, API design, open questions.
```

## Running locally

### Backend

Requires the .NET 8 SDK (not installed in the sandbox this skeleton was
built in — the code has not been `dotnet build`-verified here).

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/KMSS.Api
```

Set a real PostgreSQL connection string in
`src/KMSS.Api/appsettings.Development.json` (or via `dotnet user-secrets` /
environment variables — never commit real credentials) before running against
a real database. `GET /api/health` should return `{"status":"ok"}` once running.

### Frontend

```bash
cd frontend
npm install
cp .env.example .env   # points at the local API by default
npm run dev
```

## Tech stack and why

| Layer | Choice | Why |
|---|---|---|
| Backend | ASP.NET Core 8 Web API, C# | Career-relevant .NET skill deepening; financial rules belong server-side, never in the frontend or an AI layer. |
| Frontend | React + TypeScript (Vite) | Deliberately outside the author's existing .NET comfort zone — a stated learning goal — while staying a plain responsive website (no native mobile requirement exists yet). |
| Database | PostgreSQL | Best fit for the $0/month hosting goal — Neon/Supabase/Railway all offer usable free Postgres tiers. |
| Architecture | Modular monolith | ~100 users doesn't justify microservices; a single deployable with clean internal layering is both simpler to run for free and easier to reason about for financial correctness. |

AI is intentionally **not** part of v1. Per the project brief, AI is a
phase-2+ feature layered on top of a working, trustworthy deterministic
ledger — it will never be the source of truth for a balance or an interest
figure.
