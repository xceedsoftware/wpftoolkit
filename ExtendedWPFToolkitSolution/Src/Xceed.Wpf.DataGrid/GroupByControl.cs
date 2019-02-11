/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Utils.Wpf.DragDrop;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  public class GroupByControl : ItemsControl, IDropTarget
  {
    static GroupByControl()
    {
      // This DefaultStyleKey will only be used in design-time.
      DefaultStyleKeyProperty.OverrideMetadata( typeof( GroupByControl ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( GroupByControl ) ) ) );

      FrameworkElementFactory staircaseFactory = new FrameworkElementFactory( typeof( StaircasePanel ) );
      ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate( staircaseFactory );
      RelativeSource ancestorSource = new RelativeSource( RelativeSourceMode.FindAncestor, typeof( GroupByControl ), 1 );

      Binding binding = new Binding();
      binding.Path = new PropertyPath( GroupByControl.ConnectionLineAlignmentProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLineAlignmentProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( GroupByControl.ConnectionLineOffsetProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLineOffsetProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( GroupByControl.ConnectionLinePenProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLinePenProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( GroupByControl.StairHeightProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.StairHeightProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( GroupByControl.StairSpacingProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.StairSpacingProperty, binding );

      itemsPanelTemplate.Seal();

      ItemsControl.ItemsPanelProperty.OverrideMetadata( typeof( GroupByControl ), new FrameworkPropertyMetadata( itemsPanelTemplate ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( GroupByControl ), new FrameworkPropertyMetadata( new PropertyChangedCallback( GroupByControl.ParentGridControlChangedCallback ) ) );
      DataGridControl.DataGridContextPropertyKey.OverrideMetadata( typeof( GroupByControl ), new FrameworkPropertyMetadata( new PropertyChangedCallback( GroupByControl.DataGridContextChangedCallback ) ) );

      FocusableProperty.OverrideMetadata( typeof( GroupByControl ), new FrameworkPropertyMetadata( false ) );
    }

    #region AllowGroupingModification Property

    public static readonly DependencyProperty AllowGroupingModificationProperty =
        DependencyProperty.Register( "AllowGroupingModification", typeof( bool ), typeof( GroupByControl ), new UIPropertyMetadata( true ) );

    public bool AllowGroupingModification
    {
      get
      {
        return ( bool )this.GetValue( GroupByControl.AllowGroupingModificationProperty );
      }
      set
      {
        this.SetValue( GroupByControl.AllowGroupingModificationProperty, value );
      }
    }

    #endregion AllowGroupingModification Property

    #region AllowSort Property

    public static readonly DependencyProperty AllowSortProperty =
        ColumnManagerRow.AllowSortProperty.AddOwner( typeof( GroupByControl ), new UIPropertyMetadata( true ) );

    public bool AllowSort
    {
      get
      {
        return ( bool )this.GetValue( GroupByControl.AllowSortProperty );
      }
      set
      {
        this.SetValue( GroupByControl.AllowSortProperty, value );
      }
    }

    #endregion AllowSort Property

    #region ConnectionLineAlignment Property

    public static readonly DependencyProperty ConnectionLineAlignmentProperty =
      StaircasePanel.ConnectionLineAlignmentProperty.AddOwner( typeof( GroupByControl ) );

    public ConnectionLineAlignment ConnectionLineAlignment
    {
      get
      {
        return ( ConnectionLineAlignment )this.GetValue( GroupByControl.ConnectionLineAlignmentProperty );
      }
      set
      {
        this.SetValue( GroupByControl.ConnectionLineAlignmentProperty, value );
      }
    }

    #endregion ConnectionLineAlignment Property

    #region ConnectionLineOffset Property

    public static readonly DependencyProperty ConnectionLineOffsetProperty =
      StaircasePanel.ConnectionLineOffsetProperty.AddOwner( typeof( GroupByControl ) );

    public double ConnectionLineOffset
    {
      get
      {
        return ( double )this.GetValue( GroupByControl.ConnectionLineOffsetProperty );
      }
      set
      {
        this.SetValue( GroupByControl.ConnectionLineOffsetProperty, value );
      }
    }

    #endregion ConnectionLineOffset Property

    #region ConnectionLinePen Property

    public static readonly DependencyProperty ConnectionLinePenProperty =
      StaircasePanel.ConnectionLinePenProperty.AddOwner( typeof( GroupByControl ) );

    public Pen ConnectionLinePen
    {
      get
      {
        return ( Pen )this.GetValue( GroupByControl.ConnectionLinePenProperty );
      }
      set
      {
        this.SetValue( GroupByControl.ConnectionLinePenProperty, value );
      }
    }

    #endregion ConnectionLinePen Property

    #region StairHeight Property

    public static readonly DependencyProperty StairHeightProperty =
      StaircasePanel.StairHeightProperty.AddOwner( typeof( GroupByControl ) );

    public double StairHeight
    {
      get
      {
        return ( double )this.GetValue( GroupByControl.StairHeightProperty );
      }
      set
      {
        this.SetValue( GroupByControl.StairHeightProperty, value );
      }
    }

    #endregion StairHeight Property

    #region StairSpacing Property

    public static readonly DependencyProperty StairSpacingProperty =
      StaircasePanel.StairSpacingProperty.AddOwner( typeof( GroupByControl ) );

    public double StairSpacing
    {
      get
      {
        return ( double )this.GetValue( GroupByControl.StairSpacingProperty );
      }
      set
      {
        this.SetValue( GroupByControl.StairSpacingProperty, value );
      }
    }

    #endregion StairSpacing Property

    #region NoGroupContent Property

    public static readonly DependencyProperty NoGroupContentProperty =
      DependencyProperty.Register(
        "NoGroupContent",
        typeof( object ),
        typeof( GroupByControl ),
        new PropertyMetadata( "Drag a column header here to group by that column." ) );

    public object NoGroupContent
    {
      get
      {
        return this.GetValue( GroupByControl.NoGroupContentProperty );
      }
      set
      {
        this.SetValue( GroupByControl.NoGroupContentProperty, value );
      }
    }

    #endregion NoGroupContent Property

    protected override DependencyObject GetContainerForItemOverride()
    {
      return new GroupByItem();
    }

    protected override bool IsItemItsOwnContainerOverride( object item )
    {
      return ( item is GroupByItem );
    }

    protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl grid = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      base.PrepareContainerForItemOverride( element, item );

      if( grid != null )
      {
        GroupByItem groupByItem = ( GroupByItem )element;
        groupByItem.PrepareDefaultStyleKey( grid.GetView() );
      }
    }

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupByControl ) );
    }

    internal bool IsGroupingModificationAllowed
    {
      get
      {
        bool allowGroupingModification = this.AllowGroupingModification;

        if( allowGroupingModification )
        {
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

          if( dataGridContext == null )
          {
            allowGroupingModification = false;
          }
          else
          {
            allowGroupingModification = dataGridContext.Items.CanGroup;
          }
        }

        return allowGroupingModification;
      }
    }

    private void RegisterParentDataGridContext( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
      {
        this.ItemsSource = null;
      }
      else
      {
        this.ItemsSource = dataGridContext.GroupLevelDescriptions;
      }
    }

    private static void ParentGridControlChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl grid = e.NewValue as DataGridControl;
      GroupByControl panel = ( GroupByControl )sender;

      if( grid != null )
        panel.PrepareDefaultStyleKey( grid.GetView() );
    }

    private static void DataGridContextChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridContext dataGridContext = e.NewValue as DataGridContext;
      GroupByControl panel = ( GroupByControl )sender;

      if( dataGridContext != null )
        panel.RegisterParentDataGridContext( dataGridContext );
    }

    #region IDropTarget Members

    bool IDropTarget.CanDropElement( UIElement draggedElement, RelativePoint mousePosition )
    {
      var isAlreadyGroupedBy = false;
      var cell = draggedElement as ColumnManagerCell;

      if( cell != null )
      {
        isAlreadyGroupedBy = GroupingHelper.IsAlreadyGroupedBy( cell );

        var parentColumn = cell.ParentColumn;
        if( ( parentColumn == null ) || ( !parentColumn.AllowGroup ) )
          return false;
      }

      var sourceDetailContext = DataGridControl.GetDataGridContext( this );
      Debug.Assert( sourceDetailContext != null );
      var sourceDetailConfig = ( sourceDetailContext != null ) ? sourceDetailContext.SourceDetailConfiguration : null;

      var draggedElementContext = DataGridControl.GetDataGridContext( draggedElement );
      Debug.Assert( draggedElementContext != null );
      var draggedDetailConfig = ( draggedElementContext != null ) ? draggedElementContext.SourceDetailConfiguration : null;


      var canDrop = ( sourceDetailConfig == draggedDetailConfig ) &&
                    ( sourceDetailContext != null ) &&
                    ( draggedElementContext != null ) &&
                    ( sourceDetailContext.GroupLevelDescriptions == draggedElementContext.GroupLevelDescriptions ) &&
                    ( this.IsGroupingModificationAllowed ) &&
                    ( ( draggedElement is ColumnManagerCell ) || ( draggedElement is GroupByItem ) ) &&
                    ( !isAlreadyGroupedBy );

      if( canDrop && ( cell != null ) )
      {
        canDrop = GroupingHelper.ValidateMaxGroupDescriptions( draggedElementContext );
      }

      return canDrop;
    }

    void IDropTarget.DragEnter( UIElement draggedElement )
    {
    }

    void IDropTarget.DragOver( UIElement draggedElement, RelativePoint mousePosition )
    {
      var cell = draggedElement as ColumnManagerCell;
      if( cell == null )
        return;

      var draggedElementContext = DataGridControl.GetDataGridContext( draggedElement );
      var lastIndex = draggedElementContext.GroupLevelDescriptions.Count - 1;
      if( lastIndex < 0 )
        return;

      var groupByItem = this.ItemContainerGenerator.ContainerFromIndex( lastIndex ) as GroupByItem;
      if( groupByItem == null )
        throw new DataGridInternalException( "groupByItem is null." );

      groupByItem.ShowDropMark( mousePosition );
    }

    void IDropTarget.DragLeave( UIElement draggedElement )
    {
      var cell = draggedElement as ColumnManagerCell;
      if( cell == null )
        return;

      var draggedElementContext = DataGridControl.GetDataGridContext( draggedElement );
      var lastIndex = draggedElementContext.GroupLevelDescriptions.Count - 1;
      if( lastIndex < 0 )
        return;

      var groupByItem = this.ItemContainerGenerator.ContainerFromIndex( lastIndex ) as GroupByItem;
      if( groupByItem == null )
        throw new DataGridInternalException( "groupByItem is null." );

      groupByItem.HideDropMark();
    }

    void IDropTarget.Drop( UIElement draggedElement, RelativePoint mousePosition )
    {
      var cell = draggedElement as ColumnManagerCell;
      if( cell == null )
        return;

      var draggedElementContext = DataGridControl.GetDataGridContext( draggedElement );
      var lastIndex = draggedElementContext.GroupLevelDescriptions.Count - 1;

      if( lastIndex >= 0 )
      {
        var groupByItem = this.ItemContainerGenerator.ContainerFromIndex( lastIndex ) as GroupByItem;
        if( groupByItem == null )
          throw new DataGridInternalException( "groupByItem is null." );

        groupByItem.HideDropMark();
      }

      var dataGridContext = DataGridControl.GetDataGridContext( this );
      var dataGridControl = ( dataGridContext != null ) ? dataGridContext.DataGridControl : null;

      GroupingHelper.AppendNewGroupFromColumnManagerCell( cell, dataGridControl );
    }

    #endregion
  }
}
