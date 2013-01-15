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
using System.Text;
using System.Windows;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal partial class DefaultCellEditorSelector : FrameworkElement
  {
    #region Constructors

    static DefaultCellEditorSelector()
    {
      Instance = new DefaultCellEditorSelector();
    } 

    private DefaultCellEditorSelector()
    {
      this.InitializeComponent();
    }

    #endregion

    #region Static Fields

    private static readonly DefaultCellEditorSelector Instance;

    #endregion

    #region TextBoxEditor Public Static Property

    public static CellEditor TextBoxEditorCache;

    public static CellEditor TextBoxEditor
    {
      get
      {
        if( TextBoxEditorCache == null )
          TextBoxEditorCache = DefaultCellEditorSelector.SelectCellEditor( typeof( string ) );

        return TextBoxEditorCache;
      }
    } 

    #endregion

    #region CheckBoxEditor Public Static Property

    public static CellEditor CheckBoxEditorCache;

    public static CellEditor CheckBoxEditor
    {
      get
      {
        if( CheckBoxEditorCache == null )
          CheckBoxEditorCache = DefaultCellEditorSelector.SelectCellEditor( typeof( bool ) );

        return CheckBoxEditorCache;
      }
    } 

    #endregion

    #region DateTimeEditor Public Static Editor

    public static CellEditor DateTimeEditorCache;

    public static CellEditor DateTimeEditor
    {
      get
      {
        if( DateTimeEditorCache == null )
          DateTimeEditorCache = DefaultCellEditorSelector.SelectCellEditor( typeof( DateTime ) );

        return DateTimeEditorCache;
      }
    }

    #endregion

    #region ForeignKeyCellEditor Public Static Property

    private static CellEditor ForeignKeyEditorCache;

    public static CellEditor ForeignKeyCellEditor
    {
      get
      {
        if( ForeignKeyEditorCache == null )
        {
          ForeignKeyEditorCache = DefaultCellEditorSelector.ThreadSafeFindResource( "foreignKeyCellEditor" ) as CellEditor;
          ForeignKeyEditorCache.EditTemplate.Seal();
        }

        return ForeignKeyEditorCache;
      }
    } 

    #endregion

    #region LinqCellEditor Public Static Editor

    private static CellEditor LinqCellEditorCache; 

    public static CellEditor LinqCellEditor
    {
      get
      {
        if( LinqCellEditorCache == null )
        {
          LinqCellEditorCache = DefaultCellEditorSelector.ThreadSafeFindResource( "linqCellEditor" ) as CellEditor;
          LinqCellEditorCache.EditTemplate.Seal();
        }

        return LinqCellEditorCache;
      }
    }

    #endregion

    #region Public Methods

    public static CellEditor SelectCellEditor( Type dataType )
    {
      CellEditor cellEditor = null;

      if( dataType != null )
      {
        if( ( dataType.IsGenericType ) && ( dataType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) )
          dataType = Nullable.GetUnderlyingType( dataType );

        cellEditor = DefaultCellEditorSelector.ThreadSafeTryFindResource( dataType.FullName ) as CellEditor;

        if( cellEditor != null )
          DefaultCellEditorSelector.ThreadSafeTryFreezeEditor( cellEditor );
      }

      return cellEditor;
    } 

    #endregion

    #region Private Methods

    private static object ThreadSafeTryFindResource( string key )
    {
      Func<string, object> func = ( resourceKey => DefaultCellEditorSelector.Instance.TryFindResource( resourceKey ) );

      if( DefaultCellEditorSelector.Instance.CheckAccess() )
        return func( key );

      return DefaultCellEditorSelector.Instance.Dispatcher.Invoke( func, key );
    }

    private static object ThreadSafeFindResource( string key )
    {
      Func<string, object> func = ( resourceKey => DefaultCellEditorSelector.Instance.FindResource( resourceKey ) );

      if( DefaultCellEditorSelector.Instance.CheckAccess() )
        return func( key );

      return DefaultCellEditorSelector.Instance.Dispatcher.Invoke( func, key );
    }

    private static void ThreadSafeTryFreezeEditor( CellEditor cellEditor )
    {
      if( cellEditor == null )
        throw new ArgumentNullException( "cellEditor" );

      if( DefaultCellEditorSelector.Instance.CheckAccess() )
      {
        if( cellEditor.IsFrozen )
          return;

        if( ( cellEditor.EditTemplate != null ) && ( !cellEditor.EditTemplate.IsSealed ) )
          cellEditor.EditTemplate.Seal();

        System.Diagnostics.Debug.Assert( cellEditor.CanFreeze );

        if( cellEditor.CanFreeze )
          cellEditor.Freeze();
      }
      else
      {
        DefaultCellEditorSelector.Instance.Dispatcher.Invoke( 
          new Action<CellEditor>( DefaultCellEditorSelector.ThreadSafeTryFreezeEditor ), cellEditor );
      }
    } 

    #endregion
  }
}
