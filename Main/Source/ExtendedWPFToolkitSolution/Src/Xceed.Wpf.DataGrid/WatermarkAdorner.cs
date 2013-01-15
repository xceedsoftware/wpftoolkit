/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Resources;
using System.Security;
using System.Windows.Media.Imaging;

namespace Xceed.Wpf.DataGrid
{
  internal class WatermarkAdorner : Adorner
  {
    public WatermarkAdorner( UIElement adornedElement )
      : base( adornedElement )
    {
      BitmapImage bitmapImage = null;

      try
      {
        Uri uri = new Uri( _XceedVersionInfo.CurrentAssemblyPackUri + ";component/Watermark.png" );
        bitmapImage = new BitmapImage( uri );
        bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
      }
      catch( SecurityException )
      {
      }
      catch( UriFormatException )
      {
        // This will occur during the VisualStudio Toolbox refreshing.
      }

      m_content = 
        new Image()
        {
          Stretch = Stretch.None,
          Source = bitmapImage
        };
      }

    protected override Size ArrangeOverride( Size finalSize )
    {
      m_content.Arrange( new Rect( finalSize ) );
      return finalSize;
    }

    protected override Size MeasureOverride( Size constraint )
    {
      m_content.Measure( constraint );
      return m_content.DesiredSize;
    }

    protected override Visual GetVisualChild( int index )
    {
      return m_content;
    }

    protected override int VisualChildrenCount
    {
      get
      {
        return 1;
      }
    }

    public override GeneralTransform GetDesiredTransform( GeneralTransform transform )
    {
      Size gridRenderSize = this.AdornedElement.RenderSize;
      Size contentDesiredSize = m_content.DesiredSize;

      GeneralTransformGroup result = new GeneralTransformGroup();
      result.Children.Add( base.GetDesiredTransform( transform ) );
      result.Children.Add( new TranslateTransform( gridRenderSize.Width - contentDesiredSize.Width - 9, 4 ) );
      return result;
    }

    private UIElement m_content;
  }
}
