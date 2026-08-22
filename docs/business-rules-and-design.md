# Kanta Maheswara Seva Sangam — Business Rules & System Design

Status: **draft — pending admin sign-off on the open questions in §4**
Source material: 5 photographs of the Sangam's physical ledger books (member khata pages, monthly cash-report pages, the loan/guarantor register, and the receipts-&-payments statement dated 11-04-2026).

This document is the output of step 1–7 of the project's own instructions
("validate business rules → understand the ledger → design the financial
model → design the database → define calculation rules and test cases →
design the API") before any business logic is implemented. Code in this
branch is limited to a non-functional project skeleton (§7); no accounting
logic has been written yet.

---

## 1. What the ledger actually contains

The physical book has three kinds of pages per month, repeated for every khata (member book) and every month:

1. **Member khata page** (per member): one row per month with columns for
   share/installment amount, ordinary-loan principal installment, ordinary-loan
   interest, special-loan principal installment (అనలు), special-loan interest —
   mirrored for both "ordinary" (సాధారణ) and "special" (ప్రత్యేక) loan products.
2. **Society-side ledger page** (per member, opposite the khata page): running
   cumulative share total, fines (జరిమానా), other dues, total amount paid that
   month, loans received (with guarantor name/number), and remaining loan balance.
3. **Monthly pages**, once per month:
   - A **loan/guarantor register**: for each new loan disbursed that month, the
     borrower's khata number, the guarantor's name, and the amount sanctioned.
   - A **receipts & payments statement** ("జమా ఖర్చుల పట్టిక"): the monthly
     financial close.

### Confirmed by cross-checking the numbers

I recomputed several months of one member's khata (Khata No. 44) against the
1%-of-outstanding-principal rule from §3 of the original brief, and it
matches exactly:

- **Loan 1: ₹20,000**, ₹2,000 principal/month → interest sequence
  200, 180, 160, 140, 120, 100, 80, 60, 40, 20 → paid off in exactly 10 months.
  Every value in this sequence is 1% of the outstanding balance *before* that
  month's principal payment, decreasing by ₹20 each month because principal
  drops by a flat ₹2,000. This matches the ledger digit-for-digit.
- **Loan 2 (same member, taken right after Loan 1 closed): ₹27,000**, same
  ₹2,000/month principal → interest sequence 270, 250, 230, 210, 190, 170,
  150, 130 across 8 months, remaining balance ends at ₹11,000 — again an
  exact match to both the khata page and the society-side page.
- **Monthly close for period 04 (dated 11-04-2026)** reconciles exactly:
  - Receipts: Shares ₹12,400 + Ordinary loan principal collected ₹76,500 +
    Ordinary loan interest collected ₹9,255 = **₹98,155**
  - \+ Previous month balance (11-03-2026) ₹4,900 = **₹1,03,055 total available**
  - Expenditure: Ordinary loans disbursed **₹1,00,000** (four loans of
    ₹25,000 each, per the guarantor register — see below)
  - Closing balance = 1,03,055 − 1,00,000 = **₹3,055**, matching the book exactly.
  - Shares also cross-check: ₹12,400 ÷ ₹200 = 62 members, matching the
    community's stated member count for that month with zero misses.
- **The guarantor register for period 04** lists 4 disbursed loans, each
  ₹25,000, each against a named guarantor (a different member), totaling the
  ₹1,00,000 expenditure line above.

This confirms, with real numbers rather than the brief's illustrative
example, that:

1. Interest = `InterestRate% × OutstandingPrincipalBeforeThisPayment`, not on
   the original principal and not on a post-payment balance.
2. Principal repayment amounts are chosen per month (the brief's flexible-repayment
   assumption is correct — this member paid a flat ₹2,000/month, but the
   amount is a recorded fact, not a formula).
3. A member can take a second loan after fully closing the first — loans are
   sequential per member in the observed data, though nothing in the ledger
   proves only one *could* be open at a time (open question, §4).
4. **There are two loan products** — "ఆర్డినరీ/సాధారణ" (ordinary) and
   "ప్రత్యేక" (special) — tracked in fully parallel columns everywhere. The
   sampled book has zero special-loan activity, so its rules are unverified
   from the data, but the schema must have the column to avoid a rewrite later.
5. **Every loan has a named guarantor**, recorded in a dedicated register at
   disbursement time.
6. The monthly close has a fixed structure with more income/expense lines
   than the original brief's example used (some blank in the sampled month,
   but present as ledger columns — see §5.3): fines, extra installments, form
   sales, and other income on the receipts side; wages, stationery, and
   "సాధర" (misc) on the expenditure side, alongside loan disbursement/collection.

