using System.Drawing;
using Xunit;

namespace LightWay.Tests;

public class GameEngineTests
{
    [Fact]
    public void StraightPath_ReachesReceiver_OnLoad()
    {
        var map = EmptyMap();
        map[2, 1] = (int)CellType.Source;
        map[2, 8] = (int)CellType.Receiver;

        var level = Level.FromDigitMap(map, 2, 1, Direction.Right, maxMirrorClicks: 5, timeLimitSeconds: 0);
        var engine = new GameEngine(level, 1);

        Assert.True(engine.IsLevelComplete);
        Assert.Contains(new Point(8, 2), engine.BeamPath);
    }

    [Fact]
    public void MirrorBlock_StopsWin_UntilClickExhausted()
    {
        var map = EmptyMap();
        map[0, 0] = (int)CellType.Source;
        map[0, 3] = (int)CellType.MirrorRight; // «/» — луч уйдёт вверх, до приёмника не дойдёт
        map[0, 9] = (int)CellType.Receiver;

        var level = Level.FromDigitMap(map, 0, 0, Direction.Right, maxMirrorClicks: 1, timeLimitSeconds: 0);
        var engine = new GameEngine(level, 1);

        Assert.False(engine.IsLevelComplete);
        engine.HandleCellClick(0, 3);
        Assert.True(engine.IsLevelFailed);
        Assert.False(engine.IsLevelComplete);
    }

    [Fact]
    public void TimerRunsOut_FailsLevel()
    {
        var map = EmptyMap();
        map[1, 0] = (int)CellType.Source;
        map[1, 2] = (int)CellType.MirrorRight; // луч уходит со строки
        map[1, 9] = (int)CellType.Receiver;

        var level = Level.FromDigitMap(map, 1, 0, Direction.Right, maxMirrorClicks: 20, timeLimitSeconds: 1);
        var engine = new GameEngine(level, 1);
        Assert.False(engine.IsLevelComplete);

        engine.TickTimer();
        Assert.True(engine.IsLevelFailed);
    }

    [Fact]
    public void WinAfterMirrorClick_AddsScore()
    {
        var map = EmptyMap();
        map[1, 0] = (int)CellType.Source;
        map[1, 3] = (int)CellType.MirrorLeft; // «\» — пускает луч вниз, до цели не доберётся
        map[0, 3] = (int)CellType.Receiver;

        var level = Level.FromDigitMap(map, 1, 0, Direction.Right, maxMirrorClicks: 5, timeLimitSeconds: 0);
        var engine = new GameEngine(level, 1);
        Assert.False(engine.IsLevelComplete);

        engine.HandleCellClick(1, 3); // становится «/» — луч отражается вверх в приёмник

        Assert.True(engine.IsLevelComplete);
        Assert.True(engine.LastLevelScore > 0);
        Assert.True(engine.TotalScore > 0);
    }

    [Fact]
    public void ResetLevel_ClearsFail_AndMirrorMoves()
    {
        var map = EmptyMap();
        map[0, 0] = (int)CellType.Source;
        map[0, 3] = (int)CellType.MirrorRight;
        map[0, 9] = (int)CellType.Receiver;

        var level = Level.FromDigitMap(map, 0, 0, Direction.Right, maxMirrorClicks: 1, timeLimitSeconds: 0);
        var engine = new GameEngine(level, 1);
        engine.HandleCellClick(0, 3);
        Assert.True(engine.IsLevelFailed);

        engine.ResetLevel();
        Assert.False(engine.IsLevelFailed);
        Assert.Equal(0, engine.MirrorRotationsUsed);
    }

    [Fact]
    public void ClicksIgnored_WhenLevelAlreadyFailed()
    {
        var map = EmptyMap();
        map[0, 0] = (int)CellType.Source;
        map[0, 3] = (int)CellType.MirrorRight;
        map[0, 9] = (int)CellType.Receiver;

        var level = Level.FromDigitMap(map, 0, 0, Direction.Right, maxMirrorClicks: 1, timeLimitSeconds: 0);
        var engine = new GameEngine(level, 1);
        engine.HandleCellClick(0, 3);
        Assert.True(engine.IsLevelFailed);

        int used = engine.MirrorRotationsUsed;
        engine.HandleCellClick(0, 3);
        Assert.Equal(used, engine.MirrorRotationsUsed);
    }

    [Fact]
    public void NextLevel_IncrementsNumber_WhenPossible()
    {
        var map = EmptyMap();
        map[2, 1] = (int)CellType.Source;
        map[2, 8] = (int)CellType.Receiver;
        var level = Level.FromDigitMap(map, 2, 1, Direction.Right, 5, 0);
        var engine = new GameEngine(level, 1);

        bool moved = engine.NextLevel();
        Assert.True(moved);
        Assert.Equal(2, engine.CurrentLevelNumber);
    }

    private static int[,] EmptyMap()
    {
        var map = new int[Level.GridSize, Level.GridSize];
        return map;
    }
}
