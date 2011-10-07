using Samples.Infrastructure.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System;
using System.Windows.Media;
using System.Windows;
using Microsoft.Windows.Controls.PropertyGrid.Attributes;

namespace Samples.Modules.PropertyGrid.Views
{
    /// <summary>
    /// Interaction logic for CustomItemsSource.xaml
    /// </summary>
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
        public IList<object> GetValues()
        {
            List<object> sizes = new List<object>()
            {
                5.0,5.5,6.0,6.5,7.0,7.5,8.0,8.5,9.0,9.5,10.0,12.0,14.0,16.0,18.0,20.0
            };
            return sizes;
        }
    }
}
