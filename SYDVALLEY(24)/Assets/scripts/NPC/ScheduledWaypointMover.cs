using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldTime;

[System.Serializable]
public class WaypointSchedule
{
    [Header("Schedule Conditions")]
    public string scheduleName = "Default Route";
    
    [Tooltip("Leave empty to apply to all months")]
    public List<CalendarManager.Month> validMonths = new List<CalendarManager.Month>();
    
    [Tooltip("Leave empty to apply to all days (1-31)")]
    public List<int> validDays = new List<int>();
    
    [Header("Time Window")]
    [Range(0f, 23.99f)]
    public float startHour = 6f;  // 6:00 AM
    
    [Range(0f, 23.99f)]
    public float endHour = 22f;   // 10:00 PM
    
    [Header("Waypoints")]
    public Transform waypointParent;
    public float moveSpeed = 3f;
    public float waitTime = 1.5f;
    public bool loopWaypoints = true;
    
    [Header("Scene Objects")]
    [Tooltip("Objects to enable when this schedule is active")]
    public List<GameObject> objectsToEnable = new List<GameObject>();
    
    [Tooltip("Objects to disable when this schedule is active")]
    public List<GameObject> objectsToDisable = new List<GameObject>();
    
    [Header("Priority")]
    [Tooltip("Higher priority schedules override lower ones when multiple match")]
    public int priority = 0;
}

public class ScheduledWaypointMover : MonoBehaviour
{
    [Header("Schedules")]
    [Tooltip("Add multiple schedules - the best matching one will be used")]
    public List<WaypointSchedule> schedules = new List<WaypointSchedule>();
    
    [Header("Fallback Behavior")]
    [Tooltip("What to do when no schedule is active")]
    public bool hideWhenInactive = false;
    public Vector3 inactivePosition = Vector3.zero;
    
    [Header("Debug")]
    public bool showGizmos = true;
    public bool showDebugLogs = false;

    private WaypointSchedule currentSchedule;
    private Transform[] currentWaypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool isActive = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private float lastDirX = 1f;
    private float lastDirY = 0f;
    
    private Coroutine movementCoroutine;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Initial schedule check
        CheckAndUpdateSchedule();
        
