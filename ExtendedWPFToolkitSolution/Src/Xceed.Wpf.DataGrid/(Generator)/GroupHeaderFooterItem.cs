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
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public struct GroupHeaderFooterItem
  {
    #region Static Fields

    public static readonly GroupHeaderFooterItem Empty = new GroupHeaderFooterItem();

    #endregion

    public GroupHeaderFooterItem( CollectionViewGroup collectionViewGroup, object template )
    {
      if( !( template is DataTemplate ) && !( template is GroupHeaderFooterItemTemplate ) )
        throw new ArgumentException( "A GroupHeaderFooterItem can only be created with a DataTemplate or VisibleWhenCollapsed objects.", "template" );

      Debug.Assert( template != null );

      m_template = template;
      m_group = new WeakReference( collectionViewGroup );

      m_hashCode = template.GetHashCode();
      if( collectionViewGroup != null )
      {
        m_hashCode ^= collectionViewGroup.GetHashCode();
      }
    }

    #region Template Property

    public object Template
    {
      get
      {
        return m_template;
      }
    }

    private readonly object m_template;

    #endregion

    #region Group Property

    public CollectionViewGroup Group
    {
      get
      {
        if( m_group != null )
          return m_group.Target as CollectionViewGroup;

        return null;
      }
    }

    private readonly WeakReference m_group;

    #endregion

    public static bool Equals( GroupHeaderFooterItem item1, GroupHeaderFooterItem item2 )
    {
      return ( item1.m_hashCode == item2.m_hashCode )
          && ( item1.m_template == item2.m_template )
          && ( item1.Group == item2.Group );
    }

    public static bool operator ==( GroupHeaderFooterItem item1, GroupHeaderFooterItem item2 )
    {
      return GroupHeaderFooterItem.Equals( item1, item2 );
    }

    public static bool operator !=( GroupHeaderFooterItem item1, GroupHeaderFooterItem item2 )
    {
      return !GroupHeaderFooterItem.Equals( item1, item2 );
    }

    public override bool Equals( object obj )
    {
      if( ( obj == null ) || !( obj is GroupHeaderFooterItem ) )
        return false;

      return this.Equals( ( GroupHeaderFooterItem )obj );
    }

    public bool Equals( GroupHeaderFooterItem item )
    {
      return GroupHeaderFooterItem.Equals( this, item );
    }

    public override int GetHashCode()
    {
      return m_hashCode;
    }

    private readonly int m_hashCode;
  }
}
