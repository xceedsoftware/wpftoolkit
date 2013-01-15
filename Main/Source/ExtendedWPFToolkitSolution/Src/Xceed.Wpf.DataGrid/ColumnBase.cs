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
  public abstract class ColumnBase : Freezable, INotifyPropertyChanged
  {
    static ColumnBase()
    {
      ColumnBase.IsFirstVisibleProperty = ColumnBase.IsFirstVisiblePropertyKey.DependencyProperty;
      ColumnBase.IsLastVisibleProperty = ColumnBase.IsLastVisiblePropertyKey.DependencyProperty;
    }

    protected ColumnBase()
    {
      // Set the ActualWidth "default" value.
      this.UpdateActualWidth();
    }

    protected ColumnBase( string fieldName, object title )
      : this()
    {
      this.FieldName = fieldName;
      this.Title = title;
    }

    #region CellContentTemplate Property

    internal event EventHandler CellContentTemplateChanged;

    public static readonly DependencyProperty CellContentTemplateProperty =
        DependencyProperty.Register( "CellContentTemplate",
        typeof( DataTemplate ),
        typeof( ColumnBase ),
        new FrameworkPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnCellContentTemplateChanged ) ) );

    public DataTemplate CellContentTemplate
    {
      get
      {
        return m_cellContentTemplate;
      }
      set
      {
        this.SetValue( ColumnBase.CellContentTemplateProperty, value );
      }
    }

    private DataTemplate m_cellContentTemplate;

    #endregion CellContentTemplate Property

    #region CellContentTemplateSelector Property

    public static readonly DependencyProperty CellContentTemplateSelectorProperty =
      DependencyProperty.Register(
        "CellContentTemplateSelector",
        typeof( DataTemplateSelector ),
        typeof( ColumnBase ),
        new FrameworkPropertyMetadata( GenericContentTemplateSelector.Instance,
          new PropertyChangedCallback( ColumnBase.OnCellContentTemplateSelectorChanged ) ) );

    public DataTemplateSelector CellContentTemplateSelector
    {
      get
      {
        return m_cellContentTemplateSelector;
      }
      set
      {
        this.SetValue( ColumnBase.CellContentTemplateSelectorProperty, value );
      }
    }

    private DataTemplateSelector m_cellContentTemplateSelector = GenericContentTemplateSelector.Instance;

    #endregion CellContentTemplateSelector Property

    #region CellHorizontalContentAlignment Property

    public static readonly DependencyProperty CellHorizontalContentAlignmentProperty =
        DependencyProperty.Register( "CellHorizontalContentAlignment",
        typeof( HorizontalAlignment ),
        typeof( ColumnBase ),
        new FrameworkPropertyMetadata( HorizontalAlignment.Stretch ) );

    public HorizontalAlignment CellHorizontalContentAlignment
    {
      get
      {
        return ( HorizontalAlignment )this.GetValue( ColumnBase.CellHorizontalContentAlignmentProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellHorizontalContentAlignmentProperty, value );
      }
    }

    #endregion CellHorizontalContentAlignment

    #region CellVerticalContentAlignment Property

    public static readonly DependencyProperty CellVerticalContentAlignmentProperty =
        DependencyProperty.Register( "CellVerticalContentAlignment",
        typeof( VerticalAlignment ),
        typeof( ColumnBase ),
        new FrameworkPropertyMetadata( VerticalAlignment.Stretch ) );

    public VerticalAlignment CellVerticalContentAlignment
    {
      get
      {
        return ( VerticalAlignment )this.GetValue( ColumnBase.CellVerticalContentAlignmentProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellVerticalContentAlignmentProperty, value );
      }
    }

    #endregion CellVerticalContentAlignment

    #region Title Property

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
      "Title",
      typeof( object ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnTitleChanged ) ) );

    private static void OnTitleChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = ( ColumnBase )sender;

      column.m_title = e.NewValue;

      if( column.TitleChanged != null )
        column.TitleChanged( column, EventArgs.Empty );
    }

    public object Title
    {
      get
      {
        return m_title;
      }
      set
      {
        this.SetValue( ColumnBase.TitleProperty, value );
      }
    }

    internal event EventHandler TitleChanged;
    private object m_title;

    #endregion Title Property

    #region TitleTemplate Property

    public static readonly DependencyProperty TitleTemplateProperty =
        DependencyProperty.Register( "TitleTemplate", typeof( DataTemplate ), typeof( ColumnBase ), new UIPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnTitleTemplateChanged ) ) );

    private DataTemplate m_titleTemplate;

    private static void OnTitleTemplateChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_titleTemplate = e.NewValue as DataTemplate;
    }

    public DataTemplate TitleTemplate
    {
      get
      {
        return m_titleTemplate;
      }
      set
      {
        this.SetValue( ColumnBase.TitleTemplateProperty, value );
      }
    }

    #endregion TitleTemplate Property

    #region TitleTemplateSelector Property

    public static readonly DependencyProperty TitleTemplateSelectorProperty =
        DependencyProperty.Register( "TitleTemplateSelector", typeof( DataTemplateSelector ), typeof( ColumnBase ), new UIPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnTitleTemplateSelectorChanged ) ) );

    private DataTemplateSelector m_titleTemplateSelector;

    private static void OnTitleTemplateSelectorChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_titleTemplateSelector = e.NewValue as DataTemplateSelector;
    }

    public DataTemplateSelector TitleTemplateSelector
    {
      get
      {
        return m_titleTemplateSelector;
      }
      set
      {
        this.SetValue( ColumnBase.TitleTemplateSelectorProperty, value );
      }
    }

    #endregion TitleTemplateSelector Property

    #region ActualWidth Property

    private static readonly DependencyPropertyKey ActualWidthPropertyKey =
      DependencyProperty.RegisterReadOnly( "ActualWidth", typeof( double ), typeof( ColumnBase ), new PropertyMetadata( ColumnBase.OnActualWidthChanged ) );

    public static readonly DependencyProperty ActualWidthProperty = ColumnBase.ActualWidthPropertyKey.DependencyProperty;

    private static void OnActualWidthChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      if( column.ActualWidthChanged != null )
      {
        Debug.Assert( e.NewValue != null );
        Debug.Assert( e.OldValue != null );
        column.m_actualWidth = ( double )e.NewValue;
        column.ActualWidthChanged( column, new ColumnActualWidthChangedEventArgs( column, ( double )e.OldValue, ( double )e.NewValue ) );
      }
    }

    private void UpdateActualWidth()
    {
      double actualWidth = ( ( this.Width.UnitType == ColumnWidthUnitType.Pixel ) ? this.Width.Value : ( ( ColumnWidth )WidthProperty.DefaultMetadata.DefaultValue ).Value );
      double maxWidth = this.MaxWidth;
      double minWidth = this.MinWidth;
      bool useDesiredWidth = false;

      if( this.DesiredWidth >= 0d )
      {
        actualWidth = this.DesiredWidth;
        useDesiredWidth = true;
      }

      if( actualWidth > maxWidth )
        actualWidth = maxWidth;

      // If Width is set to a value lesser than MinWidth or, if MaxWidth is inferior to
      // MinWidth, ActualWidth will become equal to MinWidth.
      if( actualWidth < minWidth )
        actualWidth = minWidth;

      this.SetValue( ColumnBase.ActualWidthPropertyKey, actualWidth );
      m_actualWidth = actualWidth;

      if( ( minWidth >= maxWidth ) || ( useDesiredWidth ) )
      {
        this.SetHasFixedWidth( true );
      }
      else
      {
        this.ClearValue( ColumnBase.HasFixedWidthPropertyKey );
      }
    }

    public double ActualWidth
    {
      get
      {
        return m_actualWidth;
      }
    }

    private double m_actualWidth;

    #endregion ActualWidth Property

    #region Width Property

    public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
      "Width",
      typeof( ColumnWidth ),
      typeof( ColumnBase ),
      new PropertyMetadata( new ColumnWidth( 125d ), new PropertyChangedCallback( ColumnBase.WidthChanged ) ) );

    public ColumnWidth Width
    {
      get
      {
        return ( ColumnWidth )this.GetValue( ColumnBase.WidthProperty );
      }
      set
      {
        this.SetValue( ColumnBase.WidthProperty, value );
      }
    }

    private static void WidthChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = ( ColumnBase )sender;

      if( e.Property == ColumnBase.WidthProperty )
      {
        ColumnWidth oldValue = ( ColumnWidth )e.OldValue;
        ColumnWidth newValue = ( ColumnWidth )e.NewValue;

        if( ( oldValue.UnitType == ColumnWidthUnitType.Star ) &&
            ( newValue.UnitType == ColumnWidthUnitType.Pixel ) )
        {
          // If the old width was a star, reset the desired width before updating the 
          // ActualWidth. Otherwise, ActualWidth would still return the old desired width.
          column.ClearValue( ColumnBase.DesiredWidthProperty );
        }
        else if( newValue.UnitType == ColumnWidthUnitType.Star )
        {
          // Assign a temporary value to DesiredWidth (different from ActualWidth) 
          // to force a new Measure pass. Mandatory, for instance, when the old star
          // value was limited by a MaxValue and the new star value would give a 
          // smaller value to DesiredWidth.
          double oldActualWidth = column.ActualWidth;

          if( column.ActualWidth > column.MinWidth )
          {
            column.DesiredWidth = column.MinWidth;
          }
          else
          {
            column.DesiredWidth = column.ActualWidth + 1d;
          }
        }
      }

      column.UpdateActualWidth();
    }

    #endregion Width Property

    #region MinWidth Property

    public static readonly DependencyProperty MinWidthProperty =
        FrameworkElement.MinWidthProperty.AddOwner( typeof( ColumnBase ), new PropertyMetadata( new PropertyChangedCallback( ColumnBase.WidthChanged ) ) );

    public double MinWidth
    {
      get
      {
        return ( double )this.GetValue( ColumnBase.MinWidthProperty );
      }
      set
      {
        this.SetValue( ColumnBase.MinWidthProperty, value );
      }
    }

    #endregion MinWidth Property

    #region MaxWidth Property

    public static readonly DependencyProperty MaxWidthProperty =
        FrameworkElement.MaxWidthProperty.AddOwner( typeof( ColumnBase ), new PropertyMetadata( new PropertyChangedCallback( ColumnBase.WidthChanged ) ) );

    public double MaxWidth
    {
      get
      {
        return ( double )this.GetValue( ColumnBase.MaxWidthProperty );
      }
      set
      {
        this.SetValue( ColumnBase.MaxWidthProperty, value );
      }
    }

    #endregion MaxWidth Property

    #region DesiredWidth Property

    internal static readonly DependencyProperty DesiredWidthProperty =
        DependencyProperty.Register( "DesiredWidth", typeof( double ), typeof( ColumnBase ), new PropertyMetadata( -1d, new PropertyChangedCallback( ColumnBase.WidthChanged ) ) );

    internal double DesiredWidth
    {
      get
      {
        return ( double )this.GetValue( ColumnBase.DesiredWidthProperty );
      }
      set
      {
        this.SetValue( ColumnBase.DesiredWidthProperty, value );
      }
    }

    #endregion DesiredWidth Property

    #region TextTrimming Property

    public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
      "TextTrimming",
      typeof( TextTrimming ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( TextTrimming.CharacterEllipsis, new PropertyChangedCallback( ColumnBase.OnTextTrimmingPropertyChanged ) ) );

    private TextTrimming m_textTrimming = TextTrimming.CharacterEllipsis;

    private static void OnTextTrimmingPropertyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      if( e.NewValue != null )
        column.m_textTrimming = ( TextTrimming )e.NewValue;
    }

    public TextTrimming TextTrimming
    {
      get
      {
        return m_textTrimming;
      }
      set
      {
        this.SetValue( ColumnBase.TextTrimmingProperty, value );
      }
    }

    #endregion TextTrimming Property

    #region TextWrapping Property

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
      "TextWrapping",
      typeof( TextWrapping ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( TextWrapping.NoWrap, new PropertyChangedCallback( ColumnBase.OnTextWrappingChanged ) ) );

    private TextWrapping m_textWrapping = TextWrapping.NoWrap;

    private static void OnTextWrappingChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      if( e.NewValue != null )
        column.m_textWrapping = ( TextWrapping )e.NewValue;
    }

    public TextWrapping TextWrapping
    {
      get
      {
        return m_textWrapping;
      }
      set
      {
        this.SetValue( ColumnBase.TextWrappingProperty, value );
      }
    }

    #endregion TextWrapping Property

    #region HasFixedWidth Read-Only Property

    private static readonly DependencyPropertyKey HasFixedWidthPropertyKey =
      DependencyProperty.RegisterReadOnly( "HasFixedWidth", typeof( bool ), typeof( ColumnBase ), new PropertyMetadata( false, new PropertyChangedCallback( ColumnBase.OnHasFixedWidthChanged ) ) );

    public static readonly DependencyProperty HasFixedWidthProperty =
      ColumnBase.HasFixedWidthPropertyKey.DependencyProperty;

    private bool m_hasFixedWidth = false;

    private static void OnHasFixedWidthChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_hasFixedWidth = ( bool )e.NewValue;
    }

    public bool HasFixedWidth
    {
      get
      {
        return m_hasFixedWidth;
      }
    }

    private void SetHasFixedWidth( bool value )
    {
      this.SetValue( ColumnBase.HasFixedWidthPropertyKey, value );
    }

    #endregion HasFixedWidth Read-Only Property

    #region VisiblePosition Property

    public static readonly DependencyProperty VisiblePositionProperty =
      DependencyProperty.Register(
      "VisiblePosition",
      typeof( int ),
      typeof( ColumnBase ),
      new UIPropertyMetadata(
        int.MaxValue,
        new PropertyChangedCallback( ColumnBase.OnVisibilePositionChanged ),
        new CoerceValueCallback( ColumnBase.OnCoerceVisiblePosition ) ) );

    public int VisiblePosition
    {
      get
      {
        return m_visiblePosition;
      }
      set
      {
        this.SetValue( ColumnBase.VisiblePositionProperty, value );
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    private static object OnCoerceVisiblePosition( DependencyObject sender, object value )
    {
      if( ( int )value < 0 )
        throw new ArgumentOutOfRangeException( "VisiblePosition", "VisiblePosition must be greater than or equal to zero." );

      return value;
    }

    private static void OnVisibilePositionChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = ( ColumnBase )sender;

      column.m_visiblePosition = ( int )e.NewValue;

      if( column.VisiblePositionChanged != null )
        column.VisiblePositionChanged( column, new ColumnVisiblePositionChangedEventArgs( column, ( int )e.OldValue, ( int )e.NewValue ) );

      // Invalidate own PreviousVisibleColumnProperty
      column.InvalidatePreviousVisibleColumnProperty();

      // Also invalidate next visible column's PreviousVisibleColumnProperty
      var nextVisibleColumn = column.NextVisibleColumn;

      if( nextVisibleColumn != null )
      {
        nextVisibleColumn.InvalidatePreviousVisibleColumnProperty();
      }
    }

    internal event EventHandler VisiblePositionChanged;
    private int m_visiblePosition = Int32.MaxValue;

    #endregion VisiblePosition Property

    #region IsFirstVisible Read-Only Property

    private static readonly DependencyPropertyKey IsFirstVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly( "IsFirstVisible", typeof( bool ), typeof( ColumnBase ), new PropertyMetadata( false, new PropertyChangedCallback( ColumnBase.OnIsFirstVisibleChanged ) ) );

    public static readonly DependencyProperty IsFirstVisibleProperty;

    private bool m_isFirstVisible; // = false;

    private static void OnIsFirstVisibleChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_isFirstVisible = ( bool )e.NewValue;
    }

    public bool IsFirstVisible
    {
      get
      {
        return m_isFirstVisible;
      }
    }

    internal void SetIsFirstVisible( bool value )
    {
      this.SetValue( ColumnBase.IsFirstVisiblePropertyKey, value );
    }

    internal void ClearIsFirstVisible()
    {
      this.ClearValue( ColumnBase.IsFirstVisiblePropertyKey );
      m_isFirstVisible = false;
    }

    #endregion IsFirstVisible Read-Only Property

    #region IsLastVisible Read-Only Property

    private static readonly DependencyPropertyKey IsLastVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly( "IsLastVisible", typeof( bool ), typeof( ColumnBase ), new PropertyMetadata( false, new PropertyChangedCallback( ColumnBase.OnIsLastVisibleChanged ) ) );

    public static readonly DependencyProperty IsLastVisibleProperty;

    private bool m_isLastVisible; // = false;

    private static void OnIsLastVisibleChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_isLastVisible = ( bool )e.NewValue;
    }

    public bool IsLastVisible
    {
      get
      {
        return m_isLastVisible;
      }
    }

    internal void SetIsLastVisible( bool value )
    {
      this.SetValue( ColumnBase.IsLastVisiblePropertyKey, value );
    }

    internal void ClearIsLastVisible()
    {
      this.ClearValue( ColumnBase.IsLastVisiblePropertyKey );
    }

    #endregion IsLastVisible Read-Only Property

    #region FieldName Property

    public static readonly DependencyProperty FieldNameProperty =
        DependencyProperty.Register( "FieldName", typeof( string ), typeof( ColumnBase ), new UIPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnFieldNameChanged ), new CoerceValueCallback( ColumnBase.OnCoerceFieldName ) ) );

    private string m_fieldName; // = null;

    private static void OnFieldNameChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_fieldName = ( string )e.NewValue;
    }

    private static object OnCoerceFieldName( DependencyObject sender, object requestedValue )
    {
      ColumnBase column = sender as ColumnBase;

      if( ( column.m_containingCollection != null ) && ( !DesignerProperties.GetIsInDesignMode( column ) ) )
        throw new InvalidOperationException( "An attempt was made to change the FieldName of a column that is contained in a grid." );

      return requestedValue;
    }

    public string FieldName
    {
      get
      {
        return m_fieldName;
      }
      set
      {
        this.SetValue( ColumnBase.FieldNameProperty, value );
      }
    }

    #endregion FieldName Property

    #region Visible Property

    public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
      "Visible",
      typeof( bool ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( true, new PropertyChangedCallback( ColumnBase.OnVisiblePropertyChanged ) ) );

    public bool Visible
    {
      get
      {
        return m_visible;
      }
      set
      {
        this.SetValue( ColumnBase.VisibleProperty, value );
      }
    }

    private static void OnVisiblePropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = ( ColumnBase )sender;

      column.m_visible = ( bool )e.NewValue;

      if( column.VisibleChanged != null )
        column.VisibleChanged( column, EventArgs.Empty );
    }

    internal event EventHandler VisibleChanged;
    private bool m_visible = true;

    #endregion Visible Property

    #region Index Property

    public int Index
    {
      get
      {
        if( m_containingCollection == null )
          return -1;

        return m_containingCollection.IndexOf( this );
      }
    }

    #endregion Index Property

    #region DataGridControl Property

    public DataGridControl DataGridControl
    {
      get
      {
        if( m_containingCollection == null )
          return null;

        return m_containingCollection.DataGridControl;
      }
    }

    internal void NotifyDataGridControlChanged()
    {
      this.OnPropertyChanged( new PropertyChangedEventArgs( "DataGridControl" ) );
    }

    #endregion DataGridControl Property

    #region ParentDetailConfiguration Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get
      {
        if( m_containingCollection == null )
          return null;

        return m_containingCollection.ParentDetailConfiguration;
      }
    }

    #endregion ParentDetailConfiguration Property

    #region ContainingCollection Property

    internal ColumnCollection ContainingCollection
    {
      get
      {
        return m_containingCollection;
      }
    }

    #endregion ContainingCollection Property

    #region AllowSort Property

    public virtual bool AllowSort
    {
      get
      {
        return false;
      }
      set
      {
        throw new NotSupportedException( "An attempt was made to set the AllowSort property of a column that does not support sorting." );
      }
    }

    #endregion AllowSort Property

    #region AllowGroup Property

    public virtual bool AllowGroup
    {
      get
      {
        return false;
      }
      set
      {
        throw new NotSupportedException( "An attempt was made to set the AllowGroup property of a column that does not support grouping." );
      }
    }

    #endregion AllowGroup Property

    #region GroupValueTemplate Property

    public static readonly DependencyProperty GroupValueTemplateProperty =
        DependencyProperty.Register( "GroupValueTemplate", typeof( DataTemplate ), typeof( ColumnBase ), new FrameworkPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnGroupValueTemplateChanged ) ) );

    private DataTemplate m_groupValueTemplate;

    private static void OnGroupValueTemplateChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_groupValueTemplate = e.NewValue as DataTemplate;
    }

    public DataTemplate GroupValueTemplate
    {
      get
      {
        return m_groupValueTemplate;
      }
      set
      {
        this.SetValue( ColumnBase.GroupValueTemplateProperty, value );
      }
    }

    #endregion GroupValueTemplate Property

    #region GroupValueTemplateSelector Property

    public static readonly DependencyProperty GroupValueTemplateSelectorProperty =
        DependencyProperty.Register( "GroupValueTemplateSelector",
        typeof( DataTemplateSelector ),
        typeof( ColumnBase ),
        new FrameworkPropertyMetadata( null,
          new PropertyChangedCallback( ColumnBase.OnGroupValueTemplateSelectorChanged ) ) );

    private DataTemplateSelector m_groupValueTemplateSelector; // = null;

    private static void OnGroupValueTemplateSelectorChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_groupValueTemplateSelector = e.NewValue as DataTemplateSelector;
    }

    public DataTemplateSelector GroupValueTemplateSelector
    {
      get
      {
        return m_groupValueTemplateSelector;
      }
      set
      {
        this.SetValue( ColumnBase.GroupValueTemplateSelectorProperty, value );
      }
    }

    #endregion GroupValueTemplateSelector Property

    #region CellEditor Property

    public static readonly DependencyProperty CellEditorProperty = DependencyProperty.Register(
      "CellEditor",
      typeof( CellEditor ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnCellEditorChanged ) ) );

    private CellEditor m_cellEditor = null;

    private static void OnCellEditorChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_cellEditor = e.NewValue as CellEditor;
    }

    public CellEditor CellEditor
    {
      get
      {
        return m_cellEditor;
      }
      set
      {
        this.SetValue( ColumnBase.CellEditorProperty, value );
      }
    }

    #endregion CellEditor Property

    #region CellEditorDisplayConditions Property

    public static readonly DependencyProperty CellEditorDisplayConditionsProperty =
        DataGridControl.CellEditorDisplayConditionsProperty.AddOwner( typeof( ColumnBase ), new FrameworkPropertyMetadata( new PropertyChangedCallback( ColumnBase.OnCellEditorDisplayConditionsChanged ) ) );

    private CellEditorDisplayConditions m_cellEditorDisplayConditions;

    public CellEditorDisplayConditions CellEditorDisplayConditions
    {
      get
      {
        return m_cellEditorDisplayConditions;
      }
      set
      {
        this.SetValue( ColumnBase.CellEditorDisplayConditionsProperty, value );
      }
    }

    internal event EventHandler CellEditorDisplayConditionsChanged;

    private static void OnCellEditorDisplayConditionsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = ( ColumnBase )sender;

      if( column == null )
        return;

      if( e.NewValue != null )
        column.m_cellEditorDisplayConditions = ( CellEditorDisplayConditions )e.NewValue;

      if( column.CellEditorDisplayConditionsChanged != null )
      {
        column.CellEditorDisplayConditionsChanged( column, new EventArgs() );
      }
    }

    #endregion CellEditorDisplayConditions Property

    #region CellValidationRules Property

    public Collection<CellValidationRule> CellValidationRules
    {
      get
      {
        if( m_cellValidationRules == null )
          m_cellValidationRules = new Collection<CellValidationRule>();

        return m_cellValidationRules;
      }
    }

    private Collection<CellValidationRule> m_cellValidationRules; // = null

    #endregion CellValidationRules Property

    #region CellErrorStyle Property

    public static readonly DependencyProperty CellErrorStyleProperty =
      DataGridControl.CellErrorStyleProperty.AddOwner( typeof( ColumnBase ) );

    public Style CellErrorStyle
    {
      get
      {
        return ( Style )this.GetValue( ColumnBase.CellErrorStyleProperty );
      }

      set
      {
        this.SetValue( ColumnBase.CellErrorStyleProperty, value );
      }
    }

    #endregion CellErrorStyle Property

    #region CanBeCurrentWhenReadOnly Property

    internal event EventHandler CanBeCurrentWhenReadOnlyChanged;

    public static readonly DependencyProperty CanBeCurrentWhenReadOnlyProperty =
      DependencyProperty.Register(
      "CanBeCurrentWhenReadOnly",
      typeof( bool ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( ( bool )true, new PropertyChangedCallback( ColumnBase.OnCanBeCurrentWhenReadOnlyChanged ) ) );

    public bool CanBeCurrentWhenReadOnly
    {
      get
      {
        return m_canBeCurrentWhenReadOnly;
      }
      set
      {
        this.SetValue( ColumnBase.CanBeCurrentWhenReadOnlyProperty, value );
      }
    }
    private bool m_canBeCurrentWhenReadOnly = true;

    private static void OnCanBeCurrentWhenReadOnlyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = sender as ColumnBase;

      if( column == null )
        return;

      column.m_canBeCurrentWhenReadOnly = ( bool )e.NewValue;

      if( column.CanBeCurrentWhenReadOnlyChanged != null )
      {
        column.CanBeCurrentWhenReadOnlyChanged( sender, new EventArgs() );
      }
    }

    #endregion CanBeCurrentWhenReadOnly

    #region HasValidationError Property

    private static readonly DependencyPropertyKey HasValidationErrorPropertyKey =
        DependencyProperty.RegisterReadOnly( "HasValidationError", typeof( bool ), typeof( ColumnBase ), new UIPropertyMetadata( false, new PropertyChangedCallback( ColumnBase.OnHasValidationErrorChanged ) ) );

    public static readonly DependencyProperty HasValidationErrorProperty =
      HasValidationErrorPropertyKey.DependencyProperty;

    private bool m_hasValidationError;

    private static void OnHasValidationErrorChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      if( e.NewValue != null )
        column.m_hasValidationError = ( bool )e.NewValue;
    }

    public bool HasValidationError
    {
      get
      {
        return m_hasValidationError;
      }
    }

    internal void SetHasValidationError( bool value )
    {
      if( value != this.HasValidationError )
      {
        if( value )
        {
          this.SetValue( ColumnBase.HasValidationErrorPropertyKey, value );
        }
        else
        {
          this.SetValue( ColumnBase.HasValidationErrorPropertyKey, DependencyProperty.UnsetValue );
        }
      }
    }

    #endregion HasValidationError Property

    #region SortDirection Property

    private static readonly DependencyPropertyKey SortDirectionPropertyKey =
        DependencyProperty.RegisterReadOnly( "SortDirection", typeof( SortDirection ), typeof( ColumnBase ), new PropertyMetadata( SortDirection.None, new PropertyChangedCallback( ColumnBase.OnSortDirectionChanged ) ) );

    public static readonly DependencyProperty SortDirectionProperty =
      ColumnBase.SortDirectionPropertyKey.DependencyProperty;

    private SortDirection m_sortDirection = SortDirection.None;

    private static void OnSortDirectionChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_sortDirection = ( SortDirection )e.NewValue;
    }

    public SortDirection SortDirection
    {
      get
      {
        return m_sortDirection;
      }
    }

    internal void SetSortDirection( SortDirection value )
    {
      if( value == SortDirection.None )
      {
        this.SetValue( ColumnBase.SortDirectionPropertyKey, DependencyProperty.UnsetValue );
      }
      else
      {
        this.SetValue( ColumnBase.SortDirectionPropertyKey, value );
      }
    }

    #endregion SortDirection Property

    #region SortIndex Read-Only Property

    private static readonly DependencyPropertyKey SortIndexPropertyKey =
        DependencyProperty.RegisterReadOnly( "SortIndex", typeof( int ), typeof( ColumnBase ), new PropertyMetadata( -1, new PropertyChangedCallback( ColumnBase.OnSortIndexChanged ) ) );

    public static readonly DependencyProperty SortIndexProperty =
      ColumnBase.SortIndexPropertyKey.DependencyProperty;

    private int m_sortIndex = -1;

    private static void OnSortIndexChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = o as ColumnBase;

      if( column == null )
        return;

      column.m_sortIndex = ( int )e.NewValue;
    }

    public int SortIndex
    {
      get
      {
        return m_sortIndex;
      }
    }

    internal void SetSortIndex( int value )
    {
      if( value == -1 )
      {
        this.SetValue( ColumnBase.SortIndexPropertyKey, DependencyProperty.UnsetValue );
      }
      else
      {
        this.SetValue( ColumnBase.SortIndexPropertyKey, value );
      }
    }

    #endregion SortIndex Read-Only Property

    #region GroupDescription Property

    public static readonly DependencyProperty GroupDescriptionProperty =
        DependencyProperty.Register( "GroupDescription", typeof( GroupDescription ), typeof( ColumnBase ), new UIPropertyMetadata( null ) );

    public GroupDescription GroupDescription
    {
      get
      {
        return ( GroupDescription )GetValue( ColumnBase.GroupDescriptionProperty );
      }
      set
      {
        SetValue( ColumnBase.GroupDescriptionProperty, value );
      }
    }

    #endregion GroupDescription Property

    #region GroupConfiguration Property

    public static readonly DependencyProperty GroupConfigurationProperty =
        DependencyProperty.Register( "GroupConfiguration", typeof( GroupConfiguration ), typeof( ColumnBase ), new UIPropertyMetadata( null ) );

    public GroupConfiguration GroupConfiguration
    {
      get
      {
        return ( GroupConfiguration )this.GetValue( ColumnBase.GroupConfigurationProperty );
      }
      set
      {
        this.SetValue( ColumnBase.GroupConfigurationProperty, value );
      }
    }

    #endregion GroupConfiguration Property

    #region IsMainColumn Property

    private Nullable<bool> m_isMainColumn = null;

    public bool IsMainColumn
    {
      get
      {
        ColumnCollection containingCollection = this.ContainingCollection;

        if( containingCollection == null )
          return ( m_isMainColumn.HasValue ? m_isMainColumn.Value : false );

        return ( containingCollection.MainColumn == this );
      }
      set
      {
        ColumnCollection containingCollection = this.ContainingCollection;

        if( containingCollection == null )
        {
          if( ( !m_isMainColumn.HasValue ) || ( m_isMainColumn.Value != value ) )
          {
            m_isMainColumn = value;
            this.OnPropertyChanged( new PropertyChangedEventArgs( "IsMainColumn" ) );
          }
        }
        else
        {
          ColumnBase oldMainColumn = containingCollection.MainColumn;

          if( oldMainColumn != this )
          {
            containingCollection.MainColumn = this;
            // These two PropertyChanged are done in this order to be consistent with
            // the events' order when adding a Column already "IsMainColumn".
            this.OnPropertyChanged( new PropertyChangedEventArgs( "IsMainColumn" ) );

            if( oldMainColumn != null )
            {
              oldMainColumn.OnPropertyChanged( new PropertyChangedEventArgs( "IsMainColumn" ) );
            }
          }
        }
      }
    }

    #endregion IsMainColumn Property

    #region ReadOnly Property

    public static readonly DependencyProperty ReadOnlyProperty =
        DataGridControl.ReadOnlyProperty.AddOwner( typeof( ColumnBase ) );

    public virtual bool ReadOnly
    {
      get
      {
        return ( bool )this.GetValue( ColumnBase.ReadOnlyProperty );
      }
      set
      {
        this.SetValue( ColumnBase.ReadOnlyProperty, value );
      }
    }

    #endregion ReadOnly Property

    #region OverrideReadOnlyForInsertion Property

    public static readonly DependencyProperty OverrideReadOnlyForInsertionProperty = DependencyProperty.Register(
      "OverrideReadOnlyForInsertion",
      typeof( bool ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( false ) );

    public bool OverrideReadOnlyForInsertion
    {
      get
      {
        return ( bool )this.GetValue( ColumnBase.OverrideReadOnlyForInsertionProperty );
      }
      set
      {
        this.SetValue( ColumnBase.OverrideReadOnlyForInsertionProperty, value );
      }
    }

    #endregion OverrideReadOnlyForInsertion Property

    #region PreviousVisibleColumn Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    public ColumnBase PreviousVisibleColumn
    {
      get
      {
        // Column is being initialized.
        if( this.DataGridControl == null )
          return null;

        var columnsByVisiblePosition =
          ( this.ParentDetailConfiguration != null ) ? this.ParentDetailConfiguration.ColumnsByVisiblePosition : this.DataGridControl.ColumnsByVisiblePosition;

        var previousColumnNode = columnsByVisiblePosition.Find( this ).Previous;

        while( previousColumnNode != null )
        {
          var previousColumn = previousColumnNode.Value;

          if( previousColumn.Visible )
            return previousColumn;

          previousColumnNode = previousColumnNode.Previous;
        }

        return null;
      }
    }

    internal void InvalidatePreviousVisibleColumnProperty()
    {
      this.OnPropertyChanged( new PropertyChangedEventArgs( "PreviousVisibleColumn" ) );
    }

    #endregion

    #region NextVisibleColumn Property

    private ColumnBase NextVisibleColumn
    {
      get
      {
        // Column is being initialized.
        if( this.DataGridControl == null )
          return null;

        var columnsByVisiblePosition =
          ( this.ParentDetailConfiguration != null ) ? this.ParentDetailConfiguration.ColumnsByVisiblePosition : this.DataGridControl.ColumnsByVisiblePosition;

        var nextColumnNode = columnsByVisiblePosition.Find( this ).Next;

        while( nextColumnNode != null )
        {
          var previousColumn = nextColumnNode.Value;

          if( previousColumn.Visible )
            return previousColumn;

          nextColumnNode = nextColumnNode.Next;
        }

        return null;
      }
    }

    #endregion

    #region CellRecyclingGroup Property

    public static readonly DependencyProperty CellRecyclingGroupProperty = DependencyProperty.Register(
      "CellRecyclingGroup",
      typeof( object ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnCellRecyclingGroupChanged ) ) );

    private static void OnCellRecyclingGroupChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = sender as ColumnBase;

      if( column == null )
        return;

      column.m_cellRecyclingGroup = e.NewValue;
    }

    public object CellRecyclingGroup
    {
      get
      {
        return m_cellRecyclingGroup;
      }
      set
      {
        this.SetValue( ColumnBase.CellRecyclingGroupProperty, value );
      }
    }

    internal object GetCellRecyclingGroupOrDefault()
    {
      //Always return the one assigned by the client application
      if( m_cellRecyclingGroup != null )
        return m_cellRecyclingGroup;

      //If not assigned, then generate and return the default.
      if( m_defaultCellRecyclingGroup == null )
      {
        m_defaultCellRecyclingGroup = new CellRecyclingGroupKey( this );
      }

      return m_defaultCellRecyclingGroup;
    }

    private object m_cellRecyclingGroup;
    private object m_defaultCellRecyclingGroup;

    #endregion CellRecyclingGroup Property

    #region CurrentRowInEditionCellState Property

    internal CellState CurrentRowInEditionCellState
    {
      get
      {
        return m_currentRowInEditionCellState;
      }
      set
      {
        m_currentRowInEditionCellState = value;
      }
    }

    #endregion

    #region RealizedContainersRequested Event

    internal event RealizedContainersRequestedEventHandler RealizedContainersRequested;

    #endregion

    #region ActualWidthChanged Event

    internal event ColumnActualWidthChangedHandler ActualWidthChanged;

    #endregion

    #region DistinctValuesRequested Event

    internal event DistinctValuesRequestedEventHandler DistinctValuesRequested;

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

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
    public double GetFittedWidth()
    {
      double fittedWidth = -1;

      RealizedContainersRequestedEventArgs realizedContainers = new RealizedContainersRequestedEventArgs();

      if( this.RealizedContainersRequested != null )
      {
        this.RealizedContainersRequested( this, realizedContainers );
      }

      if( realizedContainers.RealizedContainers.Count > 0 )
      {
        fittedWidth = Math.Max( fittedWidth, this.GetElementCollectionFittedWidth( realizedContainers.RealizedContainers ) );
      }

      return fittedWidth;
    }

    // Not to be confused with the DependencyObject.OnPropertyChanged(DependencyPropertyChangedEventArgs) !!! This is an overload.
    protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
    {
      if( this.PropertyChanged != null )
        this.PropertyChanged( this, e );
    }

    protected override Freezable CreateInstanceCore()
    {
      // Only derived from Freezable to have DataContext and ElementName binding.
      // So we have not implemented the Clone.
      throw new NotImplementedException();
    }

    protected override bool FreezeCore( bool isChecking )
    {
      // Only derived from Freezable to have DataContext and ElementName binding.
      // So we don't want to be freezable.
      return false;
    }

    internal void AttachToContainingCollection( ColumnCollection columnCollection )
    {
      if( columnCollection == null )
        throw new ArgumentNullException( "columnCollection" );

      if( m_containingCollection != null )
        throw new InvalidOperationException( "An attempt was made to add a column to a grid while it already exists in another grid." );

      if( m_containingCollection == columnCollection )
        return;

      this.AttachToContainingCollectionCore( columnCollection );
    }

    internal void AttachToContainingCollectionCore( ColumnCollection columnCollection )
    {
      m_containingCollection = columnCollection;
      this.NotifyDataGridControlChanged();

      this.RealizedContainersRequested += m_containingCollection.OnRealizedContainersRequested;
      this.ActualWidthChanged += m_containingCollection.OnActualWidthChanged;
      this.DistinctValuesRequested += columnCollection.OnDistinctValuesRequested;

      if( m_isMainColumn.HasValue )
      {
        if( m_isMainColumn.Value )
        {
          ColumnBase oldMainColumn = columnCollection.MainColumn;
          columnCollection.MainColumn = this;

          if( oldMainColumn != null )
          {
            oldMainColumn.OnPropertyChanged( new PropertyChangedEventArgs( "IsMainColumn" ) );
          }
          // The PropertyChanged of this instance has already been fired before it 
          // was added to the collection.
        }

        m_isMainColumn = null;
      }
      else
      {
        if( columnCollection.Count == 0 )
        {
          this.IsMainColumn = true;
        }
      }
    }

    internal void DetachFromContainingCollection()
    {
      if( m_containingCollection == null )
        return;

      this.DetachFromContainingCollectionCore();
    }

    internal void DetachFromContainingCollectionCore()
    {
      this.RealizedContainersRequested -= m_containingCollection.OnRealizedContainersRequested;
      this.ActualWidthChanged -= m_containingCollection.OnActualWidthChanged;
      this.DistinctValuesRequested -= m_containingCollection.OnDistinctValuesRequested;

      if( this == m_containingCollection.MainColumn )
        m_containingCollection.MainColumn = null;

      m_containingCollection = null;
      this.NotifyDataGridControlChanged();
    }

    internal void OnDistinctValuesRequested( object sender, DistinctValuesRequestedEventArgs e )
    {
      if( this.DistinctValuesRequested != null )
        this.DistinctValuesRequested( this, e );
    }

    private double GetElementCollectionFittedWidth( IEnumerable collection )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );

      // Ensure to use the FieldName of the
      // Cell instead of the visible position
      // since a lookup dictionary is used underneath
      // and if the Indexer is used, the Cells are returned
      // in the order they were added, not visible position.
      double fittedWidth = -1;
      string fieldName = this.FieldName;

      foreach( object item in collection )
      {
        Row row = item as Row;

        if( row == null )
        {
          HeaderFooterItem headerFooter = item as HeaderFooterItem;

          if( headerFooter != null )
          {
            row = headerFooter.AsVisual() as Row;
          }
        }

        if( row == null )
          continue;

        Cell cell = row.Cells[ fieldName ];

        if( cell == null )
          continue;

        fittedWidth = Math.Max( fittedWidth, cell.GetFittedWidth() );
      }

      return fittedWidth;
    }

    private static void OnCellContentTemplateChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = sender as ColumnBase;
      Debug.Assert( column != null );

      if( column != null )
      {
        column.m_cellContentTemplate = e.NewValue as DataTemplate;

        //Make sure cells pertaining to this group are assigned to the correct recycle bin.
        //The recycle bin based on the previous key value will be clean up later by the FixedCellPanel, if no other column uses it.
        column.m_defaultCellRecyclingGroup = new CellRecyclingGroupKey( column );

        if( column.CellContentTemplateChanged != null )
        {
          column.CellContentTemplateChanged( sender, new EventArgs() );
        }
      }
    }

    private static void OnCellContentTemplateSelectorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ColumnBase column = sender as ColumnBase;
      Debug.Assert( column != null );

      if( column != null )
      {
        column.m_cellContentTemplateSelector = e.NewValue as DataTemplateSelector;

        if( column.CellContentTemplateChanged != null )
        {
          column.CellContentTemplateChanged( sender, new EventArgs() );
        }
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion INotifyPropertyChanged Members

    private ColumnCollection m_containingCollection; // = null
    private CellState m_currentRowInEditionCellState;

    internal partial class GenericContentTemplateSelector : DataTemplateSelector
    {
      static GenericContentTemplateSelector()
      {
        // We need to initalize the ResourceDictionary before accessing since we access
        // it in a static constructor and will be called before the Layout was performed
        GenericContentTemplateSelector.GenericContentTemplateResources.InitializeComponent();

        GenericContentTemplateSelector.BoolTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "booleanDefaultContentTemplate" ] as DataTemplate;
        Debug.Assert( GenericContentTemplateSelector.BoolTemplate != null );
        GenericContentTemplateSelector.BoolTemplate.Seal();

        GenericContentTemplateSelector.ImageTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "imageDefaultContentTemplate" ] as DataTemplate;
        Debug.Assert( GenericContentTemplateSelector.ImageTemplate != null );
        GenericContentTemplateSelector.ImageTemplate.Seal();

        GenericContentTemplateSelector.ForeignKeyDistinctValueItemContentTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "foreignKeyDistinctValueItemDefaultContentTemplate" ] as DataTemplate;
        Debug.Assert( GenericContentTemplateSelector.ForeignKeyDistinctValueItemContentTemplate != null );
        GenericContentTemplateSelector.ForeignKeyDistinctValueItemContentTemplate.Seal();

        GenericContentTemplateSelector.ForeignKeyCellContentTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "foreignKeyDefaultContentTemplate" ] as DataTemplate;
        Debug.Assert( GenericContentTemplateSelector.ForeignKeyCellContentTemplate != null );
        GenericContentTemplateSelector.ForeignKeyCellContentTemplate.Seal();

        GenericContentTemplateSelector.ForeignKeyGroupValueTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "foreignKeyGroupValueDefaultContentTemplate" ] as DataTemplate;
        Debug.Assert( GenericContentTemplateSelector.ForeignKeyGroupValueTemplate != null );
        GenericContentTemplateSelector.ForeignKeyGroupValueTemplate.Seal();

        GenericContentTemplateSelector.ForeignKeyScrollTipContentTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "foreignKeyScrollTipDefaultContentTemplate" ] as DataTemplate;
        Debug.Assert( GenericContentTemplateSelector.ForeignKeyScrollTipContentTemplate != null );
        GenericContentTemplateSelector.ForeignKeyScrollTipContentTemplate.Seal();

      }

      #region Singleton Pattern

      private GenericContentTemplateSelector()
      {
      }

      public static GenericContentTemplateSelector Instance
      {
        get
        {
          return m_instance;
        }
      }

      private static GenericContentTemplateSelector m_instance = new GenericContentTemplateSelector();

      #endregion

      public override DataTemplate SelectTemplate( object item, DependencyObject container )
      {
        DataTemplate template = null;
        bool useImageTemplate = false;

        if( ( item is byte[] ) || ( item is System.Drawing.Image ) )
        {
          ImageConverter converter = new ImageConverter();
          object convertedValue = null;

          try
          {
            convertedValue = converter.Convert( item, typeof( ImageSource ), null, System.Globalization.CultureInfo.CurrentCulture );
          }
          catch( NotSupportedException )
          {
            //suppress the exception, the byte[] is not an image. convertedValue will remain null
          }

          if( convertedValue != null )
            useImageTemplate = true;
        }
        else if( item is ImageSource )
        {
          useImageTemplate = true;
        }
        else if( item is bool )
        {
          template = GenericContentTemplateSelector.BoolTemplate;
        }

        if( useImageTemplate )
        {
          template = GenericContentTemplateSelector.ImageTemplate;

          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( container );

          DataGridControl parentGrid = ( dataGridContext != null )
            ? dataGridContext.DataGridControl
            : null;
        }

        if( template == null )
          template = base.SelectTemplate( item, container );

        return template;
      }

      private static GenericContentTemplateSelectorResources GenericContentTemplateResources = new GenericContentTemplateSelectorResources();

      private static readonly DataTemplate BoolTemplate;
      private static readonly DataTemplate ImageTemplate;

      internal static readonly DataTemplate ForeignKeyDistinctValueItemContentTemplate;
      internal static readonly DataTemplate ForeignKeyCellContentTemplate;
      internal static readonly DataTemplate ForeignKeyGroupValueTemplate;
      internal static readonly DataTemplate ForeignKeyScrollTipContentTemplate;
    }

    private sealed class CellRecyclingGroupKey
    {
      internal CellRecyclingGroupKey( ColumnBase column )
      {
        //This is a value that is cached at the cell level and that is costly to update, thus basing the recycling on it optimizes performance.
        m_cellContentTemplate = column.CellContentTemplate;
      }

      public override int GetHashCode()
      {
        if( m_cellContentTemplate != null )
        {
          return m_cellContentTemplate.GetHashCode();
        }

        return 0;
      }

      public override bool Equals( object obj )
      {
        CellRecyclingGroupKey key = obj as CellRecyclingGroupKey;
        if( key == null )
          return false;

        if( key == this )
          return true;

        return object.Equals( key.m_cellContentTemplate, m_cellContentTemplate );
      }

      private readonly DataTemplate m_cellContentTemplate;
    }
  }
}
