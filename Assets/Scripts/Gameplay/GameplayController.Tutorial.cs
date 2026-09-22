using UnityEngine;
using UnityEngine.UI;
using AnimalGrid.Core;
using AnimalGrid.Audio;
using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    // ---------- Tutorial ----------
    // Split out of the original monolithic GameplayController.cs. Same class,
    // same fields (tutorialActive, tutorialSteps, etc. are declared in the
    // core GameplayController.cs) — this file only groups the tutorial-related
    // methods together so they're easy to find.
    public partial class GameplayController
    {
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
                new Vector2(260f, 100f), new Color(0.45f, 0.75f, 0.35f), Color.white, artKey: "button_gold");
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
    }
}
