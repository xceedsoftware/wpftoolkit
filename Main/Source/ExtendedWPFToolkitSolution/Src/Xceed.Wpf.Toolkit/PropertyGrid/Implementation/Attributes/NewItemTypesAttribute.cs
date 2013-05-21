/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Attributes
{
  /// <summary>
  /// This attribute can decorate the collection properties (i.e., IList) 
  /// of your selected object in order to control the types that will be allowed
  /// to be instantiated in the CollectionControl.
  /// </summary>
  [AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
  public class NewItemTypesAttribute : Attribute
  {
    public IList<Type> Types
    {
      get;
      set;
    }

    public NewItemTypesAttribute( params Type[] types )
    {
      this.Types = new List<Type>( types );
    }

    public NewItemTypesAttribute()
    {
    }
  }
}
