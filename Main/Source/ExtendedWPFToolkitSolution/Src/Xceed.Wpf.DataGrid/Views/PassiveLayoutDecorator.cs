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
using System.Windows.Controls;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Views
{
  public class PassiveLayoutDecorator : Decorator
  {
    #region Axis Property

    public static readonly DependencyProperty AxisProperty =
        DependencyProperty.Register( "Axis", typeof( PassiveLayoutAxis ), typeof( PassiveLayoutDecorator ), new UIPropertyMetadata( PassiveLayoutAxis.Vertical ) );

    public PassiveLayoutAxis Axis
    {
      get
      {
        return ( PassiveLayoutAxis )this.GetValue( PassiveLayoutDecorator.AxisProperty );
      }
      set
      {
        this.SetValue( PassiveLayoutDecorator.AxisProperty, value );
      }
    }

    #endregion Axis Property

    #region RealDesiredSize Internal Property

    internal Size RealDesiredSize
    {
      get
      {
        return m_realDesiredSize;
      }
    }

    private Size m_realDesiredSize;

    #endregion RealDesiredSize Property

    protected override Size MeasureOverride( Size availableSize )
    {
      Size size = base.MeasureOverride( availableSize );
      m_realDesiredSize = size;

      switch( this.Axis )
      {
        case PassiveLayoutAxis.Vertical:
          return new Size( size.Width, 0d );

        case PassiveLayoutAxis.Horizontal:
          return new Size( 0d, size.Height );

        case PassiveLayoutAxis.Both:
          return new Size( 0d, 0d );

        default:
          System.Diagnostics.Debug.Assert( false, "Unknown PassiveLayoutAxis." );
          return size;
      }
    }
  }
}
