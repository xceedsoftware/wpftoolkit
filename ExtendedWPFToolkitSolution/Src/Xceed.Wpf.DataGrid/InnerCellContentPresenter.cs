/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
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
      m_sTextBlockStyle.Seal();
    }

    public InnerCellContentPresenter()
    {
      //ContentPresenter.ContentProperty.OverrideMetadata( typeof( InnerCellContentPresenter ), new PropertyMetadata( defaultContent ) );
      this.Resources.Add( typeof( TextBlock ), m_sTextBlockStyle );
    }

    private static Style m_sTextBlockStyle;
  }
}
