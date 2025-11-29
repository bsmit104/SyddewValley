using System.Collections;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform waypointParent;
    public float moveSpeed = 3f;
    public float waitTime = 1.5f;
    public bool loopWaypoints = true;

    [Header("Debug")]
    public bool showGizmos = true;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private Animator animator;

    // Track last valid direction
    private float lastDirX = 1f;
    private float lastDirY = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Collect waypoints
        waypoints = new Transform[waypointParent.childCount];
        for (int i = 0; i < waypointParent.childCount; i++)
            waypoints[i] = waypointParent.GetChild(i);

        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints found under " + waypointParent.name);
            enabled = false;
            return;
        }

        // Snap to first waypoint
        transform.position = waypoints[0].position;
        currentWaypointIndex = 0;
        
        // Start moving to next waypoint
        StartCoroutine(MoveToNextWaypoint());
    }

    void Update()
    {
        // Always keep animator parameters updated based on current state
        if (isWaiting)
        {
            // When idle, set Input to 0 but keep Last direction
            animator.SetFloat("InputX", 0f);
            animator.SetFloat("InputY", 0f);
            animator.SetFloat("LastInputX", lastDirX);
            animator.SetFloat("LastInputY", lastDirY);
            animator.SetBool("isWalking", false);
        }
        else
        {
            // When moving, keep direction updated
            animator.SetFloat("InputX", lastDirX);
            animator.SetFloat("InputY", lastDirY);
            animator.SetFloat("LastInputX", lastDirX);
            animator.SetFloat("LastInputY", lastDirY);
        }
    }

    IEnumerator MoveToNextWaypoint()
    {
        while (true)
        {
            // Determine next waypoint
            int nextIndex;
            if (loopWaypoints)
                nextIndex = (currentWaypointIndex + 1) % waypoints.Length;
            else
                nextIndex = currentWaypointIndex + 1;

            // Check if we've reached the end (non-looping)
            if (!loopWaypoints && nextIndex >= waypoints.Length)
            {
                SetIdleState();
                yield break;
            }

            Vector3 startPos = transform.position;
            Vector3 targetPos = waypoints[nextIndex].position;
            
            // Calculate direction once at the start of movement
            Vector2 delta = targetPos - startPos;
            UpdateDirection(delta);

            // Move towards target
            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                // // Wait if game is paused
                // if (PauseController.IsGamePaused)
                // {
                //     yield return null;
                //     continue;
                // }

                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
                
                yield return null;
            }

            // Snap to exact position
            transform.position = targetPos;
            currentWaypointIndex = nextIndex;

            // Wait at waypoint
            isWaiting = true;
            SetIdleState();
            
            yield return new WaitForSeconds(waitTime);
            
            isWaiting = false;
        }
    }

    void UpdateDirection(Vector2 delta)
    {
        // Skip if movement is negligible
        if (delta.sqrMagnitude < 0.001f)
            return;

        float newDirX = 0f;
        float newDirY = 0f;

        // Prioritize the larger component (horizontal vs vertical)
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Primarily horizontal
            newDirX = Mathf.Sign(delta.x);
            newDirY = 0f;
        }
        else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            // Primarily vertical
            newDirX = 0f;
            newDirY = Mathf.Sign(delta.y);
        }
        else
        {
            // Exactly diagonal - prefer horizontal
            newDirX = Mathf.Sign(delta.x);
            newDirY = 0f;
        }

        // Update direction
        lastDirX = newDirX;
        lastDirY = newDirY;

        // Set animator parameters
        animator.SetFloat("InputX", lastDirX);
        animator.SetFloat("InputY", lastDirY);
        animator.SetFloat("LastInputX", lastDirX);
        animator.SetFloat("LastInputY", lastDirY);
        animator.SetBool("isWalking", true);
    }

    void SetIdleState()
    {
        // Set isWalking to false first
        animator.SetBool("isWalking", false);
        
        // IMPORTANT: Keep the direction values set so the idle animation knows which way to face
        // Don't zero out InputX/InputY - let them keep the last movement direction
        animator.SetFloat("InputX", lastDirX);
        animator.SetFloat("InputY", lastDirY);
        animator.SetFloat("LastInputX", lastDirX);
        animator.SetFloat("LastInputY", lastDirY);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || waypointParent == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            Transform a = waypointParent.GetChild(i);
            Transform b = waypointParent.GetChild((i + 1) % waypointParent.childCount);
            Gizmos.DrawLine(a.position, b.position);
            Gizmos.DrawSphere(a.position, 0.1f);
        }
    }
}