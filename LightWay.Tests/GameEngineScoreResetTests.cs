using Xunit;

namespace LightWay.Tests;

public class GameEngineScoreResetTests
{
    [Fact]
    public void LossByMoves_ResetsTotalScore_ToZero()
    {
        var loseMap = new int[Level.GridSize, Level.GridSize];
        loseMap[0, 0] = (int)CellType.Source;
        loseMap[0, 3] = (int)CellType.MirrorRight;
        loseMap[0, 9] = (int)CellType.Receiver;

        var loseLevel = Level.FromDigitMap(loseMap, 0, 0, Direction.Right, maxMirrorClicks: 1, timeLimitSeconds: 0);
        var engine = new GameEngine(loseLevel, 1);
        engine.TestingSetTotalScore(999);
        engine.HandleCellClick(0, 3);

        Assert.True(engine.IsLevelFailed);
        Assert.Equal(0, engine.TotalScore);
    }

    [Fact]
    public void LossByTimer_ResetsTotalScore_ToZero()
    {
        var map = new int[Level.GridSize, Level.GridSize];
        map[1, 0] = (int)CellType.Source;
        map[1, 2] = (int)CellType.MirrorRight;
        map[1, 9] = (int)CellType.Receiver;

        var level = Level.FromDigitMap(map, 1, 0, Direction.Right, maxMirrorClicks: 20, timeLimitSeconds: 1);
        var engine = new GameEngine(level, 1);
        engine.TestingSetTotalScore(500);
        engine.TickTimer();

        Assert.True(engine.IsLevelFailed);
        Assert.Equal(0, engine.TotalScore);
    }
}
