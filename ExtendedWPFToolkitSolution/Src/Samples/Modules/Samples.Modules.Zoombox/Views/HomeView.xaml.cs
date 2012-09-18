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

using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Windows;

namespace Samples.Modules.Zoombox.Views
{
  /// <summary>
  /// Interaction logic for HomeView.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class HomeView : DemoView
  {
    public HomeView()
    {
      InitializeComponent();
    }

    private void AdjustAnimationDuration( object sender, RoutedPropertyChangedEventArgs<double> e )
    {
      Slider slider = sender as Slider;
      if( slider == null )
        return;

      zoombox.AnimationDuration = TimeSpan.FromMilliseconds( slider.Value );
    }

    private void CoerceAnimationRatios( object sender, RoutedPropertyChangedEventArgs<double> e )
    {
      Slider slider = sender as Slider;
      if( slider == null )
        return;

      Slider otherRatio = ( sender == this.AccelerationSlider ) ? this.DecelerationSlider : this.AccelerationSlider;

      if( slider.Value + otherRatio.Value > 1 )
      {
        otherRatio.Value = 1 - slider.Value;
      }
    }
  }

  public abstract class SimpleConverter : IValueConverter
  {
    protected abstract object Convert( object value );

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      return this.Convert( value );
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }

  public class ViewNameConverter : SimpleConverter
  {
    protected override object Convert( object value )
    {
      return value.ToString().Remove( 0, 13 );
    }
  }


  public class ViewFinderConverter : SimpleConverter
  {
    protected override object Convert( object value )
    {
      return ( value != null ) ? value.GetType().Name : null;
    }
  }

  public class RectConverter : SimpleConverter
  {
    protected override object Convert( object value )
    {
      return string.Format( "({0}),({1})",
        PointConverter.ConvertPoint( ( ( Rect )value ).TopLeft ),
        PointConverter.ConvertPoint( ( ( Rect )value ).BottomRight ) );
    }
  }

  public class PointConverter : SimpleConverter
  {
    protected override object Convert( object value )
    {
      return PointConverter.ConvertPoint( ( Point )value );
    }

    public static string ConvertPoint( Point point )
    {
      return string.Format( "{0},{1}",
        Math.Round( point.X ), Math.Round( point.Y ) );
    }
  }

  public class ViewStackCountConverter : SimpleConverter
  {
    protected override object Convert( object value )
    {
      return ( ( int )value ) - 1;
    }
  }  


}
