# Step 4 — Coin economy + Shop (hints & extra lives)

## What it does
- **Earn coins** by clearing a level: `gridSize × 5` coins, `+15` bonus for a
  flawless "Exact!" clear (see `Celebrate()` in `GameplayController.Fx.cs`).
  A floating "+N coins" popup shows it, and the HUD coin readout updates.
- **Spend coins** in two places:
  1. **Home → Shop** (the old "coming soon" Store tile now opens a real
     screen): buy a **Hint Pack** (+3 hints, 40 coins) or **Hint Value
     Pack** (+8 hints, 90 coins). These go into a persistent bonus-hint
     bank, separate from the 3 free hints every level starts with.
  2. **In-level, contextually**:
     - Tap the hint button after your 3 free hints run out → if you have
       bonus hints banked, one is drawn in automatically; if not, a buy
       prompt opens right there (same Hint Pack as the Shop).
     - Run out of lives → the fail screen now offers **"Continue — 30
       coins"** (restores 1 life, keeps your current board progress,
       no restart) alongside the existing "Restart Level".

## New files
- `Assets/Scripts/Save/CoinSave.cs` — coin balance (PlayerPrefs, same
  pattern as the existing `CampaignSave`/`SettingsSave`). Starts everyone
  with 60 coins so the Shop isn't empty on first run.
- `Assets/Scripts/Save/HintBankSave.cs` — persistent bonus-hint stockpile.
- `Assets/Scripts/Gameplay/ShopCatalog.cs` — **single source of truth for
  all prices/quantities**. The Shop screen and the in-level prompts both
  call into this, so they can never disagree. Tune the economy by editing
  the constants at the top of this one file.

## Files changed
- `GameplayController.cs` — added `coinText` field.
- `GameplayController.Hud.cs` — coin readout next to the score, `RefreshCoinText()`.
- `GameplayController.Fx.cs` — coins awarded in `Celebrate()`.
- `GameplayController.Hints.cs` — draws from the bonus bank / opens the buy prompt when free hints run out.
- `GameplayController.Overlays.cs` — `ShowBuyHintPrompt()` (new), fail screen now offers a coin-based continue.
- `GameplayBootstrap.cs` — Store tile now opens `BuildShopOverlay()` (new) instead of "Coming Soon"; Leaderboard and Daily Challenge are still coming-soon placeholders.

## Not built (deliberately, to keep this scoped)
- No real-money purchases — this is entirely the in-game coin economy you
  asked for, not an IAP/store integration.
- No cosmetic items — only hints and the life-continue, per your answer.
- Prices are a reasonable first guess, not balanced against real playtesting
  — they're all in one file (`ShopCatalog.cs`) specifically so they're easy
  to retune once you see how players actually earn/spend.

## Try it
Play and clear a level → watch the coin popup and the HUD number go up.
Open Home → Shop → buy a hint pack. In a level, burn through all 3 free
hints → the buy prompt should appear (or silently draw from your bank if
you bought one). Lose all 3 lives → the fail screen should offer to
continue for coins.
