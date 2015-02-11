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

namespace Xceed.Wpf.DataGrid.Views
{
  internal sealed class TableViewStartPageInfo : TableViewPartialPageInfo
  {
    #region Constructor

    internal TableViewStartPageInfo( int value )
    {
      if( value < 0 )
        throw new ArgumentException( "value must be greater than or equal to zero.", "value" );

      m_value = value;
    }

    #endregion

    #region Start Property

    public override int Start
    {
      get
      {
        return m_value;
      }
    }

    private readonly int m_value;

    #endregion

    #region End Property

    public override int End
    {
      get
      {
        throw new NotSupportedException();
      }
    }

    #endregion

    protected override int GetHashCodeImpl()
    {
      return m_value;
    }

    protected override bool EqualsImpl( TableViewPageInfo obj )
    {
      var page = obj as TableViewStartPageInfo;
      if( object.ReferenceEquals( page, null ) )
        return false;

      return ( page.m_value == m_value );
    }

    public override bool TryGetStart( out int value )
    {
      value = m_value;

      return true;
    }

    public override bool TryGetEnd( out int value )
    {
      value = 0;

      return false;
    }
  }
}
