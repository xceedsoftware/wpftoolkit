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
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Input;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid
{
  public abstract class DataGridItemsHost : FrameworkElement, ICustomVirtualizingPanel
  {
    #region Constructors

    static DataGridItemsHost()
    {
      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata(
        typeof( DataGridItemsHost ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( OnParentDataGridControlChanged ) ) );
    }

    #endregion

    #region ParentDataGridControl property

    protected DataGridControl ParentDataGridControl
    {
      get
      {
        return ( this.CachedRootDataGridContext != null ) ? this.CachedRootDataGridContext.DataGridControl : null;
      }
    }

    #endregion

    #region CustomItemContainerGenerator property

    private CustomItemContainerGenerator m_generator;
    private bool m_generatorInitialized; // = false
    protected ICustomItemContainerGenerator CustomItemContainerGenerator
    {
      get
      {
        if( !m_generatorInitialized )
        {
          this.InitializeGenerator();
        }

        return m_generator;
      }
    }

    private void InitializeGenerator()
    {
      DataGridControl dataGridControl = this.ParentDataGridControl;

      if( dataGridControl == null )
        throw new DataGridInternalException( "No DataGridContext set on the DataGridItemsHost while trying to initalize the ItemContainerGenerator" );

      if( m_generator == null )
      {
        m_generator = dataGridControl.CustomItemContainerGenerator;
      }

      Debug.Assert( m_generator != null );

      m_generator.ItemsChanged += this.HandleGeneratorItemsChanged;
      m_generator.ContainersRemoved += this.HandleGeneratorContainersRemoved;

      m_generatorInitialized = true;
    }

    #endregion

    #region CachedRootDataGridContext internal property

    private DataGridContext m_cachedRootDataGridContext;

    internal DataGridContext CachedRootDataGridContext
    {
      get
      {
        // Ensure to get the DataGridContext of the ItemsHost
        // if non was explicitly set
        if( m_cachedRootDataGridContext == null )
        {
          m_cachedRootDataGridContext = DataGridControl.GetDataGridContext( this );
        }

        return m_cachedRootDataGridContext;
      }
    }

    #endregion

    #region Children property

    protected internal IList<UIElement> Children
    {
      get
      {
        if( m_children == null )
        {
          m_children = this.CreateChildCollection();
        }

        return m_children;
      }
    }

    private IList<UIElement> m_children;

    #endregion

    #region Visual Tree Override

    protected override IEnumerator LogicalChildren
    {
      get
      {
        if( m_children != null )
          return m_children.GetEnumerator();

        return null;
      }
    }

    protected override int VisualChildrenCount
    {
      get
      {
        if( m_children == null )
          return 0;

        return m_children.Count;
      }
    }

    protected override Visual GetVisualChild( int index )
    {
      if( m_children == null )
        throw new InvalidOperationException( "An attempt was made to retrieve a visual child when none exists for this element." );

      //TODO (case 117289): Insert ZIndex lookup table checkup here.

      if( ( index < 0 ) || ( index >= m_children.Count ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than or equal to the number of contained UI elements." );

      return m_children[ index ];

    }

    #endregion

    #region  PreviewKeyDown and KeyDown handling overrides

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      switch( e.Key )
      {
        // Handle the System key definition (basically with ALT key pressed)
        case Key.System:
          this.HandlePreviewSystemKey( e );
          break;

        case Key.Tab:
          this.HandlePreviewTabKey( e );
          break;

        case Key.PageUp:
          this.HandlePreviewPageUpKey( e );
          break;

        case Key.PageDown:
          this.HandlePreviewPageDownKey( e );
          break;

        case Key.Home:
          this.HandlePreviewHomeKey( e );
          break;

        case Key.End:
          this.HandlePreviewEndKey( e );
          break;

        case Key.Up:
          this.HandlePreviewUpKey( e );
          break;

        case Key.Down:
          this.HandlePreviewDownKey( e );
          break;

        case Key.Left:
          this.HandlePreviewLeftKey( e );
          break;

        case Key.Right:
          this.HandlePreviewRightKey( e );
          break;

        default:
          base.OnPreviewKeyDown( e );
          break;
      }
    }

    protected virtual void HandlePreviewSystemKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePreviewTabKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext == null )
        return;

      DataGridContext currentDataGridContext = dataGridContext.DataGridControl.CurrentContext;

      if( currentDataGridContext == null )
        return;

      DependencyObject container = currentDataGridContext.GetContainerFromItem( currentDataGridContext.InternalCurrentItem );

      if( container != null )
      {
        KeyboardNavigationMode tabbingMode = KeyboardNavigation.GetTabNavigation( container );

        if( tabbingMode != KeyboardNavigationMode.None )
        {
          if( ( Keyboard.Modifiers == ModifierKeys.None ) || ( Keyboard.Modifiers == ModifierKeys.Shift ) )
          {
            DataGridItemsHost.BringIntoViewKeyboardFocusedElement();

            //Force the "inline" relayout of the panel
            //This has no effect if the panel do not have to be updated.
            this.UpdateLayout();
          }
        }
      }
    }

    protected virtual void HandlePreviewLeftKey( KeyEventArgs e )
    {
      DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
      this.UpdateLayout();
    }

    protected virtual void HandlePreviewRightKey( KeyEventArgs e )
    {
      DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
      this.UpdateLayout();
    }

    protected virtual void HandlePreviewUpKey( KeyEventArgs e )
    {
      DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
      this.UpdateLayout();
    }

    protected virtual void HandlePreviewDownKey( KeyEventArgs e )
    {
      DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
      this.UpdateLayout();
    }

    protected virtual void HandlePreviewPageUpKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePreviewPageDownKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePreviewHomeKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePreviewEndKey( KeyEventArgs e )
    {
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      switch( e.Key )
      {
        // Handle the System key definition (basically with ALT key pressed)
        case Key.System:
          this.HandleSystemKey( e );
          break;

        case Key.Tab:
          this.HandleTabKey( e );
          break;

        case Key.PageUp:
          this.HandlePageUpKey( e );
          break;

        case Key.PageDown:
          this.HandlePageDownKey( e );
          break;

        case Key.Home:
          this.HandleHomeKey( e );
          break;

        case Key.End:
          this.HandleEndKey( e );
          break;

        case Key.Up:
          this.HandleUpKey( e );
          break;

        case Key.Down:
          this.HandleDownKey( e );
          break;

        case Key.Left:
          this.HandleLeftKey( e );
          break;

        case Key.Right:
          this.HandleRightKey( e );
          break;

        default:
          base.OnKeyDown( e );
          break;
      }
    }

    protected virtual void HandleSystemKey( KeyEventArgs e )
    {
    }

    protected virtual void HandleTabKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePageUpKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePageDownKey( KeyEventArgs e )
    {
    }

    protected virtual void HandleHomeKey( KeyEventArgs e )
    {
    }

    protected virtual void HandleEndKey( KeyEventArgs e )
    {
    }

    protected virtual void HandleUpKey( KeyEventArgs e )
    {
      e.Handled = DataGridItemsHost.ProcessMoveFocus( e.Key );
    }

    protected virtual void HandleDownKey( KeyEventArgs e )
    {
      e.Handled = DataGridItemsHost.ProcessMoveFocus( e.Key );
    }

    protected virtual void HandleLeftKey( KeyEventArgs e )
    {
      e.Handled = DataGridItemsHost.ProcessMoveFocus( e.Key );
    }

    protected virtual void HandleRightKey( KeyEventArgs e )
    {
      e.Handled = DataGridItemsHost.ProcessMoveFocus( e.Key );
    }

    #endregion

    #region Protected Methods

    protected virtual IList<UIElement> CreateChildCollection()
    {
      return new ItemsHostUIElementCollection( this );
    }

    protected abstract void OnItemsAdded( GeneratorPosition position, int index, int itemCount );

    protected abstract void OnItemsMoved( GeneratorPosition position, int index, GeneratorPosition oldPosition, int oldIndex, int itemCount, int itemUICount, IList<DependencyObject> affectedContainers );

    protected abstract void OnItemsReplaced( GeneratorPosition position, int index, GeneratorPosition oldPosition, int oldIndex, int itemCount, int itemUICount, IList<DependencyObject> affectedContainers );

    protected abstract void OnItemsRemoved( GeneratorPosition position, int index, GeneratorPosition oldPosition, int oldIndex, int itemCount, int itemUICount, IList<DependencyObject> affectedContainers );

    protected abstract void OnItemsReset();

    protected abstract void OnContainersRemoved( IList<DependencyObject> removedContainers );

    protected virtual void OnParentDataGridControlChanged( DataGridControl oldValue, DataGridControl newValue )
    {
      if( ( m_generator != null ) && ( newValue != null ) )
      {
        if( newValue.CustomItemContainerGenerator != m_generator )
          throw new DataGridInternalException();
      }

      // We must ensure that the old ItemsHost clears the Content of the
      // ItemContainerGenerator to avoid a container in the tree of the
      // old panel is used into the new one.
      //
      // This also ensures to remove every link between the RowSelectorPane 
      // and the containers of the old ItemsHost.
      //
      // This only occurs when not specifying a Theme on the View of the
      // DataGridControl and when the System color scheme is changed.
      if( ( oldValue != null ) && ( newValue == null ) )
      {
        oldValue.CustomItemContainerGenerator.CleanupGenerator( false );
      }

      if( newValue != null )
      {
        //In order to ensure that the ItemsHost has the latest geneartor content ( since we just [re]subscribed to the geneartor events ), 
        //we invalidate the ItemsHost layout. If the grid was just loaded, then the measure pass that is triggered is bound to be of almost
        //no significance (performance wise) since there will be no generation and no preparation of new containers (content does not change).
        this.InvalidateMeasure();
      }
      else
      {
        this.ClearCachedRootDataGridContextAndGenerator();
      }
    }

    protected void InvalidateAutomationPeerChildren()
    {
      DataGridContext dataGridContext = this.CachedRootDataGridContext;

      if( ( dataGridContext == null ) || ( dataGridContext.Peer == null ) )
        return;

      DataGridControl dataGridControl = dataGridContext.DataGridControl;

      if( dataGridControl == null )
        return;

      dataGridControl.QueueDataGridContextPeerChlidrenRefresh( dataGridContext );
    }

    #endregion

    #region Internal Methods

    internal static bool ProcessMoveFocus( Key key )
    {
      bool moveFocusSucceeded = false;

      //1. Determine the direction in which the focus would navigate
      FocusNavigationDirection navDirection;
      switch( key )
      {
        case Key.Down:
          navDirection = FocusNavigationDirection.Down;
          break;
        case Key.Left:
          navDirection = FocusNavigationDirection.Left;
          break;
        case Key.Right:
          navDirection = FocusNavigationDirection.Right;
          break;
        case Key.Up:
          navDirection = FocusNavigationDirection.Up;
          break;
        default:
          throw new DataGridInternalException();
      }

      //2. Call MoveFocus() on the currently focused keyboard element, 
      // since this is call within the OnKeyDown of the DataGridControl, it would normally mean
      // that the focused element is within the DataGridControl
      DependencyObject element = Keyboard.FocusedElement as DependencyObject;
      UIElement uiElement = element as UIElement;

      // If the focused element is not a UIElement (e.g. : Hyperlink), we go up until we find one.
      while( uiElement == null && element != null )
      {
        element = TreeHelper.GetParent( element );
        uiElement = element as UIElement;
      }

      if( uiElement != null )
      {
        uiElement.MoveFocus( new TraversalRequest( navDirection ) );

        moveFocusSucceeded = !( uiElement == Keyboard.FocusedElement );
      }

      return moveFocusSucceeded;
    }

    internal static void BringIntoViewKeyboardFocusedElement()
    {
      FrameworkElement frameworkElement = Keyboard.FocusedElement as FrameworkElement;

      if( frameworkElement != null )
        frameworkElement.BringIntoView();
    }

    internal static UIElement GetItemsHostContainerFromElement( DataGridItemsHost itemsHost, DependencyObject element )
    {
      UIElement uiElement = element as UIElement;

      // If the focused element is not a UIElement (e.g. : Hyperlink), we go up until we find one.
      while( ( uiElement == null ) && ( element != null ) )
      {
        element = TreeHelper.GetParent( element );
        uiElement = element as UIElement;
      }

      if( uiElement == null )
        return null;

      // Try to do a Contains before enumerating
      // to avoid manually enumerating if we
      // already have a container
      if( itemsHost.Children.Contains( uiElement ) )
      {
        return uiElement;
      }
      else
      {
        foreach( UIElement container in itemsHost.Children )
        {
          if( container.IsAncestorOf( uiElement ) )
          {
            return container;
          }
        }
      }

      return null;
    }

    #endregion

    #region Private Methods

    private static void OnParentDataGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridItemsHost itemsHost = sender as DataGridItemsHost;

      if( itemsHost != null )
      {
        DataGridControl newDataGridControl = e.NewValue as DataGridControl;
        DataGridControl oldDataGridControl = e.OldValue as DataGridControl;
        itemsHost.OnParentDataGridControlChanged( oldDataGridControl, newDataGridControl );
      }
    }

    private void ClearCachedRootDataGridContextAndGenerator()
    {
      if( m_generator != null )
      {
        // Clean up the previous generator that was used by the ItemsHost
        m_generator.ItemsChanged -= this.HandleGeneratorItemsChanged;
        m_generator.ContainersRemoved -= this.HandleGeneratorContainersRemoved;
      }
      m_generatorInitialized = false;
      m_cachedRootDataGridContext = null;
    }

    private void HandleGeneratorContainersRemoved( object sender, ContainersRemovedEventArgs e )
    {
      this.OnContainersRemoved( e.RemovedContainers );
    }

    private void HandleGeneratorItemsChanged( object sender, CustomGeneratorChangedEventArgs e )
    {
      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Add:
          {
            this.OnItemsAdded( e.Position, e.Index, e.ItemCount );
            break;
          }
        case NotifyCollectionChangedAction.Move:
          {
            this.OnItemsMoved( e.Position, e.Index, e.OldPosition, e.OldIndex, e.ItemCount, e.ItemUICount, e.RemovedContainers );
            break;
          }
        case NotifyCollectionChangedAction.Remove:
          {
            this.OnItemsRemoved( e.Position, e.Index, e.OldPosition, e.OldIndex, e.ItemCount, e.ItemUICount, e.RemovedContainers );
            break;
          }
        case NotifyCollectionChangedAction.Replace:
          {
            this.OnItemsReplaced( e.Position, e.Index, e.OldPosition, e.OldIndex, e.ItemCount, e.ItemUICount, e.RemovedContainers );
            break;
          }
        case NotifyCollectionChangedAction.Reset:
          {
            this.OnItemsReset();
            break;
          }
        default:
          {
            throw new System.ComponentModel.InvalidEnumArgumentException( "An unknown action was specified." );
          }
      }
    }

    #endregion

    #region ICustomVirtualizingPanel Members

    void ICustomVirtualizingPanel.BringIntoView( int index )
    {
      this.BringIntoViewCore( index );
    }

    protected virtual void BringIntoViewCore( int index )
    {
    }

    #endregion
  }
}
