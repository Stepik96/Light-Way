using Xunit;

namespace LightWay.Tests;

public class GameEngineInteractionTests
{
    [Fact]
    public void ClickOnEmptyCell_DoesNotSpendMoves()
    {
        var map = new int[Level.GridSize, Level.GridSize];
        map[1, 0] = (int)CellType.Source;
        map[1, 2] = (int)CellType.MirrorLeft;
        map[0, 2] = (int)CellType.Receiver;

        var level = Level.FromDigitMap(map, 1, 0, Direction.Right, maxMirrorClicks: 5, timeLimitSeconds: 0);
        var engine = new GameEngine(level, 1);

        engine.HandleCellClick(5, 5);
        Assert.Equal(0, engine.MirrorRotationsUsed);
    }

    [Fact]
    public void NextLevel_OnLast_ReturnsFalse()
    {
        var engine = new GameEngine(Level.LoadLevel(5), 5);
        Assert.False(engine.NextLevel());
        Assert.Equal(5, engine.CurrentLevelNumber);
    }
}
