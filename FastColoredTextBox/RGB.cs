using System;
using System.Drawing;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Represents an RGB color.
    /// </summary>
    public struct RGB
    {
        /// <summary>
        /// Gets the alpha component.
        /// </summary>
        public int A { get; }

        /// <summary>
        /// Gets the red component.
        /// </summary>
        public int R { get; }

        /// <summary>
        /// Gets the green component.
        /// </summary>
        public int G { get; }

        /// <summary>
        /// Gets the blue component.
        /// </summary>
        public int B { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RGB"/> struct with the specified red, green, and blue values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public RGB(int a, int r, int g, int b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RGB"/> struct with the specified red, green, and blue values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public RGB(int r, int g, int b)
        {
            A = 255;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Converts the RGB value to a <see cref="Color"/>.
        /// </summary>
        /// <returns>A <see cref="Color"/> corresponding to the RGB values.</returns>
        public Color ToColor()
        {
            return Color.FromArgb(A, R, G, B);
        }

        /// <summary>
        /// Converts the RGB value to a <see cref="Color"/>, applying a shift value to each component.
        /// The result is clamped to the 0–255 range.
        /// </summary>
        /// <param name="colorShift">The shift value to apply to each RGB component.</param>
        /// <param name="alphaShift">The shift value to apply to alpha component.</param>
        /// <returns>A <see cref="Color"/> corresponding to the shifted RGB values.</returns>
        public Color ToColor(int colorShift, int alphaShift)
        {
            int shiftedA = Clamp(A + alphaShift, 0, 255);
            int shiftedR = Clamp(R + colorShift, 0, 255);
            int shiftedG = Clamp(G + colorShift, 0, 255);
            int shiftedB = Clamp(B + colorShift, 0, 255);
            return Color.FromArgb(shiftedA, shiftedR, shiftedG, shiftedB);
        }

        /// <summary>
        /// Converts the RGB value to a <see cref="Color"/>, applying a shift value to each component.
        /// The result is clamped to the 0–255 range.
        /// </summary>
        /// <param name="colorShift">The shift value to apply to each component.</param>
        /// <returns>A <see cref="Color"/> corresponding to the shifted RGB values.</returns>
        public Color ToColor(int colorShift)
        {
            int shiftedR = Clamp(R + colorShift, 0, 255);
            int shiftedG = Clamp(G + colorShift, 0, 255);
            int shiftedB = Clamp(B + colorShift, 0, 255);
            return Color.FromArgb(shiftedR, shiftedG, shiftedB);
        }

        /// <summary>
        /// Converts the RGB value to a <see cref="Color"/>, applying a shift value to each component.
        /// The result is clamped to the 0–255 range.
        /// </summary>
        /// <param name="colorShift">The shift value to apply to each component.</param>
        /// <returns>A <see cref="Color"/> corresponding to the shifted RGB values.</returns>
        public Color ToColor(int redShift, int greenShift, int blueShift)
        {
            int shiftedR = Clamp(R + redShift, 0, 255);
            int shiftedG = Clamp(G + greenShift, 0, 255);
            int shiftedB = Clamp(B + blueShift, 0, 255);
            return Color.FromArgb(shiftedR, shiftedG, shiftedB);
        }

        /// <summary>
        /// Creates a <see cref="Color"/> from RGB values provided in a tuple.
        /// </summary>
        /// <param name="shift">
        /// A tuple containing the RGB component values:
        /// <list type="bullet">
        /// <item><description>Item1: Red component (0-255)</description></item>
        /// <item><description>Item2: Green component (0-255)</description></item>
        /// <item><description>Item3: Blue component (0-255)</description></item>
        /// </list>
        /// </param>
        /// <returns>A <see cref="Color"/> with the specified RGB values and full opacity (Alpha = 255).</returns>
        /// <remarks>
        /// Values outside the 0-255 range will cause an <see cref="ArgumentException"/>.
        /// This method assumes full opacity; use <see cref="Color.FromArgb(int, int, int, int)"/> if you need to specify alpha.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if any RGB component is outside the valid range (0-255).</exception>
        public Color ToColor(Tuple<int, int, int> shift)
        {
            return ToColor(shift.Item1, shift.Item2, shift.Item3);
        }

        /// <summary>
        /// Converts the RGB value to its grayscale representation.
        /// </summary>
        /// <returns>A <see cref="Color"/> representing the grayscale value of the RGB color.</returns>
        public Color ToGrayscale()
        {
            // Use the luminance formula.
            int gray = (int)(R * 0.299 + G * 0.587 + B * 0.114);
            return Color.FromArgb(gray, gray, gray);
        }

        /// <summary>
        /// Returns a string that represents the current RGB value.
        /// </summary>
        /// <returns>A string in the format "RGB(r, g, b)".</returns>
        public override string ToString()
        {
            return $"ARGB({A}, {R}, {G}, {B})";
        }

        /// <summary>
        /// Clamps a value between a specified minimum and maximum value.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum allowable value.</param>
        /// <param name="max">The maximum allowable value.</param>
        /// <returns>The clamped value.</returns>
        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }
    }
}
