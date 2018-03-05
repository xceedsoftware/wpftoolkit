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
using System.Windows.Documents;

namespace Xceed.Wpf.DataGrid
{
  public class CellContentPresenter : ContentPresenter
  {
    static CellContentPresenter()
    {
      CellContentPresenter.MinHeightProperty.OverrideMetadata( typeof( CellContentPresenter ), new FrameworkPropertyMetadata( null, new CoerceValueCallback( CellContentPresenter.CoerceMinHeight ) ) );
      TextElement.FontFamilyProperty.OverrideMetadata( typeof( CellContentPresenter ), new FrameworkPropertyMetadata( new PropertyChangedCallback( CellContentPresenter.InvalidateMinHeight ) ) );
      TextElement.FontSizeProperty.OverrideMetadata( typeof( CellContentPresenter ), new FrameworkPropertyMetadata( new PropertyChangedCallback( CellContentPresenter.InvalidateMinHeight ) ) );
      TextElement.FontStretchProperty.OverrideMetadata( typeof( CellContentPresenter ), new FrameworkPropertyMetadata( new PropertyChangedCallback( CellContentPresenter.InvalidateMinHeight ) ) );
      TextElement.FontStyleProperty.OverrideMetadata( typeof( CellContentPresenter ), new FrameworkPropertyMetadata( new PropertyChangedCallback( CellContentPresenter.InvalidateMinHeight ) ) );
      TextElement.FontWeightProperty.OverrideMetadata( typeof( CellContentPresenter ), new FrameworkPropertyMetadata( new PropertyChangedCallback( CellContentPresenter.InvalidateMinHeight ) ) );

      m_sContentBinding = new Binding();
      m_sContentBinding.RelativeSource = RelativeSource.TemplatedParent;
      m_sContentBinding.Mode = BindingMode.OneWay;
      m_sContentBinding.Path = new PropertyPath( Cell.ContentProperty );

      m_sContentTemplateBinding = new Binding();
      m_sContentTemplateBinding.RelativeSource = RelativeSource.TemplatedParent;
      m_sContentTemplateBinding.Mode = BindingMode.OneWay;
      m_sContentTemplateBinding.Path = new PropertyPath( Cell.CoercedContentTemplateProperty );

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

    public CellContentPresenter()
    {
      this.Resources.Add( typeof( TextBlock ), m_sTextBlockStyle );
      this.DataContext = null;

      this.SetCurrentValue( CellContentPresenter.MinHeightProperty, 0d );
    }

    public override void EndInit()
    {
      base.EndInit();

      BindingOperations.SetBinding( this, CellContentPresenter.ContentProperty, m_sContentBinding );
      BindingOperations.SetBinding( this, CellContentPresenter.ContentTemplateProperty, m_sContentTemplateBinding );
    }

    private static object CoerceMinHeight( DependencyObject sender, object value )
    {
      var self = sender as CellContentPresenter;
      if( self == null )
        return value;

      return self.CoerceMinHeight( new Thickness(), value );
    }

    private static void InvalidateMinHeight( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as CellContentPresenter;
      if( self == null )
        return;

      self.CoerceValue( CellContentPresenter.MinHeightProperty );
    }

    private static Binding m_sContentTemplateBinding;
    private static Binding m_sContentBinding;
    private static Style m_sTextBlockStyle;
  }
}
