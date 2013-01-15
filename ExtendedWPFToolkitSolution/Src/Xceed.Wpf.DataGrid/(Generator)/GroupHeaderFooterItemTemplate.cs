/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Windows;
using System.Windows.Markup;
using System;

namespace Xceed.Wpf.DataGrid
{
  [ContentProperty("Template")]
  public class GroupHeaderFooterItemTemplate
  {
    public DataTemplate Template
    {
     get
     {
       return m_template;
     }
     set
     {
       if( m_isSealed == true )
         throw new InvalidOperationException( "An attempt was made to modify a GroupHeaderFooterItemTemplate that is currently in use." );

       m_template = value;
     }
    }

    public bool VisibleWhenCollapsed
    {
      get
      {
        return m_visible;
      }
      set
      {
        if( m_isSealed == true )
          throw new InvalidOperationException( "An attempt was made to modify a GroupHeaderFooterItemTemplate that is currently in use." );

        m_visible = value;
      }
    }

    internal void Seal()
    {
      m_isSealed = true;
    }

    private DataTemplate m_template;
    private bool m_visible;
    private bool m_isSealed;

  }
}
