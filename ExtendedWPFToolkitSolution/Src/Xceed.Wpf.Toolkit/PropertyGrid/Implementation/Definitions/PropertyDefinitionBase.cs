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
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Converters;
using System.Windows;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public abstract class PropertyDefinitionBase : DependencyObject
  {
    private IList _targetProperties;
    private bool _isLocked;

    internal PropertyDefinitionBase()
    {
      _targetProperties = new List<object>();
    }

    [TypeConverter(typeof(ListConverter))]
    public IList TargetProperties 
    {
      get { return _targetProperties; }
      set 
      {
        if( this.IsLocked )
          throw new InvalidOperationException( @"Cannot modify TargetProperties once the definition has beed added to a collection." );

        _targetProperties = value; 
      }
    }

    internal bool IsLocked
    {
      get{ return _isLocked; }
    }

    internal virtual void Lock()
    {
      if( this.IsLocked )
        return;

      // Just create a new copy of the properties target to ensure 
      // that the list doesn't ever get modified.

      List<object> newList = new List<object>();
      if( _targetProperties != null )
      {
        foreach( object p in _targetProperties )
        {
          object prop = p;
          // Convert all TargetPropertyType to Types
          var targetType = prop as TargetPropertyType;
          if( targetType != null )
          {
            prop = targetType.Type;
          }
          newList.Add( prop );
        }
      }

      _targetProperties = new ReadOnlyCollection<object>( newList );
      _isLocked = true;
    }
  }
}
