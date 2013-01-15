/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

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
