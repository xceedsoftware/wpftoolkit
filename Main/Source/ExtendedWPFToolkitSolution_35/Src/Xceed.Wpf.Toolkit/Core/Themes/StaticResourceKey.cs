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
using System.Windows;

namespace Xceed.Wpf.Toolkit.Themes
{
  public sealed class StaticResourceKey : ResourceKey
  {
    private string _key;
    public string Key
    {
      get
      {
        return _key;
      }
    }

    private Type _type;
    public Type Type
    {
      get
      {
        return _type;
      }
    }

    public StaticResourceKey( Type type, string key )
    {
      _type = type;
      _key = key;
    }

    public override System.Reflection.Assembly Assembly
    {
      get
      {
        return _type.Assembly;
      }
    }
  }
}
