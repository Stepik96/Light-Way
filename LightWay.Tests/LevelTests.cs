using System;
using Xunit;

namespace LightWay.Tests;

public class LevelTests
{
    [Fact]
    public void TotalLevels_IsFive()
    {
        Assert.Equal(5, Level.TotalLevels);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void LoadLevel_ReturnsNonNull_ForBuiltInLevels(int n)
    {
        var level = Level.LoadLevel(n);
        Assert.NotNull(level.Grid);
        Assert.True(level.MaxMirrorClicks > 0);
        Assert.True(level.TimeLimitSeconds >= 0);
    }

    [Fact]
    public void FromDigitMap_Throws_WhenNot10x10()
    {
        var bad = new int[3, 3];
        Assert.Throws<ArgumentException>(() =>
            Level.FromDigitMap(bad, 0, 0, Direction.Right));
    }
}
