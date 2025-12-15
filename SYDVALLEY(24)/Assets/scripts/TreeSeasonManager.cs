using UnityEngine;
using UnityEngine.SceneManagement;
using WorldTime;

public class TreeSeasonManager : MonoBehaviour
{
    [System.Serializable]
    public class TreeSeasonPrefabs
    {
        public GameObject augtomberPrefab;
        public GameObject novecanuaryPrefab;
        public GameObject febmaprilPrefab;
        public GameObject mayunlyPrefab;
    }

    [Header("Tree Style A")]
    [SerializeField] private TreeSeasonPrefabs styleA;
    [Header("Tree Style B")]
    [SerializeField] private TreeSeasonPrefabs styleB;

    private void Start()
    {
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.OnMonthChanged += HandleMonthChanged;
            UpdateTreesForCurrentMonth();
        }
        
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.OnMonthChanged -= HandleMonthChanged;
        }
        
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update trees in the newly loaded scene
        UpdateTreesForCurrentMonth();
    }

    private void HandleMonthChanged(CalendarManager.Month newMonth)
    {
        UpdateTreesForCurrentMonth();
    }

    private void UpdateTreesForCurrentMonth()
    {
        // Find all trees with their respective tags
        GameObject[] styleATrees = GameObject.FindGameObjectsWithTag("TreeStyleA");
        GameObject[] styleBTrees = GameObject.FindGameObjectsWithTag("TreeStyleB");

        // Update Style A trees
        foreach (GameObject tree in styleATrees)
        {
            ReplaceTree(tree, styleA, "TreeStyleA");
        }

        // Update Style B trees
        foreach (GameObject tree in styleBTrees)
        {
            ReplaceTree(tree, styleB, "TreeStyleB");
        }
    }

    private void ReplaceTree(GameObject currentTree, TreeSeasonPrefabs seasonPrefabs, string treeTag)
    {
        if (currentTree == null) return;

        // Store the current tree's position and rotation
        Vector3 position = currentTree.transform.position;
        Quaternion rotation = currentTree.transform.rotation;
        Transform parent = currentTree.transform.parent;

        // Get the appropriate prefab for the current month
        GameObject newPrefab = GetPrefabForCurrentMonth(seasonPrefabs);
        if (newPrefab == null) return;

        // Instantiate the new tree
        GameObject newTree = Instantiate(newPrefab, position, rotation, parent);
        
        // Set the appropriate tag
        newTree.tag = treeTag;
        
        // Copy any necessary components from the old tree to maintain functionality
        CopyTreeComponents(currentTree, newTree);

        // Destroy the old tree
        Destroy(currentTree);
    }

    private GameObject GetPrefabForCurrentMonth(TreeSeasonPrefabs seasonPrefabs)
    {
        if (CalendarManager.Instance == null) return null;

        switch (CalendarManager.Instance.CurrentMonth)
        {
            case CalendarManager.Month.Augtomber:
                return seasonPrefabs.augtomberPrefab;
            case CalendarManager.Month.Novecanuary:
                return seasonPrefabs.novecanuaryPrefab;
            case CalendarManager.Month.Febmapril:
                return seasonPrefabs.febmaprilPrefab;
            case CalendarManager.Month.Mayunly:
                return seasonPrefabs.mayunlyPrefab;
            default:
                return seasonPrefabs.augtomberPrefab;
        }
    }

    private void CopyTreeComponents(GameObject oldTree, GameObject newTree)
    {
        // Copy the TransparencyManager component if it exists
        TransparencyManager oldTransparency = oldTree.GetComponent<TransparencyManager>();
        if (oldTransparency != null)
        {
            TransparencyManager newTransparency = newTree.GetComponent<TransparencyManager>();
            if (newTransparency == null)
            {
                newTransparency = newTree.AddComponent<TransparencyManager>();
            }
            newTransparency.treeTransparencyAmount = oldTransparency.treeTransparencyAmount;
            newTransparency.playerTransparencyAmount = oldTransparency.playerTransparencyAmount;
        }

        // Copy Collider2D component if it exists
        Collider2D oldCollider = oldTree.GetComponent<Collider2D>();
        if (oldCollider != null)
        {
            Collider2D newCollider = newTree.GetComponent<Collider2D>();
            if (newCollider == null)
            {
                newCollider = newTree.AddComponent<Collider2D>();
            }
            // Copy collider properties
            if (oldCollider is BoxCollider2D oldBox && newCollider is BoxCollider2D newBox)
            {
                newBox.size = oldBox.size;
                newBox.offset = oldBox.offset;
            }
            else if (oldCollider is CircleCollider2D oldCircle && newCollider is CircleCollider2D newCircle)
            {
                newCircle.radius = oldCircle.radius;
                newCircle.offset = oldCircle.offset;
            }
        }

        // Copy any other components that need to be preserved
        // Add more component copying logic here as needed
    }
}

// using UnityEngine;
// using WorldTime;

