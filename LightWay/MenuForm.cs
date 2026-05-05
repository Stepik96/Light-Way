using System;
using System.Drawing;
using System.Windows.Forms;

namespace LightWay
{
    /// <summary>
    /// Главное меню игры (View).
    /// Открывается при старте и при возврате из игры.
    /// Показывает актуальные очки текущей сессии.
    /// </summary>
    public class MenuForm : Form
    {
        // Очки передаются сюда при возврате из GameForm
        private int _currentScore;

        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;
        private Label _lblScore = null!;
        private Button _btnStart = null!;
        private Button _btnTutorial = null!;
        private Button _btnSelectLevel = null!;
        private Button _btnExit = null!;
        private Button _btnResetAll = null!;

        // Цвета — те же, что в игре
        private readonly Color _colorBackground = Color.FromArgb(20, 20, 35);
        private readonly Color _colorAccent     = Color.FromArgb(255, 220, 50);
        private readonly Color _colorText       = Color.FromArgb(180, 180, 200);
        private readonly Color _colorBtnDefault = Color.FromArgb(50, 55, 80);
        private readonly Color _colorBtnBorder  = Color.FromArgb(80, 85, 120);

        public MenuForm(int currentScore = 0)
        {
            // Загружаем очки из файла, а не из параметра
            _currentScore = ScoreSaver.Load();
            InitializeForm();
            InitializeControls();
        }

        private void InitializeForm()
        {
            this.Text = "Light Way — Главное меню";
            this.ClientSize = new Size(320, 510);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = _colorBackground;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeControls()
        {
            // Название игры
            _lblTitle = new Label
            {
                Text = "Light Way",
                ForeColor = _colorAccent,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 40),
                Size = new Size(320, 50),
            };
            this.Controls.Add(_lblTitle);

            // Подзаголовок
            _lblSubtitle = new Label
            {
                Text = "Световой путь",
                ForeColor = _colorText,
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 94),
                Size = new Size(320, 24),
            };
            this.Controls.Add(_lblSubtitle);

            // Очки текущей сессии
            _lblScore = new Label
            {
                Text = _currentScore > 0
                    ? $"Очки сессии: {_currentScore}"
                    : "Очки сессии: —",
                ForeColor = _currentScore > 0
                    ? Color.FromArgb(100, 220, 120)
                    : _colorText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 130),
                Size = new Size(320, 22),
            };
            this.Controls.Add(_lblScore);

            // Кнопка «Начать игру» (с уровня 1)
            _btnStart = MakeButton("▶  Начать игру", new Point(80, 175));
            _btnStart.BackColor = Color.FromArgb(40, 100, 60);
            _btnStart.FlatAppearance.BorderColor = Color.FromArgb(60, 140, 80);
            _btnStart.Click += (s, e) => StartGame(1);
            this.Controls.Add(_btnStart);

            // Кнопка «Обучение» — отдельный экран с подсказками
            _btnTutorial = MakeButton("📖  Обучение", new Point(80, 228));
            _btnTutorial.Click += (s, e) =>
            {
                this.Hide();
                using (var tutorial = new TutorialForm())
                {
                    tutorial.ShowDialog();
                }
                this.Show();
            };
            this.Controls.Add(_btnTutorial);

            // Кнопка «Выбор уровня»
            _btnSelectLevel = MakeButton("☰  Выбор уровня", new Point(80, 281));
            _btnSelectLevel.Click += (s, e) =>
            {
                // Подтягиваем актуальные флаги пройденных уровней с диска
                bool[] completed = CompletedLevelsSaver.Load();

                // Открываем окно выбора уровня
                using (var sel = new LevelSelectForm(_currentScore, completed))
                {
                    // Прячем меню пока открыт выбор
                    this.Hide();
                    sel.ShowDialog();

                    // Если пользователь выбрал уровень — запускаем игру
                    if (sel.SelectedLevel > 0)
                        StartGame(sel.SelectedLevel);
                    else
                        this.Show(); // Вернулись назад без выбора
                }
            };
            this.Controls.Add(_btnSelectLevel);

            // Кнопка «Выйти»
            _btnExit = MakeButton("✕  Выйти", new Point(80, 334));
            _btnExit.BackColor = Color.FromArgb(80, 35, 35);
            _btnExit.FlatAppearance.BorderColor = Color.FromArgb(120, 50, 50);
            _btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(_btnExit);

            // Кнопка «Начать заново» — полный сброс очков и пройденных уровней
            _btnResetAll = MakeButton("↺  Начать заново", new Point(80, 387));
            _btnResetAll.BackColor = Color.FromArgb(80, 50, 20);
            _btnResetAll.FlatAppearance.BorderColor = Color.FromArgb(120, 80, 30);
            _btnResetAll.Click += (s, e) => ResetAllProgress();
            this.Controls.Add(_btnResetAll);
        }

        /// <summary>
        /// Полный сброс прогресса. Спрашиваем подтверждение,
        /// затем обнуляем очки и пройденные уровни на диске.
        /// </summary>
        private void ResetAllProgress()
        {
            DialogResult result = MessageBox.Show(
                "Начать игру с самого начала?\n\nОчки и прогресс уровней будут сброшены.",
                "Подтверждение сброса",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            ScoreSaver.Save(0);
            CompletedLevelsSaver.Reset();
            _currentScore = 0;

            _lblScore.Text = "Очки сессии: —";
            _lblScore.ForeColor = _colorText;
        }

        /// <summary>
        /// Запустить игру с нужного уровня.
        /// При возврате из GameForm обновляем очки и показываем меню снова.
        /// </summary>
        private void StartGame(int levelNumber)
        {
            this.Hide();
            using (var game = new GameForm(levelNumber, _currentScore))
            {
                game.ShowDialog();
                // Забираем накопленные очки обратно в меню
                _currentScore = game.FinalScore;
                // Сохраняем в файл сразу после выхода из игры
                ScoreSaver.Save(_currentScore);

                // Сливаем сессионные пройденные уровни с теми что на диске.
                // Проигрыш на других уровнях не сбрасывает уже отмеченные —
                // мы делаем OR, а не перезапись.
                bool[] saved = CompletedLevelsSaver.Load();
                bool[] session = game.SessionCompletedLevels;
                bool[] merged = new bool[Level.TotalLevels];
                for (int i = 0; i < Level.TotalLevels; i++)
                {
                    bool savedDone = i < saved.Length && saved[i];
                    bool sessionDone = session != null && i < session.Length && session[i];
                    merged[i] = savedDone || sessionDone;
                }
                CompletedLevelsSaver.Save(merged);
            }
            // Обновляем метку очков и показываем меню
            _lblScore.Text = _currentScore > 0
                ? $"Очки сессии: {_currentScore}"
                : "Очки сессии: —";
            _lblScore.ForeColor = _currentScore > 0
                ? Color.FromArgb(100, 220, 120)
                : _colorText;
            this.Show();
        }

        /// <summary>
        /// Вспомогательный метод — создать кнопку в едином стиле.
        /// </summary>
        private Button MakeButton(string text, Point location)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = _colorBtnDefault,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                Size = new Size(160, 42),
                Location = location,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            btn.FlatAppearance.BorderColor = _colorBtnBorder;
            return btn;
        }
    }
}
