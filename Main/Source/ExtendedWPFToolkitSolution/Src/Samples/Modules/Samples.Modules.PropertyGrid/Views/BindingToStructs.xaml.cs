using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;
using System.ComponentModel;

namespace Samples.Modules.PropertyGrid.Views
{
    /// <summary>
    /// Interaction logic for BindingToStructs.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
    public partial class BindingToStructs : DemoView
    {
        public BindingToStructs()
        {
            InitializeComponent();
            _propertyGrid.SelectedObject = Person.CreatePerson();
        }

        public class Person
        {
            [Category("Information")]
            [DisplayName("First Name")]
            [Description("This property uses a TextBox as the default editor.")]
            public string FirstName { get; set; }

            [Category("Information")]
            [DisplayName("Last Name")]
            [Description("This property uses a TextBox as the default editor.")]
            public string LastName { get; set; }

            public Dimension Dimensions { get; set; }

            public static Person CreatePerson()
            {
                var person = new Person();
                person.FirstName = "John";
                person.LastName = "Doe";
                person.Dimensions = new Dimension() { Height = 75.0, Weight = 185.76 };
                return person;
            }
        }
    }

    public struct Dimension
    {
        public double Height;
        public double Weight;

        public Dimension(double height, double weight)
        {
            this.Height = height;
            this.Weight = weight;
        }
    }

    public class DimensionsConverter : IValueConverter
    {
        static Dimension _originalValue; // the static struct that stores original value at the start of editing

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            _originalValue = ((Dimension)value);

            if (parameter.ToString() == "Height")
                return ((Dimension)value).Height;
            if (parameter.ToString() == "Weight")
                return ((Dimension)value).Weight;

            return _originalValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter.ToString() == "Height")
                _originalValue = new Dimension(double.Parse(value.ToString()), _originalValue.Weight);
            if (parameter.ToString() == "Weight")
                _originalValue = new Dimension(_originalValue.Height, double.Parse(value.ToString()));

            return _originalValue;

        }
    }
}
