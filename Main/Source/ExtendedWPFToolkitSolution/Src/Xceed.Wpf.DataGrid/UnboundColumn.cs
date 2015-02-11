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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.DataGrid.ValidationRules;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid
{
  public class UnboundColumn : ColumnBase
  {
    public UnboundColumn()
    {
    }

    public UnboundColumn( string fieldName, object title )
      : base( fieldName, title )
    {
    }

    #region AllowSort Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public override bool AllowSort
    {
      get
      {
        return base.AllowSort;
      }
      set
      {
        base.AllowSort = value;
      }
    }

    #endregion AllowSort Property

    #region AllowGroup Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public override bool AllowGroup
    {
      get
      {
        return base.AllowGroup;
      }
      set
      {
        base.AllowGroup = value;
      }
    }

    #endregion AllowGroup Property

    #region GroupValueStringFormat Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new string GroupValueStringFormat
    {
      get
      {
        return base.GroupValueStringFormat;
      }
      set
      {
        base.GroupValueStringFormat = value;
      }
    }

    #endregion GroupValueStringFormat Property

    #region GroupValueTemplate Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new DataTemplate GroupValueTemplate
    {
      get
      {
        return base.GroupValueTemplate;
      }
      set
      {
        base.GroupValueTemplate = value;
      }
    }

    #endregion GroupValueTemplate Property

    #region GroupValueTemplateSelector Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new DataTemplateSelector GroupValueTemplateSelector
    {
      get
      {
        return base.GroupValueTemplateSelector;
      }
      set
      {
        base.GroupValueTemplateSelector = value;
      }
    }

    #endregion GroupValueTemplateSelector Property

    #region CellEditor Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new CellEditor CellEditor
    {
      get
      {
        return base.CellEditor;
      }
      set
      {
        base.CellEditor = value;
      }
    }

    #endregion CellEditor Property

    #region CellEditorDisplayConditions Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new CellEditorDisplayConditions CellEditorDisplayConditions
    {
      get
      {
        return base.CellEditorDisplayConditions;
      }
      set
      {
        base.CellEditorDisplayConditions = value;
      }
    }

    #endregion CellEditorDisplayConditions Property

    #region CellValidationRules Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new Collection<CellValidationRule> CellValidationRules
    {
      get
      {
        return base.CellValidationRules;
      }
    }

    #endregion CellValidationRules Property

    #region CellErrorStyle Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new Style CellErrorStyle
    {
      get
      {
        return base.CellErrorStyle;
      }
      set
      {
        base.CellErrorStyle = value;
      }
    }

    #endregion CellErrorStyle Property

    #region HasValidationError Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new bool HasValidationError
    {
      get
      {
        return base.HasValidationError;
      }
    }

    #endregion HasValidationError Property

    #region SortDirection Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new SortDirection SortDirection
    {
      get
      {
        return base.SortDirection;
      }
    }

    #endregion SortDirection Property

    #region SortIndex Read-Only Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new int SortIndex
    {
      get
      {
        return base.SortIndex;
      }
    }

    #endregion SortIndex Read-Only Property

    #region GroupDescription Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new GroupDescription GroupDescription
    {
      get
      {
        return base.GroupDescription;
      }
      set
      {
        base.GroupDescription = value;
      }
    }

    #endregion GroupDescription Property

    #region GroupConfiguration Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public new GroupConfiguration GroupConfiguration
    {
      get
      {
        return base.GroupConfiguration;
      }
      set
      {
        base.GroupConfiguration = value;
      }
    }

    #endregion GroupConfiguration Property

    #region ReadOnly Property

    public static new readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
      "ReadOnly", typeof( bool ), typeof( UnboundColumn ), new FrameworkPropertyMetadata( true ) );

    public override bool ReadOnly
    {
      get
      {
        return ( bool )this.GetValue( UnboundColumn.ReadOnlyProperty );
      }
      set
      {
        if( value == this.ReadOnly )
          return;

        throw new InvalidOperationException( "An attempt was made to set the ReadOnly property of a UnboundColumn, which cannot be edited, to false." );
      }
    }

    #endregion
  }
}
