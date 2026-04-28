using System;

namespace LightWay
{
    /// <summary>
    /// Подсчёт очков за уровень.
    /// Идея простая: чем больше «запаса» по ходам осталось и чем быстрее уложился игрок,
    /// тем выше число очков.
    /// </summary>
    public static class GameScore
    {
        public static int Calculate(int maxClicks, int clicksUsed, int timeLimitSeconds, int elapsedSeconds)
        {
            clicksUsed = Math.Max(0, clicksUsed);
            maxClicks = Math.Max(1, maxClicks);

            int unusedClicks = Math.Max(0, maxClicks - clicksUsed);
            int score = 50 + unusedClicks * 15;
            if (score < 10)
                score = 10;

            if (timeLimitSeconds > 0)
            {
                int secondsLeft = Math.Max(0, timeLimitSeconds - elapsedSeconds);
                score += secondsLeft * 3;
            }

            return score;
        }
    }
}
