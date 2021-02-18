/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridSpecifyingPropertiesView.xaml
  /// </summary>
  public partial class PropertyGridSpecifyingPropertiesView : DemoView
  {
    public PropertyGridSpecifyingPropertiesView()
    {
      InitializeComponent();
      _propertyGrid.SelectedObject = Person.CreatePerson();
    }

    public class Person
    {
      [Category( "Information" )]
      [DisplayName( "First Name" )]
      [Description( "This property uses a TextBox as the default editor." )]
      public string FirstName
      {
        get;
        set;
      }

      [Category( "Information" )]
      [DisplayName( "Last Name" )]
      [Description( "This property uses a TextBox as the default editor." )]
      public string LastName
      {
        get;
        set;
      }

      [Category( "Information" )]
      [DisplayName( "Date of Birth" )]
      [Description( "This property uses the DateTimeUpDown as the default editor." )]
      public System.DateTime DateOfBirth
      {
        get;
        set;
      }

      [DisplayName( "Grade Point Average" )]
      [Description( "This property uses the DoubleUpDown as the default editor." )]
      public double GradePointAvg
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Description( "This property uses the IntegerUpDown as the default editor." )]
      public int Age
      {
        get;
        set;
      }

      [Category( "Information" )]
      [DisplayName( "Is Male" )]
      [Description( "This property uses a CheckBox as the default editor." )]
      public bool IsMale
      {
        get;
        set;
      }

      [Category( "Information" )]
      [DisplayName( "Favorite Color" )]
      [Description( "This property uses the ColorPicker as the default editor." )]
      public System.Windows.Media.Color? FavoriteColor
      {
        get;
        set;
      }

      [Category( "Writing" )]
      [DisplayName( "Writing Hand" )]
      [Description( "This property uses a ComboBox as the default editor.  The ComboBox is auto populated with the enum values" )]
      public HorizontalAlignment WritingHand
      {
        get;
        set;
      }

      [Category( "Writing" )]
      [DisplayName( "Writing Font" )]
      [Description( "This property uses a ComboBox as the default editor.  The ComboBox is auto populated with the enum values" )]
      public FontFamily WritingFont
      {
        get;
        set;
      }

      [Category( "Writing" )]
      [DisplayName( "Writing Font Size" )]
      [Description( "This property uses the DoubleUpDown as the default editor." )]
      public double WritingFontSize
      {
        get;
        set;
      }

      [Category( "Conections" )]
      [DisplayName( "Pet Names" )]
      [Description( "This property uses the PrimitiveTypeCollectionEditor as the default editor." )]
      public List<String> PetNames
      {
        get;
        set;
      }

      [Category( "Conections" )]
      [Description( "This property uses the CollectionEditor as the default editor." )]
      public List<Person> Friends
      {
        get;
        set;
      }

      [Category( "Conections" )]
      [Description( "This property is a complex property and has no default editor." )]
      public Person Spouse
      {
        get;
        set;
      }

      public static Person CreatePerson()
      {
        var person = new Person();
        person.FirstName = "John";
        person.LastName = "Doe";
        person.DateOfBirth = new System.DateTime( 1975, 1, 23 );
        person.Age = System.DateTime.Today.Year - person.DateOfBirth.Year;
        person.GradePointAvg = 3.98;
        person.IsMale = true;
        person.FavoriteColor = Colors.Blue;
        person.WritingHand = System.Windows.HorizontalAlignment.Right;
        person.WritingFont = new FontFamily( "Arial" );
        person.WritingFontSize = 12.5;
        person.PetNames = new List<string>() { "Pet 1", "Pet 2", "Pet 3" };
        person.Friends = new List<Person>() { new Person() { FirstName = "First", LastName = "Friend" }, new Person() { FirstName = "Second", LastName = "Friend" } };
        person.Spouse = new Person()
        {
          FirstName = "Jane",
          LastName = "Doe"
        };
        return person;
      }
    }
  }
}
