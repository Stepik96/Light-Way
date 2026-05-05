using System;
using System.IO;

namespace LightWay
{
    /// <summary>
    /// Сохранение и загрузка флагов пройденных уровней.
    /// Файл лежит в Документы\LightWay\completed.txt.
    /// Формат — одна строка из 5 цифр, например "10100" — пройдены уровни 1 и 3.
    /// </summary>
    public static class CompletedLevelsSaver
    {
        // Всегда работаем с массивом длиной 5 (по числу уровней игры)
        private const int LevelsCount = 5;

        // Папка: C:\Users\ИМЯ\Documents\LightWay\
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "LightWay",
            "completed.txt");

        /// <summary>
        /// Загрузить флаги пройденных уровней.
        /// Если файла нет или он повреждён — все флаги false.
        /// </summary>
        public static bool[] Load()
        {
            bool[] result = new bool[LevelsCount];

            try
            {
                if (!File.Exists(FilePath))
                    return result;

                string text = File.ReadAllText(FilePath).Trim();

                // Защита от повреждённого файла: длина может отличаться
                for (int i = 0; i < LevelsCount && i < text.Length; i++)
                {
                    if (text[i] == '1')
                        result[i] = true;
                }
            }
            catch
            {
                // Любая ошибка чтения — возвращаем массив по умолчанию (всё false)
            }

            return result;
        }

        /// <summary>
        /// Сохранить флаги пройденных уровней.
        /// Папка создаётся автоматически если её нет.
        /// </summary>
        public static void Save(bool[] completed)
        {
            try
            {
                string dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);

                // Собираем строку из 5 цифр — '1' для пройденных, '0' для непройденных
                char[] chars = new char[LevelsCount];
                for (int i = 0; i < LevelsCount; i++)
                {
                    bool done = (completed != null && i < completed.Length && completed[i]);
                    chars[i] = done ? '1' : '0';
                }

                File.WriteAllText(FilePath, new string(chars));
            }
            catch
            {
                // Не удалось сохранить — игра продолжается
            }
        }

        /// <summary>
        /// Сбросить все флаги в "00000".
        /// </summary>
        public static void Reset()
        {
            Save(new bool[LevelsCount]);
        }
    }
}
