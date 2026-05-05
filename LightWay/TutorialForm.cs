using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LightWay
{
    /// <summary>
    /// Окно обучения. Использует свой собственный GameEngine с уровнем
    /// Level.LoadTutorial(). На очки игры не влияет — это отдельный
    /// экземпляр движка, общий счёт здесь не учитывается.
    /// </summary>
    public class TutorialForm : Form
    {
        private GameEngine _engine;

        // Размеры отрисовки — как в GameForm
        private const int CellSize = 46;
        private const int GridOffset = 20;
        private const int GridPixels = Level.GridSize * CellSize; // 460
        private const int HintsPanelLeft = GridOffset + GridPixels + 20; // 500
        private const int HintsPanelWidth = 280;

        // Цвета — те же, что в GameForm
        private readonly Color _colorBackground  = Color.FromArgb(20, 20, 35);
        private readonly Color _colorGridLine    = Color.FromArgb(40, 50, 80);
        private readonly Color _colorCellDefault = Color.FromArgb(28, 28, 48);
        private readonly Color _colorCellLit     = Color.FromArgb(45, 45, 70);
        private readonly Color _colorSource      = Color.FromArgb(255, 220, 50);
        private readonly Color _colorReceiver    = Color.FromArgb(50, 200, 50);
        private readonly Color _colorReceiverLit = Color.FromArgb(100, 255, 100);
        private readonly Color _colorMirror      = Color.FromArgb(140, 180, 220);
        private readonly Color _colorBeam        = Color.FromArgb(255, 255, 100);
        private readonly Color _colorBeamGlow    = Color.FromArgb(80, 255, 255, 100);
        private readonly Color _colorTextDim     = Color.FromArgb(180, 180, 200);
        private readonly Color _colorAccent      = Color.FromArgb(255, 220, 50);

        // Подсказки и динамические надписи
        private Label _lblTitle = null!;
        private Label _lblMoves = null!;
        private Label _lblTime = null!;
        private Label _lblStatus = null!;
        private Button _btnRetry = null!;
        private Button _btnMenu = null!;

        private readonly System.Windows.Forms.Timer _gameTimer = new System.Windows.Forms.Timer();

        public TutorialForm()
        {
            var level = Level.LoadTutorial();
            _engine = new GameEngine(level, 0);

            InitializeForm();
            InitializeControls();

            _engine.StateChanged += OnStateChanged;
            _engine.LevelCompleted += OnLevelCompleted;
            _engine.LevelFailed += OnLevelFailed;

            _gameTimer.Interval = 1000;
            _gameTimer.Tick += (s, e) =>
            {
                _engine.TickTimer();
                Invalidate();
            };
            _gameTimer.Enabled = true;

            UpdateLabels();
        }

        private void InitializeForm()
        {
            this.Text = "Light Way — Обучение";
            this.ClientSize = new Size(
                HintsPanelLeft + HintsPanelWidth + GridOffset,
                GridPixels + GridOffset * 2
            );
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = _colorBackground;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Двойная буферизация — устранение мерцания
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );
        }

        private void InitializeControls()
        {
            int x = HintsPanelLeft;
            int y = GridOffset;

            // Заголовок панели подсказок
            _lblTitle = new Label
            {
                Text = "Обучение",
                ForeColor = _colorAccent,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(x, y),
                Size = new Size(HintsPanelWidth, 28),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            this.Controls.Add(_lblTitle);
            y += 36;

            // Подсказки — всегда видны
            string[] hints = {
                "☀ Источник — отсюда выходит луч",
                "🎯 Приёмник — доведи луч до него",
                "Клик по зеркалу — поворачивает его (\\ ↔ /)",
                "⬛ Стена — луч не проходит сквозь неё",
                "⏱ Следи за временем и ходами!",
                "💡 Цель: довести луч от ☀ до 🎯",
            };
            for (int i = 0; i < hints.Length; i++)
            {
                var lbl = new Label
                {
                    Text = hints[i],
                    ForeColor = _colorTextDim,
                    Font = new Font("Segoe UI", 9),
                    Location = new Point(x, y),
                    Size = new Size(HintsPanelWidth, 34),
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                this.Controls.Add(lbl);
                y += 36;
            }
            y += 8;

            // Динамические показатели — ходы и время
            _lblMoves = new Label
            {
                Text = "Ходы: 0 / 0",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(x, y),
                Size = new Size(HintsPanelWidth, 22),
            };
            this.Controls.Add(_lblMoves);
            y += 24;

            _lblTime = new Label
            {
                Text = "Время: 0 с",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(x, y),
                Size = new Size(HintsPanelWidth, 22),
            };
            this.Controls.Add(_lblTime);
            y += 30;

            // Статус — победа/поражение/в процессе
            _lblStatus = new Label
            {
                Text = "Кликай по зеркалам, чтобы повернуть их",
                ForeColor = _colorTextDim,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(x, y),
                Size = new Size(HintsPanelWidth, 56),
                TextAlign = ContentAlignment.TopLeft,
            };
            this.Controls.Add(_lblStatus);
            y += 64;

            // Кнопка «В меню»
            _btnMenu = new Button
            {
                Text = "В меню",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 55, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Size = new Size(HintsPanelWidth / 2 - 4, 36),
                Location = new Point(x, y),
                Cursor = Cursors.Hand,
            };
            _btnMenu.FlatAppearance.BorderColor = Color.FromArgb(80, 85, 120);
            _btnMenu.Click += (s, e) =>
            {
                _gameTimer.Stop();
                this.Close();
            };
            this.Controls.Add(_btnMenu);

            // Кнопка «Повторить» — появляется только после проигрыша
            _btnRetry = new Button
            {
                Text = "Повторить",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 100, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Size = new Size(HintsPanelWidth / 2 - 4, 36),
                Location = new Point(x + HintsPanelWidth / 2 + 4, y),
                Cursor = Cursors.Hand,
                Visible = false,
            };
            _btnRetry.FlatAppearance.BorderColor = Color.FromArgb(60, 140, 80);
            _btnRetry.Click += (s, e) => Retry();
            this.Controls.Add(_btnRetry);
        }

        /// <summary>
        /// Перезапустить уровень обучения с нуля.
        /// Создаём новый движок с свежим уровнем.
        /// </summary>
        private void Retry()
        {
            _engine.StateChanged -= OnStateChanged;
            _engine.LevelCompleted -= OnLevelCompleted;
            _engine.LevelFailed -= OnLevelFailed;

            var level = Level.LoadTutorial();
            _engine = new GameEngine(level, 0);

            _engine.StateChanged += OnStateChanged;
            _engine.LevelCompleted += OnLevelCompleted;
            _engine.LevelFailed += OnLevelFailed;

            _btnRetry.Visible = false;
            _gameTimer.Enabled = true;
            _lblStatus.Text = "Кликай по зеркалам, чтобы повернуть их";
            _lblStatus.ForeColor = _colorTextDim;
            UpdateLabels();
            Invalidate();
        }

        private void OnStateChanged()
        {
            UpdateLabels();
            Invalidate();
        }

        private void OnLevelCompleted()
        {
            _gameTimer.Stop();
            _lblStatus.Text = "Отлично! Ты понял как играть. Возвращайся в меню!";
            _lblStatus.ForeColor = _colorReceiverLit;
            _btnRetry.Visible = false;
            Invalidate();
        }

        private void OnLevelFailed()
        {
            _gameTimer.Stop();
            _lblStatus.Text = "Попробуй ещё раз — анализируй путь луча!";
            _lblStatus.ForeColor = Color.FromArgb(255, 120, 120);
            _btnRetry.Visible = true;
            Invalidate();
        }

        private void UpdateLabels()
        {
            var lvl = _engine.CurrentLevel;
            _lblMoves.Text = $"Ходы: {_engine.MirrorRotationsUsed} / {lvl.MaxMirrorClicks}";

            int rem = _engine.SecondsRemaining ?? 0;
            _lblTime.Text = $"Время: {rem} с";
        }

        // ============== ОТРИСОВКА (скопировано из GameForm) ==============

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawGrid(g);
            DrawBeam(g);
            DrawObjects(g);
        }

        private void DrawGrid(Graphics g)
        {
            var level = _engine.CurrentLevel;

            for (int r = 0; r < Level.GridSize; r++)
            {
                for (int c = 0; c < Level.GridSize; c++)
                {
                    int x = GridOffset + c * CellSize;
                    int y = GridOffset + r * CellSize;

                    Color bgColor = level.Grid[r, c].IsLit ? _colorCellLit : _colorCellDefault;
                    using (var brush = new SolidBrush(bgColor))
                        g.FillRectangle(brush, x, y, CellSize, CellSize);

                    using (var pen = new Pen(_colorGridLine, 1))
                        g.DrawRectangle(pen, x, y, CellSize, CellSize);
                }
            }
        }

        private void DrawBeam(Graphics g)
        {
            var path = _engine.BeamPath;
            if (path.Count < 2) return;

            using (var glowPen = new Pen(_colorBeamGlow, 8))
            {
                glowPen.StartCap = LineCap.Round;
                glowPen.EndCap = LineCap.Round;
                glowPen.LineJoin = LineJoin.Round;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    PointF from = CellCenter(path[i].Y, path[i].X);
                    PointF to = CellCenter(path[i + 1].Y, path[i + 1].X);
                    g.DrawLine(glowPen, from, to);
                }
            }

            using (var beamPen = new Pen(_colorBeam, 3))
            {
                beamPen.StartCap = LineCap.Round;
                beamPen.EndCap = LineCap.Round;
                beamPen.LineJoin = LineJoin.Round;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    PointF from = CellCenter(path[i].Y, path[i].X);
                    PointF to = CellCenter(path[i + 1].Y, path[i + 1].X);
                    g.DrawLine(beamPen, from, to);
                }
            }
        }

        private void DrawObjects(Graphics g)
        {
            var level = _engine.CurrentLevel;

            for (int r = 0; r < Level.GridSize; r++)
            {
                for (int c = 0; c < Level.GridSize; c++)
                {
                    Cell cell = level.Grid[r, c];
                    int x = GridOffset + c * CellSize;
                    int y = GridOffset + r * CellSize;
                    int pad = 8;

                    switch (cell.Type)
                    {
                        case CellType.Source:
                            DrawSource(g, x, y, pad);
                            break;
                        case CellType.Receiver:
                            DrawReceiver(g, x, y, pad, cell.IsLit);
                            break;
                        case CellType.MirrorLeft:
                            DrawMirror(g, x, y, pad, isLeft: true);
                            break;
                        case CellType.MirrorRight:
                            DrawMirror(g, x, y, pad, isLeft: false);
                            break;
                        case CellType.Wall:
                            DrawWall(g, x, y);
                            break;
                    }
                }
            }
        }

        private void DrawSource(Graphics g, int x, int y, int pad)
        {
            using (var glowBrush = new SolidBrush(Color.FromArgb(40, 255, 220, 50)))
                g.FillEllipse(glowBrush, x + 2, y + 2, CellSize - 4, CellSize - 4);

            using (var brush = new SolidBrush(_colorSource))
                g.FillEllipse(brush, x + pad, y + pad, CellSize - pad * 2, CellSize - pad * 2);

            using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(180, 120, 0)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("☀", font, brush, new RectangleF(x, y, CellSize, CellSize), sf);
            }
        }

        private void DrawReceiver(Graphics g, int x, int y, int pad, bool isLit)
        {
            Color color = isLit ? _colorReceiverLit : _colorReceiver;

            if (isLit)
            {
                using (var glowBrush = new SolidBrush(Color.FromArgb(50, 100, 255, 100)))
                    g.FillRectangle(glowBrush, x + 2, y + 2, CellSize - 4, CellSize - 4);
            }

            using (var brush = new SolidBrush(color))
                g.FillRectangle(brush, x + pad, y + pad, CellSize - pad * 2, CellSize - pad * 2);

            using (var pen = new Pen(Color.White, 2))
            {
                int cx = x + CellSize / 2;
                int cy = y + CellSize / 2;
                g.DrawEllipse(pen, cx - 10, cy - 10, 20, 20);
                g.DrawEllipse(pen, cx - 4, cy - 4, 8, 8);
            }
        }

        private void DrawMirror(Graphics g, int x, int y, int pad, bool isLeft)
        {
            using (var brush = new SolidBrush(Color.FromArgb(35, 40, 65)))
                g.FillRectangle(brush, x + pad / 2, y + pad / 2, CellSize - pad, CellSize - pad);

            using (var pen = new Pen(Color.FromArgb(80, 100, 140), 1))
                g.DrawRectangle(pen, x + pad / 2, y + pad / 2, CellSize - pad, CellSize - pad);

            using (var pen = new Pen(_colorMirror, 3))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                if (isLeft)
                    g.DrawLine(pen, x + pad + 2, y + pad + 2,
                        x + CellSize - pad - 2, y + CellSize - pad - 2);
                else
                    g.DrawLine(pen, x + pad + 2, y + CellSize - pad - 2,
                        x + CellSize - pad - 2, y + pad + 2);
            }
        }

        private void DrawWall(Graphics g, int x, int y)
        {
            using (var brush = new SolidBrush(Color.FromArgb(60, 65, 85)))
                g.FillRectangle(brush, x + 2, y + 2, CellSize - 4, CellSize - 4);

            using (var pen = new Pen(Color.FromArgb(90, 95, 120), 2))
                g.DrawRectangle(pen, x + 2, y + 2, CellSize - 4, CellSize - 4);

            using (var pen = new Pen(Color.FromArgb(110, 115, 140), 2))
            {
                int m = 10;
                g.DrawLine(pen, x + m, y + m, x + CellSize - m, y + CellSize - m);
                g.DrawLine(pen, x + CellSize - m, y + m, x + m, y + CellSize - m);
            }
        }

        private PointF CellCenter(int row, int col)
        {
            return new PointF(
                GridOffset + col * CellSize + CellSize / 2f,
                GridOffset + row * CellSize + CellSize / 2f
            );
        }

        // ============== ОБРАБОТКА КЛИКОВ ==============

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int col = (e.X - GridOffset) / CellSize;
            int row = (e.Y - GridOffset) / CellSize;

            if (e.X >= GridOffset && e.Y >= GridOffset
                && row >= 0 && row < Level.GridSize
                && col >= 0 && col < Level.GridSize)
            {
                _engine.HandleCellClick(row, col);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int col = (e.X - GridOffset) / CellSize;
            int row = (e.Y - GridOffset) / CellSize;

            if (row >= 0 && row < Level.GridSize && col >= 0 && col < Level.GridSize
                && e.X >= GridOffset && e.Y >= GridOffset)
            {
                Cell cell = _engine.CurrentLevel.Grid[row, col];
                this.Cursor = cell.IsMirror ? Cursors.Hand : Cursors.Default;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}