---

## 2. Decisions already made (this session)

| Decision | Choice | Rationale |
|---|---|---|
| Guarantor tracking | **In scope for MVP** — `Loan.GuarantorMemberId` | Matches the paper process exactly; expensive to retrofit onto existing loan records later. |
| Loan types | **Modeled in schema, only Ordinary exposed in v1 UI** | `LoanType` enum exists now so Special loans are a UI/config change later, not a migration — but the sampled book shows no Special-loan activity, so it isn't built out. |
| Historical data (2021→) | **Not migrated for v1.** System launches with each member's *current* outstanding loan balance and running contribution total as opening balances. Schema supports a later bulk import (period-tagged rows) without redesign. | Migrating ~4.5 years × ~62 members of handwritten data is a large, separate effort with its own risk of transcription error; it shouldn't block getting the live monthly process off paper. |
| This session's deliverable | **Design doc (this file) + non-functional project skeleton** | Matches the project's own instruction to validate rules and design before implementing. |

---

## 3. Financial model

### 3.1 Entities and the money flow

```
Member ──contributes──▶ Contribution (₹200/month, one row per member per period)
Member ──borrows──────▶ Loan (Ordinary | Special, has a Guarantor Member)
Loan   ──repaid via───▶ LoanRepayment (principal chosen by admin, interest computed)
Every inflow/outflow ─▶ Transaction  (append-only ledger — the source of truth
                                       for the monthly financial summary)
```

`Transaction` is the general ledger: **every** rupee that moves — a
contribution, a loan disbursement, a principal or interest repayment, a fine,
a wage payment — is written there exactly once, tagged with a
`TransactionType`, a period (year+month), and who recorded it. `Contribution`,
`Loan`, and `LoanRepayment` are the specialized tables the UI and business
rules operate on; writing to them and writing the corresponding `Transaction`
row happen in the same database transaction (unit of work), so the ledger
can never drift from the domain tables. **The monthly summary report is a
computed aggregation over `Transaction`, grouped by period and type — it is
never a hand-entered or separately-maintained total**, which is the core
"replace manual calculation" win for the admin.

### 3.2 Interest calculation rule (confirmed against real data, §1)

For a repayment recorded against a loan in a given month:

```
interestDue      = round(loan.OutstandingPrincipal × loan.InterestRatePercent / 100, 2)
principalPaid    = admin-entered amount (0 ≤ principalPaid ≤ loan.OutstandingPrincipal)
newOutstanding   = loan.OutstandingPrincipal − principalPaid
totalCollected   = principalPaid + interestDue   (+ that period's ₹200 share, collected as a separate Contribution)
```

Interest is **never** computed on the original disbursed principal, and never
on the balance *after* the current payment — only on the balance the loan
carried into the payment. This is a pure function of `(OutstandingPrincipal,
InterestRatePercent)`, always computed server-side; the client only submits
`principalPaid` and the period.

### 3.3 Worked test cases (to become unit tests)

**Case A — brief's own example (₹10,000 loan, ₹1,000/month principal):**

| Month | Opening | Interest (1%) | Principal paid | Total collected | Closing |
|---|---|---|---|---|---|
| 1 | 10,000 | 100 | 1,000 | 1,300 (incl. ₹200 share) | 9,000 |
| 2 | 9,000 | 90 | 1,000 | 1,290 | 8,000 |

**Case B — Khata 44, Loan 1, verified against the physical ledger (₹20,000, ₹2,000/month):**

| Month | Opening | Interest | Principal | Closing |
|---|---|---|---|---|
| 1 | 20,000 | 200 | 2,000 | 18,000 |
| 2 | 18,000 | 180 | 2,000 | 16,000 |
| … | … | … | … | … |
| 10 | 2,000 | 20 | 2,000 | 0 (loan closes) |

**Case C — Khata 44, Loan 2, verified (₹27,000, ₹2,000/month):**

