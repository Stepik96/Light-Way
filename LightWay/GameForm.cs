using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LightWay
{
    /// <summary>
    /// Главная форма игры (View + Controller в MVC).
    /// Рисует сетку, объекты, луч. Обрабатывает клики мышью.
    /// </summary>
    public class GameForm : Form
    {
        private readonly GameEngine _engine;

        // Размеры отрисовки
        private const int CellSize = 46;       // Размер одной клетки в пикселях
        private const int GridOffset = 20;      // Отступ сетки от края формы
        /// <summary>Высота полосы под сеткой: таймер сразу под «ходами», затем статус и кнопки (окно ниже, без обрезки).</summary>
        private const int BottomStripHeight = 158;

        // Кнопки управления
        private Button _btnReset = null!;
        private Button _btnNext = null!;
        private Label _lblLevel = null!;
        private Label _lblStatus = null!;
        private Label _lblScore = null!;
        private Label _lblMoves = null!;
        private Label _lblTimeHint = null!;
        private Label _lblTimeSeconds = null!;
        private Label _lblTimeSuffix = null!;
        private readonly System.Windows.Forms.Timer _gameTimer = new System.Windows.Forms.Timer();

        // Цвета (тема игры)
        private readonly Color _colorBackground  = Color.FromArgb(20, 20, 35);
        private readonly Color _colorGridLine     = Color.FromArgb(40, 50, 80);
        private readonly Color _colorCellDefault  = Color.FromArgb(28, 28, 48);
        private readonly Color _colorCellLit      = Color.FromArgb(45, 45, 70);
        private readonly Color _colorSource       = Color.FromArgb(255, 220, 50);
        private readonly Color _colorReceiver     = Color.FromArgb(50, 200, 50);
        private readonly Color _colorReceiverLit  = Color.FromArgb(100, 255, 100);
        private readonly Color _colorMirror       = Color.FromArgb(140, 180, 220);
        private readonly Color _colorBeam         = Color.FromArgb(255, 255, 100);
        private readonly Color _colorBeamGlow     = Color.FromArgb(80, 255, 255, 100);

        public GameForm()
        {
            _engine = new GameEngine();

            InitializeForm();
            InitializeControls();

            // Подписка на события движка
            _engine.StateChanged += () => {
                UpdateUI();
                Invalidate(); // Перерисовка
            };

            _engine.LevelCompleted += () => {
                _lblStatus.Text = $"✨ Уровень пройден! +{_engine.LastLevelScore} очков";
                _lblStatus.ForeColor = _colorReceiverLit;
                _btnNext.Enabled = true;
                RefreshTimerRunning();
                Invalidate();
            };

            _engine.LevelFailed += () => {
                _lblStatus.Text = "Проигрыш: закончились ходы или время. Нажмите Reset.";
                _lblStatus.ForeColor = Color.FromArgb(255, 120, 120);
                RefreshTimerRunning();
                Invalidate();
            };

            _gameTimer.Interval = 1000;
            _gameTimer.Tick += (s, e) =>
            {
                _engine.TickTimer();
                UpdateUI();
                Invalidate();
            };
            RefreshTimerRunning();

            UpdateUI();
        }

        private void InitializeForm()
        {
            int gridPixels = Level.GridSize * CellSize;
            this.Text = "Light Way — Световой путь";
            this.ClientSize = new Size(
                gridPixels + GridOffset * 2,
                gridPixels + GridOffset * 2 + BottomStripHeight
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
            int gridPixels = Level.GridSize * CellSize;
            int formW = gridPixels + GridOffset * 2;
            // Чуть ближе к сетке — меньше пустоты, всё помещается на экран
            int panelY = GridOffset + gridPixels + 8;

            // Метка уровня
            _lblLevel = new Label
            {
                Text = "Уровень 1",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(GridOffset, panelY + 2),
                AutoSize = true
            };
            this.Controls.Add(_lblLevel);

            _lblScore = new Label
            {
                Text = "Всего очков: 0",
                ForeColor = Color.FromArgb(200, 200, 230),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true
            };
            this.Controls.Add(_lblScore);

            _lblMoves = new Label
            {
                Text = "Повороты зеркал: 0 / 0",
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Segoe UI", 9),
                Location = new Point(GridOffset, panelY + 20),
                AutoSize = true
            };
            this.Controls.Add(_lblMoves);

            _lblStatus = new Label
            {
                Text = "Кликайте по зеркалам для поворота",
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Segoe UI", 9),
                Location = new Point(GridOffset, panelY + 106),
                AutoSize = true,
                MaximumSize = new Size(formW - GridOffset * 2, 0)
            };
            this.Controls.Add(_lblStatus);

            _lblTimeHint = new Label
            {
                Text = "Осталось времени",
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Segoe UI", 9),
                AutoSize = true
            };
            this.Controls.Add(_lblTimeHint);

            _lblTimeSeconds = new Label
            {
                Text = "0",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(34, 36, 56),
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSize = false,
                Size = new Size(46, 46)
            };
            this.Controls.Add(_lblTimeSeconds);

            _lblTimeSuffix = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(160, 160, 180),
                Font = new Font("Segoe UI", 9),
                AutoSize = true
            };
            this.Controls.Add(_lblTimeSuffix);

            // Кнопки — над таймером, по краям (Z-order: добавляем последними)
            _btnReset = new Button
            {
                Text = "↻ Reset",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 90),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Size = new Size(90, 36),
                Location = new Point(GridOffset, panelY + 128),
                Cursor = Cursors.Hand
            };
            _btnReset.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 120);
            _btnReset.Click += (s, e) =>
            {
                _engine.ResetLevel();
                _lblStatus.Text = "Кликайте по зеркалам для поворота";
                _lblStatus.ForeColor = Color.FromArgb(180, 180, 200);
                RefreshTimerRunning();
                UpdateUI();
                Invalidate();
            };
            this.Controls.Add(_btnReset);

            // Кнопка Next
            _btnNext = new Button
            {
                Text = "Next ▸",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 100, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Size = new Size(90, 36),
                Location = new Point(formW - GridOffset - 90, panelY + 128),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            _btnNext.FlatAppearance.BorderColor = Color.FromArgb(60, 140, 80);
            _btnNext.Click += (s, e) =>
            {
                if (!_engine.NextLevel())
                {
                    MessageBox.Show(
                        $"Поздравляем! Все уровни пройдены.\nНабрано очков: {_engine.TotalScore}",
                        "Победа",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }
                _lblStatus.Text = "Кликайте по зеркалам для поворота";
                _lblStatus.ForeColor = Color.FromArgb(180, 180, 200);
                RefreshTimerRunning();
                UpdateUI();
                Invalidate();
            };
            this.Controls.Add(_btnNext);

            ArrangeBottomPanel(panelY, formW);
        }

        /// <summary>
        /// Раскладка: очки справа; таймер по центру сразу под строкой ходов (виден на невысоком мониторе).
        /// </summary>
        private void ArrangeBottomPanel(int panelY, int formW)
        {
            int centerX = formW / 2;

            Size scoreSize = TextRenderer.MeasureText(_lblScore.Text, _lblScore.Font);
            _lblScore.Location = new Point(formW - GridOffset - scoreSize.Width, panelY + 2);

            // Таймер под «Поворотами», выше статуса и кнопок
            int timerBlockTop = panelY + 40;
            if (!_lblTimeSeconds.Visible)
            {
                _lblTimeHint.Location = new Point(centerX - _lblTimeHint.Width / 2, timerBlockTop + 10);
                return;
            }

            _lblTimeHint.Location = new Point(centerX - _lblTimeHint.Width / 2, timerBlockTop);

            int boxY = timerBlockTop + 16;
            _lblTimeSeconds.Location = new Point(centerX - 52, boxY);
            _lblTimeSuffix.Location = new Point(centerX + 4, boxY + 14);
        }

        private void RefreshTimerRunning()
        {
            bool needTimer = _engine.CurrentLevel.TimeLimitSeconds > 0
                && !_engine.IsLevelComplete
                && !_engine.IsLevelFailed;
            _gameTimer.Enabled = needTimer;
        }

        private void UpdateUI()
        {
            int gridPixels = Level.GridSize * CellSize;
            int formW = gridPixels + GridOffset * 2;
            // Чуть ближе к сетке — меньше пустоты, всё помещается на экран
            int panelY = GridOffset + gridPixels + 8;

            _lblLevel.Text = $"Уровень {_engine.CurrentLevelNumber} / {Level.TotalLevels}";
            _btnNext.Enabled = _engine.IsLevelComplete;
            _lblScore.Text = $"Всего очков: {_engine.TotalScore}";

            var lvl = _engine.CurrentLevel;
            _lblMoves.Text = $"Повороты зеркал: {_engine.MirrorRotationsUsed} / {lvl.MaxMirrorClicks} " +
                $"(осталось {_engine.MirrorRotationsRemaining})";

            if (lvl.TimeLimitSeconds <= 0)
            {
                _lblTimeHint.Text = "Время: без ограничения";
                _lblTimeSeconds.Visible = false;
                _lblTimeSuffix.Visible = false;
            }
            else
            {
                _lblTimeHint.Text = "Осталось времени";
                _lblTimeSeconds.Visible = true;
                _lblTimeSuffix.Visible = true;

                int rem = _engine.SecondsRemaining ?? 0;
                int lim = lvl.TimeLimitSeconds;
                _lblTimeSeconds.Text = rem.ToString();
                _lblTimeSuffix.Text = $"из {lim} с";

                bool thirdOrLess = lim > 0 && rem <= lim / 3;
                bool quarterOrLess = lim > 0 && rem <= lim / 4;
                bool danger = thirdOrLess || quarterOrLess;

                if (danger)
                {
                    _lblTimeSeconds.BackColor = Color.FromArgb(170, 35, 50);
                    _lblTimeSeconds.ForeColor = Color.FromArgb(255, 235, 235);
                }
                else
                {
                    _lblTimeSeconds.BackColor = Color.FromArgb(34, 36, 56);
                    _lblTimeSeconds.ForeColor = Color.White;
                }
            }

            ArrangeBottomPanel(panelY, formW);

            if (_engine.CurrentLevelNumber >= Level.TotalLevels && !_engine.IsLevelComplete)
                _btnNext.Text = "Next ▸";
        }

        // ============== ОТРИСОВКА (View) ==============

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawGrid(g);
            DrawBeam(g);
            DrawObjects(g);
        }

        /// <summary>
        /// Отрисовка сетки 10×10 с подсветкой клеток на пути луча.
        /// </summary>
        private void DrawGrid(Graphics g)
        {
            var level = _engine.CurrentLevel;

            for (int r = 0; r < Level.GridSize; r++)
            {
                for (int c = 0; c < Level.GridSize; c++)
                {
                    int x = GridOffset + c * CellSize;
                    int y = GridOffset + r * CellSize;

                    // Фон клетки (подсветка если луч проходит)
                    Color bgColor = level.Grid[r, c].IsLit ? _colorCellLit : _colorCellDefault;
                    using (var brush = new SolidBrush(bgColor))
                        g.FillRectangle(brush, x, y, CellSize, CellSize);

                    // Рамка клетки
                    using (var pen = new Pen(_colorGridLine, 1))
                        g.DrawRectangle(pen, x, y, CellSize, CellSize);
                }
            }
        }

        /// <summary>
        /// Отрисовка луча — линия по точкам BeamPath с эффектом свечения.
        /// </summary>
        private void DrawBeam(Graphics g)
        {
            var path = _engine.BeamPath;
            if (path.Count < 2) return;

            // Внешнее свечение (толстая полупрозрачная линия)
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

            // Основная линия луча
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

        /// <summary>
        /// Отрисовка объектов: источник, приёмник, зеркала.
        /// </summary>
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
                    int pad = 8; // Отступ от краёв клетки

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
                    }
                }
            }
        }

        /// <summary>
        /// Источник света — жёлтый круг с лучами.
        /// </summary>
        private void DrawSource(Graphics g, int x, int y, int pad)
        {
            // Свечение вокруг
            using (var glowBrush = new SolidBrush(Color.FromArgb(40, 255, 220, 50)))
                g.FillEllipse(glowBrush, x + 2, y + 2, CellSize - 4, CellSize - 4);

            // Основной круг
            using (var brush = new SolidBrush(_colorSource))
                g.FillEllipse(brush, x + pad, y + pad, CellSize - pad * 2, CellSize - pad * 2);

            // Символ "☀" в центре
            using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(180, 120, 0)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("☀", font, brush,
                    new RectangleF(x, y, CellSize, CellSize), sf);
            }
        }

        /// <summary>
        /// Приёмник — зелёный квадрат с мишенью. Ярче если луч попал.
        /// </summary>
        private void DrawReceiver(Graphics g, int x, int y, int pad, bool isLit)
        {
            Color color = isLit ? _colorReceiverLit : _colorReceiver;

            if (isLit)
            {
                // Свечение при попадании
                using (var glowBrush = new SolidBrush(Color.FromArgb(50, 100, 255, 100)))
                    g.FillRectangle(glowBrush, x + 2, y + 2, CellSize - 4, CellSize - 4);
            }

            using (var brush = new SolidBrush(color))
                g.FillRectangle(brush, x + pad, y + pad, CellSize - pad * 2, CellSize - pad * 2);

            // Мишень внутри
            using (var pen = new Pen(Color.White, 2))
            {
                int cx = x + CellSize / 2;
                int cy = y + CellSize / 2;
                g.DrawEllipse(pen, cx - 10, cy - 10, 20, 20);
                g.DrawEllipse(pen, cx - 4, cy - 4, 8, 8);
            }
        }

        /// <summary>
        /// Зеркало — линия "\" или "/" с рамкой.
        /// </summary>
        private void DrawMirror(Graphics g, int x, int y, int pad, bool isLeft)
        {
            // Фон зеркала
            using (var brush = new SolidBrush(Color.FromArgb(35, 40, 65)))
                g.FillRectangle(brush, x + pad / 2, y + pad / 2,
                    CellSize - pad, CellSize - pad);

            // Рамка
            using (var pen = new Pen(Color.FromArgb(80, 100, 140), 1))
                g.DrawRectangle(pen, x + pad / 2, y + pad / 2,
                    CellSize - pad, CellSize - pad);

            // Линия зеркала
            using (var pen = new Pen(_colorMirror, 3))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                if (isLeft) // "\" — из верхнего-левого в нижний-правый
                {
                    g.DrawLine(pen,
                        x + pad + 2, y + pad + 2,
                        x + CellSize - pad - 2, y + CellSize - pad - 2);
                }
                else // "/" — из нижнего-левого в верхний-правый
                {
                    g.DrawLine(pen,
                        x + pad + 2, y + CellSize - pad - 2,
                        x + CellSize - pad - 2, y + pad + 2);
                }
            }
        }

        /// <summary>
        /// Координаты центра клетки (для рисования луча).
        /// </summary>
        private PointF CellCenter(int row, int col)
        {
            return new PointF(
                GridOffset + col * CellSize + CellSize / 2f,
                GridOffset + row * CellSize + CellSize / 2f
            );
        }

        // ============== ОБРАБОТКА КЛИКОВ (Controller) ==============

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            // Пересчёт координат мыши → координаты клетки
            int col = (e.X - GridOffset) / CellSize;
            int row = (e.Y - GridOffset) / CellSize;

            // Проверка попадания в сетку
            if (e.X >= GridOffset && e.Y >= GridOffset)
                _engine.HandleCellClick(row, col);
        }

        /// <summary>
        /// Изменение курсора при наведении на зеркало.
        /// </summary>
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
