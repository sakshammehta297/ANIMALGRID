using UnityEngine;
using AnimalGrid.Core;
using AnimalGrid.Audio;

namespace AnimalGrid.Gameplay
{
    // ---------- Input ----------
    // Handles single/double-tap on a cell: X-marking, placing/removing an
    // animal, the double-tap-to-place gesture, and correctness checking.
    public partial class GameplayController
    {
        private void OnCellTapped(int row, int col)
        {
            if (finished || inputLocked) return;
            var cell = state.GetCell(row, col);
            if (cell == null) return;

            bool isDouble = lastTapRow == row && lastTapCol == col &&
                            lastTapTime >= 0f && (Time.time - lastTapTime) <= DoubleTapWindow;

            if (isDouble)
            {
                lastTapTime = -1f;
                RevertLastSingleAction(row, col);
                HandlePlaceIntent(row, col);
                return;
            }

            lastTapRow = row;
            lastTapCol = col;
            lastTapTime = Time.time;

            if (cell.state == CellState.Selected)
            {
                state.ClearCell(row, col);
                RefreshCell(row, col);
                lastAction = LastAction.RemovedAnimal;
                SoundManager.Instance?.PlayTap();
                return;
            }

            if (cell.state == CellState.Empty)
            {
                state.MarkEliminated(row, col);
                RefreshCell(row, col);
                lastAction = LastAction.PlacedX;
                SoundManager.Instance?.PlayX();
                EnsureTimerStarted();
                if (tutorialActive && tutorialStep == 5)
                {
                    tutorialStep = 6;
                    ShowTutorialStep();
                }
                return;
            }

            state.ClearCell(row, col);
            RefreshCell(row, col);
            lastAction = LastAction.ClearedX;
            SoundManager.Instance?.PlayTap();
        }

        private void RevertLastSingleAction(int row, int col)
        {
            switch (lastAction)
            {
                case LastAction.PlacedX:
                    state.ClearCell(row, col);
                    break;
                case LastAction.ClearedX:
                    state.MarkEliminated(row, col);
                    break;
                case LastAction.RemovedAnimal:
                    state.PlaceAnimal(row, col, animalIdByColor[state.GetCell(row, col).colorId]);
                    break;
            }
            lastAction = LastAction.None;
            RefreshCell(row, col);
        }

        private void HandlePlaceIntent(int row, int col)
        {
            var cell = state.GetCell(row, col);

            if (cell.state == CellState.Selected)
            {
                state.ClearCell(row, col);
                RefreshCell(row, col);
                SoundManager.Instance?.PlayTap();
                return;
            }

            if (!IsCorrectCell(row, col))
            {
                invalidAttempts++;
                if (!tutorialActive)
                {
                    lives--;
                    SoundManager.Instance?.PlayHeart();
                    UpdateHearts();
                }

                var wrongView = board.GetCellView(row, col);
                StartCoroutine(InvalidFlash(wrongView));
                StartCoroutine(ShakeCell(wrongView));
                StartCoroutine(FloatText(wrongView.transform, "Oops!",
                    new Color(0.85f, 0.2f, 0.2f), 48, Vector2.zero, new Vector2(0f, 100f)));
                SoundManager.Instance?.PlayInvalid();

                if (lives <= 0)
                {
                    ShowFailOverlay();
                }
                return;
            }

            string colorId = cell.colorId;
            state.PlaceAnimal(row, col, animalIdByColor[colorId]);
            RefreshCell(row, col);
            RegisterPlacement(row, col, colorId);
            EnsureTimerStarted();
            SoundManager.Instance?.PlayPlace();

            if (tutorialActive && tutorialStep == 7)
            {
                EndTutorial();
            }

            if (state.IsComplete())
            {
                finished = true;
                Celebrate();
            }
        }

        private bool IsCorrectCell(int row, int col)
        {
            foreach (var pos in puzzle.solution)
            {
                if (pos.row == row && pos.column == col) return true;
            }
            return false;
        }

        private void EnsureTimerStarted()
        {
            if (!timerStarted)
            {
                timerStarted = true;
                startTime = Time.time;
            }
        }
    }
}
