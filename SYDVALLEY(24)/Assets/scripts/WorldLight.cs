using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace WorldTime
{
    [RequireComponent(typeof(Light2D))]
    public class WorldLight : MonoBehaviour
    {
        public float duration = 5f;
        [SerializeField] private Gradient gradient;
        private Light2D Sun;
        private float startTime;
        // Start is called before the first frame update
        private void Start()
        {
            Sun = GetComponent<Light2D>();
            startTime = Time.time;
        }

        // Update is called once per frame
        private void Update()
        {
            float timeElapsed = Time.time - startTime;
            float percentage = Mathf.Sin(f: timeElapsed / duration * Mathf.PI * 2) * .5f + .5f;
            percentage = Mathf.Clamp01(percentage);
            Sun.color = gradient.Evaluate(percentage);
        }
    }
}

//https://www.youtube.com/watch?v=BCR2xQ7jWMU&ab_channel=PitiIT
//https://www.youtube.com/watch?v=WxxNfyxpvhE&ab_channel=GrowthforGames