using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Avalonia.Controls;
using Avalonia.Media;
using DebugReplay.Helpers;
using Pen = System.Drawing.Pen;

namespace DebugReplay;

public partial class MainWindow : Window
{
       private Random random = new();

        public MainWindow()
        {
            InitializeComponent();
            // Рисуем изображение и передаем в элемент формы, который умеет показывать изображения
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Image.Source = BuildScene().ConvertToAvaloniaBitmap();
            stopwatch.Stop();
            DurationLabel.Text = $"Время отрисовки: {stopwatch.ElapsedMilliseconds} миллисекунд";
        }

        private Bitmap BuildScene()
        {
            const int width = 600;
            const int height = 600;
            const int maxGeneration = 9;

            var image = new Bitmap(width, height);
            var graphics = Graphics.FromImage(image);

            // Небо
            graphics.FillRectangle(System.Drawing.Brushes.LightSkyBlue, 0, 0, width, height / 2);
            // Трава
            graphics.FillRectangle(System.Drawing.Brushes.ForestGreen, 0, height / 2, width, height);

            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    var rootX = (i * 250 - 100 * j)
                        + 50 * random.NextDouble(); // Немного случайности в положение дерева
                    var rootY = (height / 2 + 50) + 100 * j
                        + 50 * random.NextDouble();        // Немного случайности в положение дерева
                    var branchLength = 50 * (1 + 0.3 * j); // Длина ветвей зависит от перспективы: чем ближе, тем больше
                    // Ось Y в 2D графике идет вниз, а дерево должны расти вверх, поэтому угол отрицательный.
                    var branchAngle = -Math.PI / 2;
                    DrawTree(graphics, rootX, rootY, branchAngle, branchLength,
                        0, maxGeneration);
                }
            }

            return image;
        }
        
        private void DrawTree(Graphics graphics,
                              double startX, double startY, double branchAngle, double branchLength,
                              int currentGeneration, int maxGeneration)
        {
            // Условие выхода из рекурсии
            if (currentGeneration > maxGeneration)
                return;

            var branchPen = GetBranchPen(currentGeneration, maxGeneration);
            var endX = startX + Math.Cos(branchAngle) * branchLength;
            var endY = startY + Math.Sin(branchAngle) * branchLength;
            graphics.DrawLine(branchPen, (int)startX, (int)startY, (int)endX, (int)endY);

            for (var i = -1; i <= 1; i++)
            {
                var newBranchAngle = branchAngle + i * Math.PI / 6 * (1 + 0.4 * random.NextDouble());
                var newBranchLength = branchLength * 0.6 * (1 + 0.2 * random.NextDouble());
                DrawTree(graphics, endX, endY, newBranchAngle, newBranchLength,
                    currentGeneration + 1, maxGeneration);
            }
        }
        
        private Pen GetBranchPen(int currentGeneration, int maxGeneration)
        {
            //// Для кэширования.
            //// Если pen для текущего поколения уже построен и закэширован, то берем его из кэша
            var generationKey = BuildGenerationKey(currentGeneration, maxGeneration);
            if (branchPenCache.ContainsKey(generationKey))
                return branchPenCache[generationKey];

            // Иначе строим новый pen
            var startBranchColor = Colors.SaddleBrown;
            var endBranchColor = Colors.DarkGreen;

            var red = ((maxGeneration - currentGeneration) * startBranchColor.R + currentGeneration * endBranchColor.R)
                / maxGeneration;
            var green = ((maxGeneration - currentGeneration) * startBranchColor.G + currentGeneration * endBranchColor.G)
                / maxGeneration;
            var blue = ((maxGeneration - currentGeneration) * startBranchColor.B + currentGeneration * endBranchColor.B)
                / maxGeneration;
            var branchColor = System.Drawing.Color.FromArgb((byte)red, (byte)green, (byte)blue);

            var branchWidth = Math.Pow(1.5, 6 * (maxGeneration - currentGeneration) / maxGeneration);

            var branchPen = new Pen(branchColor, (float)branchWidth);

            //// Для кэширования.
            //// Кэшируем построенный для поколения pen
            branchPenCache[generationKey] = branchPen;

            return branchPen;
        }

        // Для кэширования.
        private Dictionary<long, Pen> branchPenCache = new();

        // Для кэширования.
        private long BuildGenerationKey(int currentGeneration, int maxGeneration)
        {
            long key = currentGeneration;
            key <<= 32;
            key += maxGeneration;
            return key;
        }
}