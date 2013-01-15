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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Security;
using System.Security.Permissions;
using System.Windows.Automation.Peers;

using Xceed.Wpf.DataGrid.Print;
using Xceed.Wpf.DataGrid.ValidationRules;
using Xceed.Wpf.DataGrid.Views;
using Xceed.Utils.Wpf;
using Xceed.Wpf.DataGrid.Export;
using Xceed.Wpf.DataGrid.Automation;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_CellsHost", Type = typeof( Panel ) )]
  [TemplatePart( Name = "PART_RowFocusRoot", Type = typeof( FrameworkElement ) )]
  [ContentProperty( "Cells" )]
  public abstract class Row : Control, IPrintInfo, IDataGridItemContainer
  {
    // This validation rule is meant to be used as the rule in error set in a Row's ValidationError when
    // we want to flag a validation error even though no binding's ValidationRules or CellValidationRules are invalid.
    // ie: Row EditEnding event throws or returns with e.Cancel set to True, IEditableObject's EndEdit throws.
    private static readonly ValidationRule CustomRowValidationExceptionValidationRule = new ExceptionValidationRule();

    static Row()
    {
      // We need to override the default Validation ErrorTemplate, else the default adorner will appear
      // around the row when the main column's cell has a Validation error.
      // We handle validation errors at the cell level by using error styles, not through Validation.ErrorTemplate.
      Validation.ErrorTemplateProperty.OverrideMetadata( typeof( Xceed.Wpf.DataGrid.Row ),
        new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior ) );

      Row.IsCurrentProperty = Row.IsCurrentPropertyKey.DependencyProperty;
      Row.CellsProperty = Row.CellsPropertyKey.DependencyProperty;
      Row.IsBeingEditedProperty = Row.IsBeingEditedPropertyKey.DependencyProperty;
      Row.IsDirtyProperty = Row.IsDirtyPropertyKey.DependencyProperty;
      Row.IsSelectedProperty = Row.IsSelectedPropertyKey.DependencyProperty;

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( Row ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( OnParentGridControlChanged ) ) );

      DataGridControl.CellEditorDisplayConditionsProperty.OverrideMetadata( typeof( Row ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( OnCellEditorDisplayConditionsChanged ) ) );

      // We do this last to ensure all the Static content of the row is done before using 
      // any static readonly field of the cell.
      EventManager.RegisterClassHandler( typeof( Row ), Cell.IsDirtyEvent, new RoutedEventHandler( OnCellIsDirty_ClassHandler ) );
      Row.IsDirtyEvent = Cell.IsDirtyEvent.AddOwner( typeof( Row ) );
    }

    protected Row()
    {
      this.CommandBindings.Add( new CommandBinding( DataGridCommands.BeginEdit,
                                                      new ExecutedRoutedEventHandler( OnBeginEditExecuted ),
                                                      new CanExecuteRoutedEventHandler( OnBeginEditCanExecute ) ) );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.EndEdit,
                                              new ExecutedRoutedEventHandler( OnEndEditExecuted ),
                                              new CanExecuteRoutedEventHandler( OnEndEditCanExecute ) ) );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.CancelEdit,
                                              new ExecutedRoutedEventHandler( OnCancelEditExecuted ),
                                              new CanExecuteRoutedEventHandler( OnCancelEditCanExecute ) ) );

      // Cache the CellsCollection and always return it in the 
      // getter of the DependencyProperty's CLR accessor
      m_cellsCache = new VirtualizingCellCollection( this );

      // Set the Value of the DependencyProperty
      this.SetValue( Row.CellsPropertyKey, m_cellsCache );
    }

    #region NavigationBehavior Property

    public static readonly DependencyProperty NavigationBehaviorProperty = DataGridControl.NavigationBehaviorProperty.AddOwner(
      typeof( Row ),
      new FrameworkPropertyMetadata( NavigationBehavior.CellOnly, FrameworkPropertyMetadataOptions.Inherits,
        new PropertyChangedCallback( OnNavigationBehaviorChanged ) ) );

    public NavigationBehavior NavigationBehavior
    {
      get
      {
        return ( NavigationBehavior )this.GetValue( Row.NavigationBehaviorProperty );
      }
      set
      {
        this.SetValue( Row.NavigationBehaviorProperty, value );
      }
    }

    #endregion NavigationBehavior Property

    #region Cells Read-Only Property

    private static readonly DependencyPropertyKey CellsPropertyKey =
        DependencyProperty.RegisterReadOnly( "Cells",
        typeof( CellCollection ),
        typeof( Row ),
        new PropertyMetadata( null ) );

    public static readonly DependencyProperty CellsProperty;

    // Define it as Virtualizing to avoid casts
    private VirtualizingCellCollection m_cellsCache; // = null;

    public CellCollection Cells
    {
      get
      {
        return m_cellsCache;
      }
    }

    #endregion Cells Read-Only Property

    #region CellEditorDisplayConditions Property

    public CellEditorDisplayConditions CellEditorDisplayConditions
    {
      get
      {
        return ( CellEditorDisplayConditions )this.GetValue( DataGridControl.CellEditorDisplayConditionsProperty );
      }
      set
      {
        this.SetValue( DataGridControl.CellEditorDisplayConditionsProperty, value );
      }
    }

    #endregion CellEditorDisplayConditions Property

    #region CellErrorStyle Property

    public static readonly DependencyProperty CellErrorStyleProperty =
      DataGridControl.CellErrorStyleProperty.AddOwner( typeof( Row ) );

    public Style CellErrorStyle
    {
      get
      {
        return ( Style )this.GetValue( Row.CellErrorStyleProperty );
      }

      set
      {
        this.SetValue( Row.CellErrorStyleProperty, value );
      }
    }

    #endregion CellErrorStyle Property

    #region CellContentOpacity Internal Property

    internal static readonly DependencyProperty CellContentOpacityProperty = DependencyProperty.RegisterAttached(
      "CellContentOpacity",
      typeof( double ),
      typeof( Row ),
      new FrameworkPropertyMetadata( 1d ) );

    internal static double GetCellContentOpacity( DependencyObject obj )
    {
      return ( double )obj.GetValue( Row.CellContentOpacityProperty );
    }
    internal static void SetCellContentOpacity( DependencyObject obj, double value )
    {
      obj.SetValue( Row.CellContentOpacityProperty, value );
    }

    #endregion CellContentOpacity Internal Property

    #region EditTriggers Property

    public static readonly DependencyProperty EditTriggersProperty = DataGridControl.EditTriggersProperty.AddOwner( typeof( Row ) );

    public EditTriggers EditTriggers
    {
      get
      {
        return ( EditTriggers )this.GetValue( Row.EditTriggersProperty );
      }
      set
      {
        this.SetValue( Row.EditTriggersProperty, value );
      }
    }

    #endregion EditTriggers Property

    #region HasValidationError Read-Only Property

    internal static readonly DependencyPropertyKey HasValidationErrorPropertyKey =
        DependencyProperty.RegisterReadOnly( "HasValidationError", typeof( bool ), typeof( Row ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty HasValidationErrorProperty =
      HasValidationErrorPropertyKey.DependencyProperty;

    public bool HasValidationError
    {
      get
      {
        return ( bool )this.GetValue( Row.HasValidationErrorProperty );
      }
    }

    internal void SetHasValidationError( bool value )
    {
      if( value != this.HasValidationError )
      {
        if( value )
        {
          this.SetValue( Row.HasValidationErrorPropertyKey, value );
        }
        else
        {
          this.SetValue( Row.HasValidationErrorPropertyKey, DependencyProperty.UnsetValue );
        }
      }
    }

    #endregion HasValidationError Read-Only Property

    #region IsValidationErrorRestrictive Read-Only Property

    private static readonly DependencyPropertyKey IsValidationErrorRestrictivePropertyKey =
        DependencyProperty.RegisterReadOnly( "IsValidationErrorRestrictive", typeof( bool ), typeof( Row ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty IsValidationErrorRestrictiveProperty =
      IsValidationErrorRestrictivePropertyKey.DependencyProperty;

    public bool IsValidationErrorRestrictive
    {
      get
      {
        return ( bool )this.GetValue( Row.IsValidationErrorRestrictiveProperty );
      }
    }

    internal void SetIsValidationErrorRestrictive( bool value )
    {
      if( value != this.IsValidationErrorRestrictive )
      {
        if( value )
        {
          this.SetValue( Row.IsValidationErrorRestrictivePropertyKey, value );
        }
        else
        {
          this.SetValue( Row.IsValidationErrorRestrictivePropertyKey, DependencyProperty.UnsetValue );
        }
      }
    }

    #endregion IsValidationErrorRestrictive Read-Only Property

    #region IsBeingEdited Read-Only Property

    internal static readonly DependencyPropertyKey IsBeingEditedPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsBeingEdited", typeof( bool ), typeof( Row ), new PropertyMetadata( false, new PropertyChangedCallback( OnIsBeingEditedChanged ) ) );

    public static readonly DependencyProperty IsBeingEditedProperty;

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
        this.IsBeingEditedCache = value;

        if( value )
        {
          this.SetValue( Row.IsBeingEditedPropertyKey, value );
        }
        else
        {
          this.SetValue( Row.IsBeingEditedPropertyKey, DependencyProperty.UnsetValue );
        }
      }
    }

    #endregion IsBeingEdited Read-Only Property

    #region IsDirty Read-Only Property

    private static readonly DependencyPropertyKey IsDirtyPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsDirty", typeof( bool ), typeof( Row ), new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsDirtyProperty;

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    internal static RoutedEvent IsDirtyEvent; // = null;

    public bool IsDirty
    {
      get
      {
        return ( bool )this.GetValue( Row.IsDirtyProperty );
      }
    }

    private void SetIsDirty( bool value )
    {
      if( value != this.IsDirty )
      {
        if( value )
        {
          this.SetValue( Row.IsDirtyPropertyKey, value );
        }
        else
        {
          this.SetValue( Row.IsDirtyPropertyKey, DependencyProperty.UnsetValue );
        }

        if( this.IsBeingEdited )
        {
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

          DataGridControl gridControl = ( dataGridContext != null )
            ? dataGridContext.DataGridControl
            : null;

          if( gridControl != null )
          {
            RowState rowState = gridControl.CurrentRowInEditionState;
            Debug.Assert( rowState != null );

            if( rowState != null )
              rowState.SetIsDirty( value );
          }
        }
      }
    }

    #endregion IsDirty Read-Only Property

    #region RowDisplayEditorMatchingConditions Property

    internal static readonly DependencyProperty RowDisplayEditorMatchingConditionsProperty =
        DependencyProperty.Register( "RowDisplayEditorMatchingConditions", typeof( CellEditorDisplayConditions ), typeof( Row ), new FrameworkPropertyMetadata( CellEditorDisplayConditions.None ) );

    internal void SetDisplayEditorMatchingCondition( CellEditorDisplayConditions condition )
    {
      CellEditorDisplayConditions previousValue = ( CellEditorDisplayConditions )this.GetValue( Row.RowDisplayEditorMatchingConditionsProperty );

      previousValue = previousValue | condition;

      this.SetValue( Row.RowDisplayEditorMatchingConditionsProperty, previousValue );
    }

    internal void RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions condition )
    {
      CellEditorDisplayConditions previousValue = ( CellEditorDisplayConditions )this.GetValue( Row.RowDisplayEditorMatchingConditionsProperty );

      previousValue = previousValue & ~condition;

      this.SetValue( Row.RowDisplayEditorMatchingConditionsProperty, previousValue );
    }

    #endregion RowDisplayEditorMatchingConditions Property

    #region ReadOnly Property

    public static readonly DependencyProperty ReadOnlyProperty =
        DataGridControl.ReadOnlyProperty.AddOwner( typeof( Row ) );

    public bool ReadOnly
    {
      get
      {
        return ( bool )this.GetValue( Row.ReadOnlyProperty );
      }
      set
      {
        this.SetValue( Row.ReadOnlyProperty, value );
      }
    }

    #endregion ReadOnly Property

    #region IsCurrent Read-Only Property

    internal static readonly DependencyPropertyKey IsCurrentPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsCurrent", typeof( bool ), typeof( Row ), new UIPropertyMetadata( false, new PropertyChangedCallback( Row.OnIsCurrentChanged ) ) );

    public static readonly DependencyProperty IsCurrentProperty;

    public bool IsCurrent
    {
      get
      {
        return ( bool )this.GetValue( Row.IsCurrentProperty );
      }
    }

    internal void SetIsCurrent( bool value )
    {
      if( value )
      {
        this.SetValue( Row.IsCurrentPropertyKey, true );
      }
      else
      {
        this.SetValue( Row.IsCurrentPropertyKey, DependencyProperty.UnsetValue );
      }

      this.UpdateNavigationBehavior();
    }

    #endregion IsCurrent Read-Only Property

    #region IsSelected Read-only Property

    private static readonly DependencyPropertyKey IsSelectedPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsSelected", typeof( bool ), typeof( Row ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty IsSelectedProperty;

    public bool IsSelected
    {
      get
      {
        return ( bool )this.GetValue( Row.IsSelectedProperty );
      }
    }

    internal void SetIsSelected( bool value )
    {
      if( this.IsSelected != value )
      {
        if( value )
        {
          this.SetValue( Row.IsSelectedPropertyKey, value );
        }
        else
        {
          this.ClearValue( Row.IsSelectedPropertyKey );
        }

        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext != null )
          dataGridContext.RaiseIsSelectedChangedAutomationEvent( this, value );
      }
    }

    #endregion IsSelected Property

    #region CellsHostPanel Read-Only Property

    internal Panel CellsHostPanel
    {
      get
      {
        return m_cellsHostPanel;
      }
    }

    #endregion CellsHostPanel Read-Only Property

    #region SelectionBackground Property

    public static readonly DependencyProperty SelectionBackgroundProperty =
      DependencyProperty.Register( "SelectionBackground",
      typeof( Brush ),
      typeof( Row ),
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
      typeof( Row ),
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
      typeof( Row ),
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
      typeof( Row ),
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

    #region RowFocusRoot Property

    private FrameworkElement m_rowFocusRoot;

    internal FrameworkElement RowFocusRoot
    {
      get
      {
        return m_rowFocusRoot;
      }
    }

    #endregion

    #region IsTemplateCell Attached Property

    internal static readonly DependencyProperty IsTemplateCellProperty =
        DependencyProperty.RegisterAttached( "IsTemplateCell", typeof( bool ), typeof( Row ), new UIPropertyMetadata( false ) );

    internal static bool GetIsTemplateCell( DependencyObject obj )
    {
      return ( bool )obj.GetValue( Row.IsTemplateCellProperty );
    }

    private static void SetIsTemplateCell( DependencyObject obj, bool value )
    {
      obj.SetValue( Row.IsTemplateCellProperty, value );
    }

    #endregion IsTemplateCell Attached Property

    #region ValidationError Read-Only Property

    public static readonly RoutedEvent ValidationErrorChangingEvent = EventManager.RegisterRoutedEvent( "ValidationErrorChanging", RoutingStrategy.Bubble, typeof( RowValidationErrorRoutedEventHandler ), typeof( Row ) );

    public event RowValidationErrorRoutedEventHandler ValidationErrorChanging
    {
      add
      {
        base.AddHandler( Row.ValidationErrorChangingEvent, value );
      }
      remove
      {

        base.RemoveHandler( Row.ValidationErrorChangingEvent, value );
      }
    }

    protected virtual void OnValidationErrorChanging( RowValidationErrorRoutedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    private static readonly DependencyPropertyKey ValidationErrorPropertyKey =
        DependencyProperty.RegisterReadOnly( "ValidationError", typeof( RowValidationError ), typeof( Row ),
        new PropertyMetadata( null,
          new PropertyChangedCallback( Row.OnValidationErrorChanged ),
          new CoerceValueCallback( Row.OnCoerceValidationError ) ) );

    public static readonly DependencyProperty ValidationErrorProperty = Row.ValidationErrorPropertyKey.DependencyProperty;

    public RowValidationError ValidationError
    {
      get
      {
        return ( RowValidationError )this.GetValue( Row.ValidationErrorProperty );
      }
    }

    internal void SetValidationError( RowValidationError value )
    {
      this.SetValue( Row.ValidationErrorPropertyKey, value );
    }

    private static object OnCoerceValidationError( DependencyObject sender, object value )
    {
      if( value == null )
        return value;

      Row row = ( Row )sender;

      RowValidationErrorRoutedEventArgs rowValidationErrorRoutedEventArgs =
        new RowValidationErrorRoutedEventArgs( Row.ValidationErrorChangingEvent, row, ( RowValidationError )value );

      row.OnValidationErrorChanging( rowValidationErrorRoutedEventArgs );

      return rowValidationErrorRoutedEventArgs.RowValidationError;
    }

    private static void OnValidationErrorChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      Row row = ( Row )sender;

      if( row.IsBeingEdited )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( row );

        if( dataGridContext != null )
        {
          DataGridControl dataGridControl = dataGridContext.DataGridControl;

          if( dataGridControl != null )
          {
            RowState rowState = dataGridControl.CurrentRowInEditionState;
            Debug.Assert( rowState != null );

            if( rowState != null )
              rowState.SetItemValidationError( e.NewValue as RowValidationError );
          }
        }
      }

      if( e.OldValue == null )
      {
        row.m_cachedLocalToolTip = row.ReadLocalValue( Row.ToolTipProperty );
        row.ToolTip = ( ( RowValidationError )e.NewValue ).ErrorContent;
      }
      else if( e.NewValue == null )
      {
        if( row.m_cachedLocalToolTip == DependencyProperty.UnsetValue )
        {
          row.ClearValue( Row.ToolTipProperty );
        }
        else
        {
          row.ToolTip = row.m_cachedLocalToolTip;
        }
      }
      else
      {
        row.ToolTip = ( ( RowValidationError )e.NewValue ).ErrorContent;
      }


      row.UpdateHasErrorFlags( null );
    }

    private object m_cachedLocalToolTip;

    #endregion ValidationError Read-Only Property

    #region UnboundDataItemContext Property

    internal UnboundDataItem UnboundDataItemContext
    {
      get
      {
        return m_unboundDataItem;
      }
    }

    internal void ClearUnboundDataItemContext()
    {
      if( m_unboundDataItemNode != null )
      {
        UnboundDataItem.FreeUnboundDataItem( m_unboundDataItemNode );
        m_unboundDataItemNode = null;
      }

      m_unboundDataItem = null;
    }

    internal void UpdateUnboundDataItemContext()
    {
      object dataItem = this.DataContext;

      if( dataItem == null )
      {
        this.ClearUnboundDataItemContext();
        return;
      }

      if( m_unboundDataItemNode != null )
      {
        object oldDataItem = m_unboundDataItemNode.DataItem;

        if( ( m_unboundDataItemNode.IsAlive ) && ( object.Equals( oldDataItem, dataItem ) ) )
          return;

        UnboundDataItem.FreeUnboundDataItem( m_unboundDataItemNode );
      }

      m_unboundDataItemNode = UnboundDataItem.GetUnboundDataItemNode( dataItem, out m_unboundDataItem );
    }

    private UnboundDataItem.UnboundDataItemNode m_unboundDataItemNode;
    private UnboundDataItem m_unboundDataItem;

    #endregion

    #region CreatedCells Property

    internal IEnumerable<Cell> CreatedCells
    {
      get
      {
        Debug.Assert( m_cellsCache != null );
        return m_cellsCache.BindedCells;
      }
    }

    #endregion

    #region CreatedCellsCount Property

    internal int CreatedCellsCount
    {
      get
      {
        Debug.Assert( m_cellsCache != null );
        return m_cellsCache.BindedCells.Count;
      }
    }

    #endregion

    #region IsClearingContainer Property

    internal bool IsClearingContainer
    {
      get
      {
        return m_flags[ ( int )RowFlags.IsClearingContainer ];
      }
      set
      {
        m_flags[ ( int )RowFlags.IsClearingContainer ] = value;
      }
    }

    #endregion

    #region IsContainerPrepared Property

    internal bool IsContainerPrepared
    {
      get
      {
        return m_flags[ ( int )RowFlags.IsContainerPrepared ];
      }
      set
      {
        m_flags[ ( int )RowFlags.IsContainerPrepared ] = value;
      }
    }

    #endregion

    #region IsInitializingInsertionRow Property

    internal bool IsInitializingInsertionRow
    {
      get
      {
        return m_flags[ ( int )RowFlags.IsInitializingInsertionRow ];
      }
      set
      {
        m_flags[ ( int )RowFlags.IsInitializingInsertionRow ] = value;
      }
    }

    #endregion

    #region EmptyDataItem Property

    internal EmptyDataItem EmptyDataItem
    {
      get
      {
        if( m_emptyDataItem == null )
        {
          m_emptyDataItem = new EmptyDataItem();
        }

        return m_emptyDataItem;
      }
    }

    private EmptyDataItem m_emptyDataItem; // = null;

    #endregion

    public static Row FindFromChild( DependencyObject child )
    {
      return Row.FindFromChild( ( DataGridContext )null, child );
    }

    public static Row FindFromChild( DataGridContext dataGridContext, DependencyObject child )
    {
      // In this situation, the dataGridContext is the DataGridContext of the Row to find.
      // Useful when a grid is used as a Cell editor and want the Row for a specific DataGridContext.
      if( child == null )
        return null;

      Row row = null;

      while( ( row == null ) && ( child != null ) )
      {
        child = TreeHelper.GetParent( child );
        row = child as Row;

        if( row == null )
        {
          RowSelector rowSelector = child as RowSelector;

          if( rowSelector != null )
            row = rowSelector.DataContext as Row;
        }

        if( ( row != null )
          && ( dataGridContext != null )
          && ( dataGridContext != DataGridControl.GetDataGridContext( row ) ) )
        {
          row = null;
        }
      }

      return row;
    }

    public static Row FindFromChild( DataGridControl dataGridControl, DependencyObject child )
    {
      // In this situation, the dataGridControl is the DataGridControl of the Cell to find.
      // Useful when a grid is used as a Cell editor and want the Row for a specific DataGridControl.
      if( child == null )
        return null;

      Row row = null;

      while( ( row == null ) && ( child != null ) )
      {
        child = TreeHelper.GetParent( child );
        row = child as Row;

        if( ( row != null ) && ( dataGridControl != null ) )
        {
          DataGridContext tempDataGridContext = DataGridControl.GetDataGridContext( row );

          if( ( tempDataGridContext == null ) || ( tempDataGridContext.DataGridControl != dataGridControl ) )
          {
            row = null;
          }
        }
      }

      return row;
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        throw new DataGridInternalException();

      DataGridControl gridControl = dataGridContext.DataGridControl;
      if( gridControl == null )
        throw new DataGridInternalException();

      m_rowFocusRoot = this.GetTemplateChild( "PART_RowFocusRoot" ) as FrameworkElement;

      {
        m_oldCellsHostPanel = m_cellsHostPanel;

        IVirtualizingCellsHost oldVirtualizingCellsHost = m_oldCellsHostPanel as IVirtualizingCellsHost;

        if( oldVirtualizingCellsHost != null )
          oldVirtualizingCellsHost.ClearCellsHost();

        m_cellsHostPanel = this.GetTemplateChild( "PART_CellsHost" ) as Panel;

        if( m_rowFocusRoot == null )
          m_rowFocusRoot = m_cellsHostPanel;

        m_templateCells.Clear();

        this.GetTemplateCells( this, m_templateCells );

        this.PreparePreviousTemplateCells();

        this.SynchronizeCellsWithColumns( gridControl, m_templateCells, false );

        if( m_oldCellsHostPanel != null )
          m_oldCellsHostPanel.Children.Clear();

        IVirtualizingCellsHost virtualizingCellsHost = m_cellsHostPanel as IVirtualizingCellsHost;

        if( virtualizingCellsHost != null )
        {
          // Ensure to register to the TableViewColumnVirtualizationManager
          virtualizingCellsHost.PrepareCellsHost( dataGridContext );

          // We force an InvalidateMeasure on the CellsHost to be sure
          // the visible Cells are the good ones
          virtualizingCellsHost.InvalidateCellsHostMeasure();
        }
        else
        {
          // Reset the flag
          if( gridControl.ForceGeneratorReset == false )
            gridControl.ForceGeneratorReset = true;

          this.PutCellsInVisualTree( m_templateCells );
        }


        m_oldCellsHostPanel = null;
      }

      this.UpdateNavigationBehavior();
      this.UpdateCellsFocusableStatus();
    }

    protected override void OnMouseMove( MouseEventArgs e )
    {
      base.OnMouseMove( e );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      if( e.LeftButton == MouseButtonState.Pressed )
      {
        dataGridContext.DataGridControl.DoDrag( e );
      }
      else
      {
        dataGridContext.DataGridControl.ResetDragDataObject();
      }
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

      if( this.NavigationBehavior != NavigationBehavior.None )
      {
        // We accept NavigationBehavior.CellOnly because it will be handled by the OnPreviewGotKeyboardFocus
        // of DataGridControl to redirect it to the current column if possible.
        DependencyObject sourceElement = e.OriginalSource as DependencyObject;

        // Is not FixedColumnSplitter
        if( !( sourceElement is Views.FixedColumnSplitter ) && ( !Row.IsPartOfFixedColumnSplitter( sourceElement ) ) )
        {
          bool focused = false;

          if( m_rowFocusRoot != null )
          {
            focused = m_rowFocusRoot.Focus();
          }
          else
          {
            focused = this.Focus();
          }

          e.Handled = true;

          if( focused )
          {
            // Keep a reference to the mouse position so we can calculate when a drag operation is actually started.
            dataGridContext.DataGridControl.InitializeDragPostion( e );
          }
        }
      }
    }

    protected override void OnMouseEnter( MouseEventArgs e )
    {
      base.OnMouseEnter( e );

      //If the current CellEditorDisplayConditions requires display when mouse is over the Row 
      if( Row.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.MouseOverRow ) )
      {
        //Display the editors for the Row
        this.SetDisplayEditorMatchingCondition( CellEditorDisplayConditions.MouseOverRow );
      }

      // In case a value was explicitly specified on the Cell's ParentColumn
      this.RefreshCellsDisplayedTemplate();
    }

    protected override void OnMouseLeave( MouseEventArgs e )
    {
      base.OnMouseLeave( e );

      //If the current CellEditorDisplayConditions requires display when mouse is over the Row 
      if( Row.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.MouseOverRow ) )
      {
        //Display the editors for the Row
        this.RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions.MouseOverRow );
      }

      // In case a value was explicitly specified on the Cell's ParentColumn
      this.RefreshCellsDisplayedTemplate();
    }

    protected override void OnIsKeyboardFocusWithinChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnIsKeyboardFocusWithinChanged( e );

      //Update the NavigationBehavior related parameter
      this.UpdateNavigationBehavior();
      this.UpdateCellsFocusableStatus();
    }

    protected override void OnGotKeyboardFocus( KeyboardFocusChangedEventArgs e )
    {
      //this element or a child element of the row is receiving the keyboard focus
      //update the cell navigation parameters
      this.UpdateNavigationBehavior();
      this.UpdateCellsFocusableStatus();

      base.OnGotKeyboardFocus( e );
    }

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      object currentThemeKey = view.GetDefaultStyleKey( typeof( Row ) );

      if( currentThemeKey.Equals( this.DefaultStyleKey ) == false )
      {
        this.DefaultStyleKey = currentThemeKey;
      }
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new RowAutomationPeer( this );
    }

    protected virtual void SetDataContext( object item )
    {
      this.DataContext = item;
    }

    protected virtual void ClearDataContext()
    {
      this.DataContext = null;
    }

    protected internal virtual void PrepareContainer( DataGridContext dataGridContext, object item )
    {
      if( this.IsContainerPrepared )
        Debug.Fail( "A Row can't be prepared twice, it must be cleaned before PrepareContainer is called again" );

      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      DataGridControl gridControl = dataGridContext.DataGridControl;

      this.SetDataContext( item );

      if( dataGridContext.InternalCurrentItem == item )
      {
        this.SetIsCurrent( true );
        // The cell.SetIsCurrent is set later since at that stage we have no cell.

        if( !gridControl.IsSetFocusInhibited )
        {
          gridControl.QueueSetFocusHelper( false );
        }
      }

      //this Forces the generation of the template and linked to that, of the cells.
      this.ApplyTemplate();

      // We ensure to create the CurrentCell by fetching it from the CellsCollection
      // If there is a current column on the DataGridContext, try to restore the currency of the cell
      if( ( this.IsCurrent ) && ( dataGridContext.CurrentColumn != null ) )
      {
        //This will also trigger the creation of the Cell if it is virtualized.
        Cell currentCell = m_cellsCache[ dataGridContext.CurrentColumn ];
      }

      IVirtualizingCellsHost virtualizingCellsHost = this.CellsHostPanel as IVirtualizingCellsHost;

      if( virtualizingCellsHost != null )
      {
        virtualizingCellsHost.PrepareCellsHost( dataGridContext );

        // We force an InvalidateMeasure on the CellsHost to be sure
        // the visible Cells are the good ones
        virtualizingCellsHost.InvalidateCellsHostMeasure();
      }
      else
      {
        //re-Initialize all created cells for the new "assignation"
        foreach( Cell cell in this.CreatedCells )
        {
          // Call Cell.PrepareContainer for cells already created
          cell.PrepareContainer( dataGridContext, item );
        }
      }

      if( m_templateCells != null )
      {
        // We must ensure every template cells are prepared
        foreach( Cell cell in m_templateCells.Values )
        {
          cell.PrepareContainer( dataGridContext, item );
        }
      }

      // We must set isContainerPrepared because a verification is made in BeginEdit
      // to ensure the container is prepared before entering in edition. The following
      // RestoreEditionState will possibly trigger BeginEdit.
      this.IsContainerPrepared = true;

      // Use the CurrentContext.InternalCurrentItem when restoring current item
      object currentItemInEdition = gridControl.CurrentItemInEdition;

      // Restore the edition state on the container
      // Voluntarilly using the .Equals() method instead of == operator to allow Struct comparison (in case of GroupHeaderFooterItem struct)
      if( ( currentItemInEdition != null ) && ( currentItemInEdition.Equals( item ) ) )
      {
        //PL: Should only occur when hitting a CollectionView reset while editing.
        this.RestoreEditionState( gridControl.CurrentRowInEditionState, dataGridContext.CurrentColumn );
      }


      this.UpdateMatchingDisplayConditions();
      this.UpdateNavigationBehavior();
    }

    protected abstract Cell CreateCell( ColumnBase column );

    protected abstract bool IsValidCellType( Cell cell );

    protected internal virtual void ClearContainer()
    {
      this.IsClearingContainer = true;

      try
      {
        // Clear every Cells before clearing the Row's values to allow the Cell
        // to check some properties on there parent row when processing a Cell.ClearContainer
        IVirtualizingCellsHost virtualizingCellsHost = this.CellsHostPanel as IVirtualizingCellsHost;

        // If there were some validation errors, we want to clear every Cells to be sure 
        // they will never reflect the error state on another data item if they are not 
        // explicitly prepared
        if( virtualizingCellsHost != null )
        {
          virtualizingCellsHost.ClearCellsHost();
        }

        // Ensure every prepared Cells are cleared correctly
        foreach( Cell cell in m_cellsCache.Cells )
        {
          if( cell.IsContainerPrepared )
          {
            cell.ClearContainer();
          }
        }

        // Clear all the DP's that are either public or somehow inherited.
        this.ClearValue( Row.NavigationBehaviorProperty );
        this.ClearValue( DataGridControl.CellEditorDisplayConditionsProperty );

        // We need to clear both the ValidationError and the HasValidationError property.
        // The clearing of ValidationError will take care of the tooltip/cached local tooltip
        // The clearing of HasValidationError will take care of updating the DataGridControl HasValidationError property.
        this.ClearValue( Row.ValidationErrorPropertyKey );
        this.ClearValue( Row.HasValidationErrorPropertyKey );

        this.ClearValue( Row.IsBeingEditedPropertyKey );
        this.ClearValue( Row.IsDirtyPropertyKey );
        this.ClearValue( Row.RowDisplayEditorMatchingConditionsProperty );
        this.ClearValue( Row.IsCurrentPropertyKey );
        this.ClearValue( Row.IsSelectedPropertyKey );

        this.ClearValue( DataGridControl.ContainerGroupConfigurationProperty );

        this.ClearDataContext();
      }
      finally
      {
        this.IsClearingContainer = false;
        this.IsContainerPrepared = false;
      }
    }

    internal static Row FromContainer( DependencyObject container )
    {
      if( container == null )
        return null;

      Row row = container as Row;

      if( row != null )
        return row;

      HeaderFooterItem headerFooterItem = container as HeaderFooterItem;

      if( headerFooterItem != null )
      {
        row = HeaderFooterItem.FindIDataGridItemContainerInChildren(
          headerFooterItem, headerFooterItem.AsVisual() ) as Row;

        if( row != null )
          return row;
      }

      return null;
    }

    internal static void SetRowValidationErrorOnException( Row row, Exception exception )
    {
      System.Diagnostics.Debug.Assert( ( row != null ) && ( exception != null ) );

      // This method will set a validation error on the row and throw back a DataGridValidationException so that 
      // the row stays in edition.

      if( exception is TargetInvocationException )
        exception = exception.InnerException;

      row.SetValidationError( new RowValidationError(
      Row.CustomRowValidationExceptionValidationRule,
      row,
      exception.Message,
      exception ) );

      // Throwing a DataGridValidationException will be caught by the grid and will make the cell stay in edition.
      throw new DataGridValidationException( "An error occurred while attempting to end the edit process.", exception );
    }

    internal static bool IsCellEditorDisplayConditionsSet( Row row, CellEditorDisplayConditions condition )
    {
      return ( ( row.CellEditorDisplayConditions & condition ) == condition );
    }

    internal void UpdateCellsContentBindingTarget()
    {
      // We need to refresh the Cell.Content target binding in case the dataObject value was coerced to something else.
      foreach( Cell cell in this.CreatedCells )
      {
        cell.UpdateContentBindingTarget();
      }
    }

    internal void SynchronizeCellsWithColumns(
      DataGridControl parentGrid,
      Dictionary<string, Cell> templateCells,
      bool onlyIfParentGridHasChanged )
    {
      if( ( !onlyIfParentGridHasChanged ) ||
        ( parentGrid != m_parentGridUsedForCellsGeneration ) )
      {
        if( templateCells == null )
        {
          // Create a list of manually specified Cells in the Template. These will be 
          // ignored by the Cell adding process.
          templateCells = m_templateCells;
        }

        this.GenerateMissingAndRemoveUnusedCells( templateCells );
        m_parentGridUsedForCellsGeneration = parentGrid;
      }
    }

    internal void UpdateHasErrorFlags( Cell errorChangedCell )
    {
      System.Diagnostics.Debug.Assert( ( errorChangedCell == null ) || ( errorChangedCell.ParentRow == this ) );

      RowValidationError itemValidationError = this.ValidationError;

      bool rowHasValidationError = ( itemValidationError != null );
      bool rowHasRestrictiveValidationError = ( rowHasValidationError ) ? Row.GetIsValidationErrorRestrictive( itemValidationError ) : false;

      if( ( !rowHasRestrictiveValidationError ) && ( errorChangedCell != null ) )
      {
        // We must check the passed cell since it might not yet be part of the row's Cells collection.
        // This can occur when initializing a cell and its Content binding already has a ValidationError.
        if( errorChangedCell.HasValidationError )
        {
          rowHasValidationError = true;

          if( errorChangedCell.IsValidationErrorRestrictive )
            rowHasRestrictiveValidationError = true;
        }

        if( !rowHasRestrictiveValidationError )
        {
          // Create a clone of the list to avoid concurrent access when iterating 
          // and a Cell is added to CreatedCells because of ColumnVirtualization
          List<Cell> createdCells = new List<Cell>( this.CreatedCells );

          foreach( Cell cell in createdCells )
          {
            if( cell == errorChangedCell )
              continue;

            if( cell.HasValidationError )
            {
              rowHasValidationError = true;

              if( cell.IsValidationErrorRestrictive )
              {
                rowHasRestrictiveValidationError = true;
                break;
              }
            }
          }
        }
      }

      this.SetHasValidationError( rowHasValidationError );
      this.SetIsValidationErrorRestrictive( rowHasRestrictiveValidationError );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl gridControl = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      if( gridControl != null )
      {
        bool gridHasValidationError = rowHasValidationError;

        if( !gridHasValidationError )
        {
          var generator = gridControl.CustomItemContainerGenerator;
          foreach( var item in generator.GetRealizedDataItems() )
          {
            var row = gridControl.GetContainerFromItem( item ) as Row;

            if( ( row != null ) && ( row.HasValidationError ) )
            {
              gridHasValidationError = true;
              break;
            }
          }
        }

        gridControl.SetHasValidationError( gridHasValidationError );
      }
    }

    internal virtual bool IsEditTriggerSet( EditTriggers triggers )
    {
      return ( ( this.EditTriggers & triggers ) == triggers );
    }

    internal Cell ProvideCell( ColumnBase column )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
      bool isCurrentColumn = ( dataGridContext.CurrentColumn == column );
      bool rowIsCurrent = this.IsCurrent;

      Xceed.Wpf.DataGrid.Views.ViewBase view = dataGridContext.DataGridControl.GetView();

      string fieldName = column.FieldName;
      Cell cell = null;

      if( fieldName != null )
      {
        // Get the Cell from either the explicitly positioned cells of the row template
        // or from the already created cells.
        m_templateCells.TryGetValue( fieldName, out cell );
      }

      // We get the IVirtualizingCellsHost to be able to modify the LogicalParent if required
      IVirtualizingCellsHost virtualizingCellsHost = m_cellsHostPanel as IVirtualizingCellsHost;

      if( cell == null )
      {
        cell = this.CreateCell( column );

        bool isNewDataContext;
        Cell.AssignDataContext( cell, this.DataContext, this.UnboundDataItemContext, column, out isNewDataContext );

        cell.Initialize( dataGridContext, this, column );

        cell.PrepareDefaultStyleKey( view );

        if( ( virtualizingCellsHost != null ) && ( virtualizingCellsHost.CanModifyLogicalParent ) )
          virtualizingCellsHost.SetLogicalParent( cell );

        //optimization so that lookup speed will be increased for the Cells
        cell.SetValue( DataGridControl.DataGridContextPropertyKey, dataGridContext );

        if( ( rowIsCurrent ) && ( isCurrentColumn ) )
          cell.SetIsCurrent( true );

        // We must update the Focusable status for the newly created cell since
        // UpdateCellsFocusableStatus is only called in Row.OnApplyTemplate
        this.UpdateCellFocusableStatus( cell );

        // The cell was just created, we must prepare it manually the first time
        // since it is added by the PART_CellsHost and not directly put in it
        // by the Row. Further PrepareContainer will be done when Row.PrepareContainer
        // is called since it will be contained in Row.CreatedCells
        cell.PrepareContainer( dataGridContext, this.DataContext );
      }
      else
      {
        if( virtualizingCellsHost == null )
        {
          cell.Initialize( dataGridContext, this, column );
        }
        else
        {
          Debug.Fail( "We do not want to return a template Cell since it already has a visual parent." );
          // We do not want to return a template Cell since it already has a visual parent
          cell = null;
        }
      }

      return cell;
    }

    internal void RefreshCellsDisplayedTemplate()
    {
      // We never want to update the cell's template while clearing container
      if( this.IsClearingContainer )
        return;

      // In case a value was explicitly specified on the Cell's ParentColumn
      foreach( Cell cell in this.CreatedCells )
      {
        cell.RefreshDisplayedTemplate();
      }
    }

    private static bool GetIsValidationErrorRestrictive( RowValidationError validationError )
    {
      if( validationError == null )
        return false;

      return !( validationError.RuleInError is DataErrorValidationRule );
    }

    private static bool IsPartOfFixedColumnSplitter( DependencyObject element )
    {
      DependencyObject parent = TreeHelper.GetParent( element );

      while( parent != null )
      {
        if( parent is Views.FixedColumnSplitter )
          break;

        // It is not necessary to go further than the Row.
        if( parent is Row )
          break;

        parent = TreeHelper.GetParent( parent );
      }

      return ( parent is Views.FixedColumnSplitter );
    }

    private static bool IsPartOfCell( DependencyObject element )
    {
      bool retval = false;

      DependencyObject parent = element;

      while( ( parent is Row ) == false )
      {
        parent = TreeHelper.GetParent( parent );

        //if the parent is null, then the item "clicked" is floating... let's consider it as a cell, so that 
        //no specific actions are taken
        if( parent == null )
        {
          retval = true;
          break;
        }

        if( parent is Cell )
        {
          retval = true;
          break;
        }
      }

      return retval;
    }

    private void UpdateMatchingDisplayConditions()
    {
      CellEditorDisplayConditions newEffectiveValue = CellEditorDisplayConditions.None;

      if( ( Row.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.RowIsBeingEdited ) ) &&
        ( this.IsBeingEdited ) )
      {
        newEffectiveValue |= CellEditorDisplayConditions.RowIsBeingEdited;
      }

      if( ( Row.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.MouseOverRow ) ) &&
        ( this.IsMouseOver ) )
      {
        newEffectiveValue |= CellEditorDisplayConditions.MouseOverRow;
      }

      if( ( Row.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.RowIsCurrent ) ) &&
        ( this.IsCurrent ) )
      {
        newEffectiveValue |= CellEditorDisplayConditions.RowIsCurrent;
      }

      if( Row.IsCellEditorDisplayConditionsSet( this, CellEditorDisplayConditions.Always ) )
      {
        newEffectiveValue |= CellEditorDisplayConditions.Always;
      }

      this.SetValue( Row.RowDisplayEditorMatchingConditionsProperty, newEffectiveValue );
    }

    private Cell GetCellForCurrentColumn()
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return null;

      return m_cellsCache[ dataGridContext.CurrentColumn ];
    }

    private void UpdateNavigationBehavior()
    {
      DataGridContext context = DataGridControl.GetDataGridContext( this );

      // We check if the container has an Item binded to it and if
      // it's not the case, we don't need to update the navigation 
      // mode since it has already been changed to None by the 
      // PrepareIsTabStop of the TableViewItemsHost class.
      if( ( context == null )
        || ( context.GetItemFromContainer( this ) == null ) )
      {
        return;
      }

      bool rowFocusable = false;
      KeyboardNavigationMode tabNavigation = KeyboardNavigationMode.None;
      KeyboardNavigationMode keyboardNavigation = KeyboardNavigationMode.None;

      FrameworkElement focusItem = this.RowFocusRoot;

      bool isKeyboardFocused = ( focusItem != null ) ? focusItem.IsKeyboardFocused : this.IsKeyboardFocused;

      //do not want to update the navigation behavior of the row if i'm currently editing
      if( !this.IsBeingEdited )
      {
        if( !this.IsCurrent )
        {
          rowFocusable = ( this.NavigationBehavior != NavigationBehavior.None );
          tabNavigation = KeyboardNavigationMode.None;
          keyboardNavigation = KeyboardNavigationMode.None;
        }
        else
        {
          switch( this.NavigationBehavior )
          {
            case NavigationBehavior.None:
              rowFocusable = false;
              tabNavigation = KeyboardNavigationMode.None;
              keyboardNavigation = KeyboardNavigationMode.None;
              break;

            case NavigationBehavior.RowOnly:
              rowFocusable = true;
              tabNavigation = KeyboardNavigationMode.None;
              keyboardNavigation = KeyboardNavigationMode.None;
              break;

            case NavigationBehavior.RowOrCell:
              //There is an identified weakness with the IsKeyboardFocusWithin property where it cannot tell if the focus is within a Popup which is within the element
              //This has been identified, and only the places where it caused problems were fixed... This comment is only here to remind developpers of the flaw
              if( ( this.IsKeyboardFocusWithin ) && ( !isKeyboardFocused ) )
              {
                rowFocusable = false;
                tabNavigation = KeyboardNavigationMode.None; //for case 99719: modified this to disable the Tab navigation between cells.
                keyboardNavigation = KeyboardNavigationMode.Continue;
              }
              else
              {
                rowFocusable = true;
                tabNavigation = KeyboardNavigationMode.None;
                keyboardNavigation = KeyboardNavigationMode.None;
              }
              break;

            case NavigationBehavior.CellOnly:
              rowFocusable = false;
              tabNavigation = KeyboardNavigationMode.None; 
              keyboardNavigation = KeyboardNavigationMode.Continue;
              break;
          }
        }
      }
      else
      {
        rowFocusable = true;
        tabNavigation = KeyboardNavigationMode.Cycle;
        keyboardNavigation = KeyboardNavigationMode.Continue;
      }

      if( focusItem != null )
      {
        this.Focusable = false;
        KeyboardNavigation.SetTabNavigation( this, KeyboardNavigationMode.None ); 
        KeyboardNavigation.SetDirectionalNavigation( this, KeyboardNavigationMode.Continue );

        if( focusItem.FocusVisualStyle != this.FocusVisualStyle )
        {
          if( focusItem.ReadLocalValue( FrameworkElement.FocusVisualStyleProperty ) == DependencyProperty.UnsetValue )
          {
            focusItem.FocusVisualStyle = this.FocusVisualStyle;
          }
        }

        focusItem.Focusable = rowFocusable;
        KeyboardNavigation.SetTabNavigation( focusItem, tabNavigation );
        KeyboardNavigation.SetDirectionalNavigation( focusItem, keyboardNavigation );
      }
      else
      {
        this.Focusable = rowFocusable;
        KeyboardNavigation.SetTabNavigation( this, tabNavigation );
        KeyboardNavigation.SetDirectionalNavigation( this, keyboardNavigation );
      }
    }

    private void UpdateCellsFocusableStatus()
    {
      //cycle through all cells and 
      foreach( Cell cell in this.CreatedCells )
      {

        //force an update of the NavigationBehavior characteristics
        this.UpdateCellFocusableStatus( cell );
      }
    }

    private void UpdateCellFocusableStatus( Cell cell )
    {
      if( cell == null )
        return;

      bool cellFocusable = true;

      ColumnBase parentColumn = cell.ParentColumn;

      if( !this.IsBeingEdited )
      {
        switch( this.NavigationBehavior )
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
      cell.Focusable = ( cellFocusable && cell.GetCalculatedCanBeCurrent() );
    }

    private void RestoreEditionState( RowState savedState, ColumnBase currentColumn )
    {
      if( savedState == null )
        return;

      savedState = savedState.Clone();

      Dictionary<ColumnBase, CellState> cachedCellStates = new Dictionary<ColumnBase, CellState>();

      foreach( Cell cell in this.CreatedCells )
      {
        ColumnBase parentColumn = cell.ParentColumn;

        if( parentColumn == null )
          continue;

        var currentRowInEditionCellState = parentColumn.CurrentRowInEditionCellState;

        if( currentRowInEditionCellState != null )
          cachedCellStates.Add( parentColumn, currentRowInEditionCellState.Clone() );
      }

      try
      {
        this.BeginEdit();
      }
      catch( DataGridException )
      {
        // We swallow exception if it occurs because of a validation error or Cell was read-only or
        // any other GridException.
      }

      if( this.IsBeingEdited )
      {
        this.SetValidationError( savedState.ItemValidationError );

        this.SetIsDirty( savedState.IsDirty );

        foreach( Cell cell in this.CreatedCells )
        {
          ColumnBase parentColumn = cell.ParentColumn;
          CellState cachedCellState;

          if( cachedCellStates.TryGetValue( parentColumn, out cachedCellState ) )
            parentColumn.CurrentRowInEditionCellState = cachedCellState;

          cell.RestoreEditionState( currentColumn );
        }
      }
    }

    private void PutCellsInVisualTree( Dictionary<string, Cell> templateCells )
    {
      if( m_cellsCache == null )
      {
        Debug.Fail( "PutCellsInVisualTree was called on a Row that is not a DataGridControl's descendant or whose DataGridControl ancestor has a null VisibleColumns property." );
        return;
      }

      if( m_cellsHostPanel == null )
      {
        Debug.Fail( "PutCellsInVisualTree was called on a Row that doesn't have a Panel identified as the CellsHost in its VisualTree." );
        return;
      }

      if( m_cellsHostPanel != m_oldCellsHostPanel )
        m_cellsHostPanel.Children.Clear();

      m_oldCellsHostPanel = null;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      object dataItem = this.DataContext;

      // fill the cellsHostPanel in visible order.
      foreach( ColumnBase column in dataGridContext.ColumnsByVisiblePosition )
      {
        Cell cell = m_cellsCache[ column ];

        if( cell == null )
        {
          //This situation is now a proof that something is awfully wrong since
          //the VirtualizingCellCollection will create a cell if none is present.
          throw new DataGridInternalException();
        }

        // We don't add a cell specifically (manually) positioned in the row template.
        if( cell.FieldName == null || !templateCells.ContainsKey( cell.FieldName ) )
          m_cellsHostPanel.Children.Add( cell );

        // When explicitly put in the PART_CellsHost, we must call Cell.PrepareContainer
        cell.PrepareContainer( dataGridContext, dataItem );
      }
    }

    private void GetTemplateCells( Visual root, Dictionary<string, Cell> templateCells )
    {
      int childrenCount = VisualTreeHelper.GetChildrenCount( root );

      if( childrenCount > 0 )
      {
        Visual child = null;
        Cell cell = null;

        for( int i = 0; i < childrenCount; i++ )
        {
          child = VisualTreeHelper.GetChild( root, i ) as Visual;

          if( child != null )
          {
            cell = child as Cell;

            if( cell == null )
            {
              this.GetTemplateCells( child, templateCells );
            }
            else
            {
              //if the cell is of the appropriate type for the Row, then ...
              if( this.IsValidCellType( cell ) )
              {
                //mark the Cell as a Fixed Tempalte Cell.
                Row.SetIsTemplateCell( cell, true );

                templateCells.Add( cell.FieldName, cell );
              }

              //if the cell is not of the appropriate type, don't do anything, it will remain blank.
            }
          }
        }
      }
      else
      {
        Visual child = null;
        ContentControl contentControl = root as ContentControl;

        if( contentControl == null )
        {
          ContentPresenter contentPresenter = root as ContentPresenter;

          if( contentPresenter != null )
          {
            child = contentPresenter.Content as Visual;
          }
        }
        else
        {
          child = contentControl.Content as Visual;
        }

        //avoid recursing into a Row object... that can only mean that the Row object is the data item for the Row (self contained, unbound).
        if( ( child != null ) && ( ( child is Row ) == false ) )
        {
          Cell cell = child as Cell;

          if( cell == null )
          {
            this.GetTemplateCells( child, templateCells );
          }
          else
          {
            //mark the Cell as a Fixed Tempalte Cell.
            Row.SetIsTemplateCell( cell, true );

            templateCells.Add( cell.FieldName, cell );
          }
        }
      }
    }

    private void GenerateMissingAndRemoveUnusedCells( Dictionary<string, Cell> templateCells )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      Debug.Assert( ( dataGridContext != null ) || ( DesignerProperties.GetIsInDesignMode( this ) ) );

      if( dataGridContext == null )
        return;

      ColumnCollection columns = dataGridContext.Columns;
      Hashtable cellsDictionary = new Hashtable();

      //Take each and every cells already created and place them in a dictionary
      //This dictionary is gonna be used to manage cells that need to be removed at the
      //end of the method's body.
      foreach( Cell cell in this.CreatedCells )
      {
        cellsDictionary.Add( cell.FieldName, cell );
      }

      ColumnBase currentColumn = dataGridContext.CurrentColumn;
      bool rowIsCurrent = this.IsCurrent;

      Xceed.Wpf.DataGrid.Views.ViewBase view = dataGridContext.DataGridControl.GetView();

      foreach( ColumnBase column in columns )
      {
        string fieldName = column.FieldName;
        Cell cell = null;

        //Try to get the Template Cell defined for this column
        templateCells.TryGetValue( fieldName, out cell );

        //The cell has been found in the template cells.
        if( cell != null )
        {
          //A cell for this Column has been found in the Template cells, get rid of the corresponding Cell in the Cells collection (if any)
          Cell oldCell;
          if( m_cellsCache.TryGetCell( column, out oldCell ) )
          {
            //If an oldCell was present, clear it and remove it from the Cells collection
            oldCell.ClearContainer();
            m_cellsCache.InternalRemove( oldCell );
          }

          cell.Initialize( dataGridContext, this, column );

          // Ensure to prepare a Cell if it is not prepared or virtualized
          if( !cell.IsContainerPrepared || cell.IsContainerVirtualized )
          {
            cell.PrepareContainer( dataGridContext, this.DataContext );
          }

          //then add the Template cell to the Cells collection.
          m_cellsCache.InternalAdd( cell );

          //finally, if the Row is current and the column in question is current, set the Currency state on the template cell.
          if( ( rowIsCurrent ) && ( column == currentColumn ) )
          {
            cell.SetIsCurrent( true );
          }
        }
        //The cell is not part of the Template Cells... But it still needs some work to be done.
        else
        {
          if( !m_cellsCache.TryGetCell( column, out cell ) )
          {
            cell = null;
          }
        }

        //the Cell is a Template Cell OR is already present in the Row
        if( cell != null )
        {
          //To ensure that the DefaultStyleKey is set appropriatly
          cell.PrepareDefaultStyleKey( view );
        }

        //remove the Column's FieldName from the dictionary of Cells to remove
        //This is to prevent the removal of the cell in the cleanup code below.
        cellsDictionary.Remove( fieldName );
      }

      // clean unmatched cells
      foreach( Cell cell in cellsDictionary.Values )
      {
        cell.ClearContainer();
        m_cellsCache.InternalRemove( cell );
      }
    }

    private void PreparePreviousTemplateCells()
    {
      List<Cell> removeList = new List<Cell>();

      //cycle through all the cells, and if any of them have the 
      //attached property identifying them as Fixed Template Cells, 
      // then clear their FieldName property
      foreach( Cell cell in this.CreatedCells )
      {
        if( Row.GetIsTemplateCell( cell ) )
        {
          removeList.Add( cell );
        }
      }

      foreach( Cell removedCell in removeList )
      {
        removedCell.ClearContainer();
        m_cellsCache.InternalRemove( removedCell );
      }
    }

    #region EDITION

    public static readonly RoutedEvent EditBeginningEvent = EventManager.RegisterRoutedEvent( "EditBeginning", RoutingStrategy.Bubble, typeof( CancelRoutedEventHandler ), typeof( Row ) );
    public static readonly RoutedEvent EditBegunEvent = EventManager.RegisterRoutedEvent( "EditBegun", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Row ) );

    #region IsBeginningEditFromCell Property

    internal bool IsBeginningEditFromCell
    {
      get
      {
        return m_flags[ ( int )RowFlags.IsBeginningEditFromCell ];
      }
      set
      {
        m_flags[ ( int )RowFlags.IsBeginningEditFromCell ] = value;
      }
    }

    #endregion

    #region IsBeginningEdition Property

    internal bool IsBeginningEdition
    {
      get
      {
        return m_flags[ ( int )RowFlags.IsBeginningEdition ];
      }
      private set
      {
        m_flags[ ( int )RowFlags.IsBeginningEdition ] = value;
      }
    }

    #endregion

    #region IsEndingEdition Property

    internal bool IsEndingEdition
    {
      get
      {
        return m_flags[ ( int )RowFlags.IsEndingEdition ];
      }
      private set
      {
        m_flags[ ( int )RowFlags.IsEndingEdition ] = value;
      }
    }

    #endregion

    #region IsCancelingEdition Property

    internal bool IsCancelingEdition
    {
      get
      {
        return m_flags[ ( int )RowFlags.IsCancelingEdition ];
      }
      private set
      {
        m_flags[ ( int )RowFlags.IsCancelingEdition ] = value;
      }
    }

    #endregion

    #region IsBeingEditedCache Property

    private bool IsBeingEditedCache
    {
      get
      {
        return m_flags[ ( int )RowFlags.IsBeingEditedCache ];
      }
      set
      {
        m_flags[ ( int )RowFlags.IsBeingEditedCache ] = value;
      }
    }

    #endregion

    public event CancelRoutedEventHandler EditBeginning
    {
      add
      {
        base.AddHandler( Row.EditBeginningEvent, value );
      }
      remove
      {
        base.RemoveHandler( Row.EditBeginningEvent, value );
      }
    }

    public event RoutedEventHandler EditBegun
    {
      add
      {
        base.AddHandler( Row.EditBegunEvent, value );
      }
      remove
      {
        base.RemoveHandler( Row.EditBegunEvent, value );
      }
    }

    protected internal virtual void OnEditBeginning( CancelRoutedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    protected internal virtual void OnEditBegun()
    {
      RoutedEventArgs e = new RoutedEventArgs( Row.EditBegunEvent, this );
      this.RaiseEvent( e );
    }

    public void BeginEdit()
    {
      if( this.IsBeingEdited )
        return;

      // If there is no dataItem mapped to this container, we don't want to
      // enter in edition
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext != null )
      {
        object dataItem = dataGridContext.GetItemFromContainer( this );

        if( dataItem == null )
          return;
      }

      // We must prevent entering in edition of an EmptyDataItem.
      if( this.DataContext is EmptyDataItem )
        return;

      Debug.Assert( this.IsContainerPrepared, "Can't edit a container that has not been prepared." );

      if( this.ReadOnly )
        throw new DataGridException( "An attempt was made to edit a read-only row." );

      if( this.IsBeginningEdition )
        throw new DataGridException( "An attempt was made to edit a row for which the edit process has already begun." );

      this.IsBeginningEdition = true;

      try
      {
        if( !this.IsBeginningEditFromCell )
        {
          CancelRoutedEventArgs e = new CancelRoutedEventArgs( Row.EditBeginningEvent, this );
          this.OnEditBeginning( e );

          if( e.Cancel )
            throw new DataGridException( "BeginEdit was canceled." );
        }

        // We must update the CellStates before calling BeginEditCore to ensure we have the
        // values that are currently present in the Cells before entering in edition.
        DataGridControl gridControl = ( dataGridContext != null )
          ? dataGridContext.DataGridControl
          : null;

        // This call will also validate that we're not starting a second row edition.
        // It will also save the row state and update the columns' CurrentRowInEditionCellState.
        if( gridControl != null )
          gridControl.UpdateCurrentRowInEditionCellStates( this, this.DataContext );

        this.SetIsBeingEdited( true );

        try
        {
          this.BeginEditCore();
        }
        catch
        {
          this.SetIsBeingEdited( false );

          // Ensure to clear the CellStates
          if( gridControl != null )
            gridControl.UpdateCurrentRowInEditionCellStates( null, null );

          throw;
        }
      }
      finally
      {
        this.IsBeginningEdition = false;
      }

      if( !this.IsBeginningEditFromCell )
        this.OnEditBegun();
    }

    protected virtual void BeginEditCore()
    {
      if( this.IsBeginningEditFromCell )
        return;

      Cell currentCell = this.GetCellForCurrentColumn();

      try
      {
        if( currentCell != null )
        {
          if( !currentCell.IsBeingEdited )
          {
            currentCell.BeginEdit();
          }
        }
        else
        {
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

          if( dataGridContext != null )
          {
            int firstEditableColumn = DataGridScrollViewer.GetFirstVisibleFocusableColumnIndex( dataGridContext );
            if( firstEditableColumn < 0 )
              throw new DataGridException( "Trying to edit while no cell is focusable. " );

            currentCell = m_cellsCache[ dataGridContext.VisibleColumns[ firstEditableColumn ] ];

            if( currentCell != null )
              currentCell.BeginEdit();
          }
        }
      }
      catch( DataGridException )
      {
        // We swallow exception if it occurs because of a validation error or Cell was read-only or
        // any other GridException.
      }
    }

    public static readonly RoutedEvent EditEndingEvent = EventManager.RegisterRoutedEvent( "EditEnding", RoutingStrategy.Bubble, typeof( CancelRoutedEventHandler ), typeof( Row ) );
    public static readonly RoutedEvent EditEndedEvent = EventManager.RegisterRoutedEvent( "EditEnded", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Row ) );

    public event CancelRoutedEventHandler EditEnding
    {
      add
      {
        base.AddHandler( Row.EditEndingEvent, value );
      }
      remove
      {
        base.RemoveHandler( Row.EditEndingEvent, value );
      }
    }

    public event RoutedEventHandler EditEnded
    {
      add
      {
        base.AddHandler( Row.EditEndedEvent, value );
      }
      remove
      {
        base.RemoveHandler( Row.EditEndedEvent, value );
      }
    }

    protected virtual void OnEditEnding( CancelRoutedEventArgs e )
    {
      this.RaiseEvent( e );
    }

    protected virtual void OnEditEnded()
    {
      RoutedEventArgs e = new RoutedEventArgs( Row.EditEndedEvent, this );
      this.RaiseEvent( e );
    }

    public void EndEdit()
    {
      if( !this.IsBeingEdited )
        return;

      if( this.IsEndingEdition )
        throw new InvalidOperationException( "An attempt was made to end the edit process while it is already in the process of being ended." );

      if( this.IsCancelingEdition )
        throw new InvalidOperationException( "An attempt was made to end the edit process while it is being canceled." );

      CancelRoutedEventArgs e = new CancelRoutedEventArgs( Row.EditEndingEvent, this );

      try
      {
        this.OnEditEnding( e );

        if( e.Cancel )
          throw new DataGridValidationException( "EndEdit was canceled." );
      }
      catch( Exception exception )
      {
        // This method will set a validation error on the row and throw back a DataGridValidationException so that 
        // the row stays in edition.
        Row.SetRowValidationErrorOnException( this, exception );
      }

      this.IsEndingEdition = true;

      try
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext == null )
        {
          Debug.Fail( "DataGridContext cannot be null." );
          return;
        }

        IDisposable deferRefresh = ( this is DataRow )
          ? DataRow.DeferCollectionViewRefresh( dataGridContext ) : null;

        try
        {
          this.EndEditCore();
          this.TerminateEditionFromEndEdit();
        }
        finally
        {
          if( deferRefresh != null )
          {
            deferRefresh.Dispose();
          }
        }

        this.PreEditEnded();

        // Item validation has passed if we reached this far.  
        // Since it is the Row's HasValidationError which drives the error styles, setting 
        // the ItemValidationError to null will not get in the way of the row's cells validation.
        this.SetValidationError( null );

      }
      finally
      {
        this.IsEndingEdition = false;
      }

      this.OnEditEnded();
    }

    private void TerminateEditionFromEndEdit()
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl gridControl = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      // Lower IsDirty flags.
      foreach( Cell cell in this.CreatedCells )
      {
        cell.SetIsDirty( false );
        cell.SetIsDirtyFromInitializingInsertionRow( false );
      }

      this.SetIsDirty( false );

      // Reset current row in edition.
      if( gridControl != null )
        gridControl.UpdateCurrentRowInEditionCellStates( null, null );

      this.SetIsBeingEdited( false );

      this.UpdateCellsContentBindingTarget();

      if( ( this.NavigationBehavior == NavigationBehavior.RowOnly ) && ( this.IsCurrent ) )
      {
        if( gridControl != null )
          gridControl.QueueClearCurrentColumn( gridControl.CurrentItem );
      }
    }

    internal virtual void PreEditEnded()
    {
    }

    protected virtual void EndEditCore()
    {
      bool hasRestrictiveError = false;

      // Create a clone of the list to avoid concurrent access when iterating 
      // and a Cell is added to CreatedCells because of ColumnVirtualization
      List<Cell> createdCells = new List<Cell>();

      Cell editCell = null;

      foreach( Cell cell in this.CreatedCells )
      {
        createdCells.Add( cell );

        if( cell.IsBeingEdited )
          editCell = cell;
      }

      if( editCell != null )
      {
        try
        {
          editCell.EndEdit( true, true, true );
          hasRestrictiveError |= Cell.GetIsValidationErrorRestrictive( editCell.ValidationError );
        }
        catch( DataGridValidationException )
        {
          hasRestrictiveError = true;
        }
      }

      foreach( Cell cell in createdCells )
      {
        if( cell == editCell )
          continue;

        Nullable<DataGridUpdateSourceTrigger> contentBindingUpdateSourceTrigger = cell.GetContentBindingUpdateSourceTrigger();

        bool updateContentBindingSource = ( contentBindingUpdateSourceTrigger == DataGridUpdateSourceTrigger.RowEndingEdit );

        bool cascadeValidate = updateContentBindingSource;

        Exception exception;
        CellValidationRule ruleInError;

        ValidationResult result =
          cell.ValidateAndSetAllErrors( true, true, updateContentBindingSource, cascadeValidate, out exception, out ruleInError );

        if( ( !result.IsValid ) && ( Cell.GetIsRuleInErrorRestrictive( ruleInError ) ) )
          hasRestrictiveError = true;
      }

      // Throwing a DataGridValidationException will be caught by the grid and will make the cell stay in edition.
      if( hasRestrictiveError )
        throw new DataGridValidationException( "Row.EndEdit cannot complete because the cell content is invalid." );
    }

    public static readonly RoutedEvent EditCancelingEvent = EventManager.RegisterRoutedEvent( "EditCanceling", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Row ) );
    public static readonly RoutedEvent EditCanceledEvent = EventManager.RegisterRoutedEvent( "EditCanceled", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( Row ) );

    public event RoutedEventHandler EditCanceling
    {
      add
      {
        base.AddHandler( Row.EditCancelingEvent, value );
      }
      remove
      {
        base.RemoveHandler( Row.EditCancelingEvent, value );
      }
    }

    public event RoutedEventHandler EditCanceled
    {
      add
      {
        base.AddHandler( Row.EditCanceledEvent, value );
      }
      remove
      {
        base.RemoveHandler( Row.EditCanceledEvent, value );
      }
    }

    protected virtual void OnEditCanceling()
    {
      RoutedEventArgs e = new RoutedEventArgs( Row.EditCancelingEvent, this );
      this.RaiseEvent( e );
    }

    protected virtual void OnEditCanceled()
    {
      RoutedEventArgs e = new RoutedEventArgs( Row.EditCanceledEvent, this );
      this.RaiseEvent( e );
    }

    public void CancelEdit()
    {
      this.CancelEdit( false );
    }

    internal void CancelEdit( bool forceCancelEdit )
    {
      if( !this.IsBeingEdited )
        return;

      if( this.IsCancelingEdition )
        throw new InvalidOperationException( "An attempt was made to cancel the edit process while it is already in the process of being canceled." );

      if( ( !forceCancelEdit ) && ( this.IsEndingEdition ) )
        throw new InvalidOperationException( "An attempt was made to cancel the edit process while it is being ended." );

      this.OnEditCanceling();
      this.IsCancelingEdition = true;

      try
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
        IDisposable deferRefresh = null;

        if( dataGridContext != null )
        {
          deferRefresh = ( this is DataRow )
            ? DataRow.DeferCollectionViewRefresh( dataGridContext ) : null;
        }

        try
        {
          this.CancelEditCore();
          this.TerminateEditionFromCancelEdit();
        }
        finally
        {
          if( deferRefresh != null )
          {
            deferRefresh.Dispose();
          }
        }

        this.PreEditCanceled();

        // A row cannot have an ItemValidationError when not being edited.
        // Since it is the Row's HasValidationError which drives the error styles, setting 
        // the ItemValidationError to null will not get in the way of the row's cells validation.
        this.SetValidationError( null );

      }
      finally
      {
        this.IsCancelingEdition = false;
      }

      this.OnEditCanceled();
    }

    private void TerminateEditionFromCancelEdit()
    {
      this.SetIsDirty( false );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl gridControl = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      if( gridControl != null )
        gridControl.UpdateCurrentRowInEditionCellStates( null, null );

      this.SetIsBeingEdited( false );

      if( ( this.NavigationBehavior == NavigationBehavior.RowOnly )
         && ( this.IsCurrent ) )
      {
        if( gridControl != null )
          gridControl.QueueClearCurrentColumn( gridControl.CurrentItem );
      }
    }

    internal virtual void PreEditCanceled()
    {
    }

    protected virtual void CancelEditCore()
    {
      // Restore the Cell.Content
      foreach( Cell cell in this.CreatedCells )
      {
        if( cell.IsBeingEdited )
          cell.CancelEdit();

        cell.RevertEditedValue();
      }

#if DEBUG
      foreach( Cell cell in this.CreatedCells )
      {
        Debug.Assert( Cell.GetIsValidationErrorRestrictive( cell.ValidationError ) == false );
      }
#endif
    }

    #endregion EDITION

    #region DP CHANGED HANDLERS

    private static void OnIsBeingEditedChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Row row = ( Row )sender;

      row.UpdateNavigationBehavior();
      row.UpdateCellsFocusableStatus();

      //If the current CellEditorDisplayConditions requires display when Row is Editing
      if( Row.IsCellEditorDisplayConditionsSet( row, CellEditorDisplayConditions.RowIsBeingEdited ) )
      {
        if( ( bool )e.NewValue )
        {
          //Display the editors for the Row
          row.SetDisplayEditorMatchingCondition( CellEditorDisplayConditions.RowIsBeingEdited );
        }
        else
        {
          //Hide the editors for the Row
          row.RemoveDisplayEditorMatchingCondition( CellEditorDisplayConditions.RowIsBeingEdited );
        }
      }

      // In case a value was explicitly specified on the Cell's ParentColumn
      row.RefreshCellsDisplayedTemplate();
    }

    private static void OnIsCurrentChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Row row = ( Row )sender;

      if( row == null )
        return;

      // In case a value was explicitly specified on the Cell's ParentColumn
      row.RefreshCellsDisplayedTemplate();
    }

    private static void OnCellEditorDisplayConditionsChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Row obj = ( Row )sender;

      obj.UpdateMatchingDisplayConditions();
    }

    private static void OnNavigationBehaviorChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      Row obj = ( Row )sender;

      obj.UpdateNavigationBehavior();
      obj.UpdateCellsFocusableStatus();
    }

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl grid = e.NewValue as DataGridControl;
      Row row = sender as Row;

      if( ( row != null ) && ( grid != null ) )
      {
        row.PrepareDefaultStyleKey( grid.GetView() );
      }
    }

    private static void OnCellIsDirty_ClassHandler( object sender, RoutedEventArgs e )
    {
      Row row = ( Row )sender;

      row.SetIsDirty( true );

      e.Source = row;
    }

    #endregion DP CHANGED HANDLERS

    #region COMMANDS

    private void OnBeginEditExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      try
      {
        this.BeginEdit();
      }
      catch( DataGridException )
      {
        // We swallow exception if it occurs because of a validation error or Cell was readonly or
        // any other GridException.
      }
    }

    private void OnBeginEditCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl parentGrid = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      if( parentGrid == null )
      {
        e.CanExecute = false;
      }
      else
      {
        e.CanExecute = ( !this.IsBeingEdited ) && ( !this.ReadOnly )
          && ( this.IsEditTriggerSet( EditTriggers.BeginEditCommand ) );
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

    private void OnEndEditExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.OnEndEditCommandExecutedCore( e );
    }

    internal virtual void OnEndEditCommandExecutedCore( ExecutedRoutedEventArgs e )
    {
      try
      {
        this.EndEdit();
      }
      catch( DataGridException )
      {
        // We swallow exception if it occurs because of a validation error or Cell was read-only or
        // any other GridException.
      }
    }

    private void OnEndEditCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      this.OnEndEditCommandCanExecuteCore( e );
    }

    internal virtual void OnEndEditCommandCanExecuteCore( CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = this.IsBeingEdited;
    }

    #endregion COMMANDS

    #region IPrintInfo Members

    double IPrintInfo.GetPageRightOffset( double horizontalOffset, double viewportWidth )
    {
      IPrintInfo subPrintInfo = this.CellsHostPanel as IPrintInfo;

      if( subPrintInfo != null )
        return subPrintInfo.GetPageRightOffset( horizontalOffset, viewportWidth );

      return horizontalOffset + viewportWidth;
    }

    void IPrintInfo.UpdateElementVisibility( double horizontalOffset, double viewportWidth, object state )
    {
      IPrintInfo subPrintInfo = this.CellsHostPanel as IPrintInfo;

      if( subPrintInfo != null )
        subPrintInfo.UpdateElementVisibility( horizontalOffset, viewportWidth, state );
    }

    object IPrintInfo.CreateElementVisibilityState()
    {
      IPrintInfo subPrintInfo = this.CellsHostPanel as IPrintInfo;
      if( subPrintInfo == null )
        return null;

      return subPrintInfo.CreateElementVisibilityState();
    }

    #endregion

    #region IDataGridItemContainer Members

    void IDataGridItemContainer.PrepareContainer( DataGridContext dataGridContext, object item )
    {
      this.PrepareContainer( dataGridContext, item );
    }

    void IDataGridItemContainer.ClearContainer()
    {
      this.ClearContainer();
    }

    #endregion

    private DataGridControl m_parentGridUsedForCellsGeneration; // = null
    private Panel m_oldCellsHostPanel; // = null
    private Panel m_cellsHostPanel; // = null
    private Dictionary<string, Cell> m_templateCells = new Dictionary<string, Cell>();
    private BitVector32 m_flags = new BitVector32();

    [Flags]
    private enum RowFlags
    {
      IsBeginningEdition = 1,
      IsEndingEdition = 2,
      IsCancelingEdition = 4,
      IsBeginningEditFromCell = 8,
      IsBeingEditedCache = 16,
      IsClearingContainer = 32,
      IsContainerPrepared = 64,
      IsInitializingInsertionRow = 128
    }
  }
}
