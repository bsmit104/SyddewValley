// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class ChangeScenes : MonoBehaviour
// {
//     public string sceneName;

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Player"))
//         {
//             SceneManager.LoadScene(sceneName);
//         }
//     }
// }














// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class ChangeScenes : MonoBehaviour
// {
//     [Header("Scene Change Settings")]
//     public string sceneName;      // Scene to load
//     public string entranceID;     // ID to tell new scene where to spawn

//     // Stores the last entrance used between scenes
//     public static string lastEntrance;

//     [Header("Scene Spawn Settings (only used when this object is a Spawn Point)")]
//     public bool isSpawnPoint;      // Check if this object is a spawn point in new scene
//     public string spawnID;         // ID this spawn point represents

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         if (!isSpawnPoint && other.CompareTag("Player"))
//         {
//             // Record where we came from
//             lastEntrance = entranceID;

//             // Load new scene
//             SceneManager.LoadScene(sceneName);
//         }
//     }

//     private void Start()
//     {
//         // If this object is a spawn point, try to spawn the player here
//         if (isSpawnPoint && spawnID == lastEntrance)
//         {
//             GameObject player = GameObject.FindGameObjectWithTag("Player");

//             if (player != null)
//             {
//                 player.transform.position = transform.position;
//             }
//         }
//     }
// }



using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChangeScenes : MonoBehaviour
{
    [Header("Scene Change Settings")]
    public string sceneName;      // Scene to load
    public string entranceID;     // ID for where the player came from

    public static string lastEntrance; // persists between scenes

    [Header("Scene Spawn Settings (for spawn point objects)")]
    public bool isSpawnPoint = false; // set true only on spawn points
    public string spawnID;            // match this to entranceID

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isSpawnPoint && other.CompareTag("Player"))
        {
            // Record which door/entrance triggered this
            lastEntrance = entranceID;

            // Smooth scene fade transition
            StartCoroutine(FadeController.Instance.FadeOutAndLoad(sceneName));
        }
    }

    private void Start()
    {
        // If this object is a spawn point AND matches the last entrance, move player here
        if (isSpawnPoint && spawnID == lastEntrance)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                player.transform.position = transform.position;
            }
        }
    }
}
