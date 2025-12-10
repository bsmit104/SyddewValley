using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldTime
{
    /// <summary>
    /// Syncs WorldLight with WorldClock whenever a new scene loads
    /// Attach this to your WorldClock GameObject
    /// </summary>
    public class WorldLightManager : MonoBehaviour
    {
        private WorldClock worldClock;

        private void Awake()
        {
            worldClock = GetComponent<WorldClock>();
            if (worldClock == null)
            {
                Debug.LogError("WorldLightManager must be on the same GameObject as WorldClock!");
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Find the WorldLight in the new scene and sync it
            WorldLight worldLight = FindObjectOfType<WorldLight>();
            if (worldLight != null && worldClock != null)
            {
                SyncLightWithClock(worldLight);
                Debug.Log($"Synced WorldLight in {scene.name} with current time: {worldClock.CurrentTimeOfDay:F2}");
            }
        }

        private void SyncLightWithClock(WorldLight worldLight)
        {
            float currentTime = worldClock.CurrentTimeOfDay;
            
            // Sync WorldLight's internal timer
            float elapsedToday = currentTime * worldLight.duration;
            worldLight.startTime = Time.time - elapsedToday;
            worldLight.lastDayTime = Time.time - elapsedToday;
            
            // Set the color immediately
            worldLight.SetTimeOfDay(currentTime);
        }
    }
}