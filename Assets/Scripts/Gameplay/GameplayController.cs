using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AnimalGrid.Core;
using AnimalGrid.Audio;
using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// The on-screen referee: strict correctness, lives, hints, boss intro,
    /// tutorial with pointing ring, campaign-aware advancement.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        private const float DoubleTapWindow = 0.3f;

        private enum LastAction { None, PlacedX, ClearedX, RemovedAnimal }

        private BoardView board;
        private PuzzleDefinition puzzle;
        private PuzzleState state;
        private Canvas canvas;
        private LevelConfig config;
        private CampaignState campaign;

        private int levelNumber = 1;
        private int worldIndex;
        private bool isBoss;
        private bool inputLocked;

        private Dictionary<string, string> animalIdByColor;
        private Dictionary<string, Image> trayTokensByColor;
        private Text scoreText;
        private Text hintCountText;
        private RectTransform trayRoot;
        private GameObject hintRoot;

        private readonly List<List<Image>> heartParts = new List<List<Image>>();
        private readonly HintProvider hintProvider = new HintProvider();

        private int lives = 3;
        private int hintsLeft = 3;
        private int placementsDone;
        private int invalidAttempts;
        private readonly HashSet<int> scoredCells = new HashSet<int>();
        private int score;
        private bool finished;

        private float startTime = -1f;
        private bool timerStarted;

        private int lastTapRow = -1;
        private int lastTapCol = -1;
        private float lastTapTime = -1f;
        private LastAction lastAction = LastAction.None;

        private bool tutorialActive;
        private int tutorialStep;
        private string[] tutorialSteps;
        private GameObject tutorialPanel;
        private Text tutorialText;
        private GameObject tutorialNextGo;
        private GameObject pointerGo;

        private static readonly Color HeartFull = new Color(0.90f, 0.25f, 0.35f);
        private static readonly Color HeartLost = new Color(0.6f, 0.6f, 0.6f, 0.35f);

        private static readonly string[] PraiseWords =
        {
            "Nice!", "Great!", "Perfect!", "Excellent!", "Amazing!", "Unreal!", "Brilliant!", "Superb!"
        };

        public void Initialize(BoardView board, PuzzleDefinition puzzle, int levelNumber,
            LevelConfig config, CampaignState campaign)
        {
            this.board = board;
            this.puzzle = puzzle;
            this.levelNumber = levelNumber;
            this.config = config;
            this.campaign = campaign;
            this.worldIndex = campaign.worldIndex;
            this.isBoss = config.isBoss;
            this.canvas = board.GetComponentInParent<Canvas>();

            state = new PuzzleState(puzzle.gridSize);
            for (int r = 0; r < puzzle.gridSize; r++)
            {
                for (int c = 0; c < puzzle.gridSize; c++)
                {
                    state.SetCellColor(r, c, puzzle.GetCellColor(r, c));
                }
            }

            animalIdByColor = new Dictionary<string, string>();
            foreach (var animal in puzzle.animals)
            {
                animalIdByColor[animal.colorName] = animal.id;
            }

            board.OnCellClicked = OnCellTapped;
            BuildHud();
            BuildHearts();
            BuildHintButton();

            if (isBoss)
            {
                inputLocked = true;
                BuildBossIntro();
            }
        }

        private void Update()
        {
            // DEBUG celebration previews: U = unlock, K = world complete, I = finale
            if (Input.GetKeyDown(KeyCode.U) && !finished)
            {
                pendingResult = new ClearResult();
                ShowUnlockOverlay(1);
            }
            if (Input.GetKeyDown(KeyCode.K) && !finished)
            {
                pendingResult = new ClearResult
                {
                    worldCompleted = true,
                    nextWorld = Mathf.Min(worldIndex + 1, Worlds.Count - 1)
                };
                ShowWorldOverlay();
            }
            if (Input.GetKeyDown(KeyCode.I) && !finished)
            {
                pendingResult = new ClearResult { worldCompleted = true, gameCompleted = true };
                ShowFinaleOverlay();
            }        }

        // ---------- Tutorial ----------

        public void StartTutorial()
        {
            tutorialActive = true;
            tutorialStep = 0;
            int n = puzzle.gridSize;
            tutorialSteps = new[]
            {
                "Welcome! Find " + n + " animals on this board.",
                "Each animal has its own color. One animal per color.",
                "One animal per row, and one per column.",
                "Animals cannot touch - not even diagonally.",
                "SINGLE tap a cell = X mark. It means 'impossible'.",
                "Try it! SINGLE tap the ringed cell to X it.",
                "DOUBLE tap the same cell quickly = place an animal there.",
                "Now DOUBLE tap the ringed cell - that is your first animal's true spot!"
            };
            BuildTutorialPanel();
            BuildPointer();
            ShowTutorialStep();
        }

        private void BuildTutorialPanel()
        {
            tutorialPanel = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image));
            tutorialPanel.transform.SetParent(canvas.transform, false);
            var rect = tutorialPanel.transform as RectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 330f);
            rect.sizeDelta = new Vector2(960f, 240f);
            tutorialPanel.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.95f);

            tutorialText = UiFactory.MakeText(tutorialPanel.transform, "", new Vector2(0f, 45f),
                new Vector2(880f, 130f), 40, new Color(0.25f, 0.2f, 0.15f));

            var next = UiFactory.MakeButton(tutorialPanel.transform, "Next", new Vector2(0f, -70f),
                new Vector2(260f, 100f), new Color(0.45f, 0.75f, 0.35f), Color.white);
            tutorialNextGo = next.gameObject;
            next.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                tutorialStep++;
                if (tutorialStep >= tutorialSteps.Length) EndTutorial();
                else ShowTutorialStep();
            });
        }

        private void BuildPointer()
        {
            pointerGo = new GameObject("TutorialPointer", typeof(RectTransform), typeof(Image));
            pointerGo.transform.SetParent(canvas.transform, false);
            var img = pointerGo.GetComponent<Image>();
            img.sprite = UiSprites.Ring;
            img.color = new Color(1f, 0.8f, 0.2f, 0.95f);
            img.raycastTarget = false;
            pointerGo.AddComponent<TutorialPointer>();
        }

        private void ShowTutorialStep()
        {
            tutorialText.text = tutorialSteps[tutorialStep];
            bool gated = tutorialStep == 5 || tutorialStep == 7;
            tutorialNextGo.SetActive(!gated);

            RectTransform target = null;
            if (tutorialStep == 1) target = trayRoot;
            else if (tutorialStep == 5)
            {
                int mid = puzzle.gridSize / 2;
                target = board.GetCellView(mid, mid).GetComponent<RectTransform>();
            }
            else if (tutorialStep == 7) target = FirstUnplacedSolutionCell();

            if (pointerGo != null)
            {
                pointerGo.SetActive(target != null);
                if (target != null) pointerGo.GetComponent<TutorialPointer>().target = target;
            }
        }

        private RectTransform FirstUnplacedSolutionCell()
        {
            foreach (var pos in puzzle.solution)
            {
                if (state.GetCell(pos.row, pos.column).state != CellState.Selected)
                {
                    return board.GetCellView(pos.row, pos.column).GetComponent<RectTransform>();
                }
            }
            return hintRoot.transform as RectTransform;
        }

        private void EndTutorial()
        {
            tutorialActive = false;
            CampaignSave.TutorialDone = true;
            if (tutorialPanel != null) Destroy(tutorialPanel);
            if (pointerGo != null) Destroy(pointerGo);
            ShowPraise("Great job!");
        }

        // ---------- Boss intro ----------

        private void BuildBossIntro()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.72f));
            UiFactory.MakeText(panel.transform, "CHALLENGE LEVEL", new Vector2(0f, 300f),
                new Vector2(950f, 160f), 84, new Color(0.95f, 0.3f, 0.3f));
            UiFactory.MakeText(panel.transform,
                "W" + (worldIndex + 1) + " - LEVEL " + levelNumber, new Vector2(0f, 160f),
                new Vector2(700f, 120f), 64, Color.white);
            UiFactory.MakeText(panel.transform,
                config.gridSize + "×" + config.gridSize + "  —  Find " + config.gridSize + " animals",
                new Vector2(0f, 60f), new Vector2(900f, 100f), 48, new Color(1f, 1f, 1f, 0.85f));

            var start = UiFactory.MakeButton(panel.transform, "Start", new Vector2(0f, -160f),
                new Vector2(420f, 130f), new Color(0.45f, 0.75f, 0.35f), Color.white);
            start.onClick.AddListener(() =>
            {
                inputLocked = false;
                Destroy(panel);
                SoundManager.Instance?.PlayButton();
            });
        }

        // ---------- Input ----------

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

        // ---------- Hints ----------

        private void UseHint()
        {
            if (finished || inputLocked || hintsLeft <= 0) return;
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

        // ---------- Lives ----------

        private void UpdateHearts()
        {
            for (int i = 0; i < heartParts.Count; i++)
            {
                Color color = i < lives ? HeartFull : HeartLost;
                foreach (var img in heartParts[i])
                {
                    img.color = color;
                }
            }
        }

        private void ShowFailOverlay()
        {
            finished = true;

            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.72f));
            UiFactory.MakeText(panel.transform, "Out of Lives!", new Vector2(0f, 200f),
                new Vector2(900f, 160f), 88, Color.white);
            UiFactory.MakeText(panel.transform, "The level restarts from the beginning.", new Vector2(0f, 60f),
                new Vector2(900f, 100f), 44, new Color(1f, 1f, 1f, 0.8f));

            var retry = UiFactory.MakeButton(panel.transform, "Try Again", new Vector2(0f, -160f),
                new Vector2(460f, 130f), new Color(0.95f, 0.8f, 0.3f), new Color(0.3f, 0.2f, 0.05f));
            retry.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                ReloadScene();
            });
        }

        // ---------- Scoring & celebration ----------

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

            for (int r = 0; r < puzzle.gridSize; r++)
            {
                for (int c = 0; c < puzzle.gridSize; c++)
                {
                    if (state.GetCell(r, c).state == CellState.Selected)
                    {
                        board.GetCellView(r, c).SetRegionColor(new Color(1f, 0.85f, 0.3f));
                    }
                }
            }

            StartCoroutine(ShowCompleteOverlayDelayed());
        }

        private IEnumerator ShowCompleteOverlayDelayed()
        {
            yield return new WaitForSeconds(0.9f);
            BuildCompleteOverlay();
        }

        private void BuildCompleteOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.72f));

            string title = isBoss ? "CHALLENGE CLEARED!" : "Level Complete!";
            UiFactory.MakeText(panel.transform, title, new Vector2(0f, 430f),
                new Vector2(950f, 160f), 84, new Color(1f, 0.85f, 0.2f));

            string sub = invalidAttempts == 0
                ? "Exact! Every placement was perfect!"
                : "Well played!";
            UiFactory.MakeText(panel.transform, sub, new Vector2(0f, 300f),
                new Vector2(900f, 100f), 44, Color.white);

            string stats = "Time   " + FormatTime()
                         + "\nHints Used   " + (3 - hintsLeft)
                         + "\nScore   " + score;
            UiFactory.MakeText(panel.transform, stats, new Vector2(0f, 60f),
                new Vector2(700f, 280f), 52, new Color(1f, 1f, 1f, 0.92f));

            var next = UiFactory.MakeButton(panel.transform, "Next Level", new Vector2(0f, -190f),
                new Vector2(460f, 130f), new Color(0.45f, 0.75f, 0.35f), Color.white);
            next.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                AdvanceCampaign();
            });

            var replay = UiFactory.MakeButton(panel.transform, "Replay", new Vector2(0f, -350f),
                new Vector2(460f, 130f), new Color(0.87f, 0.78f, 0.62f), new Color(0.3f, 0.2f, 0.1f));
            replay.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                ReloadScene();
            });
        }

        /// <summary>
        /// The bar fills HERE: when the player moves on. Boss fills twice.
        /// Replay / Try Again never record.
        /// </summary>
        private ClearResult pendingResult;

        private void AdvanceCampaign()
        {
            var result = campaign.RecordLevelClear(isBoss);
            CampaignSave.Save(campaign);
            pendingResult = result;
            Debug.Log("ADVANCE -> World " + (result.nextWorld + 1) + " Level " + result.nextLevel
                + (result.worldCompleted ? " [WORLD COMPLETE]" : "")
                + (result.gameCompleted ? " [GAME COMPLETE]" : ""));

            if (result.newlyUnlocked.Count > 0) ShowUnlockOverlay(result.newlyUnlocked[0]);
            else if (result.gameCompleted) ShowFinaleOverlay();
            else if (result.worldCompleted) ShowWorldOverlay();
            else FinishAdvance();
        }

        private void FinishAdvance()
        {
            GameplayBootstrap.ReturnToGame = pendingResult != null && !pendingResult.gameCompleted;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ContinueAfterCelebration()
        {
            if (pendingResult != null && pendingResult.gameCompleted) { ShowFinaleOverlay(); return; }
            if (pendingResult != null && pendingResult.worldCompleted) { ShowWorldOverlay(); return; }
            FinishAdvance();
        }

        // ---------- Celebrations ----------

        private void ShowUnlockOverlay(int animalIndex)
        {
            var world = Worlds.Get(worldIndex);
            string id = world.animalIds[animalIndex];
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.78f));

            UiFactory.MakeText(panel.transform, "New pet unlocked!", new Vector2(0f, 430f),
                new Vector2(900f, 140f), 80, new Color(1f, 0.85f, 0.2f));

            var artGo = new GameObject("Art", typeof(RectTransform), typeof(Image));
            artGo.transform.SetParent(panel.transform, false);
            var aRect = artGo.transform as RectTransform;
            aRect.anchorMin = aRect.anchorMax = new Vector2(0.5f, 0.5f);
            aRect.anchoredPosition = new Vector2(0f, 90f);
            aRect.sizeDelta = new Vector2(380f, 380f);
            var aImg = artGo.GetComponent<Image>();
            var sprite = ArtLoader.GetAnimal(id);
            aImg.sprite = sprite != null ? sprite : UiSprites.Circle;
            aImg.preserveAspect = true;
            aImg.raycastTarget = false;

            UiFactory.MakeText(panel.transform, Worlds.DisplayName(id), new Vector2(0f, -160f),
                new Vector2(600f, 100f), 64, Color.white);
            UiFactory.MakeText(panel.transform, "Added to your collection!", new Vector2(0f, -260f),
                new Vector2(700f, 80f), 42, new Color(1f, 1f, 1f, 0.85f));

            var btn = UiFactory.MakeButton(panel.transform, "Continue", new Vector2(0f, -420f),
                new Vector2(420f, 120f), new Color(0.45f, 0.75f, 0.35f), Color.white);
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                Destroy(panel);
                ContinueAfterCelebration();
            });
        }

        private void ShowWorldOverlay()
        {
            int doneIndex = worldIndex;
            if (pendingResult != null && !pendingResult.gameCompleted) doneIndex = pendingResult.nextWorld - 1;
            var done = Worlds.Get(doneIndex);
            var next = Worlds.Get(pendingResult != null ? pendingResult.nextWorld : worldIndex);

            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.8f));
            UiFactory.MakeText(panel.transform, "WORLD COMPLETE!", new Vector2(0f, 380f),
                new Vector2(950f, 150f), 84, new Color(1f, 0.85f, 0.2f));
            UiFactory.MakeText(panel.transform, done.name + " - all 10 animals discovered!",
                new Vector2(0f, 240f), new Vector2(900f, 90f), 46, Color.white);
            UiFactory.MakeText(panel.transform, "A new world awaits...", new Vector2(0f, 140f),
                new Vector2(700f, 80f), 42, new Color(1f, 1f, 1f, 0.8f));

            var btn = UiFactory.MakeButton(panel.transform, "Enter " + next.name, new Vector2(0f, -300f),
                new Vector2(520f, 130f), new Color(0.45f, 0.75f, 0.35f), Color.white);
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                FinishAdvance();
            });
        }

        private void ShowFinaleOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.85f));
            UiFactory.MakeText(panel.transform, "YOU DID IT!", new Vector2(0f, 420f),
                new Vector2(950f, 160f), 90, new Color(1f, 0.85f, 0.2f));
            UiFactory.MakeText(panel.transform, "All 3 worlds complete - 30 animals collected!",
                new Vector2(0f, 280f), new Vector2(950f, 90f), 46, Color.white);
            UiFactory.MakeText(panel.transform, "Thank you for playing. More worlds coming soon!",
                new Vector2(0f, 180f), new Vector2(950f, 80f), 40, new Color(1f, 1f, 1f, 0.8f));

            var btn = UiFactory.MakeButton(panel.transform, "Back to Home", new Vector2(0f, -300f),
                new Vector2(460f, 130f), new Color(0.87f, 0.78f, 0.62f), new Color(0.3f, 0.2f, 0.1f));
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                GameplayBootstrap.ReturnToGame = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
        private void ReloadScene()
        {
            GameplayBootstrap.ReturnToGame = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private string FormatTime()
        {
            float t = timerStarted ? Time.time - startTime : 0f;
            int seconds = Mathf.FloorToInt(t);
            return string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
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

        // ---------- Visual feedback ----------

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

        // ---------- HUD ----------

        private void BuildHud()
        {
            var levelGo = new GameObject("LevelText", typeof(RectTransform), typeof(Text));
            levelGo.transform.SetParent(canvas.transform, false);
            var lRect = levelGo.transform as RectTransform;
            lRect.anchorMin = new Vector2(0.5f, 1f);
            lRect.anchorMax = new Vector2(0.5f, 1f);
            lRect.anchoredPosition = new Vector2(-260f, -90f);
            lRect.sizeDelta = new Vector2(420f, 110f);
            var levelText = levelGo.GetComponent<Text>();
            levelText.font = UiFonts.Default;
            levelText.fontSize = isBoss ? 44 : 52;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.color = isBoss ? new Color(0.85f, 0.2f, 0.25f) : new Color(0.35f, 0.2f, 0.25f);
            levelText.text = isBoss
                ? "W" + (worldIndex + 1) + " BOSS " + levelNumber
                : "W" + (worldIndex + 1) + " · L" + levelNumber;
            levelText.raycastTarget = false;

            var scoreGo = new GameObject("ScoreText", typeof(RectTransform), typeof(Text));
            scoreGo.transform.SetParent(canvas.transform, false);
            var sRect = scoreGo.transform as RectTransform;
            sRect.anchorMin = new Vector2(0.5f, 1f);
            sRect.anchorMax = new Vector2(0.5f, 1f);
            sRect.anchoredPosition = new Vector2(180f, -90f);
            sRect.sizeDelta = new Vector2(460f, 110f);
            scoreText = scoreGo.GetComponent<Text>();
            scoreText.font = UiFonts.Default;
            scoreText.fontSize = 56;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.color = new Color(0.35f, 0.2f, 0.25f);

            // Unlock progress bar (HUD)
            int pts = campaign.worldPoints[worldIndex];
            int unlockedCount = UnlockProgression.UnlockedCount(pts);
            var barWorld = Worlds.Get(worldIndex);
            float fraction = UnlockProgression.BarFraction(pts);
            string barLabel;
            if (unlockedCount >= barWorld.animalIds.Length)
            {
                barLabel = barWorld.name + " collection complete!";
                fraction = 1f;
            }
            else
            {
                int nextTh = UnlockProgression.Thresholds[unlockedCount - 1];
                barLabel = "Next: " + Worlds.DisplayName(barWorld.animalIds[unlockedCount])
                    + "  ·  " + pts + "/" + nextTh;
            }
            UiFactory.MakeUnlockBar(canvas.transform, new Vector2(0.5f, 1f),
                new Vector2(-140f, -235f), fraction, barLabel, Color.white);

            var tray = new GameObject("Tray", typeof(RectTransform));
            tray.transform.SetParent(canvas.transform, false);
            trayRoot = tray.transform as RectTransform;
            trayRoot.anchorMin = trayRoot.anchorMax = new Vector2(0.5f, 0.5f);
            trayRoot.anchoredPosition = new Vector2(0f, 560f);

            int n = puzzle.gridSize;
            float tokenSize = 70f;
            float gap = 26f;
            float total = n * tokenSize + (n - 1) * gap;
            trayTokensByColor = new Dictionary<string, Image>();

            for (int i = 0; i < n; i++)
            {
                string colorId = puzzle.colors[i];
                var token = new GameObject("Token_" + colorId, typeof(RectTransform), typeof(Image));
                token.transform.SetParent(tray.transform, false);
                var rect = token.transform as RectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                float x = -total / 2f + tokenSize / 2f + i * (tokenSize + gap);
                rect.anchoredPosition = new Vector2(x, 0f);
                rect.sizeDelta = new Vector2(tokenSize, tokenSize);
                var img = token.GetComponent<Image>();
                img.preserveAspect = true;
                var animalSprite = ArtLoader.GetAnimal(animalIdByColor[colorId]);
                img.sprite = animalSprite != null ? animalSprite : UiSprites.Circle;
                if (animalSprite != null)
                {
                    img.color = new Color(1f, 1f, 1f, 0.35f);
                }
                else
                {
                    var faded = BaseColor(colorId);
                    faded.a = 0.25f;
                    img.color = faded;
                }
                img.raycastTarget = false;
                trayTokensByColor[colorId] = img;
            }
        }

        private void BuildHearts()
        {
            var root = new GameObject("Hearts", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rect = root.transform as RectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-160f, -220f);

            for (int i = 0; i < 3; i++)
            {
                var heart = new GameObject("Heart_" + i, typeof(RectTransform));
                heart.transform.SetParent(root.transform, false);
                var hRect = heart.transform as RectTransform;
                hRect.anchorMin = hRect.anchorMax = new Vector2(0.5f, 0.5f);
                hRect.anchoredPosition = new Vector2(-i * 90f, 0f);
                hRect.sizeDelta = new Vector2(70f, 70f);
                heartParts.Add(MakeHeart(heart.transform));
            }
            UpdateHearts();
        }

        private List<Image> MakeHeart(Transform parent)
        {
            var imgs = new List<Image>();
            imgs.Add(MakeHeartPart(parent, new Vector2(-15f, 12f), 38f, 0f));
            imgs.Add(MakeHeartPart(parent, new Vector2(15f, 12f), 38f, 0f));
            imgs.Add(MakeHeartPart(parent, new Vector2(0f, -8f), 52f, 45f));
            return imgs;
        }

        private Image MakeHeartPart(Transform parent, Vector2 pos, float size, float angle)
        {
            var go = new GameObject("Part", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(size, size);
            rect.localEulerAngles = new Vector3(0f, 0f, angle);
            var img = go.GetComponent<Image>();
            img.sprite = angle > 0f ? UiSprites.White : UiSprites.Circle;
            img.color = HeartFull;
            img.raycastTarget = false;
            return img;
        }

        private void BuildHintButton()
        {
            hintRoot = new GameObject("HintButton", typeof(RectTransform), typeof(Image), typeof(Button));
            hintRoot.transform.SetParent(canvas.transform, false);
            var rect = hintRoot.transform as RectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 120f);
            rect.sizeDelta = new Vector2(140f, 140f);
            var img = hintRoot.GetComponent<Image>();
            img.sprite = UiSprites.Circle;
            img.color = new Color(0.98f, 0.85f, 0.35f);

            var countGo = new GameObject("Count", typeof(RectTransform), typeof(Text));
            countGo.transform.SetParent(hintRoot.transform, false);
            var cRect = countGo.transform as RectTransform;
            cRect.anchorMin = cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.anchoredPosition = Vector2.zero;
            cRect.sizeDelta = new Vector2(120f, 120f);
            hintCountText = countGo.GetComponent<Text>();
            hintCountText.font = UiFonts.Default;
            hintCountText.fontSize = 64;
            hintCountText.alignment = TextAnchor.MiddleCenter;
            hintCountText.color = new Color(0.35f, 0.25f, 0.05f);
            hintCountText.text = hintsLeft.ToString();
            hintCountText.raycastTarget = false;

            hintRoot.GetComponent<Button>().onClick.AddListener(UseHint);
        }
    }
}