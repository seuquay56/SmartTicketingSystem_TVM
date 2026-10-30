using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SmartTicketing.UI.Utils
{
    public static class BarcodeRenderer
    {
        /// <summary>
        /// Generates a realistic Code 128 transport barcode image using GDI+.
        /// </summary>
        public static Bitmap GenerateBarcodeBitmap(string text, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.None;

            int padding = 15;
            int barcodeHeight = height - 25;
            int drawWidth = width - (padding * 2);

            // Pseudo-random deterministic bar patterns based on hash of text
            var hash = text.GetHashCode();
            var rand = new Random(Math.Abs(hash));

            int currentX = padding;
            using var brushBlack = new SolidBrush(Color.Black);

            while (currentX < width - padding)
            {
                int barWidth = rand.Next(1, 4);
                int spaceWidth = rand.Next(1, 4);

                if (currentX + barWidth > width - padding)
                    barWidth = (width - padding) - currentX;

                g.FillRectangle(brushBlack, currentX, 5, barWidth, barcodeHeight);
                currentX += barWidth + spaceWidth;
            }

            // Draw text below barcode
            using var font = new Font("Consolas", 9, FontStyle.Regular);
            using var textBrush = new SolidBrush(Color.FromArgb(50, 50, 50));
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(text, font, textBrush, new RectangleF(0, height - 18, width, 18), sf);

            return bmp;
        }

        /// <summary>
        /// Generates a high-contrast dynamic QR code grid image using GDI+.
        /// </summary>
        public static Bitmap GenerateQRCodeBitmap(string payload, int size)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.None;

            int matrixSize = 25; // 25x25 QR matrix modules
            int cellSize = size / matrixSize;
            int offset = (size - (cellSize * matrixSize)) / 2;

            var rand = new Random(Math.Abs(payload.GetHashCode()));
            using var blackBrush = new SolidBrush(Color.Black);

            // Generate matrix with 3 position detection patterns (corners)
            bool[,] matrix = new bool[matrixSize, matrixSize];

            // Corner finder patterns (7x7)
            void DrawFinder(int startR, int startC)
            {
                for (int r = 0; r < 7; r++)
                {
                    for (int c = 0; c < 7; c++)
                    {
                        if (r == 0 || r == 6 || c == 0 || c == 6 || (r >= 2 && r <= 4 && c >= 2 && c <= 4))
                            matrix[startR + r, startC + c] = true;
                    }
                }
            }

            DrawFinder(0, 0);
            DrawFinder(0, matrixSize - 7);
            DrawFinder(matrixSize - 7, 0);

            // Random fill rest for pseudo QR appearance
            for (int r = 0; r < matrixSize; r++)
            {
                for (int c = 0; c < matrixSize; c++)
                {
                    if ((r < 8 && c < 8) || (r < 8 && c >= matrixSize - 8) || (r >= matrixSize - 8 && c < 8))
                        continue;

                    // Alternating timing marks
                    if (r == 6 || c == 6)
                        matrix[r, c] = ((r + c) % 2 == 0);
                    else
                        matrix[r, c] = (rand.Next(100) < 45);
                }
            }

            // Draw to image
            for (int r = 0; r < matrixSize; r++)
            {
                for (int c = 0; c < matrixSize; c++)
                {
                    if (matrix[r, c])
                    {
                        g.FillRectangle(blackBrush, offset + c * cellSize, offset + r * cellSize, cellSize, cellSize);
                    }
                }
            }

            return bmp;
        }
    }
}
