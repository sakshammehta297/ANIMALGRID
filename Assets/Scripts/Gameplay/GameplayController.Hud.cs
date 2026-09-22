using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AnimalGrid.Core;
using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    // ---------- HUD ----------
    // Level/score text, hearts, unlock progress bar, animal tray, hint button —
    // everything that's always on screen during a level (as opposed to the
    // full-screen overlays in GameplayController.Overlays.cs).
    public partial class GameplayController
    {
        private void BuildHud()
        {
            AddHomeButton(canvas.transform, new Vector2(-445f, 870f));

            var levelGo = new GameObject("LevelText", typeof(RectTransform), typeof(Text));
            levelGo.transform.SetParent(canvas.transform, false);
            var lRect = levelGo.transform as RectTransform;
            lRect.anchorMin = new Vector2(0.5f, 1f);
            lRect.anchorMax = new Vector2(0.5f, 1f);
            lRect.anchoredPosition = new Vector2(-140f, -90f);
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
            sRect.anchoredPosition = new Vector2(300f, -90f);
            sRect.sizeDelta = new Vector2(460f, 110f);
            scoreText = scoreGo.GetComponent<Text>();
            scoreText.font = UiFonts.Default;
            scoreText.fontSize = 56;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.color = new Color(0.35f, 0.2f, 0.25f);
            scoreText.text = "Score 0";
            scoreText.raycastTarget = false;

            var coinGo = new GameObject("CoinText", typeof(RectTransform), typeof(Text));
            coinGo.transform.SetParent(canvas.transform, false);
            var coinRect = coinGo.transform as RectTransform;
            coinRect.anchorMin = new Vector2(0.5f, 1f);
            coinRect.anchorMax = new Vector2(0.5f, 1f);
            coinRect.anchoredPosition = new Vector2(300f, -160f);
            coinRect.sizeDelta = new Vector2(460f, 70f);
            coinText = coinGo.GetComponent<Text>();
            coinText.font = UiFonts.Default;
            coinText.fontSize = 36;
            coinText.alignment = TextAnchor.MiddleCenter;
            coinText.color = new Color(0.6f, 0.45f, 0.05f);
            coinText.raycastTarget = false;
            RefreshCoinText();

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

        /// <summary>Refreshes the HUD coin readout from CoinSave. Called after
        /// any purchase or coin award — see ShopCatalog.cs / Celebrate().</summary>
        private void RefreshCoinText()
        {
            if (coinText != null) coinText.text = "Coins " + CoinSave.Balance;
        }
    }
}
