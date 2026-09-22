using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AnimalGrid.Core;
using AnimalGrid.Audio;
using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    // ---------- Scoring, celebration & visual feedback ----------
    // Everything that reacts to a placement: score/tray updates, the win
    // sweep, and the small coroutine-based animations (flash/shake/pulse/
    // float text) used across Input, Hints, and here. No external tween
    // package — see Assets/Scripts/UI/Ease.cs for the shared easing helpers.
    public partial class GameplayController
    {
        private void RegisterPlacement(int row, int col, string colorId)
        {
            placementsDone++;

            // Each box scores AT MOST ONCE per level (no remove/re-place farming)
            int key = row * puzzle.gridSize + col;
            bool firstTime = scoredCells.Add(key);
            if (!firstTime) return;

            int points = 96 * (5 + scoredCells.Count);
            score += points;
            if (scoreText != null) scoreText.text = "Score " + score;

            if (trayTokensByColor != null && trayTokensByColor.ContainsKey(colorId))
            {
                var tokenImage = trayTokensByColor[colorId];
                bool hasSprite = tokenImage.sprite != null && tokenImage.sprite != UiSprites.Circle;
                tokenImage.color = hasSprite ? Color.white : BaseColor(colorId);
            }
            SoundManager.Instance?.PlayTray();

            var view = board.GetCellView(row, col);
            StartCoroutine(FloatText(view.transform, "+" + points,
                new Color(1f, 0.6f, 0.1f), 56, Vector2.zero, new Vector2(0f, 140f)));

            ShowPraise(PraiseWords[(scoredCells.Count - 1) % PraiseWords.Length]);
        }

        private void ShowPraise(string word)
        {
            StartCoroutine(FloatText(canvas.transform, word,
                new Color(0.95f, 0.75f, 0.2f), 76, new Vector2(0f, 660f), new Vector2(0f, 90f)));
        }

        private void Celebrate()
        {
            string word = invalidAttempts == 0 ? "Exact!" : "Complete!";
            Debug.Log("LEVEL COMPLETE! " + word + " Score " + score);

            int bonus = 250 * puzzle.gridSize * config.rewardMultiplier;
            score += bonus;
            StartCoroutine(FloatText(canvas.transform, "+" + bonus + " BONUS",
                new Color(1f, 0.85f, 0.2f), 64, new Vector2(0f, 520f), new Vector2(0f, 80f), 1.6f));

            StartCoroutine(FloatText(canvas.transform, word,
                new Color(1f, 0.85f, 0.2f), 96, new Vector2(0f, 660f), new Vector2(0f, 60f), 1.4f));

            if (isBoss) SoundManager.Instance?.PlayBossWin();
            else SoundManager.Instance?.PlayWin();

            // Coins: bigger boards pay more, and a flawless ("Exact!") clear
            // earns a bonus — see ShopCatalog.cs for what they're spent on.
            int coinsEarned = puzzle.gridSize * 5 + (invalidAttempts == 0 ? 15 : 0);
            CoinSave.Add(coinsEarned);
            RefreshCoinText();
            StartCoroutine(FloatText(canvas.transform, "+" + coinsEarned + " coins",
                new Color(1f, 0.85f, 0.3f), 48, new Vector2(0f, 440f), new Vector2(0f, 60f), 1.3f));

            var solvedCells = new List<CellView>();
            for (int r = 0; r < puzzle.gridSize; r++)
            {
                for (int c = 0; c < puzzle.gridSize; c++)
                {
                    if (state.GetCell(r, c).state == CellState.Selected)
                    {
                        solvedCells.Add(board.GetCellView(r, c));
                    }
                }
            }

            // Sweep a gold tint + a small bounce across the solved cells one by
            // one instead of all at once, so the win reads as a celebration
            // rather than a snap-change.
            const float stagger = 0.06f;
            for (int i = 0; i < solvedCells.Count; i++)
            {
                solvedCells[i].SetRegionColor(new Color(1f, 0.85f, 0.3f));
                solvedCells[i].PlayWinBounce(i * stagger);
            }

            StartCoroutine(ShowCompleteOverlayDelayed(0.9f + solvedCells.Count * stagger));
        }

        private IEnumerator ShowCompleteOverlayDelayed(float delay = 0.9f)
        {
            yield return new WaitForSeconds(delay);
            BuildCompleteOverlay();
        }

        private void AutoSolve()
        {
            foreach (var pos in puzzle.solution)
            {
                var cell = state.GetCell(pos.row, pos.column);
                if (cell.state == CellState.Selected) continue;
                state.PlaceAnimal(pos.row, pos.column, animalIdByColor[cell.colorId]);
                RefreshCell(pos.row, pos.column);
                RegisterPlacement(pos.row, pos.column, cell.colorId);
            }
            EnsureTimerStarted();
            if (tutorialActive) EndTutorial();
            finished = true;
            Celebrate();
        }

        private void RefreshCell(int row, int col)
        {
            var cell = state.GetCell(row, col);
            var view = board.GetCellView(row, col);
            view.SetXVisible(cell.state == CellState.Eliminated);
            view.SetAnimalVisible(cell.state == CellState.Selected, BaseColor(cell.colorId));
        }

        private Color BaseColor(string colorId)
        {
            int index = puzzle.colors.IndexOf(colorId);
            if (index < 0) index = 0;
            return BoardView.Palette[index % BoardView.Palette.Length];
        }

        private IEnumerator InvalidFlash(CellView view)
        {
            Color original = view.background.color;
            view.SetRegionColor(new Color(0.9f, 0.3f, 0.3f));
            yield return new WaitForSeconds(0.25f);
            view.SetRegionColor(original);
        }

        private IEnumerator ShakeCell(CellView view)
        {
            var rect = view.transform as RectTransform;
            Vector2 original = rect.anchoredPosition;
            float t = 0f;
            const float duration = 0.3f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float offset = Mathf.Sin(t * 40f) * 8f * (1f - t / duration);
                rect.anchoredPosition = original + new Vector2(offset, 0f);
                yield return null;
            }
            rect.anchoredPosition = original;
        }

        private IEnumerator PulseCell(CellView view)
        {
            Color original = view.background.color;
            float t = 0f;
            const float duration = 2.5f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = (Mathf.Sin(t * 6f) + 1f) / 2f;
                view.SetRegionColor(Color.Lerp(original, Color.white, k * 0.8f));
                yield return null;
            }
            view.SetRegionColor(original);
        }

        private IEnumerator FloatText(Transform parent, string message, Color color,
            float size, Vector2 start, Vector2 rise, float duration = 0.9f)
        {
            var go = new GameObject("Popup", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchoredPosition = start;
            rect.sizeDelta = new Vector2(500f, 140f);
            var text = go.GetComponent<Text>();
            text.font = UiFonts.Default;
            text.fontSize = (int)size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = message;
            text.raycastTarget = false;

            float fadeStart = duration * 0.6f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                rect.anchoredPosition = start + rise * Mathf.Clamp01(t / duration);
                var c = text.color;
                c.a = t < fadeStart ? 1f : 1f - (t - fadeStart) / (duration - fadeStart);
                text.color = c;
                yield return null;
            }
            Destroy(go);
        }
    }
}
