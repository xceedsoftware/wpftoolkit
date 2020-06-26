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
  public enum UsageContextEnum
  {
    Alphabetical,
    Categorized,
    Both
  }

  [AttributeUsage( AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
  public class PropertyOrderAttribute : Attribute
  {
    #region Properties

    public int Order
    {
      get;
      set;
    }

    public UsageContextEnum UsageContext
    {
      get;
      set;
    }

    public override object TypeId
    {
      get
      {
        return this;
      }
    }

    #endregion

    #region Initialization

    public PropertyOrderAttribute( int order )
      : this( order, UsageContextEnum.Both )
    {
    }

    public PropertyOrderAttribute( int order, UsageContextEnum usageContext )
    {
      Order = order;
      UsageContext = usageContext;
    }

    #endregion
  }
}
