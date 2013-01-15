/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Collections.ObjectModel;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyDefinitionCollection : ObservableCollection<PropertyDefinition>
  {
    public PropertyDefinition this[ string propertyName ]
    {
      get
      {
        foreach( var item in Items )
        {
          if( item.Name == propertyName )
            return item;
        }

        return null;
      }
    }
  }
}
