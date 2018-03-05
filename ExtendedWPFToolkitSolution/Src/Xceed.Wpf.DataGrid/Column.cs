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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  [DebuggerDisplay( "FieldName = {FieldName}" )]
  public class Column : ColumnBase
  {
    #region Static Fields

    internal static readonly string AllowSortPropertyName = PropertyHelper.GetPropertyName( ( Column c ) => c.AllowSort );
    internal static readonly string AllowGroupPropertyName = PropertyHelper.GetPropertyName( ( Column c ) => c.AllowGroup );
#pragma warning disable 618
    internal static readonly string DisplayMemberBindingPropertyName = PropertyHelper.GetPropertyName( ( Column c ) => c.DisplayMemberBinding );
#pragma warning restore 618
    internal static readonly string DisplayMemberBindingInfoPropertyName = PropertyHelper.GetPropertyName( ( Column c ) => c.DisplayMemberBindingInfo );

    #endregion

    public Column()
    {
    }

    [Obsolete( "The DisplayMemberBinding property is obsolete and has been replaced by the DisplayMemberBindingInfo property.", false )]
    public Column( string fieldName, object title, BindingBase displayMemberBinding )
      : base( fieldName, title )
    {
      this.SetDisplayMemberBinding( displayMemberBinding );
    }

    internal static Column Create( string fieldName, object title, BindingBase displayMemberBinding )
    {
      // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618
      var column = new Column( fieldName, title, displayMemberBinding );
#pragma warning restore 618

      return column;
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
        if( value == m_displayMemberBinding )
          return;

        m_displayMemberBinding = value;

        this.OnPropertyChanged( new PropertyChangedEventArgs( Column.DisplayMemberBindingPropertyName ) );
      }
    }

    internal BindingBase GetDisplayMemberBinding()
    {
      // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618
      var value = this.DisplayMemberBinding;
#pragma warning restore 618

      return value;
    }

    internal void SetDisplayMemberBinding( BindingBase value )
    {
      // Disable warning for DisplayMemberBinding when internaly used
#pragma warning disable 618
      this.DisplayMemberBinding = value;
#pragma warning restore 618
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

          this.OnPropertyChanged( new PropertyChangedEventArgs( Column.DisplayMemberBindingInfoPropertyName ) );
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
        this.OnPropertyChanged( new PropertyChangedEventArgs( Column.AllowSortPropertyName ) );
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
        this.OnPropertyChanged( new PropertyChangedEventArgs( Column.AllowGroupPropertyName ) );
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
      new FrameworkPropertyMetadata(
        null,
        new PropertyChangedCallback( Column.OnForeignKeyConfigurationChanged ) ) );

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
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    #endregion

    #region IsBindingAutoCreated Property

    internal bool IsBindingAutoCreated
    {
      get
      {
        return m_isBindingAutoCreated;
      }
      set
      {
        m_isBindingAutoCreated = value;
      }
    }

    private bool m_isBindingAutoCreated; //false

    #endregion

    #region IsBoundToDataGridUnboundItemProperty Property

    internal bool IsBoundToDataGridUnboundItemProperty
    {
      get
      {
        return m_isBoundToDataGridUnboundItemProperty;
      }
      set
      {
        if( value == m_isBoundToDataGridUnboundItemProperty )
          return;

        m_isBoundToDataGridUnboundItemProperty = value;
        this.ResetDefaultCellRecyclingGroup();
      }
    }

    private bool m_isBoundToDataGridUnboundItemProperty; //false

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

    internal override object CreateDefaultCellRecyclingGroup()
    {
      var key = base.CreateDefaultCellRecyclingGroup();

      if( !this.IsBoundToDataGridUnboundItemProperty )
        return key;

      return new UnboundColumnCellRecyclingGroupKey( key );
    }

    #region UnboundColumnCellRecyclingGroupKey Private Nested Type

    private sealed class UnboundColumnCellRecyclingGroupKey
    {
      internal UnboundColumnCellRecyclingGroupKey( object key )
      {
        m_key = key;
      }

      public override int GetHashCode()
      {
        if( m_key != null )
          return m_key.GetHashCode();

        return 0;
      }

      public override bool Equals( object obj )
      {
        UnboundColumnCellRecyclingGroupKey key = obj as UnboundColumnCellRecyclingGroupKey;
        if( key == null )
          return false;

        if( key == this )
          return true;

        return object.Equals( key.m_key, m_key );
      }

      private readonly object m_key;
    }

    #endregion
  }
}
