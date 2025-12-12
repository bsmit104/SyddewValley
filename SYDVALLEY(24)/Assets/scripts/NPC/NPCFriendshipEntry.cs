using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCFriendshipEntry : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Transform heartsContainer;
    public GameObject heartPrefab;

    private List<Image> heartImages = new List<Image>();

    public void Initialize(string npcName, int heartLevel, float progress, Sprite fullHeart, Sprite emptyHeart)
    {
        // Set name
        if (nameText != null)
        {
            nameText.text = npcName;
        }

        // Clear existing hearts
        foreach (var heart in heartImages)
        {
            if (heart != null)
                Destroy(heart.gameObject);
        }
        heartImages.Clear();

        // Create heart icons
        for (int i = 0; i < FriendshipManager.MAX_HEARTS; i++)
        {
            GameObject heartObj;
            Image heartImage;

            if (heartPrefab != null && heartsContainer != null)
            {
                heartObj = Instantiate(heartPrefab, heartsContainer);
                heartImage = heartObj.GetComponent<Image>();
            }
            else
            {
                Debug.LogWarning("Heart prefab or container not assigned!");
                return;
            }

            if (heartImage != null)
            {
                // Set sprite based on heart level
                if (i < heartLevel)
                {
                    heartImage.sprite = fullHeart;
                    heartImage.color = Color.white;
                }
                else if (i == heartLevel && progress > 0.5f)
                {
                    // Optional: Show half heart for progress
                    heartImage.sprite = fullHeart;
                    heartImage.color = new Color(1f, 1f, 1f, 0.5f);
                }
                else
                {
                    heartImage.sprite = emptyHeart;
                    heartImage.color = new Color(1f, 1f, 1f, 0.3f);
                }

                heartImages.Add(heartImage);
            }
        }
    }
}