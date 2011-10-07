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
    /// Interaction logic for ExpandableProperties.xaml
    /// </summary>
    public partial class ExpandableProperties : DemoView
    {
        public ExpandableProperties()
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

            [Category("Writing")]
            [DisplayName("Writing Font")]
            [Description("This property uses a ComboBox as the default editor.  The ComboBox is auto populated with the enum values")]
            public FontFamily WritingFont { get; set; }

            [Category("Writing")]
            [DisplayName("Writing Font Size")]
            [Description("This property uses the DoubleUpDown as the default editor.")]
            public double WritingFontSize { get; set; }


            [Category("Conections")]
            [Description("This property is a complex property and has no default editor.")]
            [ExpandableObject]
            public Person Spouse { get; set; }

            public static Person CreatePerson()
            {
                var person = new Person();
                person.FirstName = "John";
                person.LastName = "Doe";
                person.WritingFont = new FontFamily("Arial");
                person.WritingFontSize = 12.5;
                person.Spouse = new Person() { FirstName = "Jane", LastName = "Doe" };
                return person;
            }
        }
    }
}
