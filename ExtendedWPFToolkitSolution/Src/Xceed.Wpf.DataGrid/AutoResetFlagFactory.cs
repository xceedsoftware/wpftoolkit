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
using System.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal static class AutoResetFlagFactory
  {
    public static AutoResetFlag Create()
    {
      return AutoResetFlagFactory.Create( true );
    }

    public static AutoResetFlag Create( bool singleSet )
    {
      if( singleSet )
        return new SingleAutoResetFlag();

      return new MultiAutoResetFlag();
    }

    #region IAutoResetFlag Private Interface

    private interface IAutoResetFlag
    {
      void Set();
      void Unset();
    }

    #endregion

    #region ResetFlagDisposable Private Class

    private sealed class ResetFlagDisposable : IDisposable
    {
      internal ResetFlagDisposable( IAutoResetFlag target )
      {
        Debug.Assert( target != null );

        m_target = target;
        m_target.Set();
      }

      private void Dispose( bool disposing )
      {
        // The disposed method has already been called at least once.
        var target = Interlocked.Exchange( ref m_target, null );
        if( target == null )
          return;

        Debug.Assert( m_target == null );
        Debug.Assert( target != null );

        // We can only get here if this is the first call to Dispose.
        target.Unset();
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      ~ResetFlagDisposable()
      {
        this.Dispose( false );
      }

      private IAutoResetFlag m_target;
    }

    #endregion

    #region SingleAutoResetFlag Private Class

    private sealed class SingleAutoResetFlag : AutoResetFlag, IAutoResetFlag
    {
      public override bool IsSet
      {
        get
        {
          return ( Interlocked.CompareExchange( ref m_isSet, null, null ) == this );
        }
      }

      public override IDisposable Set()
      {
        return new ResetFlagDisposable( this );
      }

      void IAutoResetFlag.Set()
      {
        if( Interlocked.CompareExchange( ref m_isSet, this, null ) != null )
          throw new InvalidOperationException( "The flag is already set." );
      }

      void IAutoResetFlag.Unset()
      {
        Interlocked.CompareExchange( ref m_isSet, null, this );
      }

      private object m_isSet; //null
    }

    #endregion

    #region MultiAutoResetFlag Private Class

    private sealed class MultiAutoResetFlag : AutoResetFlag, IAutoResetFlag
    {
      public override bool IsSet
      {
        get
        {
          return ( m_count > 0 );
        }
      }

      public override IDisposable Set()
      {
        return new ResetFlagDisposable( this );
      }

      void IAutoResetFlag.Set()
      {
        Interlocked.Increment( ref m_count );
      }

      void IAutoResetFlag.Unset()
      {
        Interlocked.Decrement( ref m_count );
      }

      private int m_count; //0
    }

    #endregion
  }
}
