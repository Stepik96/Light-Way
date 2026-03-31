using System;
using System.Drawing;
using System.Windows.Forms;

namespace LightWay
{
    /// <summary>
    /// Главная форма игры.
    /// На данном этапе: отрисовка игрового поля 10×10.
    /// </summary>
    public class GameForm : Form
    {
        // Константы отрисовки
        private const int GridSize  = 10;   // Количество клеток по горизонтали и вертикали
        private const int CellSize  = 56;   // Размер одной клетки в пикселях
        private const int GridOffset = 20;  // Отступ сетки от края окна

        public GameForm()
        {
            // Размер окна = размер сетки + отступы с обеих сторон
            int gridPixels = GridSize * CellSize;
            this.Text             = "Light Way — Световой путь";
            this.ClientSize       = new Size(gridPixels + GridOffset * 2,
                                             gridPixels + GridOffset * 2);
            this.FormBorderStyle  = FormBorderStyle.FixedSingle;
            this.MaximizeBox      = false;
            this.BackColor        = Color.FromArgb(20, 20, 35);
            this.StartPosition    = FormStartPosition.CenterScreen;

            // Двойная буферизация — убирает мерцание при перерисовке
            this.DoubleBuffered = true;
        }

        /// <summary>
        /// Отрисовка игрового поля.
        /// Вызывается автоматически при каждом обновлении окна.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            DrawGrid(g);
        }

        /// <summary>
        /// Рисует сетку 10×10: фон каждой клетки и её рамку.
        /// </summary>
        private void DrawGrid(Graphics g)
        {
            Color colorCell     = Color.FromArgb(28, 28, 48);  // Цвет фона клетки
            Color colorGridLine = Color.FromArgb(40, 50, 80);  // Цвет линий сетки

            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    // Вычисляем пиксельные координаты левого верхнего угла клетки
                    int x = GridOffset + col * CellSize;
                    int y = GridOffset + row * CellSize;

                    // Заливка фона клетки
                    using (var brush = new SolidBrush(colorCell))
                        g.FillRectangle(brush, x, y, CellSize, CellSize);

                    // Рамка клетки
                    using (var pen = new Pen(colorGridLine, 1))
                        g.DrawRectangle(pen, x, y, CellSize, CellSize);
                }
            }
        }
    }
}
