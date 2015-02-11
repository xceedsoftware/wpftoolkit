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
  internal sealed class TableViewFullPageInfo : TableViewPageInfo
  {
    #region Constructor

    internal TableViewFullPageInfo( int start, int end, double size )
    {
      if( start < 0 )
        throw new ArgumentException( "start must be greater than or equal to zero.", "start" );

      if( end < 0 )
        throw new ArgumentException( "end must be greater than or equal to zero.", "end" );

      if( end < start )
        throw new ArgumentException( "end must be greater than or equal to start.", "end" );

      if( size < 0 )
        throw new ArgumentException( "size must be greater than or equal to zero.", "size" );

      m_start = start;
      m_end = end;
      m_size = size;
    }

    #endregion

    #region Start Property

    public override int Start
    {
      get
      {
        return m_start;
      }
    }

    private readonly int m_start;

    #endregion

    #region End Property

    public override int End
    {
      get
      {
        return m_end;
      }
    }

    private readonly int m_end;

    #endregion

    #region Length Property

    public override int Length
    {
      get
      {
        return m_end - m_start + 1;
      }
    }

    #endregion

    #region Size Property

    public override double Size
    {
      get
      {
        return m_size;
      }
    }

    private readonly double m_size;

    #endregion

    public override bool TryGetStart( out int value )
    {
      value = m_start;

      return true;
    }

    public override bool TryGetEnd( out int value )
    {
      value = m_end;

      return true;
    }

    protected override int GetHashCodeImpl()
    {
      return ( m_start ^ m_end );
    }

    protected override bool EqualsImpl( TableViewPageInfo obj )
    {
      var page = obj as TableViewFullPageInfo;
      if( object.ReferenceEquals( page, null ) )
        return false;

      return ( page.m_start == m_start )
          && ( page.m_end == m_end )
          && ( page.m_size == m_size );
    }
  }
}
