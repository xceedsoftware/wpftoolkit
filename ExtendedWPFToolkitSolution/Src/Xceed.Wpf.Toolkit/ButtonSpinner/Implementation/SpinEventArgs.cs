/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2023 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  public class SpinEventArgs : RoutedEventArgs
  {
    public SpinDirection Direction
    {
      get;
      private set;
    }

    public bool UsingMouseWheel
    {
      get;
      private set;
    }

    public SpinEventArgs( SpinDirection direction )
      : base()
    {
      Direction = direction;
    }

    public SpinEventArgs( RoutedEvent routedEvent, SpinDirection direction )
      : base( routedEvent )
    {
      Direction = direction;
    }

    public SpinEventArgs( SpinDirection direction, bool usingMouseWheel )
      : base()
    {
      Direction = direction;
      UsingMouseWheel = usingMouseWheel;
    }

    public SpinEventArgs( RoutedEvent routedEvent, SpinDirection direction, bool usingMouseWheel )
      : base( routedEvent )
    {
      Direction = direction;
      UsingMouseWheel = usingMouseWheel;
    }
  }
}
