/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;

namespace Samples.Modules.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for CustomEditors.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class CustomEditors : DemoView
  {
    public CustomEditors()
    {
      InitializeComponent();
      _propertyGrid1.SelectedObject = Person.CreatePerson();
      _propertyGrid2.SelectedObject = CustomAttributEditorPerson.CreateCustomAttributEditorPerson();
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
      public DateTime DateOfBirth
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
      public Color FavoriteColor
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
        person.DateOfBirth = new DateTime( 1975, 1, 23 );
        person.Age = DateTime.Today.Year - person.DateOfBirth.Year;
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

    public class CustomAttributEditorPerson
    {
      [Category( "Information" )]
      [DisplayName( "First Name" )]
      [Description( "This property uses a TextBox as the default editor." )]
      //This custom editor is a Class that implements the ITypeEditor interface
      [Editor( typeof( FirstNameEditor ), typeof( FirstNameEditor ) )]
      public string FirstName
      {
        get;
        set;
      }

      [Category( "Information" )]
      [DisplayName( "Last Name" )]
      [Description( "This property uses a TextBox as the default editor." )]
      //This custom editor is a UserControl that implements the ITypeEditor interface
      [Editor( typeof( LastNameUserControlEditor ), typeof( LastNameUserControlEditor ) )]
      public string LastName
      {
        get;
        set;
      }

      [Category( "Information" )]
      [DisplayName( "Date of Birth" )]
      [Description( "This property uses the DateTimeUpDown as the default editor." )]
      public DateTime DateOfBirth
      {
        get;
        set;
      }

      public static CustomAttributEditorPerson CreateCustomAttributEditorPerson()
      {
        var person = new CustomAttributEditorPerson();
        person.FirstName = "John";
        person.LastName = "Doe";
        person.DateOfBirth = new DateTime( 1975, 1, 23 );
        return person;
      }
    }
  }

  //Custom editors that are used as attributes MUST implement the ITypeEditor interface.
  public class FirstNameEditor : Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
  {
    public FrameworkElement ResolveEditor( Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem )
    {
      TextBox textBox = new TextBox();
      textBox.Background = new SolidColorBrush( Colors.Red );

      //create the binding from the bound property item to the editor
      var _binding = new Binding( "Value" ); //bind to the Value property of the PropertyItem
      _binding.Source = propertyItem;
      _binding.ValidatesOnExceptions = true;
      _binding.ValidatesOnDataErrors = true;
      _binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
      BindingOperations.SetBinding( textBox, TextBox.TextProperty, _binding );
      return textBox;
    }
  }
}
