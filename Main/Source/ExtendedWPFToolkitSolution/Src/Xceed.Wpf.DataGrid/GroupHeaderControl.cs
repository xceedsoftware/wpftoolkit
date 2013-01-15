/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Automation.Peers;
using System.Collections.Specialized;
using System;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  public class GroupHeaderControl : ContentControl, INotifyPropertyChanged, IDataGridItemContainer
  {
    #region Constructors

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
    }

    #endregion

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

    private Group m_group;

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

      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, new PropertyChangedEventArgs( "Group" ) );
      }
    }

    #endregion Group Property

    #region Protected Overrides Methods

    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
    {
      return new FrameworkElementAutomationPeer( this );
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
              dataGridContext.SetCurrent( item, null, -1, dataGridContext.CurrentColumn, true, true, false );
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

    #endregion

    #region Protected Internal Methods

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupHeaderControl ) );
    }

    protected internal virtual void PrepareContainer( DataGridContext dataGridContext, object item )
    {
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

      m_isContainerPrepared = true;
    }

    protected internal virtual void ClearContainer()
    {
      //Do nothing!
      m_isContainerPrepared = false;
    }

    #endregion

    #region Private Static Methods

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl grid = e.NewValue as DataGridControl;
      GroupHeaderControl groupHeaderControl = sender as GroupHeaderControl;

      if( ( groupHeaderControl != null ) && ( grid != null ) )
      {
        groupHeaderControl.PrepareDefaultStyleKey( grid.GetView() );
      }
    }

    #endregion

    #region Private Methods

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

    #endregion

    #region Private Fields

    private bool m_isContainerPrepared;

    #endregion Private Fields

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

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
  }
}
