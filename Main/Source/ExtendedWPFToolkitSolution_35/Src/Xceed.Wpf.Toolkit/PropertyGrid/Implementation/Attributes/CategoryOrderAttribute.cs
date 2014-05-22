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

namespace Xceed.Wpf.Toolkit.PropertyGrid.Attributes
{
  [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
  public class CategoryOrderAttribute : Attribute
  {
    #region Properties

    #region Order

    public int Order
    {
      get;
      set;
    }

    #endregion

    #region Category

    public virtual string Category
    {
      get
      {
        return CategoryValue;
      }
    }

    #endregion

    #region CategoryValue

    public string CategoryValue
    {
      get;
      private set;
    }

    #endregion

    #endregion

    #region constructor

    public CategoryOrderAttribute()
    {
    }

    public CategoryOrderAttribute( string categoryName, int order )
      :this()
    {
      CategoryValue = categoryName;
      Order = order;
    }

    #endregion
  }
}

