using UnityEngine;

/// <summary>
/// Add this to your Player GameObject to make it persist across scenes
/// </summary>
public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Player is now persistent across scenes");
        }
        else
        {
            // Destroy duplicate player
            Debug.Log("Destroying duplicate player");
            Destroy(gameObject);
        }
    }
}