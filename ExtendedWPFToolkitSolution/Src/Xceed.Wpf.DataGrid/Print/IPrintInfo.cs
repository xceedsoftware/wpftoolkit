/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid.Print
{
  internal interface IPrintInfo
  {
    double GetPageRightOffset( double horizontalOffset, double viewportWidth );

    void UpdateElementVisibility( double horizontalOffset, double viewportWidth, object state );

    object CreateElementVisibilityState();
  }
}
