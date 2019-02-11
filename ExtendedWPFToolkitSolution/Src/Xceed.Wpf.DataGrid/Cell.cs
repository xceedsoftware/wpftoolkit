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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Utils.Wpf;
using Xceed.Wpf.DataGrid.Utils;
using Xceed.Wpf.DataGrid.ValidationRules;
using Xceed.Wpf.DataGrid.Views;
using Xceed.Wpf.Toolkit.Core;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_CellContentPresenter", Type = typeof( ContentPresenter ) )]
  public class Cell : ContentControl, INotifyPropertyChanged, IWeakEventListener
  {
    internal static readonly string FieldNamePropertyName = PropertyHelper.GetPropertyName( ( Cell c ) => c.FieldName );

    // This validation rule is meant to be used as the rule in error set in a Cell's ValidationError when the CellEditor's HasError attached property returns true.
    private static readonly CellEditorErrorValidationRule CellEditorErrorValidationRule = new CellEditorErrorValidationRule();

    // This validation rule is meant to be used as the rule in error set in a Cell's ValidationError when
    // we want to flag a validation error even though no binding's ValidationRules or CellValidationRules are invalid.
    // ie: Cell EditEnding event throws or returns with e.Cancel set to True.
    private static readonly PassthroughCellValidationRule CustomCellValidationExceptionValidationRule = new PassthroughCellValidationRule( new ExceptionValidationRule() );

    static Cell()
    {
      Cell.ContentProperty.OverrideMetadata( typeof( Cell ),
          new FrameworkPropertyMetadata( new PropertyChangedCallback( OnContentChanged ), new CoerceValueCallback( OnCoerceContent ) ) );

      //This configures the TAB key to go trough the controls of the cell once... then continue with other controls
      KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( Cell ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );

      //This configures the directional keys to go trough the controls of the cell once... then continue with other controls
      KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata( typeof( Cell ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );

      ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata( typeof( Cell ), new FrameworkPropertyMetadata( HorizontalAlignment.Stretch ) );

      ContentControl.VerticalContentAlignmentProperty.OverrideMetadata( typeof( Cell ), new FrameworkPropertyMetadata( VerticalAlignment.Stretch ) );

      Cell.ContentTemplateProperty.OverrideMetadata( typeof( Cell ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnContentTemplateChanged ) ) );

      Cell.ContentTemplateSelectorProperty.OverrideMetadata( typeof( Cell ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnContentTemplateSelectorChanged ) ) );

      Cell.IsCurrentProperty = Cell.IsCurrentPropertyKey.DependencyProperty;
      Cell.IsSelectedProperty = Cell.IsSelectedPropertyKey.DependencyProperty;
      Cell.ParentCellProperty = Cell.ParentCellPropertyKey.DependencyProperty;
      Cell.ParentColumnProperty = Cell.ParentColumnPropertyKey.DependencyProperty;
      Cell.ParentRowProperty = Cell.ParentRowPropertyKey.DependencyProperty;
      Cell.CoercedContentTemplateProperty = Cell.CoercedContentTemplatePropertyKey.DependencyProperty;

      //Editing stuff
      DataGridControl.CellEditorDisplayConditionsProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( OnCellEditorDisplayConditionsChanged ) ) );

      Cell.IsBeingEditedProperty = Cell.IsBeingEditedPropertyKey.DependencyProperty;
      Cell.IsDirtyProperty = Cell.IsDirtyPropertyKey.DependencyProperty;

      // We do this last to ensure all the Static content of the cell is done before using 
      // any static read-only field of the row.
      Cell.RowDisplayEditorMatchingConditionsProperty =
        Row.RowDisplayEditorMatchingConditionsProperty.AddOwner( typeof( Cell ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( OnMatchingDisplayEditorChanged ) ) );

      Cell.CellEditorContextProperty = Cell.CellEditorContextPropertyKey.DependencyProperty;

      // Animated Column reordering Binding
      Cell.ParentColumnTranslationBinding = new Binding();
      Cell.ParentColumnTranslationBinding.Path = new PropertyPath( "ParentColumn.(0)", ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty );
      Cell.ParentColumnTranslationBinding.Mode = BindingMode.OneWay;
      Cell.ParentColumnTranslationBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      Cell.ParentColumnIsBeingDraggedBinding = new Binding();
      Cell.ParentColumnIsBeingDraggedBinding.Path = new PropertyPath( "ParentColumn.(0)", TableflowView.IsBeingDraggedAnimatedProperty );
      Cell.ParentColumnIsBeingDraggedBinding.Mode = BindingMode.OneWay;
      Cell.ParentColumnIsBeingDraggedBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      Cell.ParentColumnReorderingDragSourceManagerBinding = new Binding();
      Cell.ParentColumnReorderingDragSourceManagerBinding.Path = new PropertyPath( "ParentColumn.(0)", TableflowView.ColumnReorderingDragSourceManagerProperty );
      Cell.ParentColumnReorderingDragSourceManagerBinding.Mode = BindingMode.OneWay;
      Cell.ParentColumnReorderingDragSourceManagerBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      Cell.ParentRowCellContentOpacityBinding = new Binding();
      Cell.ParentRowCellContentOpacityBinding.Path = new PropertyPath( "(0).(1)", Cell.ParentRowProperty, Row.CellContentOpacityProperty );
      Cell.ParentRowCellContentOpacityBinding.Mode = BindingMode.OneWay;
      Cell.ParentRowCellContentOpacityBinding.RelativeSource = new RelativeSource( RelativeSourceMode.TemplatedParent );

      Row.IsTemplateCellProperty.OverrideMetadata( typeof( Cell ), new FrameworkPropertyMetadata( new PropertyChangedCallback( Cell.OnIsTemplateCellChanged ) ) );

      EventManager.RegisterClassHandler( typeof( Cell ), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler( Cell.Cell_RequestBringIntoView ) );
    }

    public Cell()
    {
      this.SetParentCell();

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.BeginEdit,
                                                      new ExecutedRoutedEventHandler( OnBeginEditExecuted ),
                                                      new CanExecuteRoutedEventHandler( OnBeginEditCanExecute ) ) );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.CancelEdit,
                                              new ExecutedRoutedEventHandler( OnCancelEditExecuted ),
                                              new CanExecuteRoutedEventHandler( OnCancelEditCanExecute ) ) );

      // Remove the error template by default.
      this.SetValue( Validation.ErrorTemplateProperty, null );

      Validation.AddErrorHandler( this, Cell.OnContentBindingNotifyValidationError );
      this.TargetUpdated += new EventHandler<DataTransferEventArgs>( this.OnContentBindingTargetUpdated );
      this.LayoutUpdated += new EventHandler( this.Cell_LayoutUpdated );
    }

    #region IsCellFocusScope Attached Property


    public static readonly DependencyProperty IsCellFocusScopeProperty =
        DependencyProperty.RegisterAttached( "IsCellFocusScope", typeof( bool ), typeof( Cell ), new UIPropertyMetadata( false ) );


    public static bool GetIsCellFocusScope( DependencyObject obj )
    {
      return ( bool )obj.GetValue( Cell.IsCellFocusScopeProperty );
    }


    public static void SetIsCellFocusScope( DependencyObject obj, bool value )
    {
      obj.SetValue( Cell.IsCellFocusScopeProperty, value );
    }

    #endregion IsCellFocusScope Attached Property

    #region ReadOnly Property

    public static readonly DependencyProperty ReadOnlyProperty = DataGridControl.ReadOnlyProperty.AddOwner( typeof( Cell ) );

    public bool ReadOnly
    {
      get
      {
        return ( bool )this.GetValue( Cell.ReadOnlyProperty );
      }
      set
      {
        this.SetValue( Cell.ReadOnlyProperty, value );
      }
    }

    #endregion ReadOnly Property

    #region IsCurrent Read-Only Property

    private static readonly DependencyPropertyKey IsCurrentPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsCurrent", typeof( bool ), typeof( Cell ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty IsCurrentProperty;

    public bool IsCurrent
    {
      get
      {
        return ( bool )this.GetValue( Cell.IsCurrentProperty );
      }
    }

    internal void SetIsCurrent( bool value )
    {
      if( value )
      {
        this.SetValue( Cell.IsCurrentPropertyKey, true );
      }
      else
      {
        this.SetValue( Cell.IsCurrentPropertyKey, DependencyProperty.UnsetValue );
      }
    }

    #endregion IsCurrent Read-Only Property

    #region IsSelected Read-Only Property

    private static readonly DependencyPropertyKey IsSelectedPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsSelected", typeof( bool ), typeof( Cell ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty IsSelectedProperty;

    public bool IsSelected
    {
      get
      {
        return ( bool )this.GetValue( Cell.IsSelectedProperty );
      }
    }

    internal void SetIsSelected( bool value )
    {
      if( this.IsSelected != value )
      {
        if( value )
        {
          this.SetValue( Cell.IsSelectedPropertyKey, value );
        }
        else
        {
          this.ClearValue( Cell.IsSelectedPropertyKey );
        }
      }
    }

    #endregion IsSelected Read-Only Property

    #region IsBeingEdited Read-Only Property

    private static readonly DependencyPropertyKey IsBeingEditedPropertyKey = DependencyProperty.RegisterReadOnly(
      "IsBeingEdited",
      typeof( bool ),
      typeof( Cell ),
      new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsBeingEditedProperty;

    internal static readonly RoutedEvent IsBeingEditedEvent = EventManager.RegisterRoutedEvent( "IsBeingEdited", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Cell ) );

    public bool IsBeingEdited
    {
      get
      {
        return this.IsBeingEditedCache;
      }
    }

    private void SetIsBeingEdited( bool value )
    {
      if( value != this.IsBeingEdited )
      {
        this.FocusEditor = false;
        this.IsBeingEditedCache = value;

        if( value )
        {
          this.SetValue( Cell.IsBeingEditedPropertyKey, true );
        }
        else
        {
          this.SetValue( Cell.IsBeingEditedPropertyKey, DependencyProperty.UnsetValue );
        }

        CellState cellState = this.GetEditCachedState();

        if( cellState != null )
        {
          cellState.SetIsBeingEdited( value );
        }

        if( this.ParentRow.IsClearingContainer )
          return;

        //If cell is being edited
        if( value )
        {
          //Raise the RoutedEvent that notifies any Row that the cell is being edited.
          RoutedEventArgs eventArgs = new RoutedEventArgs( Cell.IsBeingEditedEvent );
          this.RaiseEvent( eventArgs );
          this.FocusEditor = true;
        }

        this.RefreshDisplayedTemplate();
      }
    }

    #endregion IsBeingEdited Read-Only Property

    #region IsDirty Read-Only Property

    private static readonly DependencyPropertyKey IsDirtyPropertyKey = DependencyProperty.RegisterReadOnly(
      "IsDirty",
      typeof( bool ),
      typeof( Cell ),
      new PropertyMetadata( false, new PropertyChangedCallback( Cell.OnIsDirtyChanged ) ) );

    public static readonly DependencyProperty IsDirtyProperty;

    internal static readonly RoutedEvent IsDirtyEvent = EventManager.RegisterRoutedEvent( "IsDirty", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Cell ) );

    public bool IsDirty
    {
      get
      {
        return ( bool )this.GetValue( Cell.IsDirtyProperty );
      }
    }

    internal void SetIsDirty( bool value )
    {
      if( value != this.IsDirty )
      {
        if( value )
        {
          this.SetValue( Cell.IsDirtyPropertyKey, value );
        }
        else
        {
          this.SetValue( Cell.IsDirtyPropertyKey, DependencyProperty.UnsetValue );
        }

        CellState cellState = this.GetEditCachedState();

        if( cellState != null )
        {
          cellState.SetIsDirty( value );
        }
      }
    }

    #endregion IsDirty Read-Only Property

    #region IsCellEditorDisplayed Read-Only Property

    public bool IsCellEditorDisplayed
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsCellEditorDisplayed ];
      }
    }

    private void SetIsCellEditorDisplayed( bool value )
    {
      if( value != this.IsCellEditorDisplayed )
      {
        m_flags[ ( int )CellFlags.IsCellEditorDisplayed ] = value;
        this.OnPropertyChanged( "IsCellEditorDisplayed" );
      }
    }

    #endregion IsCellEditorDisplayed Read-Only Property

    #region CurrentBackground Property

    public static readonly DependencyProperty CurrentBackgroundProperty = DependencyProperty.Register(
      "CurrentBackground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush CurrentBackground
    {
      get
      {
        return ( Brush )this.GetValue( Cell.CurrentBackgroundProperty );
      }
      set
      {
        this.SetValue( Cell.CurrentBackgroundProperty, value );
      }
    }

    #endregion CurrentBackground Property

    #region CurrentForeground Property

    public static readonly DependencyProperty CurrentForegroundProperty =
      DependencyProperty.Register( "CurrentForeground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush CurrentForeground
    {
      get
      {
        return ( Brush )this.GetValue( Cell.CurrentForegroundProperty );
      }
      set
      {
        this.SetValue( Cell.CurrentForegroundProperty, value );
      }
    }

    #endregion CurrentForeground Property

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

    public static readonly DependencyProperty CellErrorStyleProperty = DataGridControl.CellErrorStyleProperty.AddOwner( typeof( Cell ) );

    public Style CellErrorStyle
    {
      get
      {
        return ( Style )this.GetValue( Cell.CellErrorStyleProperty );
      }

      set
      {
        this.SetValue( Cell.CellErrorStyleProperty, value );
      }
    }

    #endregion CellErrorStyle Property

    #region HasValidationError Read-Only Property

    private static readonly DependencyPropertyKey HasValidationErrorPropertyKey = DependencyProperty.RegisterReadOnly(
      "HasValidationError",
      typeof( bool ),
      typeof( Cell ),
      new UIPropertyMetadata( false, new PropertyChangedCallback( Cell.OnHasValidationErrorChanged ) ) );

    public static readonly DependencyProperty HasValidationErrorProperty = HasValidationErrorPropertyKey.DependencyProperty;

    public bool HasValidationError
    {
      get
      {
        return ( bool )this.GetValue( Cell.HasValidationErrorProperty );
      }
    }

    private void SetHasValidationError( bool value )
    {
      if( value != this.HasValidationError )
      {
        if( value )
        {
          this.SetValue( Cell.HasValidationErrorPropertyKey, value );
        }
        else
        {
          this.SetValue( Cell.HasValidationErrorPropertyKey, DependencyProperty.UnsetValue );
        }
      }
    }

    private static void OnHasValidationErrorChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      Cell cell = ( Cell )sender;

      // If we are in the process of clearing the cell, don't refresh the error style.
      // It is imperative to take care of resetting all the flags used by the DelayedRefreshErrorStyle architecture in the Cell's ClearContainer method though.
      var parentRow = cell.ParentRow;
      if( ( parentRow != null ) && parentRow.IsClearingContainer )
        return;

      // This flag is used to optimize the synchronization of error flags on the parent row/column so that it is done only once at the end of the UpdateHasErrorFlags method.
      cell.HasPendingSyncParentErrorFlags = true;

      if( cell.HasPendingErrorStyleRefresh )
        return;

      cell.HasPendingErrorStyleRefresh = true;

      if( !cell.IsLoaded )
      {
        // If the cell isn't loaded, wait for the loaded event to dispatch the change of style.
        System.Diagnostics.Debug.Assert( cell.m_loadedRoutedEventHandler == null );

        cell.m_loadedRoutedEventHandler = new RoutedEventHandler( cell.Cell_Loaded );
        cell.Loaded += cell.m_loadedRoutedEventHandler;
        return;
      }

      cell.DelayRefreshErrorStyle();
    }

    private void DelayRefreshErrorStyle()
    {
      if( m_delayedRefreshErrorStyleDispatcherOperation != null )
        return;

      // Push on the dispatcher so that we prevent multiple affectation of Style when the binding is re-evaluated.
      // Re-evaluating a binding causes it to clear all errors, then validate and finally sets the errors.
      // By using the following strategy, we make sure of not affecting Style when it will change right afterward.
      m_delayedRefreshErrorStyleDispatcherOperation = this.Dispatcher.BeginInvoke( DispatcherPriority.Render, new Action( this.DelayedRefreshErrorStyle ) );
    }

    private void AbortRefreshErrorStyle()
    {
      if( m_delayedRefreshErrorStyleDispatcherOperation == null )
        return;

      Debug.Assert( m_delayedRefreshErrorStyleDispatcherOperation.Status == DispatcherOperationStatus.Pending );
      m_delayedRefreshErrorStyleDispatcherOperation.Abort();
      m_delayedRefreshErrorStyleDispatcherOperation = null;
    }

    private void DelayedRefreshErrorStyle()
    {
      m_delayedRefreshErrorStyleDispatcherOperation = null;
      System.Diagnostics.Debug.Assert( this.HasPendingErrorStyleRefresh, "The operation should have been aborted." );
      System.Diagnostics.Debug.Assert( ( ( this.ParentRow != null ) && !this.ParentRow.IsClearingContainer ), "The operation should have been aborted." );

      if( !this.IsLoaded )
      {
        // Such a sitation can occur when the cell is Loaded when OnHasErrorChanged is raised but unloaded without a ClearContainer afterward.
        // Therefore, we need to push back the refreshing of the style to the cell's Loaded event.
        System.Diagnostics.Debug.Assert( m_loadedRoutedEventHandler == null );

        m_loadedRoutedEventHandler = new RoutedEventHandler( this.Cell_Loaded );
        this.Loaded += m_loadedRoutedEventHandler;
        return;
      }

      try
      {
        bool hasValidationError = this.HasValidationError;
        bool shouldChangeStyle = ( this.IsErrorStyleApplied != hasValidationError );

        if( shouldChangeStyle )
        {
          if( hasValidationError )
          {
            // Cache the normal style.
            m_styleBeforeError = this.ReadLocalValue( Cell.StyleProperty );

            //Apply the style.
            Style errorStyle = this.GetInheritedErrorStyle();

            if( errorStyle != null )
            {
              this.Style = errorStyle;
              this.IsErrorStyleApplied = true;
            }
          }
          else
          {
            // Revert to the normal style.
            if( m_styleBeforeError == DependencyProperty.UnsetValue )
            {
              this.ClearValue( Cell.StyleProperty );
            }
            else
            {
              this.Style = m_styleBeforeError as Style;
            }

            this.IsErrorStyleApplied = false;
          }
        }
      }
      finally
      {
        this.HasPendingErrorStyleRefresh = false;
      }
    }

    #endregion HasValidationError Read-Only Property

    #region IsValidationErrorRestrictive Read-Only Property

    private static readonly DependencyPropertyKey IsValidationErrorRestrictivePropertyKey = DependencyProperty.RegisterReadOnly(
      "IsValidationErrorRestrictive",
      typeof( bool ),
      typeof( Cell ),
      new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsValidationErrorRestrictiveProperty = Cell.IsValidationErrorRestrictivePropertyKey.DependencyProperty;

    public bool IsValidationErrorRestrictive
    {
      get
      {
        return ( bool )this.GetValue( Cell.IsValidationErrorRestrictiveProperty );
      }
    }

    private void SetIsValidationErrorRestrictive( bool value )
    {
      if( value != this.IsValidationErrorRestrictive )
      {
        if( value )
        {
          this.SetValue( Cell.IsValidationErrorRestrictivePropertyKey, value );
        }
        else
        {
          this.SetValue( Cell.IsValidationErrorRestrictivePropertyKey, DependencyProperty.UnsetValue );
        }
      }
    }

    private static void OnIsValidationErrorRestrictiveChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      Cell cell = ( Cell )sender;
      cell.HasPendingSyncParentErrorFlags = true;
    }

    #endregion IsValidationErrorRestrictive Read-Only Property

    #region ValidationError Read-Only Property

    public static readonly RoutedEvent ValidationErrorChangingEvent =
                  EventManager.RegisterRoutedEvent( "ValidationErrorChanging", RoutingStrategy.Bubble, typeof( CellValidationErrorRoutedEventHandler ), typeof( Cell ) );

    public event CellValidationErrorRoutedEventHandler ValidationErrorChanging
    {
      add
      {
        base.AddHandler( Cell.ValidationErrorChangingEvent, value );
      }
      remove
      {
        base.RemoveHandler( Cell.ValidationErrorChangingEvent, value );
      }
    }

    protected virtual void OnValidationErrorChanging( CellValidationErrorRoutedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    private static readonly DependencyPropertyKey ValidationErrorPropertyKey = DependencyProperty.RegisterReadOnly(
      "ValidationError",
      typeof( CellValidationError ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null, new PropertyChangedCallback( Cell.OnValidationErrorChanged ), new CoerceValueCallback( Cell.OnCoerceValidationError ) ) );

    public static readonly DependencyProperty ValidationErrorProperty = ValidationErrorPropertyKey.DependencyProperty;

    public CellValidationError ValidationError
    {
      get
      {
        return ( CellValidationError )this.GetValue( Cell.ValidationErrorProperty );
      }
    }

    private static object OnCoerceValidationError( DependencyObject sender, object value )
    {
      if( value == null )
        return value;

      Cell cell = ( Cell )sender;

      CellValidationErrorRoutedEventArgs cellValidationErrorRoutedEventArgs =
        new CellValidationErrorRoutedEventArgs( Cell.ValidationErrorChangingEvent, cell, ( CellValidationError )value );

      cell.OnValidationErrorChanging( cellValidationErrorRoutedEventArgs );

      return cellValidationErrorRoutedEventArgs.CellValidationError;
    }

    private static void OnValidationErrorChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      Cell cell = ( Cell )sender;

      CellValidationError newCellValidationError = e.NewValue as CellValidationError;

      // Update the flags telling us wether or not the current validation error is CellEditor error, or a UI error and update the HasValidationError dependency property.
      cell.UpdateHasErrorFlags( newCellValidationError );

      // Refresh which content template is displayed (editTemplate or not) in case there was a CellEditor validation error on the cell and now there isn't one anymore.
      cell.RefreshDisplayedTemplate();
    }

    private void UpdateHasErrorFlags( CellValidationError cellValidationError )
    {
      bool hasValidationError = ( cellValidationError != null );

      try
      {
        if( hasValidationError )
        {
          this.HasCellEditorError = Cell.GetIsCellEditorError( cellValidationError );
          this.HasUIValidationError = Cell.GetIsUIValidationError( cellValidationError );
          this.HasContentBindingValidationError = Cell.GetIsContentBindingValidationError( cellValidationError );

          this.SetIsValidationErrorRestrictive( Cell.GetIsValidationErrorRestrictive( cellValidationError ) );

          this.SetHasValidationError( true );
        }
        else
        {
          this.HasCellEditorError = false;
          this.HasUIValidationError = false;
          this.HasContentBindingValidationError = false;

          this.SetIsValidationErrorRestrictive( false );

          this.SetHasValidationError( false );
        }

        // This flag is raised in the PropertyChanged callbacks of the Has_X_Error properties in order to
        // minimize the calls to the cell's parent row's UpdateHasErrorFlags method.
        if( this.HasPendingSyncParentErrorFlags )
        {
          ColumnBase column = this.ParentColumn;

          if( column != null )
          {
            column.SetHasValidationError( hasValidationError );
          }

          Row row = this.ParentRow;

          if( row != null )
          {
            row.UpdateHasErrorFlags( this );
          }
        }
      }
      finally
      {
        this.HasPendingSyncParentErrorFlags = false;
      }
    }

    internal void SetValidationError( CellValidationError value )
    {
      if( value != this.ValidationError )
      {
        if( value != null )
        {
          this.SetValue( Cell.ValidationErrorPropertyKey, value );
        }
        else
        {
          this.SetValue( Cell.ValidationErrorPropertyKey, DependencyProperty.UnsetValue );
        }

        CellState cellState = this.GetEditCachedState();

        if( cellState != null )
        {
          cellState.SetCellValidationError( value );
        }
      }
    }

    #endregion ValidationError Read-Only Property

    #region RowDisplayEditorMatchingConditions Property

    private static readonly DependencyProperty RowDisplayEditorMatchingConditionsProperty;

    #endregion RowDisplayEditorMatchingConditions Property

    #region CellDisplayEditorMatchingConditions Property

    private static readonly DependencyProperty CellDisplayEditorMatchingConditionsProperty = DependencyProperty.Register(
      "CellDisplayEditorMatchingConditions",
      typeof( CellEditorDisplayConditions ),
      typeof( Cell ),
      new UIPropertyMetadata( CellEditorDisplayConditions.None, new PropertyChangedCallback( OnMatchingDisplayEditorChanged ) ) );

    internal void SetDisplayEditorMatchingCondition( CellEditorDisplayConditions condition )
    {
      CellEditorDisplayConditions previousValue = ( CellEditorDisplayConditions )this.GetValue( Cell.CellDisplayEditorMatchingConditionsProperty );

      previousValue = previousValue | condition;

      this.SetValue( Cell.CellDisplayEditorMatchingConditionsProperty, previousValue );
    }

    internal void RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions condition )
    {
      CellEditorDisplayConditions previousValue = ( CellEditorDisplayConditions )this.GetValue( Cell.CellDisplayEditorMatchingConditionsProperty );

      previousValue = previousValue & ~condition;

      this.SetValue( Cell.CellDisplayEditorMatchingConditionsProperty, previousValue );
    }

    #endregion CellDisplayEditorMatchingConditions Property

    #region ShouldDisplayEditor Property

    private bool ShouldDisplayEditor
    {
      get
      {
        if( this.GetRealDataContext() is EmptyDataItem )
          return false;

        // If the cell is considered read-only, the EditTemplate should never be displayed.
        if( this.GetInheritedReadOnly() )
          return false;

        // If the cell currently has a restrictive error, the EditTemplate should always be displayed.
        if( Cell.GetIsValidationErrorRestrictive( this.ValidationError ) )
          return true;

        CellEditor cellEditor = this.GetCellEditor();

        // If no cell editor is retrieved, we don't even have an edit template to display.
        if( cellEditor == null )
          return false;

        //Retrieve the matching display conditions from both the Row and the Cell
        CellEditorDisplayConditions rowValue = ( CellEditorDisplayConditions )this.GetValue( Cell.RowDisplayEditorMatchingConditionsProperty );
        CellEditorDisplayConditions cellValue = ( CellEditorDisplayConditions )this.GetValue( Cell.CellDisplayEditorMatchingConditionsProperty );

        //If there is at least one "matching conditions"
        if( ( rowValue | cellValue ) != CellEditorDisplayConditions.None )
          return true;

        CellEditorDisplayConditions cellEditorDisplayConditions = this.CellEditorDisplayConditions;

        // We always want to display the editor if it is set on the ParentColumn.CellEditorDisplayConditions are Always
        if( ( cellEditorDisplayConditions & CellEditorDisplayConditions.Always ) == CellEditorDisplayConditions.Always )
          return true;

        Row parentRow = this.ParentRow;

        if( parentRow == null )
          return false;

        // this.CellEditorDisplayConditions will get the value set on ParentColumn if any in case it was explicitly set
        if( parentRow.IsMouseOver && ( ( cellEditorDisplayConditions & CellEditorDisplayConditions.MouseOverRow ) == CellEditorDisplayConditions.MouseOverRow ) )
          return true;

        if( parentRow.IsBeingEdited && ( ( cellEditorDisplayConditions & CellEditorDisplayConditions.RowIsBeingEdited ) == CellEditorDisplayConditions.RowIsBeingEdited ) )
          return true;

        if( parentRow.IsCurrent && ( ( cellEditorDisplayConditions & CellEditorDisplayConditions.RowIsCurrent ) == CellEditorDisplayConditions.RowIsCurrent ) )
          return true;

        // If the cell is being edited, the edit template should be displayed.
        return this.IsBeingEdited;
      }
    }

    #endregion

    #region CellEditorDisplayConditions Property

    internal CellEditorDisplayConditions CellEditorDisplayConditions
    {
      get
      {

        //Read the Value of the Parent column
        object propertyValue = DependencyProperty.UnsetValue;
        ColumnBase parentColumn = this.ParentColumn;

        if( parentColumn != null )
        {
          propertyValue = parentColumn.ReadLocalValue( DataGridControl.CellEditorDisplayConditionsProperty );

          // In case we have a BindingExpression, we need to get the resulting value.
          if( propertyValue is BindingExpressionBase )
            propertyValue = parentColumn.CellEditorDisplayConditions;
        }

        //If the ParentColumn has no value defined for the CellEditorDisplayConditions DP,
        if( propertyValue == DependencyProperty.UnsetValue )
        {
          //Then interrogate the ParentRow
          propertyValue = this.GetValue( DataGridControl.CellEditorDisplayConditionsProperty );
        }

        return ( CellEditorDisplayConditions )propertyValue;
      }
    }

    #endregion

    #region ParentCell Property

    private static readonly DependencyPropertyKey ParentCellPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "ParentCell",
      typeof( Cell ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty ParentCellProperty;

    private void SetParentCell()
    {
      this.SetValue( Cell.ParentCellPropertyKey, this );
    }

    #endregion ParentCell Property

    #region ParentColumn Read-Only Property

    private static readonly DependencyPropertyKey ParentColumnPropertyKey = DependencyProperty.RegisterReadOnly(
      "ParentColumn",
      typeof( ColumnBase ),
      typeof( Cell ),
      new PropertyMetadata( null, new PropertyChangedCallback( Cell.OnParentColumnChanged ) ) );

    public static readonly DependencyProperty ParentColumnProperty;

    public ColumnBase ParentColumn
    {
      get
      {
        return ( ColumnBase )this.GetValue( Cell.ParentColumnProperty );
      }
      private set
      {
        this.SetValue( Cell.ParentColumnPropertyKey, value );
      }
    }

    internal virtual void OnParentColumnChanged( ColumnBase oldColumn, ColumnBase newColumn )
    {
      if( oldColumn != null )
      {
        PropertyChangedEventManager.RemoveListener( oldColumn, this, string.Empty );
      }

      if( newColumn != null )
      {
        PropertyChangedEventManager.AddListener( newColumn, this, string.Empty );
      }

      this.UpdateFocusable();
    }

    private static void OnParentColumnChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as Cell;
      if( self == null )
        return;

      self.OnParentColumnChanged( ( ColumnBase )e.OldValue, ( ColumnBase )e.NewValue );

      // Since Cell.FieldName in fact returns Cell.ParentColumn.FieldName, a PropertyChanged corresponding to FieldName must be raised when the ParentColumn has changed.
      self.OnPropertyChanged( Cell.FieldNamePropertyName );
    }

    #endregion

    #region ParentRow Read-Only Property

    private static readonly DependencyPropertyKey ParentRowPropertyKey = DependencyProperty.RegisterReadOnly(
      "ParentRow",
      typeof( Row ),
      typeof( Cell ),
      new PropertyMetadata( null ) );

    public static readonly DependencyProperty ParentRowProperty;

    public Row ParentRow
    {
      get
      {
        return ( Row )this.GetValue( Cell.ParentRowProperty );
      }
      private set
      {
        this.SetValue( Cell.ParentRowPropertyKey, value );
      }
    }

    #endregion

    #region FieldName Property

    public string FieldName
    {
      get
      {
        ColumnBase parentColumn = this.ParentColumn;

        if( parentColumn != null )
          return parentColumn.FieldName;

        return m_fieldName;
      }
      set
      {
        ColumnBase parentColumn = this.ParentColumn;

        if( parentColumn != null )
          throw new InvalidOperationException( "An attempt was made to change the FieldName of a cell already associated with a column." );

        m_fieldName = value;
        this.OnPropertyChanged( Cell.FieldNamePropertyName );
      }
    }

    private void SetFieldName( string fieldName )
    {
      m_fieldName = fieldName;
    }

    #endregion FieldName Property

    #region CoercedContentTemplate Property

    private static readonly DependencyPropertyKey CoercedContentTemplatePropertyKey = DependencyProperty.RegisterReadOnly(
      "CoercedContentTemplate",
      typeof( DataTemplate ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public static readonly DependencyProperty CoercedContentTemplateProperty;

    public DataTemplate CoercedContentTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( Cell.CoercedContentTemplateProperty );
      }
    }

    private void SetCoercedContentTemplate( DataTemplate viewerTemplate, DataTemplate editorTemplate )
    {
      var contentTemplate = this.CoercedContentTemplate;
      var editorDataTemplate = contentTemplate as EditorDataTemplate;

      if( editorDataTemplate != null )
      {
        if( ( editorDataTemplate.Viewer == viewerTemplate ) && ( editorDataTemplate.Editor == editorTemplate ) )
          return;
      }
      else if( editorTemplate == null )
      {
        if( contentTemplate == viewerTemplate )
          return;
      }

      var newContentTemplate = ( editorTemplate != null )
                                 ? EditorDataTemplate.Create( viewerTemplate, editorTemplate )
                                 : viewerTemplate;

      this.SetValue( Cell.CoercedContentTemplatePropertyKey, newContentTemplate );
    }

    private void UpdateCoercedContentTemplate( bool force )
    {
      if( force || this.IsContainerPrepared )
      {
        this.SetCoercedContentTemplate( this.GetCoercedCellContentTemplate(), null );
      }
    }

    #endregion

    #region ContentTemplateInternal Property (private)

    private DataTemplate ContentTemplateInternal
    {
      get
      {
        if( m_contentTemplateCache == Cell.NotInitializedDataTemplate )
        {
          m_contentTemplateCache = this.GetValue( Cell.ContentTemplateProperty ) as DataTemplate;
        }
        return m_contentTemplateCache;
      }
    }

    private static readonly DataTemplate NotInitializedDataTemplate = new DataTemplate();

    private DataTemplate m_contentTemplateCache = Cell.NotInitializedDataTemplate;

    #endregion

    #region ContentTemplateSelectorInternal Property (private)

    private DataTemplateSelector ContentTemplateSelectorInternal
    {
      get
      {
        if( m_contentTemplateSelectorCache == NotInitializedSelector )
        {
          m_contentTemplateSelectorCache = this.GetValue( Cell.ContentTemplateSelectorProperty ) as DataTemplateSelector;
        }
        return m_contentTemplateSelectorCache;
      }
    }

    private static readonly DataTemplateSelector NotInitializedSelector = new DataTemplateSelector();

    private DataTemplateSelector m_contentTemplateSelectorCache = Cell.NotInitializedSelector;

    #endregion

    #region OverrideColumnCellContentTemplate Property (protected virtual)

    protected virtual bool OverrideColumnCellContentTemplate
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region CellEditorContext Attached Property

    private static readonly DependencyPropertyKey CellEditorContextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "CellEditorContext", typeof( CellEditorContext ), typeof( Cell ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty CellEditorContextProperty;

    public static CellEditorContext GetCellEditorContext( DependencyObject obj )
    {
      return ( CellEditorContext )obj.GetValue( Cell.CellEditorContextProperty );
    }

    private static void SetCellEditorContext( DependencyObject obj, CellEditorContext value )
    {
      obj.SetValue( Cell.CellEditorContextPropertyKey, value );
    }

    internal static void ClearCellEditorContext( DependencyObject obj )
    {
      obj.ClearValue( Cell.CellEditorContextPropertyKey );
    }

    #endregion

    #region ParentColumnIsBeingDragged Private Property

    private static readonly DependencyProperty ParentColumnIsBeingDraggedProperty = DependencyProperty.Register(
      "ParentColumnIsBeingDragged",
      typeof( bool ),
      typeof( Cell ),
      new FrameworkPropertyMetadata(
        ( bool )false,
        new PropertyChangedCallback( Cell.OnParentColumnIsBeingDraggedChanged ),
        new CoerceValueCallback( Cell.OnCoerceParentColumnIsBeingDragged ) ) );

    private bool ParentColumnIsBeingDragged
    {
      get
      {
        return ( bool )this.GetValue( Cell.ParentColumnIsBeingDraggedProperty );
      }
      set
      {
        this.SetValue( Cell.ParentColumnIsBeingDraggedProperty, value );
      }
    }

    private static void OnParentColumnIsBeingDraggedChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = ( Cell )sender;
      Debug.Assert( self != null );

      var row = self.ParentRow;
      if( row == null )
        return;

      var fixedCellPanel = row.CellsHostPanel as FixedCellPanel;
      if( fixedCellPanel == null )
        return;

      if( ( bool )e.NewValue )
      {
        fixedCellPanel.ChangeCellZOrder( self, fixedCellPanel.Children.Count );
      }
      else
      {
        fixedCellPanel.ClearCellZOrder( self );
      }
    }

    private static object OnCoerceParentColumnIsBeingDragged( DependencyObject sender, object newValue )
    {
      var self = ( Cell )sender;
      Debug.Assert( self != null );

      var parentColumn = self.ParentColumn;
      if( parentColumn == null )
        return newValue;

      var manager = TableflowView.GetColumnReorderingDragSourceManager( parentColumn );
      if( manager == null )
        return newValue;

      if( ( bool )newValue )
      {
        self.AddDraggedColumnGhost( manager, true );
      }
      else
      {
        self.RemoveDraggedColumnGhost( manager );
      }

      return newValue;
    }

    #endregion

    #region ParentColumnReorderingDragSourceManager Property

    internal static readonly DependencyProperty ParentColumnReorderingDragSourceManagerProperty = DependencyProperty.Register(
      "ParentColumnReorderingDragSourceManager",
      typeof( ColumnReorderingDragSourceManager ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null, new PropertyChangedCallback( Cell.OnParentColumnReorderingDragSourceManagerChanged ) ) );

    internal ColumnReorderingDragSourceManager ParentColumnReorderingDragSourceManager
    {
      get
      {
        return ( ColumnReorderingDragSourceManager )this.GetValue( Cell.ParentColumnReorderingDragSourceManagerProperty );
      }
      set
      {
        this.SetValue( Cell.ParentColumnReorderingDragSourceManagerProperty, value );
      }
    }

    private static void OnParentColumnReorderingDragSourceManagerChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as Cell;
      Debug.Assert( self != null );

      var oldManager = ( ColumnReorderingDragSourceManager )e.OldValue;
      var newManager = ( ColumnReorderingDragSourceManager )e.NewValue;

      if( ( oldManager != null ) && ( newManager != null ) )
      {
        self.RemoveDraggedColumnGhost( oldManager );
        self.AddDraggedColumnGhost( newManager, self.ParentColumnIsBeingDragged );
      }
    }

    #endregion

    #region SelectionBackground Property

    public static readonly DependencyProperty SelectionBackgroundProperty = DependencyProperty.Register(
      "SelectionBackground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush SelectionBackground
    {
      get
      {
        return ( Brush )this.GetValue( Cell.SelectionBackgroundProperty );
      }
      set
      {
        this.SetValue( Cell.SelectionBackgroundProperty, value );
      }
    }

    #endregion SelectionBackground Property

    #region SelectionForeground Property

    public static readonly DependencyProperty SelectionForegroundProperty = DependencyProperty.Register(
      "SelectionForeground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush SelectionForeground
    {
      get
      {
        return ( Brush )this.GetValue( Cell.SelectionForegroundProperty );
      }
      set
      {
        this.SetValue( Cell.SelectionForegroundProperty, value );
      }
    }

    #endregion SelectionForeground Property

    #region InactiveSelectionBackground Property

    public static readonly DependencyProperty InactiveSelectionBackgroundProperty = DependencyProperty.Register(
      "InactiveSelectionBackground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush InactiveSelectionBackground
    {
      get
      {
        return ( Brush )this.GetValue( Cell.InactiveSelectionBackgroundProperty );
      }
      set
      {
        this.SetValue( Cell.InactiveSelectionBackgroundProperty, value );
      }
    }

    #endregion InactiveSelectionBackground Property

    #region InactiveSelectionForeground Property

    public static readonly DependencyProperty InactiveSelectionForegroundProperty = DependencyProperty.Register(
      "InactiveSelectionForeground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush InactiveSelectionForeground
    {
      get
      {
        return ( Brush )this.GetValue( Cell.InactiveSelectionForegroundProperty );
      }
      set
      {
        this.SetValue( Cell.InactiveSelectionForegroundProperty, value );
      }
    }

    #endregion InactiveSelectionForeground Property

    #region ParentForeground Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty ParentForegroundProperty = DependencyProperty.Register(
      "ParentForeground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( TextElement.ForegroundProperty.DefaultMetadata.DefaultValue ) );

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public Brush ParentForeground
    {
      get
      {
        return ( Brush )this.GetValue( Cell.ParentForegroundProperty );
      }
      set
      {
        this.SetValue( Cell.ParentForegroundProperty, value );
      }
    }

    private void UpdateParentForeground()
    {
      // Use the visual parent when there is no logical parent.
      var parent = LogicalTreeHelper.GetParent( this ) ?? VisualTreeHelper.GetParent( this );

      if( parent != null )
      {
        var binding = new Binding();
        binding.Path = new PropertyPath( TextElement.ForegroundProperty );
        binding.Source = parent;

        this.SetBinding( Cell.ParentForegroundProperty, binding );
      }
      else
      {
        this.ClearValue( Cell.ParentForegroundProperty );
      }
    }

    #endregion ParentForeground Property

    #region IsInternalyInitialized Property

    internal bool IsInternalyInitialized
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsInternalyInitialized ];
      }
      private set
      {
        m_flags[ ( int )CellFlags.IsInternalyInitialized ] = value;
      }
    }

    #endregion

    #region IsContainerPrepared Property

    internal bool IsContainerPrepared
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsContainerPrepared ];
      }
      private set
      {
        m_flags[ ( int )CellFlags.IsContainerPrepared ] = value;
      }
    }

    #endregion

    #region IsContainerVirtualized Property

    internal bool IsContainerVirtualized
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsContainerVirtualized ];
      }
      private set
      {
        m_flags[ ( int )CellFlags.IsContainerVirtualized ] = value;
      }
    }

    #endregion

    #region IsContainerRecycled Property

    internal bool IsContainerRecycled
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsContainerRecycled ];
      }
      private set
      {
        m_flags[ ( int )CellFlags.IsContainerRecycled ] = value;
      }
    }

    #endregion

    #region IsContainerPartiallyCleared Property

    internal bool IsContainerPartiallyCleared
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsContainerPartiallyCleared ];
      }
      private set
      {
        m_flags[ ( int )CellFlags.IsContainerPartiallyCleared ] = value;
      }
    }

    #endregion

    #region CanBeRecycled Property

    protected internal virtual bool CanBeRecycled
    {
      get
      {
        if( !this.IsErrorStyleApplied )
        {
          // We should not recycle the cell if the user has set a style or template explicitly.
          if( Cell.IsValueSourceHazardingCellRecycling( DependencyPropertyHelper.GetValueSource( this, Cell.StyleProperty ) )
            || Cell.IsValueSourceHazardingCellRecycling( DependencyPropertyHelper.GetValueSource( this, Cell.TemplateProperty ) ) )
            return false;
        }
        else if( m_styleBeforeError != DependencyProperty.UnsetValue )
        {
          // The regular style has been set by the user.
          return false;
        }

        return true;
      }
    }

    #endregion

    #region CanBeCollapsed Property

    internal virtual bool CanBeCollapsed
    {
      get
      {
        if( this.IsCurrent || this.IsDirty || this.HasValidationError || this.IsBeingEdited || this.IsKeyboardFocused || this.IsKeyboardFocusWithin )
          return false;

        var parentColumn = this.ParentColumn;
        if( parentColumn == null )
          return true;

        return !TableflowView.GetIsBeingDraggedAnimated( parentColumn );
      }
    }

    #endregion

    #region FocusEditor Property

    private bool FocusEditor
    {
      get
      {
        return m_flags[ ( int )CellFlags.FocusEditor ];
      }
      set
      {
        m_flags[ ( int )CellFlags.FocusEditor ] = value;
      }
    }

    #endregion

    #region PreventValidateAndSetAllErrors Property

    private bool PreventValidateAndSetAllErrors
    {
      get
      {
        return m_flags[ ( int )CellFlags.PreventValidateAndSetAllErrors ];
      }
      set
      {
        m_flags[ ( int )CellFlags.PreventValidateAndSetAllErrors ] = value;
      }
    }

    #endregion

    #region IsBeingEditedCache Property

    private bool IsBeingEditedCache
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsBeingEdited ];
      }
      set
      {
        m_flags[ ( int )CellFlags.IsBeingEdited ] = value;
      }
    }

    #endregion

    #region HasCellEditorError Property

    internal bool HasCellEditorError
    {
      get
      {
        return m_flags[ ( int )CellFlags.HasCellEditorError ];
      }
      set
      {
        m_flags[ ( int )CellFlags.HasCellEditorError ] = value;
      }
    }

    #endregion

    #region HasUIValidationError Property

    private bool HasUIValidationError
    {
      get
      {
        return m_flags[ ( int )CellFlags.HasUIValidationError ];
      }
      set
      {
        m_flags[ ( int )CellFlags.HasUIValidationError ] = value;
      }
    }

    #endregion

    #region HasContentBindingValidationError Property

    private bool HasContentBindingValidationError
    {
      get
      {
        return m_flags[ ( int )CellFlags.HasContentBindingValidationError ];
      }
      set
      {
        m_flags[ ( int )CellFlags.HasContentBindingValidationError ] = value;
      }
    }

    #endregion

    #region HasPendingErrorStyleRefresh Property

    private bool HasPendingErrorStyleRefresh
    {
      get
      {
        return m_flags[ ( int )CellFlags.HasPendingErrorStyleRefresh ];
      }
      set
      {
        m_flags[ ( int )CellFlags.HasPendingErrorStyleRefresh ] = value;
      }
    }

    #endregion

    #region IsErrorStyleApplied Property

    private bool IsErrorStyleApplied
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsErrorStyleApplied ];
      }
      set
      {
        m_flags[ ( int )CellFlags.IsErrorStyleApplied ] = value;
      }
    }

    #endregion

    #region IsUpdatingContentBindingSource Property

    private bool IsUpdatingContentBindingSource
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsUpdatingContentBindingSource ];
      }
      set
      {
        m_flags[ ( int )CellFlags.IsUpdatingContentBindingSource ] = value;
      }
    }

    #endregion

    #region IsInCascadingValidation Property

    private bool IsInCascadingValidation
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsInCascadingValidation ];
      }
      set
      {
        m_flags[ ( int )CellFlags.IsInCascadingValidation ] = value;
      }
    }

    #endregion

    #region HasPendingSyncParentErrorFlags Property

    private bool HasPendingSyncParentErrorFlags
    {
      get
      {
        return m_flags[ ( int )CellFlags.HasPendingSyncParentErrorFlags ];
      }
      set
      {
        m_flags[ ( int )CellFlags.HasPendingSyncParentErrorFlags ] = value;
      }
    }

    #endregion

    #region IsRestoringEditionState Property

    private bool IsRestoringEditionState
    {
      get
      {
        return m_flags[ ( int )CellFlags.IsRestoringEditionState ];
      }
      set
      {
        m_flags[ ( int )CellFlags.IsRestoringEditionState ] = value;
      }
    }

    #endregion

    #region AnimatedColumnReorderingBindingApplied Property

    private bool AnimatedColumnReorderingBindingApplied
    {
      get
      {
        return m_flags[ ( int )CellFlags.AnimatedColumnReorderingBindingApplied ];
      }
      set
      {
        m_flags[ ( int )CellFlags.AnimatedColumnReorderingBindingApplied ] = value;
      }
    }

    #endregion

    #region PreventMakeVisible Property

    private bool PreventMakeVisible
    {
      get
      {
        return m_flags[ ( int )CellFlags.PreventMakeVisible ];
      }
      set
      {
        m_flags[ ( int )CellFlags.PreventMakeVisible ] = value;
      }
    }

    #endregion

    #region ResetNonTransientFlags Property

    private void ResetNonTransientFlags()
    {
      m_flags = new BitVector32( m_flags.Data & ( int )( CellFlags.IsInternalyInitialized ) );

      if( !this.HasPendingErrorStyleRefresh )
      {
        this.AbortRefreshErrorStyle();
      }
    }

    #endregion

    #region HasAliveContentBinding

    protected internal virtual bool HasAliveContentBinding
    {
      get
      {
        return true;
      }
    }

    #endregion

    public static Cell FindFromChild( DependencyObject child )
    {
      return Cell.FindFromChild( ( DataGridContext )null, child );
    }

    public static Cell FindFromChild( DataGridContext dataGridContext, DependencyObject child )
    {
      // In this situation, the dataGridContext is the DataGridContext of the Cell to find.
      // Useful when a grid is used as a Cell editor and want the Cell for a specific DataGridContext.
      if( child == null )
        return null;

      Cell cell = null;

      while( ( cell == null ) && ( child != null ) )
      {
        child = TreeHelper.GetParent( child );
        cell = child as Cell;

        if( ( cell != null )
          && ( dataGridContext != null )
          && ( dataGridContext != DataGridControl.GetDataGridContext( cell ) ) )
        {
          cell = null;
        }
      }

      return cell;
    }

    public static Cell FindFromChild( DataGridControl dataGridControl, DependencyObject child )
    {
      // In this situation, the dataGridControl is the DataGridControl of the Cell to find.
      // Useful when a grid is used as a Cell editor and want the Cell for a specific DataGridControl.
      if( child == null )
        return null;

      Cell cell = null;

      while( ( cell == null ) && ( child != null ) )
      {
        child = TreeHelper.GetParent( child );
        cell = child as Cell;

        if( ( cell != null ) && ( dataGridControl != null ) )
        {
          DataGridContext tempDataGridContext = DataGridControl.GetDataGridContext( cell );

          if( ( tempDataGridContext == null ) || ( tempDataGridContext.DataGridControl != dataGridControl ) )
          {
            cell = null;
          }
        }
      }

      return cell;
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( m_cellContentPresenter != null )
      {
        BindingOperations.ClearBinding( m_cellContentPresenter, UIElement.OpacityProperty );
      }

      m_cellContentPresenter = this.GetTemplateChild( "PART_CellContentPresenter" ) as ContentPresenter;

      if( m_cellContentPresenter != null )
      {
        BindingOperations.SetBinding( m_cellContentPresenter, UIElement.OpacityProperty, Cell.ParentRowCellContentOpacityBinding );
      }
    }

    public override string ToString()
    {
      string fieldName = this.FieldName;
      object content = this.Content;
      string cellType = string.Empty;

      Type type = this.GetType();

      if( type != null )
        cellType = type.ToString();

      try
      {
        return String.Format( "{0} : FieldName = {1}, Content = {2}", cellType, ( fieldName != null ) ? fieldName : "", ( content != null ) ? content.ToString() : "null" );
      }
      catch( Exception )
      {
        return base.ToString();
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
    public double GetFittedWidth()
    {
      if( !this.IsContainerPrepared || this.IsContainerVirtualized )
        return -1d;

      this.ApplyTemplate();

      var child = ( this.VisualChildrenCount > 0 ) ? this.GetVisualChild( 0 ) as UIElement : null;
      if( child == null )
        return -1d;

      child.Measure( new Size( double.PositiveInfinity, this.ActualHeight ) );

      return child.DesiredSize.Width;
    }

    protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      base.OnMouseLeftButtonDown( e );
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      dataGridContext.DataGridControl.ResetDragDataObject();

      if( e.Handled )
        return;

      Row parentRow = this.ParentRow;
      ColumnBase parentColumn = this.ParentColumn;

      if( ( parentRow != null ) && ( parentColumn != null )
        && ( parentRow.NavigationBehavior != NavigationBehavior.None )
        && ( parentRow.NavigationBehavior != NavigationBehavior.RowOnly ) )
      {
        bool wasAlreadyCurrent = this.IsCurrent;

        if( !this.GetCalculatedCanBeCurrent() )
        {
          if( ( this.ParentRow == dataGridContext.CurrentRow ) && ( this.ParentRow.NavigationBehavior == NavigationBehavior.RowOrCell ) )
          {
            try
            {
              dataGridContext.SetCurrentColumnCore( null, true, false, AutoScrollCurrentItemSourceTriggers.Navigation );
            }
            catch( DataGridException )
            {
              // We swallow the exception if it occurs because of a validation error or Cell was read-only or
              // any other GridException.
            }
          }

          return;
        }

        bool focused = this.Focus();
        e.Handled = true;

        if( !focused )
          return;

        // Keep a reference to the mouse position so we can calculate when a drag operation is actually started.
        dataGridContext.DataGridControl.InitializeDragPostion( e );

        //only process the SingleClick and ClickOnCurrentCell edit triggers if the CTRL or the SHIFT key is not pressed.
        if( ( ( Keyboard.Modifiers & ModifierKeys.Control ) != ModifierKeys.Control )
          && ( ( Keyboard.Modifiers & ModifierKeys.Shift ) != ModifierKeys.Shift ) )
        {
          bool readOnly = this.GetInheritedReadOnly();

          if( readOnly )
            return;

          //If grid is configured for SingleClickEdit, set edition status
          if( ( parentRow.IsEditTriggerSet( EditTriggers.SingleClick ) )
            || ( ( wasAlreadyCurrent ) && ( parentRow.IsEditTriggerSet( EditTriggers.ClickOnCurrentCell ) ) ) )
          {
            try
            {
              this.BeginEdit();
            }
            catch( DataGridException )
            {
              // We swallow the exception if it occurs because of a validation error or Cell was read-only or
              // any other GridException.
            }
          }
        }
      }
    }

    protected override void OnPreviewTextInput( TextCompositionEventArgs e )
    {
      base.OnPreviewTextInput( e );

      if( e.Handled )
        return;

      var parentRow = this.ParentRow;
      if( parentRow == null )
        return;

      //Do not try to process the Activation Gesture if the Cell is readonly!
      var readOnly = this.GetInheritedReadOnly();
      if( readOnly )
        return;

      //This condition ensures that we process the TextInput against the editor only if the Grid is configured to answer to activation gestures.
      if( !( parentRow.IsEditTriggerSet( EditTriggers.ActivationGesture ) ) )
        return;

      var editor = this.GetCellEditor();

      //This condition is more complex and is used to filter cases were a TextInput is done and there is no need to process for activation keys...
      //Condition is: Editor is not null, editor is not displaued and editor is not pending showing.
      if( ( editor != null ) && ( this.IsBeingEdited == false ) && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) )
      {

        //If the Activation gestures for the editor includes TextInput
        //Note that this condition also filters out TextInput which are other than Regular (non system or control) TextInput
        if( ( !string.IsNullOrEmpty( e.Text ) ) && ( string.IsNullOrEmpty( e.SystemText ) ) && ( string.IsNullOrEmpty( e.ControlText ) )
          && ( editor.IsTextInputActivation() ) )
        {
          m_textInputArgs = e;
          e.Handled = true;

          try
          {
            //enter edit mode for cell
            this.BeginEdit();
          }
          catch( DataGridException )
          {
            // We swallow the exception if it occurs because of a validation error or Cell was read-only or
            // any other GridException.
          }
        }
      }
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      base.OnPreviewKeyDown( e );

      if( e.Handled )
        return;

      Row parentRow = this.ParentRow;

      if( parentRow == null )
        return;

      //This condition ensures that we process the KeyDown against the editor only if the Grid is configured to answer to activation gestures.
      if( !( parentRow.IsEditTriggerSet( EditTriggers.ActivationGesture ) ) )
        return;

      CellEditor editor = this.GetCellEditor();

      //This condition is more complex and is used to filter cases were a KeyDown is done and there is no need to process for activation keys...
      //Condition is: Editor is not null, editor is not displaued and editor is not pending showing.

      if( ( editor != null ) && ( !this.IsBeingEdited ) && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) )
      {
        KeyActivationGesture gesture = editor.GetMatchingKeyActivationGesture( e.Key, e.SystemKey, Keyboard.Modifiers );

        //Check if the key stroke received corresponds to one matching one of the KeyActivationGesture of the editor
        if( gesture == null )
          return;

        //preserve the keystroke so it can be "resent" to the editor once layout is updated.
        m_keyGesture = gesture;

        e.Handled = true;

        try
        {
          //If the key match, then enter edit mode.
          this.BeginEdit();
        }
        catch( DataGridException )
        {
          // We swallow the exception if it occurs because of a validation error or Cell was read-only or
          // any other GridException.
        }
      }
    }

    protected override void OnVisualParentChanged( DependencyObject oldParent )
    {
      base.OnVisualParentChanged( oldParent );

      // Since the parent property isn't a DP, we must update the binding ourself.
      this.UpdateParentForeground();
    }


    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );
      this.OnPropertyChanged( e.Property.Name );
    }

    protected virtual void InitializeCore( DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn )
    {
      this.ParentRow = parentRow;
      this.ParentColumn = parentColumn;

      this.UpdateAnimatedColumnReorderingBindings( dataGridContext, Row.GetIsTemplateCell( this ) );

      if( !this.IsInternalyInitialized )
      {
        Binding rowMatchingConditionsBinding = new Binding();
        rowMatchingConditionsBinding.Path = new PropertyPath( "(0).(1)", Cell.ParentRowProperty, Row.RowDisplayEditorMatchingConditionsProperty );
        rowMatchingConditionsBinding.Source = this;
        rowMatchingConditionsBinding.Mode = BindingMode.OneWay;

        //Initialize the RowDisplayEditorMatchingConditions binding to the ParentRow
        BindingOperations.SetBinding( this, Cell.RowDisplayEditorMatchingConditionsProperty, rowMatchingConditionsBinding );

        // Clear the FieldName since it will be taken from the column instead of directly from the Cell.
        this.SetFieldName( string.Empty );
      }
    }

    protected virtual CellEditor GetCellEditor()
    {
      if( !this.IsInternalyInitialized )
        return null;

      var parentColumnBase = this.ParentColumn;

      if( parentColumnBase == null )
        return null;

      //Since the CellEditor property is set by default, first verify if the CelLEditorSelector property has been explicitly set.
      if( parentColumnBase.CellEditorSelector != null )
        return parentColumnBase.CellEditorSelector.SelectCellEditor( parentColumnBase, this.DataContext );

      if( parentColumnBase.CellEditor != null )
        return parentColumnBase.CellEditor;

      var parentColumn = parentColumnBase as Column;

      if( parentColumn != null )
      {
        // Try to get a CellEditor from the ParentColumn.ForeignKeyConfiguration
        var foreignKeyConfiguration = parentColumn.ForeignKeyConfiguration;

        if( foreignKeyConfiguration != null )
        {
          return foreignKeyConfiguration.DefaultCellEditor;
        }
      }

      var content = this.Content;

      if( content == null )
        return null;

      var contentType = content.GetType();

      var cellEditor = DefaultCellEditorSelector.SelectCellEditor( contentType );

      if( cellEditor != null )
        return cellEditor;

      var typeConverter = TypeDescriptor.GetConverter( contentType );

      if( typeConverter == null )
        return null;

      if( typeConverter.CanConvertFrom( typeof( string ) ) && typeConverter.CanConvertTo( typeof( string ) ) )
      {
        return DefaultCellEditorSelector.TextBoxEditor;
      }

      return null;
    }

    protected internal void Initialize( DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn )
    {
      //Check that both parameters are valid.
      if( parentRow == null )
        throw new ArgumentNullException( "parentRow" );

      if( parentColumn == null )
        throw new ArgumentNullException( "parentColumn" );

      if( ( this.IsInternalyInitialized )
        && ( this.IsContainerPrepared )
        && ( parentColumn == this.ParentColumn )
        && ( parentRow == this.ParentRow ) )
        return;

      //Mark the cell has being recycled to prevent some check to occur.
      this.IsContainerRecycled = this.IsInternalyInitialized;

      try
      {
        //A cell that hasn't been prepare once or that has a new parent column
        //due to recycling needs to be prepared again.
        this.IsContainerPrepared = false;

        this.InitializeCore( dataGridContext, parentRow, parentColumn );

        this.IsInternalyInitialized = true;

        this.PostInitialize();
      }
      finally
      {
        //From here, there is no difference between a fresh new cell and
        //a recycled cell.
        this.IsContainerRecycled = false;
      }
    }

    protected internal virtual void PostInitialize()
    {
      if( this.ParentRow.IsBeingEdited )
      {
        // Cell was added to the row's CreatedCells.  Update the parentColumn's cell in edition state.
        var parentColumn = this.ParentColumn;

        if( ( parentColumn != null ) && ( parentColumn.CurrentRowInEditionCellState == null ) )
        {
          CellState cellState = new CellState();
          cellState.SetContentBeforeRowEdition( this.Content );
          parentColumn.CurrentRowInEditionCellState = cellState;
        }
      }
    }

    protected internal virtual void PrepareContainer( DataGridContext dataGridContext, object item )
    {
      this.IsContainerPartiallyCleared = false;

      // If the container is already prepared and not virtualized and the DataContext is the same, just ignore this call.
      // For example, a cell generated through the xaml parser can already be prepared (went through this method once) but
      // still have its DataContext set only after it has been prepared, thus the need to excute this method once more.
      var dataContext = this.DataContext;
      if( this.IsContainerPrepared && !this.IsContainerVirtualized &&
        ( ( dataContext == item ) || ( ( dataContext is UnboundDataItem ) && this.IsSameUnboundItem( dataContext, item ) ) ) )
        return;

      //Make sure that the DataGridContext is set appropriatly on the cells that are already created.
      //This is to ensure that the value is appropriate when the container is recycled within another DataGridContext ( another detail on the same level ).
      DataGridControl.SetDataGridContext( this, dataGridContext );

      var parentColumn = this.ParentColumn;
      var isSameDataContext = Cell.AssignDataContext( this, item, null, parentColumn );

      // In some scenarios, there may be validation errors on the content binding that is not reflected on the cell or the row, so make sure it is.
      if( isSameDataContext && ( item is IDataErrorInfo ) && ( Validation.GetErrors( this ).Count > 0 ) && !this.HasValidationError
          && !this.IsDirty)
      {
        this.NotifyContentBindingValidationError();
      }

      // If there is a current column on the DataGridContext, try to restore the currency of the cell
      var parentRow = this.ParentRow;
      var currentColumn = dataGridContext.CurrentColumn;
      if( ( parentRow != null ) && ( parentRow.IsCurrent ) && ( currentColumn != null ) && ( currentColumn == parentColumn ) )
      {
        this.SetIsCurrent( true );
      }

      // This will force invalidation of the CoercedContentTemplate, only if the content is null after the prepare
      // and if the content template is set to something else than default. However, when binded to an EmptyDataItem,
      // our content will always be null but a CellContentTemplate will be applied. In this case, we do NOT want to  reapplied the template every time.
      if( ( this.Content == null ) && ( !( this.GetRealDataContext() is EmptyDataItem ) ) )
      {
        this.ClearValue( Cell.CoercedContentTemplatePropertyKey );
      }

      this.UpdateCoercedContentTemplate( true );
      this.UpdateMatchingDisplayConditions();

      // Ensure to reset both container recycling flags
      this.IsContainerPrepared = true;
      this.IsContainerVirtualized = false;

      // Force a refresh of the displayed template in case the display conditions where initially matched (e.g. Always).
      this.RefreshDisplayedTemplate();
    }

    protected internal virtual void ClearContainer()
    {
      // If the container is not prepared just ignore this call.
      if( !this.IsContainerPrepared )
        return;

      this.IsContainerPrepared = false;
      this.IsContainerPartiallyCleared = false;

      this.AbortRefreshErrorStyle();
      this.AbortHideEditTemplate();

      if( m_loadedRoutedEventHandler != null )
      {
        this.Loaded -= m_loadedRoutedEventHandler;
        m_loadedRoutedEventHandler = null;
      }

      // In DP's PropertyChanged Callbacks, for the sake of performance, you could return immediatly if the cell is in process of being cleared.

      this.ClearValue( Cell.StyleProperty );
      this.ClearValue( Cell.IsCurrentPropertyKey );
      this.ClearValue( Cell.IsSelectedPropertyKey );
      this.ClearValue( Cell.IsBeingEditedPropertyKey );
      this.ClearValue( Cell.ValidationErrorPropertyKey );
      this.ClearValue( Cell.HasValidationErrorPropertyKey );
      this.ClearValue( Cell.IsValidationErrorRestrictivePropertyKey );

      // We must not clear the IsDirty flag of the Cell here since its Content could have been edited and this flag is checked
      // to determine if the new value is commited or not in the source

      // No need to clear the Cell.CellDisplayEditorMatchingConditionsProperty neither DataGridControl.CellEditorDisplayConditionsProperty since
      // both will be updated when Cell.PrepareContainer is called again

      m_cellValidationRules = null;
      m_styleBeforeError = DependencyProperty.UnsetValue;
      this.CurrentEditorPendingDisplayState = EditorDisplayState.None;
      this.IsContainerVirtualized = true;

      this.SetIsCellEditorDisplayed( false );

      // This will reset every flags maintained in m_flags
      this.ResetNonTransientFlags();
    }

    protected internal virtual void PartialClearContainer()
    {
      // If the container is not prepared just ignore this call.
      if( !this.IsContainerPrepared )
        return;

      this.IsContainerPartiallyCleared = true;

      this.AbortRefreshErrorStyle();
      this.AbortHideEditTemplate();

      this.ClearValue( Cell.StyleProperty );
      this.ClearValue( Cell.IsCurrentPropertyKey );
      this.ClearValue( Cell.IsSelectedPropertyKey );
      this.ClearValue( Cell.IsBeingEditedPropertyKey );
      this.ClearValue( Cell.ValidationErrorPropertyKey );
      this.ClearValue( Cell.HasValidationErrorPropertyKey );
      this.ClearValue( Cell.IsValidationErrorRestrictivePropertyKey );

      m_styleBeforeError = DependencyProperty.UnsetValue;
      this.CurrentEditorPendingDisplayState = EditorDisplayState.None;

      this.SetIsCellEditorDisplayed( false );

      // This will reset every flags maintained in m_flags
      this.ResetNonTransientFlags();
    }

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      var newThemeKey = view.GetDefaultStyleKey( typeof( Cell ) );
      if( object.Equals( this.DefaultStyleKey, newThemeKey ) )
        return;

      this.DefaultStyleKey = newThemeKey;
    }

    protected internal virtual void AddContentBinding( DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn )
    {
    }

    protected internal virtual void RemoveContentBinding()
    {
    }

    protected internal virtual void CleanUpOnRemove()
    {
      this.AbortRefreshErrorStyle();
      this.AbortHideEditTemplate();

      if( m_loadedRoutedEventHandler != null )
      {
        this.Loaded -= m_loadedRoutedEventHandler;
        m_loadedRoutedEventHandler = null;
      }

      //This is done so the WeakEventListener on ParentColumn is removed.
      this.ParentColumn = null;
    }

    internal static Cell FindFromChildOrSelf( DataGridControl dataGridControl, DependencyObject child )
    {
      var cell = child as Cell;
      if( cell != null )
      {
        if( dataGridControl == null )
          return cell;

        var dataGridContext = DataGridControl.GetDataGridContext( cell );
        if( ( dataGridContext != null ) && ( dataGridContext.DataGridControl == dataGridControl ) )
          return cell;
      }

      return Cell.FindFromChild( dataGridControl, child );
    }

    internal static bool GetIsUIValidationError( CellValidationError validationError )
    {
      if( validationError == null )
        return false;

      return ( !Cell.GetIsContentBindingValidationError( validationError ) ) && ( !Cell.GetIsCellEditorError( validationError ) );
    }

    internal static bool GetIsCellEditorError( CellValidationError validationError )
    {
      if( validationError == null )
        return false;

      return validationError.RuleInError is CellEditorValidationRule;
    }

    internal static bool GetIsContentBindingValidationError( CellValidationError validationError )
    {
      if( validationError == null )
        return false;

      return validationError.RuleInError is CellContentBindingValidationRule;
    }

    internal static bool GetIsValidationErrorRestrictive( CellValidationError cellValidationError )
    {
      if( cellValidationError == null )
        return false;

      return Cell.GetIsRuleInErrorRestrictive( cellValidationError.RuleInError );
    }

    internal static bool GetIsValidationErrorRestrictive( ValidationError validationError )
    {
      if( validationError == null )
        return false;

      return Cell.GetIsRuleInErrorRestrictive( validationError.RuleInError );
    }

    internal static bool GetIsRuleInErrorRestrictive( CellValidationRule ruleInError )
    {
      if( ruleInError == null )
        return false;

      CellContentBindingValidationRule cellContentBindingValidationRule = ruleInError as CellContentBindingValidationRule;

      if( cellContentBindingValidationRule != null )
        return Cell.GetIsRuleInErrorRestrictive( cellContentBindingValidationRule.ValidationRule );

      return true;
    }

    internal static bool GetIsRuleInErrorRestrictive( ValidationRule ruleInError )
    {
      if( ruleInError == null )
        return false;

      return !( ruleInError is DataErrorValidationRule );
    }

    internal static bool IsCellEditorDisplayConditionsSet( Cell cell, CellEditorDisplayConditions condition )
    {
      if( ( cell.CellEditorDisplayConditions & condition ) == condition )
        return true;

      return false;
    }

    internal static UIElement GetCellFocusScope( UIElement reference )
    {
      if( reference == null )
        return null;

      if( Cell.GetIsCellFocusScope( reference ) )
      {
        return reference;
      }

      UIElement cellFocusScope = null;

      for( int i = 0; i < VisualTreeHelper.GetChildrenCount( reference ); i++ )
      {
        cellFocusScope = Cell.GetCellFocusScope( VisualTreeHelper.GetChild( reference, i ) as UIElement );

        if( cellFocusScope != null )
        {
          return cellFocusScope;
        }
      }

      return null;
    }

    internal static bool IsValueSourceHazardingCellRecycling( ValueSource valueSource )
    {
      switch( valueSource.BaseValueSource )
      {
        case BaseValueSource.Local:
        case BaseValueSource.ParentTemplate:
        case BaseValueSource.ParentTemplateTrigger:
        case BaseValueSource.Unknown:
          return true;

        default:
          return false;
      }
    }

    internal static bool AssignDataContext( Cell cell, object dataContext, UnboundDataItem unboundDataItemContext, ColumnBase parentColumn )
    {
      var column = parentColumn as Column;
      if( ( column != null ) && ( column.IsBoundToDataGridUnboundItemProperty ) )
      {
        dataContext = unboundDataItemContext ?? UnboundDataItem.GetUnboundDataItem( dataContext );
      }

      // Read the LocalValue of the DataContext to avoid getting the one inherited from the ParentRow.
      // This prevent the DataContext to become null when the Cell is virtualized.
      object localDataContext = cell.ReadLocalValue( Cell.DataContextProperty );

      if( localDataContext != dataContext )
      {
        // The system will call Equals instead of RefEquals on the DataContext change.  If Equals has been overriden and returns true, the DataContext will not be updated.
        // Hence setting it to null will make sure the right DataContext is used by the cell, and thus the cell content will be correctly updated.
        // This would not have to be done if a reference to the old DataItem was not keeped, which DataItem may not be part of the source anymore.
        if( object.Equals( localDataContext, dataContext ) )
        {
          cell.DataContext = null;
        }
        cell.DataContext = dataContext;

        return false;
      }

      return true;
    }

    //This ensures the CellEditorContext is set on the Cell to avoid problems with RelativeSource causing undesired behaviors when ComboBox is used as default CellEditor
    internal void EnsureCellEditorContext()
    {
      ForeignKeyConfiguration configuration = null;

      var parentColumn = this.ParentColumn as Column;

      if( parentColumn != null )
      {
        configuration = parentColumn.ForeignKeyConfiguration;
        CellEditorContext context = new CellEditorContext( parentColumn, configuration );
        Cell.SetCellEditorContext( this, context );
      }
    }

    internal void RefreshDisplayedTemplate()
    {
      // Never refresh any templates when the Cell is not prepared or virtualized
      if( !this.IsContainerPrepared || this.IsContainerVirtualized )
        return;

      Row parentRow = this.ParentRow;

      // Never refresh template while clearing container
      if( ( parentRow == null ) || parentRow.IsClearingContainer )
        return;

      if( this.ShouldDisplayEditor )
      {
        this.EnsureCellEditorContext();
        this.ShowEditTemplate();
      }
      else
      {
        this.DelayHideEditTemplate();
      }
    }

    internal void RevertEditedValue()
    {
      CellState cellState = this.GetEditCachedState();

      if( cellState != null )
      {
        Debug.Assert( ( cellState.ContentBeforeRowEdition != DependencyProperty.UnsetValue ), "It seems the column was recreated or the cellstate overwritten while the row/cell was in edition." );

        if( cellState.ContentBeforeRowEdition != DependencyProperty.UnsetValue )
        {
          this.PreventValidateAndSetAllErrors = true;
          try
          {
            if( ( this.GetInheritedReadOnly() ) || ( !this.GetIsContentBindingSupportingSourceUpdate() ) )
            {
              this.UpdateContentBindingTarget();
            }
            else
            {
              this.Content = cellState.ContentBeforeRowEdition;
            }
          }
          finally
          {
            this.PreventValidateAndSetAllErrors = false;
          }

          // Since it is impossible to leave edition on a row containing restrictive errors, there's no need to re-validate the UI Rules.
          // The Validation.Error handler on the Content Binding will take care of updating the ValidationError property if it need to,
          // which will in turn synch the HasValidationError property and refresh the Cell's style either to its normal style or
          // to its error style.
          Exception exception;
          CellValidationRule ruleInErrorWrapper;

          // We always want to update the content binding source.
          this.ClearAllErrors();
          this.ValidateAndSetAllErrors( false, false, true, true, out exception, out ruleInErrorWrapper );
        }
      }

      this.SetIsDirty( false );
    }

    internal void UpdateContentBindingTarget()
    {
      // Never update the target of a binding to a an item if the Cell is dirty
      if( ( !this.IsDirty ) )
      {
        // We need to refresh the Cell.Content target binding in case the dataObject value was coerced to something else.
        BindingExpressionBase bindingExpression = this.GetContentBindingExpression();

        if( bindingExpression != null )
        {
          bindingExpression.UpdateTarget();
        }
      }
    }

    internal void RestoreEditionState( ColumnBase currentColumn )
    {
      Debug.Assert( ( this.ParentRow != null ) && ( this.ParentRow.IsBeingEdited ) && ( this.ParentColumn != null ) );

      ColumnBase parentColumn = this.ParentColumn;

      if( parentColumn == null )
        return;

      CellState savedState = parentColumn.CurrentRowInEditionCellState;

      Debug.Assert( savedState != null );

      if( savedState == null )
        return;

      bool wasDirty = savedState.IsDirty;
      this.SetIsDirty( wasDirty );

      if( wasDirty )
      {
        if( ( savedState.Content != DependencyProperty.UnsetValue )
          && ( !object.Equals( this.Content, savedState.Content ) )
          && ( this.GetIsContentBindingSupportingSourceUpdate() ) )
        {
          this.IsRestoringEditionState = true;

          try
          {
            this.Content = savedState.Content;
          }
          finally
          {
            this.IsRestoringEditionState = false;
          }
        }
      }

      if( ( savedState.IsBeingEdited ) && ( parentColumn == currentColumn ) )
        this.BeginEdit();


      this.SetValidationError( savedState.CellValidationError );
    }

    internal void OnIsTemplateCellChanged( bool newValue )
    {
      this.UpdateAnimatedColumnReorderingBindings( DataGridControl.GetDataGridContext( this ), newValue );
    }

    internal ValidationResult UpdateContentBindingSource( out Exception exception, out CellValidationRule ruleInErrorWrapper )
    {
      Debug.Assert( ( ( this.IsDirty ) || ( this.IsInCascadingValidation ) ),
        "UpdateContentBindingSource should not be called when the cell isn't dirty beside when cascading validation.  Call ValidateContentBindingRules instead." );

      exception = null;
      ruleInErrorWrapper = null;

      var validationResult = ValidationResult.ValidResult;
      var contentBindingExpression = this.GetContentBindingExpression();

      if( contentBindingExpression != null )
      {
        // The caller of UpdateContentBindingSource will take care of setting the errors.
        this.PreventValidateAndSetAllErrors = true;
        this.IsUpdatingContentBindingSource = true;
        try
        {
          contentBindingExpression.UpdateSource();
        }
        finally
        {
          this.PreventValidateAndSetAllErrors = false;
          this.IsUpdatingContentBindingSource = false;
        }

        validationResult = this.ValidateContentBindingRules( out exception, out ruleInErrorWrapper );
      }

      // Only consider the content committed if the source was correctly updated.
      if( ( validationResult.IsValid ) || ( !( Cell.GetIsRuleInErrorRestrictive( ruleInErrorWrapper ) ) ) )
      {
        this.ContentCommitted();
      }

      // The dirty flag will only be lowered when the row ends or cancels edition, or if the cell cancels edition and it wasn't dirty when begining edition.
      return validationResult;
    }

    internal CellState GetEditCachedState()
    {
      var cellState = default( CellState );
      var parentRow = this.ParentRow;

      if( ( parentRow != null ) && ( ( parentRow.IsBeingEdited ) || ( parentRow.IsBeginningEdition ) ) )
      {
        var dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext != null )
        {
          var dataGridControl = dataGridContext.DataGridControl;

          if( ( dataGridControl != null ) && ( dataGridControl.CurrentRowInEditionState != null ) )
          {
            var parentColumn = this.ParentColumn;

            if( parentColumn != null )
            {
              cellState = parentColumn.CurrentRowInEditionCellState;
            }
          }
        }
      }

      return cellState;
    }

    internal Nullable<DataGridUpdateSourceTrigger> GetContentBindingUpdateSourceTrigger()
    {
      BindingExpressionBase contentBindingExpression = this.GetContentBindingExpression();

      if( contentBindingExpression != null )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext != null )
        {
          DataGridControl parentDataGridControl = dataGridContext.DataGridControl;

          if( parentDataGridControl != null )
            return parentDataGridControl.UpdateSourceTrigger;
        }
      }

      return null;
    }

    internal IDisposable InhibitMakeVisible()
    {
      return new PreventMakeVisibleDisposable( this );
    }

    internal virtual bool GetCalculatedCanBeCurrent()
    {
      var column = this.ParentColumn;
      if( ( column == null ) || ( this.ParentRow == null ) )
        return false;

      if( column.CanBeCurrentWhenReadOnly )
        return true;

      return !this.GetInheritedReadOnly();
    }

    internal virtual void ContentCommitted()
    {
    }

    internal void EnsureInVisualTree()
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return;

      TableView view = dataGridContext.DataGridControl.GetView() as TableView;
      if( view == null )
        return;

      Row parentRow = this.ParentRow;
      if( parentRow == null )
        return;

      FixedCellPanel fixedCellPanel = parentRow.CellsHostPanel as FixedCellPanel;
      if( fixedCellPanel == null )
        return;

      var columnVirtualizationManager = ColumnVirtualizationManager.GetColumnVirtualizationManager( dataGridContext ) as TableViewColumnVirtualizationManagerBase;
      if( columnVirtualizationManager == null )
        return;

      if( columnVirtualizationManager.GetFixedFieldNames( this.ParentRow.LevelCache ).Contains( this.FieldName ) )
        return;

      if( !fixedCellPanel.ForceScrollingCellToLayout( this ) )
        return;

      UIElement container = dataGridContext.GetContainerFromItem( this.GetRealDataContext() ) as UIElement;
      if( container == null )
        return;

      // We must Call UpdateLayout on the container when a cell is 
      // added to VisualTree to ensure the offset relative to this 
      // container are correctly returned
      container.UpdateLayout();
    }

    internal virtual DataTemplate GetForeignKeyDataTemplate()
    {
      return null;
    }

    internal virtual DataTemplate GetCellStringFormatDataTemplate( DataTemplate contentTemplate )
    {
      return null;
    }

    private static object OnCoerceContent( DependencyObject sender, object value )
    {
      var cell = sender as Cell;

      // If we are updating the Content binding source and content is refreshed as a result, hold-on.
      // We will manually update Content from the source by calling UpdateTarget on the binding later.
      if( ( cell != null ) && cell.IsUpdatingContentBindingSource )
        return DependencyProperty.UnsetValue;

      return value;
    }

    private static void OnContentChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var cell = sender as Cell;
      if( cell == null )
        return;

      var parentRow = cell.ParentRow;
      if( parentRow == null )
        return;

      var doCellValidation = false;
      var parentColumn = cell.ParentColumn;

      var changeDuringRowIsInEdit = ( cell.IsInternalyInitialized )
        && ( !cell.IsContainerRecycled )
        && ( !parentRow.IsBeginningEdition )
        && ( !cell.IsUpdatingContentBindingSource )
        && ( !cell.IsRestoringEditionState )
        && ( parentRow.IsBeingEdited );

      if( changeDuringRowIsInEdit )
      {
        if( parentColumn != null )
        {
          if( parentColumn.CurrentRowInEditionCellState == null )
          {
#pragma warning disable 618
            Debug.Assert( false, "Should only happen with MultiSurfaceView. Unless there is a new template assigned to the Row in a trigger?" );
#pragma warning restore 618
            parentColumn.CurrentRowInEditionCellState = new CellState();
          }

          parentColumn.CurrentRowInEditionCellState.SetContent( cell.Content );
        }
      }

      if( changeDuringRowIsInEdit )
      {
        if( !object.Equals( e.OldValue, e.NewValue ) )
        {
          cell.SetIsDirty( true );
        }

        if( !cell.PreventValidateAndSetAllErrors )
        {
          if( cell.GetContentBindingUpdateSourceTrigger() == DataGridUpdateSourceTrigger.CellContentChanged )
          {
            doCellValidation = true;
          }
        }
      }

      if( doCellValidation )
      {
        // This method is called to make sure we are not calling ValidateAndSetAllErrors at all nor triggering a cascade validation
        // as a side-effect of updating a cell's content binding source while another cell is bound to the very same source.
        // We would be surprised of such a usage, but this fail-safe will take care of this possibility.
        if( !cell.GetIsSiblingUpdatingContentBindingSource() )
        {
          // The CellEditorRules must not be ckecked when UpdateSourceTrigger is set to CellContentChanged because the editor's HasValidationError property
          // changes only AFTER the new cell value is commited.  It would thus result in nothing being processed here if it were to be checked.

          Exception exception;
          CellValidationRule ruleInError;
          var validationResult = cell.ValidateCellRules( out exception, out ruleInError );
          if( validationResult.IsValid )
          {
            cell.ValidateAndSetAllErrors( false, false, true, true, out exception, out ruleInError );
          }
        }
      }

      //When the content changes, check if the CellContentTemplateSelector needs to be updated.
      //This depdends on several factor, including if a Selector is present and if the editor is not displayed
      if( cell.ShouldInvalidateCellContentTemplateSelector( parentColumn ) )
      {
        cell.UpdateCoercedContentTemplate( false );
      }
    }

    private static void OnIsDirtyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var cell = sender as Cell;
      if( cell == null )
        return;

      if( ( bool )e.NewValue )
      {
        //Raise the RoutedEvent that notifies any Row that the cell is becoming dirty.
        RoutedEventArgs eventArgs = new RoutedEventArgs( Cell.IsDirtyEvent );
        cell.RaiseEvent( eventArgs );
      }
    }

    private static void OnMatchingDisplayEditorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var cell = sender as Cell;
      if( cell == null )
        return;

      //performing this check because at the end of the Initializing function, I will call this explicitelly, to ensure 
      //proper display of the Editor if the appropriate conditions are met.
      if( cell.IsInternalyInitialized )
      {
        cell.RefreshDisplayedTemplate();
      }
    }

    private static void OnCellEditorDisplayConditionsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var cell = sender as Cell;
      if( cell == null )
        return;

      // The cell.ParentRow can be null if this Cell is a TemplateCell
      var parentRow = cell.ParentRow;
      if( ( parentRow != null ) && parentRow.IsClearingContainer )
        return;

      cell.UpdateMatchingDisplayConditions();
    }

    private static void OnContentTemplateChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var cell = sender as Cell;
      if( cell == null )
        return;

      cell.m_contentTemplateCache = e.NewValue as DataTemplate;

      if( ( cell.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) && ( !cell.IsCellEditorDisplayed ) )
      {
        cell.UpdateCoercedContentTemplate( false );
      }
    }

    private static void OnContentTemplateSelectorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var cell = sender as Cell;
      if( cell == null )
        return;

      cell.m_contentTemplateSelectorCache = e.NewValue as DataTemplateSelector;

      if( ( cell.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) && ( !cell.IsCellEditorDisplayed ) )
      {
        cell.UpdateCoercedContentTemplate( false );
      }
    }

    private static void OnIsTemplateCellChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var cell = sender as Cell;
      if( cell == null )
        return;

      cell.OnIsTemplateCellChanged( ( bool )e.NewValue );
    }

    private bool GetInheritedReadOnly()
    {
      var isReadOnly = false;
      var parentRow = this.ParentRow;
      var parentColumn = this.ParentColumn;

      if( m_readOnlyIsInternalySet )
      {
        this.ClearValue( Cell.ReadOnlyProperty );
        m_readOnlyIsInternalySet = false;
      }

      var readOnlyPropertyValueSource = DependencyPropertyHelper.GetValueSource( this, Cell.ReadOnlyProperty );
      var isDataBound = readOnlyPropertyValueSource.BaseValueSource > BaseValueSource.Inherited;

      //If a value is set in any way (explicitly or implicitly) by the user on the cell, use that value.
      if( isDataBound )
      {
        isReadOnly = this.ReadOnly;
      }

      //If a value is set in any way (explicitly or implicitly) by the user on the row, use that value.
      if( !isDataBound && ( parentRow != null ) )
      {
        readOnlyPropertyValueSource = DependencyPropertyHelper.GetValueSource( parentRow, Row.ReadOnlyProperty );
        isDataBound = readOnlyPropertyValueSource.BaseValueSource > BaseValueSource.Inherited;

        if( isDataBound )
        {
          isReadOnly = parentRow.ReadOnly;
          this.ReadOnly = isReadOnly;
          m_readOnlyIsInternalySet = true;
        }
      }

      //If a value is set in any way (explicitly or implicitly) by the user on the column, use that value.
      if( !isDataBound && ( parentColumn != null ) )
      {
        readOnlyPropertyValueSource = DependencyPropertyHelper.GetValueSource( parentColumn, ColumnBase.ReadOnlyProperty );
        isDataBound = readOnlyPropertyValueSource.BaseValueSource > BaseValueSource.Inherited;

        if( isDataBound )
        {
          isReadOnly = parentColumn.ReadOnly;
          this.ReadOnly = isReadOnly;
          m_readOnlyIsInternalySet = true;
        }
      }

      //If no value set by the user at any level, use the one set by the framework on the cell.
      if( !isDataBound )
      {
        isReadOnly = this.ReadOnly;
      }

      if( isReadOnly )
        return true;

      var contentBindingExpression = this.GetContentBindingExpression();
      if( contentBindingExpression == null )
        return false;

      Nullable<BindingMode> contentBindingMode = null;
      if( contentBindingExpression is BindingExpression )
      {
        contentBindingMode = ( ( Binding )contentBindingExpression.ParentBindingBase ).Mode;
      }
      else if( contentBindingExpression is MultiBindingExpression )
      {
        contentBindingMode = ( ( MultiBinding )contentBindingExpression.ParentBindingBase ).Mode;
      }

      return ( contentBindingMode.HasValue ) && ( contentBindingMode.Value != BindingMode.TwoWay );
    }

    private bool ShouldInvalidateCellContentTemplateSelector( ColumnBase parentColumn )
    {
      if( parentColumn == null )
        return false;

      var onlyInternalContentTemplateSelectorAvailable = ( ( this.ContentTemplateInternal == null ) && ( this.ContentTemplateSelectorInternal != null ) );
      var onlyColumnContentTemplateSelectorAvailable = ( ( parentColumn.CellContentTemplate == null ) && ( parentColumn.CellContentTemplateSelector != null ) );
      var onlyTemplateSelectorAvailable = onlyColumnContentTemplateSelectorAvailable || onlyInternalContentTemplateSelectorAvailable;
      var editorHiddenAndNotPendingShow = !this.IsCellEditorDisplayed && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow );

      // Only TemplateSelectors are available and editor is not hidden and pending showing
      return ( onlyTemplateSelectorAvailable && editorHiddenAndNotPendingShow );
    }

    private bool GetIsContentBindingSupportingSourceUpdate()
    {
      var binding = this.GetContentBindingExpression();
      if( binding == null )
        return false;

      BindingMode mode;
      var singleBinding = binding.ParentBindingBase as Binding;

      if( singleBinding == null )
      {
        var multiBinding = binding.ParentBindingBase as MultiBinding;
        if( multiBinding == null )
          return false;

        mode = multiBinding.Mode;
      }
      else
      {
        mode = singleBinding.Mode;
      }

      return ( mode == BindingMode.OneWayToSource ) || ( mode == BindingMode.TwoWay );
    }

    private bool GetIsSiblingUpdatingContentBindingSource()
    {
      // This method is called to make sure we are not calling ValidateAndSetAllErrors at all nor triggering a cascade validation
      // as a side-effect of updating a cell's content binding source while another cell is bound to the very same source.
      // We would be surprised of such a usage, but this fail-safe will take care of this possibility.
      var parentRow = this.ParentRow;
      if( parentRow != null )
      {
        foreach( Cell siblingCell in parentRow.CreatedCells )
        {
          if( ( siblingCell != this ) && ( siblingCell.IsUpdatingContentBindingSource ) )
          {
            // Such a situation should only occur when one of the two (or more) cells sharing the exact same source as their content binding.
            return true;
          }
        }
      }

      return false;
    }

    private DataTemplate GetCoercedCellContentTemplate()
    {
      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return null;

      var contentTemplate = this.ContentTemplateInternal;
      var content = this.Content;

      if( contentTemplate == null )
      {
        //If none, check if there is a ContentTemplateSelector directly assigned on cell.
        var contentTemplateSelector = this.ContentTemplateSelectorInternal;
        if( contentTemplateSelector != null )
        {
          //If there is one, the query it for a ContentTemplate.
          contentTemplate = contentTemplateSelector.SelectTemplate( content, this );
        }
      }

      var parentColumn = this.ParentColumn;

      //If there was no ContentTemplateSelector on Cell or if the output of the selector was Null.
      if( ( !this.OverrideColumnCellContentTemplate ) && ( contentTemplate == null ) && ( parentColumn != null ) )
      {
        contentTemplate = this.GetForeignKeyDataTemplate();
        if( contentTemplate != null )
          return contentTemplate;

        if( contentTemplate == null )
        {
          //If the parent Column defines a CellContentTemplate, then use it
          contentTemplate = parentColumn.CellContentTemplate;

          //if it doesn't, then check for a selector on Column
          if( contentTemplate == null )
          {
            var contentTemplateSelector = parentColumn.CellContentTemplateSelector;
            if( contentTemplateSelector != null )
            {
              //If a selector exists on Column, then use it.
              contentTemplate = contentTemplateSelector.SelectTemplate( content, this );
            }
          }
        }
      }

      //After all of this, if there is still no ContentTemplate found, then use the default basic one.
      if( contentTemplate == null )
      {
        contentTemplate = GenericContentTemplateSelector.Instance.SelectTemplate( content, this );
      }

      if( ( contentTemplate != null ) && ( parentColumn != null ) && !string.IsNullOrEmpty( parentColumn.CellContentStringFormat ) )
      {
        var newContentTemplate = this.GetCellStringFormatDataTemplate( contentTemplate );
        if( newContentTemplate != null )
        {
          contentTemplate = newContentTemplate;
        }
      }

      return contentTemplate;
    }

    private Style GetInheritedErrorStyle()
    {
      object style = this.ReadLocalValue( Cell.CellErrorStyleProperty );

      // In case we have a BindingExpression, we need to get the resulting value.
      if( style is BindingExpressionBase )
        style = this.CellErrorStyle;

      if( style == DependencyProperty.UnsetValue )
      {
        ColumnBase column = this.ParentColumn;

        if( column != null )
        {
          style = column.ReadLocalValue( Column.CellErrorStyleProperty );

          // In case we have a BindingExpression, we need to get the resulting value.
          if( style is BindingExpressionBase )
            style = column.CellErrorStyle;
        }
      }

      if( style == DependencyProperty.UnsetValue )
        style = this.CellErrorStyle;

      return style as Style;
    }

    private BindingExpressionBase GetContentBindingExpression()
    {
      return BindingOperations.GetBindingExpressionBase( this, Cell.ContentProperty );
    }

    private EditorDisplayState CurrentEditorPendingDisplayState
    {
      get;
      set;
    }

    private object GetRealDataContext()
    {
      object dataContext = this.DataContext;
      UnboundDataItem unboundDataItem = dataContext as UnboundDataItem;

      if( unboundDataItem == null )
        return dataContext;

      return unboundDataItem.DataItem;
    }

    private bool IsSameUnboundItem( object dataContext, object item )
    {
      UnboundDataItem unboundDataItem = dataContext as UnboundDataItem;

      return unboundDataItem.DataItem == item;
    }

    private void UpdateAnimatedColumnReorderingBindings( DataGridContext dataGridContext, bool isTemplateCell )
    {
      // Only set the Binding if the view is a TableflowView.
      var setBindings = ( !isTemplateCell )
                     && ( dataGridContext != null )
                     && ( dataGridContext.DataGridControl.GetView() is TableflowView );

      if( setBindings == this.AnimatedColumnReorderingBindingApplied )
        return;

      if( setBindings )
      {
        BindingOperations.SetBinding( this, Cell.RenderTransformProperty, Cell.ParentColumnTranslationBinding );
        BindingOperations.SetBinding( this, Cell.ParentColumnIsBeingDraggedProperty, Cell.ParentColumnIsBeingDraggedBinding );
        BindingOperations.SetBinding( this, Cell.ParentColumnReorderingDragSourceManagerProperty, Cell.ParentColumnReorderingDragSourceManagerBinding );
      }
      else
      {
        BindingOperations.ClearBinding( this, Cell.RenderTransformProperty );
        BindingOperations.ClearBinding( this, Cell.ParentColumnIsBeingDraggedProperty );
        BindingOperations.ClearBinding( this, Cell.ParentColumnReorderingDragSourceManagerProperty );
      }

      this.AnimatedColumnReorderingBindingApplied = setBindings;
    }

    private void ShowEditTemplate()
    {
      this.AbortHideEditTemplate();

      // The editor is not pending showing
      if( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow )
      {
        // If it is not displayed or pending hiding
        if( !this.IsCellEditorDisplayed || ( this.CurrentEditorPendingDisplayState == EditorDisplayState.PendingHide ) || m_forceEditorRefresh )
        {
          var cellEditor = this.GetCellEditor();
          if( cellEditor != null )
          {
            this.SetCoercedContentTemplate( this.GetCoercedCellContentTemplate(), cellEditor.EditTemplate );

            // Force the editor to update its editor state on next layout pass
            this.CurrentEditorPendingDisplayState = EditorDisplayState.PendingShow;
          }
        }
        else
        {
          // This will force re-layout of the control, raising the LayoutUpdated event
          // and correctly update the display state for this editor
          this.InvalidateArrange();
        }
      }
    }

    private void DelayHideEditTemplate()
    {
      if( m_delayedHideEditTemplateDispatcherOperation != null )
        return;

      if( ( !this.IsCellEditorDisplayed ) && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) )
        return;

      m_delayedHideEditTemplateDispatcherOperation = this.Dispatcher.BeginInvoke( new Action( this.HideEditTemplate ), DispatcherPriority.DataBind );
    }

    private void AbortHideEditTemplate()
    {
      if( m_delayedHideEditTemplateDispatcherOperation == null )
        return;

      Debug.Assert( m_delayedHideEditTemplateDispatcherOperation.Status == DispatcherOperationStatus.Pending );
      m_delayedHideEditTemplateDispatcherOperation.Abort();
      m_delayedHideEditTemplateDispatcherOperation = null;
    }

    private void HideEditTemplate()
    {
      m_delayedHideEditTemplateDispatcherOperation = null;

      // The Cell is not prepared or virtualized, no need to 
      // hide the edit template since it will be refreshed
      // the the Cell is prepared again.
      if( !this.IsContainerPrepared || this.IsContainerVirtualized )
        return;

      if( !this.IsCellEditorDisplayed && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) )
        return;

      this.SetCoercedContentTemplate( this.GetCoercedCellContentTemplate(), null );
      this.CurrentEditorPendingDisplayState = EditorDisplayState.PendingHide;

      // We must clear the CellEditorContext when the Template is hidden
      // to avoid side effects of the CellEditorBinding affecting null
      // in the source. The reason is that the CellEditorContext is the
      // binding source of the default foreign key CellEditor Template
      Cell.ClearCellEditorContext( this );
    }

    private void AddDraggedColumnGhost( ColumnReorderingDragSourceManager manager, bool isBeingDraggedAnimated )
    {
      if( !isBeingDraggedAnimated || !this.IsContainerPrepared || manager.ContainsDraggedColumnGhost( this ) )
        return;

      this.ClearValue( Cell.OpacityProperty );

      manager.AddDraggedColumnGhost( this );
    }

    private void RemoveDraggedColumnGhost( ColumnReorderingDragSourceManager manager )
    {
      if( !manager.ContainsDraggedColumnGhost( this ) )
        return;

      manager.RemoveDraggedColumnGhost( this );

      this.ClearValue( Cell.OpacityProperty );
    }

    private void UpdateMatchingDisplayConditions()
    {
      CellEditorDisplayConditions newEffectiveValue = CellEditorDisplayConditions.None;

      if( ( Cell.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.MouseOverCell ) ) &&
        ( this.IsMouseOver ) )
      {
        newEffectiveValue |= CellEditorDisplayConditions.MouseOverCell;
      }

      if( ( Cell.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.CellIsCurrent ) ) &&
        ( this.IsCurrent ) )
      {
        newEffectiveValue |= CellEditorDisplayConditions.CellIsCurrent;
      }

      if( Cell.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.Always ) )
      {
        newEffectiveValue |= CellEditorDisplayConditions.Always;
      }

      // No need to call RefreshDisplayedTemplate since 
      // this method is called when the CellDisplayEditorMatchingConditions
      // property changes on Cell or ParentColumn 
      // OR
      // when PrepareContainer is called on Cell
      this.SetValue( Cell.CellDisplayEditorMatchingConditionsProperty, newEffectiveValue );
    }

    private void UpdateFocusable()
    {
      var isFocusable = false;
      var dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext != null )
      {
        var column = this.ParentColumn;
        var dataGridControl = dataGridContext.DataGridControl;

        Debug.Assert( dataGridControl != null );

        if( ( !this.GetCalculatedCanBeCurrent() ) && ( column != null ) && ( column == dataGridContext.CurrentColumn ) )
        {
          try
          {
            dataGridContext.SetCurrentColumnCore( null, false, dataGridControl.SynchronizeSelectionWithCurrent, AutoScrollCurrentItemSourceTriggers.CurrentColumnChanged );
          }
          catch( DataGridException )
          {
            // We swallow the exception if it occurs because of a validation error or Cell was read-only or
            // any other GridException.
          }
        }

        isFocusable = true;

        if( !this.IsBeingEdited )
        {
          var row = this.ParentRow;
          if( row != null )
          {
            switch( row.NavigationBehavior )
            {
              case NavigationBehavior.None:
              case NavigationBehavior.RowOnly:
                isFocusable = false;
                break;
            }
          }
        }
      }

      //force an update of the NavigationBehavior characteristics
      this.Focusable = isFocusable && this.GetCalculatedCanBeCurrent();
    }

    private void OnBeginEditExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      try
      {
        this.BeginEdit();
      }
      catch( DataGridException )
      {
        // We swallow the exception if it occurs because of a validation error or Cell was read-only or
        // any other GridException.
      }
    }

    private void OnBeginEditCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext != null )
      {
        DataGridControl parentGrid = dataGridContext.DataGridControl;

        if( parentGrid == null || this.ParentRow == null )
        {
          e.CanExecute = false;
        }
        else
        {
          e.CanExecute = ( !this.IsBeingEdited )
                      && ( !this.GetInheritedReadOnly() )
                      && ( this.ParentRow.IsEditTriggerSet( EditTriggers.BeginEditCommand ) );
        }
      }
    }

    private void OnCancelEditExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.CancelEdit();
    }

    private void OnCancelEditCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = this.IsBeingEdited;
    }

    private void Cell_Loaded( object sender, RoutedEventArgs e )
    {
      System.Diagnostics.Debug.Assert( this.HasPendingErrorStyleRefresh, "The cell should have unsubcribed from the event." );
      System.Diagnostics.Debug.Assert( m_loadedRoutedEventHandler != null );

      this.Loaded -= m_loadedRoutedEventHandler;
      m_loadedRoutedEventHandler = null;

      this.DelayRefreshErrorStyle();
    }

    private void Cell_LayoutUpdated( object sender, EventArgs e )
    {
      if( !this.IsContainerPrepared || this.IsContainerVirtualized )
        return;

      if( this.CurrentEditorPendingDisplayState == EditorDisplayState.PendingShow )
      {
        //If the Cell is marked to be pending the display of the editor, then
        //this LayoutUpdated means that Editor has been effectively displayed



        //Change the IsCellEditorDisplayed flag accordingly
        this.SetIsCellEditorDisplayed( true );
      }
      else if( this.CurrentEditorPendingDisplayState == EditorDisplayState.PendingHide )
      {
        //If the Cell is marked to be pending the Display of the viewer, then
        //this LayoutUpdated means that Editor has been effectively hidden



        //Change the IsCellEditorDisplayed flag accordingly
        this.SetIsCellEditorDisplayed( false );
      }

      // Reset the Editor/Viewer displaying flag.
      this.CurrentEditorPendingDisplayState = EditorDisplayState.None;

      // This condition allows detecting if the focus needs to be changed 
      // after having displayed the editor.
      if( !this.IsCellEditorDisplayed || !this.FocusEditor )
        return;

      // Reset the flag that indicates if the focus needs to be set in the template
      this.FocusEditor = false;
      bool editorFocused = false;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl != null )
      {
        //Focus need to be changed.
        editorFocused = dataGridControl.SetFocusHelper( this.ParentRow, this.ParentColumn, false, true );
      }

      if( !editorFocused )
      {
        m_keyGesture = null;
        m_textInputArgs = null;
      }
      else
      {
        //If a keystroke was recorded for "resend" upon layout update
        if( m_keyGesture != null )
        {
          //Generate an EventArgs for the PreviewKeyDown for the KeyStroke
          Key realKey;
          if( ( m_keyGesture.Key == Key.None ) && ( m_keyGesture.SystemKey != Key.None ) )
          {
            realKey = m_keyGesture.SystemKey;
          }
          else
          {
            realKey = m_keyGesture.Key;
          }

          try
          {
            KeyEventArgs kea = new KeyEventArgs( Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, realKey );
            kea.RoutedEvent = Keyboard.PreviewKeyDownEvent;

            //send the event
            Keyboard.PrimaryDevice.FocusedElement.RaiseEvent( kea );

            if( kea.Handled == false )
            {
              //Generate an EventArgs for the KeyDown event Args for the preserved KeyStroke
              kea = new KeyEventArgs( Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, realKey );
              kea.RoutedEvent = Keyboard.KeyDownEvent;

              //Send the Event to the Input Manager
              Keyboard.PrimaryDevice.FocusedElement.RaiseEvent( kea );
            }
          }
          catch( SecurityException ex )
          {
            //if the exception is for the UIPermission, then we want to suppress it
            if( !( ex.PermissionType.FullName == "System.Security.Permissions.UIPermission" ) )
            {
              //not correct type, then rethrow the exception
              throw;
            }
          }

          //clear the KeyStroke preserved.
          m_keyGesture = null;
        }

        //If TextInput was preserved for resend
        if( m_textInputArgs != null )
        {
          //Generate new TextInput Event Args
          TextCompositionEventArgs new_event = new TextCompositionEventArgs( m_textInputArgs.Device, m_textInputArgs.TextComposition );
          new_event.RoutedEvent = UIElement.PreviewTextInputEvent;

          //Send the event to the InputManager.
          Keyboard.PrimaryDevice.FocusedElement.RaiseEvent( new_event );

          if( !new_event.Handled )
          {
            new_event = new TextCompositionEventArgs( m_textInputArgs.Device, m_textInputArgs.TextComposition );
            new_event.RoutedEvent = UIElement.TextInputEvent;

            //Send the event to the InputManager.
            Keyboard.PrimaryDevice.FocusedElement.RaiseEvent( new_event );
          }
        }

        //Clear the TextInput preserved.
        m_textInputArgs = null;
      }
    }

    #region EDITION

    public static readonly RoutedEvent EditBeginningEvent = EventManager.RegisterRoutedEvent( "EditBeginning", RoutingStrategy.Bubble, typeof( CancelRoutedEventHandler ), typeof( Cell ) );
    public static readonly RoutedEvent EditBegunEvent = EventManager.RegisterRoutedEvent( "EditBegun", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Cell ) );
    public static readonly RoutedEvent EditEndingEvent = EventManager.RegisterRoutedEvent( "EditEnding", RoutingStrategy.Bubble, typeof( CancelRoutedEventHandler ), typeof( Cell ) );
    public static readonly RoutedEvent EditEndedEvent = EventManager.RegisterRoutedEvent( "EditEnded", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Cell ) );
    public static readonly RoutedEvent EditCancelingEvent = EventManager.RegisterRoutedEvent( "EditCanceling", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Cell ) );
    public static readonly RoutedEvent EditCanceledEvent = EventManager.RegisterRoutedEvent( "EditCanceled", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Cell ) );

    public event CancelRoutedEventHandler EditBeginning
    {
      add
      {
        base.AddHandler( Cell.EditBeginningEvent, value );
      }
      remove
      {
        base.RemoveHandler( Cell.EditBeginningEvent, value );
      }
    }

    public event RoutedEventHandler EditBegun
    {
      add
      {
        base.AddHandler( Cell.EditBegunEvent, value );
      }
      remove
      {
        base.RemoveHandler( Cell.EditBegunEvent, value );
      }
    }

    public event CancelRoutedEventHandler EditEnding
    {
      add
      {
        base.AddHandler( Cell.EditEndingEvent, value );
      }
      remove
      {
        base.RemoveHandler( Cell.EditEndingEvent, value );
      }
    }

    public event RoutedEventHandler EditEnded
    {
      add
      {
        base.AddHandler( Cell.EditEndedEvent, value );
      }
      remove
      {
        base.RemoveHandler( Cell.EditEndedEvent, value );
      }
    }

    public event RoutedEventHandler EditCanceling
    {
      add
      {
        base.AddHandler( Cell.EditCancelingEvent, value );
      }
      remove
      {
        base.RemoveHandler( Cell.EditCancelingEvent, value );
      }
    }

    public event RoutedEventHandler EditCanceled
    {
      add
      {
        base.AddHandler( Cell.EditCanceledEvent, value );
      }
      remove
      {
        base.RemoveHandler( Cell.EditCanceledEvent, value );
      }
    }

    public void BeginEdit()
    {
      if( this.IsBeingEdited )
        return;

      var readOnly = this.GetInheritedReadOnly();
      if( readOnly )
        throw new DataGridException( "An attempt was made to edit a read-only cell or the cell content is not bound using two way binding." );

      // If there is no dataItem mapped to this container, we don't want to
      // enter in edition
      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext != null )
      {
        var dataItem = dataGridContext.GetItemFromContainer( this );
        if( dataItem == null )
          return;
      }

      Debug.Assert( this.IsContainerPrepared, "Can't edit a cell that has not been prepared." );

      var parentRow = this.ParentRow;

      try
      {
        if( !parentRow.IsBeingEdited )
        {
          // Prevent the Row Beginning and Begun events from being raised.  The cell will handle them in order
          // to maintain the same logical order as in other scenarios.
          parentRow.IsBeginningEditFromCell = true;

          CancelRoutedEventArgs rowCancelEventArgs = new CancelRoutedEventArgs( Row.EditBeginningEvent, parentRow );
          parentRow.OnEditBeginning( rowCancelEventArgs );

          if( rowCancelEventArgs.Cancel )
            throw new DataGridException( "BeginEdit was canceled." );
        }

        CancelRoutedEventArgs e = new CancelRoutedEventArgs( Cell.EditBeginningEvent, this );
        this.OnEditBeginning( e );

        if( e.Cancel )
          throw new DataGridException( "BeginEdit was canceled." );

        if( !this.IsCurrent )
        {
          dataGridContext.SetCurrent(
            dataGridContext.GetItemFromContainer( parentRow ),
            parentRow,
            DataGridVirtualizingPanel.GetItemIndex( parentRow ),
            this.ParentColumn,
            false,
            true,
            dataGridContext.DataGridControl.SynchronizeSelectionWithCurrent,
            AutoScrollCurrentItemSourceTriggers.Editing );
        }

        if( ( !parentRow.IsBeingEdited ) && ( !parentRow.IsBeginningEdition ) )
        {
          parentRow.BeginEdit();
        }

        this.SetIsBeingEdited( true );
        var cellState = this.GetEditCachedState();

        if( cellState != null )
        {
          if( cellState.ContentBeforeCellEdition == DependencyProperty.UnsetValue )
          {
            cellState.SetContentBeforeCellEdition( this.Content );
          }

          cellState.SetIsDirtyBeforeEdition( this.IsDirty );
        }

        this.OnEditBegun();

        if( parentRow.IsBeginningEditFromCell )
        {
          parentRow.OnEditBegun();
        }
      }
      finally
      {
        parentRow.IsBeginningEditFromCell = false;
      }
    }

    public void EndEdit()
    {
      if( !this.IsBeingEdited )
        return;

      var dataGridUpdateSourceTrigger = this.GetContentBindingUpdateSourceTrigger();
      var updateContentBindingSource = ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellEndingEdit )
                                        || ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellContentChanged );

      this.EndEdit( true, true, updateContentBindingSource );
    }

    public void CancelEdit()
    {
      if( !this.IsBeingEdited )
        return;

      this.OnEditCanceling();

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      //There is an identified weakness with the IsKeyboardFocusWithin property where it cannot tell if the focus is within a Popup which is within the element
      //This has been identified, and only the places where it caused problems were fixed... This comment is only here to remind developpers of the flaw
      if( ( !this.IsKeyboardFocused ) && ( this.IsKeyboardFocusWithin ) )
      {
        // We delay the set focus on the cell to prevent the lost focus from being raised for the editor.
        // This is to prevent execution of some validation code that may be in the LostFocus of the editor.
        if( dataGridContext != null )
        {
          dataGridContext.DelayBringIntoViewAndFocusCurrent( AutoScrollCurrentItemSourceTriggers.Editing );
        }
      }

      CellState cellState = this.GetEditCachedState();

      Debug.Assert( cellState != null );

      if( cellState != null )
      {
        Debug.Assert( ( cellState.ContentBeforeCellEdition != DependencyProperty.UnsetValue ) && ( cellState.IsDirtyBeforeEdition.HasValue ), "It seems the column was recreated or the cellstate overwritten while the row/cell was in edition." );

        if( cellState.ContentBeforeCellEdition != DependencyProperty.UnsetValue )
        {

          // Prevent ValidateAndSetAllErrors since we will call it later.
          this.PreventValidateAndSetAllErrors = true;

          try
          {
            Debug.Assert( !this.GetInheritedReadOnly() );
            this.Content = cellState.ContentBeforeCellEdition;
          }
          finally
          {
            this.PreventValidateAndSetAllErrors = false;
          }

          var dataGridUpdateSourceTrigger = this.GetContentBindingUpdateSourceTrigger();
          var updateContentBindingSource = ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellEndingEdit )
                                           || ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellContentChanged );

          Exception exception;
          CellValidationRule ruleInError;
          this.ValidateAndSetAllErrors( true, true, updateContentBindingSource, true, out exception, out ruleInError );

          cellState.SetContentBeforeCellEdition( DependencyProperty.UnsetValue );
        }

        if( cellState.IsDirtyBeforeEdition.HasValue )
        {
          this.SetIsDirty( cellState.IsDirtyBeforeEdition.Value );
          cellState.SetIsDirtyBeforeEdition( null );
        }
      }

      this.SetIsBeingEdited( false );
      this.OnEditCanceled();
    }

    protected virtual void OnEditBeginning( CancelRoutedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    protected virtual void OnEditBegun()
    {
      RoutedEventArgs e = new RoutedEventArgs( Cell.EditBegunEvent, this );
      this.RaiseEvent( e );
    }

    protected virtual void OnEditEnding( CancelRoutedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    protected virtual void OnEditEnded()
    {
      RoutedEventArgs e = new RoutedEventArgs( Cell.EditEndedEvent, this );
      this.RaiseEvent( e );
    }

    protected virtual void OnEditCanceling()
    {
      RoutedEventArgs e = new RoutedEventArgs( Cell.EditCancelingEvent, this );
      this.RaiseEvent( e );
    }

    protected virtual void OnEditCanceled()
    {
      RoutedEventArgs e = new RoutedEventArgs( Cell.EditCanceledEvent, this );
      this.RaiseEvent( e );
    }

    internal void EndEdit( bool validateCellEditorRules, bool validateUIRules, bool updateContentBindingSource )
    {
      if( !this.IsBeingEdited )
        return;

      try
      {
        var e = new CancelRoutedEventArgs( Cell.EditEndingEvent, this );

        this.OnEditEnding( e );

        // Throwing a DataGridValidationException will be caught by the grid and will make the cell stay in edition.
        if( e.Cancel )
          throw new DataGridValidationException( "EndEdit was canceled." );

        //There is an identified weakness with the IsKeyboardFocusWithin property where it cannot tell if the focus is within a Popup which is within the element
        //This has been identified, and only the places where it caused problems were fixed... This comment is only here to remind developpers of the flaw
        if( ( !this.IsKeyboardFocused ) && ( this.IsKeyboardFocusWithin ) )
        {
          var dataGridContext = DataGridControl.GetDataGridContext( this );

          if( ( dataGridContext != null ) && ( !dataGridContext.DataGridControl.IsSetFocusInhibited ) )
          {
            // Prevent the focus to make a RequestBringIntoView
            using( this.InhibitMakeVisible() )
            {
              // We want to try to focus the Cell before we continue the EndEdit to ensure  any validation or process in the lost focus is done
              if( !dataGridContext.DataGridControl.SetFocusHelper( this.ParentRow, this.ParentColumn, false, false ) )
                throw new DataGridFocusException( "Unable to set focus on the Cell." );
            }
          }
        }
      }
      catch( Exception exception )
      {
        if( exception is TargetInvocationException )
        {
          exception = exception.InnerException;
        }

        // In the case it's the focus that failed, we don't want to make that a ValidationError since it 
        // must be retained by the editor itself with his own error mechanic.
        //
        // Also, for example, clicking on another cell will trigger the lost focus first and editor 
        // will retain focus without any EndEdit being called and without any error displayed by the grid.  
        // So with that focus exception we make sure the behaviour is uniform.
        if( exception is DataGridFocusException )
          throw;

        this.SetValidationError( new CellValidationError( Cell.CustomCellValidationExceptionValidationRule, this, exception.Message, exception ) );

        // Throwing a DataGridValidationException will be caught by the grid and will make the cell stay in edition.
        throw new DataGridValidationException( "An error occurred while attempting to end the edit process.", exception );
      }

      Exception notUsed;
      CellValidationRule ruleInError;
      var result = this.ValidateAndSetAllErrors( validateCellEditorRules, validateUIRules, updateContentBindingSource, true, out notUsed, out ruleInError );

      var cellState = this.GetEditCachedState();
      if( cellState != null )
      {
        if( result.IsValid )
        {
          cellState.SetContentBeforeCellEdition( DependencyProperty.UnsetValue );
        }

        cellState.SetIsDirtyBeforeEdition( null );
      }

      this.SetIsBeingEdited( false );
      this.OnEditEnded();
    }

    internal FrameworkElement CellEditorBoundControl
    {
      get
      {
        return this.GetCellEditorBoundControl( this );
      }
    }

    private static void Cell_RequestBringIntoView( object sender, RequestBringIntoViewEventArgs e )
    {
      Cell cell = sender as Cell;

      if( cell.PreventMakeVisible )
      {
        e.Handled = true;
      }
    }

    private FrameworkElement GetCellEditorBoundControl( FrameworkElement frameworkElement )
    {
      int childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount( frameworkElement );

      for( int i = 0; i < childrenCount; i++ )
      {
        FrameworkElement child = System.Windows.Media.VisualTreeHelper.GetChild( frameworkElement, i ) as FrameworkElement;

        if( child != null )
        {
          if( child.Name == "PART_CellEditorBoundControl" )
            return child;

          FrameworkElement matchingPart = this.GetCellEditorBoundControl( child );

          if( matchingPart != null )
            return matchingPart;
        }
      }

      return null;
    }

    #endregion EDITION

    #region VALIDATION

    internal ValidationResult ValidateAndSetAllErrors(
      bool validateCellEditorRules,
      bool validateUIRules,
      bool updateContentBindingSource,
      bool cascadeValidate,
      out Exception exception,
      out CellValidationRule ruleInError )
    {
      System.Diagnostics.Debug.Assert( !this.PreventValidateAndSetAllErrors,
        "We should not have called this method while in this state." );

      System.Diagnostics.Debug.Assert( !( cascadeValidate && this.IsInCascadingValidation ),
        "We should never be calling ValidateAndSetAllErrors with a request to start cascade validation if we are currently cascading validation." );

      if( this.IsInCascadingValidation && cascadeValidate )
      {
        throw new DataGridInternalException(
              "We should never be calling ValidateAndSetAllErrors with a request to start cascade validation if we are currently cascading validation.",
              DataGridControl.GetDataGridContext( this ).DataGridControl );
      }

      var result = ValidationResult.ValidResult;
      exception = null;
      ruleInError = null;

      // Validate that the CellEditor isn't in error.
      if( validateCellEditorRules )
      {
        result = this.ValidateCellEditorRules( out exception, out ruleInError );

        // If the CellEditor is in error, it must be shown, no matter what.
        if( !result.IsValid )
        {
          this.SetAllError( result, exception, ruleInError );
          return result;
        }
      }

      // Validate CellValidationRules against the cell's Content property.
      if( validateUIRules )
      {
        result = this.ValidateCellRules( out exception, out ruleInError );
      }

      if( result.IsValid )
      {
        // Only need to update the Content binding source if the cell is dirty or if we are cascading the validation.
        if( ( updateContentBindingSource )
          && ( ( this.IsDirty ) || ( this.IsInCascadingValidation ) ) )
        {
          // Update the Content binding's source and check for errors.
          result = this.UpdateContentBindingSource( out exception, out ruleInError );
        }
        else
        {
          // Just check for errors. We must validate even if the cell isn't dirty since its value might have been in error even before entering edit on the row. 
          // ie: DataErrorInfo or any other non-restrictive validation error.
          result = this.ValidateContentBindingRules( out exception, out ruleInError );
        }
      }

      // Only refresh ValidationError property, HasValidationError property, and the cell's style
      // if if we are in a validating the UI Rules or if there wasn't any UI error before this validation pass.
      if( ( validateUIRules ) || ( !this.HasUIValidationError ) )
      {
        this.SetAllError( result, exception, ruleInError );

        if( cascadeValidate )
        {
          this.CascadeValidation();
        }
      }

      return result;
    }

    internal ValidationResult ValidateCellEditorRules( out Exception validationException, out CellValidationRule ruleInError )
    {
      validationException = null;
      ruleInError = null;

      FrameworkElement cellEditorBoundControl = this.CellEditorBoundControl;

      ValidationResult validationResult =
        Cell.CellEditorErrorValidationRule.Validate( this.Content,
        CultureInfo.CurrentCulture,
        new CellValidationContext( this.GetRealDataContext(), this ),
        cellEditorBoundControl );

      if( !validationResult.IsValid )
      {
        ruleInError = Cell.CellEditorErrorValidationRule;
        validationException = new DataGridException( "An invalid or incomplete value was provided." );
      }

      return validationResult;
    }

    internal void ClearAllErrors()
    {
      this.SetAllError( ValidationResult.ValidResult, null, null );
    }

    private static void OnContentBindingNotifyValidationError( object sender, ValidationErrorEventArgs e )
    {
      Cell cell = ( Cell )sender;

      cell.NotifyContentBindingValidationError();
    }

    internal void NotifyContentBindingValidationError()
    {
      if( this.PreventValidateAndSetAllErrors )
        return;

      // Proceed only if the isn't any UI ValidationRules error flagged since last UI validation pass.
      if( this.HasUIValidationError )
        return;

      var validationError = this.GetContentBindingValidationError();

      // In order to minimize switching of styles when we need to update the Content Binding's source, the SetAllError method will not do anything while it is being prevented.
      // e.g. : When we need to update the ContentBinding's source manually through EndEdit.
      if( validationError != null )
      {
        this.SetAllError( new ValidationResult( false, validationError.ErrorContent ), validationError.Exception,
                          new CellContentBindingValidationRule( validationError.RuleInError ) );
      }
      else
      {
        this.SetAllError( ValidationResult.ValidResult, null, null );
      }
    }

    private void OnContentBindingTargetUpdated( object sender, DataTransferEventArgs e )
    {
      if( e.Property != Cell.ContentProperty )
        return;

      var cell = e.TargetObject as Cell;

      // Under certain circumstances which have not yet been clearly identified, the TargetObject is the ContentPresenter of the ScrollTip.
      // This would raise a NullReferenceException when trying to access cell's members, as the previous cast would return null.
      if( cell == null )
        return;

      var parentRow = cell.ParentRow;

      if( ( cell.IsUpdatingContentBindingSource ) || ( cell.PreventValidateAndSetAllErrors )
        || ( parentRow == null ) || ( !parentRow.IsBeingEdited ) || ( !parentRow.IsDirty ) )
      {
        return;
      }

      // This method is called to make sure we are not calling ValidateAndSetAllErrors at all nor triggering a cascade validation
      // as a side-effect of updating a cell's content binding source while another cell is bound to the very same source.
      // We would be surprised of such a usage, but this fail-safe will take care of this possibility.
      if( cell.GetIsSiblingUpdatingContentBindingSource() )
        return;

      Exception exception;
      CellValidationRule ruleInError;
      cell.ValidateAndSetAllErrors( false, true, false, true, out exception, out ruleInError );
    }

    private void CascadeValidation()
    {
      var parentRow = this.ParentRow;

      if( parentRow == null )
        return;

      var dataGridUpdateSourceTrigger = this.GetContentBindingUpdateSourceTrigger();
      var updateContentBindingSource = ( parentRow.IsEndingEdition || parentRow.IsCancelingEdition )
                                        || ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellEndingEdit )
                                        || ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellContentChanged );

      // Create a clone of the list to avoid concurrent access when iterating and a Cell is added to CreatedCells because of ColumnVirtualization
      var createdCells = new List<Cell>( parentRow.CreatedCells );

      foreach( Cell siblingCell in createdCells )
      {
        if( ( siblingCell == this ) || ( siblingCell.ReadOnly ) || ( siblingCell.IsBeingEdited ) )
          continue;

        CellValidationError siblingValidationError = siblingCell.ValidationError;

        if( ( siblingValidationError == null ) || ( Cell.GetIsCellEditorError( siblingValidationError ) ) )
          continue;

        siblingCell.IsInCascadingValidation = true;
        try
        {
          Exception exception;
          CellValidationRule ruleInError;
          siblingCell.ValidateAndSetAllErrors( false, true, updateContentBindingSource, false, out exception, out ruleInError );
        }
        finally
        {
          siblingCell.IsInCascadingValidation = false;
        }
      }
    }

    private void SetAllError( ValidationResult result, Exception validationException, CellValidationRule ruleInError )
    {
      bool invalid = ( !result.IsValid );

      if( invalid )
      {
        CellValidationError validationError = new CellValidationError( ruleInError, this, result.ErrorContent, validationException );

        this.SetValidationError( validationError );
      }
      else
      {
        this.SetValidationError( null );
      }
    }

    private ValidationResult ValidateCellRules( out Exception validationException, out CellValidationRule ruleInError )
    {
      ValidationResult result = ValidationResult.ValidResult;
      ruleInError = null;
      validationException = null;

      CellValidationContext cellValidationContext = new CellValidationContext( this.GetRealDataContext(), this );

      CultureInfo culture = this.Language.GetSpecificCulture();

      foreach( CellValidationRule cellValidationRule in this.CellValidationRules )
      {
        try
        {
          result = cellValidationRule.Validate( this.Content, culture, cellValidationContext );
        }
        catch( Exception exception )
        {
          validationException = exception;
          result = new ValidationResult( false, exception.Message );
        }

        if( !result.IsValid )
        {
          ruleInError = cellValidationRule;
          break;
        }
      }

      ColumnBase parentColumn = this.ParentColumn;

      if( ( parentColumn != null ) && ( result.IsValid ) )
      {
        foreach( CellValidationRule cellValidationRule in parentColumn.CellValidationRules )
        {
          try
          {
            result = cellValidationRule.Validate( this.Content, culture, cellValidationContext );
          }
          catch( Exception exception )
          {
            validationException = exception;
            result = new ValidationResult( false, exception.Message );
          }

          if( !result.IsValid )
          {
            ruleInError = cellValidationRule;
            break;
          }
        }
      }

      return result;
    }

    private ValidationResult ValidateContentBindingRules( out Exception validationException, out CellValidationRule ruleInErrorWrapper )
    {
      validationException = null;
      ruleInErrorWrapper = null;

      ValidationError cellContentBindingValidationError = this.GetContentBindingValidationError();

      if( cellContentBindingValidationError == null )
        return ValidationResult.ValidResult;

      ruleInErrorWrapper = new CellContentBindingValidationRule( cellContentBindingValidationError.RuleInError );
      validationException = cellContentBindingValidationError.Exception;

      return new ValidationResult( false, cellContentBindingValidationError.ErrorContent );
    }

    private ValidationError GetContentBindingValidationError()
    {
      BindingExpressionBase contentBindingExpression = this.GetContentBindingExpression();

      return ( contentBindingExpression != null ) ? contentBindingExpression.ValidationError : null;
    }

    private delegate void ClearAllErrorsDelegate();

    #endregion VALIDATION

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      if( managerType == typeof( PropertyChangedEventManager ) )
      {
        var column = sender as ColumnBase;
        if( ( column != null ) && ( column == this.ParentColumn ) )
        {
          this.OnParentColumnPropertyChanged( ( PropertyChangedEventArgs )e );
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    internal virtual void OnParentColumnPropertyChanged( PropertyChangedEventArgs e )
    {
      if( !this.IsContainerPrepared || this.IsContainerVirtualized )
        return;

      var propertyName = e.PropertyName;

      if( string.IsNullOrEmpty( propertyName ) )
      {
        if( !this.IsCellEditorDisplayed && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) )
        {
          this.UpdateCoercedContentTemplate( false );
        }

        this.UpdateFocusable();
        this.UpdateMatchingDisplayConditions();
      }
      else if( propertyName == ColumnBase.CanBeCurrentWhenReadOnlyProperty.Name )
      {
        this.UpdateFocusable();
      }
      else if( propertyName == ColumnBase.CellEditorDisplayConditionsProperty.Name )
      {
        this.UpdateMatchingDisplayConditions();
      }
      else if( ( propertyName == ColumnBase.CellContentTemplateProperty.Name )
            || ( propertyName == ColumnBase.CellContentTemplateSelectorProperty.Name ) )
      {
        if( !this.IsCellEditorDisplayed && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) )
        {
          if( ( this.ContentTemplateInternal == null ) && ( this.ContentTemplateSelectorInternal == null ) )
          {
            this.UpdateCoercedContentTemplate( false );
          }
        }
      }
      else if( ( propertyName == ColumnBase.CellContentStringFormatProperty.Name )
            || ( propertyName == ColumnBase.DefaultCultureProperty.Name )
            || ( propertyName == Column.ForeignKeyConfigurationProperty.Name ) )
      {
        if( !this.IsCellEditorDisplayed && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) )
        {
          this.UpdateCoercedContentTemplate( false );
        }
      }
      else if( propertyName == ColumnBase.CellEditorSelectorProperty.Name )
      {
        //Make sure the editor is refreshed even if it is currently displayed.
        m_forceEditorRefresh = true;
        this.RefreshDisplayedTemplate();
        m_forceEditorRefresh = false;
      }
    }

    #endregion

    private KeyActivationGesture m_keyGesture; // = null
    private TextCompositionEventArgs m_textInputArgs; // = null
    private string m_fieldName; // = string.Empty
    private object m_styleBeforeError = DependencyProperty.UnsetValue;

    private RoutedEventHandler m_loadedRoutedEventHandler; // = null;
    private DispatcherOperation m_delayedRefreshErrorStyleDispatcherOperation; // = null
    private DispatcherOperation m_delayedHideEditTemplateDispatcherOperation;

    private BitVector32 m_flags = new BitVector32();
    private bool m_forceEditorRefresh;

    // Binding used by the animated Column reordering feature
    private static Binding ParentColumnTranslationBinding;
    private static Binding ParentColumnIsBeingDraggedBinding;
    private static Binding ParentColumnReorderingDragSourceManagerBinding;

    private static Binding ParentRowCellContentOpacityBinding;
    private ContentPresenter m_cellContentPresenter;

    private int m_preventMakeVisibleCount; // = 0;
    private bool m_readOnlyIsInternalySet;

    #region CellFlags Private Type

    [Flags]
    private enum CellFlags
    {
      IsInternalyInitialized = 1,
      IsContainerPrepared = 2,
      FocusEditor = 4,
      IsBeingEdited = 8,
      IsCellEditorDisplayed = 16,
      PreventValidateAndSetAllErrors = 32,
      HasCellEditorError = 64,
      HasUIValidationError = 128,
      HasContentBindingValidationError = 256,
      HasPendingErrorStyleRefresh = 512,
      IsErrorStyleApplied = 1024,
      HasPendingSyncParentErrorFlags = 2048,
      IsUpdatingContentBindingSource = 4096,
      IsInCascadingValidation = 8192,
      IsRestoringEditionState = 16384,
      IsContainerVirtualized = 32768,
      AnimatedColumnReorderingBindingApplied = 65536,
      PreventMakeVisible = 131072,
      IsContainerRecycled = 262144,
      IsContainerPartiallyCleared = 524288
    }

    #endregion

    #region EditorDisplayState Private Type

    private enum EditorDisplayState
    {
      None = 0,
      PendingShow,
      PendingHide
    }

    #endregion

    #region PreventMakeVisibleDisposable Private Class

    private class PreventMakeVisibleDisposable : IDisposable
    {
      public PreventMakeVisibleDisposable( Cell cell )
      {
        if( cell == null )
          throw new ArgumentNullException( "cell" );

        m_cell = cell;
        m_cell.PreventMakeVisible = true;
        m_cell.m_preventMakeVisibleCount++;
      }

      public void Dispose()
      {
        if( m_cell == null )
          return;

        m_cell.m_preventMakeVisibleCount--;

        if( m_cell.m_preventMakeVisibleCount == 0 )
          m_cell.PreventMakeVisible = false;

        m_cell = null;
      }

      private Cell m_cell;
    }

    #endregion

    #region EditorDataTemplate Private Class

    private sealed class EditorDataTemplate : DataTemplate
    {
      private EditorDataTemplate( DataTemplate viewer, DataTemplate editor )
      {
        Debug.Assert( editor != null );

        m_viewer = viewer;
        m_editor = editor;
      }

      internal DataTemplate Viewer
      {
        get
        {
          return m_viewer;
        }
      }

      internal DataTemplate Editor
      {
        get
        {
          return m_editor;
        }
      }

      internal static EditorDataTemplate Create( DataTemplate viewer, DataTemplate editor )
      {
        var viewerContent = new FrameworkElementFactory( typeof( InnerCellContentPresenter ) );
        viewerContent.SetValue( InnerCellContentPresenter.VisibilityProperty, Visibility.Hidden );
        viewerContent.SetValue( InnerCellContentPresenter.ContentTemplateProperty, viewer );

        var editorContent = new FrameworkElementFactory( typeof( InnerCellContentPresenter ) );
        editorContent.SetValue( InnerCellContentPresenter.ContentTemplateProperty, editor );

        var grid = new FrameworkElementFactory( typeof( Grid ) );
        grid.AppendChild( viewerContent );
        grid.AppendChild( editorContent );

        var template = new EditorDataTemplate( viewer, editor );
        template.VisualTree = grid;
        template.Seal();

        return template;
      }

      private readonly DataTemplate m_viewer;
      private readonly DataTemplate m_editor;
    }

    #endregion
  }
}
