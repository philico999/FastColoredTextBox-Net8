using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FastColoredTextBoxNS
{
    public class ColorHelper
    {
        /// <summary>
        /// Gets the default luminance threshold value used for color contrast calculations.
        /// </summary>
        public const int LuminanceThreshold = 160;
        public const int LuminanceRadius = 40;

        /// <summary>
        /// Adjusts a foreground <see cref="Color"/> to improve visual contrast against a given background
        /// using a luminance-threshold "mirroring" rule plus a minimum separation radius.
        /// </summary>
        /// <param name="foreColor">
        /// The starting (developer-chosen) foreground color. The algorithm preserves its hue/saturation as much
        /// as possible by shifting luminance; RGB channels are shifted uniformly and clamped to the 0–255 range.
        /// </param>
        /// <param name="backColor">
        /// The background color to contrast against. Its luminance is used to decide whether to mirror the
        /// foreground across the threshold and how far to push it away.
        /// </param>
        /// <param name="luminanceThreshold">
        /// The luminance split point (0–255) used to decide "sides." If both the background luminance and the
        /// foreground luminance are <em>below</em> this value or both are <em>above</em> it, the foreground luminance
        /// is mirrored across the threshold via <c>fLumaNew = 2 * lumaThreshold - fLuma</c>. A typical value is 180.
        /// </param>
        /// <param name="luminanceRadius">
        /// A minimum luminance "push" (in 0–255 units) used after mirroring. If the absolute difference between
        /// the (possibly mirrored) foreground luminance and the background luminance is <em>less</em> than this radius,
        /// the foreground luminance is pushed an additional <paramref name="luminanceRadius"/> away from the background
        /// (i.e., not merely up to the radius but by the full radius), then clamped to 0–255. A typical value is 50.
        /// </param>
        /// <returns>
        /// A new ARGB color derived from <paramref name="foreColor"/> whose luminance has been adjusted to improve
        /// contrast against <paramref name="backColor"/> while attempting to preserve the original hue and saturation.
        /// </returns>
        /// <remarks>
        /// <para><strong>Algorithm overview</strong></para>
        /// <list type="number">
        ///   <item>
        ///     <description>Compute luminances using <see cref="ColorLuminance(System.Drawing.Color)"/>:
        ///     <c>bLuma = Y(backColor)</c>, <c>fLuma = Y(foreColor)</c>.</description>
        ///   </item>
        ///   <item>
        ///     <description><em>Mirroring:</em> If <c>(bLuma &lt; T &amp;&amp; fLuma &lt; T)</c> or <c>(bLuma &gt; T &amp;&amp; fLuma &gt; T)</c>
        ///     where <c>T = lumaThreshold</c>, set <c>fLumaNew = 2*T - fLuma</c>. Otherwise (they're on opposite sides),
        ///     set <c>fLumaNew = fLuma</c>. Clamp <c>fLumaNew</c> to [0,255].</description>
        ///   </item>
        ///   <item>
        ///     <description><em>Radius push:</em> Let <c>sep = |fLumaNew - bLuma|</c>. If <c>sep &lt; lumaRadius</c>,
        ///     push the foreground farther away from the background by <paramref name="luminanceRadius"/> in the direction
        ///     opposite <c>bLuma</c>:
        ///     <c>fLumaAdj = fLumaNew - lumaRadius</c> when <c>fLumaNew ≤ bLuma</c>, otherwise
        ///     <c>fLumaAdj = fLumaNew + lumaRadius</c>; then clamp to [0,255]. If <c>sep ≥ lumaRadius</c>,
        ///     use <c>fLumaAdj = fLumaNew</c>.</description>
        ///   </item>
        ///   <item>
        ///     <description>Apply the luminance delta <c>Δ = (fLumaAdj - fLuma)</c> to the original RGB channels
        ///     uniformly: <c>R' = clamp(R + Δ)</c>, <c>G' = clamp(G + Δ)</c>, <c>B' = clamp(B + Δ)</c>.
        ///     This approximately preserves hue/saturation, subject to clamping.</description>
        ///   </item>
        /// </list>
        ///
        /// <para><strong>Notes</strong></para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>This is a luminance-based heuristic; it does not guarantee a specific WCAG contrast ratio.
        ///     If strict accessibility ratios are required, consider a separate contrast-ratio check &amp; adjustment pass.</description>
        ///   </item>
        ///   <item>
        ///     <description>Clamping at channel bounds (0 or 255) may limit how closely the final luminance follows the
        ///     theoretical target, especially for colors near gamut limits.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Example 1
        /// // bLuma = 130, fLuma = 210, T = 180, radius = 50
        /// // Different sides of T ⇒ no mirror; sep = |210 - 130| = 80 ≥ 50 ⇒ no extra push
        /// // Result luminance stays ~210 (subject to channel clamping)
        ///
        /// // Example 2
        /// // bLuma = 190, fLuma = 210, T = 180, radius = 50
        /// // Same side (both &gt; T) ⇒ mirror: fLumaNew = 2*180 - 210 = 150
        /// // sep = |150 - 190| = 40 &lt; 50 ⇒ push farther away by 50 toward darker: fLumaAdj = 150 - 50 = 100
        ///
        /// // Example 3
        /// // bLuma = 200, fLuma = 255, T = 180, radius = 50
        /// // Same side (both &gt; T) ⇒ mirror: fLumaNew = 105
        /// // sep = |105 - 200| = 95 ≥ 50 ⇒ no extra push ⇒ fLumaAdj = 105
        ///
        /// // Example 4
        /// // bLuma = 200, fLuma = 40, T = 180, radius = 50
        /// // Different sides of T ⇒ no mirror; sep = |40 - 200| = 160 ≥ 50 ⇒ no extra push ⇒ ~40
        /// </code>
        /// </example>
        /// <seealso cref="ColorLuminance(System.Drawing.Color)"/>
        public static Color AdjustForeColorClamped(
                Color backColor,
                Color foreColor,
                int luminanceThreshold = LuminanceThreshold,
                int luminanceRadius = LuminanceRadius)
        {
            int bLuma = ColorLuminance(backColor);
            int fLuma = ColorLuminance(foreColor);

            // 1) Mirror if both are on the same side of the threshold
            bool bothBelow = (bLuma < luminanceThreshold) && (fLuma < luminanceThreshold);
            bool bothAbove = (bLuma > luminanceThreshold) && (fLuma > luminanceThreshold);

            int fLumaNew = fLuma;
            if (bothBelow || bothAbove)
                fLumaNew = (2 * luminanceThreshold) - fLuma;     // mirror across threshold

            fLumaNew = ClampByte(fLumaNew);                 // keep 0..255

            // 2) If too close to the background, push farther away by lumaRadius
            int sep = Math.Abs(fLumaNew - bLuma);
            int fLumaAdj = fLumaNew;

            if (luminanceRadius > sep)
            {
                // Move away from the background by lumaRadius (not just the difference)
                if (fLumaNew <= bLuma)
                    fLumaAdj = ClampByte(fLumaNew - luminanceRadius);
                else
                    fLumaAdj = ClampByte(fLumaNew + luminanceRadius);
            }

            // 3) Apply the luminance change to the original color.
            // Because the luminance formula coefficients sum to 1,
            // adding the same delta to each channel changes luma by ~delta (subject to clamping).
            int delta = fLumaAdj - fLuma;

            int r = ClampByte(foreColor.R + delta);
            int g = ClampByte(foreColor.G + delta);
            int b = ClampByte(foreColor.B + delta);

            return Color.FromArgb(foreColor.A, r, g, b);
        }

        /// <summary>
        /// Determines an optimal foreground color (black or white) based on the luminance of the background color 
        /// to ensure proper contrast and readability.
        /// </summary>
        /// <param name="backColor">The background color to evaluate for contrast against the foreground.</param>
        /// <param name="luminanceThreshold">The luminance threshold value used to determine if the background 
        /// is light or dark. It is scaled from its normal range of 0..1 to a range of 0..255.</param>
        /// Returns an adjusted version of the base foreground color (black or white) that provides optimal contrast 
        /// against the background color. For darker backgrounds, the color is white and for lighter is black
        public static Color AdjustForeColor(Color backColor, int luminanceThreshold = LuminanceThreshold)
        {
            return ColorLuminance(backColor) > SafeLuminanceThreshold(luminanceThreshold) ? Color.Black : Color.White;
        }

        /// <summary>
        /// Adjusts a color to ensure sufficient contrast against a reference color for optimal readability.
        /// </summary>
        /// <param name="referenceColor">The reference color to contrast against (typically background, but can be any base color).</param>
        /// <param name="targetColor">The color to adjust (typically foreground, glow, or border color).</param>
        /// <param name="desiredContrast">The target WCAG contrast ratio to achieve (default 6.5 for enhanced readability).</param>
        /// <param name="luminanceThreshold">The minimum luminance difference required for readability (0-255, default 128).</param>
        /// <returns>An adjusted color with sufficient contrast against the reference color.</returns>
        /// <remarks>
        /// Uses default settings: no snap to black/white, preserves alpha channel.
        /// This is the simplified version suitable for most contrast adjustment scenarios.
        /// </remarks>
        public static Color AdjustColorWithDesiredContrast(Color targetColor, Color referenceColor, double desiredContrast = 6.5, int luminanceThreshold = LuminanceThreshold)
            => AdjustColorWithDesiredContrast(targetColor, referenceColor, luminanceThreshold, desiredContrast: desiredContrast, snapToBW: false, preserveAlpha: true);
        /// <summary>
        /// Advanced color adjustment that provides fine-grained control over contrast requirements.
        /// Minimally adjusts the target color toward optimal contrast (black or white) until the desired
        /// contrast ratio is achieved, without overshooting to pure extremes unless necessary.
        /// </summary>
        /// <param name="referenceColor">The reference color to contrast against (can be background, foreground, or any base color).</param>
        /// <param name="targetColor">The color to adjust (can be foreground, background, glow, border, or any color needing contrast).</param>
        /// <param name="luminanceThreshold">The minimum luminance difference required for readability (0-255, clamped to valid range).</param>
        /// <param name="desiredContrast">The target WCAG contrast ratio to achieve (e.g., 3.0 for AA, 4.5 for AAA, 7.0 for AAA large text).</param>
        /// <param name="snapToBW">If true, forces pure black/white when desired contrast cannot be achieved naturally.</param>
        /// <param name="preserveAlpha">If true, maintains the original alpha channel of the target color.</param>
        /// <returns>An optimally adjusted color that meets the specified contrast requirements.</returns>
        /// <remarks>
        /// This method uses WCAG contrast ratio calculations and alpha blending for accurate contrast measurement.
        /// It performs binary search to find the minimal adjustment needed to achieve the desired contrast ratio.
        /// The method is versatile and can adjust any color property (foreground, background, glow, border) 
        /// relative to any reference color for consistent visual contrast across UI elements.
        /// </remarks>
        public static Color AdjustColorWithDesiredContrast(
            Color targetColor,
            Color referenceColor,
            int luminanceThreshold,
            double desiredContrast,
            bool snapToBW = false,
            bool preserveAlpha = true)
        {
            luminanceThreshold = SafeLuminanceThreshold(luminanceThreshold);

            // Work with input alpha; contrast is evaluated on the composited result.
            var fg = targetColor;
            var bg = referenceColor;

            // If already good enough (by both luminance and desired contrast), keep it.
            if (Math.Abs(ColorLuminance(AlphaBlend(fg, bg)) - ColorLuminance(bg)) >= luminanceThreshold
                && ContrastRatioComposite(fg, bg) >= desiredContrast)
                return fg;

            // Which pole would help more?
            var poleBlack = Color.FromArgb(preserveAlpha ? fg.A : (byte)255, 0, 0, 0);
            var poleWhite = Color.FromArgb(preserveAlpha ? fg.A : (byte)255, 255, 255, 255);
            double cBlack = ContrastRatioComposite(poleBlack, bg);
            double cWhite = ContrastRatioComposite(poleWhite, bg);
            var pole = (cWhite >= cBlack) ? poleWhite : poleBlack;

            // If the current color already exceeds desired contrast, we're done.
            if (ContrastRatioComposite(fg, bg) >= desiredContrast)
                return fg;

            // Move the *minimum* amount toward the chosen pole to meet desired contrast.
            var best = fg;
            double lo = 0.0, hi = 1.0;
            for (int i = 0; i < 24; i++) // binary search for minimal t
            {
                double mid = (lo + hi) * 0.5;
                var test = Lerp(fg, pole, mid);
                if (ContrastRatioComposite(test, bg) >= desiredContrast)
                {
                    best = test;
                    hi = mid;
                }
                else
                {
                    lo = mid;
                }
            }

            // If even pure pole can't hit the target (rare), optionally snap.
            if (snapToBW && ContrastRatioComposite(best, bg) < desiredContrast)
                best = pole;

            return best;
        }

        /// <summary>
        /// Adjusts a color's brightness based on the luminance of a reference color to ensure proper contrast.
        /// </summary>
        /// <param name="color">The color to adjust.</param>
        /// <param name="referenceColor">The reference color whose luminance determines the shift direction and magnitude.</param>
        /// <param name="rgbShift">The RGB shift amount (0-255). Positive values brighten dark reference colors and darken bright ones.</param>
        /// <param name="luminanceThreshold">The luminance threshold (default 128) that determines shift behavior. 
        /// Colors above this threshold are darkened, colors below are brightened.</param>
        /// <returns>A new color with adjusted brightness based on the reference color's luminance.</returns>
        /// <remarks>
        /// The shift direction is determined by the reference color's luminance:
        /// <list type="bullet">
        /// <item>High luminance (>170): applies double negative shift (darkens significantly)</item>
        /// <item>Medium luminance (128-170): applies single negative shift (darkens moderately)</item>
        /// <item>Low luminance (<128): applies positive shift (brightens)</item>
        /// </list>
        /// This ensures that colors are adjusted for readability against varying background luminances.
        /// </remarks>
        public static Color NormalizeColor(Color color, Color referenceColor, int rgbShift, int luminanceThreshold = LuminanceThreshold)
        {
            int sgn = Math.Sign(rgbShift);
            int shift = NormalizeColorShift(referenceColor, rgbShift, luminanceThreshold);
            return ShiftColor(color, sgn * shift);
        }

        /// <summary>
        /// Adjusts a color using separate RGB and alpha shift values, with shift direction determined by the reference color's luminance.
        /// </summary>
        /// <param name="color">The color to adjust.</param>
        /// <param name="referenceColor">The reference color whose luminance determines the shift direction and magnitude.</param>
        /// <param name="redShift">The red channel shift amount (default 20).</param>
        /// <param name="greenShift">The green channel shift amount (default 20).</param>
        /// <param name="blueShift">The blue channel shift amount (default 20).</param>
        /// <param name="alphaShift">The alpha channel shift amount (default 0).</param>
        /// <param name="luminanceThreshold">The luminance threshold (default 128) that determines shift behavior.</param>
        /// <returns>A new color with adjusted RGB and alpha values based on the reference color's luminance.</returns>
        /// <remarks>
        /// This overload calculates the normalized shift direction based on the reference color's luminance 
        /// (see <see cref="NormalizeColor(Color, Color, int, int)"/> for shift behavior), but applies the 
        /// individual RGB and alpha shift values directly to the color channels. The alpha channel is adjusted 
        /// independently of the luminance-based normalization.
        /// </remarks>
        public static Color NormalizeColor(Color color, Color referenceColor, int redShift = 20, int greenShift = 20,
            int blueShift = 20, int alphaShift = 0, int luminanceThreshold = LuminanceThreshold)
        {
            int rgbShift = (int)((redShift + greenShift + blueShift) / 3);
            int shift = NormalizeColorShift(referenceColor, rgbShift, luminanceThreshold);
            return ShiftColor(color, redShift: redShift, greenShift: greenShift, blueShift: blueShift, alphaShift: alphaShift);
        }

        /// <summary>
        /// Normalizes the color by applying a shift to its RGB components, adjusting the shift based on the color's luminance.
        /// This ensures that the resulting color maintains visual contrast, especially useful for background/foreground differentiation.
        /// </summary>
        /// <param name="color">The original <see cref="Color"/> to be normalized.</param>
        /// <param name="colorShift">
        /// The amount to shift the RGB components of the color (positive value from 0 to 255).
        /// The actual shift applied may be adjusted based on the color's luminance.
        /// </param>
        /// <param name="luminanceThreshold">
        /// The base luminance value used to determine if the color is 'bright'.
        /// If the color's luminance exceeds this threshold, the shift is inverted or doubled to maintain contrast.
        /// Defaults to <see cref="LuminanceThreshold"/>.
        /// </param>
        /// <returns>
        /// A new <see cref="Color"/> with adjusted RGB values, ensuring visual contrast.
        /// The shift is positive for dark colors, negative for moderately bright colors, and doubled negative for very bright colors.
        /// </returns>
        /// <remarks>
        /// This method is typically used to generate visually distinct background colors for UI elements,
        /// ensuring readability and contrast regardless of the base color's brightness.
        /// </remarks>
        /// <example>
        /// <code>
        /// Color baseColor = Color.LightGray;
        /// Color normalizedColor = NormalizeColor(baseColor, 30);
        /// // For a bright color, the result will be darker to maintain contrast.
        /// </code>
        /// </remarks>
        public static Color NormalizeColor(Color color, int colorShift, int luminanceThreshold = LuminanceThreshold)
        {
            return NormalizeColor(color, color, colorShift, luminanceThreshold);
        }

        /// <summary>
        /// Calculates an adjusted color shift value based on the input color's luminance.
        /// This method inverts the shift for bright colors to ensure visual contrast is maintained.
        /// </summary>
        /// <param name="color">The base <see cref="Color"/> whose luminance will be checked.</param>
        /// <param name="colorShift">The original color shift amount (positive value from 0 to 255).</param>
        /// <param name="luminanceThreshold">The base luminance value used to determine if a color is 'bright' (default is 160).</param>
        /// <returns>
        /// A normalized shift value. Returns the original positive shift for dark colors,
        /// a negative shift for moderately bright colors, and a doubled negative shift for very bright colors.
        /// </returns>
        /// <remarks>
        /// This logic is commonly used to shift the background color of an element in a way that
        /// keeps the shifted color visually distinct from the text/foreground, regardless of
        /// whether the base color is dark or light.
        /// </remarks>
        public static int NormalizeColorShift(Color color, int colorShift, int luminanceThreshold = LuminanceThreshold)
        {
            int shift = colorShift;
            shift = shift < 0 ? 0 : shift;
            shift = shift > 255 ? 255 : shift;
            int luminance = ColorLuminance(color);
            int highLuminanceThreshold = (int)(2 * (luminanceThreshold + 255) / 3);
            if (luminance > highLuminanceThreshold)
            {
                return -shift * 2;
            }
            else if (luminance > luminanceThreshold)
            {
                return -shift;
            }
            else
            {
                return shift;
            }
        }

        /// <summary>
        /// Normalizes RGB shift values based on a color's luminance relative to a threshold, reversing the shift direction for colors that exceed the threshold.
        /// This ensures color shifts maintain appropriate contrast across light and dark backgrounds.
        /// </summary>
        /// <param name="color">The color whose luminance determines the shift direction.</param>
        /// <param name="redShift">The base Red channel shift amount. Will be negated if color luminance exceeds threshold.</param>
        /// <param name="greenShift">The base Green channel shift amount. Will be negated if color luminance exceeds threshold.</param>
        /// <param name="blueShift">The base Blue channel shift amount. Will be negated if color luminance exceeds threshold.</param>
        /// <param name="luminanceThreshold">The luminance threshold (0-255) for determining shift direction. Defaults to <see cref="LuminanceThreshold"/>.</param>
        /// <returns>
        /// A tuple containing the normalized (R, G, B) shift values:
        /// <list type="bullet">
        /// <item><description>Item1: Normalized red shift</description></item>
        /// <item><description>Item2: Normalized green shift</description></item>
        /// <item><description>Item3: Normalized blue shift</description></item>
        /// </list>
        /// If color luminance > threshold, all shifts are negated (reversed).
        /// </returns>
        /// <remarks>
        /// This method delegates to individual channel normalization via <see cref="NormalizeColorShift(Color, int, int)"/>.
        /// Use this when you need to normalize all RGB channels simultaneously while maintaining consistent contrast behavior.
        /// </remarks>
        public static Tuple<int, int, int> NormalizeColorShift(Color color, int redShift, int greenShift, int blueShift, int luminanceThreshold = LuminanceThreshold)
        {
            int normalizedRed = NormalizeColorShift(color, colorShift: redShift, luminanceThreshold: luminanceThreshold);
            int normalizedGreen = NormalizeColorShift(color, colorShift: greenShift, luminanceThreshold: luminanceThreshold);
            int normalizedBlue = NormalizeColorShift(color, colorShift: blueShift, luminanceThreshold: luminanceThreshold);

            return Tuple.Create(normalizedRed, normalizedGreen, normalizedBlue);
        }

        /// <summary>
        /// Clamps an integer value to ensure it falls within the standard 8-bit color channel range [0, 255].
        /// </summary>
        /// <param name="v">The integer value to clamp.</param>
        /// <returns>The clamped integer value between 0 and 255.</returns>
        private static int ClampByte(int v) => v < 0 ? 0 : (v > 255 ? 255 : v);

        // --- helpers ---

        /// <summary>
        /// Calculates the perceived luminance of a color on a scale from 0 (darkest) to 255 (lightest),
        /// based on the standard luminance formula using human visual sensitivity coefficients.
        /// </summary>
        /// <param name="ARGB">The color in ARGB to evaluate.</param>
        /// <returns>A value between 0 and 255 representing the perceived brightness of the color.</returns>
        /// <remarks>
        /// Uses the standard luminance formula: Y = 0.299*R + 0.587*G + 0.114*B
        /// These coefficients reflect human eye sensitivity to different color channels.
        /// </remarks>
        public static int ColorLuminance(RGB c)
            => ColorLuminance(Color.FromArgb(c.A, c.R, c.G, c.B));

        /// <summary>
        /// Calculates the perceived luminance of a color on a scale from 0 (darkest) to 255 (lightest),
        /// based on the standard luminance formula using human visual sensitivity coefficients.
        /// </summary>
        /// <param name="color">The color to evaluate.</param>
        /// <returns>A value between 0 and 255 representing the perceived brightness of the color.</returns>
        /// <remarks>
        /// Uses the standard luminance formula: Y = 0.299*R + 0.587*G + 0.114*B
        /// These coefficients reflect human eye sensitivity to different color channels.
        /// </remarks>
        public static int ColorLuminance(Color c)
                => (int)Math.Round(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);

        /// <summary>
        /// Calculates the relative luminance of a color according to WCAG 2.0 specification.
        /// </summary>
        /// <param name="c">The ARGB color to calculate luminance for.</param>
        /// <returns>
        /// The relative luminance value in the range [0.0, 1.0], where 0.0 is black
        /// and 1.0 is white.
        /// </returns>
        /// <remarks>
        /// Uses sRGB linearization and Rec. 709 coefficients as specified by WCAG 2.0.
        /// Formula: L = 0.2126 * R + 0.7152 * G + 0.0722 * B (after gamma correction).
        /// </remarks>
        private static double RelativeLuminance(RGB c)
        => RelativeLuminance(Color.FromArgb(c.A, c.R, c.G, c.B));

        /// <summary>
        /// Calculates the relative luminance of a color according to WCAG 2.0 specification.
        /// </summary>
        /// <param name="c">The color to calculate luminance for.</param>
        /// <returns>
        /// The relative luminance value in the range [0.0, 1.0], where 0.0 is black
        /// and 1.0 is white.
        /// </returns>
        /// <remarks>
        /// Uses sRGB linearization and Rec. 709 coefficients as specified by WCAG 2.0.
        /// Formula: L = 0.2126 * R + 0.7152 * G + 0.0722 * B (after gamma correction).
        /// </remarks>
        public static double RelativeLuminance(Color c)
        {
            // sRGB to linear
            double sr = c.R / 255.0, sg = c.G / 255.0, sb = c.B / 255.0;
            double r = sr <= 0.03928 ? sr / 12.92 : Math.Pow((sr + 0.055) / 1.055, 2.4);
            double g = sg <= 0.03928 ? sg / 12.92 : Math.Pow((sg + 0.055) / 1.055, 2.4);
            double b = sb <= 0.03928 ? sb / 12.92 : Math.Pow((sb + 0.055) / 1.055, 2.4);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        /// <summary>
        /// Blends a foreground color with a background color using alpha compositing.
        /// </summary>
        /// <param name="fg">The foreground color (may have transparency).</param>
        /// <param name="bg">The background color.</param>
        /// <returns>The resulting opaque color after alpha blending.</returns>
        public static Color AlphaBlend(Color fg, Color bg)
        {
            double a = fg.A / 255.0;
            byte r = (byte)Math.Round(fg.R * a + bg.R * (1 - a));
            byte g = (byte)Math.Round(fg.G * a + bg.G * (1 - a));
            byte b = (byte)Math.Round(fg.B * a + bg.B * (1 - a));
            return Color.FromArgb(255, r, g, b); // blended color is opaque
        }

        /// <summary>
        /// Performs linear interpolation between two colors.
        /// </summary>
        /// <param name="a">The starting color.</param>
        /// <param name="b">The ending color.</param>
        /// <param name="t">The interpolation factor (0.0 = color a, 1.0 = color b, clamped to 0-1 range).</param>
        /// <returns>The interpolated color, preserving the alpha channel from color a.</returns>
        public static Color Lerp(Color a, Color b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            byte r = (byte)Math.Round(a.R + (b.R - a.R) * t);
            byte g = (byte)Math.Round(a.G + (b.G - a.G) * t);
            byte bl = (byte)Math.Round(a.B + (b.B - a.B) * t);
            // keep original alpha
            return Color.FromArgb(a.A, r, g, bl);
        }

        /// <summary>
        /// Extracts the lightness component from a color in HSL color space.
        /// </summary>
        /// <param name="c">The color to extract lightness from.</param>
        /// <returns>The lightness value in the range [0.0, 1.0].</returns>
        private static double GetL(Color c)
        {
            ColorToHSL(c, out _, out _, out double l);
            return l;
        }

        // Contrast of the *displayed* foreground (alpha-blended) vs background
        /// <summary>
        /// Calculates the WCAG contrast ratio between two colors after alpha blending the foreground with the background.
        /// </summary>
        /// <param name="fg">The foreground color (may have transparency).</param>
        /// <param name="bg">The background color.</param>
        /// <returns>The contrast ratio as defined by WCAG guidelines (1:1 to 21:1 scale).</returns>
        private static double ContrastRatioComposite(Color fg, Color bg)
        {
            var shown = AlphaBlend(fg, bg);
            return ContrastRatio(shown, bg);
        }

        /// <summary>
        /// Calculates the WCAG 2.0 contrast ratio between two colors.
        /// </summary>
        /// <param name="a">The first color.</param>
        /// <param name="b">The second color.</param>
        /// <returns>
        /// The contrast ratio in the range [1.0, 21.0], where 1.0 is no contrast
        /// (identical colors) and 21.0 is maximum contrast (black vs white).
        /// </returns>
        /// <remarks>
        /// WCAG 2.0 contrast requirements:
        /// - Level AA: 4.5:1 for normal text, 3:1 for large text
        /// - Level AAA: 7:1 for normal text, 4.5:1 for large text
        /// </remarks>
        private static double ContrastRatio(Color a, Color b)
        {
            double La = RelativeLuminance(a);
            double Lb = RelativeLuminance(b);
            double lighter = Math.Max(La, Lb);
            double darker = Math.Min(La, Lb);
            return (lighter + 0.05) / (darker + 0.05);
        }

        /// <summary>
        /// Converts a color from RGB to HSL (Hue, Saturation, Lightness) color space.
        /// </summary>
        /// <param name="color">The RGB color to convert.</param>
        /// <param name="hue">
        /// Output parameter for the hue component in degrees [0.0, 360.0).
        /// Red = 0°, Green = 120°, Blue = 240°.
        /// </param>
        /// <param name="saturation">
        /// Output parameter for the saturation component [0.0, 1.0],
        /// where 0.0 is grayscale and 1.0 is fully saturated.
        /// </param>
        /// <param name="lightness">
        /// Output parameter for the lightness component [0.0, 1.0],
        /// where 0.0 is black, 0.5 is pure color, and 1.0 is white.
        /// </param>
        private static void ColorToHSL(Color color, out double hue, out double saturation, out double lightness)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            lightness = (max + min) / 2.0;

            if (delta == 0)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                saturation = lightness > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

                if (max == r) hue = ((g - b) / delta + (g < b ? 6 : 0)) * 60;
                else if (max == g) hue = ((b - r) / delta + 2) * 60;
                else hue = ((r - g) / delta + 4) * 60;
            }
        }

        /// <summary>
        /// Converts HSL (Hue, Saturation, Lightness) color values to an RGB Color.
        /// </summary>
        /// <param name="hue">
        /// The hue component in degrees [0.0, 360.0). Values outside this range
        /// are automatically normalized. Red = 0°, Green = 120°, Blue = 240°.
        /// </param>
        /// <param name="saturation">
        /// The saturation component [0.0, 1.0], where 0.0 is grayscale
        /// and 1.0 is fully saturated.
        /// </param>
        /// <param name="lightness">
        /// The lightness component [0.0, 1.0], where 0.0 is black,
        /// 0.5 is pure color, and 1.0 is white.
        /// </param>
        /// <returns>
        /// An RGB Color with components clamped to the valid range [0, 255].
        /// </returns>
        private static Color HSLToColor(double hue, double saturation, double lightness)
        {
            hue = (hue % 360 + 360) % 360; // normalize
            double c = (1.0 - Math.Abs(2.0 * lightness - 1.0)) * saturation;
            double x = c * (1.0 - Math.Abs((hue / 60.0) % 2.0 - 1.0));
            double m = lightness - c / 2.0;

            double r, g, b;
            if (hue < 60) { r = c; g = x; b = 0; }
            else if (hue < 120) { r = x; g = c; b = 0; }
            else if (hue < 180) { r = 0; g = c; b = x; }
            else if (hue < 240) { r = 0; g = x; b = c; }
            else if (hue < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            int R = (int)Math.Round((r + m) * 255);
            int G = (int)Math.Round((g + m) * 255);
            int B = (int)Math.Round((b + m) * 255);

            return Color.FromArgb(
                Math.Max(0, Math.Min(255, R)),
                Math.Max(0, Math.Min(255, G)),
                Math.Max(0, Math.Min(255, B))
            );
        }

        /// <summary>
        /// Converts a .NET Color to HTML hex format (#RRGGBB)
        /// </summary>
        /// <param name="color">The Color to convert</param>
        /// <returns>HTML hex color string</returns>
        public static string ColorToHtml(Color color)
        {
            if (color == Color.Empty)
            {
                return "#FFFFFF";  // Default to white if empty
            }
            return string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        /// <summary>
        /// Adjusts the foreground color so that it has at least the specified
        /// luminance difference (contrast) from the background color.
        /// </summary>
        /// <param name="targetColor">
        /// The original foreground <see cref="Color"/> that will be adjusted
        /// if its luminance is too close to <paramref name="referenceColor"/>.
        /// </param>
        /// <param name="referenceColor">
        /// The background <see cref="Color"/> used as the reference when
        /// computing the luminance difference.
        /// </param>
        /// <param name="threshold">
        /// Minimum allowed luminance difference between foreground and background,
        /// in the same units returned by <c>ColorLuminance</c> (typically 0–255).
        /// If the current difference is already greater than or equal to this
        /// value, the original foreground color is returned unchanged.
        /// </param>
        /// <returns>
        /// A <see cref="Color"/> based on <paramref name="targetColor"/> whose
        /// luminance has been shifted lighter or darker so that its luminance
        /// differs from <paramref name="referenceColor"/> by at least
        /// <paramref name="threshold"/>.
        /// </returns>
        /// <remarks>
        /// This method uses <c>ColorLuminance</c> to compute perceived brightness
        /// for both colors. If the current luminance difference is below
        /// <paramref name="threshold"/>, it computes a new target luminance on the
        /// lighter or darker side of <paramref name="referenceColor"/> (depending on
        /// whether <paramref name="targetColor"/> is currently lighter or darker),
        /// then calls <c>ShiftColor</c> to generate a new foreground color with
        /// that target luminance.
        /// </remarks>
        public static Color AdjustContrast(Color targetColor, Color referenceColor, int threshold = 60)
        {
            int foreLuma = ColorLuminance(targetColor);
            int backLuma = ColorLuminance(referenceColor);

            int diff = foreLuma - backLuma;
            int absDiff = Math.Abs(diff);

            if (absDiff >= threshold)
                return targetColor;

            int direction = diff >= 0 ? 1 : -1;
            int newLuma = direction * threshold;

            return ShiftColor(targetColor, newLuma);
        }

        /// <summary>
        /// Adjusts the alpha (transparency/opacity) value of a color by a specified amount or sets it to an explicit value.
        /// </summary>
        /// <param name="color">The color whose alpha value should be adjusted.</param>
        /// <param name="alphaShift">
        /// The explicit alpha shift value to set (0-255). 
        /// </param>
        /// <returns>
        /// A new <see cref="Color"/> with the adjusted or explicitly set alpha shift value. The RGB components remain unchanged.
        /// </returns>
        public static Color ShiftAlpha(Color color, int alphaShift)
        {
            alphaShift = Math.Clamp(alphaShift, 0, 255);

            return Color.FromArgb(alphaShift, color.R, color.G, color.B);
        }

        /// <summary>
        /// Shifts the RGB values of a color by a specified amount, with the direction determined by the relative luminance of the input color.
        /// If the input color is lighter than the foreground, shifts in the negative direction (darker).
        /// This helps maintain appropriate contrast between foreground and reference colors.
        /// </summary>
        /// <param name="color">The color to shift.</param>
        /// <param name="rgbShift">The amount to shift the RGB components by. The actual direction depends on the reference color's luminance.</param>
        /// <param name="alphaShift">The amount to shift the Alpha component by. Positive amounts make the color more opaque and negative amounts make it more transparent.</param>
        /// <param name="luminanceThreshold">The threshold luminance amount which backcolor luminance is compare to.</param>
        /// <returns>A new Color with RGB values shifted to maintain appropriate contrast with the reference.</returns>
        public static Color ShiftColor(Color color, int rgbShift, int alphaShift, int luminanceThreshold)
        {
            if (ColorLuminance(color) > SafeLuminanceThreshold(luminanceThreshold))
            {
                return ShiftColor(color, -rgbShift, alphaShift);
            }
            else
            {
                return ShiftColor(color, rgbShift, alphaShift);
            }
        }

        /// <summary>
        /// Shifts the ARGB values of a color by specified amounts, with automatic direction reversal based on luminance to maintain contrast.
        /// If the input color's luminance exceeds the threshold (lighter colors), shifts are applied in the negative direction (darker).
        /// If the input color's luminance is at or below the threshold (darker colors), shifts are applied in the positive direction (lighter).
        /// This ensures the shifted color maintains appropriate contrast relative to the luminance threshold.
        /// </summary>
        /// <param name="color">The color to shift.</param>
        /// <param name="redShift">The base amount to shift the Red component by. Direction is automatically reversed for lighter colors.</param>
        /// <param name="greenShift">The base amount to shift the Green component by. Direction is automatically reversed for lighter colors.</param>
        /// <param name="blueShift">The base amount to shift the Blue component by. Direction is automatically reversed for lighter colors.</param>
        /// <param name="alphaShift">The amount to shift the Alpha component by. Positive amounts make the color more opaque (less transparent) and negative amounts make it more transparent. This value is not reversed.</param>
        /// <param name="luminanceThreshold">The luminance threshold (0-255) used to determine shift direction. Colors with luminance above this value are shifted darker, colors at or below are shifted lighter.</param>
        /// <returns>A new <see cref="Color"/> with ARGB values shifted to maintain appropriate contrast, clamped to valid range (0-255).</returns>
        /// <remarks>
        /// This method provides contrast-aware color shifting:
        /// <list type="bullet">
        /// <item><description>Light colors (luminance > threshold): Shifts become negative (darker)</description></item>
        /// <item><description>Dark colors (luminance ≤ threshold): Shifts remain positive (lighter)</description></item>
        /// </list>
        /// Example: On a light background (luminance 200, threshold 128), a color with redShift=30 will shift by -30 (darker).
        /// On a dark background (luminance 50), the same color shifts by +30 (lighter).
        /// </remarks>
        public static Color ShiftColor(Color color, int redShift, int greenShift, int blueShift, int alphaShift, int luminanceThreshold)
        {
            if (ColorLuminance(color) > SafeLuminanceThreshold(luminanceThreshold))
            {
                return ShiftColor(color, -redShift, -greenShift, -blueShift, alphaShift);
            }
            else
            {
                return ShiftColor(color, redShift, greenShift, blueShift, alphaShift);
            }
        }

        /// <summary>
        /// Shifts the RGB values of the input color by a specified amount, clamping between 0 and 255.
        /// </summary>
        /// <param name="color">The original color to shift.</param>
        /// <param name="rgbShift">The amount to shift the RGB components by (positive or negative).</param>
        /// <param name="alphaShift">The amount to shift the Alpha component by. 
        /// Positive amounts make the color more opaque and negative amounts make it more transparent.</param>
        /// <returns>A new Color with shifted RGB values.</returns>
        public static Color ShiftColor(Color color, int rgbShift, int alphaShift)
        {
            int a = Clamp(color.A + alphaShift, 0, 255);
            int r = Clamp(color.R + rgbShift, 0, 255);
            int g = Clamp(color.G + rgbShift, 0, 255);
            int b = Clamp(color.B + rgbShift, 0, 255);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Shifts the ARGB values of the input color by specified amounts for each channel, clamping between 0 and 255.
        /// </summary>
        /// <param name="color">The original color to shift.</param>
        /// <param name="redShift">The amount to shift the Red component by (positive or negative).</param>
        /// <param name="greenShift">The amount to shift the Green component by (positive or negative).</param>
        /// <param name="blueShift">The amount to shift the Blue component by (positive or negative).</param>
        /// <param name="alphaShift">The amount to shift the Alpha component by. Positive amounts make the color 
        /// more opaque (less transparent) and negative amounts make it more transparent.</param>
        /// <returns>A new <see cref="Color"/> with shifted ARGB values, clamped to valid range (0-255).</returns>
        /// <remarks>
        /// Each color channel is shifted independently and clamped to ensure valid color values.
        /// For example, shifting red by +50 on a color with R=220 results in R=255 (clamped).
        /// </remarks>
        public static Color ShiftColor(Color color, int redShift, int greenShift, int blueShift, int alphaShift)
        {
            int a = Clamp(color.A + alphaShift, 0, 255);
            int r = Clamp(color.R + redShift, 0, 255);
            int g = Clamp(color.G + greenShift, 0, 255);
            int b = Clamp(color.B + blueShift, 0, 255);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Shifts the RGB values of the input color by a specified amount, clamping between 0 and 255.
        /// </summary>
        /// <param name="color">The original color to shift.</param>
        /// <param name="rgbShift">The amount to shift the RGB components by (positive or negative).</param>
        /// <returns>A new Color with shifted RGB values.</returns>
        public static Color ShiftColor(Color color, int rgbShift)
        {
            return ShiftColor(color, rgbShift, 0);
        }

        /// <summary>
        /// Shifts a color's RGB values based on the luminance of a reference color.
        /// Shifts negatively if reference color is bright, positively if dark.
        /// </summary>
        /// <param name="color">The color to shift.</param>
        /// <param name="referenceColor">The color whose luminance determines shift direction.</param>
        /// <param name="rgbShift">The amount to shift all RGB channels (positive or negative based on reference luminance).</param>
        /// <param name="alphaShift">The amount to shift the alpha channel.</param>
        /// <param name="luminanceThreshold">The luminance threshold (0-255) that determines bright vs dark.</param>
        /// <returns>A new color with shifted RGB and alpha values.</returns>
        public static Color ShiftColor(Color color, Color referenceColor, int rgbShift, int alphaShift, int luminanceThreshold)
        {
            if (ColorLuminance(referenceColor) > SafeLuminanceThreshold(luminanceThreshold))
            {
                return ShiftColor(color, -rgbShift, alphaShift);
            }
            else
            {
                return ShiftColor(color, rgbShift, alphaShift);
            }
        }

        /// <summary>
        /// Shifts a color's individual RGB channels based on the luminance of the color itself.
        /// Shifts negatively if the color is bright, positively if dark.
        /// </summary>
        /// <param name="color">The color to shift.</param>
        /// <param name="referenceColor">Not used in this overload (parameter name suggests intent but implementation uses color's own luminance).</param>
        /// <param name="redShift">The amount to shift the red channel (positive or negative based on color luminance).</param>
        /// <param name="greenShift">The amount to shift the green channel (positive or negative based on color luminance).</param>
        /// <param name="blueShift">The amount to shift the blue channel (positive or negative based on color luminance).</param>
        /// <param name="alphaShift">The amount to shift the alpha channel.</param>
        /// <param name="luminanceThreshold">The luminance threshold (0-255) that determines bright vs dark.</param>
        /// <returns>A new color with shifted RGB and alpha values.</returns>
        public static Color ShiftColor(Color color, Color referenceColor, int redShift, int greenShift, int blueShift, int alphaShift, int luminanceThreshold)
        {
            if (ColorLuminance(referenceColor) > SafeLuminanceThreshold(luminanceThreshold))
            {
                return ShiftColor(color, -redShift, -greenShift, -blueShift, alphaShift);
            }
            else
            {
                return ShiftColor(color, redShift, greenShift, blueShift, alphaShift);
            }
        }

        /// <summary>
        /// Shifts a color's RGB values based on the luminance of a reference color, with no alpha shift.
        /// Shifts negatively if reference color is bright, positively if dark.
        /// </summary>
        /// <param name="color">The color to shift.</param>
        /// <param name="referenceColor">The color whose luminance determines shift direction.</param>
        /// <param name="rgbShift">The amount to shift all RGB channels (positive or negative based on reference luminance).</param>
        /// <returns>A new color with shifted RGB values and unchanged alpha.</returns>
        public static Color ShiftColor(Color color, Color referenceColor, int rgbShift, int alphaShift = 0)
        {
            if (ColorLuminance(referenceColor) > SafeLuminanceThreshold(ColorHelper.LuminanceThreshold))
            {
                return ShiftColor(color, -rgbShift, alphaShift);
            }
            else
            {
                return ShiftColor(color, rgbShift, alphaShift);
            }
        }

        /// <summary>
        /// Ensures the luminance threshold value is within the valid range of 0 to 255.
        /// </summary>
        /// <param name="luminanceThreshold">The proposed luminance threshold value.</param>
        /// <returns>
        /// The input <paramref name="luminanceThreshold"/> if it is between 0 and 255 (inclusive).
        /// Otherwise, returns the default fallback value specified by <c>ColorHelper.LuminanceThreshold</c>.
        /// </returns>
        /// <remarks>
        /// Luminance threshold is used in color-related calculations to determine perceived brightness,
        /// and its value must correspond to the 8-bit color range.
        /// </remarks>
        private static int SafeLuminanceThreshold(int luminanceThreshold)
        {
            return luminanceThreshold >= 0 && luminanceThreshold <= 255 ? luminanceThreshold : ColorHelper.LuminanceThreshold;
        }

        /// <summary>
        /// Clamps an integer value to ensure it stays within the specified minimum and maximum bounds.
        /// </summary>
        /// <param name="value">The value to be clamped.</param>
        /// <param name="min">The minimum allowable value.</param>
        /// <param name="max">The maximum allowable value.</param>
        /// <returns>
        /// The clamped value: 
        /// - Returns <paramref name="min"/> if <paramref name="value"/> is less than <paramref name="min"/>.
        /// - Returns <paramref name="max"/> if <paramref name="value"/> is greater than <paramref name="max"/>.
        /// - Otherwise, returns <paramref name="value"/>.
        /// </returns>
        public static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        /// <summary>
        /// Determines whether the specified string is a valid RGB color representation.
        /// Only accepts formats without alpha channels (named colors, #RRGGBB, #RGB, R=n,G=n,B=n).
        /// </summary>
        /// <param name="colorString">The string to validate as an RGB color.</param>
        /// <param name="strictRgbOnly">If true, rejects formats that include alpha channels.</param>
        /// <returns>
        /// <c>true</c> if the string represents a valid RGB color; otherwise, <c>false</c>.
        /// </returns>
        /// <example>
        /// Valid strict RGB formats:
        /// - Named: "Red", "CornflowerBlue"
        /// - Hex: "#FF5733", "#F57" (shorthand)
        /// - RGB: "R=255, G=0, B=0"
        /// - ToString: "Color [Red]"
        /// Invalid when strict:
        /// - "#80FF5733" (has alpha)
        /// - "A=255, R=255, G=0, B=0" (has alpha)
        /// </example>
        public static bool IsRGB(string colorString, bool strictRgbOnly = false)
        {
            if (!strictRgbOnly)
                return IsColor(colorString);

            if (string.IsNullOrWhiteSpace(colorString))
                return false;

            colorString = colorString.Trim();

            // Remove "Color [" and "]" if present
            if (colorString.StartsWith("Color [", StringComparison.OrdinalIgnoreCase) &&
                colorString.EndsWith("]"))
            {
                colorString = colorString.Substring(7, colorString.Length - 8);
            }

            // Check for hex color format
            if (colorString.StartsWith("#"))
            {
                string hex = colorString.Substring(1);
                // Only accept 3 or 6 character hex (no alpha)
                return (hex.Length == 3 || hex.Length == 6) &&
                       hex.All(c => Uri.IsHexDigit(c));
            }

            // Check for named color (these don't have explicit alpha)
            Color namedColor = Color.FromName(colorString);
            if (namedColor.IsKnownColor)
                return true;

            // Check for RGB format: "R=255, G=0, B=0" (no A=)
            if (colorString.Contains("="))
            {
                // Reject if contains alpha component
                if (colorString.Contains("A=", StringComparison.OrdinalIgnoreCase))
                    return false;

                var parts = colorString.Split(',');
                var values = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length != 2)
                        return false;

                    string key = keyValue[0].Trim().ToUpper();
                    if (!int.TryParse(keyValue[1].Trim(), out int value))
                        return false;

                    if (value < 0 || value > 255)
                        return false;

                    if (key != "R" && key != "G" && key != "B")
                        return false;

                    values[key] = value;
                }

                // Must have exactly R, G, B (no A)
                return values.ContainsKey("R") && values.ContainsKey("G") &&
                       values.ContainsKey("B") && values.Count == 3;
            }

            return false;
        }

        /// <summary>
        /// Attempts to extract a <see cref="Color"/> from a <see cref="Brush"/>.
        /// </summary>
        /// <param name="brush">The brush to extract the color from.</param>
        /// <param name="fallback">
        /// The color to return if the brush is null or not a <see cref="SolidBrush"/>.
        /// </param>
        /// <param name="alphaShift">
        /// The alpha adjustment (-255 to 255). 
        /// Positive values decrease opacity, negative values increase opacity.
        /// </param>
        /// <returns>
        /// The brush color if the brush is a <see cref="SolidBrush"/>; otherwise, <paramref name="fallback"/>.
        /// </returns>
        public static Color BrushToColor(Brush brush, Color fallback, int alphaShift = 0)
        {
            if (brush == null)
                return ShiftAlpha(fallback, alphaShift);

            if (brush is SolidBrush sb)
                return ShiftAlpha(sb.Color, alphaShift);

            return ShiftAlpha(fallback, alphaShift);
        }

        /// <summary>
        /// Converts a <see cref="Color"/> to a <see cref="SolidBrush"/>.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>A new <see cref="SolidBrush"/> using the specified color.</returns>
        public static SolidBrush ToBrush(Color color)
        {
            // Caller should Dispose() the returned brush when done.
            return new SolidBrush(color);
        }

        /// <summary>
        /// Converts a <see cref="Color"/> to an <see cref="RGB"/> struct.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>An RGB struct containing the red, green, and blue components of the color.</returns>
        public static RGB ToRGB(Color color)
        {
            return new RGB(color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts a color string representation to an <see cref="RGB"/> struct.
        /// </summary>
        /// <param name="colorString">The color string to convert. Supports hex format (#RRGGBB, #RGB) and named colors.</param>
        /// <returns>An RGB struct containing the red, green, and blue components of the color.</returns>
        /// <exception cref="ArgumentException">Thrown when the color string format is invalid.</exception>
        public static RGB ToRGB(string colorString)
        {
            return ToRGB(ToColor(colorString));
        }

        /// <summary>
        /// Determines whether the specified color is considered a light color based on its luminance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to evaluate.</param>
        /// <param name="referenceLuminance">
        /// The luminance threshold used to determine if the color is light.
        /// Defaults to <see cref="LuminanceThreshold"/> if not specified.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the color's luminance is greater than the <paramref name="referenceLuminance"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method is useful for dynamically choosing between light and dark themes,
        /// or for ensuring contrast between foreground and background colors.
        /// </remarks>
        public static bool IsLightColor(Color color, int referenceLuminance = LuminanceThreshold)
        {
            return ColorLuminance(color) > referenceLuminance;
        }

        /// <summary>
        /// Determines whether the specified string is a valid color representation.
        /// Supports named colors, ARGB format ("A=255, R=255, G=0, B=0"), 
        /// Color.ToString() format ("Color [Red]"), and hex format (#RRGGBB, #AARRGGBB).
        /// </summary>
        /// <param name="colorString">The string to validate as a color.</param>
        /// <returns>
        /// <c>true</c> if the string represents a valid color; otherwise, <c>false</c>.
        /// Returns <c>false</c> for null, empty, or whitespace strings.
        /// </returns>
        /// <example>
        /// Valid formats:
        /// - Named: "Red", "CornflowerBlue"
        /// - ToString: "Color [Red]", "Color [A=255, R=255, G=0, B=0]"
        /// - ARGB: "A=255, R=255, G=0, B=0"
        /// - Hex: "#FF5733", "#80FF5733"
        /// </example>
        public static bool IsColor(string colorString)
        {
            if (string.IsNullOrWhiteSpace(colorString))
                return false;

            colorString = colorString.Trim().ToLower().Replace(" ", "");

            // Remove "Color [" and "]" if present
            if (colorString.StartsWith("Color[", StringComparison.OrdinalIgnoreCase) &&
                colorString.EndsWith("]"))
            {
                colorString = colorString.Substring(6, colorString.Length - 8);
            }

            // Check for hex color format
            if (colorString.StartsWith("#"))
            {
                string hex = colorString.Substring(1);
                // Valid hex: 3, 6, or 8 characters
                return (hex.Length == 3 || hex.Length == 6 || hex.Length == 8) &&
                       hex.All(c => Uri.IsHexDigit(c));
            }

            // Check for named color
            Color namedColor = Color.FromName(colorString);
            if (namedColor.IsKnownColor)
                return true;

            // Check for ARGB format: "A=255, R=255, G=0, B=0"
            if (colorString.Contains("="))
            {
                var parts = colorString.Split(',');
                var values = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length != 2)
                        return false;

                    string key = keyValue[0].Trim().ToUpper();
                    if (!int.TryParse(keyValue[1].Trim(), out int value))
                        return false;

                    if (value < 0 || value > 255)
                        return false;

                    if (key != "A" && key != "R" && key != "G" && key != "B")
                        return false;

                    values[key] = value;
                }

                // Must have at least R, G, B
                return values.ContainsKey("R") && values.ContainsKey("G") && values.ContainsKey("B");
            }

            return false;
        }

        /// <summary>
        /// Converts a RGB representation of a color back to a Color object.
        /// </summary>
        /// <param name="color">The RGB representation of the color.</param>
        /// <param name="alphaShift">The Alpha amount by which the color is shifted. 
        /// Higher value, makes color more opaque, while lower values makes it more transparent</param>
        /// <returns>The parsed Color.</returns>
        public static Color ToColor(RGB color, int alphaShift = 0)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts a string representation of a Color (from Color.ToString()) back to a Color object.
        /// Handles both named colors ("Color [Red]") and ARGB format ("Color [A=255, R=255, G=0, B=0]").
        /// </summary>
        /// <param name="colorString">The string representation of the color.</param>
        /// <param name="alphaShift">The Alpha amount by which the color is shifted. 
        /// Higher value, makes color more opaque, while lower values makes it more transparent</param>
        /// <returns>The parsed Color, or Color.Empty if parsing fails.</returns>
        public static Color ToColor(string colorString, int alphaShift = 0)
        {
            if (string.IsNullOrWhiteSpace(colorString))
                return Color.Empty;

            // Normalize the string
            string cleaned = colorString.Trim()
                                        .Replace("Color [", "", StringComparison.OrdinalIgnoreCase)
                                        .Replace("Color[", "", StringComparison.OrdinalIgnoreCase)
                                        .Replace("]", "")
                                        .Trim();

            // --- 1) Try match named color (SkyBlue, LightGreen, etc.) ---
            // Try parse using Color.FromName (case-insensitive)
            Color named = Color.FromName(cleaned);

            if (named.A != 0 || named == Color.Black || cleaned.Equals("black", StringComparison.OrdinalIgnoreCase))
            {
                // It's a valid known color name
                if (alphaShift != 0)
                    named = Color.FromArgb(
                        Clamp(named.A + alphaShift, 0, 255),
                        named.R,
                        named.G,
                        named.B);

                return named;
            }

            // --- 2) If not a named color, continue with ARGB parsing ---
            string[] arr = cleaned.ToLower()
                                  .Replace(" ", "")
                                  .Replace("argb(", "")
                                  .Replace("(", "")
                                  .Split(",");

            if (arr[0] == "empty")
                return Color.Empty;

            int a = Clamp((arr[0].Contains("=") ? ToInt(arr[0].Split("=")[1]) : ToInt(arr[0])) + alphaShift, 0, 255);
            int r = Clamp((arr[1].Contains("=") ? ToInt(arr[1].Split("=")[1]) : ToInt(arr[1])), 0, 255);
            int g = Clamp((arr[2].Contains("=") ? ToInt(arr[2].Split("=")[1]) : ToInt(arr[2])), 0, 255);
            int b = Clamp((arr[3].Contains("=") ? ToInt(arr[3].Split("=")[1]) : ToInt(arr[3])), 0, 255);

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Adjusts the foreground <see cref="Brush"/> so that its color has at least
        /// the specified luminance difference (contrast) from the background brush.
        /// </summary>
        /// <param name="foreBrush">
        /// The original foreground <see cref="Brush"/> whose color will be adjusted
        /// if its luminance is too close to that of <paramref name="backBrush"/>.
        /// Must be a <see cref="SolidBrush"/>.
        /// </param>
        /// <param name="backBrush">
        /// The background <see cref="Brush"/> used as the luminance reference.
        /// Must be a <see cref="SolidBrush"/>.
        /// </param>
        /// <param name="threshold">
        /// Minimum allowed luminance difference between foreground and background
        /// (0–255 scale). If the existing difference meets or exceeds this value,
        /// the original brush is returned unchanged.
        /// </param>
        /// <returns>
        /// A new <see cref="SolidBrush"/> based on the color of
        /// <paramref name="foreBrush"/>, shifted lighter or darker so its luminance
        /// differs from <paramref name="backBrush"/> by at least <paramref name="threshold"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if either brush is not a <see cref="SolidBrush"/>.
        /// </exception>
        /// <remarks>
        /// This method extracts the RGB color from each <see cref="SolidBrush"/>,
        /// computes luminance using <c>ColorLuminance</c>, and if the difference
        /// is below the threshold, adjusts the foreground color by shifting its
        /// luminance upward or downward (depending on which side of the background
        /// it lies) using <c>ShiftColor</c>.
        /// </remarks>
        public static Brush AdjustContrast(Brush foreBrush, Brush backBrush, int threshold = 60)
        {
            if (foreBrush is not SolidBrush foreSolid)
                throw new ArgumentException("Foreground brush must be a SolidBrush.", nameof(foreBrush));

            if (backBrush is not SolidBrush backSolid)
                throw new ArgumentException("Background brush must be a SolidBrush.", nameof(backBrush));

            Color foreColor = foreSolid.Color;
            Color backColor = backSolid.Color;

            int foreLuma = ColorHelper.ColorLuminance(foreColor);
            int backLuma = ColorHelper.ColorLuminance(backColor);

            int diff = foreLuma - backLuma;
            int absDiff = Math.Abs(diff);

            // Already enough contrast → no change
            if (absDiff >= threshold)
                return new SolidBrush(foreColor);

            // Choose direction: lighten or darken
            int direction = diff >= 0 ? 1 : -1;

            // Required luminance shift amount
            int newLuma = direction * threshold;

            // Shift only RGB components; alpha is preserved
            Color shifted = ColorHelper.ShiftColor(foreColor, newLuma, 0);

            return new SolidBrush(shifted);
        }

        /// <summary>
        /// Determines whether the specified string is a valid hexadecimal color representation.
        /// Supports standard hex formats: #RGB, #RRGGBB, and #AARRGGBB.
        /// </summary>
        /// <param name="colorString">The string to validate as a hex color.</param>
        /// <returns>
        /// <c>true</c> if the string represents a valid hexadecimal color; otherwise, <c>false</c>.
        /// Returns <c>false</c> for null, empty, or whitespace strings.
        /// </returns>
        /// <remarks>
        /// The hash symbol (#) prefix is required. All hexadecimal digits (0-9, A-F) are case-insensitive.
        /// </remarks>
        /// <example>
        /// Valid formats:
        /// - "#RGB" (shorthand): "#F00", "#09C"
        /// - "#RRGGBB" (standard): "#FF0000", "#0099CC"
        /// - "#AARRGGBB" (with alpha): "#80FF0000", "#FF0099CC"
        /// Invalid formats:
        /// - "FF0000" (missing #)
        /// - "#GGHHII" (invalid hex digits)
        /// - "#12345" (wrong length)
        /// </example>
        public static bool IsHexColor(string colorString)
        {
            if (string.IsNullOrWhiteSpace(colorString))
                return false;

            colorString = colorString.Trim();

            // Must start with #
            if (!colorString.StartsWith("#"))
                return false;

            string hex = colorString.Substring(1);

            // Valid lengths: 3 (#RGB), 6 (#RRGGBB), or 8 (#AARRGGBB)
            if (hex.Length != 3 && hex.Length != 6 && hex.Length != 8)
                return false;

            // All characters must be valid hex digits
            return hex.All(c => Uri.IsHexDigit(c));
        }

        /// <summary>
        /// Converts a <see cref="Color"/> to a hexadecimal color string.
        /// </summary>
        /// <param name="c">The color to convert. Can be null.</param>
        /// <returns>
        /// A hexadecimal string representation of the color in the format "#RRGGBB".
        /// Returns an empty string if the input color is null.
        /// </returns>
        public static String ToHexColor(Color? c)
        {
            return c == null ? string.Empty : "#" + ((Color)c).R.ToString("X2") + ((Color)c).G.ToString("X2") + ((Color)c).B.ToString("X2");
        }

        /// <summary>
        /// Determines whether the specified string is a valid HSV (Hue, Saturation, Value) color representation.
        /// Supports formats: "H=hue, S=saturation, V=value" or "hsv(hue, saturation, value)".
        /// </summary>
        /// <param name="colorString">The string to validate as an HSV color.</param>
        /// <returns>
        /// <c>true</c> if the string represents a valid HSV color; otherwise, <c>false</c>.
        /// Returns <c>false</c> for null, empty, or whitespace strings.
        /// </returns>
        /// <remarks>
        /// Valid ranges:
        /// <list type="bullet">
        /// <item><term>H (Hue)</term><description>0.0 to 360.0 degrees</description></item>
        /// <item><term>S (Saturation)</term><description>0.0 to 1.0 (or 0% to 100%)</description></item>
        /// <item><term>V (Value)</term><description>0.0 to 1.0 (or 0% to 100%)</description></item>
        /// </list>
        /// Percentage values (e.g., "50%") are automatically converted to decimal (0.5).
        /// </remarks>
        /// <example>
        /// Valid formats:
        /// - "H=180, S=0.5, V=0.75"
        /// - "H=180, S=50%, V=75%"
        /// - "hsv(180, 0.5, 0.75)"
        /// - "HSV(180, 50%, 75%)"
        /// Invalid formats:
        /// - "H=400, S=0.5, V=0.75" (hue out of range)
        /// - "H=180, S=1.5, V=0.75" (saturation out of range)
        /// </example>
        public static bool IsHsvColor(string colorString)
        {
            if (string.IsNullOrWhiteSpace(colorString))
                return false;

            colorString = colorString.Trim();

            // Check for function notation: hsv(h, s, v)
            if (colorString.StartsWith("hsv(", StringComparison.OrdinalIgnoreCase) &&
                colorString.EndsWith(")"))
            {
                string values = colorString.Substring(4, colorString.Length - 5);
                return ValidateHsvValues(values, useFunctionFormat: true);
            }

            // Check for key-value format: H=h, S=s, V=v
            if (colorString.Contains("="))
            {
                return ValidateHsvValues(colorString, useFunctionFormat: false);
            }

            return false;
        }

        /// <summary>
        /// Validates HSV color values from a string.
        /// </summary>
        private static bool ValidateHsvValues(string values, bool useFunctionFormat)
        {
            var parts = values.Split(',');

            if (parts.Length != 3)
                return false;

            double h = 0, s = 0, v = 0;
            bool hasH = false, hasS = false, hasV = false;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();

                if (useFunctionFormat)
                {
                    // Function format: just values in order (h, s, v)
                    if (!TryParseHsvValue(part, out double value))
                        return false;

                    switch (i)
                    {
                        case 0: h = value; hasH = true; break;
                        case 1: s = value; hasS = true; break;
                        case 2: v = value; hasV = true; break;
                    }
                }
                else
                {
                    // Key-value format: H=value, S=value, V=value
                    var keyValue = part.Split('=');
                    if (keyValue.Length != 2)
                        return false;

                    string key = keyValue[0].Trim().ToUpper();
                    if (!TryParseHsvValue(keyValue[1].Trim(), out double value))
                        return false;

                    switch (key)
                    {
                        case "H": h = value; hasH = true; break;
                        case "S": s = value; hasS = true; break;
                        case "V": v = value; hasV = true; break;
                        default: return false;
                    }
                }
            }

            // All three components must be present
            if (!hasH || !hasS || !hasV)
                return false;

            // Validate ranges
            if (h < 0 || h > 360)
                return false;
            if (s < 0 || s > 1)
                return false;
            if (v < 0 || v > 1)
                return false;

            return true;
        }

        /// <summary>
        /// Tries to parse an HSV component value, handling both decimal and percentage formats.
        /// </summary>
        private static bool TryParseHsvValue(string valueString, out double result)
        {
            result = 0;
            valueString = valueString.Trim();

            if (string.IsNullOrEmpty(valueString))
                return false;

            // Handle percentage values (e.g., "50%")
            bool isPercentage = valueString.EndsWith("%");
            if (isPercentage)
            {
                valueString = valueString.Substring(0, valueString.Length - 1).Trim();
            }

            if (!double.TryParse(valueString, out double value))
                return false;

            // Convert percentage to decimal (0-100% -> 0.0-1.0)
            if (isPercentage)
            {
                if (value < 0 || value > 100)
                    return false;
                result = value / 100.0;
            }
            else
            {
                result = value;
            }

            return true;
        }

        public static int ToInt(object input)
        {
            return Information.IsNumeric(input) ? Convert.ToInt32(input) : default;
        }

        /// <summary>
        /// Converts a <see cref="T:System.Drawing.Color"/> (RGB) value to its corresponding HSV (Hue, Saturation, Value) representation.
        /// </summary>
        /// <param name="c">The <see cref="T:System.Drawing.Color"/> value to convert. Must not be null.</param>
        /// <returns>
        /// A tuple containing the HSV components:
        /// <list type="bullet">
        /// <item><term>H (Hue)</term><description>The hue component in degrees (0.0 to 360.0).</description></item>
        /// <item><term>S (Saturation)</term><description>The saturation component (0.0 to 1.0).</description></item>
        /// <item><term>V (Value/Brightness)</term><description>The value/brightness component (0.0 to 1.0).</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if the input color parameter <paramref name="c"/> is null.</exception>
        /// <remarks>
        /// This method first converts the RGB components to the range [0, 1] and then uses the standard algorithm to calculate HSV.
        /// </remarks>
        public static (double H, double S, double V) ToHsvColor(Color? c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));

            var color = c.Value;

            // Convert RGB to [0,1] range
            double red = color.R / 255.0;
            double green = color.G / 255.0;
            double blue = color.B / 255.0;

            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));
            double delta = max - min;

            double h = 0.0;
            if (delta > 0)
            {
                if (max == red)
                {
                    h = 60 * (((green - blue) / delta) % 6);
                }
                else if (max == green)
                {
                    h = 60 * (((blue - red) / delta) + 2);
                }
                else // max == blue
                {
                    h = 60 * (((red - green) / delta) + 4);
                }
            }

            if (h < 0) h += 360;

            double s = max == 0 ? 0 : delta / max;
            double v = max;

            return (H: h, S: s, V: v);
        }

    }
}