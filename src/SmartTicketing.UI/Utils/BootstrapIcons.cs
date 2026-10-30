using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SmartTicketing.UI.Utils
{
    public static class BootstrapIcons
    {
        public static Bitmap GeoAltFill(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var path = new GraphicsPath();
            // Head circle arc from 215 deg to -35 deg
            path.AddArc(2f * s, 0.5f * s, 12f * s, 11.5f * s, 215, 110);
            // Right taper down to point (8, 15.5)
            path.AddBezier(13.6f * s, 4.8f * s, 14f * s, 8f * s, 11f * s, 12f * s, 8f * s, 15.5f * s);
            // Left taper from point (8, 15.5) up to head arc
            path.AddBezier(8f * s, 15.5f * s, 5f * s, 12f * s, 2f * s, 8f * s, 2.4f * s, 4.8f * s);
            path.CloseFigure();

            // Subtract center hole
            var region = new Region(path);
            using var hole = new GraphicsPath();
            hole.AddEllipse(5.6f * s, 3.8f * s, 4.8f * s, 4.8f * s);
            region.Exclude(hole);

            using var brush = new SolidBrush(color);
            g.FillRegion(brush, region);
            return bmp;
        }

        public static Bitmap ArrowRight(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var pen = new Pen(color, Math.Max(2f, 2f * s)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            // Horizontal line
            g.DrawLine(pen, 2f * s, 8f * s, 13f * s, 8f * s);
            // Top arrowhead
            g.DrawLine(pen, 9f * s, 4f * s, 13.5f * s, 8f * s);
            // Bottom arrowhead
            g.DrawLine(pen, 9f * s, 12f * s, 13.5f * s, 8f * s);
            return bmp;
        }

        public static Bitmap ArrowLeft(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var pen = new Pen(color, Math.Max(2f, 2f * s)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLine(pen, 14f * s, 8f * s, 3f * s, 8f * s);
            g.DrawLine(pen, 7f * s, 4f * s, 2.5f * s, 8f * s);
            g.DrawLine(pen, 7f * s, 12f * s, 2.5f * s, 8f * s);
            return bmp;
        }

        public static Bitmap Globe(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var pen = new Pen(color, Math.Max(1.5f, 1.5f * s));
            // Outer sphere
            g.DrawEllipse(pen, 1.5f * s, 1.5f * s, 13f * s, 13f * s);
            // Equator
            g.DrawLine(pen, 1.5f * s, 8f * s, 14.5f * s, 8f * s);
            // Vertical meridian
            g.DrawEllipse(pen, 5f * s, 1.5f * s, 6f * s, 13f * s);
            return bmp;
        }

        public static Bitmap VolumeUp(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var brush = new SolidBrush(color);
            // Speaker cone
            var pts = new PointF[]
            {
                new(2f * s, 5.5f * s),
                new(5f * s, 5.5f * s),
                new(8.5f * s, 2.5f * s),
                new(8.5f * s, 13.5f * s),
                new(5f * s, 10.5f * s),
                new(2f * s, 10.5f * s)
            };
            g.FillPolygon(brush, pts);

            // Sound waves
            using var pen = new Pen(color, Math.Max(1.5f, 1.5f * s)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawArc(pen, 6f * s, 5f * s, 6f * s, 6f * s, -45, 90);
            g.DrawArc(pen, 5f * s, 2.5f * s, 10f * s, 11f * s, -45, 90);
            return bmp;
        }

        public static Bitmap QuestionCircle(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var pen = new Pen(color, Math.Max(1.5f, 1.5f * s));
            g.DrawEllipse(pen, 1.5f * s, 1.5f * s, 13f * s, 13f * s);

            using var font = new Font("Segoe UI", Math.Max(8f, 9f * s), FontStyle.Bold, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(color);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("?", font, brush, new RectangleF(1.5f * s, 1.5f * s, 13f * s, 13f * s), sf);
            return bmp;
        }

        public static Bitmap CreditCard(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var pen = new Pen(color, Math.Max(1.6f, 1.6f * s));
            DrawRoundedRectangle(g, pen, 1f * s, 3f * s, 14f * s, 10f * s, 2f * s);

            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, 1.2f * s, 6f * s, 13.6f * s, 2.2f * s);
            return bmp;
        }

        public static Bitmap QrCode(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.None;
            float s = size / 16f;
            using var b = new SolidBrush(color);

            // Top-left box
            g.FillRectangle(b, 1 * s, 1 * s, 5 * s, 5 * s);
            g.FillRectangle(Brushes.Transparent, 2 * s, 2 * s, 3 * s, 3 * s);
            g.FillRectangle(b, 3 * s, 3 * s, 1 * s, 1 * s);

            // Top-right box
            g.FillRectangle(b, 10 * s, 1 * s, 5 * s, 5 * s);
            g.FillRectangle(Brushes.Transparent, 11 * s, 2 * s, 3 * s, 3 * s);
            g.FillRectangle(b, 12 * s, 3 * s, 1 * s, 1 * s);

            // Bottom-left box
            g.FillRectangle(b, 1 * s, 10 * s, 5 * s, 5 * s);
            g.FillRectangle(Brushes.Transparent, 2 * s, 11 * s, 3 * s, 3 * s);
            g.FillRectangle(b, 3 * s, 12 * s, 1 * s, 1 * s);

            // Center details
            g.FillRectangle(b, 7 * s, 2 * s, 2 * s, 3 * s);
            g.FillRectangle(b, 7 * s, 7 * s, 2 * s, 2 * s);
            g.FillRectangle(b, 11 * s, 8 * s, 3 * s, 2 * s);
            g.FillRectangle(b, 7 * s, 11 * s, 3 * s, 3 * s);
            return bmp;
        }

        public static Bitmap CheckCircleFill(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, 1f * s, 1f * s, 14f * s, 14f * s);

            using var pen = new Pen(Color.White, Math.Max(2f, 2f * s)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLine(pen, 4.5f * s, 8f * s, 7f * s, 10.5f * s);
            g.DrawLine(pen, 7f * s, 10.5f * s, 11.5f * s, 5.5f * s);
            return bmp;
        }

        public static Bitmap XCircle(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = size / 16f;

            using var pen = new Pen(color, Math.Max(1.6f, 1.6f * s)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawEllipse(pen, 1.5f * s, 1.5f * s, 13f * s, 13f * s);
            g.DrawLine(pen, 5.5f * s, 5.5f * s, 10.5f * s, 10.5f * s);
            g.DrawLine(pen, 10.5f * s, 5.5f * s, 5.5f * s, 10.5f * s);
            return bmp;
        }

        private static void DrawRoundedRectangle(Graphics g, Pen pen, float x, float y, float w, float h, float r)
        {
            using var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }
    }
}
