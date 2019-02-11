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
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal partial class DefaultCellEditorSelector : FrameworkElement
  {
    #region Static Fields

    private static DefaultCellEditorSelector s_instance = default( DefaultCellEditorSelector );
    private static readonly List<WeakReference> s_instances = new List<WeakReference>( 1 );

    #endregion

    private DefaultCellEditorSelector()
    {
      this.InitializeComponent();
    }

    #region ForeignKeyCellEditor Public Static Property

    public static CellEditor ForeignKeyCellEditor
    {
      get
      {
        return DefaultCellEditorSelector.GetCellEditor( "foreignKeyCellEditor" );
      }
    }

    #endregion

    #region TextBoxEditor Public Static Property

    public static CellEditor TextBoxEditor
    {
      get
      {
        return DefaultCellEditorSelector.SelectCellEditor( typeof( string ) );
      }
    }

    #endregion

    #region CheckBoxEditor Public Static Property

    public static CellEditor CheckBoxEditor
    {
      get
      {
        return DefaultCellEditorSelector.SelectCellEditor( typeof( bool ) );
      }
    }

    #endregion

    #region DateTimeEditor Public Static Editor

    public static CellEditor DateTimeEditor
    {
      get
      {
        return DefaultCellEditorSelector.SelectCellEditor( typeof( DateTime ) );
      }
    }

    #endregion

    public static CellEditor SelectCellEditor( Type dataType )
    {
      if( dataType == null )
        return null;

      if( dataType.IsGenericType && ( dataType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) )
      {
        dataType = Nullable.GetUnderlyingType( dataType );
      }

      var editor = default( CellEditor );
      if( DefaultCellEditorSelector.TryGetCellEditor( dataType.FullName, out editor ) )
        return editor;

      return null;
    }

    private static DataTemplate GetDataTemplate( object key )
    {
      var resource = default( DataTemplate );
      if( !DefaultCellEditorSelector.TryGetDataTemplate( key, out resource ) )
        throw new KeyNotFoundException( "Resource not found" );

      return resource;
    }

    private static bool TryGetDataTemplate( object key, out DataTemplate value )
    {
      var resource = default( object );
      if( !DefaultCellEditorSelector.TryGetResource( key, out resource ) )
      {
        value = default( DataTemplate );
        return false;
      }
      else
      {
        value = DefaultCellEditorSelector.TrySeal( resource as DataTemplate );
        return ( value != null );
      }
    }

    private static CellEditor GetCellEditor( object key )
    {
      var resource = default( CellEditor );
      if( !DefaultCellEditorSelector.TryGetCellEditor( key, out resource ) )
        throw new KeyNotFoundException( "Resource not found" );

      return resource;
    }

    private static bool TryGetCellEditor( object key, out CellEditor value )
    {
      var resource = default( object );
      if( !DefaultCellEditorSelector.TryGetResource( key, out resource ) )
      {
        value = default( CellEditor );
        return false;
      }
      else
      {
        value = DefaultCellEditorSelector.TryFreeze( resource as CellEditor );
        return ( value != null );
      }
    }

    private static bool TryGetResource( object key, out object value )
    {
      value = null;

      var dispatcher = Dispatcher.CurrentDispatcher;
      if( dispatcher == null )
        return false;

      var target = default( DefaultCellEditorSelector );

      lock( ( ( ICollection )s_instances ).SyncRoot )
      {
        for( int i = s_instances.Count - 1; i >= 0; i-- )
        {
          var instance = s_instances[ i ].Target as DefaultCellEditorSelector;

          if( instance == null )
          {
            s_instances.RemoveAt( i );
          }
          else if( instance.Dispatcher == dispatcher )
          {
            target = instance;

            // We could exit the loop but we don't because we want to clean up the list
            // and keep it small.  We don't want it to become large with lots of empty
            // WeakReference.
            //break;
          }
        }

        if( target == null )
        {
          target = new DefaultCellEditorSelector();
          s_instances.Add( new WeakReference( target ) );
        }

        // We keep a strong reference on the target instance to prevent that instance
        // from being garbage collected.  We assume that we will need that instance
        // again for other calls.  We don't want to have to create a new instance to
        // query for a single resource.
        s_instance = target;
      }

      Debug.Assert( target != null );

      value = target.TryFindResource( key );

      return ( value != null );
    }

    private static DataTemplate TrySeal( DataTemplate template )
    {
      if( ( template != null ) && !template.IsSealed )
      {
        template.Seal();
      }

      return template;
    }

    private static CellEditor TryFreeze( CellEditor editor )
    {
      if( ( editor != null ) && !editor.IsFrozen )
      {
        DefaultCellEditorSelector.TrySeal( editor.EditTemplate );

        if( editor.CanFreeze )
        {
          editor.Freeze();
        }
      }

      return editor;
    }
  }
}
