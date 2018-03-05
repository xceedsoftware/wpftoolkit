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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class FittedWidthRequestedEventArgs : EventArgs
  {
    #region Value Internal Read-Only Property

    internal Nullable<double> Value
    {
      get
      {
        return m_value;
      }
    }

    internal void SetValue( double value )
    {
      if( m_value.HasValue )
      {
        // We keep the highest of the two values.
        if( m_value.Value >= value )
          return;
      }

      m_value = value;
    }

    private Nullable<double> m_value;

    #endregion
  }

  internal delegate void FittedWidthRequestedEventHandler( object sender, FittedWidthRequestedEventArgs e );
}
