/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/

using System;
using System.ComponentModel;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridBindingToStructsView.xaml
  /// </summary>
  public partial class PropertyGridBindingToStructsView : DemoView
  {
    public PropertyGridBindingToStructsView()
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

      public Dimension Dimensions
      {
        get;
        set;
      }

      public static Person CreatePerson()
      {
        var person = new Person();
        person.FirstName = "John";
        person.LastName = "Doe";
        person.Dimensions = new Dimension()
        {
          Height = 75.0,
          Weight = 185.76
        };
        return person;
      }
    }
  }

  public struct Dimension
  {
    public double Height;
    public double Weight;

    public Dimension( double height, double weight )
    {
      this.Height = height;
      this.Weight = weight;
    }
  }
}
