using UnityEngine;
using UnityEngine.Rendering.Universal;   // This is REQUIRED for Light2D

namespace WorldTime
{
    [RequireComponent(typeof(Light2D))]
    public class WorldLight : MonoBehaviour
    {
        [Header("Day/Night Cycle")]
        public float duration = 300f;           // ← Match this with WorldClock.dayDurationInSeconds

        [SerializeField] private Gradient gradient;   // ← Keep private + SerializeField

        // These MUST be public so WorldClock can reset them
        public float startTime;
        public float lastDayTime;

        private Light2D sunLight;

        private void Awake()
        {
            sunLight = GetComponent<Light2D>();
            startTime = Time.time;
            lastDayTime = startTime;
        }

        private void Update()
        {
            float timeElapsed = Time.time - startTime;
            float t = timeElapsed / duration;

            // Exact same sine-wave math used everywhere
            float percentage = Mathf.Sin(t * Mathf.PI * 2f) * 0.5f + 0.5f;
            percentage = Mathf.Clamp01(percentage);

            sunLight.color = gradient.Evaluate(percentage);
        }

        // Helper for instant color update (used when sleeping)
        public void SetTimeOfDay(float time01) // 0 = midnight, 0.25 = 6 AM, 0.5 = noon, etc.
        {
            float percentage = Mathf.Sin(time01 * Mathf.PI * 2f) * 0.5f + 0.5f;
            sunLight.color = gradient.Evaluate(percentage);
        }
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering.Universal;
// using UnityEngine.Serialization;

// namespace WorldTime
// {
//     [RequireComponent(typeof(Light2D))]
//     public class WorldLight : MonoBehaviour
//     {
//         public float duration = 5f;
//         [SerializeField] private Gradient gradient;
//         private Light2D Sun;
//         private float startTime;
//         private float lastDayTime;
//         private bool dayChanged = false;

//         // Start is called before the first frame update
//         private void Start()
//         {
//             Sun = GetComponent<Light2D>();
//             startTime = Time.time;
//             lastDayTime = startTime;
//         }

//         // Update is called once per frame
//         private void Update()
//         {
//             float timeElapsed = Time.time - startTime;
//             float percentage = Mathf.Sin(f: timeElapsed / duration * Mathf.PI * 2) * .5f + .5f;
//             percentage = Mathf.Clamp01(percentage);
//             Sun.color = gradient.Evaluate(percentage);

//             // Check if a full day has passed
//             if (timeElapsed - lastDayTime >= duration && !dayChanged)
//             {
//                 dayChanged = true;
//                 if (CalendarManager.Instance != null)
//                 {
//                     CalendarManager.Instance.AdvanceDay();
//                 }
//                 lastDayTime = timeElapsed;
//             }
//             else if (timeElapsed - lastDayTime < duration)
//             {
//                 dayChanged = false;
//             }
//         }
//     }
// }

//https://www.youtube.com/watch?v=BCR2xQ7jWMU&ab_channel=PitiIT
//https://www.youtube.com/watch?v=WxxNfyxpvhE&ab_channel=GrowthforGames