// File: DayNightSettings.cs
using UnityEngine;

[CreateAssetMenu(menuName = "World Time/Day-Night Settings", fileName = "DayNightSettings")]
public class DayNightSettings : ScriptableObject
{
    public Gradient gradient = new Gradient();
    public float dayDurationInSeconds = 300f;
}