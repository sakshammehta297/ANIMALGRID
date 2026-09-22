using UnityEngine;
using AnimalGrid.Core;
using AnimalGrid.Audio;
using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    // ---------- Hints ----------
    // 3 free hints per level (hintsLeft, declared in the core file). Once
    // those run out, this draws from the persistent bonus-hint bank
    // (HintBankSave) if any are owned; if the bank is empty too, it opens the
    // buy-hints prompt (GameplayController.Overlays.cs) instead of no-op'ing.
    public partial class GameplayController
    {
        private void UseHint()
        {
            if (finished || inputLocked) return;

            if (hintsLeft <= 0)
            {
                if (HintBankSave.TryConsumeOne())
                {
                    hintsLeft = 1; // draw one bonus hint into play immediately
                }
                else
                {
                    ShowBuyHintPrompt();
                    return;
                }
            }

            hintsLeft--;
            if (hintCountText != null) hintCountText.text = hintsLeft.ToString();
            SoundManager.Instance?.PlayHint();

            var hint = hintProvider.GetHint(puzzle, state);
            if (!hint.found) return;

            var view = board.GetCellView(hint.row, hint.column);
            StartCoroutine(PulseCell(view));

            bool elimination = hint.kind == HintKind.Elimination;
            string label = elimination ? "X here!" : "Place here!";
            var color = elimination
                ? new Color(0.35f, 0.35f, 0.4f)
                : new Color(0.95f, 0.75f, 0.2f);
            StartCoroutine(FloatText(view.transform, label, color, 44,
                Vector2.zero, new Vector2(0f, 120f), 2.5f));
        }
    }
}
