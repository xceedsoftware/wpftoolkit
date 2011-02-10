using System;
using System.Linq;
using System.Windows.Media;
using System.Reflection;
using System.Collections.Generic;

namespace Microsoft.Windows.Controls
{
    static class ColorUtilities
    {
        public static string GetColorName(this Color color)
        {
            return _knownColors.Where(kvp => kvp.Value.Equals(color)).Select(kvp => kvp.Key).FirstOrDefault();
        }
        static readonly Dictionary<string, Color> _knownColors = GetKnownColors();
        static Dictionary<string, Color> GetKnownColors()
        {
            var colorProperties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            return colorProperties.ToDictionary(p => p.Name, p => (Color)p.GetValue(null, null));
        }
    }
}
