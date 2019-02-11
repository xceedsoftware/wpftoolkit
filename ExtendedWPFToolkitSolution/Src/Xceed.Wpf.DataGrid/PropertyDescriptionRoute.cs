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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class PropertyDescriptionRoute
  {
    internal PropertyDescriptionRoute( PropertyDescription description )
      : this( description, null )
    {
    }

    private PropertyDescriptionRoute( PropertyDescription description, PropertyDescriptionRoute parent )
    {
      if( description == null )
        throw new ArgumentNullException( "description" );

      m_description = description;
      m_parent = parent;
    }

    #region Current Property

    internal PropertyDescription Current
    {
      get
      {
        return m_description;
      }
    }

    private readonly PropertyDescription m_description;

    #endregion

    #region Parent Property

    internal PropertyDescriptionRoute Parent
    {
      get
      {
        return m_parent;
      }
    }

    private readonly PropertyDescriptionRoute m_parent;

    #endregion

    internal static PropertyDescriptionRoute Combine( PropertyDescription description, PropertyDescriptionRoute ancestors )
    {
      return new PropertyDescriptionRoute( description, ancestors );
    }

    internal static PropertyDescriptionRoute Combine( PropertyDescriptionRoute descendants, PropertyDescription description )
    {
      if( description == null )
        return descendants;

      return PropertyDescriptionRoute.Combine( descendants, new PropertyDescriptionRoute( description ) );
    }

    internal static PropertyDescriptionRoute Combine( PropertyDescriptionRoute descendants, PropertyDescriptionRoute ancestors )
    {
      if( ancestors == null )
        return descendants;

      if( descendants == null )
        return ancestors;

      return PropertyDescriptionRoute.Combine(
               descendants.Current,
               PropertyDescriptionRoute.Combine( descendants.Parent, ancestors ) );
    }
  }
}
