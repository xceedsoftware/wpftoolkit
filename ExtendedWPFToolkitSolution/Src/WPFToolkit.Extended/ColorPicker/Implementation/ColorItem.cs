using System;
using System.Windows.Media;

namespace Microsoft.Windows.Controls
{
    public class ColorItem
    {
        public Color Color { get; set; }
        public string Name { get; set; }

        public ColorItem(Color color, string name)
        {
            Color = color;
            Name = name;
        }
    }
}
