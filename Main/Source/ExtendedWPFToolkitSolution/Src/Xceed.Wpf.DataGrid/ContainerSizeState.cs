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
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal struct ContainerSizeState
  {
    public ContainerSizeState( FrameworkElement container )
    {
      m_width = container.ReadLocalValue( FrameworkElement.WidthProperty );
      m_minWidth = container.ReadLocalValue( FrameworkElement.MinWidthProperty );
      m_maxWidth = container.ReadLocalValue( FrameworkElement.MaxWidthProperty );
      m_height = container.ReadLocalValue( FrameworkElement.HeightProperty );
      m_minHeight = container.ReadLocalValue( FrameworkElement.MinHeightProperty );
      m_maxHeight = container.ReadLocalValue( FrameworkElement.MaxHeightProperty );
    }

    public bool IsEmpty()
    {
      return ( ( this.Height == DependencyProperty.UnsetValue ) &&  
               ( this.Width == DependencyProperty.UnsetValue ) &&  
               ( this.MinHeight == DependencyProperty.UnsetValue ) &&  
               ( this.MinWidth == DependencyProperty.UnsetValue ) &&  
               ( this.MaxHeight == DependencyProperty.UnsetValue ) &&  
               ( this.MaxWidth == DependencyProperty.UnsetValue ) );
    }

    public object Width
    {
      get
      {
        return m_width;
      }
    }

    public object MinWidth
    {
      get
      {
        return m_minWidth;
      }
    }

    public object MaxWidth
    {
      get
      {
        return m_maxWidth;
      }
    }

    public object Height
    {
      get
      {
        return m_height;
      }
    }

    public object MinHeight
    {
      get
      {
        return m_minHeight;
      }
    }

    public object MaxHeight
    {
      get
      {
        return m_maxHeight;
      }
    }

    private object m_width;
    private object m_minWidth;
    private object m_maxWidth;
    private object m_height;
    private object m_minHeight;
    private object m_maxHeight;
  }
}
