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
using System.Collections.ObjectModel;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyDefinitionCollection : PropertyDefinitionBaseCollection<PropertyDefinition> { }
  public class EditorDefinitionCollection : PropertyDefinitionBaseCollection<EditorDefinitionBase> { }

  public abstract class PropertyDefinitionBaseCollection<T> : ObservableCollection<T> where T : PropertyDefinitionBase
  {
    internal PropertyDefinitionBaseCollection() { }

    public T this[ object propertyId ]
    {
      get
      {
        foreach( var item in Items )
        {
          if( item.TargetProperties.Contains( propertyId ) )
            return item;
        }

        return null;
      }
    }

    protected override void InsertItem( int index, T item )
    {
      if( item == null )
        throw new InvalidOperationException( @"Cannot insert null items in the collection." );

      item.Lock();
      base.InsertItem( index, item );
    }

    protected override void SetItem( int index, T item )
    {
      if( item == null )
        throw new InvalidOperationException( @"Cannot insert null items in the collection." );

      item.Lock();
      base.SetItem( index, item );
    }
  }
}