| Month | Opening | Interest | Principal | Closing |
|---|---|---|---|---|
| 1 | 27,000 | 270 | 2,000 | 25,000 |
| … | … | … | … | … |
| 8 | 13,000 | 130 | 2,000 | 11,000 |

**Case D — monthly summary reconciliation, period 04 (verified against the printed statement):**

```
Receipts:      Shares 12,400 + Loan principal 76,500 + Loan interest 9,255 = 98,155
Total available = 98,155 + opening balance 4,900                          = 103,055
Expenditure:   Loans disbursed 100,000                                    = 100,000
Closing balance = 103,055 − 100,000                                       = 3,055
```

These four cases should be the first unit tests written against the interest
engine and the summary aggregation query, before any UI is built on top of them.

### 3.4 Rounding

The sampled data never produces a fraction (all interest values are whole
rupees because outstanding balances are always multiples of 100 in the
sample). The rule as designed rounds to 2 decimal places (paise) using
banker's-safe `decimal` arithmetic throughout — `float`/`double` must never
touch a money value. The rounding *mode* (round-half-up vs. round-half-even)
is still an open question (§4) since no sampled case disambiguates it.

---

## 4. Open questions still needing the admin's confirmation

Answered this session: guarantor tracking (yes), loan-type scope (model both,
ship Ordinary only), historical migration (no, fresh start). Everything below
is unconfirmed and the code must not silently assume an answer — each of
these is a `TODO(business-rule)` in the codebase once logic is written.

**Contributions**
- Can ₹200 change over time (and if so, per-member or Sangam-wide, effective from which month)?
- What happens when a member misses a month — does it accumulate as arrears, and is there a penalty?
- Mid-year joiners/leavers — prorated first month, or skipped?

