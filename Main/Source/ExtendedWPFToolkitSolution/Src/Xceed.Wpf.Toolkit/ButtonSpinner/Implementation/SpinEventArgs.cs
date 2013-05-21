/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;

namespace Xceed.Wpf.Toolkit
{
  /// <summary>
  /// Provides data for the Spinner.Spin event.
  /// </summary>
  /// <QualityBand>Preview</QualityBand>
  public class SpinEventArgs : RoutedEventArgs
  {
    /// <summary>
    /// Gets the SpinDirection for the spin that has been initiated by the 
    /// end-user.
    /// </summary>
    public SpinDirection Direction
    {
      get;
      private set;
    }

    /// <summary>
    /// Get or set whheter the spin event originated from a mouse wheel event.
    /// </summary>
    public bool UsingMouseWheel
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes a new instance of the SpinEventArgs class.
    /// </summary>
    /// <param name="direction">Spin direction.</param>
    public SpinEventArgs( SpinDirection direction )
      : base()
    {
      Direction = direction;
    }

    public SpinEventArgs( SpinDirection direction, bool usingMouseWheel )
      : base()
    {
      Direction = direction;
      UsingMouseWheel = usingMouseWheel;
    }
  }
}
