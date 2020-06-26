/**************************************************************************************

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridCustomEditorsView.xaml
  /// </summary>
  public partial class PropertyGridCustomEditorsView : DemoView
  {
    public PropertyGridCustomEditorsView()
    {
      InitializeComponent();

      var person = new Person();
      person.FirstName = "John";
      person.LastName = "Doe";
      person.DateOfBirth = new System.DateTime( 1975, 1, 23 );
      person.Age = System.DateTime.Today.Year - person.DateOfBirth.Year;
      person.GradePointAvg = 3.98;
      person.IsMale = true;
      person.FavoriteColor = Colors.Blue;
      person.WritingFont = new FontFamily( "Arial" );
      person.WritingHand = System.Windows.HorizontalAlignment.Right;
      person.WritingFontSize = 12.5;
      var friends = new ObservableCollection<Friend>() { new Friend() { FirstName = "Tom", LastName = "Sawyer", Age = 11 }, new Friend() { FirstName = "Mike", LastName = "Jones", Age = 9 } };
      person.Friends = friends;
      person.BestFriend = friends[ 1 ];

      this.DataContext = person;
    }

    public class Person
    {
      [Category( "Information" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #2 because it targets the property named 'FirstName'." )]
      public string FirstName
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #2 because it targets the property named 'LastName'." )]
      public string LastName
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #3 because it targets all properties of Type 'DateTime'." )]
      public System.DateTime DateOfBirth
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #1 because it targets all properties of Type 'Double'." )]
      public double GradePointAvg
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #3 because it targets the property named 'Age'." )]
      public int Age
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #4 because it targets all properties of Type 'Boolean'." )]
      public bool IsMale
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Description( "This property uses the default editor because no EditorDefinition targets this property name or type." )]
      public System.Windows.Media.Color? FavoriteColor
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #5 because it targets the property 'Friends'." )]
      public ObservableCollection<Friend> Friends
      {
        get;
        set;
      }

      [Category( "Information" )]
      [Browsable(false)]
      [Description( "This property is not displayed in the PropertyGrid, but its used as the selectedItem of the ComboBox for the 'Friends' property Editor." )]
      public Friend BestFriend
      {
        get;
        set;
      }

      [Category( "Writing" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #4 because it targets all properties of Type 'HorizontalAlignment'." )]
      public HorizontalAlignment WritingHand
      {
        get;
        set;
      }

      [Category( "Writing" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #2 because it targets the property named 'WritingFont'. Although EditorTemplateDefinition #4 targets 'FontFamily' type, priority is given to the name." )]
      public FontFamily WritingFont
      {
        get;
        set;
      }

      [Category( "Writing" )]
      [Description( "This property uses the editor defined by EditorTemplateDefinition #1 because it targets all properties of Type 'Double'." )]
      public double WritingFontSize
      {
        get;
        set;
      }     
    }

    public class Friend
    {
      public string FirstName
      {
        get;
        set;
      }

      public string LastName
      {
        get;
        set;
      }

      public int Age
      {
        get;
        set;
      }
    }
  }
}
