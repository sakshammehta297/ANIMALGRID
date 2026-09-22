using UnityEngine;
using UnityEngine.SceneManagement;
using AnimalGrid.Save;

namespace AnimalGrid.Gameplay
{
    // ---------- Campaign advancement ----------
    // Decides what happens after a level clears (unlock/world/finale overlay
    // or straight to the next level) and the scene reloads that carry it out.
    public partial class GameplayController
    {
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

        private void ReloadScene()
        {
            GameplayBootstrap.ReturnToGame = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
