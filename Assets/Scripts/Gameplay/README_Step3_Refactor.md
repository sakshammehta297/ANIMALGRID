# Step 3 — GameplayController.cs split into partial classes

## What changed
`GameplayController.cs` was 1041 lines — flagged in the earlier review as
exactly the "giant GameManager monolith" the project's own `AGENTS.md` style
rule warns against. It's now split into 8 files by responsibility, using
C#'s `partial class` — **same type, same fields, same public API, zero
behavior change**. This is the lowest-risk way to break up a big
MonoBehaviour: anything in the scene/Inspector that references
`GameplayController` keeps working untouched, because it's still one class.

| File | Responsibility | Lines |
|---|---|---|
| `GameplayController.cs` | fields, `Initialize`, `Update` (debug keys), `AddHomeButton` | 197 |
| `GameplayController.Input.cs` | tap handling, placement rules, correctness check | 154 |
| `GameplayController.Hints.cs` | hint button | 32 |
| `GameplayController.Hud.cs` | always-on-screen HUD: score, hearts, tray, hint button build | 199 |
| `GameplayController.Overlays.cs` | full-screen panels: boss intro, fail, complete, unlock, world, finale | 188 |
| `GameplayController.Campaign.cs` | post-level advancement / scene reload | 46 |
| `GameplayController.Fx.cs` | scoring, win celebration sweep, flash/shake/pulse/float-text coroutines | 198 |
| `GameplayController.Tutorial.cs` | onboarding tutorial flow | 117 |

Largest file went from 1041 lines to 199 — every file is now small enough to
read top-to-bottom in one sitting.

## Verified before packaging
- Every one of the original 42 methods appears in exactly one file, with no
  duplicates and none missing (checked by grepping method signatures across
  all 8 files).
- Only one file declares the `: MonoBehaviour` base and only one `Initialize`/
  `Update` exist — Unity requires exactly one base declaration across a
  partial class's files.
- Brace-balance checked file by file.

## What this does NOT do
This is a structural split only — no logic changed, no method renamed, no
field moved out of the core file. If two methods in different files feel
like they belong together differently than I grouped them, moving a method
between these files is a safe, mechanical cut-and-paste (they're all still
the same class).

## Drop-in
Copy all 8 `GameplayController*.cs` files into
`Assets/Scripts/Gameplay/`, replacing the old single file.
