// using UnityEngine;

// public class SceneSpawnManager : MonoBehaviour
// {
//     [System.Serializable]
//     public struct SpawnPoint
//     {
//         public string entranceID;
//         public Transform spawnLocation;
//     }

//     public SpawnPoint[] spawnPoints;

//     void Start()
//     {
//         string entrance = SceneTransition.lastEntrance;
//         GameObject player = GameObject.FindGameObjectWithTag("Player");

//         foreach (var point in spawnPoints)
//         {
//             if (point.entranceID == entrance)
//             {
//                 player.transform.position = point.spawnLocation.position;
//                 return;
//             }
//         }
//     }
// }
