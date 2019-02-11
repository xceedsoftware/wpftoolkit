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
using System.ComponentModel;
using System.Threading;

namespace Xceed.Wpf.DataGrid.Markup
{
  [Browsable( false )]
  [EditorBrowsable( EditorBrowsableState.Never )]
  public sealed class DataGridThemeResourceDictionary : SharedThemeResourceDictionary
  {
    #region Static Fields

    private static Uri s_baseUri; //null

    #endregion

    protected override Uri GetBaseUri()
    {
      if( s_baseUri == null )
      {
        var baseUri = this.CreateBaseUri( this.GetType().Assembly );

        Interlocked.CompareExchange<Uri>( ref s_baseUri, baseUri, null );
      }

      return s_baseUri;
    }
  }
}
