/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

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
    /// Initializes a new instance of the SpinEventArgs class.
    /// </summary>
    /// <param name="direction">Spin direction.</param>
    public SpinEventArgs( SpinDirection direction )
      : base()
    {
      Direction = direction;
    }
  }
}
