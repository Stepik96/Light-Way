using System;
using System.Collections.Generic;
using System.Drawing;

namespace LightWay
{
    /// <summary>
    /// Игровой движок (Controller в MVC).
    /// Отвечает за: трассировку луча, поворот зеркал,
    /// переключение уровней, проверку победы и поражения, очки.
    /// </summary>
    public class GameEngine
    {
        public Level CurrentLevel { get; private set; }
        public int CurrentLevelNumber { get; private set; }

        public List<Point> BeamPath { get; private set; }

        public bool IsLevelComplete { get; private set; }

        /// <summary>
        /// Исчерпаны клики или время, уровень не пройден.
        /// </summary>
        public bool IsLevelFailed { get; private set; }

        /// <summary>
        /// Сколько раз повернули зеркало на текущей попытке.
        /// </summary>
        public int MirrorRotationsUsed { get; private set; }

        /// <summary>
        /// Сколько секунд прошло с начала текущей попытки (для таймера).
        /// </summary>
        public int ElapsedSeconds { get; private set; }

        /// <summary>
        /// Очки за последний выигранный уровень.
        /// </summary>
        public int LastLevelScore { get; private set; }

        /// <summary>
        /// Сумма очков за все пройденные уровни в этой сессии.
        /// </summary>
        public int TotalScore { get; private set; }

        /// <summary>Только для unit-тестов (сборка видит через InternalsVisibleTo).</summary>
        internal void TestingSetTotalScore(int value) => TotalScore = value;

        public event Action? StateChanged;
        public event Action? LevelCompleted;
        public event Action? LevelFailed;

        public GameEngine()
            : this(Level.LoadLevel(1), 1)
        {
        }

        /// <summary>
        /// Конструктор для тестов и особых сценариев с готовым уровнем.
        /// </summary>
        public GameEngine(Level level, int levelNumber)
        {
            BeamPath = new List<Point>();
            CurrentLevelNumber = levelNumber;
            CurrentLevel = level;
            BeginAttempt();
            TraceBeam();
        }

        /// <summary>
        /// Сколько поворотов ещё можно сделать (не ниже нуля).
        /// </summary>
        public int MirrorRotationsRemaining =>
            Math.Max(0, CurrentLevel.MaxMirrorClicks - MirrorRotationsUsed);

        /// <summary>
        /// Оставшиеся секунды; null, если на уровне нет лимита времени.
        /// </summary>
        public int? SecondsRemaining
        {
            get
            {
                if (CurrentLevel.TimeLimitSeconds <= 0)
                    return null;
                return Math.Max(0, CurrentLevel.TimeLimitSeconds - ElapsedSeconds);
            }
        }

        /// <summary>
        /// Вызов раз в секунду из таймера формы.
        /// </summary>
        public void TickTimer()
        {
            if (IsLevelComplete || IsLevelFailed)
                return;
            if (CurrentLevel.TimeLimitSeconds <= 0)
                return;

            ElapsedSeconds++;
            if (ElapsedSeconds >= CurrentLevel.TimeLimitSeconds)
            {
                ApplyLossResetScore();
                LevelFailed?.Invoke();
            }

            StateChanged?.Invoke();
        }

        public void HandleCellClick(int row, int col)
        {
            if (IsLevelComplete || IsLevelFailed)
                return;

            if (row < 0 || row >= Level.GridSize || col < 0 || col >= Level.GridSize)
                return;

            Cell cell = CurrentLevel.Grid[row, col];

            if (cell.IsMirror)
            {
                MirrorRotationsUsed++;
                cell.RotateMirror();
                TraceBeam();

                if (IsLevelComplete)
                {
                    LastLevelScore = GameScore.Calculate(
                        CurrentLevel.MaxMirrorClicks,
                        MirrorRotationsUsed,
                        CurrentLevel.TimeLimitSeconds,
                        ElapsedSeconds);
                    TotalScore += LastLevelScore;
                    StateChanged?.Invoke();
                    LevelCompleted?.Invoke();
                }
                else if (MirrorRotationsUsed >= CurrentLevel.MaxMirrorClicks)
                {
                    ApplyLossResetScore();
                    StateChanged?.Invoke();
                    LevelFailed?.Invoke();
                }
                else
                {
                    StateChanged?.Invoke();
                }
            }
        }

        public void ResetLevel()
        {
            CurrentLevel = Level.LoadLevel(CurrentLevelNumber);
            IsLevelComplete = false;
            IsLevelFailed = false;
            BeginAttempt();
            TraceBeam();
            StateChanged?.Invoke();
        }

        public bool NextLevel()
        {
            if (CurrentLevelNumber < Level.TotalLevels)
            {
                CurrentLevelNumber++;
                CurrentLevel = Level.LoadLevel(CurrentLevelNumber);
                IsLevelComplete = false;
                IsLevelFailed = false;
                BeginAttempt();
                TraceBeam();
                StateChanged?.Invoke();
                return true;
            }
            return false;
        }

        private void BeginAttempt()
        {
            MirrorRotationsUsed = 0;
            ElapsedSeconds = 0;
            LastLevelScore = 0;
        }

        /// <summary>
        /// При проигрыше общий счёт обнуляется (как договорено в правилах игры).
        /// </summary>
        private void ApplyLossResetScore()
        {
            IsLevelFailed = true;
            TotalScore = 0;
            LastLevelScore = 0;
        }

        public void TraceBeam()
        {
            BeamPath.Clear();
            CurrentLevel.ClearLit();
            IsLevelComplete = false;

            int row = CurrentLevel.SourceRow;
            int col = CurrentLevel.SourceCol;
            Direction dir = CurrentLevel.StartDirection;

            BeamPath.Add(new Point(col, row));
            CurrentLevel.Grid[row, col].IsLit = true;

            int maxSteps = Level.GridSize * Level.GridSize * 2;

            for (int step = 0; step < maxSteps; step++)
            {
                int nextRow = row, nextCol = col;

                switch (dir)
                {
                    case Direction.Up:    nextRow--; break;
                    case Direction.Down:  nextRow++; break;
                    case Direction.Left:  nextCol--; break;
                    case Direction.Right: nextCol++; break;
                }

                if (nextRow < 0 || nextRow >= Level.GridSize ||
                    nextCol < 0 || nextCol >= Level.GridSize)
                {
                    break;
                }

                row = nextRow;
                col = nextCol;

                Cell cell = CurrentLevel.Grid[row, col];
                cell.IsLit = true;
                BeamPath.Add(new Point(col, row));

                switch (cell.Type)
                {
                    case CellType.Receiver:
                        IsLevelComplete = true;
                        return;

                    case CellType.Source:
                        return;

                    case CellType.Wall:
                        // Луч упирается в стену и гаснет
                        return;

                    case CellType.MirrorLeft:
                        dir = ReflectMirrorLeft(dir);
                        break;

                    case CellType.MirrorRight:
                        dir = ReflectMirrorRight(dir);
                        break;

                    case CellType.Empty:
                        break;
                }
            }
        }

        private Direction ReflectMirrorLeft(Direction incoming)
        {
            switch (incoming)
            {
                case Direction.Right: return Direction.Down;
                case Direction.Down:  return Direction.Right;
                case Direction.Left:  return Direction.Up;
                case Direction.Up:    return Direction.Left;
                default: return incoming;
            }
        }

        private Direction ReflectMirrorRight(Direction incoming)
        {
            switch (incoming)
            {
                case Direction.Right: return Direction.Up;
                case Direction.Up:    return Direction.Right;
                case Direction.Left:  return Direction.Down;
                case Direction.Down:  return Direction.Left;
                default: return incoming;
            }
        }
    }
}
