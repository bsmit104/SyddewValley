using UnityEngine;
using System.Collections;

public class SwordSwing : MonoBehaviour
{
    [Header("Sword Visual")]
    public Sprite swordSprite;

    [Header("Swing Settings")]
    public float swingDuration = 0.40f; // slower swing
    public int damage = 25;

    [Header("Offsets")]
    public Vector2 offsetUp = new Vector2(0, 0.6f);
    public Vector2 offsetDown = new Vector2(0, -0.6f);
    public Vector2 offsetLeft = new Vector2(-0.6f, 0);
    public Vector2 offsetRight = new Vector2(0.6f, 0);

    private SpriteRenderer sr;
    private Collider2D col;
    private bool swinging = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        sr.enabled = false;
        col.enabled = false;

        if (swordSprite != null)
            sr.sprite = swordSprite;
    }

    public void Swing(Vector2 dir)
    {
        // DEFAULT IDLE SWING = DOWN
        if (dir == Vector2.zero)
            dir = Vector2.down;

        if (!swinging)
            StartCoroutine(SwingRoutine(dir.normalized));
    }

    private IEnumerator SwingRoutine(Vector2 dir)
    {
        swinging = true;

        sr.enabled = true;
        col.enabled = true;

        // Put sword near player based on direction
        ApplyOffset(dir);

        // Base rotation (sword pointing outward)
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // Behind → forward arc
        float startAngle = -70f;
        float endAngle = 70f;

        float elapsed = 0f;

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swingDuration;

            // smoother, slower, better looking arc
            t = Mathf.SmoothStep(0, 1, t);

            float angle = Mathf.Lerp(startAngle, endAngle, t);
            transform.localRotation = Quaternion.Euler(0, 0, baseAngle + angle);

            yield return null;
        }

        sr.enabled = false;
        col.enabled = false;
        swinging = false;
    }

    private void ApplyOffset(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            transform.localPosition = dir.x > 0 ? offsetRight : offsetLeft;
        }
        else
        {
            transform.localPosition = dir.y > 0 ? offsetUp : offsetDown;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth hp = other.GetComponent<EnemyHealth>();
        if (hp)
        {
            hp.TakeDamage(damage);
            Debug.Log("Slash hit for " + damage);
        }
    }
}



// using UnityEngine;
// using System.Collections;

// public class SwordSwing : MonoBehaviour
// {
//     [Header("Sword Visual")]
//     public Sprite swordSprite;

//     [Header("Swing Settings")]
//     public float swingDuration = 0.40f; // slower swing
//     public int damage = 25;

//     [Header("Offsets")]
//     public Vector2 offsetUp = new Vector2(0, 0.6f);
//     public Vector2 offsetDown = new Vector2(0, -0.6f);
//     public Vector2 offsetLeft = new Vector2(-0.6f, 0);
//     public Vector2 offsetRight = new Vector2(0.6f, 0);

//     private SpriteRenderer sr;
//     private Collider2D col;
//     private bool swinging = false;

//     private void Awake()
//     {
//         sr = GetComponent<SpriteRenderer>();
//         col = GetComponent<Collider2D>();

//         sr.enabled = false;
//         col.enabled = false;

//         if (swordSprite != null)
//             sr.sprite = swordSprite;
//     }

//     public void Swing(Vector2 dir)
//     {
//         if (!swinging)
//             StartCoroutine(SwingRoutine(dir.normalized));
//     }

//     private IEnumerator SwingRoutine(Vector2 dir)
//     {
//         swinging = true;

//         // Show sword
//         sr.enabled = true;
//         col.enabled = true;

//         // Position sword relative to player
//         ApplyOffset(dir);

//         // Base angle so sword points outward
//         float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

//         // The swing will rotate from -70° behind → +70° in front
//         float startAngle = -70f;
//         float endAngle = 70f;

//         float elapsed = 0f;

//         while (elapsed < swingDuration)
//         {
//             elapsed += Time.deltaTime;
//             float t = elapsed / swingDuration;

//             // smooth motion
//             t = Mathf.SmoothStep(0, 1, t);

//             float angle = Mathf.Lerp(startAngle, endAngle, t);

//             transform.localRotation = Quaternion.Euler(0, 0, baseAngle + angle);

//             yield return null;
//         }

//         // Hide sword again
//         sr.enabled = false;
//         col.enabled = false;
//         swinging = false;
//     }

//     private void ApplyOffset(Vector2 dir)
//     {
//         if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
//         {
//             transform.localPosition = dir.x > 0 ? offsetRight : offsetLeft;
//         }
//         else
//         {
//             transform.localPosition = dir.y > 0 ? offsetUp : offsetDown;
//         }
//     }

//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         EnemyHealth hp = other.GetComponent<EnemyHealth>();
//         if (hp)
//         {
//             hp.TakeDamage(damage);
//             Debug.Log("Slash hit for " + damage);
//         }
//     }
// }
