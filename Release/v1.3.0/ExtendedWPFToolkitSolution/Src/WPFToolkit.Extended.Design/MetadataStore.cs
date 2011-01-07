using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Controls;
using Microsoft.Windows.Design.Features;
using System.ComponentModel;

[assembly: ProvideMetadata(typeof(WPFToolkit.Extended.Design.MetadataStore))]

namespace WPFToolkit.Extended.Design
{
    internal class MetadataStore : IProvideAttributeTable
    {
        public AttributeTable AttributeTable
        {
            get
            {
                AttributeTableBuilder builder = new AttributeTableBuilder();

               
                //builder.AddCustomAttributes(typeof(NumericUpDown), new FeatureAttribute(typeof(NumericUpDownEditor)));
                builder.AddCustomAttributes(typeof(NumericUpDown), "Increment",
                    new DescriptionAttribute("Specifies the amount in which to increment the value."),
                    new DisplayNameAttribute("Increment"));

                builder.AddCustomAttributes(typeof(NumericUpDown), "IsEditable",
                    new DescriptionAttribute("Determines if direct entry is allowed in the text box."),
                    new DisplayNameAttribute("IsEditable"));

                builder.AddCustomAttributes(typeof(NumericUpDown), "Maximum",
                    new DescriptionAttribute("Gets/Sets the maximum allowed value."),
                    new DisplayNameAttribute("Maximum"));

                builder.AddCustomAttributes(typeof(NumericUpDown), "Minimum",
                    new DescriptionAttribute("Gets/Sets the minimum allowed value."),
                    new DisplayNameAttribute("Minimum"));

                builder.AddCustomAttributes(typeof(NumericUpDown), "Text",
                    new DescriptionAttribute("Gets/Sets the formated string representation of the value."),
                    new DisplayNameAttribute("Text"));

                builder.AddCustomAttributes(typeof(NumericUpDown), "Value",
                    new DescriptionAttribute("Gets/Sets the numeric value."),
                    new DisplayNameAttribute("Value"));

                return builder.CreateTable();
            }
        }
    }
}
