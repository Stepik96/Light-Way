using System;
using System.IO;

namespace LightWay
{
    /// <summary>
    /// Сохранение и загрузка очков в файл score.txt.
    /// Файл лежит в папке Документы\LightWay\score.txt
    /// </summary>
    public static class ScoreSaver
    {
        // Папка: C:\Users\ИМЯ\Documents\LightWay\
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "LightWay",
            "score.txt");

        /// <summary>
        /// Загрузить очки из файла.
        /// Если файла нет или он повреждён — вернуть 0.
        /// </summary>
        public static int Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return 0;

                string text = File.ReadAllText(FilePath).Trim();
                if (int.TryParse(text, out int score))
                    return score;

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Сохранить очки в файл.
        /// Папка создаётся автоматически если её нет.
        /// </summary>
        public static void Save(int score)
        {
            try
            {
                // Создаём папку LightWay в Документах если её ещё нет
                string dir = Path.GetDirectoryName(FilePath)!;
                Directory.CreateDirectory(dir);

                File.WriteAllText(FilePath, score.ToString());
            }
            catch
            {
                // Не удалось сохранить — игра продолжается
            }
        }
    }
}
