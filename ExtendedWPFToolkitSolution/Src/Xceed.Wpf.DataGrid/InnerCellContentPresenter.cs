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
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public class InnerCellContentPresenter : ContentPresenter
  {
    static InnerCellContentPresenter()
    {
      Binding trimmingBinding = new Binding();
      trimmingBinding.Path = new PropertyPath( "(0).(1).(2)",
        Cell.ParentCellProperty,
        Cell.ParentColumnProperty,
        ColumnBase.TextTrimmingProperty );
      trimmingBinding.Mode = BindingMode.OneWay;
      trimmingBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      Binding wrappingBinding = new Binding();
      wrappingBinding.Path = new PropertyPath( "(0).(1).(2)",
        Cell.ParentCellProperty,
        Cell.ParentColumnProperty,
        ColumnBase.TextWrappingProperty );
      wrappingBinding.Mode = BindingMode.OneWay;
      wrappingBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );

      m_sTextBlockStyle = new Style( typeof( TextBlock ) );
      m_sTextBlockStyle.Setters.Add( new Setter( TextBlock.TextTrimmingProperty, trimmingBinding ) );
      m_sTextBlockStyle.Setters.Add( new Setter( TextBlock.TextWrappingProperty, wrappingBinding ) );
    }

    public InnerCellContentPresenter()
    {
      //ContentPresenter.ContentProperty.OverrideMetadata( typeof( InnerCellContentPresenter ), new PropertyMetadata( defaultContent ) );
      this.Resources.Add( typeof( TextBlock ), m_sTextBlockStyle );
    }

    private static Style m_sTextBlockStyle;
  }
}
