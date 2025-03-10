/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

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
  public enum CategoryPropertyOrderEnum
  {
    Alphabetical,
    Declaration
  }

  [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
  public class CategoryPropertyOrderAttribute : Attribute
  {
    #region Properties

    #region CategoryPropertyOrder

    public CategoryPropertyOrderEnum CategoryPropertyOrder
    {
      get;
      private set;
    }

    #endregion

    #endregion

    #region constructor

    public CategoryPropertyOrderAttribute( CategoryPropertyOrderEnum categoryPropertyOrder = CategoryPropertyOrderEnum.Alphabetical )
    {
      CategoryPropertyOrder = categoryPropertyOrder;
    }

    #endregion
  }
}

