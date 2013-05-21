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

namespace Xceed.Wpf.DataGrid
{
  public class CellEditorContext : DependencyObject
  {
    #region Constructors

    static CellEditorContext()
    {
      CellEditorContext.ForeignKeyConfigurationProperty = CellEditorContext.ForeignKeyConfigurationPropertyKey.DependencyProperty;
      CellEditorContext.ParentColumnProperty = CellEditorContext.ParentColumnPropertyKey.DependencyProperty;
    }

    internal CellEditorContext( ColumnBase parentColumn, ForeignKeyConfiguration configuration )
    {
      this.SetParentColumn( parentColumn );
      this.SetForeignKeyConfiguration( configuration );
    } 

    #endregion

    #region ForeignKeyConfiguration Property

    private static readonly DependencyPropertyKey ForeignKeyConfigurationPropertyKey = DependencyProperty.RegisterReadOnly(
      "ForeignKeyConfiguration",
      typeof( ForeignKeyConfiguration ),
      typeof( CellEditorContext ),
      new PropertyMetadata( null ) );

    public static readonly DependencyProperty ForeignKeyConfigurationProperty;

    public ForeignKeyConfiguration ForeignKeyConfiguration
    {
      get
      {
        return ( ForeignKeyConfiguration )this.GetValue( CellEditorContext.ForeignKeyConfigurationProperty );
      }
    }

    private void SetForeignKeyConfiguration( ForeignKeyConfiguration value )
    {
      this.SetValue( CellEditorContext.ForeignKeyConfigurationPropertyKey, value );
    }

    private void ClearForeignKeyConfiguration()
    {
      this.ClearValue( CellEditorContext.ForeignKeyConfigurationPropertyKey );
    }

    #endregion

    #region ParentColumn Property

    private static readonly DependencyPropertyKey ParentColumnPropertyKey = DependencyProperty.RegisterReadOnly(
      "ParentColumn",
      typeof( ColumnBase ),
      typeof( CellEditorContext ),
      new PropertyMetadata( null ) );

    public static readonly DependencyProperty ParentColumnProperty;

    public ColumnBase ParentColumn
    {
      get
      {
        return ( ColumnBase )this.GetValue( CellEditorContext.ParentColumnProperty );
      }
    }

    private void SetParentColumn( ColumnBase value )
    {
      this.SetValue( CellEditorContext.ParentColumnPropertyKey, value );
    }

    private void ClearParentColumn()
    {
      this.ClearValue( CellEditorContext.ParentColumnPropertyKey );
    }

    #endregion
  }
}
