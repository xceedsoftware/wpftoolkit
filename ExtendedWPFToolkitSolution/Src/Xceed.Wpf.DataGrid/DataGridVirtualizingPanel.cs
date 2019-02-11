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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xceed.Wpf.DataGrid
{
  public abstract class DataGridVirtualizingPanel : VirtualizingPanel
  {
    static DataGridVirtualizingPanel()
    {
      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata(
        typeof( DataGridVirtualizingPanel ), new FrameworkPropertyMetadata( new PropertyChangedCallback( OnParentDataGridControlChanged ) ) );
    }

    public IItemContainerGenerator CustomItemContainerGenerator
    {
      get
      {
        //when generator is "requested", initialize the VPanel.
        if( m_initializedCustomGenerator == false )
        {
          this.InitializeDataGridVirtualizingPanel();
        }

        IItemContainerGenerator retval = null;

        ItemsControl control1 = ItemsControl.GetItemsOwner( this );
        if( ( control1 != null ) && ( m_customGenerator != null ) )
        {
          if( control1.IsGrouping == true )
            throw new NotSupportedException( "GroupStyles are not supported by the DataGridVirtualizingPanel." );

          retval = m_customGenerator;
        }
        else
        {
          retval = this.ItemContainerGenerator;
        }

        return retval;
      }
    }

    #region ItemCount Property

    public int ItemCount
    {
      get
      {
        CustomItemContainerGenerator generator = this.InternalCustomItemContainerGenerator;

        if( generator != null )
        {
          return generator.ItemCount;
        }
        else
        {
          // See how many items there are
          ItemsControl itemsControl = ItemsControl.GetItemsOwner( this );

          int itemCount;

          if( itemsControl != null )
          {
            itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
          }
          else
          {
            itemCount = this.Children.Count;
          }

          return itemCount;
        }
      }
    }

    #endregion

    #region InternalCustomItemContainerGenerator Property

    private CustomItemContainerGenerator InternalCustomItemContainerGenerator
    {
      get
      {
        if( m_initializedCustomGenerator == false )
        {
          this.InitializeDataGridVirtualizingPanel();

        }

        CustomItemContainerGenerator retval = null;
        ItemsControl control1 = ItemsControl.GetItemsOwner( this );
        if( ( control1 != null ) && ( control1.IsGrouping == false ) )
        {
          retval = m_customGenerator;
        }

        return retval;
      }
    }

    #endregion

    #region ItemIndex Attached Property

    public static readonly DependencyProperty ItemIndexProperty =
        DependencyProperty.RegisterAttached( "ItemIndex", typeof( int ), typeof( DataGridVirtualizingPanel ), new UIPropertyMetadata( -1 ) );

    public static int GetItemIndex( DependencyObject obj )
    {
      return ( int )obj.GetValue( DataGridVirtualizingPanel.ItemIndexProperty );
    }

    public static void SetItemIndex( DependencyObject obj, int value )
    {
      obj.SetValue( DataGridVirtualizingPanel.ItemIndexProperty, value );
    }

    #endregion ItemIndex Attached Property

    protected sealed override void OnItemsChanged( object sender, ItemsChangedEventArgs args )
    {
      //I don't want anybody to override this since we might be hooked to the CustomItemContainerGenerator
      base.OnItemsChanged( sender, args );

      //Ok, I only want to process this if i'm NOT hooked to the CustomItemContainerGenerator
      if( this.InternalCustomItemContainerGenerator == null )
      {
        switch( args.Action )
        {
          //In case of a Reset, nothing to do since the Panel base class has cleared the internal children already!
          case NotifyCollectionChangedAction.Reset:
            break;

          //If there was a removal (collapsing)
          //or if there was a Move (edition of a Sorted field)
          case NotifyCollectionChangedAction.Move:
          case NotifyCollectionChangedAction.Remove:
            //remove the concerned range
            int index = args.OldPosition.Index;

            Debug.Assert( index != -1 );

            this.RemoveInternalChildRange( index, args.ItemCount );
            break;

          //if some items were added (expansion)
          case NotifyCollectionChangedAction.Add:
            //invalidate layout so that the items will be inserted in place!
            this.InvalidateMeasure();
            break;

          case NotifyCollectionChangedAction.Replace:
            Debug.Fail( "NotifyCollectionChangedAction.Replace" );
            break;

          default:
            break;
        }
      }
    }

    protected override void OnClearChildren()
    {
      //Stop listening to the events of the System Generator if this one is not being used.
      if( ( this.InternalCustomItemContainerGenerator == null ) && ( this.ItemContainerGenerator != null ) )
      {
        this.ItemContainerGenerator.RemoveAll();
      }

      base.OnClearChildren();
    }

    private void InitializeDataGridVirtualizingPanel()
    {
      DataGridControl dataGridControl = ItemsControl.GetItemsOwner( this ) as DataGridControl;

      if( dataGridControl != null )
      {
        //Retrieve the Grouping Custom Generator
        m_customGenerator = dataGridControl.CustomItemContainerGenerator;

        //Remove all items from the generator, this is to ensure that the panel starts "fresh" with items.
        if( this.InternalChildren.Count > 0 )
        {
          this.RemoveInternalChildRange( 0, this.InternalChildren.Count );
        }
        ( ( IItemContainerGenerator )m_customGenerator ).RemoveAll();

        m_customGenerator.ItemsChanged += OnCustomGeneratorItemsChanged;

        m_initializedCustomGenerator = true;
      }
    }

    private void OnCustomGeneratorItemsChanged( object sender, CustomGeneratorChangedEventArgs e )
    {

      switch( e.Action )
      {
        //In case of a Reset, nothing to do since the Panel base class has cleared the internal children already!
        case NotifyCollectionChangedAction.Reset:
          {
            if( this.InternalChildren.Count > 0 )
            {
              this.RemoveInternalChildRange( 0, this.InternalChildren.Count );
            }
            else
            {
              this.InvalidateMeasure();
            }
          }
          break;

        //If there was a removal (collapsing)
        //or if there was a Move (edition of a Sorted field)
        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Remove:
          {
            var containers = new HashSet<DependencyObject>( e.Containers ?? new DependencyObject[ 0 ] );

            if( containers.Count > 0 )
            {
              var children = this.InternalChildren;
              var from = -1;
              var removeCount = 0;
              var i = 0;

              while( i < children.Count )
              {
                var container = children[ i ];

                if( containers.Contains( container ) )
                {
                  containers.Remove( container );

                  if( from < 0 )
                  {
                    from = i;
                  }

                  removeCount++;
                  i++;
                }
                else if( removeCount > 0 )
                {
                  Debug.Assert( from >= 0 );
                  this.RemoveInternalChildRange( from, removeCount );

                  from = -1;
                  removeCount = 0;
                }
                else
                {
                  i++;
                }
              }

              if( removeCount > 0 )
              {
                Debug.Assert( from >= 0 );
                this.RemoveInternalChildRange( from, removeCount );
              }
            }
            else
            {
              this.InvalidateMeasure();
            }
          }
          break;

        //if some items were added (expansion)
        case NotifyCollectionChangedAction.Add:
          {
            //invalidate layout so that the items will be inserted in place!
            this.InvalidateMeasure();
          }
          break;

        case NotifyCollectionChangedAction.Replace:
          {
            Debug.Fail( "NotifyCollectionChangedAction.Replace" );
          }
          break;

        default:
          break;
      }
    }

    private static void OnParentDataGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridVirtualizingPanel panel = ( DataGridVirtualizingPanel )sender;

      if( ( panel != null ) && ( e.NewValue == null ) && ( panel.m_customGenerator != null ) )
      {
        panel.m_customGenerator.ItemsChanged -= panel.OnCustomGeneratorItemsChanged;
        panel.m_customGenerator = null;
        panel.m_initializedCustomGenerator = false;
      }
    }

    private bool m_initializedCustomGenerator = false;
    private CustomItemContainerGenerator m_customGenerator = null;
  }
}
