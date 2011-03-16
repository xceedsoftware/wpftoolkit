using System;
using System.Linq;
using System.Windows.Media;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Windows.Controls.Primitives;

namespace Microsoft.Windows.Controls.Core.Utilities
{
    static class ColorUtilities
    {
        public static readonly Dictionary<string, Color> KnownColors = GetKnownColors();

        public static string GetColorName(this Color color)
        {
            string colorName = KnownColors.Where(kvp => kvp.Value.Equals(color)).Select(kvp => kvp.Key).FirstOrDefault();

            if (String.IsNullOrEmpty(colorName))
                colorName = color.ToString();

            return colorName;
        }

        private static Dictionary<string, Color> GetKnownColors()
        {
            var colorProperties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            return colorProperties.ToDictionary(p => p.Name, p => (Color)p.GetValue(null, null));
        }

        /// <summary>
        /// Converts an RGB color to an HSV color.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="b"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static HsvColor ConvertRgbToHsv(int r, int b, int g)
        {
            double delta, min;
            double h = 0, s, v;

            min = Math.Min(Math.Min(r, g), b);
            v = Math.Max(Math.Max(r, g), b);
            delta = v - min;

            if (v == 0.0)
            {
                s = 0;
            }
            else
                s = delta / v;

            if (s == 0)
                h = 0.0;

            else
            {
                if (r == v)
                    h = (g - b) / delta;
                else if (g == v)
                    h = 2 + (b - r) / delta;
                else if (b == v)
                    h = 4 + (r - g) / delta;

                h *= 60;
                if (h < 0.0)
                    h = h + 360;

            }

            return new HsvColor { H = h, S = s, V = v / 255 };
        }

        /// <summary>
        ///  Converts an HSV color to an RGB color.
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Color ConvertHsvToRgb(double h, double s, double v)
        {
            double r = 0, g = 0, b = 0;

            if (s == 0)
            {
                r = v;
                g = v;
                b = v;
            }
            else
            {
                int i;
                double f, p, q, t;

                if (h == 360)
                    h = 0;
                else
                    h = h / 60;

                i = (int)Math.Truncate(h);
                f = h - i;

                p = v * (1.0 - s);
                q = v * (1.0 - (s * f));
                t = v * (1.0 - (s * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        {
                            r = v;
                            g = t;
                            b = p;
                            break;
                        }
                    case 1:
                        {
                            r = q;
                            g = v;
                            b = p;
                            break;
                        }
                    case 2:
                        {
                            r = p;
                            g = v;
                            b = t;
                            break;
                        }
                    case 3:
                        {
                            r = p;
                            g = q;
                            b = v;
                            break;
                        }
                    case 4:
                        {
                            r = t;
                            g = p;
                            b = v;
                            break;
                        }
                    default:
                        {
                            r = v;
                            g = p;
                            b = q;
                            break;
                        }
                }

            }

            return Color.FromArgb(255, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        /// <summary>
        /// Generates a list of colors with hues ranging from 0 360 and a saturation and value of 1. 
        /// </summary>
        /// <returns></returns>
        public static List<Color> GenerateHsvSpectrum()
        {
            List<Color> colorsList = new List<Color>(8);

            for (int i = 0; i < 29; i++)
            {
                colorsList.Add(ColorUtilities.ConvertHsvToRgb(i * 12, 1, 1));
            }

            colorsList.Add(ColorUtilities.ConvertHsvToRgb(0, 1, 1));

            return colorsList;
        }
    }
}
