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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal class ForeignKeyDistinctValueItemContentControl : ForeignKeyContentControl
  {
    #region Static Fields

    private static Binding ParentAutoFilterControlBinding;

    #endregion

    #region Constructors

    static ForeignKeyDistinctValueItemContentControl()
    {
      ForeignKeyDistinctValueItemContentControl.ParentAutoFilterControlBinding = new Binding();
      ForeignKeyDistinctValueItemContentControl.ParentAutoFilterControlBinding.Mode = BindingMode.OneWay;
    }

    public ForeignKeyDistinctValueItemContentControl()
    {
    }

    #endregion

    #region Private Methods

    #endregion

    #region DefaultCellContentTemplateBinding Private Static Property

    private static Binding DefaultCellContentTemplateBinding
    {
      get
      {
        if( ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateBindingCache == null )
        {
          ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateBindingCache = new Binding();
          ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateBindingCache.Mode = BindingMode.OneWay;
          ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateBindingCache.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

          ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateBindingCache.Path = 
            new PropertyPath( "(0).(1).(2)", 
              Cell.ParentCellProperty, 
              Cell.ParentColumnProperty, 
              ColumnBase.CellContentTemplateProperty );
        }

        return ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateBindingCache;
      }
    }

    private static Binding DefaultCellContentTemplateBindingCache; // = null;

    #endregion

    #region DefaultCellContentTemplateSelectorBinding Private Static Property

    private static Binding DefaultCellContentTemplateSelectorBinding
    {
      get
      {
        if( ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateSelectorBindingCache == null )
        {
          ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateSelectorBindingCache = new Binding();
          ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateSelectorBindingCache.Mode = BindingMode.OneWay;
          ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateSelectorBindingCache.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

          ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateSelectorBindingCache.Path =
            new PropertyPath( "(0).(1).(2)",
              Cell.ParentCellProperty,
              Cell.ParentColumnProperty,
              ColumnBase.CellContentTemplateSelectorProperty );
        }

        return ForeignKeyDistinctValueItemContentControl.DefaultCellContentTemplateSelectorBindingCache;
      }
    }

    private static Binding DefaultCellContentTemplateSelectorBindingCache;

    #endregion

    #region DistinctValueItemTemplateBinding Private Static Property

    private static Binding DistinctValueItemTemplateBinding
    {
      get
      {
        if( ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateBindingCache == null )
        {
          ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateBindingCache = new Binding();
          ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateBindingCache.Mode = BindingMode.OneWay;


          ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateBindingCache.Path =
            new PropertyPath( "DistinctValueItemTemplate" );
        }

        return ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateBindingCache;
      }
    }

    private static Binding DistinctValueItemTemplateBindingCache; // = null;

    #endregion

    #region DefaultCellContentTemplateSelectorBinding Private Static Property

    private static Binding DistinctValueItemTemplateSelectorBinding
    {
      get
      {
        if( ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateSelectorBindingCache == null )
        {
          ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateSelectorBindingCache = new Binding();
          ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateSelectorBindingCache.Mode = BindingMode.OneWay;


          ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateSelectorBindingCache.Path =
            new PropertyPath( "DistinctValueItemTemplateSelector" );
        }

        return ForeignKeyDistinctValueItemContentControl.DistinctValueItemTemplateSelectorBindingCache;
      }
    }

    private static Binding DistinctValueItemTemplateSelectorBindingCache;

    #endregion
  }
}
