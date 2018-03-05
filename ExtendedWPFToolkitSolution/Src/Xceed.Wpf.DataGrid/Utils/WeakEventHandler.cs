/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

/**************************************************************
 * 
 * This code is based on public domain code written by Paul Stovell. 
 * All copyrights and other rights have been waived. 
 * 
 * ************************************************************/

using System;
using System.ComponentModel;
using System.Reflection;

namespace Xceed.Wpf.DataGrid.Utils
{
  [Obsolete( "This class is inefficient. You should derive from System.Windows.WeakEventManager instead." )]
  [Browsable( false )]
  [EditorBrowsable( EditorBrowsableState.Never )]
  public sealed class WeakEventHandler<TEventArgs>
    where TEventArgs : class
  {
    #region Constructors

    public WeakEventHandler( Action<object, TEventArgs> callback )
    {
      if( callback == null )
        throw new ArgumentNullException( "callback" );

      // We keep a WeakReference on the target so that the event
      // won't keep the object alive.
      m_method = callback.Method;
      m_targetReference = new WeakReference( callback.Target );
    }

    #endregion // Constructors

    public void Handler( object sender, TEventArgs e )
    {
      // Only invoke the callback if the target is still alive.
      if( m_targetReference.IsAlive )
      {
        var target = m_targetReference.Target;
        if( target != null )
        {
          // Recreate the callback based on the MethodInfo and target that we received in the ctor.
          var callback = Delegate.CreateDelegate( typeof( Action<object, TEventArgs> ), target, m_method, false ) as Action<object, TEventArgs>;
          if( callback != null )
          {
            // This will invoke the callback that the subscriber wants to be called for the event.
            callback( sender, e );
          }
        }
      }
    }

    #region Private Fields

    private MethodInfo m_method;
    private WeakReference m_targetReference;

    #endregion // Private Fields
  }
}
