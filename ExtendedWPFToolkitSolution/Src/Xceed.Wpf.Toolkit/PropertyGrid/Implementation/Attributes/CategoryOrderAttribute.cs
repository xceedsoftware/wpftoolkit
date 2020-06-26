/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

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

    public override object TypeId
    {
      get
      {
        return this.CategoryValue;
      }
    }

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

