using System;

namespace LightWay
{
    /// <summary>
    /// Модель уровня. Хранит сетку 10×10, координаты источника
    /// и начальное направление луча. Содержит фабрику уровней.
    /// </summary>
    public class Level
    {
        public const int GridSize = 10;

        public Cell[,] Grid { get; private set; }

        // Координаты источника света
        public int SourceRow { get; private set; }
        public int SourceCol { get; private set; }

        // Начальное направление луча из источника
        public Direction StartDirection { get; private set; }

        /// <summary>
        /// Сколько раз можно повернуть зеркало за попытку. Если лимит исчерпан
        /// и луч ещё не в приёмнике — проигрыш.
        /// </summary>
        public int MaxMirrorClicks { get; private set; }

        /// <summary>
        /// Лимит времени в секундах. Ноль — таймер выключен.
        /// </summary>
        public int TimeLimitSeconds { get; private set; }

        public Level(Cell[,] grid, int sourceRow, int sourceCol, Direction startDirection,
            int maxMirrorClicks = 12, int timeLimitSeconds = 0)
        {
            Grid = grid;
            SourceRow = sourceRow;
            SourceCol = sourceCol;
            StartDirection = startDirection;
            MaxMirrorClicks = Math.Max(1, maxMirrorClicks);
            TimeLimitSeconds = Math.Max(0, timeLimitSeconds);
        }

        /// <summary>
        /// Сбросить подсветку всех клеток (перед пересчётом луча).
        /// </summary>
        public void ClearLit()
        {
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    Grid[r, c].IsLit = false;
        }

        // ========== Фабрика уровней ==========

        /// <summary>
        /// Загрузить уровень по номеру. Пока реализован 1 уровень,
        /// остальные добавляются аналогично.
        /// Легенда массива:
        ///   0 = Empty, 1 = Source, 2 = Receiver,
        ///   3 = MirrorLeft (\), 4 = MirrorRight (/)
        /// </summary>
        public static Level LoadLevel(int levelNumber)
        {
            switch (levelNumber)
            {
                case 1: return BuildLevel1();
                case 2: return BuildLevel2();
                case 3: return BuildLevel3();
                case 4: return BuildLevel4();
                case 5: return BuildLevel5();
                default: return BuildLevel1();
            }
        }

        public static int TotalLevels => 5;

        // --- Уровень 1: простой путь вправо-вниз ---
        // Источник (0,0), луч идёт вправо.
        // Зеркало \ на (0,4) — отражает вниз.
        // Зеркало \ на (5,4) — ошибочное (отвлекающий манёвр).
        // Зеркало / на (3,4) — отражает вправо.
        // Приёмник на (3,8).
        private static Level BuildLevel1()
        {
            int[,] map = new int[GridSize, GridSize];

            // Источник
            map[0, 0] = 1;
            // Зеркало \ — луч вправо превращается в вниз
            map[0, 4] = 3;
            // Зеркало / — луч вниз превращается в вправо
            map[3, 4] = 4;
            // Приёмник
            map[3, 8] = 2;

            return BuildFromMap(map, 0, 0, Direction.Right, maxMirrorClicks: 15, timeLimitSeconds: 0);
        }

        // --- Уровень 2: зигзаг ---
        private static Level BuildLevel2()
        {
            int[,] map = new int[GridSize, GridSize];

            map[0, 0] = 1;  // Источник
            map[0, 5] = 3;  // \ → вниз
            map[4, 5] = 4;  // / → влево
            map[4, 2] = 3;  // \ → ... нужно повернуть
            map[7, 2] = 4;  // / → вправо (нужно повернуть)
            map[7, 8] = 2;  // Приёмник

            return BuildFromMap(map, 0, 0, Direction.Right, maxMirrorClicks: 14, timeLimitSeconds: 38);
        }

        // --- Уровень 3: «ломаная» из четырёх зеркал, решение — все «\» ---
        // Старт: везде «/», чтобы луч уходил не туда; хватает четырёх правильных поворотов.
        private static Level BuildLevel3()
        {
            int[,] map = new int[GridSize, GridSize];

            map[0, 0] = 1;
            map[0, 4] = 4;
            map[3, 4] = 4;
            map[3, 7] = 4;
            map[6, 7] = 4;
            map[6, 9] = 2;

            return BuildFromMap(map, 0, 0, Direction.Right, maxMirrorClicks: 13, timeLimitSeconds: 32);
        }

        // --- Уровень 4: змейка из пяти зеркал (сложнее третьего) ---
        private static Level BuildLevel4()
        {
            int[,] map = new int[GridSize, GridSize];

            map[1, 0] = 1;
            map[1, 4] = 4;
            map[4, 4] = 4;
            map[4, 8] = 4;
            map[8, 8] = 4;
            map[8, 9] = 2;

            return BuildFromMap(map, 1, 0, Direction.Right, maxMirrorClicks: 11, timeLimitSeconds: 26);
        }

        // --- Уровень 5: другая «змейка» (6 зеркал), финиш (8,9); не совпадает с уровнем 4 ---
        // Решение: все зеркала «\», если стартовали как «/».
        private static Level BuildLevel5()
        {
            int[,] map = new int[GridSize, GridSize];

            map[0, 0] = 1;
            map[0, 2] = 4;
            map[2, 2] = 4;
            map[2, 5] = 4;
            map[5, 5] = 4;
            map[5, 8] = 4;
            map[8, 8] = 4;
            map[8, 9] = 2;

            return BuildFromMap(map, 0, 0, Direction.Right, maxMirrorClicks: 11, timeLimitSeconds: 20);
        }

        /// <summary>
        /// Собрать уровень из карты 10×10 (цифры как в комментариях к LoadLevel).
        /// Вынесено отдельно, чтобы удобно собирать поля в unit-тестах.
        /// </summary>
        public static Level FromDigitMap(int[,] map, int srcRow, int srcCol, Direction dir,
            int maxMirrorClicks = 10, int timeLimitSeconds = 0)
        {
            if (map.GetLength(0) != GridSize || map.GetLength(1) != GridSize)
                throw new ArgumentException("Карта должна быть размером 10×10.");

            var grid = new Cell[GridSize, GridSize];

            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    CellType type = (CellType)map[r, c];
                    grid[r, c] = new Cell(r, c, type);
                }
            }

            return new Level(grid, srcRow, srcCol, dir, maxMirrorClicks, timeLimitSeconds);
        }

        private static Level BuildFromMap(int[,] map, int srcRow, int srcCol, Direction dir,
            int maxMirrorClicks, int timeLimitSeconds)
        {
            return FromDigitMap(map, srcRow, srcCol, dir, maxMirrorClicks, timeLimitSeconds);
        }
    }
}
