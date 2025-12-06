using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CameraBoundsConnector : MonoBehaviour
{
    private CinemachineConfiner2D confiner;
    
    void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindBounds();
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindBounds();
    }
    
    void FindBounds()
    {
        var bounds = GameObject.Find("CameraBounds")?.GetComponent<PolygonCollider2D>();
        if (bounds != null)
        {
            confiner.m_BoundingShape2D = bounds;
            confiner.InvalidateCache();
        }
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}