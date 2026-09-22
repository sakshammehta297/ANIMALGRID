# Art-ready UI system — what changed and how to use it

This upgrade makes `UiFactory` (and everything it builds — buttons, panels,
progress bars, tiles) automatically pick up real art and fonts the moment
they're dropped into the project, with **zero further code changes**. Until
then, every screen keeps working exactly as it does today, using nicer
anti-aliased procedural shapes as the fallback.

## Files changed / added
- `UiSprites.cs` — anti-aliased procedural shapes; `RoundedSquare` now carries
  9-slice border data; added a soft `Shadow` sprite; `UiFonts` now has
  `Display` / `Body` properties with automatic fallback.
- `ArtLoader.cs` — added `GetUi()`, `GetIcon()`, `GetFont()` alongside the
  existing `Get()` / `GetAnimal()`.
- `UiFactory.cs` — `MakeButton` and `MakeUnlockBar` gained optional `artKey`
  parameters (fully backward compatible — every existing call site still
  compiles unchanged); added `MakeIconButton` (icon-on-top, label-below tile,
  the pattern used for a bottom nav row) and `MakeArtImage`.
- `UiButtonFeedback.cs` — **new**. Auto-added to every button UiFactory
  creates: a small scale-down-on-press, spring-back-on-release animation.
  No package required; swap it for a DOTween call later if you want easing.
- `GameplayBootstrap.cs` — the Home screen's bottom row (Collection,
  Settings, Leaderboard, Daily Challenge, Store) now uses `MakeIconButton`
  instead of plain text buttons, as a live example.
- `Assets/Resources/Art/Icons/icon_shop.png` — the existing
  `Assets/Art/Icons/icon_store.png` copied into the `Resources` folder
  (Unity's `Resources.Load` only sees files literally inside a `Resources`
  folder) and renamed to match the `"shop"` icon key used for the Store
  button. **Try it now:** press Play — the Store tile already shows this
  icon, everything else still shows text-only. That's the fallback system
  working correctly, not a bug.

## Drop-in art folder convention
Create these under `Assets/Resources/` as real art arrives:

| Path | Used for | Loader |
|---|---|---|
| `Art/<name>.png` | full backgrounds | `ArtLoader.Get(name)` |
| `Art/animal_<id>.png` | animal stickers | `ArtLoader.GetAnimal(id)` |
| `Art/UI/<name>.png` | button/panel chrome (wood plank, gold pill, etc.) | `ArtLoader.GetUi(name)` |
| `Art/Icons/icon_<id>.png` | small nav/tile icons | `ArtLoader.GetIcon(id)` |
| `Fonts/Display.ttf` | logo/header font | `UiFonts.Display` |
| `Fonts/Body.ttf` | button/body font | `UiFonts.Body` |

Import UI chrome sprites as **Sprite (2D and UI)** and set a **Border** in
the Sprite Editor (drag the corner handles) so 9-slicing keeps corners crisp
at any button size instead of stretching. Icons and animal stickers don't
need a border.

## Using it in new/existing code
```csharp
// Today (no art yet) — unchanged, still works:
UiFactory.MakeButton(parent, "Play", pos, size, myGreen, Color.white);

// Once Resources/Art/UI/button_wood.png exists — one extra argument:
UiFactory.MakeButton(parent, "Play", pos, size, myGreen, Color.white, artKey: "button_wood");

// Icon + label tile (bottom nav row style):
UiFactory.MakeIconButton(parent, "shop", "Shop", pos, size, tan, brown);
```

## Not done yet (see main task list)
- No real art/fonts exist yet — this only builds the plumbing.
- Full Home screen layout redesign (top HUD avatar/XP bar, painted forest
  background, mascot cluster) still needs the actual art assets first.
- Cell-placement / win-celebration animation (the DOTween pass discussed
  separately) is a different file (`CellView.cs` / `BoardView.cs`) — not
  touched in this change.