        // Subscribe to time changes
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.OnMonthChanged += OnDateChanged;
            CalendarManager.Instance.OnDayChanged += OnDateChanged;
        }
    }

    void Update()
    {
        // Check schedule every frame (lightweight check)
        CheckAndUpdateSchedule();
        
        // Update animator if active
        if (isActive && animator != null)
        {
            if (isWaiting)
            {
                animator.SetFloat("InputX", 0f);
                animator.SetFloat("InputY", 0f);
                animator.SetFloat("LastInputX", lastDirX);
                animator.SetFloat("LastInputY", lastDirY);
                animator.SetBool("isWalking", false);
            }
            else
            {
                animator.SetFloat("InputX", lastDirX);
                animator.SetFloat("InputY", lastDirY);
                animator.SetFloat("LastInputX", lastDirX);
                animator.SetFloat("LastInputY", lastDirY);
            }
        }
    }

    void OnDateChanged(CalendarManager.Month month)
    {
        CheckAndUpdateSchedule();
    }
    
    void OnDateChanged(int day)
    {
        CheckAndUpdateSchedule();
    }

    void CheckAndUpdateSchedule()
    {
        WaypointSchedule bestSchedule = FindBestSchedule();
        
        if (bestSchedule != currentSchedule)
        {
            if (showDebugLogs)
                Debug.Log($"{gameObject.name}: Switching to schedule '{bestSchedule?.scheduleName ?? "NONE"}'");
            
            SwitchSchedule(bestSchedule);
        }
    }

    WaypointSchedule FindBestSchedule()
    {
        var worldClock = FindObjectOfType<WorldClock>();
        if (worldClock == null || CalendarManager.Instance == null)
            return null;

        float currentHour = GetCurrentTimeOfDay() * 24f;
        var currentMonth = CalendarManager.Instance.CurrentMonth;
        int currentDay = CalendarManager.Instance.CurrentDay;

        WaypointSchedule bestMatch = null;
        int highestPriority = int.MinValue;

        foreach (var schedule in schedules)
        {
            // Check if schedule is valid for current conditions
            if (!IsScheduleValid(schedule, currentMonth, currentDay, currentHour))
                continue;

            // Check priority
            if (schedule.priority > highestPriority)
            {
                highestPriority = schedule.priority;
                bestMatch = schedule;
            }
        }

        return bestMatch;
    }

    bool IsScheduleValid(WaypointSchedule schedule, CalendarManager.Month month, int day, float hour)
    {
        // Check month
        if (schedule.validMonths.Count > 0 && !schedule.validMonths.Contains(month))
            return false;

        // Check day
        if (schedule.validDays.Count > 0 && !schedule.validDays.Contains(day))
            return false;

        // Check time (handle wraparound for overnight schedules)
        if (schedule.startHour <= schedule.endHour)
        {
            // Normal time range (e.g., 6 AM to 10 PM)
            if (hour < schedule.startHour || hour >= schedule.endHour)
                return false;
        }
        else
        {
            // Overnight range (e.g., 10 PM to 6 AM)
            if (hour < schedule.startHour && hour >= schedule.endHour)
                return false;
        }

        return true;
    }

    void SwitchSchedule(WaypointSchedule newSchedule)
    {
        // Deactivate old schedule
        if (currentSchedule != null)
        {
            DeactivateScheduleObjects(currentSchedule);
        }

        // Stop current movement
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        currentSchedule = newSchedule;

        if (newSchedule == null)
        {
            // No active schedule
            isActive = false;
            
            if (hideWhenInactive && spriteRenderer != null)
                spriteRenderer.enabled = false;
            
            if (inactivePosition != Vector3.zero)
                transform.position = inactivePosition;
            
            if (animator != null)
                animator.SetBool("isWalking", false);
            
            return;
        }

        // Activate new schedule
        isActive = true;
        
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        ActivateScheduleObjects(newSchedule);

        // Load waypoints
        if (newSchedule.waypointParent != null)
        {
            currentWaypoints = new Transform[newSchedule.waypointParent.childCount];
            for (int i = 0; i < newSchedule.waypointParent.childCount; i++)
                currentWaypoints[i] = newSchedule.waypointParent.GetChild(i);

            if (currentWaypoints.Length > 0)
            {
                // Snap to first waypoint
                transform.position = currentWaypoints[0].position;
                currentWaypointIndex = 0;
                
                // Start movement
                movementCoroutine = StartCoroutine(MoveAlongWaypoints());
            }
        }
    }

    void ActivateScheduleObjects(WaypointSchedule schedule)
    {
        foreach (var obj in schedule.objectsToEnable)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        foreach (var obj in schedule.objectsToDisable)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    void DeactivateScheduleObjects(WaypointSchedule schedule)
    {
        foreach (var obj in schedule.objectsToEnable)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        foreach (var obj in schedule.objectsToDisable)
        {
            if (obj != null) obj.SetActive(true);
        }
    }

    IEnumerator MoveAlongWaypoints()
    {
        while (isActive && currentSchedule != null)
        {
            // Determine next waypoint
            int nextIndex;
            if (currentSchedule.loopWaypoints)
                nextIndex = (currentWaypointIndex + 1) % currentWaypoints.Length;
            else
                nextIndex = currentWaypointIndex + 1;

            // Check if we've reached the end (non-looping)
            if (!currentSchedule.loopWaypoints && nextIndex >= currentWaypoints.Length)
            {
                isWaiting = true;
                yield break;
            }

            Vector3 targetPos = currentWaypoints[nextIndex].position;
            Vector2 delta = targetPos - transform.position;
            UpdateDirection(delta);

            // Move towards target
            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                if (!isActive) yield break;

                float step = currentSchedule.moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
                
                yield return null;
            }

            // Snap to exact position
            transform.position = targetPos;
            currentWaypointIndex = nextIndex;

            // Wait at waypoint
            isWaiting = true;
            yield return new WaitForSeconds(currentSchedule.waitTime);
            isWaiting = false;
        }
    }

    void UpdateDirection(Vector2 delta)
    {
        if (delta.sqrMagnitude < 0.001f)
            return;

        float newDirX = 0f;
        float newDirY = 0f;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            newDirX = Mathf.Sign(delta.x);
            newDirY = 0f;
        }
        else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            newDirX = 0f;
            newDirY = Mathf.Sign(delta.y);
        }
        else
        {
            newDirX = Mathf.Sign(delta.x);
            newDirY = 0f;
        }

        lastDirX = newDirX;
        lastDirY = newDirY;

        if (animator != null)
        {
            animator.SetFloat("InputX", lastDirX);
            animator.SetFloat("InputY", lastDirY);
            animator.SetFloat("LastInputX", lastDirX);
            animator.SetFloat("LastInputY", lastDirY);
            animator.SetBool("isWalking", true);
        }
    }

    float GetCurrentTimeOfDay()
    {
        var worldClock = FindObjectOfType<WorldClock>();
        if (worldClock == null) return 0f;
        
        // Access the clock's current time via reflection or make it public
        // For now, we'll calculate it from the clock text
        var clockField = worldClock.GetType().GetField("currentTimeOfDay", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (clockField != null)
            return (float)clockField.GetValue(worldClock);
        
        return 0f;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || schedules == null) return;

        // Draw all schedule waypoints with different colors
        Color[] colors = { Color.cyan, Color.yellow, Color.magenta, Color.green, Color.red };
        
        for (int s = 0; s < schedules.Count; s++)
        {
            var schedule = schedules[s];
            if (schedule.waypointParent == null) continue;

            Gizmos.color = colors[s % colors.Length];
            
            for (int i = 0; i < schedule.waypointParent.childCount; i++)
            {
                Transform a = schedule.waypointParent.GetChild(i);
                Transform b = schedule.waypointParent.GetChild((i + 1) % schedule.waypointParent.childCount);
                
                Gizmos.DrawLine(a.position, b.position);
                Gizmos.DrawWireSphere(a.position, 0.15f);
            }
        }
    }

    void OnDestroy()
    {
        if (CalendarManager.Instance != null)
        {
            CalendarManager.Instance.OnMonthChanged -= OnDateChanged;
            CalendarManager.Instance.OnDayChanged -= OnDateChanged;
        }
    }
}