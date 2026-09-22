using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using AnimalGrid.Core;
using AnimalGrid.Audio;
using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    /// <summary>
    /// Entry point: home (static bg) + collection + settings + coming-soon + level start (game bg).
    /// </summary>
    public class GameplayBootstrap : MonoBehaviour
    {
        public static bool ReturnToGame;

        [Tooltip("Optional: drag your Canvas here. If empty, it is found by name.")]
        public Canvas canvas;

        [Tooltip("0 = use saved campaign. Set a number to test a specific level of the current world.")]
        public int overrideLevelNumber = 0;

        [Tooltip("Optional looping video played behind the Home screen UI (off by default).")]
        public VideoClip homeVideo;

        private CampaignState campaign;
        private Image backgroundImage;
        private VideoPlayer videoPlayer;
        private RenderTexture videoRT;
        private GameObject videoHost;
        private GameObject videoImageGo;

        private static readonly Color HomeCream = new Color(0.97f, 0.94f, 0.89f);
        private static readonly Color GameCream = new Color(0.94f, 0.92f, 0.88f);

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
            SoundManager.ApplySettings();

            campaign = CampaignSave.Load();

            BuildBackground();
            SetBackground("background", HomeCream);

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
                SoundManager.Instance?.PlayMusic("home");
        }

        private void OnDestroy()
        {
            ReleaseVideo();
        }

        // ---------- Backgrounds ----------

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
            backgroundImage = go.GetComponent<Image>();
            backgroundImage.raycastTarget = false;
        }

        private void SetBackground(string spriteName, Color fallback)
        {
            if (backgroundImage == null) return;
            var sprite = ArtLoader.Get(spriteName);
            if (sprite != null)
            {
                backgroundImage.sprite = sprite;
                backgroundImage.color = Color.white;
            }
            else
            {
                backgroundImage.sprite = null;
                backgroundImage.color = fallback;
            }
        }

        // ---------- Optional looping video (only if Animated Home is ON) ----------

        private void BuildVideoBackground()
        {
            if (homeVideo == null || !SettingsSave.AnimatedHome) return;

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

        // ---------- Home screen ----------

        private void BuildHomeOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0f));
            panel.GetComponent<Image>().raycastTarget = false;

            var logo = UiFactory.MakeArtImage(panel.transform, "logo", new Vector2(0f, 620f), new Vector2(950f, 260f));
            if (logo == null)
            {
                UiFactory.MakeText(panel.transform, "ANIMAL GRID", new Vector2(0f, 620f),
                    new Vector2(950f, 200f), 100, new Color(0.35f, 0.22f, 0.15f));
            }

            if (campaign.gameCompleted)
            {
                UiFactory.MakeText(panel.transform, "All worlds complete!", new Vector2(0f, 460f),
                    new Vector2(900f, 100f), 56, new Color(0.95f, 0.75f, 0.2f));
                UiFactory.MakeText(panel.transform, "Your collection is full. Thank you for playing!",
                    new Vector2(0f, 340f), new Vector2(900f, 100f), 42, new Color(0.55f, 0.4f, 0.3f));
            }
            else
            {
                var world = Worlds.Get(campaign.worldIndex);
                UiFactory.MakeText(panel.transform,
                    "World " + (campaign.worldIndex + 1) + " - " + world.name,
                    new Vector2(0f, 480f), new Vector2(800f, 100f), 46, new Color(0.55f, 0.4f, 0.3f));

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

                string label = (campaign.worldIndex == 0 && campaign.levelInWorld == 1 && !CampaignSave.TutorialDone)
                    ? "Start - Level 1"
                    : "Continue - Level " + campaign.levelInWorld;

                var continueBtn = UiFactory.MakeButton(panel.transform, label, new Vector2(0f, -220f),
                    new Vector2(640f, 150f), new Color(0.45f, 0.75f, 0.35f), Color.white, artKey: "button_gold");
                continueBtn.onClick.AddListener(() =>
                {
                    SoundManager.Instance?.PlayButton();
                    Destroy(panel);
                    StartLevel();
                });

                int pts = campaign.worldPoints[campaign.worldIndex];
                int unlockedCount = campaign.UnlockedInWorld(campaign.worldIndex);
                float fraction = UnlockProgression.BarFraction(pts);
                string barLabel;
                if (unlockedCount >= world.animalIds.Length)
                {
                    barLabel = world.name + " collection complete!";
                    fraction = 1f;
                }
                else
                {
                    int nextTh = UnlockProgression.Thresholds[unlockedCount - 1];
                    barLabel = "Level " + campaign.levelInWorld + "/180  ·  Next: "
                        + Worlds.DisplayName(world.animalIds[unlockedCount]) + " " + pts + "/" + nextTh;
                }
                UiFactory.MakeUnlockBar(panel.transform, new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -400f), fraction, barLabel, new Color(0.5f, 0.4f, 0.32f));
            }

            // Icon-on-top tiles (see UiFactory.MakeIconButton): each looks for
            // Resources/Art/Icons/icon_<key>.png and falls back to a text-only
            // tile automatically if that art doesn't exist yet, so this row is
            // safe to ship today and will pick up icons with zero code changes
            // as art lands. "shop" already resolves (see project README).
            var collectionBtn = UiFactory.MakeIconButton(panel.transform, "collection", "Collection",
                new Vector2(-230f, -600f), new Vector2(420f, 130f),
                new Color(0.87f, 0.78f, 0.62f), new Color(0.3f, 0.2f, 0.1f));
            collectionBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                BuildCollectionOverlay();
            });

            var settingsBtn = UiFactory.MakeIconButton(panel.transform, "settings", "Settings",
                new Vector2(230f, -600f), new Vector2(420f, 130f),
                new Color(0.87f, 0.78f, 0.62f), new Color(0.3f, 0.2f, 0.1f));
            settingsBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                BuildSettingsOverlay();
            });

            MakeComingSoonButton(panel.transform, "leaderboard", "Leaderboard", new Vector2(-340f, -740f));
            MakeComingSoonButton(panel.transform, "daily_challenge", "Daily Challenge", new Vector2(0f, -740f));

            var shopBtn = UiFactory.MakeIconButton(panel.transform, "shop", "Shop",
                new Vector2(340f, -740f), new Vector2(300f, 120f),
                new Color(0.8f, 0.75f, 0.7f), new Color(0.35f, 0.25f, 0.2f));
            shopBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                BuildShopOverlay();
            });

            var reset = UiFactory.MakeButton(panel.transform, "Reset Progress", new Vector2(0f, -880f),
                new Vector2(380f, 90f), new Color(0.8f, 0.75f, 0.7f), new Color(0.35f, 0.25f, 0.2f));
            reset.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                CampaignSave.Save(CampaignState.Fresh());
                CampaignSave.TutorialDone = false;
                ReturnToGame = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }

        private void MakeComingSoonButton(Transform parent, string iconKey, string label, Vector2 pos)
        {
            var btn = UiFactory.MakeIconButton(parent, iconKey, label, pos,
                new Vector2(300f, 120f), new Color(0.8f, 0.75f, 0.7f), new Color(0.35f, 0.25f, 0.2f));
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                ShowComingSoon(label);
            });

            UiFactory.MakeText(parent, "soon", pos + new Vector2(115f, 72f),
                new Vector2(90f, 40f), 26, new Color(0.95f, 0.55f, 0.15f));
        }

        private void ShowComingSoon(string feature)
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0f, 0f, 0f, 0.55f));
            UiFactory.MakeText(panel.transform, feature + "\nComing Soon!", new Vector2(0f, 0f),
                new Vector2(800f, 260f), 64, Color.white);
            StartCoroutine(CloseAfter(panel, 1.6f));
        }

        private IEnumerator CloseAfter(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) Destroy(go);
        }

        // ---------- Settings screen ----------

        private void BuildSettingsOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0.97f, 0.94f, 0.89f, 1f));

            UiFactory.MakeText(panel.transform, "Settings", new Vector2(0f, 700f),
                new Vector2(700f, 140f), 84, new Color(0.35f, 0.22f, 0.15f));

            var back = UiFactory.MakeButton(panel.transform, "Back", new Vector2(-400f, 710f),
                new Vector2(220f, 90f), new Color(0.8f, 0.75f, 0.7f), new Color(0.35f, 0.25f, 0.2f));
            back.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                Destroy(panel);
            });

            MakeToggleRow(panel.transform, "Sound Effects", 380f,
                () => SettingsSave.SfxOn,
                v => { SettingsSave.SfxOn = v; SoundManager.ApplySettings(); });

            MakeToggleRow(panel.transform, "Music", 200f,
                () => SettingsSave.MusicOn,
                v => { SettingsSave.MusicOn = v; SoundManager.ApplySettings(); });

            MakeToggleRow(panel.transform, "Vibration", 20f,
                () => SettingsSave.VibrationOn,
                v => { SettingsSave.VibrationOn = v; });

            MakeToggleRow(panel.transform, "Animated Home", -160f,
                () => SettingsSave.AnimatedHome,
                v => { SettingsSave.AnimatedHome = v; });

            UiFactory.MakeText(panel.transform,
                "Music plays when a music_home clip exists in Resources/Audio.\nAnimated Home applies the next time Home opens.",
                new Vector2(0f, -420f), new Vector2(900f, 140f), 34, new Color(0.55f, 0.45f, 0.35f));
        }

        private void MakeToggleRow(Transform parent, string label, float y,
            System.Func<bool> get, System.Action<bool> set)
        {
            var root = new GameObject("Row_" + label, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = root.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(700f, 110f);

            UiFactory.MakeText(root.transform, label, new Vector2(-140f, 0f),
                new Vector2(400f, 90f), 44, new Color(0.35f, 0.22f, 0.15f));

            var btnGo = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(root.transform, false);
            var bRect = btnGo.transform as RectTransform;
            bRect.anchorMin = bRect.anchorMax = new Vector2(1f, 0.5f);
            bRect.anchoredPosition = new Vector2(-90f, 0f);
            bRect.sizeDelta = new Vector2(160f, 80f);
            var bg = btnGo.GetComponent<Image>();
            bg.sprite = UiSprites.RoundedSquare;

            var knobGo = new GameObject("Knob", typeof(RectTransform), typeof(Image));
            knobGo.transform.SetParent(btnGo.transform, false);
            var kRect = knobGo.transform as RectTransform;
            kRect.anchorMin = kRect.anchorMax = new Vector2(0.5f, 0.5f);
            kRect.sizeDelta = new Vector2(64f, 64f);
            var knob = knobGo.GetComponent<Image>();
            knob.sprite = UiSprites.Circle;
            knob.color = Color.white;
            knob.raycastTarget = false;

            var onOff = UiFactory.MakeText(root.transform, "", new Vector2(250f, 0f),
                new Vector2(140f, 80f), 40, new Color(0.5f, 0.4f, 0.32f));

            System.Action refresh = null;
            refresh = () =>
            {
                bool on = get();
                bg.color = on ? new Color(0.45f, 0.75f, 0.35f) : new Color(0.6f, 0.6f, 0.6f, 0.6f);
                kRect.anchoredPosition = new Vector2(on ? 40f : -40f, 0f);
                onOff.text = on ? "ON" : "OFF";
            };
            refresh();

            btnGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                set(!get());
                refresh();
                SoundManager.Instance?.PlayButton();
            });
        }

        // ---------- Collection screen ----------

        private void BuildCollectionOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0.97f, 0.94f, 0.89f, 1f));

            UiFactory.MakeText(panel.transform, "Collection", new Vector2(0f, 830f),
                new Vector2(700f, 140f), 84, new Color(0.35f, 0.22f, 0.15f));

            var back = UiFactory.MakeButton(panel.transform, "Back", new Vector2(-400f, 840f),
                new Vector2(220f, 90f), new Color(0.8f, 0.75f, 0.7f), new Color(0.35f, 0.25f, 0.2f));
            back.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                Destroy(panel);
            });

            for (int w = 0; w < Worlds.Count; w++)
            {
                var world = Worlds.Get(w);
                int unlockedCount = campaign.UnlockedInWorld(w);
                bool worldLocked = w > campaign.worldIndex && !campaign.gameCompleted;
                float blockTop = 680f - w * 560f;

                string header = world.name + "  ·  " + unlockedCount + "/10"
                    + (worldLocked ? "  ·  Locked" : "");
                UiFactory.MakeText(panel.transform, header, new Vector2(0f, blockTop),
                    new Vector2(900f, 80f), 48, new Color(0.45f, 0.3f, 0.2f));

                for (int i = 0; i < world.animalIds.Length; i++)
                {
                    int row = i / 5;
                    int col = i % 5;
                    float x = -340f + col * 170f;
                    float y = blockTop - 160f - row * 220f;
                    bool unlocked = i < unlockedCount;
                    MakeSlot(panel.transform, new Vector2(x, y), world.animalIds[i], unlocked);
                }
            }
        }

        private void MakeSlot(Transform parent, Vector2 pos, string animalId, bool unlocked)
        {
            var root = new GameObject("Slot_" + animalId, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = root.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(150f, 200f);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(root.transform, false);
            var iRect = iconGo.transform as RectTransform;
            iRect.anchorMin = iRect.anchorMax = new Vector2(0.5f, 0.5f);
            iRect.anchoredPosition = new Vector2(0f, 30f);
            iRect.sizeDelta = new Vector2(140f, 140f);
            var img = iconGo.GetComponent<Image>();
            var sprite = ArtLoader.GetAnimal(animalId);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.preserveAspect = true;
                img.color = unlocked ? Color.white : new Color(0.15f, 0.12f, 0.12f, 0.9f);
            }
            else
            {
                img.sprite = UiSprites.RoundedSquare;
                img.color = unlocked ? new Color(0.85f, 0.8f, 0.75f) : new Color(0.3f, 0.27f, 0.25f, 0.9f);
            }
            img.raycastTarget = false;

            if (sprite == null)
            {
                UiFactory.MakeText(root.transform, unlocked ? "!" : "?", new Vector2(0f, 30f),
                    new Vector2(100f, 100f), 60,
                    unlocked ? Color.white : new Color(1f, 1f, 1f, 0.6f));
            }

            UiFactory.MakeText(root.transform, unlocked ? Worlds.DisplayName(animalId) : "???",
                new Vector2(0f, -70f), new Vector2(160f, 50f), 32,
                unlocked ? new Color(0.35f, 0.22f, 0.15f) : new Color(0.6f, 0.55f, 0.5f));
        }

        // ---------- Shop screen ----------

        private void BuildShopOverlay()
        {
            var panel = UiFactory.MakePanel(canvas.transform, new Color(0.97f, 0.94f, 0.89f, 1f));

            UiFactory.MakeText(panel.transform, "Shop", new Vector2(0f, 830f),
                new Vector2(700f, 140f), 84, new Color(0.35f, 0.22f, 0.15f));

            var back = UiFactory.MakeButton(panel.transform, "Back", new Vector2(-400f, 840f),
                new Vector2(220f, 90f), new Color(0.8f, 0.75f, 0.7f), new Color(0.35f, 0.25f, 0.2f));
            back.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayButton();
                Destroy(panel);
            });

            var balanceText = UiFactory.MakeText(panel.transform, "", new Vector2(0f, 700f),
                new Vector2(700f, 90f), 48, new Color(0.6f, 0.45f, 0.05f));
            var bankText = UiFactory.MakeText(panel.transform, "", new Vector2(0f, 630f),
                new Vector2(700f, 70f), 34, new Color(0.5f, 0.4f, 0.32f));

            System.Action refreshBalance = null;
            refreshBalance = () =>
            {
                balanceText.text = "Coins: " + CoinSave.Balance;
                bankText.text = "Bonus hints banked: " + HintBankSave.BonusHints;
            };
            refreshBalance();

            UiFactory.MakeText(panel.transform,
                "Hints point at the next correct square mid-level. Buy a pack here and it's\n"
                + "ready the moment your 3 free hints for the level run out.",
                new Vector2(0f, 500f), new Vector2(900f, 130f), 32, new Color(0.55f, 0.45f, 0.35f));

            MakeShopItem(panel.transform, new Vector2(-240f, 250f), "Hint Pack",
                "+" + ShopCatalog.HintPackHints + " hints", ShopCatalog.HintPackCost,
                ShopCatalog.TryBuyHintPack, refreshBalance);

            MakeShopItem(panel.transform, new Vector2(240f, 250f), "Hint Value Pack",
                "+" + ShopCatalog.HintValuePackHints + " hints", ShopCatalog.HintValuePackCost,
                ShopCatalog.TryBuyHintValuePack, refreshBalance);

            UiFactory.MakeText(panel.transform,
                "Out of lives mid-level? You can also continue with coins\nright from the fail screen — no need to come back here.",
                new Vector2(0f, -140f), new Vector2(900f, 120f), 32, new Color(0.55f, 0.45f, 0.35f));

            UiFactory.MakeText(panel.transform,
                "Earn coins by clearing levels — bigger boards and perfect\n('Exact!') clears pay out more.",
                new Vector2(0f, -260f), new Vector2(900f, 100f), 30, new Color(0.6f, 0.55f, 0.5f));
        }

        private void MakeShopItem(Transform parent, Vector2 pos, string title, string subtitle,
            int cost, System.Func<bool> tryBuy, System.Action onPurchased)
        {
            var root = new GameObject("ShopItem_" + title, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            var rect = root.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(420f, 380f);
            var bg = root.GetComponent<Image>();
            bg.sprite = UiSprites.RoundedSquare;
            bg.color = new Color(1f, 1f, 1f, 0.6f);

            UiFactory.MakeText(root.transform, title, new Vector2(0f, 120f),
                new Vector2(380f, 80f), 44, new Color(0.35f, 0.22f, 0.15f));
            UiFactory.MakeText(root.transform, subtitle, new Vector2(0f, 40f),
                new Vector2(380f, 60f), 36, new Color(0.5f, 0.4f, 0.32f));

            var buy = UiFactory.MakeButton(root.transform, cost + " coins", new Vector2(0f, -130f),
                new Vector2(340f, 110f), new Color(0.45f, 0.75f, 0.35f), Color.white, artKey: "button_gold");
            buy.onClick.AddListener(() =>
            {
                if (tryBuy())
                {
                    SoundManager.Instance?.PlayButton();
                    onPurchased?.Invoke();
                }
                else
                {
                    SoundManager.Instance?.PlayInvalid();
                    StartCoroutine(CloseAfter(MakeToast(root.transform, "Not enough coins"), 1.1f));
                }
            });
        }

        private GameObject MakeToast(Transform parent, string message)
        {
            var go = new GameObject("Toast", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiFactory.MakeText(go.transform, message, new Vector2(0f, -200f),
                new Vector2(380f, 60f), 30, new Color(0.85f, 0.2f, 0.2f));
            return go;
        }

        // ---------- Level start ----------

        private void StartLevel()
        {
            ReleaseVideo();
            SetBackground("background_game", GameCream);

            int levelNumber = overrideLevelNumber > 0 ? overrideLevelNumber : campaign.levelInWorld;
            if (overrideLevelNumber > 0)
            {
                Debug.LogWarning("OVERRIDE LEVEL ACTIVE: " + overrideLevelNumber
                    + "  (set Override Level Number = 0 in the Inspector for real progression!)");
            }
            Debug.Log("START -> World " + (campaign.worldIndex + 1) + " Level " + levelNumber);
            LevelConfig config = ProgressionConfig.GetLevel(levelNumber);
            SoundManager.Instance?.PlayMusic(config.isBoss ? "boss" : "game");
            var world = Worlds.Get(campaign.worldIndex);

            int unlockedCount = campaign.UnlockedInWorld(campaign.worldIndex);
            var unlocked = new List<string>();
            for (int i = 0; i < unlockedCount && i < world.animalIds.Length; i++)
            {
                unlocked.Add(world.animalIds[i]);
            }

            int seed = (campaign.worldIndex + 1) * 100000 + levelNumber * 7919 + 17;
            var regionAnimals = RosterDistributor.Distribute(unlocked, config.gridSize, seed);

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
                randomSeed = seed,
                animalIds = regionAnimals
            });

            if (puzzle == null)
            {
                Debug.LogError("Puzzle generation failed!");
                return;
            }

            board.RenderBoard(puzzle);

            var controller = boardGo.AddComponent<GameplayController>();
            controller.Initialize(board, puzzle, levelNumber, config, campaign);

            if (campaign.worldIndex == 0 && levelNumber == 1 && !CampaignSave.TutorialDone)
            {
                controller.StartTutorial();
            }
        }
    }
}