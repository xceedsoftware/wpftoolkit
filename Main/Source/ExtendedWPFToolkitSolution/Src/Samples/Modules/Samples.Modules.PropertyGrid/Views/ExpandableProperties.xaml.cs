/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System.ComponentModel;
using System.Windows.Media;
using Samples.Infrastructure.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

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
      [Description( "This property is a complex property and has no default editor." )]
      [ExpandableObject]
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
        person.WritingFont = new FontFamily( "Arial" );
        person.WritingFontSize = 12.5;
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
