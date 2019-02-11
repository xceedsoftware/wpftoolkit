/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Xceed.Utils.Wpf;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  public class GroupHeaderControl : ContentControl, INotifyPropertyChanged, IDataGridItemContainer
  {
    #region Static Fields

    internal static readonly string GroupPropertyName = PropertyHelper.GetPropertyName( ( GroupHeaderControl g ) => g.Group );

    #endregion

    static GroupHeaderControl()
    {
      // This DefaultStyleKey will only be used in design-time.
      DefaultStyleKeyProperty.OverrideMetadata( typeof( GroupHeaderControl ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( GroupHeaderControl ) ) ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( GroupHeaderControl ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnParentGridControlChanged ) ) );

      KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
        typeof( GroupHeaderControl ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );
    }

    public GroupHeaderControl()
    {
      this.CommandBindings.Add( new CommandBinding( DataGridCommands.ExpandGroup,
                                                    this.OnExpandExecuted,
                                                    this.OnExpandCanExecute ) );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.CollapseGroup,
                                                    this.OnCollapseExecuted,
                                                    this.OnCollapseCanExecute ) );

      this.CommandBindings.Add( new CommandBinding( DataGridCommands.ToggleGroupExpansion,
                                                    this.OnToggleExecuted,
                                                    this.OnToggleCanExecute ) );

      m_itemContainerManager = new DataGridItemContainerManager( this );
    }

    #region Group Internal Attached Property

    internal static readonly DependencyProperty GroupProperty = DependencyProperty.RegisterAttached(
      "Group", typeof( Group ), typeof( GroupHeaderControl ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.Inherits ) );

    internal static Group GetGroup( DependencyObject obj )
    {
      return ( Group )obj.GetValue( GroupHeaderControl.GroupProperty );
    }

    internal static void SetGroup( DependencyObject obj, Group value )
    {
      obj.SetValue( GroupHeaderControl.GroupProperty, value );
    }

    #endregion

    #region Group Property

    public Group Group
    {
      get
      {
        return m_group;
      }
    }

    internal void SetGroup( Group group )
    {
      m_group = group;
      this.Content = m_group;
      GroupHeaderControl.SetGroup( this, group );

      this.RaisePropertyChanged( GroupHeaderControl.GroupPropertyName );
    }

    private Group m_group;

    #endregion

    #region SelectionBackground Property

    public static readonly DependencyProperty SelectionBackgroundProperty = Cell.SelectionBackgroundProperty.AddOwner( typeof( GroupHeaderControl ) );

    public Brush SelectionBackground
    {
      get
      {
        return ( Brush )this.GetValue( GroupHeaderControl.SelectionBackgroundProperty );
      }
      set
      {
        this.SetValue( GroupHeaderControl.SelectionBackgroundProperty, value );
      }
    }

    #endregion SelectionBackground Property

    #region SelectionForeground Property

    public static readonly DependencyProperty SelectionForegroundProperty = Cell.SelectionForegroundProperty.AddOwner( typeof( GroupHeaderControl ) );

    public Brush SelectionForeground
    {
      get
      {
        return ( Brush )this.GetValue( GroupHeaderControl.SelectionForegroundProperty );
      }
      set
      {
        this.SetValue( GroupHeaderControl.SelectionForegroundProperty, value );
      }
    }

    #endregion SelectionForeground Property

    #region InactiveSelectionBackground Property

    public static readonly DependencyProperty InactiveSelectionBackgroundProperty = Cell.InactiveSelectionBackgroundProperty.AddOwner( typeof( GroupHeaderControl ) );

    public Brush InactiveSelectionBackground
    {
      get
      {
        return ( Brush )this.GetValue( GroupHeaderControl.InactiveSelectionBackgroundProperty );
      }
      set
      {
        this.SetValue( GroupHeaderControl.InactiveSelectionBackgroundProperty, value );
      }
    }

    #endregion InactiveSelectionBackground Property

    #region InactiveSelectionForeground Property

    public static readonly DependencyProperty InactiveSelectionForegroundProperty = Cell.InactiveSelectionForegroundProperty.AddOwner( typeof( GroupHeaderControl ) );

    public Brush InactiveSelectionForeground
    {
      get
      {
        return ( Brush )this.GetValue( GroupHeaderControl.InactiveSelectionForegroundProperty );
      }
      set
      {
        this.SetValue( GroupHeaderControl.InactiveSelectionForegroundProperty, value );
      }
    }

    #endregion InactiveSelectionForeground Property

    #region CanBeRecycled Protected Property

    protected virtual bool CanBeRecycled
    {
      get
      {
        if( this.IsKeyboardFocused || this.IsKeyboardFocusWithin )
          return false;

        return m_itemContainerManager.CanBeRecycled;
      }
    }

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      m_itemContainerManager.Update();
    }


    protected override void OnPreviewMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      base.OnPreviewMouseLeftButtonUp( e );
      return;
    }

    protected override void OnIsKeyboardFocusWithinChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnIsKeyboardFocusWithinChanged( e );

      bool newValue = ( bool )e.NewValue;

      if( newValue == true )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext != null )
        {
          object item = dataGridContext.GetItemFromContainer( this );

          if( ( item != null ) && ( dataGridContext.InternalCurrentItem != item ) )
          {
            try
            {
              dataGridContext.SetCurrent( item, null, -1, dataGridContext.CurrentColumn, true, true, false, AutoScrollCurrentItemSourceTriggers.FocusChanged );
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

    protected virtual void PrepareContainer( DataGridContext dataGridContext, object item )
    {
      m_isRecyclingCandidate = false;

      if( m_isContainerPrepared )
        Debug.Fail( "A GroupHeaderControl can't be prepared twice, it must be cleaned before PrepareContainer is called again" );

      Group group = null;
      DataGridContext gridContext = DataGridControl.GetDataGridContext( this );

      if( gridContext != null )
      {
        object dataItem = gridContext.GetItemFromContainer( this );
        if( dataItem != null )
        {
          group = gridContext.GetGroupFromItem( dataItem );
        }
      }

      this.SetGroup( group );

      m_itemContainerManager.Prepare( gridContext, item );

      m_isContainerPrepared = true;
    }

    protected virtual void ClearContainer()
    {
      m_itemContainerManager.Clear( m_isRecyclingCandidate );
      m_isContainerPrepared = false;
    }

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupHeaderControl ) );
    }

    protected internal virtual bool ShouldHandleSelectionEvent( InputEventArgs eventArgs )
    {
      var targetChild = eventArgs.OriginalSource as DependencyObject;

      // If the event is comming from a specific control inside the header (GroupNavigationControl or Collapsed button),
      // ignore the event since it is not for selection purposes that the user targeted this control. 
      var collapsedButtonParent = TreeHelper.FindParent<ToggleButton>( targetChild, true, null, this );
      if( collapsedButtonParent != null )
        return false;

      var groupNavigationControlParent = TreeHelper.FindParent<GroupNavigationControl>( targetChild, true, null, this );
      if( groupNavigationControlParent != null )
        return false;

      return true;
    }

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl grid = e.NewValue as DataGridControl;
      GroupHeaderControl groupHeaderControl = sender as GroupHeaderControl;

      if( ( groupHeaderControl != null ) && ( grid != null ) )
      {
        groupHeaderControl.PrepareDefaultStyleKey( grid.GetView() );
      }
    }

    private void OnExpandCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      if( e.Parameter == null )
      {
        //can execute the Expand command if the group is NOT expanded.
        e.CanExecute = !this.FindGroupCommandTarget( e.OriginalSource ).IsExpanded;
      }
    }

    private void OnExpandExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.FindGroupCommandTarget( e.OriginalSource ).IsExpanded = true;
    }

    private void OnCollapseCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      if( e.Parameter == null )
      {
        //can execute the Collapse command if the group is expanded.
        e.CanExecute = this.FindGroupCommandTarget( e.OriginalSource ).IsExpanded;
      }
    }

    private void OnCollapseExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      this.FindGroupCommandTarget( e.OriginalSource ).IsExpanded = false;
    }

    private void OnToggleCanExecute( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;

      if( e.Parameter == null )
      {
        e.CanExecute = true; //can always toggle
      }
    }

    private void OnToggleExecuted( object sender, ExecutedRoutedEventArgs e )
    {
      Group group = FindGroupCommandTarget( e.OriginalSource );

      group.IsExpanded = !group.IsExpanded;
    }

    // We use an ItemsControl inside the GroupHeaderControl to represent the
    // ancestors (ParentGroups) of this.Group, and each item in this ItemsControl
    // is a Group templated to look like a single, stand-alone GroupHeaderControl.
    //
    // In the ItemTemplate for each Group in the ItemsControl, we have a Border that
    // declares some InputBindings to the group commands. In this case, we want to
    // execute the command on this specific instance of Group, which is the DataContext
    // of the Border.
    private Group FindGroupCommandTarget( object originalSource )
    {
      object dataContext = null;

      FrameworkElement fe = originalSource as FrameworkElement;
      if( fe != null )
      {
        dataContext = fe.DataContext;
      }
      else
      {
        FrameworkContentElement fce = originalSource as FrameworkContentElement;
        if( fce != null )
        {
          dataContext = fce.DataContext;
        }
      }

      Group groupCommandTarget = dataContext as Group;

      return ( groupCommandTarget != null ) ? groupCommandTarget : this.Group;
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion

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
    private bool m_isContainerPrepared;
    private bool m_isRecyclingCandidate;
  }
}
