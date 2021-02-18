/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Xceed.Wpf.Toolkit.Core
{
  [MarkupExtensionReturnType( typeof( BitmapImage ) )]
  public class ImageUriExtension : PackUriExtension
  {
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Generate an absolute uri by default</remarks>
    public ImageUriExtension()
      :base( UriKind.Absolute )
    {

    }

    public override object ProvideValue( IServiceProvider serviceProvider )
    {
      // Call base class
      Uri uri = ( Uri ) base.ProvideValue( serviceProvider );

      // Create a bitmap image object using the generated uri as the source
      BitmapImage image = new BitmapImage( uri );

      // Provide the image
      return image;
    }
  }
}
