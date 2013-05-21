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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  /// <summary>
  /// This class is intended to provide the "Type" target
  /// for property definitions or editor definitions when
  /// using Property Element Syntax.
  /// </summary>
  public sealed class TargetPropertyType
  {
    private Type _type;
    private bool _sealed;

    public Type Type
    {
      get { return _type; }
      set 
      {
        if( _sealed )
          throw new InvalidOperationException(
            string.Format(
            "{0}.Type property cannot be modified once the instance is used",
            typeof( TargetPropertyType ) ) );

        _type = value; 
      }
    }

    internal void Seal()
    {
      if( _type == null )
        throw new InvalidOperationException(
          string.Format( "{0}.Type property must be initialized", typeof( TargetPropertyType ) ) );

      _sealed = true;
    }
  }
}
