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

namespace Xceed.Wpf.DataGrid.Views
{
  internal abstract class TableViewPageInfo : IEquatable<TableViewPageInfo>
  {
    #region Static Fields

    internal static readonly TableViewPageInfo Empty = new EmptyPageInfo();

    #endregion

    #region Constructor

    protected TableViewPageInfo()
    {
    }

    #endregion

    #region Start Property

    public abstract int Start
    {
      get;
    }

    #endregion

    #region End Property

    public abstract int End
    {
      get;
    }

    #endregion

    #region Length Property

    public abstract int Length
    {
      get;
    }

    #endregion

    #region Size Property

    public abstract double Size
    {
      get;
    }

    #endregion

    public static bool operator ==( TableViewPageInfo objA, TableViewPageInfo objB )
    {
      if( object.ReferenceEquals( objA, null ) )
        return object.ReferenceEquals( objB, null );

      return objA.Equals( objB );
    }

    public static bool operator !=( TableViewPageInfo objA, TableViewPageInfo objB )
    {
      return !( objA == objB );
    }

    public sealed override int GetHashCode()
    {
      return this.GetHashCodeImpl();
    }

    public sealed override bool Equals( object obj )
    {
      return this.Equals( obj as TableViewPageInfo );
    }

    public bool Equals( TableViewPageInfo obj )
    {
      if( object.ReferenceEquals( obj, null ) )
        return false;

      if( object.ReferenceEquals( obj, this ) )
        return true;

      if( obj.GetHashCode() != this.GetHashCode() )
        return false;

      return this.EqualsImpl( obj );
    }

    public abstract bool TryGetStart( out int value );
    public abstract bool TryGetEnd( out int value );

    protected abstract int GetHashCodeImpl();
    protected abstract bool EqualsImpl( TableViewPageInfo obj );

    #region EmptyPageInfo Private Class

    private sealed class EmptyPageInfo : TableViewPageInfo
    {
      public override int Start
      {
        get
        {
          throw new NotSupportedException();
        }
      }

      public override int End
      {
        get
        {
          throw new NotSupportedException();
        }
      }

      public override int Length
      {
        get
        {
          return 0;
        }
      }

      public override double Size
      {
        get
        {
          return 0d;
        }
      }

      protected override int GetHashCodeImpl()
      {
        return 0;
      }

      protected override bool EqualsImpl( TableViewPageInfo obj )
      {
        return object.ReferenceEquals( obj, this );
      }

      public override bool TryGetStart( out int value )
      {
        value = 0;

        return false;
      }

      public override bool TryGetEnd( out int value )
      {
        value = 0;

        return false;
      }
    }

    #endregion
  }
}