// public class TreeSeasonManager : MonoBehaviour
// {
//     [System.Serializable]
//     public class TreeSeasonPrefabs
//     {
//         public GameObject augtomberPrefab;
//         public GameObject novecanuaryPrefab;
//         public GameObject febmaprilPrefab;
//         public GameObject mayunlyPrefab;
//     }

//     [Header("Tree Style A")]
//     [SerializeField] private TreeSeasonPrefabs styleA;
//     [Header("Tree Style B")]
//     [SerializeField] private TreeSeasonPrefabs styleB;

//     private void Start()
//     {
//         if (CalendarManager.Instance != null)
//         {
//             CalendarManager.Instance.OnMonthChanged += HandleMonthChanged;
//             UpdateTreesForCurrentMonth();
//         }
//     }

//     private void OnDestroy()
//     {
//         if (CalendarManager.Instance != null)
//         {
//             CalendarManager.Instance.OnMonthChanged -= HandleMonthChanged;
//         }
//     }

//     private void HandleMonthChanged(CalendarManager.Month newMonth)
//     {
//         UpdateTreesForCurrentMonth();
//     }

//     private void UpdateTreesForCurrentMonth()
//     {
//         // Find all trees with their respective tags
//         GameObject[] styleATrees = GameObject.FindGameObjectsWithTag("TreeStyleA");
//         GameObject[] styleBTrees = GameObject.FindGameObjectsWithTag("TreeStyleB");

//         // Update Style A trees
//         foreach (GameObject tree in styleATrees)
//         {
//             ReplaceTree(tree, styleA, "TreeStyleA");
//         }

//         // Update Style B trees
//         foreach (GameObject tree in styleBTrees)
//         {
//             ReplaceTree(tree, styleB, "TreeStyleB");
//         }
//     }

//     private void ReplaceTree(GameObject currentTree, TreeSeasonPrefabs seasonPrefabs, string treeTag)
//     {
//         if (currentTree == null) return;

//         // Store the current tree's position and rotation
//         Vector3 position = currentTree.transform.position;
//         Quaternion rotation = currentTree.transform.rotation;
//         Transform parent = currentTree.transform.parent;

//         // Get the appropriate prefab for the current month
//         GameObject newPrefab = GetPrefabForCurrentMonth(seasonPrefabs);
//         if (newPrefab == null) return;

//         // Instantiate the new tree
//         GameObject newTree = Instantiate(newPrefab, position, rotation, parent);
        
//         // Set the appropriate tag
//         newTree.tag = treeTag;
        
//         // Copy any necessary components from the old tree to maintain functionality
//         CopyTreeComponents(currentTree, newTree);

//         // Destroy the old tree
//         Destroy(currentTree);
//     }

//     private GameObject GetPrefabForCurrentMonth(TreeSeasonPrefabs seasonPrefabs)
//     {
//         if (CalendarManager.Instance == null) return null;

//         switch (CalendarManager.Instance.CurrentMonth)
//         {
//             case CalendarManager.Month.Augtomber:
//                 return seasonPrefabs.augtomberPrefab;
//             case CalendarManager.Month.Novecanuary:
//                 return seasonPrefabs.novecanuaryPrefab;
//             case CalendarManager.Month.Febmapril:
//                 return seasonPrefabs.febmaprilPrefab;
//             case CalendarManager.Month.Mayunly:
//                 return seasonPrefabs.mayunlyPrefab;
//             default:
//                 return seasonPrefabs.augtomberPrefab;
//         }
//     }

//     private void CopyTreeComponents(GameObject oldTree, GameObject newTree)
//     {
//         // Copy the TransparencyManager component if it exists
//         TransparencyManager oldTransparency = oldTree.GetComponent<TransparencyManager>();
//         if (oldTransparency != null)
//         {
//             TransparencyManager newTransparency = newTree.GetComponent<TransparencyManager>();
//             if (newTransparency == null)
//             {
//                 newTransparency = newTree.AddComponent<TransparencyManager>();
//             }
//             newTransparency.treeTransparencyAmount = oldTransparency.treeTransparencyAmount;
//             newTransparency.playerTransparencyAmount = oldTransparency.playerTransparencyAmount;
//         }

//         // Copy Collider2D component if it exists
//         Collider2D oldCollider = oldTree.GetComponent<Collider2D>();
//         if (oldCollider != null)
//         {
//             Collider2D newCollider = newTree.GetComponent<Collider2D>();
//             if (newCollider == null)
//             {
//                 newCollider = newTree.AddComponent<Collider2D>();
//             }
//             // Copy collider properties
//             if (oldCollider is BoxCollider2D oldBox && newCollider is BoxCollider2D newBox)
//             {
//                 newBox.size = oldBox.size;
//                 newBox.offset = oldBox.offset;
//             }
//             else if (oldCollider is CircleCollider2D oldCircle && newCollider is CircleCollider2D newCircle)
//             {
//                 newCircle.radius = oldCircle.radius;
//                 newCircle.offset = oldCircle.offset;
//             }
//         }

//         // Copy any other components that need to be preserved
//         // Add more component copying logic here as needed
//     }
// } 