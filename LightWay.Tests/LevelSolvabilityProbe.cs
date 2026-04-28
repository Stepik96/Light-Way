using System.Collections.Generic;
using Xunit;

namespace LightWay.Tests;

/// <summary>
/// Проверка, что у встроенного уровня есть хотя бы одна комбинация зеркал с победой.
/// </summary>
public class LevelSolvabilityProbe
{
    public static bool HasWinningConfiguration(int levelNumber, out int minFlipsFromDefault)
    {
        minFlipsFromDefault = int.MaxValue;
        Level template = Level.LoadLevel(levelNumber);
        var mirrors = new List<(int r, int c)>();
        for (int r = 0; r < Level.GridSize; r++)
        {
            for (int c = 0; c < Level.GridSize; c++)
            {
                if (template.Grid[r, c].IsMirror)
                    mirrors.Add((r, c));
            }
        }

        int n = mirrors.Count;
        bool any = false;

        for (int mask = 0; mask < (1 << n); mask++)
        {
            Level trial = Level.LoadLevel(levelNumber);
            for (int i = 0; i < n; i++)
            {
                if (((mask >> i) & 1) == 1)
                    trial.Grid[mirrors[i].r, mirrors[i].c].RotateMirror();
            }

            var eng = new GameEngine(trial, levelNumber);
            if (eng.IsLevelComplete)
            {
                any = true;
                int flips = PopCount(mask);
                if (flips < minFlipsFromDefault)
                    minFlipsFromDefault = flips;
            }
        }

        if (minFlipsFromDefault == int.MaxValue)
            minFlipsFromDefault = -1;
        return any;
    }

    private static int PopCount(int x)
    {
        int c = 0;
        while (x != 0)
        {
            c++;
            x &= x - 1;
        }
        return c;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void BuiltinLevel_HasWinningMirrorConfiguration(int n)
    {
        bool ok = HasWinningConfiguration(n, out int minFlips);
        Assert.True(ok, $"Level {n} has no winning mirror layout (min flips would be {minFlips})");
        Assert.True(minFlips >= 0);

        Level lvl = Level.LoadLevel(n);
        Assert.True(
            minFlips <= lvl.MaxMirrorClicks,
            $"Level {n}: решение требует минимум {minFlips} поворотов, а лимит {lvl.MaxMirrorClicks}");
    }
}
