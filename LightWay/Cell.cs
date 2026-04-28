using System;

namespace LightWay
{
    // Тип объекта на клетке
    public enum CellType
    {
        Empty,      // Пустая клетка
        Source,     // Источник света
        Receiver,   // Приёмник (цель)
        MirrorLeft, // Зеркало "\" — отражает вниз/вправо
        MirrorRight // Зеркало "/" — отражает вверх/вправо
    }

    // Направление движения луча
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Модель одной клетки игрового поля.
    /// Хранит тип объекта, координаты и состояние подсветки лучом.
    /// </summary>
    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public CellType Type { get; set; }

        // Подсвечена ли клетка лучом (для визуализации)
        public bool IsLit { get; set; }

        public Cell(int row, int col, CellType type = CellType.Empty)
        {
            Row = row;
            Col = col;
            Type = type;
            IsLit = false;
        }

        /// <summary>
        /// Повернуть зеркало на 90° (переключить \ на / и обратно).
        /// </summary>
        public void RotateMirror()
        {
            if (Type == CellType.MirrorLeft)
                Type = CellType.MirrorRight;
            else if (Type == CellType.MirrorRight)
                Type = CellType.MirrorLeft;
        }

        /// <summary>
        /// Является ли клетка зеркалом?
        /// </summary>
        public bool IsMirror => Type == CellType.MirrorLeft || Type == CellType.MirrorRight;
    }
}
