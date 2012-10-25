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
