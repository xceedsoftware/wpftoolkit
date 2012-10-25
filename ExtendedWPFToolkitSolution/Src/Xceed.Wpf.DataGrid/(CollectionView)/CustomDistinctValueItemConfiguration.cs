/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class CustomDistinctValueItemConfiguration
  {
    public CustomDistinctValueItemConfiguration()
    {
    }

    public object Title
    {
      get;
      set;
    }

    public event EventHandler<CustomDistinctValueItemFilterEventArgs> Filter
    {
      add
      {
        EventHandler<CustomDistinctValueItemFilterEventArgs> eventHandler = m_filter;

        if( eventHandler != null )
        {
          throw new DataGridException( "A Filter event is already registered for this custom distinct value configuration." );
        }
        else
        {
          m_filter = value;
        }
      }
      remove
      {
        // The event handler was not registered on this configuration, ignore it
        if( ( value != null ) && ( value != m_filter ) )
          return;

        if( m_filter != null )
          EventHandler<CustomDistinctValueItemFilterEventArgs>.Remove( m_filter, value );
      }
    }

    private event EventHandler<CustomDistinctValueItemFilterEventArgs> m_filter;

    internal void RaiseCustomDistinctValueItemSelectedEvent( CustomDistinctValueItemFilterEventArgs args )
    {
      if( m_filter != null )
        m_filter( this, args );
    }
  }
}
