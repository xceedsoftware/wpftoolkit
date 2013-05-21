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
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  /// <summary>
  /// This object is used as the dataItem of a VirtualItemInfo when creating a VirtualItemPage and waiting for the
  /// fetch callback.
  /// </summary>
  internal class EmptyDataItem : INotifyPropertyChanged, ICustomTypeDescriptor
  {
    #region CONSTRUCTORS

    internal EmptyDataItem()
      : base()
    {
    }

    internal EmptyDataItem( int index, VirtualList parentVirtualList )
      : base()
    {
      if( index < 0 )
        throw new ArgumentException( "Index must be greater than or equal to zero.", "index" );

      if( parentVirtualList == null )
        throw new ArgumentNullException( "parentVirtualList" );

      m_index = index;
      m_parentVirtualList = parentVirtualList;
    }

    #endregion CONSTRUCTORS

    #region Index Property

    public int Index
    {
      get
      {
        return m_index;
      }
    }

    #endregion Index Property

    #region INTERNAL PROPERTIES

    internal VirtualList ParentVirtualList
    {
      get
      {
        return m_parentVirtualList;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region Indexer

    [EditorBrowsable( EditorBrowsableState.Never )]
    public object this[ object parameter ]
    {
      get
      {
        return null;
      }
      set
      {
      }
    }

    #endregion

    #region PRIVATE FIELDS

    private int m_index;
    private VirtualList m_parentVirtualList;

    #endregion PRIVATE FIELDS

    #region CONSTANTS

    private static readonly EmptyDataItemPropertyDescriptorCollection DefaultEmptyDataItemPropertyDescriptorCollection = new EmptyDataItemPropertyDescriptorCollection();

    #endregion CONSTANTS

    #region ICustomTypeDescriptor Members

    public AttributeCollection GetAttributes()
    {
      return TypeDescriptor.GetAttributes( this );
    }

    public string GetClassName()
    {
      return TypeDescriptor.GetClassName( this );
    }

    public string GetComponentName()
    {
      return TypeDescriptor.GetComponentName( this );
    }

    public TypeConverter GetConverter()
    {
      return TypeDescriptor.GetConverter( this );
    }

    public EventDescriptor GetDefaultEvent()
    {
      return TypeDescriptor.GetDefaultEvent( this );
    }

    public PropertyDescriptor GetDefaultProperty()
    {
      return TypeDescriptor.GetDefaultProperty( this );
    }

    public object GetEditor( Type editorBaseType )
    {
      return TypeDescriptor.GetEditor( this, editorBaseType );
    }

    public EventDescriptorCollection GetEvents( Attribute[] attributes )
    {
      return TypeDescriptor.GetEvents( this, attributes );
    }

    public EventDescriptorCollection GetEvents()
    {
      return TypeDescriptor.GetEvents( this );
    }

    public PropertyDescriptorCollection GetProperties( Attribute[] attributes )
    {
      // We use a custom PropertyDescriptorCollection that will return the same instance 
      // of an EmptyDataItemPropertyDescriptor for ANY property that will be ask. This is to avoid
      // BindingErrors when an EmptyDataItem is used in the grid.
      return EmptyDataItem.DefaultEmptyDataItemPropertyDescriptorCollection;
    }

    public PropertyDescriptorCollection GetProperties()
    {
      return this.GetProperties( null );
    }

    public object GetPropertyOwner( PropertyDescriptor pd )
    {
      return this;
    }

    #endregion ICustomTypeDescriptor Members

    #region EmptyDataItemPropertyDescriptorCollection Class

    private class EmptyDataItemPropertyDescriptorCollection : PropertyDescriptorCollection
    {
      private static readonly EmptyDataItemPropertyDescriptor DefaultEmptyDataItemPropertyDescriptor = new EmptyDataItemPropertyDescriptor();

      private static readonly EmptyDataItemPropertyDescriptor[] DefaultEmptyDataItemPropertyDescriptorArray = new EmptyDataItemPropertyDescriptor[]
        {
          EmptyDataItemPropertyDescriptorCollection.DefaultEmptyDataItemPropertyDescriptor,
        };

      public EmptyDataItemPropertyDescriptorCollection()
        : base( EmptyDataItemPropertyDescriptorCollection.DefaultEmptyDataItemPropertyDescriptorArray )
      {
      }

      public override PropertyDescriptor Find( string name, bool ignoreCase )
      {
        return EmptyDataItemPropertyDescriptorCollection.DefaultEmptyDataItemPropertyDescriptor;
      }

      public override PropertyDescriptor this[ int index ]
      {
        get
        {
          return EmptyDataItemPropertyDescriptorCollection.DefaultEmptyDataItemPropertyDescriptor;
        }
      }

      public override PropertyDescriptor this[ string name ]
      {
        get
        {
          return EmptyDataItemPropertyDescriptorCollection.DefaultEmptyDataItemPropertyDescriptor;
        }
      }
    }

    #endregion EmptyDataItemPropertyDescriptorCollection Class

    #region EmptyDataItemPropertyDescriptor Class

    private class EmptyDataItemPropertyDescriptor : PropertyDescriptor
    {
      private const string DefaultPropertyName = "Empty";
      private static readonly Type ObjectType = typeof( object );

      public EmptyDataItemPropertyDescriptor()
        : base( EmptyDataItemPropertyDescriptor.DefaultPropertyName, null )
      {
      }

      public override bool CanResetValue( object component )
      {
        return false;
      }

      public override Type ComponentType
      {
        get
        {
          return EmptyDataItemPropertyDescriptor.ObjectType;
        }
      }

      public override object GetValue( object component )
      {
        return null;
      }

      public override bool IsReadOnly
      {
        get
        {
          return false;
        }
      }

      public override Type PropertyType
      {
        get
        {
          return EmptyDataItemPropertyDescriptor.ObjectType;
        }
      }

      public override void ResetValue( object component )
      {
      }

      public override void SetValue( object component, object value )
      {
      }

      public override bool ShouldSerializeValue( object component )
      {
        return false;
      }
    }

    #endregion EmptyDataItemPropertyDescriptor Class

    #region INotifyPropertyChanged Members

    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
      add { }
      remove { }
    }

    #endregion
  }
}
