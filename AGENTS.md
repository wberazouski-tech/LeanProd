# LeanProd working agreements

## Project map

- Windows development: ASP.NET Core / .NET 8, EF Core 8, Angular 17.3, Node.js 20, SQL Server. SQLite is for isolated demos only; integration tests use SQL Server Testcontainers.
- API: `LeanProd.Api`; application contracts: `LeanProd.Application`; business rules: `LeanProd.Domain`; EF and services: `LeanProd.Infrastructure`; UI: `lean-prod-web`.
- Read the relevant feature document in `docs/` before changing business behavior. See `docs/README.md`, `docs/erp-rules.md`, and `docs/local-development.md`.
- Reply in Belarusian unless the user asks otherwise. Keep code identifiers consistent with the existing codebase.

## Authorized work

- Implement requested changes, inspect files, build, test, and fix related failures without repeatedly asking permission. Preserve unrelated uncommitted changes.
- Creating users, assigning roles, changing passwords, changing the active database, and importing business data must be explicitly within the user's authorized task. General environment setup does not imply these actions.
- Before a destructive database operation, identify the exact server/database, inspect affected data, and prepare a backup and recovery plan. Obtain explicit authorization for deletion, replacement, or rollback of existing data.
- Never print passwords, JWTs, or full secret files. Use User Secrets or local environment variables; never commit credentials.
- Do not publish, push, merge, or send external messages unless authorized. Do not stage or commit another task's changes.
- Work directly in the single branch `main` for all tasks. Create or switch to another branch, or create a worktree, only when the user explicitly requests it. Do not create task branches automatically.

## Implementing a task

- Define the actor, business rule, permissions, state transitions, and observable acceptance criteria. Use `docs/templates/erp-task.md` for a new business feature; small fixes need only the relevant parts.
- Record substantive agreed decisions in `docs/decisions/`; do not label an unapproved proposal as accepted.
- Do not change balances without a traceable business document. Preserve historical values and use decimal for quantities and money. Transactions, idempotency, cancellation, and concurrency need explicit behavior when relevant.
- API authorization must enforce the rule even when the UI hides an action. Respect the existing service boundaries and audit conventions.
- Migrations belong to `LeanProd.Infrastructure/Common/Persistence/Migrations`. Do not rewrite applied migrations. Explain data impact and verify against SQL Server.

## Skills

- Discoverable project skills are under `.agents/skills`. Their entrypoints link to the maintained instructions under `.github/skills`; read those instructions and resolve references from their original directory.
- Use only skills relevant to the task and announce first use. A skill's general advice does not expand task scope or override explicit user authorization.

## Run and verify

- Start API: `scripts\start-api.cmd`; start UI: `scripts\start-web.cmd` in another terminal.
- Local gate: `scripts\check.cmd`. This runs builds, domain tests, frontend lint/unit tests, translation checks, and pending EF model validation without applying migrations.
- Full integration gate: `scripts\check.cmd -Integration` with Docker. CI runs the full gates before merge; local mode reports that container tests are not run.
- For a targeted change, run the relevant checks first. Before handoff, report passed, failed, and unrun checks accurately. Do not weaken a check to obtain a green result.
- Use `npm.cmd` on Windows if PowerShell blocks `npm.ps1`. Database procedures and isolated demo launch: `docs/environments.md`.

## Completion

- Work in `main` and keep each task's changes reviewable, with acceptance evidence and business acceptance where applicable. A separate task does not require a separate branch.
- Summarize the resulting behavior, verification, database/configuration effects, and any remaining limitations. Do not equate a successful build with a tested business workflow.
