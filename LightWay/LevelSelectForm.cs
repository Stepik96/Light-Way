using System;
using System.Drawing;
using System.Windows.Forms;

namespace LightWay
{
    /// <summary>
    /// Окно выбора уровня (View).
    /// Показывает 5 кнопок с номерами уровней.
    /// После выбора сохраняет номер в SelectedLevel и закрывается.
    /// </summary>
    public class LevelSelectForm : Form
    {
        /// <summary>
        /// Выбранный уровень. 0 = пользователь нажал «Назад».
        /// </summary>
        public int SelectedLevel { get; private set; } = 0;

        private readonly int _currentScore;
        // Какие уровни пройдены — bool[5], индекс 0 — уровень 1
        private readonly bool[] _completedLevels;

        private readonly Color _colorBackground = Color.FromArgb(20, 20, 35);
        private readonly Color _colorAccent     = Color.FromArgb(255, 220, 50);
        private readonly Color _colorText       = Color.FromArgb(180, 180, 200);
        private readonly Color _colorBtnLevel   = Color.FromArgb(50, 55, 80);
        private readonly Color _colorBtnBorder  = Color.FromArgb(80, 85, 120);
        private readonly Color _colorBtnDone    = Color.FromArgb(40, 120, 60);

        public LevelSelectForm(int currentScore = 0, bool[]? completedLevels = null)
        {
            _currentScore = currentScore;

            // Всегда сливаем переданное с сохранённым на диске —
            // даже если параметр не передан, флаги подтянутся из файла.
            bool[] saved = CompletedLevelsSaver.Load();
            _completedLevels = new bool[Level.TotalLevels];
            for (int i = 0; i < Level.TotalLevels; i++)
            {
                bool fromParam = completedLevels != null
                    && i < completedLevels.Length
                    && completedLevels[i];
                bool fromFile = i < saved.Length && saved[i];
                _completedLevels[i] = fromParam || fromFile;
            }

            InitializeForm();
            InitializeControls();
        }

        private void InitializeForm()
        {
            this.Text = "Light Way — Выбор уровня";
            this.ClientSize = new Size(320, 310);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = _colorBackground;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeControls()
        {
            // Заголовок
            var lblTitle = new Label
            {
                Text = "Выберите уровень",
                ForeColor = _colorAccent,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 28),
                Size = new Size(320, 34),
            };
            this.Controls.Add(lblTitle);

            // Очки сессии — чтобы игрок видел прогресс
            var lblScore = new Label
            {
                Text = _currentScore > 0
                    ? $"Очки сессии: {_currentScore}"
                    : "Очки сессии: —",
                ForeColor = _currentScore > 0
                    ? Color.FromArgb(100, 220, 120)
                    : _colorText,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 66),
                Size = new Size(320, 20),
            };
            this.Controls.Add(lblScore);

            // Кнопки уровней 1–5 в ряд
            int btnSize = 46;
            int gap = 12;
            int totalW = Level.TotalLevels * btnSize + (Level.TotalLevels - 1) * gap;
            int startX = (320 - totalW) / 2;
            int btnY = 106;

            for (int i = 1; i <= Level.TotalLevels; i++)
            {
                int levelNum = i; // замыкание
                bool done = _completedLevels[levelNum - 1];

                var btn = new Button
                {
                    // Пройденные уровни помечаются галочкой перед номером
                    Text = done ? "✓" + levelNum.ToString() : levelNum.ToString(),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = done ? _colorBtnDone : _colorBtnLevel,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    Size = new Size(btnSize, btnSize),
                    Location = new Point(startX + (i - 1) * (btnSize + gap), btnY),
                    Cursor = Cursors.Hand,
                    // Tag хранит номер уровня — по нему ниже находим кнопки
                    // (по тексту нельзя — там может быть префикс "✓")
                    Tag = levelNum,
                };
                btn.FlatAppearance.BorderColor = _colorBtnBorder;
                btn.Click += (s, e) =>
                {
                    SelectedLevel = levelNum;
                    this.Close();
                };
                this.Controls.Add(btn);
            }

            // Описания уровней
            string[] hints = {
                "Уровень 1 — Обучение",
                "Уровень 2 — Первый обман",
                "Уровень 3 — Три пути",
                "Уровень 4 — Разветвление",
                "Уровень 5 — Финал",
            };
            var lblHint = new Label
            {
                Text = hints[0],
                ForeColor = _colorText,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 166),
                Size = new Size(320, 20),
                Tag = hints,
            };
            this.Controls.Add(lblHint);

            // При наведении на кнопку уровня — показываем описание.
            // Кнопки уровней опознаём по Tag (текст может содержать "✓").
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button b && b.Tag is int lvl)
                {
                    int idx = lvl - 1;
                    b.MouseEnter += (s, e) => lblHint.Text = hints[idx];
                    b.MouseLeave += (s, e) => lblHint.Text = "";
                }
            }

            // Кнопка «Назад»
            var btnBack = new Button
            {
                Text = "← Назад",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 55, 80),
                ForeColor = _colorText,
                Font = new Font("Segoe UI", 10),
                Size = new Size(120, 36),
                Location = new Point((320 - 120) / 2, 220),
                Cursor = Cursors.Hand,
            };
            btnBack.FlatAppearance.BorderColor = _colorBtnBorder;
            btnBack.Click += (s, e) =>
            {
                SelectedLevel = 0; // сигнал: пользователь вернулся назад
                this.Close();
            };
            this.Controls.Add(btnBack);
        }
    }
}
