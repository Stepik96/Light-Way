using Xunit;

namespace LightWay.Tests;

public class CellTests
{
    [Fact]
    public void RotateMirror_SwapsSlashAndBackslash()
    {
        var cell = new Cell(0, 0, CellType.MirrorLeft);
        cell.RotateMirror();
        Assert.Equal(CellType.MirrorRight, cell.Type);
        cell.RotateMirror();
        Assert.Equal(CellType.MirrorLeft, cell.Type);
    }

    [Fact]
    public void IsMirror_TrueOnlyForMirrors()
    {
        Assert.False(new Cell(0, 0, CellType.Empty).IsMirror);
        Assert.False(new Cell(0, 0, CellType.Source).IsMirror);
        Assert.True(new Cell(0, 0, CellType.MirrorLeft).IsMirror);
        Assert.True(new Cell(0, 0, CellType.MirrorRight).IsMirror);
    }
}
