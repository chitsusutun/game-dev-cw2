using UnityEngine;

public static class PlayerStats
{
    public static int status = 0;
    public static int pride = 20;
    public static int rivalStatus = 0;

    public static void IncreasePride(int amount)
    {
        pride = Mathf.Clamp(pride + amount, 0, 100);
    }

    public static void DecreasePride(int amount)
    {
        pride = Mathf.Clamp(pride - amount, 0, 100);
    }

    public static void IncreaseStatus(int amount)
    {
        status = Mathf.Clamp(status + amount, 0, 100);
    }

    public static void IncreaseRivalStatus(int amount)
    {
        rivalStatus = Mathf.Clamp(rivalStatus + amount, 0, 100);
    }

    // Call this on Main Menu or Restart button so replays start fresh
    public static void ResetStats()
    {
        status = 0;
        pride = 20;
        rivalStatus = 0;
    }
}