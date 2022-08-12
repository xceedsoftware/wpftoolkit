/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2022 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;

namespace Xceed.Wpf.Toolkit.Core
{
  public class VersionResourceDictionary : ResourceDictionary
  {
    public VersionResourceDictionary()
    {
    }

    public VersionResourceDictionary( string assemblyName, string sourcePath )
    : base( assemblyName, sourcePath )
    {

    }


    protected override Uri BuildUri()
    {
      var source = this.SourcePath;
      string uriStr = PackUriExtension.BuildAbsolutePackUriString( this.AssemblyName, _XceedVersionInfo.Version, source );
      return new Uri( uriStr, UriKind.Absolute );
    }
  }
}
