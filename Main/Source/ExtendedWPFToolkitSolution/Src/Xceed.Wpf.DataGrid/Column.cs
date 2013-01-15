/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using Xceed.Wpf.DataGrid.ValidationRules;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using Xceed.Utils.Collections;
using System.Collections.Specialized;
using Xceed.Wpf.DataGrid.Converters;

namespace Xceed.Wpf.DataGrid
{
  [DebuggerDisplay( "FieldName = {FieldName}" )]
  public class Column : ColumnBase
  {
    public Column()
    {
    }

    [Obsolete( "The DisplayMemberBinding property is obsolete and has been replaced by the DisplayMemberBindingInfo property.", false )]
    public Column( string fieldName, object title, BindingBase displayMemberBinding )
      : base( fieldName, title )
    {
      // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618

      this.DisplayMemberBinding = displayMemberBinding;

#pragma warning restore 618
    }

    #region DisplayMemberBinding Property

    private BindingBase m_displayMemberBinding; // = null

    [Obsolete( "The DisplayMemberBinding property is obsolete and has been replaced by the DisplayMemberBindingInfo property.", false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Browsable( false )]
    public BindingBase DisplayMemberBinding
    {
      get
      {
        return m_displayMemberBinding;
      }

      set
      {
        if( value != m_displayMemberBinding )
        {
          m_displayMemberBinding = value;
          this.OnPropertyChanged( new PropertyChangedEventArgs( "DisplayMemberBinding" ) );
        }
      }
    }

    #endregion DisplayMemberBinding Property

    #region DisplayMemberBindingInfo Property

    private DataGridBindingInfo m_displayMemberBindingInfo;

    public DataGridBindingInfo DisplayMemberBindingInfo
    {
      get
      {
        return m_displayMemberBindingInfo;
      }

      set
      {
        if( value != m_displayMemberBindingInfo )
        {
          m_displayMemberBindingInfo = value;

          m_displayMemberBinding = m_displayMemberBindingInfo.GetBinding();

          this.OnPropertyChanged( new PropertyChangedEventArgs( "DisplayMemberBindingInfo" ) );
        }
      }
    }

    #endregion DisplayMemberBindingInfo Property

    #region AllowSort Property

    public override bool AllowSort
    {
      get
      {
        return m_allowSort;
      }
      set
      {
        if( m_allowSort == value )
          return;

        m_allowSort = value;
        this.OnPropertyChanged( new PropertyChangedEventArgs( "AllowSort" ) );
      }
    }

    private bool m_allowSort = true;

    #endregion AllowSort Property

    #region AllowGroup Property

    public override bool AllowGroup
    {
      get
      {
        return m_allowGroup;
      }
      set
      {
        if( m_allowGroup == value )
          return;

        m_allowGroup = value;
        this.OnPropertyChanged( new PropertyChangedEventArgs( "AllowGroup" ) );
      }
    }

    private bool m_allowGroup = true;

    #endregion AllowGroup Property

    #region IsAutoCreated Property

    public bool IsAutoCreated
    {
      get;
      internal set;
    }

    #endregion IsAutoCreated Property

    #region DistinctValues Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The DistinctValues property is obsolete and has been replaced by the DataGridCollectionView.DistinctValues and DataGridContext.DistinctValues properties.", true )]
    public ICollection DistinctValues
    {
      get
      {
        return null;
      }
    }

    #endregion DistinctValues Property

    #region ParentGrid Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The ParentDataGridControl property is obsolete.", true )]
    protected DataGridControl ParentDataGridControl
    {
      get
      {
        return null;
      }
    }

    #endregion ParentGrid Property

    #region ForeignKeyConfiguration Property

    internal event EventHandler ForeignKeyConfigurationChanged;

    public static readonly DependencyProperty ForeignKeyConfigurationProperty = DependencyProperty.Register(
      "ForeignKeyConfiguration",
      typeof( ForeignKeyConfiguration ),
      typeof( Column ),
      new FrameworkPropertyMetadata( null, new PropertyChangedCallback( Column.OnForeignKeyConfigurationChanged ) ) );

    public ForeignKeyConfiguration ForeignKeyConfiguration
    {
      get
      {
        return ( ForeignKeyConfiguration )this.GetValue( Column.ForeignKeyConfigurationProperty );
      }
      set
      {
        this.SetValue( Column.ForeignKeyConfigurationProperty, value );
      }
    }

    private static void OnForeignKeyConfigurationChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var column = ( Column )sender;

      column.RaiseForeignKeyConfigurationChanged();

      if( column.ParentDetailConfiguration != null )
      {
        column.ParentDetailConfiguration.SynchronizeForeignKeyConfigurations();
      }
      else if( column.DataGridControl != null )
      {
        column.DataGridControl.SynchronizeForeignKeyConfigurations();
      }
    }

    private void RaiseForeignKeyConfigurationChanged()
    {
      var handler = this.ForeignKeyConfigurationChanged;
      if( handler != null )
      {
        handler( this, EventArgs.Empty );
      }
    }

    #endregion

    #region IsBindingAutoCreated Property

    internal bool IsBindingAutoCreated
    {
      get
      {
        return m_bindingAutoCreated;
      }
      set
      {
        m_bindingAutoCreated = value;
      }
    }

    #endregion

    #region IsBoundToDataGridUnboundItemProperty Property

    internal bool IsBoundToDataGridUnboundItemProperty
    {
      get;
      set;
    }

    #endregion

#if DEBUG
    public override string ToString()
    {
      string toString = base.ToString();

      if( string.IsNullOrEmpty( this.FieldName ) == false )
        toString += " " + this.FieldName;

      return toString;
    }
#endif

    private bool m_bindingAutoCreated; // = false
  }
}
