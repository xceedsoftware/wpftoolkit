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
