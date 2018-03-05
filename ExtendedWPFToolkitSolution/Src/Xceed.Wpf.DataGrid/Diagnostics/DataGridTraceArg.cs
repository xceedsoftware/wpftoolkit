/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Globalization;

namespace Xceed.Wpf.DataGrid.Diagnostics
{
  internal abstract class DataGridTraceArg
  {
    protected abstract string FormatOutput( CultureInfo culture );

    public sealed override string ToString()
    {
      var output = this.FormatOutput( CultureInfo.InvariantCulture );
      if( string.IsNullOrEmpty( output ) )
        return string.Empty;

      return output;
    }
  }
}
