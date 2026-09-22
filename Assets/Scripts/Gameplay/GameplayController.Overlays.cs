using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AnimalGrid.Core;
using AnimalGrid.Audio;
using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    // ---------- Overlays ----------
    // Full-screen panels shown at specific moments: boss intro, out-of-lives,
    // level-complete, animal unlock, world-complete, and the finale. All of
    // these Destroy() themselves or hand off via SceneManager, same as before.
    public partial class GameplayController
    {
        private void BuildBossIntro()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.72f));
            AddHomeButton(panel.transform, new Vector2(-445f, 870f));
            UiFactory.MakeText(panel.transform, "CHALLENGE LEVEL", new Vector2(0f, 300f),
                new Vector2(950f, 160f), 84, new Color(0.95f, 0.3f, 0.3f));
            UiFactory.MakeText(panel.transform,
                "W" + (worldIndex + 1) + " - LEVEL " + levelNumber, new Vector2(0f, 160f),
                new Vector2(700f, 120f), 64, Color.white);
            UiFactory.MakeText(panel.transform,
                config.gridSize + "×" + config.gridSize + "  —  Find " + config.gridSize + " animals",
                new Vector2(0f, 60f), new Vector2(900f, 100f), 48, new Color(1f, 1f, 1f, 0.85f));

            var start = UiFactory.MakeButton(panel.transform, "Start", new Vector2(0f, -160f),
                new Vector2(420f, 130f), new Color(0.45f, 0.75f, 0.35f), Color.white, artKey: "button_gold");
            start.onClick.AddListener(() =>
            {
                inputLocked = false;
                Destroy(panel);
                SoundManager.Instance?.PlayButton();
            });
        }

        private void ShowFailOverlay()
        {
            finished = true;

            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.72f));
            AddHomeButton(panel.transform, new Vector2(-445f, 870f));
            UiFactory.MakeText(panel.transform, "Out of Lives!", new Vector2(0f, 260f),
                new Vector2(900f, 160f), 88, Color.white);
            UiFactory.MakeText(panel.transform, "Spend coins to keep going, or restart the level.",
                new Vector2(0f, 120f), new Vector2(900f, 100f), 40, new Color(1f, 1f, 1f, 0.8f));

            var revive = UiFactory.MakeButton(panel.transform, "Continue — " + ShopCatalog.ExtraLifeCost + " coins",
                new Vector2(0f, -40f), new Vector2(580f, 130f), new Color(0.95f, 0.8f, 0.3f), new Color(0.3f, 0.2f, 0.05f));
            revive.onClick.AddListener(() =>
            {
                if (ShopCatalog.TryBuyExtraLife())
                {
                    SoundManager.Instance?.PlayButton();
                    lives = 1;
                    UpdateHearts();
                    RefreshCoinText();
                    finished = false;
                    Destroy(panel);
                }
                else
                {
                    SoundManager.Instance?.PlayInvalid();
                    StartCoroutine(FloatText(panel.transform, "Not enough coins",
                        new Color(1f, 0.4f, 0.4f), 40, new Vector2(0f, -40f), new Vector2(0f, 60f)));
                }
            });

            var retry = UiFactory.MakeButton(panel.transform, "Restart Level", new Vector2(0f, -210f),
                new Vector2(460f, 120f), new Color(0.87f, 0.78f, 0.62f), new Color(0.3f, 0.2f, 0.1f));
            retry.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                ReloadScene();
            });
        }

        /// <summary>
        /// Shown instead of a no-op when the hint button is tapped with both
        /// the free per-level hints and the bonus bank empty. Buying here
        /// immediately spends one of the newly bought hints via UseHint().
        /// </summary>
        private void ShowBuyHintPrompt()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.72f));
            UiFactory.MakeText(panel.transform, "Out of hints!", new Vector2(0f, 240f),
                new Vector2(700f, 120f), 64, Color.white);
            UiFactory.MakeText(panel.transform, "Coins: " + CoinSave.Balance, new Vector2(0f, 130f),
                new Vector2(500f, 80f), 40, new Color(1f, 0.85f, 0.3f));

            var buy = UiFactory.MakeButton(panel.transform,
                "+" + ShopCatalog.HintPackHints + " Hints — " + ShopCatalog.HintPackCost + " coins",
                new Vector2(0f, -20f), new Vector2(600f, 130f), new Color(0.45f, 0.75f, 0.35f), Color.white, artKey: "button_gold");
            buy.onClick.AddListener(() =>
            {
                if (ShopCatalog.TryBuyHintPack())
                {
                    SoundManager.Instance?.PlayButton();
                    Destroy(panel);
                    RefreshCoinText();
                    UseHint(); // spend one of the newly bought hints right away
                }
                else
                {
                    SoundManager.Instance?.PlayInvalid();
                    StartCoroutine(FloatText(panel.transform, "Not enough coins",
                        new Color(1f, 0.4f, 0.4f), 40, new Vector2(0f, -20f), new Vector2(0f, 60f)));
                }
            });

            var cancel = UiFactory.MakeButton(panel.transform, "Not now", new Vector2(0f, -190f),
                new Vector2(420f, 110f), new Color(0.87f, 0.78f, 0.62f), new Color(0.3f, 0.2f, 0.1f));
            cancel.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                Destroy(panel);
            });
        }

        private void BuildCompleteOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.72f));
            AddHomeButton(panel.transform, new Vector2(-445f, 870f));

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
                new Vector2(460f, 130f), new Color(0.45f, 0.75f, 0.35f), Color.white, artKey: "button_gold");
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

        private void ShowUnlockOverlay(int animalIndex)
        {
            var world = Worlds.Get(worldIndex);
            string id = world.animalIds[animalIndex];
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.78f));
            SoundManager.Instance?.PlayUnlock();

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
                new Vector2(420f, 120f), new Color(0.45f, 0.75f, 0.35f), Color.white, artKey: "button_gold");
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
            SoundManager.Instance?.PlaySting("world");

            UiFactory.MakeText(panel.transform, "WORLD COMPLETE!", new Vector2(0f, 380f),
                new Vector2(950f, 150f), 84, new Color(1f, 0.85f, 0.2f));
            UiFactory.MakeText(panel.transform, done.name + " - all 10 animals discovered!",
                new Vector2(0f, 240f), new Vector2(900f, 90f), 46, Color.white);
            UiFactory.MakeText(panel.transform, "A new world awaits...", new Vector2(0f, 140f),
                new Vector2(700f, 80f), 42, new Color(1f, 1f, 1f, 0.8f));

            var btn = UiFactory.MakeButton(panel.transform, "Enter " + next.name, new Vector2(0f, -300f),
                new Vector2(520f, 130f), new Color(0.45f, 0.75f, 0.35f), Color.white, artKey: "button_gold");
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                FinishAdvance();
            });
        }

        private void ShowFinaleOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.85f));
            SoundManager.Instance?.PlaySting("world");

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

        private string FormatTime()
        {
            float t = timerStarted ? Time.time - startTime : 0f;
            int seconds = Mathf.FloorToInt(t);
            return string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
        }
    }
}
