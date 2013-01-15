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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Utils.Wpf;
using Xceed.Wpf.DataGrid.Automation;
using Xceed.Wpf.DataGrid.ValidationRules;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_CellContentPresenter", Type = typeof( ContentPresenter ) )]
  public class Cell : ContentControl, INotifyPropertyChanged, IWeakEventListener
  {
    // This validation rule is meant to be used as the rule in error set in a Cell's ValidationError when the CellEditor's HasError attached property returns true.
    private static readonly CellEditorErrorValidationRule CellEditorErrorValidationRule = new CellEditorErrorValidationRule();

    // This validation rule is meant to be used as the rule in error set in a Cell's ValidationError when
    // we want to flag a validation error even though no binding's ValidationRules or CellValidationRules are invalid.
    // ie: Cell EditEnding event throws or returns with e.Cancel set to True.
    private static readonly PassthroughCellValidationRule CustomCellValidationExceptionValidationRule = new PassthroughCellValidationRule( new ExceptionValidationRule() );

    static Cell()
    {
      Cell.ContentProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata(
          new PropertyChangedCallback( OnContentChanged ),
          new CoerceValueCallback( OnCoerceContent ) ) );

      //This configures the TAB key to go trough the controls of the cell once... then continue with other controls
      KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );

      //This configures the directional keys to go trough the controls of the cell once... then continue with other controls
      KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );

      ContentControl.HorizontalContentAlignmentProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata( HorizontalAlignment.Stretch ) );

      ContentControl.VerticalContentAlignmentProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata( VerticalAlignment.Stretch ) );

      Cell.ContentTemplateProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( OnContentTemplateChanged ) ) );

      Cell.ContentTemplateSelectorProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( OnContentTemplateSelectorChanged ) ) );

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
      Cell.IsDirtyFromInitializingInsertionRowProperty = Cell.IsDirtyFromInitializingInsertionRowPropertyKey.DependencyProperty;

      // We do this last to ensure all the Static content of the cell is done before using 
      // any static read-only field of the row.
      Cell.RowDisplayEditorMatchingConditionsProperty =
        Row.RowDisplayEditorMatchingConditionsProperty.AddOwner( typeof( Cell ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( OnMatchingDisplayEditorChanged ) ) );

      Cell.CellEditorContextProperty = Cell.CellEditorContextPropertyKey.DependencyProperty;

      // Animated Column reordering Binding
      Cell.ParentColumnTranslationBinding = new Binding();
      Cell.ParentColumnTranslationBinding.Path =
        new PropertyPath( "ParentColumn.(0)",
                          ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty );
      Cell.ParentColumnTranslationBinding.Mode = BindingMode.OneWay;
      Cell.ParentColumnTranslationBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      Cell.ParentColumnIsBeingDraggedBinding = new Binding();
      Cell.ParentColumnIsBeingDraggedBinding.Path =
        new PropertyPath( "ParentColumn.(0)",
                          TableflowView.IsBeingDraggedAnimatedProperty );
      Cell.ParentColumnIsBeingDraggedBinding.Mode = BindingMode.OneWay;
      Cell.ParentColumnIsBeingDraggedBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      Row.IsTemplateCellProperty.OverrideMetadata( typeof( Cell ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( Cell.OnIsTemplateCellChanged ) ) );

      Cell.ParentRowCellContentOpacityBinding = new Binding();
      Cell.ParentRowCellContentOpacityBinding.Mode = BindingMode.OneWay;
      Cell.ParentRowCellContentOpacityBinding.RelativeSource = new RelativeSource( RelativeSourceMode.TemplatedParent );
      Cell.ParentRowCellContentOpacityBinding.Path = new PropertyPath( "(0).(1)", Cell.ParentRowProperty, Row.CellContentOpacityProperty );

      EventManager.RegisterClassHandler( typeof( Cell ), FrameworkElement.RequestBringIntoViewEvent,
        new RequestBringIntoViewEventHandler( Cell.Cell_RequestBringIntoView ) );
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

      Validation.AddErrorHandler( this, Cell.ContentBinding_ValidationError );
      this.TargetUpdated += new EventHandler<DataTransferEventArgs>( this.ContentBinding_TargetUpdated );
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

    public static readonly DependencyProperty ReadOnlyProperty =
        DataGridControl.ReadOnlyProperty.AddOwner( typeof( Cell ) );

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

    #region IsSelected Read-only Property

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

    #endregion IsSelected Property

    #region IsBeingEdited Read-Only Property

    private static readonly DependencyPropertyKey IsBeingEditedPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsBeingEdited", typeof( bool ), typeof( Cell ), new PropertyMetadata( false ) );

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
          cellState.SetIsBeingEdited( value );

        if( !m_parentRow.IsClearingContainer )
        {
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
    }

    #endregion IsBeingEdited Read-Only Property

    #region IsDirty Read-Only Property

    private static readonly DependencyPropertyKey IsDirtyPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsDirty", typeof( bool ), typeof( Cell ), new PropertyMetadata( false, new PropertyChangedCallback( OnIsDirtyChanged ) ) );

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
          cellState.SetIsDirty( value );
      }
    }

    #endregion IsDirty Read-Only Property

    #region IsDirtyFromInitializingInsertionRow Read-Only Property

    private static readonly DependencyPropertyKey IsDirtyFromInitializingInsertionRowPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsDirtyFromInitializingInsertionRow", typeof( bool ), typeof( Cell ), new PropertyMetadata( false ) );

    internal static readonly DependencyProperty IsDirtyFromInitializingInsertionRowProperty;

    internal bool IsDirtyFromInitializingInsertionRow
    {
      get
      {
        return ( bool )this.GetValue( Cell.IsDirtyFromInitializingInsertionRowProperty );
      }
    }

    internal void SetIsDirtyFromInitializingInsertionRow( bool value )
    {
      if( value != this.IsDirtyFromInitializingInsertionRow )
      {
        if( value )
        {
          this.SetValue( Cell.IsDirtyFromInitializingInsertionRowPropertyKey, value );
        }
        else
        {
          this.SetValue( Cell.IsDirtyFromInitializingInsertionRowPropertyKey, DependencyProperty.UnsetValue );
        }
      }
    }

    #endregion IsDirtyFromInitializingInsertionRow Read-Only Property

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

    public static readonly DependencyProperty CurrentBackgroundProperty =
      DependencyProperty.Register( "CurrentBackground",
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

    public static readonly DependencyProperty CellErrorStyleProperty =
      DataGridControl.CellErrorStyleProperty.AddOwner( typeof( Cell ) );

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

    private static readonly DependencyPropertyKey HasValidationErrorPropertyKey =
      DependencyProperty.RegisterReadOnly(
      "HasValidationError",
      typeof( bool ),
      typeof( Cell ),
      new UIPropertyMetadata(
        false,
        new PropertyChangedCallback( Cell.OnHasValidationErrorChanged ) ) );

    public static readonly DependencyProperty HasValidationErrorProperty =
      HasValidationErrorPropertyKey.DependencyProperty;

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
      // It is imperative to take care of resetting all the flags used by the DelayedRefreshErrorStyle architecture in the
      // Cell's ClearContainer method though.
      if( ( cell.ParentRow != null ) && ( cell.ParentRow.IsClearingContainer ) )
        return;

      // This flag is used to optimize the synchronization of error flags on the parent row/column so that
      // it is done only once at the end of the UpdateHasErrorFlags method.
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

      // Push on the dispatcher so that we prevent multiple affectation of Style when the binding is re-evaluated.
      // Re-evaluating a binding causes it to clear all errors, then validate and finally sets the errors.
      // By using the following strategy, we make sure of not affecting Style when it will change right afterward.
      cell.m_delayedRefreshErrorStyleDispatcherOperation = cell.Dispatcher.BeginInvoke(
        DispatcherPriority.Render, new Action( cell.DelayedRefreshErrorStyle ) );
    }

    private void DelayedRefreshErrorStyle()
    {
      m_delayedRefreshErrorStyleDispatcherOperation = null;
      System.Diagnostics.Debug.Assert( this.HasPendingErrorStyleRefresh, "The operation should have been aborted." );
      System.Diagnostics.Debug.Assert( ( this.ParentRow != null ) && ( !this.ParentRow.IsClearingContainer ), "The operation should have been aborted." );

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

    private static readonly DependencyPropertyKey IsValidationErrorRestrictivePropertyKey =
        DependencyProperty.RegisterReadOnly( "IsValidationErrorRestrictive", typeof( bool ), typeof( Cell ), new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsValidationErrorRestrictiveProperty =
      Cell.IsValidationErrorRestrictivePropertyKey.DependencyProperty;

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

    public static readonly RoutedEvent ValidationErrorChangingEvent = EventManager.RegisterRoutedEvent( "ValidationErrorChanging", RoutingStrategy.Bubble, typeof( CellValidationErrorRoutedEventHandler ), typeof( Cell ) );

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

    private static readonly DependencyPropertyKey ValidationErrorPropertyKey =
        DependencyProperty.RegisterReadOnly( "ValidationError", typeof( CellValidationError ), typeof( Cell ),
        new FrameworkPropertyMetadata(
          null,
          new PropertyChangedCallback( Cell.OnValidationErrorChanged ),
          new CoerceValueCallback( Cell.OnCoerceValidationError ) ) );

    public static readonly DependencyProperty ValidationErrorProperty =
      ValidationErrorPropertyKey.DependencyProperty;

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

      // Update the flags telling us wether or not the current validation error is CellEditor error, or a UI error and
      // update the HasValidationError dependency property.
      cell.UpdateHasErrorFlags( newCellValidationError );

      // Refresh which content template is displayed (editTemplate or not) in case there was a CellEditor validation error on the cell
      // and now there isn't one anymore.
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
            column.SetHasValidationError( hasValidationError );

          Row row = this.ParentRow;

          if( row != null )
            row.UpdateHasErrorFlags( this );
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
          cellState.SetCellValidationError( value );
      }
    }

    #endregion ValidationError Read-Only Property

    #region RowDisplayEditorMatchingConditions Property

    private static readonly DependencyProperty RowDisplayEditorMatchingConditionsProperty;

    #endregion RowDisplayEditorMatchingConditions Property

    #region CellDisplayEditorMatchingConditions Property

    private static readonly DependencyProperty CellDisplayEditorMatchingConditionsProperty =
        DependencyProperty.Register( "CellDisplayEditorMatchingConditions", typeof( CellEditorDisplayConditions ), typeof( Cell ), new UIPropertyMetadata( CellEditorDisplayConditions.None, new PropertyChangedCallback( OnMatchingDisplayEditorChanged ) ) );

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
      "ParentCell", typeof( Cell ), typeof( Cell ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    public static readonly DependencyProperty ParentCellProperty;

    private void SetParentCell()
    {
      this.SetValue( Cell.ParentCellPropertyKey, this );
    }

    #endregion ParentCell Property

    #region ParentColumn Read-Only Property

    private static readonly DependencyPropertyKey ParentColumnPropertyKey =
        DependencyProperty.RegisterReadOnly(
          "ParentColumn", typeof( ColumnBase ), typeof( Cell ),
          new PropertyMetadata( null ) );

    public static readonly DependencyProperty ParentColumnProperty;

    public ColumnBase ParentColumn
    {
      get
      {
        return m_parentColumn;
      }
    }

    private void SetParentColumn( ColumnBase value )
    {
      if( value == null )
        throw new ArgumentNullException( "value" );

      // There is nothing to do if the parent column is the same.
      if( value == m_parentColumn )
        return;

      // Unregister from the old Parent Column's event.
      if( m_parentColumn != null )
      {
        CellEditorDisplayConditionsChangedEventManager.RemoveListener( m_parentColumn, this );
        CellContentTemplateChangedEventManager.RemoveListener( m_parentColumn, this );
        CanBeCurrentWhenReadOnlyChangedEventManager.RemoveListener( m_parentColumn, this );

        var editableParentColumn = m_parentColumn as Column;
        if( editableParentColumn != null )
        {
          ForeignKeyConfigurationChangedEventManager.RemoveListener( editableParentColumn, this );
        }
      }

      m_parentColumn = value;
      this.SetValue( Cell.ParentColumnPropertyKey, value );

      this.UpdateAnimatedColumnReorderingBindings( Row.GetIsTemplateCell( this ) );

      // Register to the new Parent Column's event.
      if( m_parentColumn != null )
      {
        CellEditorDisplayConditionsChangedEventManager.AddListener( m_parentColumn, this );
        CellContentTemplateChangedEventManager.AddListener( m_parentColumn, this );
        CanBeCurrentWhenReadOnlyChangedEventManager.AddListener( m_parentColumn, this );

        var editableParentColumn = m_parentColumn as Column;
        if( editableParentColumn != null )
        {
          ForeignKeyConfigurationChangedEventManager.AddListener( editableParentColumn, this );
        }
      }
    }

    #endregion ParentColumn Read-Only Property

    #region ParentRow Read-Only Property

    private static readonly DependencyPropertyKey ParentRowPropertyKey =
        DependencyProperty.RegisterReadOnly( "ParentRow", typeof( Row ), typeof( Cell ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty ParentRowProperty;

    public Row ParentRow
    {
      get
      {
        return m_parentRow;
      }
    }

    private void SetParentRow( Row value )
    {
      if( value == null )
        throw new ArgumentNullException( "value" );

      // There is nothing to do if the parent row is the same.
      if( value == m_parentRow )
        return;

      m_parentRow = value;
      this.SetValue( Cell.ParentRowPropertyKey, value );
    }

    private Row m_parentRow; // local cache to speed up DP's Read property operations

    #endregion ParentRow Read-Only Property

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
      }
    }

    internal void SetFieldName( string fieldName )
    {
      m_fieldName = fieldName;
    }

    #endregion FieldName Property

    #region CoercedContentTemplate Property

    private static readonly DependencyPropertyKey CoercedContentTemplatePropertyKey
        = DependencyProperty.RegisterReadOnly( "CoercedContentTemplate", typeof( DataTemplate ), typeof( Cell ),
            new FrameworkPropertyMetadata( ( DataTemplate )null ) );

    public static readonly DependencyProperty CoercedContentTemplateProperty;

    public DataTemplate CoercedContentTemplate
    {
      get
      {
        return m_coercedContentTemplateCache;
      }
    }

    private void SetCoercedContentTemplate( DataTemplate viewerTemplate, DataTemplate editTemplate )
    {
      if( ( viewerTemplate != m_coercedContentTemplateCache ) || ( editTemplate != m_coercedContentEditorTemplateCache ) )
      {
        m_coercedContentTemplateCache = viewerTemplate;
        m_coercedContentEditorTemplateCache = editTemplate;

        if( editTemplate != null )
        {
          DataTemplate combineTemplate = new DataTemplate();
          FrameworkElementFactory grid = new FrameworkElementFactory( typeof( Grid ) );

          FrameworkElementFactory viewerContent = new FrameworkElementFactory( typeof( InnerCellContentPresenter ) );
          viewerContent.SetValue( InnerCellContentPresenter.VisibilityProperty, Visibility.Hidden );
          viewerContent.SetValue( InnerCellContentPresenter.ContentTemplateProperty, viewerTemplate );

          FrameworkElementFactory editorContent = new FrameworkElementFactory( typeof( InnerCellContentPresenter ) );
          editorContent.SetValue( InnerCellContentPresenter.ContentTemplateProperty, editTemplate );

          grid.AppendChild( viewerContent );
          grid.AppendChild( editorContent );

          combineTemplate.VisualTree = grid;
          this.SetValue( CoercedContentTemplatePropertyKey, combineTemplate );
        }
        else
        {
          this.SetValue( CoercedContentTemplatePropertyKey, viewerTemplate );
        }
      }
    }

    private DataTemplate m_coercedContentTemplateCache;
    private DataTemplate m_coercedContentEditorTemplateCache;

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

    #region ParentColumnIsBeingDragged Property

    internal static readonly DependencyProperty ParentColumnIsBeingDraggedProperty = DependencyProperty.Register(
      "ParentColumnIsBeingDragged",
      typeof( bool ),
      typeof( Cell ),
      new FrameworkPropertyMetadata(
        ( bool )false,
        new PropertyChangedCallback( Cell.OnParentColumnIsBeingDraggedChanged ),
        new CoerceValueCallback( Cell.OnCoerceParentColumnIsBeingDragged ) ) );

    internal bool ParentColumnIsBeingDragged
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
      Cell cell = sender as Cell;

      if( cell == null )
        return;

      Row row = cell.ParentRow;

      if( row == null )
        return;

      FixedCellPanel fixedCellPanel = row.CellsHostPanel as FixedCellPanel;

      if( fixedCellPanel == null )
        return;

      if( ( bool )e.NewValue )
      {
        fixedCellPanel.ChangeCellZOrder( cell, fixedCellPanel.Children.Count );
      }
      else
      {
        fixedCellPanel.ClearCellZOrder( cell );
      }
    }

    private static object OnCoerceParentColumnIsBeingDragged( DependencyObject sender, object newValue )
    {
      Cell cell = sender as Cell;

      if( cell == null )
        return newValue;

      bool isBeingDraggedAnimated = ( bool )newValue;

      ColumnReorderingDragSourceManager manager =
          TableflowView.GetColumnReorderingDragSourceManager( cell.ParentColumn );

      if( manager != null )
      {
        // Tell the manager to add or remove an adorner for this Cell.
        if( isBeingDraggedAnimated )
        {
          // Never ask for a ghost if the Cell is virtualized.
          // We still call RemoveDraggedColumnGhost to ensure
          // the ghost will be cleared if AutoScroll is enabled
          if( cell.IsContainerPrepared )
          {
            manager.AddDraggedColumnGhost( cell );
          }
        }
        else
        {
          manager.RemoveDraggedColumnGhost( cell );
          // Ensure to reset opacity on the Cell
          cell.ClearValue( Cell.OpacityProperty );
        }
      }

      return newValue;
    }

    #endregion

    #region SelectionBackground Property

    public static readonly DependencyProperty SelectionBackgroundProperty =
      DependencyProperty.Register( "SelectionBackground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush SelectionBackground
    {
      get
      {
        return ( Brush )this.GetValue( Row.SelectionBackgroundProperty );
      }
      set
      {
        this.SetValue( Row.SelectionBackgroundProperty, value );
      }
    }

    #endregion SelectionBackground Property

    #region SelectionForeground Property

    public static readonly DependencyProperty SelectionForegroundProperty =
      DependencyProperty.Register( "SelectionForeground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush SelectionForeground
    {
      get
      {
        return ( Brush )this.GetValue( Row.SelectionForegroundProperty );
      }
      set
      {
        this.SetValue( Row.SelectionForegroundProperty, value );
      }
    }

    #endregion SelectionForeground Property

    #region InactiveSelectionBackground Property

    public static readonly DependencyProperty InactiveSelectionBackgroundProperty =
      DependencyProperty.Register( "InactiveSelectionBackground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush InactiveSelectionBackground
    {
      get
      {
        return ( Brush )this.GetValue( Row.InactiveSelectionBackgroundProperty );
      }
      set
      {
        this.SetValue( Row.InactiveSelectionBackgroundProperty, value );
      }
    }

    #endregion InactiveSelectionBackground Property

    #region InactiveSelectionForeground Property

    public static readonly DependencyProperty InactiveSelectionForegroundProperty =
      DependencyProperty.Register( "InactiveSelectionForeground",
      typeof( Brush ),
      typeof( Cell ),
      new FrameworkPropertyMetadata( null ) );

    public Brush InactiveSelectionForeground
    {
      get
      {
        return ( Brush )this.GetValue( Row.InactiveSelectionForegroundProperty );
      }
      set
      {
        this.SetValue( Row.InactiveSelectionForegroundProperty, value );
      }
    }

    #endregion InactiveSelectionForeground Property

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

    #region CanBeRecycled Property

    protected internal virtual bool CanBeRecycled
    {
      get
      {
        return !this.HasCellEditorError;
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

    #region UpdateContentBindingTargetOnPrepareContainer Property

    protected virtual bool UpdateContentBindingTargetOnPrepareContainer
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
      if( ( this.IsContainerPrepared ) && ( !this.IsContainerVirtualized ) && ( this.VisualChildrenCount > 0 ) )
      {
        UIElement element = this.GetVisualChild( 0 ) as UIElement;

        if( element != null )
        {
          element.Measure( new Size( double.PositiveInfinity, this.ActualHeight ) );
          return element.DesiredSize.Width;
        }
      }

      return -1;
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
              dataGridContext.SetCurrentColumnCore( null, true, false );
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

    protected override void OnMouseEnter( MouseEventArgs e )
    {
      //If the current CellEditorDisplayConditions requires display when mouse is over the Cell 
      if( Cell.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.MouseOverCell ) )
      {
        //Display the editors for the Row
        this.SetDisplayEditorMatchingCondition( CellEditorDisplayConditions.MouseOverCell );
      }

      base.OnMouseEnter( e );
    }

    protected override void OnMouseLeave( MouseEventArgs e )
    {
      //If the current CellEditorDisplayConditions requires display when mouse is over the Cell 
      if( Cell.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.MouseOverCell ) )
      {
        //Display the editors for the Row
        this.RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions.MouseOverCell );
      }

      base.OnMouseLeave( e );
    }

    protected override void OnPreviewTextInput( TextCompositionEventArgs e )
    {
      base.OnPreviewTextInput( e );

      if( e.Handled )
        return;

      Row parentRow = this.ParentRow;

      if( parentRow == null )
        return;

      //Do not try to process the Activation Gesture if the Cell is readonly!
      bool readOnly = this.GetInheritedReadOnly();

      if( readOnly )
        return;

      //This condition ensures that we process the TextInput against the editor only if the Grid is configured to answer to activation gestures.
      if( !( parentRow.IsEditTriggerSet( EditTriggers.ActivationGesture ) ) )
        return;

      CellEditor editor = this.GetCellEditor();

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

    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new CellAutomationPeer( this );
    }

    protected virtual void InitializeCore( DataGridContext dataGridContext, Row parentRow, ColumnBase parentColumn )
    {
      //Initialize the Cell's Properties.
      this.SetParentRow( parentRow );
      this.SetParentColumn( parentColumn );

      if( !this.IsInternalyInitialized )
      {
        Binding rowMatchingConditionsBinding = new Binding();
        rowMatchingConditionsBinding.Path = new PropertyPath( "(0).(1)",
          Cell.ParentRowProperty,
          Row.RowDisplayEditorMatchingConditionsProperty );
        rowMatchingConditionsBinding.Source = this;
        rowMatchingConditionsBinding.Mode = BindingMode.OneWay;

        //Initialize the RowDisplayEditorMatchingConditions binding to the ParentRow
        BindingOperations.SetBinding(
          this, Cell.RowDisplayEditorMatchingConditionsProperty,
          rowMatchingConditionsBinding );

        // Clear the FieldName since it will be taken from the column instead of directly from the Cell.
        this.SetFieldName( string.Empty );
      }
    }

    protected virtual CellEditor GetCellEditor()
    {
      if( !this.IsInternalyInitialized )
        return null;

      ColumnBase parentColumn = this.ParentColumn;

      if( parentColumn == null )
        return null;

      if( parentColumn.CellEditor != null )
        return parentColumn.CellEditor;

      Column column = parentColumn as Column;

      if( column != null )
      {
        // Try to get a CellEditor from the ParentColumn.ForeignKeyConfiguration
        ForeignKeyConfiguration configuration = column.ForeignKeyConfiguration;

        if( configuration != null )
        {
          return configuration.DefaultCellEditor;
        }
      }

      object content = this.Content;

      if( content == null )
        return null;

      Type contentType = content.GetType();

      CellEditor cellEditor = DefaultCellEditorSelector.SelectCellEditor( contentType );

      if( cellEditor != null )
        return cellEditor;

      TypeConverter typeConverter = TypeDescriptor.GetConverter( contentType );

      if( typeConverter == null )
        return null;

      if( typeConverter.CanConvertFrom( typeof( string ) )
        && ( typeConverter.CanConvertTo( typeof( string ) ) ) )
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

      //There is nothing to be done if the cell is being reinitialized with
      //the same parent row and the same parent column.
      if( ( this.IsInternalyInitialized )
        && ( parentColumn == this.ParentColumn )
        && ( parentRow == this.ParentRow ) )
        return;

      //Mark the cell has being recycled to prevent some check to occur.
      this.IsContainerRecycled = this.IsInternalyInitialized;

      //A cell that hasn't been prepare once or that has a new parent column
      //due to recycling needs to be prepared again.
      this.IsContainerPrepared = false;

      this.InitializeCore( dataGridContext, parentRow, parentColumn );

      //Set the Initialized flag to True for the Cell instance.
      this.IsInternalyInitialized = true;

      this.PostInitialize();

      //From here, there is no difference between a fresh new cell and
      //a recycled cell.
      this.IsContainerRecycled = false;
    }

    protected internal virtual void PostInitialize()
    {
      if( this.ParentRow.IsBeingEdited )
      {
        // Cell was added to the row's CreatedCells.  Update the parentColumn's cell in edition state.
        ColumnBase parentColumn = this.ParentColumn;

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
      object dataContext = this.DataContext;

      // If the container is already prepared and not virtualized and the DataContext is the same, just ignore this call.
      // For example, a cell generated through the xaml parser can already be prepared (went through this method once) but
      // still have its DataContext set only after it has been prepared, thus the need to excute this method once more.
      if( ( this.IsContainerPrepared ) && ( !this.IsContainerVirtualized ) &&
        ( ( dataContext == item ) || ( ( dataContext is UnboundDataItem ) && ( this.IsSameUnboundItem( dataContext, item ) ) ) ) )
        return;

      //Make sure that the DataGridContext is set appropriatly on the cells that are already created.
      //This is to ensure that teh value is appropriate when the container is recycled within another DataGridContext ( another detail on the same level ).
      DataGridControl.SetDataGridContext( this, dataGridContext );

      bool isNewDataContext;
      ColumnBase parentColumn = this.ParentColumn;

      Cell.AssignDataContext( this, item, null, parentColumn, out isNewDataContext );

      if( !isNewDataContext && UpdateContentBindingTargetOnPrepareContainer )
      {
        // The DataContext is already this item, update the cell's Content binding target in order for the validation to be once again evaluated.
        this.UpdateContentBindingTarget();
      }

      if( parentColumn != null )
      {
        object cellVerticalContentAlignmentObject = parentColumn.ReadLocalValue( ColumnBase.CellVerticalContentAlignmentProperty );

        // In case we have a BindingExpression, we need to get the resulting value.
        if( cellVerticalContentAlignmentObject is BindingExpressionBase )
        {
          cellVerticalContentAlignmentObject = parentColumn.CellVerticalContentAlignment;
        }

        //If the ParentColumn has value defined
        if( ( cellVerticalContentAlignmentObject != DependencyProperty.UnsetValue ) && ( cellVerticalContentAlignmentObject != null ) )
        {
          this.VerticalContentAlignment = ( VerticalAlignment )cellVerticalContentAlignmentObject;
        }

        object cellHorizontalContentAlignmentObject = parentColumn.ReadLocalValue( ColumnBase.CellHorizontalContentAlignmentProperty );

        // In case we have a BindingExpression, we need to get the resulting value.
        if( cellHorizontalContentAlignmentObject is BindingExpressionBase )
        {
          cellHorizontalContentAlignmentObject = parentColumn.CellHorizontalContentAlignment;
        }

        //If the ParentColumn has value defined
        if( ( cellHorizontalContentAlignmentObject != DependencyProperty.UnsetValue ) && ( cellHorizontalContentAlignmentObject != null ) )
        {
          this.HorizontalContentAlignment = ( HorizontalAlignment )cellHorizontalContentAlignmentObject;
        }
      }

      Row parentRow = this.ParentRow;
      ColumnBase currentColumn = dataGridContext.CurrentColumn;

      // If there is a current column on the DataGridContext, try to restore the currency of the cell
      if( ( parentRow != null ) && ( parentRow.IsCurrent ) && ( currentColumn != null ) && ( currentColumn == parentColumn ) )
      {
        this.SetIsCurrent( true );
      }

      // This will force invalidation of the CoercedContentTemplate, only if the content is null after the prepare
      // and if the content template is set to somethign else than default. However, when binded to an EmptyDataItem,
      // our content will always be null but a CellContentTemplate will be applied. In this case, we do NOT want to 
      // reapplied the template every time.
      if( ( this.Content == null ) && ( !( this.GetRealDataContext() is EmptyDataItem ) ) )
      {
        this.ClearValue( Cell.CoercedContentTemplatePropertyKey );
        m_coercedContentTemplateCache = null;
      }

      //Determine the default CellContentTemplate for the cell.
      this.SetCoercedContentTemplate( this.GetCoercedCellContentTemplate(), null );

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

      // In DP's PropertyChanged Callbacks, for the sake of performance, you could
      // return immediatly if the cell is in process of being cleared.

      // Clear every properties related to the visual state of the Cell
      this.ClearContainerVisualState();

      // We must not clear the IsDirty flag of the Cell here since its
      // Content could have been edited and this flag is checked
      // to determine if the new value is commited or not in the source

      this.ClearValue( Cell.ValidationErrorPropertyKey );

      // No need to clear the Cell.CellDisplayEditorMatchingConditionsProperty
      // neither DataGridControl.CellEditorDisplayConditionsProperty since
      // both will be updated when Cell.PrepareContainer is called again

      if( m_delayedRefreshErrorStyleDispatcherOperation != null )
      {
        System.Diagnostics.Debug.Assert( m_delayedRefreshErrorStyleDispatcherOperation.Status == DispatcherOperationStatus.Pending );
        m_delayedRefreshErrorStyleDispatcherOperation.Abort();
        m_delayedRefreshErrorStyleDispatcherOperation = null;
      }

      if( m_delayedHideEditTemplateDispatcherOperation != null )
      {
        System.Diagnostics.Debug.Assert( m_delayedHideEditTemplateDispatcherOperation.Status == DispatcherOperationStatus.Pending );
        m_delayedHideEditTemplateDispatcherOperation.Abort();
        m_delayedHideEditTemplateDispatcherOperation = null;
      }

      if( m_loadedRoutedEventHandler != null )
      {
        this.Loaded -= m_loadedRoutedEventHandler;
        m_loadedRoutedEventHandler = null;
      }

      this.ClearValue( Cell.HasValidationErrorPropertyKey );
      this.ClearValue( Cell.IsValidationErrorRestrictivePropertyKey );

      m_styleBeforeError = DependencyProperty.UnsetValue;
      this.ClearValue( Cell.StyleProperty );
      this.ClearValue( Cell.IsSelectedPropertyKey );

      // This will reset every flags maintained in m_flags
      this.ResetNonTransientFlags();

      m_cellValidationRules = null;
    }

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      object currentThemeKey = view.GetDefaultStyleKey( typeof( Cell ) );

      if( currentThemeKey.Equals( this.DefaultStyleKey ) == false )
      {
        this.DefaultStyleKey = currentThemeKey;
      }
    }

    protected internal virtual void AddContentBinding()
    {
    }

    protected internal virtual void RemoveContentBinding()
    {
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

    internal static void AssignDataContext( Cell cell, object dataContext, UnboundDataItem unboundDataItemContext, ColumnBase parentColumn, out bool isNewDataContext )
    {
      Column column = parentColumn as Column;

      if( ( column != null ) && ( column.IsBoundToDataGridUnboundItemProperty ) )
      {
        if( unboundDataItemContext == null )
          UnboundDataItem.GetUnboundDataItemNode( dataContext, out unboundDataItemContext );

        dataContext = unboundDataItemContext;
      }

      // Read the LocalValue of the DataContext to avoid
      // getting the one inherited from the ParentRow.
      // This prevent the DataContext to become null
      // when the Cell is virtualized.
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
        isNewDataContext = true;
      }
      else
      {
        isNewDataContext = false;
      }
    }

    //Used to virtualize a Cell, but keep every associations to data item This is used to keep bindings live on the data item to allow ValidationRules to be processed correctly.
    internal void ClearContainerVisualState()
    {
      this.ClearValue( Cell.IsCurrentPropertyKey );
      this.ClearValue( Cell.IsBeingEditedPropertyKey );
      this.SetIsSelected( false );

      this.SetIsCellEditorDisplayed( false );

      this.CurrentEditorPendingDisplayState = EditorDisplayState.None;

      this.IsContainerVirtualized = true;
    }

    //This ensures the CellEditorContext is set on the Cell to avoid problems with RelativeSource causing undesired behaviors when ComboBox is used as default CellEditor
    internal void EnsureCellEditorContext()
    {
      ForeignKeyConfiguration configuration = null;

      Column parentColumn = this.ParentColumn as Column;

      if( parentColumn != null )
      {
        configuration = parentColumn.ForeignKeyConfiguration;
        CellEditorContext context = new CellEditorContext( this.ParentColumn, configuration );
        Cell.SetCellEditorContext( this, context );
      }
    }

    internal void RefreshDisplayedTemplate()
    {
      // Never refresh any templates when the Cell is not prepared
      // or virtualized
      if( !this.IsContainerPrepared || this.IsContainerVirtualized )
        return;

      Row parentRow = this.ParentRow;

      // Never refresh template while clearing container
      if( ( parentRow == null ) || ( parentRow.IsClearingContainer ) )
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
      this.SetIsDirtyFromInitializingInsertionRow( false );
    }

    internal void UpdateContentBindingTarget()
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      // Never update the target of a binding to a an item if the Cell is dirty
      if( ( !this.IsDirty ) && ( !this.IsDirtyFromInitializingInsertionRow ) )
      {
        BindingExpressionBase bindingExpression = this.GetContentBindingExpression();

        if( bindingExpression != null )
          bindingExpression.UpdateTarget();
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

      if( ( savedState.IsBeingEdited ) && ( this.ParentColumn == currentColumn ) )
        this.BeginEdit();


      this.SetValidationError( savedState.CellValidationError );
    }

    internal void OnIsTemplateCellChanged( bool newValue )
    {
      this.UpdateAnimatedColumnReorderingBindings( newValue );
    }

    internal ValidationResult UpdateContentBindingSource( out Exception exception, out CellValidationRule ruleInErrorWrapper )
    {
      Debug.Assert( ( ( this.IsDirty ) || ( this.IsDirtyFromInitializingInsertionRow ) || ( this.IsInCascadingValidation ) ),
        "UpdateContentBindingSource should not be called when the cell isn't dirty beside when cascading validation.  Call ValidateContentBindingRules instead." );

      exception = null;
      ruleInErrorWrapper = null;

      ValidationResult validationResult = ValidationResult.ValidResult;

      BindingExpressionBase contentBindingExpression = this.GetContentBindingExpression();

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

        //Update StatsCells right away.
        this.InvalidateStatsFunctions();
      }

      // The dirty flag will only be lowered when the row ends or cancels edition, or if the cell cancels edition and it wasn't dirty
      // when begining edition.
      return validationResult;
    }

    internal CellState GetEditCachedState()
    {
      CellState cellState = null;

      Row parentRow = this.ParentRow;

      if( ( parentRow != null ) && ( ( parentRow.IsBeingEdited ) || ( m_parentRow.IsBeginningEdition ) ) )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext != null )
        {
          DataGridControl dataGridControl = dataGridContext.DataGridControl;

          if( ( dataGridControl != null ) && ( dataGridControl.CurrentRowInEditionState != null ) )
          {
            ColumnBase parentColumn = this.ParentColumn;

            if( parentColumn != null )
              cellState = parentColumn.CurrentRowInEditionCellState;
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
      if( this.GetInheritedReadOnly() )
        return ParentColumn.CanBeCurrentWhenReadOnly;
      else
        return true;
    }

    internal virtual void ContentCommitted()
    {
    }

    internal virtual void UpdateAnimatedColumnReorderingBindings( bool isTemplateCell )
    {
      // Only set the Binding if the View is a TableflowView
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext != null )
      {
        TableflowView tableflowView = dataGridContext.DataGridControl.GetView() as TableflowView;

        if( tableflowView == null )
          return;
      }

      if( isTemplateCell )
      {
        // No need to enable animated Column reordering when TemplateCell is used
        BindingOperations.ClearBinding( this, Cell.RenderTransformProperty );
        BindingOperations.ClearBinding( this, Cell.ParentColumnIsBeingDraggedProperty );
        this.AnimatedColumnReorderingBindingApplied = false;
      }
      else
      {
        if( !this.AnimatedColumnReorderingBindingApplied )
        {
          // To enable animated Column reordering
          BindingOperations.SetBinding( this, Cell.RenderTransformProperty, Cell.ParentColumnTranslationBinding );
          BindingOperations.SetBinding( this, Cell.ParentColumnIsBeingDraggedProperty, Cell.ParentColumnIsBeingDraggedBinding );
          this.AnimatedColumnReorderingBindingApplied = true;
        }
      }
    }

    internal virtual void EnsureInVisualTree()
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

      TableViewColumnVirtualizationManager columnVirtualizationManager =
        ColumnVirtualizationManager.GetColumnVirtualizationManager( dataGridContext ) as TableViewColumnVirtualizationManager;

      if( columnVirtualizationManager == null )
        return;

      if( columnVirtualizationManager.FixedFieldNames.Contains( this.FieldName ) )
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

    private static object OnCoerceContent( DependencyObject sender, object value )
    {
      Cell cell = sender as Cell;

      // If we are updating the Content binding source and content is refreshed as a result, hold-on.
      // We will manually update Content from the source by calling UpdateTarget on the binding later.
      if( cell.IsUpdatingContentBindingSource )
        return DependencyProperty.UnsetValue;

      return value;
    }

    private static void OnContentChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Cell cell = ( Cell )sender;
      Row parentRow = cell.ParentRow;

      if( parentRow == null )
        return;

      bool doCellValidation = false;
      ColumnBase parentColumn = cell.ParentColumn;

      bool changeDuringRowIsInEdit = ( cell.IsInternalyInitialized )
        && ( !cell.IsContainerRecycled )
        && ( !parentRow.IsBeginningEdition )
        && ( !cell.IsUpdatingContentBindingSource )
        && ( !cell.IsRestoringEditionState )
        && ( parentRow.IsBeingEdited );

      if( ( changeDuringRowIsInEdit ) || ( parentRow.IsInitializingInsertionRow ) )
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

      if( parentRow.IsInitializingInsertionRow )
      {
        Nullable<DataGridUpdateSourceTrigger> dataGridUpdateSourceTrigger = cell.GetContentBindingUpdateSourceTrigger();

        if( ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellContentChanged )
          || ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellEndingEdit ) )
        {
          doCellValidation = true;
        }

        cell.SetIsDirtyFromInitializingInsertionRow( true );
      }
      else if( changeDuringRowIsInEdit )
      {
        if( !object.Equals( e.OldValue, e.NewValue ) )
          cell.SetIsDirty( true );

        if( !cell.PreventValidateAndSetAllErrors )
        {
          Nullable<DataGridUpdateSourceTrigger> dataGridUpdateSourceTrigger = cell.GetContentBindingUpdateSourceTrigger();

          if( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellContentChanged )
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
          Exception exception;
          CellValidationRule ruleInError;



          ValidationResult validationResult = cell.ValidateCellRules( out exception, out ruleInError );

          if( validationResult.IsValid )
            cell.ValidateAndSetAllErrors( false, false, true, true, out exception, out ruleInError );
        }
      }

      //When the content changes, check if the CellContentTemplateSelector needs to be updated.
      //This depdends on several factor, including if a Selector is present and if the editor is not displayed
      if( cell.ShouldInvalidateCellContentTemplateSelector() )
      {
        cell.SetCoercedContentTemplate( cell.GetCoercedCellContentTemplate(), null );
      }
    }

    private static void OnIsDirtyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      if( ( bool )e.NewValue )
      {
        Cell cell = ( Cell )sender;
        //Raise the RoutedEvent that notifies any Row that the cell is becoming dirty.
        RoutedEventArgs eventArgs = new RoutedEventArgs( Cell.IsDirtyEvent );
        cell.RaiseEvent( eventArgs );
      }
    }

    private static void OnMatchingDisplayEditorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Cell obj = ( Cell )sender;

      //performing this check because at the end of the Initializing function, I will call this explicitelly, to ensure 
      //proper display of the Editor if the appropriate conditions are met.
      if( obj.IsInternalyInitialized )
      {
        obj.RefreshDisplayedTemplate();
      }
    }

    private static void OnCellEditorDisplayConditionsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Cell cell = ( Cell )sender;

      // The cell.ParentRow can be null if this Cell is a TemplateCell
      if( ( cell.ParentRow != null ) && cell.ParentRow.IsClearingContainer )
        return;

      cell.UpdateMatchingDisplayConditions();
    }

    private static void OnContentTemplateChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Cell cell = sender as Cell;
      Debug.Assert( cell != null );

      if( cell == null )
        return;

      cell.m_contentTemplateCache = e.NewValue as DataTemplate;

      if( ( cell.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) && ( !cell.IsCellEditorDisplayed ) )
      {
        cell.SetCoercedContentTemplate( cell.GetCoercedCellContentTemplate(), null );
      }
    }

    private static void OnContentTemplateSelectorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Cell cell = sender as Cell;
      Debug.Assert( cell != null );

      if( cell == null )
        return;

      cell.m_contentTemplateSelectorCache = e.NewValue as DataTemplateSelector;

      if( ( cell.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) && ( !cell.IsCellEditorDisplayed ) )
      {
        cell.SetCoercedContentTemplate( cell.GetCoercedCellContentTemplate(), null );
      }
    }

    private static void OnIsTemplateCellChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Cell cell = sender as Cell;

      if( cell != null )
      {
        cell.OnIsTemplateCellChanged( ( bool )e.NewValue );
      }
    }

    private bool GetInheritedReadOnly()
    {
      ColumnBase parentColumn = this.ParentColumn;

      object readOnly;
      readOnly = this.ReadLocalValue( Cell.ReadOnlyProperty );

      // In case we have a BindingExpression, we need to get the resulting value.
      if( readOnly is BindingExpressionBase )
        readOnly = this.ReadOnly;

      if( ( readOnly == DependencyProperty.UnsetValue )
        && ( parentColumn != null ) )
      {
        readOnly = parentColumn.ReadLocalValue( ColumnBase.ReadOnlyProperty );

        // In case we have a BindingExpression, we need to get the resulting value.
        if( readOnly is BindingExpressionBase )
          readOnly = parentColumn.ReadOnly;
      }

      if( readOnly == DependencyProperty.UnsetValue )
        readOnly = this.ReadOnly;

      if( ( bool )readOnly )
        return true;

      BindingExpressionBase contentBindingExpression = this.GetContentBindingExpression();

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

      if( ( contentBindingMode == null ) || ( contentBindingMode == BindingMode.TwoWay ) )
        return false;

      return true;
    }

    private bool ShouldInvalidateCellContentTemplateSelector()
    {
      ColumnBase parentColumn = this.ParentColumn;

      bool onlyInternalContentTemplateSelectorAvailable =
        ( ( this.ContentTemplateInternal == null ) && ( this.ContentTemplateSelectorInternal != null ) );

      bool onlyColumnContentTemplateSelectorAvailable =
        ( ( parentColumn.CellContentTemplate == null ) && ( parentColumn.CellContentTemplateSelector != null ) );

      bool onlyTemplateSelectorAvailable = onlyColumnContentTemplateSelectorAvailable
        || onlyInternalContentTemplateSelectorAvailable;

      bool editorHiddenAndNotPendingShow = !this.IsCellEditorDisplayed
        && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow );

      // Only TemplateSelectors are avaiable and editor is not hidden and pending showing
      return ( onlyTemplateSelectorAvailable && editorHiddenAndNotPendingShow );
    }

    private bool GetIsContentBindingSupportingSourceUpdate()
    {
      BindingExpressionBase binding = this.GetContentBindingExpression();

      if( binding == null )
        return false;

      BindingMode mode;
      Binding singleBinding = binding.ParentBindingBase as Binding;

      if( singleBinding == null )
      {
        MultiBinding multiBinding = binding.ParentBindingBase as MultiBinding;

        if( multiBinding == null )
        {
          return false;
        }
        else
        {
          mode = multiBinding.Mode;
        }
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
      Row parentRow = this.ParentRow;

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
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return null;

      DataTemplate contentTemplate = this.ContentTemplateInternal;

      //If there is a ContentTemplate assigned directly on Cell, use it
      if( contentTemplate != null )
        return contentTemplate;

      object content = this.Content;

      //If none, check if there is a ContentTemplateSelector directly assigned on cell.
      DataTemplateSelector contentTemplateSelector = this.ContentTemplateSelectorInternal;

      if( contentTemplateSelector != null )
      {
        //If there is one, the query it for a ContentTemplate.
        contentTemplate = contentTemplateSelector.SelectTemplate( content, this );
      }

      //If there was no ContentTemplateSelector on Cell or if the output of the selector was Null.
      if( ( !this.OverrideColumnCellContentTemplate ) && ( contentTemplate == null ) )
      {
        ColumnBase parentColumn = this.ParentColumn;

        Column column = parentColumn as Column;

        if( column != null )
        {
          ForeignKeyConfiguration configuration = column.ForeignKeyConfiguration;

          DataTemplate foreignKeyContentTemplate = null;

          // If a foreignKey CellContentTemplate was found by the configuration,
          // it must be used even if a CellContentTemplate is defined because the 
          // CellContentTemplate will be used by this template
          if( configuration != null )
          {
            // Try to get a CellContentTemplate specific for a ForeignKeyConfiguration
            foreignKeyContentTemplate = configuration.DefaultCellContentTemplate;

            if( ( foreignKeyContentTemplate != null )
              && ( this is DataCell ) )
            {
              contentTemplate = foreignKeyContentTemplate;
            }
          }
        }

        if( contentTemplate == null )
        {
          //If the parent Column defines a CellContentTemplate, then use it
          contentTemplate = parentColumn.CellContentTemplate;

          //if it doesn't, then check for a selector on Column
          if( contentTemplate == null )
          {
            contentTemplateSelector = parentColumn.CellContentTemplateSelector;

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
        contentTemplate = Column.GenericContentTemplateSelector.Instance.SelectTemplate( content, this );
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

    private void ShowEditTemplate()
    {
      if( m_delayedHideEditTemplateDispatcherOperation != null )
      {
        System.Diagnostics.Debug.Assert( m_delayedHideEditTemplateDispatcherOperation.Status == DispatcherOperationStatus.Pending );
        m_delayedHideEditTemplateDispatcherOperation.Abort();
        m_delayedHideEditTemplateDispatcherOperation = null;
      }

      // The editor is not pending showing
      if( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow )
      {
        // If it is not displayed or pending hiding
        if( !this.IsCellEditorDisplayed
            || ( this.CurrentEditorPendingDisplayState == EditorDisplayState.PendingHide ) )
        {
          CellEditor cellEditor = this.GetCellEditor();

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

      m_delayedHideEditTemplateDispatcherOperation = this.Dispatcher.BeginInvoke(
        new Action( this.HideEditTemplate ), DispatcherPriority.DataBind );
    }

    private void HideEditTemplate()
    {
      m_delayedHideEditTemplateDispatcherOperation = null;

      // The Cell is not prepared or virtualized, no need to 
      // hide the edit template since it will be refreshed
      // the the Cell is prepared again.
      if( !this.IsContainerPrepared || this.IsContainerVirtualized )
        return;

      if( ( !this.IsCellEditorDisplayed )
          && ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) )
        return;

      this.SetCoercedContentTemplate( this.GetCoercedCellContentTemplate(), null );
      this.CurrentEditorPendingDisplayState = EditorDisplayState.PendingHide;

      // We must clear the CellEditorContext when the Template is hidden
      // to avoid side effects of the CellEditorBinding affecting null
      // in the source. The reason is that the CellEditorContext is the
      // binding source of the default foreign key CellEditor Template
      Cell.ClearCellEditorContext( this );
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

    private void InvalidateStatsFunctions()
    {
      //Make sure to update StatFunctions as soon as cell content changes (this part of the code is executed only if UpdateSourceTrigger is set to CellContentChanged).
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl != null )
      {
        DataGridCollectionView dataGridCollectionView = dataGridControl.ItemsSource as DataGridCollectionView;

        if( dataGridCollectionView != null )
        {
          int currentIndex;

          //If -1 is returned, it means the corresponding data item is not yet in the source (e.g insertion cell), so no stats to update at this point.
          if( ( currentIndex = dataGridCollectionView.IndexOf( this.ParentRow.DataContext ) ) == -1 )
            return;

          RawItem item = dataGridCollectionView.GetRawItemAt( currentIndex );

          DataGridCollectionViewGroup parentGroup = item.ParentGroup;

          if( parentGroup != null )
            dataGridCollectionView.DeferredOperationManager.InvalidateGroupStats( parentGroup );
        }
      }
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
          e.CanExecute = ( !this.IsBeingEdited ) && ( !this.GetInheritedReadOnly() )
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

      this.Dispatcher.BeginInvoke( DispatcherPriority.Render, new Action( this.DelayedRefreshErrorStyle ) );
    }

    private void Cell_LayoutUpdated( object sender, EventArgs e )
    {
      if( !this.IsContainerPrepared || this.IsContainerVirtualized )
        return;

      if( this.CurrentEditorPendingDisplayState == EditorDisplayState.PendingShow )
      {
        //If the Cell is marked to be pending the display of the editor, then
        //this LayoutUpdated means that Editor has been effectively displayed

        // Notify the UIAutomation if any children have changed.
        AutomationPeer automationPeer = UIElementAutomationPeer.FromElement( this );

        if( automationPeer != null )
        {
          automationPeer.ResetChildrenCache();
        }

        //Change the IsCellEditorDisplayed flag accordingly
        this.SetIsCellEditorDisplayed( true );
      }
      else if( this.CurrentEditorPendingDisplayState == EditorDisplayState.PendingHide )
      {
        //If the Cell is marked to be pending the Display of the viewer, then
        //this LayoutUpdated means that Editor has been effectively hidden

        // Notify the UIAutomation if any children have changed.
        AutomationPeer automationPeer = UIElementAutomationPeer.FromElement( this );

        if( automationPeer != null )
        {
          automationPeer.ResetChildrenCache();
        }

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

      bool readOnly = this.GetInheritedReadOnly();

      if( readOnly )
        throw new DataGridException( "An attempt was made to edit a read-only cell or the cell content is not bound using two way binding." );

      // If there is no dataItem mapped to this container, we don't want to
      // enter in edition
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext != null )
      {
        object dataItem = dataGridContext.GetItemFromContainer( this );

        if( dataItem == null )
          return;
      }

      Debug.Assert( this.IsContainerPrepared, "Can't edit a cell that has not been prepared." );

      Row parentRow = this.ParentRow;

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
            parentRow, DataGridVirtualizingPanel.GetItemIndex( parentRow ),
            this.ParentColumn, false, true, dataGridContext.DataGridControl.SynchronizeSelectionWithCurrent );
        }

        if( ( !m_parentRow.IsBeingEdited ) && ( !m_parentRow.IsBeginningEdition ) )
          m_parentRow.BeginEdit();

        this.SetIsBeingEdited( true );
        CellState cellState = this.GetEditCachedState();

        if( cellState != null )
        {
          if( cellState.ContentBeforeCellEdition == DependencyProperty.UnsetValue )
            cellState.SetContentBeforeCellEdition( this.Content );

          cellState.SetIsDirtyBeforeEdition( this.IsDirty );
        }

        this.OnEditBegun();

        if( parentRow.IsBeginningEditFromCell )
          parentRow.OnEditBegun();
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

      bool updateContentBindingSource = this.GetContentBindingUpdateSourceTrigger() == DataGridUpdateSourceTrigger.CellEndingEdit;

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
          dataGridContext.DelayBringIntoViewAndFocusCurrent();
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

          Nullable<DataGridUpdateSourceTrigger> dataGridUpdateSourceTrigger = this.GetContentBindingUpdateSourceTrigger();

          bool updateContentBindingSource =
            ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellEndingEdit ) ||
            ( dataGridUpdateSourceTrigger == DataGridUpdateSourceTrigger.CellContentChanged );

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

      this.SetIsDirtyFromInitializingInsertionRow( false );
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

      CancelRoutedEventArgs e = new CancelRoutedEventArgs( Cell.EditEndingEvent, this );

      try
      {
        this.OnEditEnding( e );

        // Throwing a DataGridValidationException will be caught by the grid and will make the cell stay in edition.
        if( e.Cancel )
          throw new DataGridValidationException( "EndEdit was canceled." );

        //There is an identified weakness with the IsKeyboardFocusWithin property where it cannot tell if the focus is within a Popup which is within the element
        //This has been identified, and only the places where it caused problems were fixed... This comment is only here to remind developpers of the flaw
        if( ( !this.IsKeyboardFocused ) && ( this.IsKeyboardFocusWithin ) )
        {
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

          if( ( dataGridContext != null ) && ( !dataGridContext.DataGridControl.IsSetFocusInhibited ) )
          {
            // Prevent the focus to make a RequestBringIntoView
            using( this.InhibitMakeVisible() )
            {
              // We want to try to focus the Cell before we continue the EndEdit to ensure 
              // any validation or process in the lost focus is done
              if( !dataGridContext.DataGridControl.SetFocusHelper( m_parentRow, m_parentColumn, false, false ) )
                throw new DataGridFocusException( "Unable to set focus on the Cell." );
            }
          }
        }
      }
      catch( Exception exception )
      {
        if( exception is TargetInvocationException )
          exception = exception.InnerException;

        // In the case it's the focus that failed, we don't want to make that a ValidationError since it 
        // must be retained by the editor itself with his own error mechanic.
        //
        // Also, for example, clicking on another cell will trigger the lost focus first and editor 
        // will retain focus without any EndEdit being called and without any error displayed by the grid.  
        // So with that focus exception we make sure the behaviour is uniform.
        if( exception is DataGridFocusException )
          throw;

        this.SetValidationError( new CellValidationError(
          Cell.CustomCellValidationExceptionValidationRule,
          this,
          exception.Message,
          exception ) );

        // Throwing a DataGridValidationException will be caught by the grid and will make the cell stay in edition.
        throw new DataGridValidationException( "An error occurred while attempting to end the edit process.", exception );
      }

      Exception notUsed;
      CellValidationRule ruleInError;

      ValidationResult result =
        this.ValidateAndSetAllErrors( validateCellEditorRules, validateUIRules, updateContentBindingSource, true, out notUsed, out ruleInError );


      CellState cellState = this.GetEditCachedState();

      if( cellState != null )
      {
        if( result.IsValid )
          cellState.SetContentBeforeCellEdition( DependencyProperty.UnsetValue );

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
        throw new DataGridInternalException();

      ValidationResult result = ValidationResult.ValidResult;
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
        result = this.ValidateCellRules( out exception, out ruleInError );


      if( result.IsValid )
      {
        // Only need to update the Content binding source if the cell is dirty or if we are cascading the validation.
        if( ( updateContentBindingSource )
          && ( ( this.IsDirty ) || ( this.IsDirtyFromInitializingInsertionRow ) || ( this.IsInCascadingValidation ) ) )
        {
          // Update the Content binding's source and check for errors.
          result = this.UpdateContentBindingSource( out exception, out ruleInError );
        }
        else
        {
          // Just check for errors.
          // We must validate even if the cell isn't dirty since its value might have been in error even before
          // entering edit on the row. 
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
          this.CascadeValidation();
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

    private static void ContentBinding_ValidationError( object sender, ValidationErrorEventArgs e )
    {
      Cell cell = ( Cell )sender;

      if( cell.PreventValidateAndSetAllErrors )
        return;

      // Proceed only if the isn't any UI ValidationRules error flagged since last UI validation pass.
      if( cell.HasUIValidationError )
        return;

      ValidationError cellContentBindingValidationError = cell.GetContentBindingValidationError();

      // In order to minimize switching of styles when we need to update the Content Binding's source,
      // the SetAllError method will not do anything while it is being prevented.
      //
      // ie: When we need to update the ContentBinding's source manually through EndEdit.
      if( cellContentBindingValidationError != null )
      {
        cell.SetAllError(
          new ValidationResult( false, cellContentBindingValidationError.ErrorContent ),
          cellContentBindingValidationError.Exception,
          new CellContentBindingValidationRule( cellContentBindingValidationError.RuleInError ) );
      }
      else
      {
        cell.SetAllError( ValidationResult.ValidResult, null, null );
      }
    }

    private void ContentBinding_TargetUpdated( object sender, DataTransferEventArgs e )
    {
      if( e.Property != Cell.ContentProperty )
        return;

      Cell cell = e.TargetObject as Cell;

      // Under certain circumstances which have not yet been clearly identified, the TargetObject
      // is the ContentPresenter of the ScrollTip. This would raise a NullReferenceException when
      // trying to access cell's members, as the previous cast would return null.
      if( cell == null )
        return;

      Row parentRow = cell.ParentRow;

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
      Row parentRow = this.ParentRow;

      if( parentRow == null )
        return;

      bool updateContentBindingSource = ( parentRow.IsEndingEdition || parentRow.IsCancelingEdition )
        || ( this.GetContentBindingUpdateSourceTrigger() == DataGridUpdateSourceTrigger.CellEndingEdit )
        || ( this.GetContentBindingUpdateSourceTrigger() == DataGridUpdateSourceTrigger.CellContentChanged );

      // Create a clone of the list to avoid concurrent access when iterating 
      // and a Cell is added to CreatedCells because of ColumnVirtualization
      List<Cell> createdCells = new List<Cell>( parentRow.CreatedCells );

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
        CellValidationError validationError =
          new CellValidationError( ruleInError, this, result.ErrorContent, validationException );

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

      CellValidationContext cellValidationContext =
        new CellValidationContext( this.GetRealDataContext(), this );

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

    private void OnPropertyChanged( string propertyName )
    {
      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      bool retval = false;

      if( managerType == typeof( CanBeCurrentWhenReadOnlyChangedEventManager ) )
      {
        ColumnBase column = sender as ColumnBase;

        if( this.ProcessEvent( column, out retval ) )
        {
          this.OnParentColumnCanBeCurrentWhenReadOnlyChanged( column );
        }
      }
      else if( managerType == typeof( CellEditorDisplayConditionsChangedEventManager ) )
      {
        ColumnBase column = sender as ColumnBase;

        if( this.ProcessEvent( column, out retval ) )
        {
          this.OnParentColumnCellEditorDisplayConditionsChanged();
        }
      }
      else if( managerType == typeof( CellContentTemplateChangedEventManager ) )
      {
        ColumnBase column = sender as ColumnBase;

        if( this.ProcessEvent( column, out retval ) )
        {
          this.OnParentColumnCellContentTemplateChanged();
        }
      }
      else if( managerType == typeof( ForeignKeyConfigurationChangedEventManager ) )
      {
        Column column = sender as Column;

        if( this.ProcessEvent( column, out retval ) )
        {
          this.OnParentColumnForeignKeyConfigurationChanged();
        }
      }

      return retval;
    }

    private bool ProcessEvent( ColumnBase column, out bool handled )
    {
      handled = ( column != null )
             && ( column.DataGridControl != null );

      return ( handled )
          && ( this.IsContainerPrepared )
          && ( !this.IsContainerVirtualized );
    }

    private void OnParentColumnCanBeCurrentWhenReadOnlyChanged( ColumnBase column )
    {
      DataGridControl dataGridControl = column.DataGridControl;
      DataGridContext dataGridContext = dataGridControl.DataGridContext;

      if( ( !this.GetCalculatedCanBeCurrent() ) && ( column == dataGridContext.CurrentColumn ) )
      {
        try
        {
          dataGridContext.SetCurrentColumnCore( null, false, dataGridControl.SynchronizeSelectionWithCurrent );
        }
        catch( DataGridException )
        {
          // We swallow the exception if it occurs because of a validation error or Cell was read-only or
          // any other GridException.
        }
      }

      bool cellFocusable = true;

      if( !this.IsBeingEdited )
      {
        switch( this.ParentRow.NavigationBehavior )
        {
          case NavigationBehavior.None:
            cellFocusable = false;
            break;
          case NavigationBehavior.RowOnly:
            cellFocusable = false;
            break;
          case NavigationBehavior.RowOrCell:
            cellFocusable = true;
            break;
          case NavigationBehavior.CellOnly:
            cellFocusable = true;
            break;
        }
      }

      //force an update of the NavigationBehavior characteristics
      this.Focusable = ( cellFocusable && this.GetCalculatedCanBeCurrent() );
    }

    private void OnParentColumnCellEditorDisplayConditionsChanged()
    {
      this.UpdateMatchingDisplayConditions();
    }

    private void OnParentColumnCellContentTemplateChanged()
    {
      //If the editor is displayed, I want to take no actions for this event.
      if( ( this.CurrentEditorPendingDisplayState == EditorDisplayState.PendingShow ) || ( this.IsCellEditorDisplayed ) )
        return;

      //If there is no local value for the ContentTemplate and ContentTemplateSelector properties, then it is possible that
      //the template to use changed.
      if( ( this.ContentTemplateInternal == null ) && ( this.ContentTemplateSelectorInternal == null ) )
      {
        //force a re-evaluation of the CellContentTemplate.
        this.SetCoercedContentTemplate( this.GetCoercedCellContentTemplate(), null );
      }
    }

    private void OnParentColumnForeignKeyConfigurationChanged()
    {
      if( ( this.CurrentEditorPendingDisplayState != EditorDisplayState.PendingShow ) && ( !this.IsCellEditorDisplayed ) )
      {
        this.SetCoercedContentTemplate( this.GetCoercedCellContentTemplate(), null );
      }
    }

    #endregion IWeakEventListener Members

    private KeyActivationGesture m_keyGesture; // = null
    private TextCompositionEventArgs m_textInputArgs; // = null
    private string m_fieldName; // = string.Empty
    private ColumnBase m_parentColumn; // = null
    private object m_styleBeforeError; // = null

    private RoutedEventHandler m_loadedRoutedEventHandler; // = null;
    private DispatcherOperation m_delayedRefreshErrorStyleDispatcherOperation; // = null
    private DispatcherOperation m_delayedHideEditTemplateDispatcherOperation;

    private BitVector32 m_flags = new BitVector32();

    // Binding used by the animated Column reordering feature
    private static Binding ParentColumnTranslationBinding;
    private static Binding ParentColumnIsBeingDraggedBinding;

    private static Binding ParentRowCellContentOpacityBinding;
    private ContentPresenter m_cellContentPresenter;

    private int m_preventMakeVisibleCount; // = 0;

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
      IsContainerRecycled = 262144
    }

    private enum EditorDisplayState
    {
      None = 0,
      PendingShow,
      PendingHide
    }

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

      private Cell m_cell;

      #region IDisposable Members

      public void Dispose()
      {
        if( m_cell == null )
          return;

        m_cell.m_preventMakeVisibleCount--;

        if( m_cell.m_preventMakeVisibleCount == 0 )
          m_cell.PreventMakeVisible = false;

        m_cell = null;
      }

      #endregion
    }
  }
}
