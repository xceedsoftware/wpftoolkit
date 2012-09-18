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

using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class EditorDefinitionCollection : ObservableCollection<EditorDefinition>
  {
    public EditorDefinition this[ string propertyName ]
    {
      get
      {
        foreach( var item in Items )
        {
          if( item.PropertiesDefinitions.Where( x => x.Name == propertyName ).Any() )
            return item;
        }

        return null;
      }
    }

    public EditorDefinition this[ Type targetType ]
    {
      get
      {
        foreach( var item in Items )
        {
          if( item.TargetType == targetType )
            return item;
        }

        return null;
      }
    }
  }
}
