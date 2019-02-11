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
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal abstract class PropertyDescription
  {
    internal abstract string Name
    {
      get;
    }

    internal virtual string DisplayName
    {
      get
      {
        return this.Name;
      }
    }

    internal abstract Type DataType
    {
      get;
    }

    internal virtual string Path
    {
      get
      {
        return null;
      }
    }

    internal virtual string XPath
    {
      get
      {
        return null;
      }
    }

    internal virtual PropertyDescriptor PropertyDescriptor
    {
      get
      {
        return null;
      }
    }

    internal virtual DataGridForeignKeyDescription ForeignKeyDescription
    {
      get
      {
        return null;
      }
    }

    internal virtual bool IsReadOnly
    {
      get
      {
        return true;
      }
    }

    internal virtual bool OverrideReadOnlyForInsertion
    {
      get
      {
        return false;
      }
    }

    internal virtual bool SupportDBNull
    {
      get
      {
        return false;
      }
    }

    internal virtual bool IsBrowsable
    {
      get
      {
        return true;
      }
    }

    internal virtual bool IsDisplayable
    {
      get
      {
        return false;
      }
    }

    internal virtual bool IsSubRelationship
    {
      get
      {
        return false;
      }
    }

    internal virtual bool IsDataGridUnboundItemProperty
    {
      get
      {
        return false;
      }
    }

    internal virtual PropertyRouteSegment ToPropertyRouteSegment()
    {
      var route = PropertyRouteParser.Parse( this.Name );
      if( ( route == null ) || ( route.Parent != null ) )
        return new PropertyRouteSegment( PropertyRouteSegmentType.Property, this.Name );

      return route.Current;
    }
  }
}
