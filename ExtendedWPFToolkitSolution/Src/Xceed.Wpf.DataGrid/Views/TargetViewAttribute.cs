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
using System.Text;

namespace Xceed.Wpf.DataGrid.Views
{
  [AttributeUsage( AttributeTargets.Class, Inherited = true, AllowMultiple = true )]
  public sealed class TargetViewAttribute : Attribute
  {
    public TargetViewAttribute( Type viewType )
    {
      if( !typeof( ViewBase ).IsAssignableFrom( viewType ) )
        throw new ArgumentException( "The specified view type must be derived from ViewBase.", "viewType" );

      m_viewType = viewType;
    }

    public Type ViewType
    {
      get
      {
        return m_viewType;
      }
    }

    private Type m_viewType;
  }
}
