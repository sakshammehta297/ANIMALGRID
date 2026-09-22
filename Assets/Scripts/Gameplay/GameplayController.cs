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
    /// tutorial with pointing ring, campaign-aware advancement, celebrations,
    /// Home escape, and full audio hooks.
    /// DEBUG keys: S auto-solve | U/K/I celebration previews | P/M campaign fast-forward.
    ///
    /// This class is split across several files by responsibility (all still
    /// the same `GameplayController` type via `partial` — no behavior change,
    /// just organization, so nothing outside this class needed to change):
    ///   GameplayController.cs          - fields, Initialize, Update, shared helpers (this file)
    ///   GameplayController.Input.cs    - tap handling / placement rules
    ///   GameplayController.Hints.cs    - hint button
    ///   GameplayController.Hud.cs      - always-on-screen HUD (score, hearts, tray)
    ///   GameplayController.Overlays.cs - full-screen panels (boss intro, fail, complete, unlock, world, finale)
    ///   GameplayController.Campaign.cs - post-level advancement / scene transitions
    ///   GameplayController.Fx.cs       - scoring, win celebration, flash/shake/pulse/float-text coroutines
    ///   GameplayController.Tutorial.cs - the onboarding tutorial flow
    /// </summary>
    public partial class GameplayController : MonoBehaviour
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
        private Text coinText;
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

        private ClearResult pendingResult;

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
            // DEBUG: auto-solve
            if (Input.GetKeyDown(KeyCode.S) && !finished && !inputLocked)
            {
                AutoSolve();
            }
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
            }
            // DEBUG campaign fast-forward: P = +10 bar points, M = bar almost full (179)
            if (Input.GetKeyDown(KeyCode.P) && !finished)
            {
                campaign.worldPoints[worldIndex] =
                    Mathf.Min(campaign.worldPoints[worldIndex] + 10, UnlockProgression.MaxPoints);
                CampaignSave.Save(campaign);
                Debug.Log("DEBUG points -> " + campaign.worldPoints[worldIndex]);
            }
            if (Input.GetKeyDown(KeyCode.M) && !finished)
            {
                campaign.worldPoints[worldIndex] = UnlockProgression.MaxPoints - 1;
                CampaignSave.Save(campaign);
                Debug.Log("DEBUG points -> 179 (win + Next to complete the world)");
            }
        }

        // ---------- Home escape ----------

        private void AddHomeButton(Transform parent, Vector2 pos)
        {
            var go = new GameObject("HomeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(170f, 90f);
            var img = go.GetComponent<Image>();
            img.sprite = UiSprites.RoundedSquare;
            img.color = new Color(0.8f, 0.75f, 0.7f, 0.95f);
            UiFactory.MakeText(go.transform, "Home", Vector2.zero,
                new Vector2(160f, 80f), 40, new Color(0.35f, 0.25f, 0.2f));
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                GameplayBootstrap.ReturnToGame = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
    }
}
