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
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Linq.Expressions;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public abstract class DefinitionBase : DependencyObject
  {
    private bool _isLocked;

    internal bool IsLocked
    {
      get { return _isLocked; }
    }

    internal void ThrowIfLocked<TMember>( Expression<Func<TMember>> propertyExpression )
    {
      //In XAML, when using any properties of PropertyDefinition, the error of ThrowIfLocked is always thrown => prevent it !
      if( DesignerProperties.GetIsInDesignMode( this ) )
        return;

      if( this.IsLocked )
      {
        string propertyName = ReflectionHelper.GetPropertyOrFieldName( propertyExpression );
        string message = string.Format(
            @"Cannot modify {0} once the definition has beed added to a collection.",
            propertyName );
        throw new InvalidOperationException( message );
      }
    }

    internal virtual void Lock()
    {
      if( !_isLocked )
      {
        _isLocked = true;
      }
    }
  }
}
