# Repository collaboration rules

This repository contains the Windows Codex usage desktop pet described in `README.md` and `docs/PLAN.md`.

## Start here

1. Read `README.md`, `docs/PLAN.md`, and the latest phase report before changing code.
2. Preserve the split between the Codex-native v2 pet package and the standalone Windows overlay.
3. Keep account data, tokens, cookies, live usage snapshots, local state, logs, and generated intermediates out of Git.

## Implementation rules

- Target `.NET 10` and WPF unless a documented migration changes the decision.
- Keep usage acquisition behind `IUsageProvider`; UI and threshold logic must not depend on raw app-server JSON.
- Prefer `account/rateLimits/read` and rolling updates from a locally launched Codex app-server. Never read Codex credential files.
- Identify the 5-hour and weekly windows by their durations (`300` and `10080` minutes), with tolerant parsing for missing or future fields.
- Compute remaining usage as `clamp(100 - usedPercent, 0, 100)`.
- Never treat missing or stale usage as zero, and never fire threshold reactions from stale/unavailable data.
- Threshold events use downward crossings of `80, 60, 40, 20, 1`; persist notified levels per window/reset period.
- Keep the state machine, threshold engine, protocol parser, and UI separable and testable.
- Use placeholder/vector visuals during runtime development. Formal character sprites must follow the hatch-pet v2 generation and QA workflow.

## Validation

- Run the test project for every behavior or usage change.
- Build the WPF app before committing a completed phase.
- Do not commit `bin`, `obj`, live snapshots, logs, account identifiers, or generated image intermediates.
- Update `README.md` and `docs/PLAN.md` when a phase or public command changes.

## Intellectual-property boundary

The planned Fengjin/Hyacine visual is a personal fan-made transformation. Do not extract, bundle, or redistribute original Honkai: Star Rail or ASUS mascot assets, logos, UI, or animation frames. Do not claim affiliation or endorsement.

