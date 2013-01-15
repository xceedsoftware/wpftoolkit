/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

namespace Xceed.Wpf.Toolkit.Primitives
{
  internal struct HsvColor
  {
    public double H;
    public double S;
    public double V;

    public HsvColor( double h, double s, double v )
    {
      H = h;
      S = s;
      V = v;
    }
  }
}
