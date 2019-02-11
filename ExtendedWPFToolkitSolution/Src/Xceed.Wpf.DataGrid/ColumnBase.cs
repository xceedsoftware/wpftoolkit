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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Utils;
using Xceed.Wpf.DataGrid.ValidationRules;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  [DebuggerDisplay( "FieldName = {FieldName}" )]
  public abstract class ColumnBase : Freezable, INotifyPropertyChanged, IWeakEventListener
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

    #region Description Property

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
      "Description",
      typeof( string ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null ) );

    public string Description
    {
      get
      {
        return ( string )this.GetValue( ColumnBase.DescriptionProperty );
      }
      set
      {
        this.SetValue( ColumnBase.DescriptionProperty, value );
      }
    }

    #endregion

    #region CellContentTemplate Property

    public static readonly DependencyProperty CellContentTemplateProperty = DependencyProperty.Register(
      "CellContentTemplate",
      typeof( DataTemplate ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( null, new PropertyChangedCallback( ColumnBase.OnCellContentTemplateChanged ) ) );

    public DataTemplate CellContentTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( ColumnBase.CellContentTemplateProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellContentTemplateProperty, value );
      }
    }

    private static void OnCellContentTemplateChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as ColumnBase;
      if( self == null )
        return;

      //Make sure cells pertaining to this group are assigned to the correct recycle bin.
      //The recycle bin based on the previous key value will be clean up later by the FixedCellPanel, if no other column uses it.
      self.ResetDefaultCellRecyclingGroup();
    }

    #endregion

    #region CellContentTemplateSelector Property

    public static readonly DependencyProperty CellContentTemplateSelectorProperty = DependencyProperty.Register(
      "CellContentTemplateSelector",
      typeof( DataTemplateSelector ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( GenericContentTemplateSelector.Instance, new PropertyChangedCallback( ColumnBase.OnCellContentTemplateSelectorChanged ) ) );

    public DataTemplateSelector CellContentTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( ColumnBase.CellContentTemplateSelectorProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellContentTemplateSelectorProperty, value );
      }
    }

    private static void OnCellContentTemplateSelectorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as ColumnBase;
      if( self == null )
        return;

      //Make sure cells pertaining to this group are assigned to the correct recycle bin.
      //The recycle bin based on the previous key value will be clean up later by the FixedCellPanel, if no other column uses it.
      self.ResetDefaultCellRecyclingGroup();
    }

    #endregion

    #region CellHorizontalContentAlignment Property

    public static readonly DependencyProperty CellHorizontalContentAlignmentProperty = DependencyProperty.Register(
      "CellHorizontalContentAlignment",
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

    #endregion

    #region CellVerticalContentAlignment Property

    public static readonly DependencyProperty CellVerticalContentAlignmentProperty = DependencyProperty.Register(
      "CellVerticalContentAlignment",
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

    #endregion

    #region CellContentStringFormat Property

    public static readonly DependencyProperty CellContentStringFormatProperty = DependencyProperty.Register(
      "CellContentStringFormat",
      typeof( string ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( null ) );

    public string CellContentStringFormat
    {
      get
      {
        return ( string )this.GetValue( ColumnBase.CellContentStringFormatProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellContentStringFormatProperty, value );
      }
    }

    #endregion

    #region Title Property

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
      "Title",
      typeof( object ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null ) );

    public object Title
    {
      get
      {
        return this.GetValue( ColumnBase.TitleProperty );
      }
      set
      {
        this.SetValue( ColumnBase.TitleProperty, value );
      }
    }

    #endregion

    #region TitleTemplate Property

    public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
      "TitleTemplate",
      typeof( DataTemplate ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null ) );

    public DataTemplate TitleTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( ColumnBase.TitleTemplateProperty );
      }
      set
      {
        this.SetValue( ColumnBase.TitleTemplateProperty, value );
      }
    }

    #endregion

    #region TitleTemplateSelector Property

    public static readonly DependencyProperty TitleTemplateSelectorProperty = DependencyProperty.Register(
      "TitleTemplateSelector",
      typeof( DataTemplateSelector ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null ) );

    public DataTemplateSelector TitleTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( ColumnBase.TitleTemplateSelectorProperty );
      }
      set
      {
        this.SetValue( ColumnBase.TitleTemplateSelectorProperty, value );
      }
    }

    #endregion

    #region ActualWidth Property

    private static readonly DependencyPropertyKey ActualWidthPropertyKey = DependencyProperty.RegisterReadOnly(
      "ActualWidth",
      typeof( double ),
      typeof( ColumnBase ),
      new PropertyMetadata( ColumnBase.OnActualWidthChanged ) );

    public static readonly DependencyProperty ActualWidthProperty = ColumnBase.ActualWidthPropertyKey.DependencyProperty;

    public double ActualWidth
    {
      get
      {
        return ( double )this.GetValue( ColumnBase.ActualWidthProperty );
      }
    }

    private static void OnActualWidthChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var column = o as ColumnBase;
      if( column == null )
        return;

      var handler = column.ActualWidthChanged;
      if( handler != null )
      {
        handler.Invoke( column, new ColumnActualWidthChangedEventArgs( column, ( double )e.OldValue, ( double )e.NewValue ) );
      }
    }

    private void UpdateActualWidth()
    {
      var width = this.Width;
      var actualWidth = ( ( width.UnitType == ColumnWidthUnitType.Pixel ) ? width.Value : ( ( ColumnWidth )ColumnBase.WidthProperty.DefaultMetadata.DefaultValue ).Value );
      var maxWidth = this.MaxWidth;
      var minWidth = this.MinWidth;
      var useDesiredWidth = false;

      if( this.DesiredWidth >= 0d )
      {
        actualWidth = this.DesiredWidth;
        useDesiredWidth = true;
      }

      if( actualWidth > maxWidth )
      {
        actualWidth = maxWidth;
      }

      // If Width is set to a value lesser than MinWidth or, if MaxWidth is inferior to
      // MinWidth, ActualWidth will become equal to MinWidth.
      if( actualWidth < minWidth )
      {
        actualWidth = minWidth;
      }

      this.SetValue( ColumnBase.ActualWidthPropertyKey, actualWidth );

      if( ( minWidth >= maxWidth ) || ( useDesiredWidth ) )
      {
        this.HasFixedWidth = true;
      }
      else
      {
        this.ClearValue( ColumnBase.HasFixedWidthPropertyKey );
      }
    }

    #endregion

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
      var column = ( ColumnBase )sender;

      if( e.Property == ColumnBase.WidthProperty )
      {
        var oldValue = ( ColumnWidth )e.OldValue;
        var newValue = ( ColumnWidth )e.NewValue;

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
          var oldActualWidth = column.ActualWidth;

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

    #endregion

    #region MinWidth Property

    public static readonly DependencyProperty MinWidthProperty = FrameworkElement.MinWidthProperty.AddOwner( typeof( ColumnBase ), new PropertyMetadata( new PropertyChangedCallback( ColumnBase.WidthChanged ) ) );

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

    #endregion

    #region MaxWidth Property

    public static readonly DependencyProperty MaxWidthProperty = FrameworkElement.MaxWidthProperty.AddOwner( typeof( ColumnBase ), new PropertyMetadata( new PropertyChangedCallback( ColumnBase.WidthChanged ) ) );

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

    #endregion

    #region DesiredWidth Property

    internal static readonly DependencyProperty DesiredWidthProperty = DependencyProperty.Register( "DesiredWidth", typeof( double ), typeof( ColumnBase ), new PropertyMetadata( -1d, new PropertyChangedCallback( ColumnBase.WidthChanged ) ) );

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

    #endregion

    #region TextTrimming Property

    public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
      "TextTrimming",
      typeof( TextTrimming ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( TextTrimming.CharacterEllipsis ) );

    public TextTrimming TextTrimming
    {
      get
      {
        return ( TextTrimming )this.GetValue( ColumnBase.TextTrimmingProperty );
      }
      set
      {
        this.SetValue( ColumnBase.TextTrimmingProperty, value );
      }
    }

    #endregion

    #region TextWrapping Property

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
      "TextWrapping",
      typeof( TextWrapping ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( TextWrapping.NoWrap ) );

    public TextWrapping TextWrapping
    {
      get
      {
        return ( TextWrapping )this.GetValue( ColumnBase.TextWrappingProperty );
      }
      set
      {
        this.SetValue( ColumnBase.TextWrappingProperty, value );
      }
    }

    #endregion

    #region HasFixedWidth Read-Only Property

    private static readonly DependencyPropertyKey HasFixedWidthPropertyKey = DependencyProperty.RegisterReadOnly(
      "HasFixedWidth",
      typeof( bool ),
      typeof( ColumnBase ),
      new PropertyMetadata( false ) );

    public static readonly DependencyProperty HasFixedWidthProperty = ColumnBase.HasFixedWidthPropertyKey.DependencyProperty;

    public bool HasFixedWidth
    {
      get
      {
        return ( bool )this.GetValue( ColumnBase.HasFixedWidthProperty );
      }
      private set
      {
        this.SetValue( ColumnBase.HasFixedWidthPropertyKey, value );
      }
    }

    #endregion

    #region VisiblePosition Property

    public static readonly DependencyProperty VisiblePositionProperty = DependencyProperty.Register(
      "VisiblePosition",
      typeof( int ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( int.MaxValue, new PropertyChangedCallback( ColumnBase.OnVisiblePositionChanged ), new CoerceValueCallback( ColumnBase.OnCoerceVisiblePosition ) ) );

    public int VisiblePosition
    {
      get
      {
        return ( int )this.GetValue( ColumnBase.VisiblePositionProperty );
      }
      set
      {
        this.SetValue( ColumnBase.VisiblePositionProperty, value );
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    private static object OnCoerceVisiblePosition( DependencyObject sender, object value )
    {
      var newValue = ( int )value;
      if( newValue < 0 )
        throw new ArgumentOutOfRangeException( "VisiblePosition", "VisiblePosition must be greater than or equal to zero." );

      var column = ( ColumnBase )sender;

      if( !column.m_inhibitVisiblePositionChanging.IsSet )
      {
        var oldValue = column.VisiblePosition;

        if( newValue != oldValue )
        {
          var handler = column.VisiblePositionChanging;
          if( handler != null )
          {
            handler.Invoke( sender, EventArgs.Empty );
          }
        }
      }

      return value;
    }

    private static void OnVisiblePositionChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var column = ( ColumnBase )sender;
      var oldValue = ( int )e.OldValue;
      var newValue = ( int )e.NewValue;

      var handler = column.VisiblePositionChanged;
      if( handler != null )
      {
        handler.Invoke( column, new ColumnVisiblePositionChangedEventArgs( column, oldValue, newValue ) );
      }

      // Invalidate own PreviousVisibleColumnProperty
      column.InvalidatePreviousVisibleColumnProperty();

      // Also invalidate next visible column's PreviousVisibleColumnProperty
      var nextVisibleColumn = column.NextVisibleColumn;

      if( nextVisibleColumn != null )
      {
        nextVisibleColumn.InvalidatePreviousVisibleColumnProperty();
      }
    }

    internal IDisposable InhibitVisiblePositionChanging()
    {
      return m_inhibitVisiblePositionChanging.Set();
    }

    internal event EventHandler VisiblePositionChanging;
    internal event EventHandler VisiblePositionChanged;

    private readonly AutoResetFlag m_inhibitVisiblePositionChanging = AutoResetFlagFactory.Create( false );

    #endregion

    #region IsFirstVisible Read-Only Property

    private static readonly DependencyPropertyKey IsFirstVisiblePropertyKey = DependencyProperty.RegisterReadOnly(
      "IsFirstVisible",
      typeof( bool ),
      typeof( ColumnBase ),
      new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsFirstVisibleProperty;

    public bool IsFirstVisible
    {
      get
      {
        return ( bool )this.GetValue( ColumnBase.IsFirstVisibleProperty );
      }
    }

    internal void SetIsFirstVisible( bool value )
    {
      this.SetValue( ColumnBase.IsFirstVisiblePropertyKey, value );
    }

    internal void ClearIsFirstVisible()
    {
      this.ClearValue( ColumnBase.IsFirstVisiblePropertyKey );
    }

    #endregion

    #region IsLastVisible Read-Only Property

    private static readonly DependencyPropertyKey IsLastVisiblePropertyKey = DependencyProperty.RegisterReadOnly(
      "IsLastVisible",
      typeof( bool ),
      typeof( ColumnBase ),
      new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsLastVisibleProperty;

    public bool IsLastVisible
    {
      get
      {
        return ( bool )this.GetValue( ColumnBase.IsLastVisibleProperty );
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

    #endregion

    #region FieldName Property

    public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register(
      "FieldName",
      typeof( string ),
      typeof( ColumnBase ),
      new UIPropertyMetadata(
        null,
        null,
        new CoerceValueCallback( ColumnBase.OnCoerceFieldName ) ) );

    public string FieldName
    {
      get
      {
        return ( string )this.GetValue( ColumnBase.FieldNameProperty );
      }
      set
      {
        this.SetValue( ColumnBase.FieldNameProperty, value );
      }
    }

    private static object OnCoerceFieldName( DependencyObject sender, object requestedValue )
    {
      var column = sender as ColumnBase;
      if( ( column.m_containingCollection != null ) && ( !DesignerProperties.GetIsInDesignMode( column ) ) )
        throw new InvalidOperationException( "An attempt was made to change the FieldName of a column that is contained in a grid." );

      return requestedValue;
    }

    #endregion

    #region Visible Property

    public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
      "Visible",
      typeof( bool ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( true ) );

    public bool Visible
    {
      get
      {
        return ( bool )this.GetValue( ColumnBase.VisibleProperty );
      }
      set
      {
        this.SetValue( ColumnBase.VisibleProperty, value );
      }
    }

    #endregion

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

    #endregion

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
      this.OnPropertyChanged( new PropertyChangedEventArgs( ColumnBase.DataGridControlPropertyName ) );
    }

    internal static readonly string DataGridControlPropertyName = PropertyHelper.GetPropertyName( ( ColumnBase c ) => c.DataGridControl );

    #endregion

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

    #endregion

    #region ContainingCollection Property

    internal ColumnCollection ContainingCollection
    {
      get
      {
        return m_containingCollection;
      }
    }

    private ColumnCollection m_containingCollection; //null

    #endregion

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

    #endregion

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

    #endregion

    #region GroupValueTemplate Property

    public static readonly DependencyProperty GroupValueTemplateProperty = DependencyProperty.Register(
      "GroupValueTemplate",
      typeof( DataTemplate ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate GroupValueTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( ColumnBase.GroupValueTemplateProperty );
      }
      set
      {
        this.SetValue( ColumnBase.GroupValueTemplateProperty, value );
      }
    }

    #endregion

    #region GroupValueTemplateSelector Property

    public static readonly DependencyProperty GroupValueTemplateSelectorProperty = DependencyProperty.Register(
      "GroupValueTemplateSelector",
      typeof( DataTemplateSelector ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplateSelector GroupValueTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( ColumnBase.GroupValueTemplateSelectorProperty );
      }
      set
      {
        this.SetValue( ColumnBase.GroupValueTemplateSelectorProperty, value );
      }
    }

    #endregion

    #region GroupValueStringFormat Property

    public static readonly DependencyProperty GroupValueStringFormatProperty = DependencyProperty.Register(
      "GroupValueStringFormat",
      typeof( string ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( null ) );

    public string GroupValueStringFormat
    {
      get
      {
        return ( string )this.GetValue( ColumnBase.GroupValueStringFormatProperty );
      }
      set
      {
        this.SetValue( ColumnBase.GroupValueStringFormatProperty, value );
      }
    }

    #endregion

    #region CellEditor Property

    public static readonly DependencyProperty CellEditorProperty = DependencyProperty.Register(
      "CellEditor",
      typeof( CellEditor ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null ) );

    public CellEditor CellEditor
    {
      get
      {
        return ( CellEditor )this.GetValue( ColumnBase.CellEditorProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellEditorProperty, value );
      }
    }

    #endregion

    #region CellEditorSelector Property

    public static readonly DependencyProperty CellEditorSelectorProperty = DependencyProperty.Register(
      "CellEditorSelector",
      typeof( CellEditorSelector ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( null ) );

    public CellEditorSelector CellEditorSelector
    {
      get
      {
        return ( CellEditorSelector )this.GetValue( ColumnBase.CellEditorSelectorProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellEditorSelectorProperty, value );
      }
    }

    #endregion

    #region CellEditorDisplayConditions Property

    public static readonly DependencyProperty CellEditorDisplayConditionsProperty = DataGridControl.CellEditorDisplayConditionsProperty.AddOwner( typeof( ColumnBase ) );

    public CellEditorDisplayConditions CellEditorDisplayConditions
    {
      get
      {
        return ( CellEditorDisplayConditions )this.GetValue( ColumnBase.CellEditorDisplayConditionsProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellEditorDisplayConditionsProperty, value );
      }
    }

    #endregion

    #region CellValidationRules Property

    public Collection<CellValidationRule> CellValidationRules
    {
      get
      {
        if( m_cellValidationRules == null )
        {
          m_cellValidationRules = new Collection<CellValidationRule>();
        }

        return m_cellValidationRules;
      }
    }

    private Collection<CellValidationRule> m_cellValidationRules; // = null

    #endregion

    #region CellErrorStyle Property

    public static readonly DependencyProperty CellErrorStyleProperty = DataGridControl.CellErrorStyleProperty.AddOwner( typeof( ColumnBase ) );

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

    #endregion

    #region CanBeCurrentWhenReadOnly Property

    public static readonly DependencyProperty CanBeCurrentWhenReadOnlyProperty = DependencyProperty.Register(
      "CanBeCurrentWhenReadOnly",
      typeof( bool ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( true ) );

    public bool CanBeCurrentWhenReadOnly
    {
      get
      {
        return ( bool )this.GetValue( ColumnBase.CanBeCurrentWhenReadOnlyProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CanBeCurrentWhenReadOnlyProperty, value );
      }
    }

    #endregion

    #region HasValidationError Property

    private static readonly DependencyPropertyKey HasValidationErrorPropertyKey = DependencyProperty.RegisterReadOnly(
      "HasValidationError",
      typeof( bool ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty HasValidationErrorProperty = ColumnBase.HasValidationErrorPropertyKey.DependencyProperty;

    public bool HasValidationError
    {
      get
      {
        return ( bool )this.GetValue( ColumnBase.HasValidationErrorProperty );
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
          this.ClearValue( ColumnBase.HasValidationErrorPropertyKey );
        }
      }
    }

    #endregion

    #region SortDirection Property

    private static readonly DependencyPropertyKey SortDirectionPropertyKey = DependencyProperty.RegisterReadOnly(
      "SortDirection",
      typeof( SortDirection ),
      typeof( ColumnBase ),
      new PropertyMetadata( SortDirection.None ) );

    public static readonly DependencyProperty SortDirectionProperty = ColumnBase.SortDirectionPropertyKey.DependencyProperty;

    public SortDirection SortDirection
    {
      get
      {
        return ( SortDirection )this.GetValue( ColumnBase.SortDirectionProperty );
      }
    }

    internal void SetSortDirection( SortDirection value )
    {
      if( value == SortDirection.None )
      {
        this.ClearValue( ColumnBase.SortDirectionPropertyKey );
      }
      else
      {
        this.SetValue( ColumnBase.SortDirectionPropertyKey, value );
      }
    }

    #endregion

    #region SortDirectionCycle Property

    private static readonly SortDirectionCycleCollection DefaultSortDirectionCycle = new SortDirectionCycleCollection(
                                                                                       new ReadOnlyCollection<SortDirection>(
                                                                                         new List<SortDirection>()
                                                                                           {
                                                                                             SortDirection.Ascending,
                                                                                             SortDirection.Descending,
                                                                                             SortDirection.None,
                                                                                           } ) );

    public static readonly DependencyProperty SortDirectionCycleProperty = DependencyProperty.Register(
      "SortDirectionCycle",
      typeof( SortDirectionCycleCollection ),
      typeof( ColumnBase ),
      new PropertyMetadata( ColumnBase.DefaultSortDirectionCycle ) );

    public SortDirectionCycleCollection SortDirectionCycle
    {
      get
      {
        return ( SortDirectionCycleCollection )this.GetValue( ColumnBase.SortDirectionCycleProperty );
      }
      set
      {
        this.SetValue( ColumnBase.SortDirectionCycleProperty, value );
      }
    }

    #endregion

    #region SortIndex Read-Only Property

    private static readonly DependencyPropertyKey SortIndexPropertyKey = DependencyProperty.RegisterReadOnly(
      "SortIndex",
      typeof( int ),
      typeof( ColumnBase ),
      new PropertyMetadata( -1 ) );

    public static readonly DependencyProperty SortIndexProperty = ColumnBase.SortIndexPropertyKey.DependencyProperty;

    public int SortIndex
    {
      get
      {
        return ( int )this.GetValue( ColumnBase.SortIndexProperty );
      }
    }

    internal void SetSortIndex( int value )
    {
      if( value == -1 )
      {
        this.ClearValue( ColumnBase.SortIndexPropertyKey );
      }
      else
      {
        this.SetValue( ColumnBase.SortIndexPropertyKey, value );
      }
    }

    #endregion

    #region GroupDescription Property

    public static readonly DependencyProperty GroupDescriptionProperty = DependencyProperty.Register(
      "GroupDescription",
      typeof( GroupDescription ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null ) );

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

    #endregion

    #region GroupConfiguration Property

    public static readonly DependencyProperty GroupConfigurationProperty = DependencyProperty.Register(
      "GroupConfiguration",
      typeof( GroupConfiguration ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( null ) );

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

    #endregion

    #region IsMainColumn Property

    public virtual bool IsMainColumn
    {
      get
      {
        var containingCollection = this.ContainingCollection;
        if( containingCollection == null )
          return ( m_isMainColumn.HasValue ? m_isMainColumn.Value : false );

        return ( containingCollection.MainColumn == this );
      }
      set
      {
        var containingCollection = this.ContainingCollection;
        if( containingCollection == null )
        {
          if( m_isMainColumn == value )
            return;

          m_isMainColumn = value;

          this.RaiseIsMainColumnChanged();
        }
        else if( value )
        {
          containingCollection.MainColumn = this;
        }
        else
        {
          if( containingCollection.MainColumn != this )
            return;

          containingCollection.MainColumn = null;
        }
      }
    }

    internal void RaiseIsMainColumnChanged()
    {
      this.RefreshDraggableStatus();
      this.OnPropertyChanged( new PropertyChangedEventArgs( ColumnBase.IsMainColumnPropertyName ) );
    }

    internal static readonly string IsMainColumnPropertyName = PropertyHelper.GetPropertyName( ( ColumnBase c ) => c.IsMainColumn );

    private Nullable<bool> m_isMainColumn = null;

    #endregion

    #region ReadOnly Property

    public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
      "ReadOnly",
      typeof( bool ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( false ) );

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

    #endregion

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

    #endregion

    #region PreviousVisibleColumn Property

    [EditorBrowsable( EditorBrowsableState.Never )]
    public virtual ColumnBase PreviousVisibleColumn
    {
      get
      {
        // Column is being initialized.
        if( this.DataGridControl == null )
          return null;

        var columnsByVisiblePosition = ( this.ParentDetailConfiguration != null )
                                         ? this.ParentDetailConfiguration.ColumnsByVisiblePosition
                                         : this.DataGridControl.ColumnsByVisiblePosition;
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
      this.OnPropertyChanged( new PropertyChangedEventArgs( ColumnBase.PreviousVisibleColumnPropertyName ) );
    }

    internal static readonly string PreviousVisibleColumnPropertyName = PropertyHelper.GetPropertyName( ( ColumnBase c ) => c.PreviousVisibleColumn );

    #endregion

    #region NextVisibleColumn Property

    internal virtual ColumnBase NextVisibleColumn
    {
      get
      {
        // Column is being initialized.
        if( this.DataGridControl == null )
          return null;

        var columnsByVisiblePosition = ( this.ParentDetailConfiguration != null )
                                         ? this.ParentDetailConfiguration.ColumnsByVisiblePosition
                                         : this.DataGridControl.ColumnsByVisiblePosition;

        var nextColumnNode = columnsByVisiblePosition.Find( this );

        if( nextColumnNode != null )
        {
          nextColumnNode = nextColumnNode.Next;
        }

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
      new FrameworkPropertyMetadata( null ) );

    public object CellRecyclingGroup
    {
      get
      {
        return this.GetValue( ColumnBase.CellRecyclingGroupProperty );
      }
      set
      {
        this.SetValue( ColumnBase.CellRecyclingGroupProperty, value );
      }
    }

    internal object GetCellRecyclingGroupOrDefault()
    {
      //Always return the one assigned by the client application
      var recyclingGroup = this.CellRecyclingGroup;
      if( recyclingGroup != null )
        return recyclingGroup;

      var defaultCellRecyclingGroup = this.DefaultCellRecyclingGroup;
      Debug.Assert( defaultCellRecyclingGroup != null );

      return defaultCellRecyclingGroup;
    }

    #endregion

    #region DefaultCellRecyclingGroupDataType Internal Property

    internal Type DefaultCellRecyclingGroupDataType
    {
      get
      {
        return m_defaultCellRecyclingGroupDataType;
      }
      set
      {
        if( value == m_defaultCellRecyclingGroupDataType )
          return;

        Debug.Assert( value != null );

        m_defaultCellRecyclingGroupDataType = value;

        this.ResetDefaultCellRecyclingGroup();
      }
    }

    private Type m_defaultCellRecyclingGroupDataType = typeof( object );

    #endregion

    #region DefaultCellRecyclingGroup Private Property

    private object DefaultCellRecyclingGroup
    {
      get
      {
        if( m_defaultCellRecyclingGroup == null )
        {
          m_defaultCellRecyclingGroup = this.CreateDefaultCellRecyclingGroup();
          if( m_defaultCellRecyclingGroup == null )
            throw new InvalidOperationException( "CreateDefaultCellRecyclingGroup must return a non-null value." );
        }

        return m_defaultCellRecyclingGroup;
      }
    }

    internal virtual object CreateDefaultCellRecyclingGroup()
    {
      return new CellRecyclingGroupKey( this.CellContentTemplate, this.DefaultCellRecyclingGroupDataType );
    }

    internal void ResetDefaultCellRecyclingGroup()
    {
      m_defaultCellRecyclingGroup = null;
    }

    private object m_defaultCellRecyclingGroup;

    #endregion

    #region CurrentRowInEditionCellState Property

    internal CellState CurrentRowInEditionCellState
    {
      get;
      set;
    }

    #endregion

    #region DisplayedValueConverter Property

    public IValueConverter DisplayedValueConverter
    {
      get;
      set;
    }

    #endregion

    #region DisplayedValueConverterParameter Property

    public object DisplayedValueConverterParameter
    {
      get;
      set;
    }

    #endregion

    #region DisplayedValueConverterCulture Property

    public CultureInfo DisplayedValueConverterCulture
    {
      get;
      set;
    }

    #endregion

    #region DefaultCulture Property

    public static readonly DependencyProperty DefaultCultureProperty = DependencyProperty.Register(
      "DefaultCulture",
      typeof( CultureInfo ),
      typeof( ColumnBase ),
      new FrameworkPropertyMetadata( CultureInfo.CurrentCulture ) );

    public CultureInfo DefaultCulture
    {
      get
      {
        return ( CultureInfo )this.GetValue( ColumnBase.DefaultCultureProperty );
      }
      set
      {
        this.SetValue( ColumnBase.DefaultCultureProperty, value );
      }
    }

    #endregion

    #region DraggableStatus Property

    public static readonly DependencyProperty DraggableStatusProperty = DependencyProperty.Register(
      "DraggableStatus",
      typeof( ColumnDraggableStatus ),
      typeof( ColumnBase ),
      new UIPropertyMetadata( ColumnDraggableStatus.Draggable, null, new CoerceValueCallback( ColumnBase.CoerceDraggableStatus ) ) );

    public ColumnDraggableStatus DraggableStatus
    {
      get
      {
        return ( ColumnDraggableStatus )this.GetValue( ColumnBase.DraggableStatusProperty );
      }
      set
      {
        this.SetValue( ColumnBase.DraggableStatusProperty, value );
      }
    }

    internal void RefreshDraggableStatus()
    {
      this.CoerceValue( ColumnBase.DraggableStatusProperty );
    }

    private static object CoerceDraggableStatus( DependencyObject sender, object value )
    {
      var self = sender as ColumnBase;
      var collection = ( self != null ) ? self.ContainingCollection : null;
      if( collection == null )
        return value;

      var dataGridControl = collection.DataGridControl;
      if( ( dataGridControl == null ) || !dataGridControl.AreDetailsFlatten || !self.IsMainColumn )
        return value;

      // The main column is always locked in first position when details are flatten.
      return ColumnDraggableStatus.FirstUndraggable;
    }

    #endregion

    #region RealizedContainersRequested Event

    internal event RealizedContainersRequestedEventHandler RealizedContainersRequested;

    #endregion

    #region ActualWidthChanged Event

    internal event ColumnActualWidthChangedEventHandler ActualWidthChanged;

    #endregion

    #region DistinctValuesRequested Event

    internal event DistinctValuesRequestedEventHandler DistinctValuesRequested;

    #endregion

    #region FittedWidthRequested Event

    internal event FittedWidthRequestedEventHandler FittedWidthRequested;

    private Nullable<double> RequestFittedWidth()
    {
      var handler = this.FittedWidthRequested;
      if( handler == null )
        return null;

      var e = new FittedWidthRequestedEventArgs();
      handler.Invoke( this, e );

      return e.Value;
    }

    #endregion

#if DEBUG
    public override string ToString()
    {
      string toString = base.ToString();

      if( !string.IsNullOrEmpty( this.FieldName ) )
      {
        toString += " " + this.FieldName;
      }

      return toString;
    }
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
    public virtual double GetFittedWidth()
    {
      var fittedWidth = this.RequestFittedWidth().GetValueOrDefault( -1d );
      var e = new RealizedContainersRequestedEventArgs();

      var handler = this.RealizedContainersRequested;
      if( handler != null )
      {
        handler.Invoke( this, e );
      }

      var realizedContainers = e.RealizedContainers;
      if( realizedContainers.Count > 0 )
      {
        fittedWidth = Math.Max( fittedWidth, this.GetElementCollectionFittedWidth( realizedContainers ) );
      }

      return fittedWidth;
    }

    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );
      this.OnPropertyChanged( new PropertyChangedEventArgs( e.Property.Name ) );
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

    internal virtual void SetIsBeingDraggedAnimated( bool value )
    {
      TableflowView.SetIsBeingDraggedAnimated( this, value );
    }

    internal virtual void SetColumnReorderingDragSourceManager( ColumnReorderingDragSourceManager value )
    {
      TableflowView.SetColumnReorderingDragSourceManager( this, value );
    }

    internal virtual void ClearColumnReorderingDragSourceManager()
    {
      TableflowView.ClearColumnReorderingDragSourceManager( this );
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

    internal void DetachFromContainingCollection()
    {
      if( m_containingCollection == null )
        return;

      this.DetachFromContainingCollectionCore();
    }

    internal void OnDistinctValuesRequested( object sender, DistinctValuesRequestedEventArgs e )
    {
      var handler = this.DistinctValuesRequested;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    internal CultureInfo GetCulture( CultureInfo value )
    {
      if( value != null )
        return value;

      return this.GetCulture();
    }

    internal CultureInfo GetCulture()
    {
      var culture = this.DefaultCulture;
      if( culture != null )
        return culture;

      return CultureInfo.CurrentCulture;
    }

    private void AttachToContainingCollectionCore( ColumnCollection columnCollection )
    {
      m_containingCollection = columnCollection;
      m_isMainColumn = null;

      this.NotifyDataGridControlChanged();
    }

    private void DetachFromContainingCollectionCore()
    {
      m_containingCollection = null;

      this.NotifyDataGridControlChanged();
    }

    private double GetElementCollectionFittedWidth( IEnumerable collection )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );

      // Ensure to use the FieldName of the Cell instead of the visible position since a lookup dictionary is used underneath
      // and if the Indexer is used, the Cells are returned in the order they were added, not visible position.
      double fittedWidth = -1;

      foreach( object item in collection )
      {
        var row = item as Row;
        if( row == null )
        {
          var headerFooter = item as HeaderFooterItem;
          if( headerFooter != null )
          {
            row = headerFooter.AsVisual() as Row;
          }
        }

        double cellFittedWidth;
        if( !this.TryGetCellFittedWidth( row, out cellFittedWidth ) )
          continue;

        fittedWidth = Math.Max( fittedWidth, cellFittedWidth );
      }

      return fittedWidth;
    }

    private bool TryGetCellFittedWidth( Row row, out double fittedWidth )
    {
      fittedWidth = -1d;

      if( row == null )
        return false;

      Cell cell;
      var cells = row.Cells;
      var virtualCells = cells as VirtualizingCellCollection;
      bool releaseCell = false;

      if( virtualCells != null )
      {
        if( !virtualCells.TryGetBindedCell( this, out cell ) )
        {
          releaseCell = true;
          cell = virtualCells[ this ];

          if( cell == null )
            return false;

          // A created cell doesn't have a template applied yet.
          // The call to Cell.GetFittedWidth will fail unless we force the creation of the child element by measuring the cell.
          if( !cell.IsMeasureValid )
          {
            cell.Measure( Size.Empty );
          }
        }
      }
      else
      {
        cell = cells[ this ];
      }

      Debug.Assert( cell != null );
      fittedWidth = cell.GetFittedWidth();

      // A cell that was created or recycled to calculate the fitted width must be released in order to minimize the number of cells created overall.
      if( releaseCell )
      {
        Debug.Assert( virtualCells != null );
        virtualCells.Release( cell );
      }

      return true;
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      Debug.Assert( e != null );
      handler.Invoke( this, e );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return false;
    }

    #endregion

    #region CellRecyclingGroupKey Private Class

    private sealed class CellRecyclingGroupKey
    {
      internal CellRecyclingGroupKey( DataTemplate contentTemplate, Type dataType )
      {
        //This is a value that is cached at the cell level and that is costly to update, thus basing the recycling on it optimizes performance.
        m_contentTemplate = contentTemplate;
        m_dataType = dataType;
      }

      public override int GetHashCode()
      {
        if( m_contentTemplate != null )
          return m_contentTemplate.GetHashCode();

        if( m_dataType != null )
          return m_dataType.GetHashCode();

        return 0;
      }

      public override bool Equals( object obj )
      {
        var key = obj as CellRecyclingGroupKey;
        if( key == null )
          return false;

        if( key == this )
          return true;

        if( ( key.m_contentTemplate == null ) && ( m_contentTemplate == null ) )
          return ( key.m_dataType == m_dataType );

        return ( key.m_contentTemplate == m_contentTemplate );
      }

      private readonly DataTemplate m_contentTemplate;
      private readonly Type m_dataType;
    }

    #endregion
  }
}
