using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using AnimalGrid.Core;
using AnimalGrid.Audio;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Entry point: static background + optional looping video behind Home,
    /// Home on cold start, or straight into the level when returning.
    /// UI always floats above the video; the video never blocks touches.
    /// </summary>
    public class GameplayBootstrap : MonoBehaviour
    {
        public static bool ReturnToGame;

        [Tooltip("Optional: drag your Canvas here. If empty, it is found by name.")]
        public Canvas canvas;

        [Tooltip("0 = use saved progress. Set a number to test a specific level.")]
        public int overrideLevelNumber = 0;

        [Tooltip("Optional looping video played behind the Home screen UI.")]
        public VideoClip homeVideo;

        private VideoPlayer videoPlayer;
        private RenderTexture videoRT;
        private GameObject videoHost;
        private GameObject videoImageGo;

        private void Start()
        {
            if (canvas == null)
            {
                GameObject found = GameObject.Find("Canvas");
                if (found != null) canvas = found.GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                Debug.LogError("No Canvas in scene! Create one: GameObject > UI > Canvas");
                return;
            }

            var soundGo = new GameObject("SoundManager");
            soundGo.AddComponent<SoundManager>();

            BuildBackground();

            if (ReturnToGame)
            {
                ReturnToGame = false;
                StartLevel();
            }
            else
            {
                BuildVideoBackground();
                BuildHomeOverlay();
            }
        }

        private void OnDestroy()
        {
            ReleaseVideo();
        }

        // ---------- Optional looping video background ----------

        private void BuildVideoBackground()
        {
            if (homeVideo == null) return;

            videoRT = new RenderTexture(720, 1280, 0, RenderTextureFormat.ARGB32);

            videoHost = new GameObject("HomeVideoPlayer");
            videoPlayer = videoHost.AddComponent<VideoPlayer>();
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = homeVideo;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoRT;
            videoPlayer.isLooping = true;
            videoPlayer.playOnAwake = false;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
            videoPlayer.Play();

            videoImageGo = new GameObject("VideoBackground", typeof(RectTransform), typeof(RawImage));
            videoImageGo.transform.SetParent(canvas.transform, false);
            var rect = videoImageGo.transform as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = videoImageGo.GetComponent<RawImage>();
            img.texture = videoRT;
            img.raycastTarget = false;
        }

        private void ReleaseVideo()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                Destroy(videoPlayer);
                videoPlayer = null;
            }
            if (videoHost != null)
            {
                Destroy(videoHost);
                videoHost = null;
            }
            if (videoImageGo != null)
            {
                Destroy(videoImageGo);
                videoImageGo = null;
            }
            if (videoRT != null)
            {
                videoRT.Release();
                Destroy(videoRT);
                videoRT = null;
            }
        }

        // ---------- Static background (fallback layer under the video) ----------

        private void BuildBackground()
        {
            var go = new GameObject("Background", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsFirstSibling();
            var rect = go.transform as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            var sprite = ArtLoader.Get("background");
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.97f, 0.94f, 0.89f);
            }
            img.raycastTarget = false;
        }

        // ---------- Home screen (spec 26) ----------

        private void BuildHomeOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0f));
            panel.GetComponent<Image>().raycastTarget = false;

            UiFactory.MakeText(panel.transform, "ANIMAL GRID", new Vector2(0f, 620f),
                new Vector2(950f, 200f), 100, new Color(0.35f, 0.22f, 0.15f));
            UiFactory.MakeText(panel.transform, "Think - Place - Complete", new Vector2(0f, 480f),
                new Vector2(800f, 100f), 46, new Color(0.55f, 0.4f, 0.3f));

            var mascot = ArtLoader.Get("mascot");
            if (mascot != null)
            {
                var mGo = new GameObject("Mascot", typeof(RectTransform), typeof(Image));
                mGo.transform.SetParent(panel.transform, false);
                var mRect = mGo.transform as RectTransform;
                mRect.anchorMin = mRect.anchorMax = new Vector2(0.5f, 0.5f);
                mRect.anchoredPosition = new Vector2(0f, 120f);
                mRect.sizeDelta = new Vector2(380f, 380f);
                var mImg = mGo.GetComponent<Image>();
                mImg.sprite = mascot;
                mImg.preserveAspect = true;
                mImg.raycastTarget = false;
            }

            int level = ProgressionSave.CurrentLevel;
            string label = (level == 1 && !ProgressionSave.TutorialDone)
                ? "Start - Level 1"
                : "Continue - Level " + level;

            var continueBtn = UiFactory.MakeButton(panel.transform, label, new Vector2(0f, -220f),
                new Vector2(640f, 150f), new Color(0.45f, 0.75f, 0.35f), Color.white);
            continueBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                Destroy(panel);
                StartLevel();
            });

            UiFactory.MakeText(panel.transform, (level - 1) + " levels completed",
                new Vector2(0f, -360f), new Vector2(600f, 80f), 40, new Color(0.5f, 0.4f, 0.32f));

            var reset = UiFactory.MakeButton(panel.transform, "Reset Progress", new Vector2(0f, -820f),
                new Vector2(380f, 90f), new Color(0.8f, 0.75f, 0.7f), new Color(0.35f, 0.25f, 0.2f));
            reset.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                ProgressionSave.CurrentLevel = 1;
                ProgressionSave.TutorialDone = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }

        // ---------- Level start ----------

        private void StartLevel()
        {
            ReleaseVideo();

            int levelNumber = overrideLevelNumber > 0 ? overrideLevelNumber : ProgressionSave.CurrentLevel;
            LevelConfig config = ProgressionConfig.GetLevel(levelNumber);

            var boardGo = new GameObject("Board", typeof(RectTransform));
            boardGo.transform.SetParent(canvas.transform, false);
            var board = boardGo.AddComponent<BoardView>();

            var generator = new PuzzleGenerator();
            var puzzle = generator.Generate(new GenerationSettings
            {
                levelId = levelNumber,
                gridSize = config.gridSize,
                isBoss = config.isBoss,
                difficulty = config.difficultyTarget,
                randomSeed = levelNumber * 7919 + 17,
                animalIds = AnimalRoster.IdsFor(config.gridSize)
            });

            if (puzzle == null)
            {
                Debug.LogError("Puzzle generation failed!");
                return;
            }

            board.RenderBoard(puzzle);

            var controller = boardGo.AddComponent<GameplayController>();
            controller.Initialize(board, puzzle, levelNumber, config);

            if (levelNumber == 1 && !ProgressionSave.TutorialDone)
            {
                controller.StartTutorial();
            }
        }
    }
}