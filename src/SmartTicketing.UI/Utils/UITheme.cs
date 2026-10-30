using System.Drawing;

namespace SmartTicketing.UI.Utils
{
    /// <summary>
    /// Color theme and styling matching Figma MetroLink UI.
    /// </summary>
    public static class UITheme
    {
        // Brand & Primary Colors
        public static readonly Color NavyDark = Color.FromArgb(11, 37, 69);      // #0b2545 Header & dark panels
        public static readonly Color NavyMedium = Color.FromArgb(13, 43, 69);    // #0d2b45
        public static readonly Color PrimaryBlue = Color.FromArgb(19, 99, 223);   // #1363df Primary action buttons
        public static readonly Color PrimaryBlueHover = Color.FromArgb(15, 80, 180);
        public static readonly Color LightBg = Color.FromArgb(244, 247, 251);     // #f4f7fb Main kiosk background
        public static readonly Color CardBg = Color.White;
        public static readonly Color BorderColor = Color.FromArgb(226, 232, 240); // #e2e8f0 Subtle borders

        // Text Colors
        public static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);    // Dark slate
        public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);// Slate grey
        public static readonly Color TextMuted = Color.FromArgb(148, 163, 184);

        // Status Colors
        public static readonly Color SuccessGreen = Color.FromArgb(21, 128, 61);  // #15803d
        public static readonly Color SuccessBg = Color.FromArgb(220, 252, 231);   // #dcfce7
        public static readonly Color DangerRed = Color.FromArgb(185, 28, 28);     // #b91c1c
        public static readonly Color DangerBg = Color.FromArgb(254, 226, 226);    // #fee2e2
        public static readonly Color WarningGold = Color.FromArgb(180, 83, 9);    // #b45309
        public static readonly Color WarningBg = Color.FromArgb(254, 243, 199);   // #fef3c7

        // Line Accent
        public static readonly Color LineBlue = Color.FromArgb(37, 99, 235);     // Blue #2563eb for Line 01
    }
}
