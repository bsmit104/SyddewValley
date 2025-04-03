using UnityEngine;
using TMPro;
using System;

namespace WorldTime
{
    public class CalendarManager : MonoBehaviour
    {
        public static CalendarManager Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI dateText;

        [Header("Calendar Controls")]
        [SerializeField] private bool useInspectorControls = false;
        [SerializeField] private Month inspectorMonth = Month.Augtomber;
        [SerializeField] private int inspectorDay = 1;
        [SerializeField] private bool applyInspectorDateOnStart = true;

        public bool UseInspectorControls => useInspectorControls;

        public enum Month
        {
            Augtomber,
            Novecanuary,
            Febmapril,
            Mayunly
        }

        public Month CurrentMonth => currentMonth;
        public int CurrentDay => currentDay;

        private Month currentMonth = Month.Augtomber;
        private int currentDay = 1;
        private int daysInMonth = 28;
        private bool initialDateApplied = false;

        public event Action<Month> OnMonthChanged;
        public event Action<int> OnDayChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (useInspectorControls && applyInspectorDateOnStart && !initialDateApplied)
            {
                SetDate(inspectorMonth, inspectorDay);
                initialDateApplied = true;
            }
            UpdateDateDisplay();
        }

        private void Update()
        {
            // Only apply inspector date once at start
            if (useInspectorControls && applyInspectorDateOnStart && !initialDateApplied)
            {
                SetDate(inspectorMonth, inspectorDay);
                initialDateApplied = true;
            }
        }

        public void SetDate(Month month, int day)
        {
            currentMonth = month;
            currentDay = Mathf.Clamp(day, 1, GetDaysInMonth(month));
            UpdateDateDisplay();
            OnMonthChanged?.Invoke(currentMonth);
            OnDayChanged?.Invoke(currentDay);
        }

        public void AdvanceDay()
        {
            currentDay++;
            
            if (currentDay > daysInMonth)
            {
                currentDay = 1;
                Month previousMonth = currentMonth;
                currentMonth = (Month)(((int)currentMonth + 1) % 4);
                daysInMonth = GetDaysInMonth(currentMonth);
                
                if (previousMonth != currentMonth)
                {
                    OnMonthChanged?.Invoke(currentMonth);
                }
            }

            OnDayChanged?.Invoke(currentDay);
            UpdateDateDisplay();
        }

        private void UpdateDateDisplay()
        {
            if (dateText != null)
            {
                string daySuffix = GetDaySuffix(currentDay);
                dateText.text = $"{currentMonth} {currentDay}{daySuffix}";
            }
        }

        private string GetDaySuffix(int day)
        {
            if (day >= 11 && day <= 13)
                return "th";

            switch (day % 10)
            {
                case 1: return "st";
                case 2: return "nd";
                case 3: return "rd";
                default: return "th";
            }
        }

        private int GetDaysInMonth(Month month)
        {
            switch (month)
            {
                case Month.Augtomber: return 28;
                case Month.Novecanuary: return 30;
                case Month.Febmapril: return 29;
                case Month.Mayunly: return 31;
                default: return 28;
            }
        }
    }
} 