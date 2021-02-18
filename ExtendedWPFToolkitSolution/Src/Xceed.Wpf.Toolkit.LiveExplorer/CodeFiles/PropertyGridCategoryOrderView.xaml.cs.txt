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
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Diagnostics;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridCategoryOrderView.xaml
  /// </summary>
  public partial class PropertyGridCategoryOrderView : DemoView
  {
    public PropertyGridCategoryOrderView()
    {
      InitializeComponent();
      _propertyGrid1.SelectedObject = PersonBase.InitPerson( new PersonNonOrdered() );
      _propertyGrid2.SelectedObject = PersonBase.InitPerson( new PersonOrdered() );
    }

    [CategoryOrder( "Information", 1 )]
    [CategoryOrder( "Hobbies", 2 )]
    [CategoryOrder( "Connections", 3 )]
    public class PersonOrdered : PersonBase { }

    public class PersonNonOrdered : PersonBase { }

    public abstract class PersonBase
    {
      [Category( "Information" )]
      public string FirstName { get; set; }

      [Category( "Information" )]
      public string LastName { get; set; }

      [Category( "Hobbies" )]
      public bool Baseball { get; set; }

      [Category( "Hobbies" )]
      public bool Football { get; set; }

      [Category( "Hobbies" )]
      public bool Basketball { get; set; }

      [Category( "Connections" )]
      public string Father { get; set; }

      [Category( "Connections" )]
      public string Mother { get; set; }

      [Category( "Connections" )]
      public bool HasChildren { get; set; }

      public static PersonBase InitPerson( PersonBase person )
      {
        person.FirstName = "John";
        person.LastName = "Doe";
        person.Baseball = true;
        person.Football = false;
        person.Basketball = true;
        person.Father = "William Doe";
        person.Mother = "Jennifer Doe";
        person.HasChildren = false;
        return person;
      }
    }
  }
}
