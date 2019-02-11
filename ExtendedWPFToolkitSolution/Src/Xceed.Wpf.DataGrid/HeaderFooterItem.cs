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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Xceed.Wpf.DataGrid
{
  internal class HeaderFooterItem : ContentPresenter, IDataGridItemContainer
  {
    #region Static Fields

    private static readonly Binding m_sIsCurrentInternalBinding;
    private static readonly Binding m_sItemIndexBinding;
    private static readonly Binding m_sIsBeingEditedInternalBinding;
    private static readonly Binding m_sHasValidationErrorInternalBinding;
    private static readonly Binding m_sRowSelectorVisibleBinding;
    private static readonly Binding m_sRowSelectorStyleBinding;

    #endregion

    static HeaderFooterItem()
    {
      KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( HeaderFooterItem ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );

      HeaderFooterItem.ContainerProperty = ContainerPropertyKey.DependencyProperty;

      m_sItemIndexBinding = new Binding();
      m_sItemIndexBinding.RelativeSource = RelativeSource.Self;
      m_sItemIndexBinding.Path = new PropertyPath( "(0).(1)", HeaderFooterItem.ContainerProperty, DataGridVirtualizingPanel.ItemIndexProperty );
      m_sItemIndexBinding.Mode = BindingMode.OneWay;

      m_sIsCurrentInternalBinding = new Binding();
      m_sIsCurrentInternalBinding.RelativeSource = RelativeSource.Self;
      m_sIsCurrentInternalBinding.Path = new PropertyPath( "(0).(1)", HeaderFooterItem.ContainerProperty, Row.IsCurrentProperty );
      m_sIsCurrentInternalBinding.Mode = BindingMode.OneWay;

      m_sIsBeingEditedInternalBinding = new Binding();
      m_sIsBeingEditedInternalBinding.RelativeSource = RelativeSource.Self;
      m_sIsBeingEditedInternalBinding.Path = new PropertyPath( "(0).(1)", HeaderFooterItem.ContainerProperty, Row.IsBeingEditedProperty );
      m_sIsBeingEditedInternalBinding.Mode = BindingMode.OneWay;

      m_sHasValidationErrorInternalBinding = new Binding();
      m_sHasValidationErrorInternalBinding.RelativeSource = RelativeSource.Self;
      m_sHasValidationErrorInternalBinding.Path = new PropertyPath( "(0).(1)", HeaderFooterItem.ContainerProperty, Row.HasValidationErrorProperty );
      m_sHasValidationErrorInternalBinding.Mode = BindingMode.OneWay;

      m_sRowSelectorVisibleBinding = new Binding();
      m_sRowSelectorVisibleBinding.RelativeSource = RelativeSource.Self;
      m_sRowSelectorVisibleBinding.Path = new PropertyPath( "(0).(1)", HeaderFooterItem.ContainerProperty, RowSelector.VisibleProperty );
      m_sRowSelectorVisibleBinding.Mode = BindingMode.OneWay;

      m_sRowSelectorStyleBinding = new Binding();
      m_sRowSelectorStyleBinding.RelativeSource = RelativeSource.Self;
      m_sRowSelectorStyleBinding.Path = new PropertyPath( "(0).(1)", HeaderFooterItem.ContainerProperty, RowSelector.RowSelectorStyleProperty );
      m_sRowSelectorStyleBinding.Mode = BindingMode.OneWay;
    }

    internal HeaderFooterItem()
    {
      m_itemContainerManager = new DataGridItemContainerManager( this );

      //to prevent creation of the headerfooteritem by anybody else than us.
      BindingOperations.SetBinding( this, HeaderFooterItem.ItemIndexProperty, m_sItemIndexBinding );
      BindingOperations.SetBinding( this, HeaderFooterItem.IsBeingEditedInternalProperty, m_sIsBeingEditedInternalBinding );
      BindingOperations.SetBinding( this, HeaderFooterItem.IsCurrentInternalProperty, m_sIsCurrentInternalBinding );
      BindingOperations.SetBinding( this, HeaderFooterItem.HasValidationErrorInternalProperty, m_sHasValidationErrorInternalBinding );

      BindingOperations.SetBinding( this, RowSelector.VisibleProperty, m_sRowSelectorVisibleBinding );
      BindingOperations.SetBinding( this, RowSelector.RowSelectorStyleProperty, m_sRowSelectorStyleBinding );
    }

    #region IsCurrentInternal Property

    private static readonly DependencyProperty IsCurrentInternalProperty = DependencyProperty.Register(
      "IsCurrentInternal",
      typeof( bool ),
      typeof( HeaderFooterItem ),
      new FrameworkPropertyMetadata(
        ( bool )false,
        new PropertyChangedCallback( OnIsCurrentInternalChanged ) ) );

    private static void OnIsCurrentInternalChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( HeaderFooterItem )d ).OnIsCurrentInternalChanged( e );
    }

    protected virtual void OnIsCurrentInternalChanged( DependencyPropertyChangedEventArgs e )
    {
      this.SetValue( Row.IsCurrentPropertyKey, e.NewValue );
    }

    #endregion

    #region IsBeingEditedInternal Property

    private static readonly DependencyProperty IsBeingEditedInternalProperty = DependencyProperty.Register(
      "IsBeingEditedInternal",
      typeof( bool ),
      typeof( HeaderFooterItem ),
      new FrameworkPropertyMetadata(
        ( bool )false,
        new PropertyChangedCallback( OnIsBeingEditedInternalChanged ) ) );

    private static void OnIsBeingEditedInternalChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( HeaderFooterItem )d ).OnIsBeingEditedInternalChanged( e );
    }

    protected virtual void OnIsBeingEditedInternalChanged( DependencyPropertyChangedEventArgs e )
    {
      this.SetValue( Row.IsBeingEditedPropertyKey, e.NewValue );
    }

    #endregion

    #region HasValidationErrorInternal Property

    private static readonly DependencyProperty HasValidationErrorInternalProperty = DependencyProperty.Register(
      "HasValidationErrorInternal",
      typeof( bool ),
      typeof( HeaderFooterItem ),
      new FrameworkPropertyMetadata(
        ( bool )false,
        new PropertyChangedCallback( OnHasValidationErrorInternalChanged ) ) );

    private static void OnHasValidationErrorInternalChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      ( ( HeaderFooterItem )d ).OnHasValidationErrorInternalChanged( e );
    }

    protected virtual void OnHasValidationErrorInternalChanged( DependencyPropertyChangedEventArgs e )
    {
      this.SetValue( Row.HasValidationErrorPropertyKey, e.NewValue );
    }

    #endregion

    #region IsCurrent Property

    public static readonly DependencyProperty IsCurrentProperty = Row.IsCurrentProperty.AddOwner( typeof( HeaderFooterItem ) );

    public bool IsCurrent
    {
      get
      {
        return ( bool )this.GetValue( IsCurrentProperty );
      }
    }

    #endregion

    #region IsBeingEdited Property

    public static readonly DependencyProperty IsBeingEditedProperty = Row.IsBeingEditedProperty.AddOwner( typeof( HeaderFooterItem ) );

    public bool IsBeingEdited
    {
      get
      {
        return ( bool )this.GetValue( IsBeingEditedProperty );
      }
    }

    #endregion

    #region HasValidationError Property

    public static readonly DependencyProperty HasValidationErrorProperty = Row.HasValidationErrorProperty.AddOwner( typeof( HeaderFooterItem ) );

    public bool HasValidationError
    {
      get
      {
        return ( bool )this.GetValue( HasValidationErrorProperty );
      }
    }

    #endregion

    #region ItemIndex Property

    public static readonly DependencyProperty ItemIndexProperty = DataGridVirtualizingPanel.ItemIndexProperty.AddOwner( typeof( HeaderFooterItem ) );

    #endregion

    #region Container Property

    private static readonly DependencyPropertyKey ContainerPropertyKey = DependencyProperty.RegisterReadOnly(
      "Container",
      typeof( DependencyObject ),
      typeof( HeaderFooterItem ),
      new FrameworkPropertyMetadata( ( DependencyObject )null ) );

    public static readonly DependencyProperty ContainerProperty;

    public DependencyObject Container
    {
      get
      {
        return ( DependencyObject )this.GetValue( ContainerProperty );
      }
    }

    private void SetContainer( DependencyObject value )
    {
      this.SetValue( ContainerPropertyKey, value );
    }

    #endregion

    #region CanBeRecycled Protected Property

    protected virtual bool CanBeRecycled
    {
      get
      {
        if( this.IsKeyboardFocused
          || this.IsKeyboardFocusWithin
          || this.IsBeingEdited )
          return false;

        return m_itemContainerManager.CanBeRecycled;
      }
    }

    #endregion

    #region VisualRootElementType Property

    internal Type VisualRootElementType
    {
      get
      {
        Type elementType = null;
        DataTemplate template = m_initializingDataItem as DataTemplate;

        if( template == null )
        {
          template = this.ContentTemplate;
        }

        if( ( template != null ) && ( template.VisualTree != null ) )
        {
          elementType = template.VisualTree.Type;
        }

        if( elementType == null )
        {
          Visual visual = this.AsVisual();

          if( visual == null )
          {
            //this will force the control to apply the content template "inline"...
            this.ApplyTemplate();

            visual = this.AsVisual();
          }

          Debug.Assert( visual != null );

          if( visual != null )
          {
            elementType = visual.GetType();
          }
        }

        return elementType;
      }
    }

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      m_itemContainerManager.Update();
    }

    public Visual AsVisual()
    {
      if( this.VisualChildrenCount == 0 )
        return null;

      return this.GetVisualChild( 0 );
    }


    protected void PrepareContainer( DataGridContext dataGridContext, object item )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      m_initializingDataGridContext = dataGridContext;
      m_initializingDataItem = item;

      if( !this.IsLoaded )
      {
        if( dataGridContext.DataGridControl.ItemsHost.IsAncestorOf( this ) )
        {
          this.ApplyTemplate();
          this.PrepareContainerWorker();
        }
        else
        {
          // When a HeaderFooterItem is in the FixedHeaders or FixedFooters of the grid,
          // we must prepare the container only when the container is loaded so that the
          // TableView.CanScrollHorizontallyProperty attached property is taken into consideration.
          m_loadedHandler = this.HeaderFooterItem_Loaded;
          this.Loaded += m_loadedHandler;
        }
      }
      else
      {
        this.PrepareContainerWorker();
      }
    }

    protected void ClearContainer()
    {
      if( !m_isRecyclingCandidate )
      {
        // Ensure we removed the HeaderFooterItem_Loaded event handler if it was not yet loaded
        if( m_loadedHandler != null )
        {
          this.Loaded -= m_loadedHandler;
          m_loadedHandler = null;
        }
      }

      m_itemContainerManager.Clear( m_isRecyclingCandidate );
    }

    protected override void OnIsKeyboardFocusWithinChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnIsKeyboardFocusWithinChanged( e );

      //In this case, since the first visual child is not a row, I want to stub the IsCurrent property with the KeyboardFocus.
      Row row = this.AsVisual() as Row;

      if( row == null )
      {
        this.SetValue( IsCurrentInternalProperty, e.NewValue );
      }
    }

    protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      base.OnMouseLeftButtonDown( e );

      if( !e.Handled )
      {
        e.Handled = DataGridControl.SetFocusUIElementHelper( this );
      }
    }

    private void HeaderFooterItem_Loaded( object sender, RoutedEventArgs e )
    {
      if( m_loadedHandler != null )
      {
        this.Loaded -= m_loadedHandler;
        m_loadedHandler = null;
      }

      this.PrepareContainerWorker();
      //Bind properties that CAN be bound (if the DataTemplate instantiated a Row as first visual child )
    }

    private void PrepareContainerWorker()
    {
      m_isRecyclingCandidate = false;

      var rootContainer = this.AsVisual();

      this.SetContainer( rootContainer );

      m_itemContainerManager.Prepare( m_initializingDataGridContext, m_initializingDataItem );

      m_initializingDataGridContext = null;
      m_initializingDataItem = null;
    }

    #region IDataGridItemContainer Members

    bool IDataGridItemContainer.CanBeRecycled
    {
      get
      {
        return this.CanBeRecycled;
      }
    }

    bool IDataGridItemContainer.IsRecyclingCandidate
    {
      get
      {
        return m_isRecyclingCandidate;
      }
      set
      {
        m_isRecyclingCandidate = value;
      }
    }

    void IDataGridItemContainer.PrepareContainer( DataGridContext dataGridContext, object item )
    {
      this.PrepareContainer( dataGridContext, item );
    }

    void IDataGridItemContainer.ClearContainer()
    {
      this.ClearContainer();
    }

    void IDataGridItemContainer.CleanRecyclingCandidate()
    {
      m_itemContainerManager.CleanRecyclingCandidates();
    }

    #endregion

    private readonly DataGridItemContainerManager m_itemContainerManager;
    private DataGridContext m_initializingDataGridContext; // = null
    private object m_initializingDataItem; // = null
    private RoutedEventHandler m_loadedHandler;
    private bool m_isRecyclingCandidate;
  }
}
