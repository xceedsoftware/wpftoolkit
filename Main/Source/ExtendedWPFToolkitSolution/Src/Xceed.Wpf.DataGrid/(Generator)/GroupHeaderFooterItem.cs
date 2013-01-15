/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Automation;

namespace Xceed.Wpf.DataGrid
{
  public struct GroupHeaderFooterItem
  {
    #region CONSTRUCTORS

    public GroupHeaderFooterItem( CollectionViewGroup collectionViewGroup, object template )
    {
      if( !( template is DataTemplate ) && !( template is GroupHeaderFooterItemTemplate ) )
        throw new ArgumentException( "A GroupHeaderFooterItem can only be created with a DataTemplate or VisibleWhenCollapsed objects.", "template" );

      m_template = template;
      m_weakGroup = new WeakReference( collectionViewGroup );
    }

    #endregion CONSTRUCTORS

    #region Template Property

    public object Template
    {
      get
      {
        return m_template;
      }
    }

    private object m_template;

    #endregion Template Property

    #region Group Property

    public CollectionViewGroup Group
    {
      get
      {
        if( m_weakGroup.IsAlive )
          return m_weakGroup.Target as CollectionViewGroup;

        if( m_weakGroup.Target != null )
          m_weakGroup.Target = null;

        return null;
      }
    }

    private WeakReference m_weakGroup;

    #endregion Group Property

    #region PUBLIC METHODS

    public static bool Equals( GroupHeaderFooterItem item1, GroupHeaderFooterItem item2 )
    {
      return ( ( item1.m_template == item2.m_template ) && ( item1.Group == item2.Group ) );
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
      if( ( obj == null ) || ( !( obj is GroupHeaderFooterItem ) ) )
        return false;

      return this.Equals( ( GroupHeaderFooterItem )obj );
    }

    public bool Equals( GroupHeaderFooterItem item )
    {
      return GroupHeaderFooterItem.Equals( this, item );
    }

    public override int GetHashCode()
    {
      int hashCode = 0;

      if( m_template != null )
        hashCode ^= m_template.GetHashCode();

      CollectionViewGroup group = this.Group;

      if( group != null )
        hashCode ^= group.GetHashCode();

      return hashCode;
    }

    #endregion PUBLIC METHODS

    public static readonly GroupHeaderFooterItem Empty = new GroupHeaderFooterItem();
  }
}
