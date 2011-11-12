using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Controls.PropertyGrid.Attributes;
using Samples.Infrastructure.Controls;

namespace Samples.Modules.PropertyGrid.Views
{
    /// <summary>
    /// Interaction logic for CustomItemsSource.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
    public partial class CustomItemsSource : DemoView
    {
        public CustomItemsSource()
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

            [Category("Information")]
            [DisplayName("Date of Birth")]
            [Description("This property uses the DateTimeUpDown as the default editor.")]
            public DateTime DateOfBirth { get; set; }

            [Category("Writing")]
            [DisplayName("Writing Hand")]
            [Description("This property uses a ComboBox as the default editor.  The ComboBox is auto populated with the enum values")]
            public HorizontalAlignment WritingHand { get; set; }

            [Category("Writing")]
            [DisplayName("Writing Font")]
            [Description("This property uses a ComboBox as the default editor.  The ComboBox is auto populated with the enum values")]
            public FontFamily WritingFont { get; set; }

            [Category("Writing")]
            [DisplayName("Writing Font Size")]
            [Description("This property uses the DoubleUpDown as the default editor.")]
            [ItemsSource(typeof(FontSizeItemsSource))]
            public double WritingFontSize { get; set; }

            public static Person CreatePerson()
            {
                var person = new Person();
                person.FirstName = "John";
                person.LastName = "Doe";
                person.DateOfBirth = new DateTime(1975, 1, 23);
                person.WritingHand = System.Windows.HorizontalAlignment.Right;
                person.WritingFont = new FontFamily("Arial");
                person.WritingFontSize = 12.0;
                return person;
            }
        }
    }

    public class FontSizeItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add(5.0, "Five");
            sizes.Add(5.5);
            sizes.Add(6.0, "Six");
            sizes.Add(6.5);
            sizes.Add(7.0, "Seven");
            sizes.Add(7.5);
            sizes.Add(8.0, "Eight");
            sizes.Add(8.5);
            sizes.Add(9.0, "Nine");
            sizes.Add(9.5);
            sizes.Add(10.0);
            sizes.Add(12.0, "Twelve");
            sizes.Add(14.0);
            sizes.Add(16.0);
            sizes.Add(18.0);
            sizes.Add(20.0);
            return sizes;
        }
    }
}