**Loans**
- Who approves a loan (is "recorded by admin" sufficient, or is there a separate approval step/status)?
- Is there a maximum loan amount or an eligibility formula (e.g., multiple of contributions paid)?
- Can a member hold more than one **active** loan at once, across either loan type? (Sampled data shows sequential, not concurrent, loans for one member — but that's one member, not a rule.)
- Can loans be disbursed any day, or only at the monthly (11th) meeting?
- Early payoff — is there a discount/rule, or just "pay remaining principal + that month's interest"?

**Interest**
- Confirm 1% is fixed Sangam-wide (vs. per-loan negotiated rate) and whether it can change over time.
- If a borrower pays ₹0 principal in a month, is interest still charged on the unchanged balance? (Model says yes — needs confirmation.)
- Any late-payment penalty, and how is it distinct from the `Fine` transaction type already seen as a ledger column?
- Confirm rounding mode for interest that isn't a whole rupee.

**Common fund**
- Does 100% of collected interest, fines, and form-sale income join the general fund available for new loans (current assumption: yes, all `Transaction` inflows are fungible), or is any of it earmarked?
- Confirm closing balance always rolls forward as next month's opening balance with no other adjustment.

**Corrections**
- Confirmed direction (matches §18 of the brief): never hard-delete a financial row. A correction is a new `Transaction`/`LoanRepayment` row referencing (`ReversalOf...Id`) the one it corrects, and the original stays in place with `IsReversed = true`. Needs the admin's confirmation that this satisfies their audit expectations.

**Growth**
- Confirm this system is single-tenant (one Sangam) for the foreseeable future — it changes whether `TenantId`-style multi-tenancy is worth designing in now versus later. Current design assumes single-tenant.

---

## 5. Database schema (as scaffolded in `backend/`)

Six tables, matching `backend/src/KMSS.Api/Domain/Entities/*.cs` and the
EF Core configuration in `KmssDbContext`:

| Table | Purpose | Key relationships |
|---|---|---|
| `Members` | One row per Sangam member. `KhataNo` (unique) is the paper book number. | — |
| `Users` | Login identity (username, password hash, role). Optionally linked to a `Member`. | `Users.MemberId → Members.Id` (nullable) |
| `Contributions` | One row per member per month for the ₹200 share. Unique on `(MemberId, PeriodYear, PeriodMonth)`. | `→ Members.Id` |
| `Loans` | One row per loan. Carries `OutstandingPrincipal` as a cached/derived balance. | `→ Members.Id` (borrower), `→ Members.Id` (guarantor) |
| `LoanRepayments` | One row per recorded repayment, snapshotting the balance after. | `→ Loans.Id` |
| `Transactions` | Append-only general ledger — every inflow/outflow, source of truth for reporting. | `→ Members.Id`, `→ Loans.Id` (both nullable) |

All money columns are `decimal(12,2)`. All foreign keys use `Restrict` delete
behavior — nothing about a member or loan can cascade-delete financial
history, in line with the "no hard deletion of financial transactions"
principle.

`OutstandingPrincipal` on `Loan` is intentionally denormalized for fast
reads (member dashboards, admin loan lists) but is a derived value: it must
always equal `PrincipalDisbursed − Σ(non-reversed LoanRepayment.PrincipalPaid)`
and is only ever written inside the same transaction as a repayment or a
correction — reconciling it against the repayment history is a natural
periodic integrity check to build once repayments exist.

---

## 6. API surface (planned, not yet implemented)

Only `GET /api/health` exists in this branch. Planned v1 surface, all
requiring auth (JWT bearer) once implemented:

**Admin**
- `GET/POST/PUT /api/members` — manage members, activate/deactivate
- `POST /api/contributions` — record a month's ₹200 for a member
- `POST /api/loans` — create a loan (borrower, guarantor, principal, type)
- `POST /api/loans/{id}/repayments` — record a repayment (principal only; interest computed server-side)
- `POST /api/loans/{id}/repayments/{repaymentId}/reverse` — correction, never a delete
- `GET /api/reports/monthly-summary?year=&month=` — the computed receipts & payments statement
- `GET /api/reports/yearly?year=` — yearly rollup
- `GET /api/members/{id}/statement` — full member history

**Member** (own data only — enforced server-side by `MemberId` matching the authenticated user, not by trusting a client-supplied id)
- `GET /api/me/dashboard` — totals, active loan, next expected payment
- `GET /api/me/contributions`
- `GET /api/me/loans`

The frontend never computes a balance or an interest figure for display
purposes beyond formatting what the API returns — this is the same
"AI/frontend is never the source of truth for money" principle applied to
the frontend generally, not just to a future AI layer.

---

## 7. What exists in this branch right now

- `backend/` — ASP.NET Core 8 Web API skeleton: solution file, the six
  entities and enums above, `KmssDbContext` with full EF Core configuration
  (precision, indexes, unique constraints, delete behavior), CORS wired for
  the frontend dev server, and a single `GET /api/health` endpoint. **No
  business logic, no auth, no other controllers yet** — those need the open
  questions in §4 resolved first. This sandbox has no .NET SDK installed, so
  the project has not been `dotnet build`-verified here; the code is
  standard EF Core 8 / ASP.NET Core 8 and should build once restored with a
  local SDK (`dotnet restore && dotnet build` from `backend/`).
- `frontend/` — Vite + React + TypeScript skeleton, verified to build
  (`npm run build`) in this sandbox. Routing shell for `/login`, `/admin`,
  `/member` with placeholder pages, a typed `models.ts` mirroring the DTOs
  above, an axios client with bearer-token injection, and a stub
  `AuthContext` — no real login flow, no data fetching yet.
- Database engine chosen for the skeleton: **PostgreSQL** (via Npgsql), as
  the stronger fit for the project's $0/month hosting goal (Neon, Supabase,
  and Railway all offer a free Postgres tier; free-tier SQL Server hosting
  options are far more limited). This is a recommendation, not yet
  reconfirmed with the admin — swapping the provider before any migration
  exists is a one-line change in `Program.cs`.

## 8. Suggested next steps

1. Get admin answers to §4 — at minimum: missed-payment handling, max
   loan/concurrent-loan rules, and rounding mode, since those three shape
   the repayment API's validation logic.
2. Implement auth (register/login, JWT issuance, role-based authorization)
   against the `Users` table.
3. Implement `Members` CRUD + `Contributions` recording, with the paired
   `Transaction` write, and unit-test against Case D's totals (§3.3).
4. Implement `Loans` + `LoanRepayments`, unit-tested against Cases A–C.
5. Implement the monthly-summary aggregation query, unit-tested against Case D.
6. Wire the React admin/member dashboards to real endpoints.
