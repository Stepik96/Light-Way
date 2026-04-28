using Xunit;

namespace LightWay.Tests;

public class GameScoreTests
{
    [Fact]
    public void MoreSpareClicks_GivesHigherScore_WithoutTimer()
    {
        int lowClicks = GameScore.Calculate(maxClicks: 10, clicksUsed: 9, timeLimitSeconds: 0, elapsedSeconds: 0);
        int highClicks = GameScore.Calculate(maxClicks: 10, clicksUsed: 2, timeLimitSeconds: 0, elapsedSeconds: 0);
        Assert.True(highClicks > lowClicks);
    }

    [Fact]
    public void UnusedTime_AddsBonus_WhenTimerEnabled()
    {
        int withTimeLeft = GameScore.Calculate(10, 3, timeLimitSeconds: 40, elapsedSeconds: 10);
        int noTimer = GameScore.Calculate(10, 3, timeLimitSeconds: 0, elapsedSeconds: 10);
        Assert.True(withTimeLeft > noTimer);
    }

    [Fact]
    public void Score_IsNeverBelowMinimumFloor()
    {
        int brutal = GameScore.Calculate(2, 50, 0, 0);
        Assert.True(brutal >= 10);
    }
}
