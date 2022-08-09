using System;

public static class Utilities
{
    public static string CovertTimeToString(int time)
    {
        TimeSpan converted = TimeSpan.FromSeconds(time);
        return converted.ToString("mm':'ss");

    }
}
