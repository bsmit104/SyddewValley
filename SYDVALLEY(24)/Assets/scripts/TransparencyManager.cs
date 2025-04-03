using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparencyManager : MonoBehaviour
{
    public float treeTransparencyAmount = 0.5f; // How transparent the tree becomes
    public float playerTransparencyAmount = 0.75f; // How transparent the player becomes
    private Dictionary<Renderer, Color> originalTreeColors = new Dictionary<Renderer, Color>();
    private Renderer playerRenderer;
    private Color originalPlayerColor;
    private bool isBehindTree = false;

    void Start()
    {
        playerRenderer = GetComponent<Renderer>();
        originalPlayerColor = playerRenderer.material.color;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TreeStyleA") || other.CompareTag("TreeStyleB"))
        {
            Renderer treeRenderer = other.GetComponent<Renderer>();
            if (treeRenderer != null && !originalTreeColors.ContainsKey(treeRenderer))
            {
                originalTreeColors[treeRenderer] = treeRenderer.material.color;
                SetTransparency(treeRenderer, treeTransparencyAmount);
            }

            // Make the player slightly transparent
            if (!isBehindTree)
            {
                SetTransparency(playerRenderer, playerTransparencyAmount);
                isBehindTree = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("TreeStyleA") || other.CompareTag("TreeStyleB"))
        {
            Renderer treeRenderer = other.GetComponent<Renderer>();
            if (treeRenderer != null && originalTreeColors.ContainsKey(treeRenderer))
            {
                SetTransparency(treeRenderer, 1.0f);
                originalTreeColors.Remove(treeRenderer);
            }

            // If no more trees in contact, reset player transparency
            if (originalTreeColors.Count == 0 && isBehindTree)
            {
                SetTransparency(playerRenderer, 1.0f);
                isBehindTree = false;
            }
        }
    }

    private void SetTransparency(Renderer renderer, float alpha)
    {
        Color color = renderer.material.color;
        color.a = alpha;
        renderer.material.color = color;
    }
}


// using UnityEngine;

// public class TransparencyManager : MonoBehaviour
// {
//     public float transparencyAmount = 0.5f; // How transparent the object becomes
//     private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

//     void OnTriggerEnter2D(Collider2D other)
//     {
//         if (other.CompareTag("Tree"))
//         {
//             Renderer rend = other.GetComponent<Renderer>();
//             if (rend != null && !originalColors.ContainsKey(rend))
//             {
//                 originalColors[rend] = rend.material.color;
//                 SetTransparency(rend, transparencyAmount);
//             }
//         }
//     }

//     void OnTriggerExit2D(Collider2D other)
//     {
//         if (other.CompareTag("Tree"))
//         {
//             Renderer rend = other.GetComponent<Renderer>();
//             if (rend != null && originalColors.ContainsKey(rend))
//             {
//                 SetTransparency(rend, 1.0f);
//                 originalColors.Remove(rend);
//             }
//         }
//     }

//     private void SetTransparency(Renderer renderer, float alpha)
//     {
//         Color color = renderer.material.color;
//         color.a = alpha;
//         renderer.material.color = color;
//     }
// }