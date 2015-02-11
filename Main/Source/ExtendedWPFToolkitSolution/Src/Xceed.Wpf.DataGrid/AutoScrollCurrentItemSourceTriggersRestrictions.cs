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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class AutoScrollCurrentItemSourceTriggersRestrictions
  {
    #region Restrictions Property

    internal AutoScrollCurrentItemSourceTriggers Restrictions
    {
      get
      {
        var result = AutoScrollCurrentItemSourceTriggers.None;

        for( int i = m_restrictions.Count - 1; i >= 0; i-- )
        {
          var item = m_restrictions[ i ].Target as SingleRestriction;

          if( item != null )
          {
            result |= item.Value;
          }
          else
          {
            m_restrictions.RemoveAt( i );
          }
        }

        return result;
      }
    }

    #endregion

    internal IDisposable SetRestriction( AutoScrollCurrentItemSourceTriggers value )
    {
      var disposable = new SingleRestriction( value );

      m_restrictions.Add( new WeakReference( disposable ) );

      return disposable;
    }

    #region Private Fields

    private readonly List<WeakReference> m_restrictions = new List<WeakReference>();

    #endregion

    #region SingleRestriction Private Class

    private sealed class SingleRestriction : IDisposable
    {
      internal SingleRestriction( AutoScrollCurrentItemSourceTriggers value )
      {
        m_value = value;
      }

      internal AutoScrollCurrentItemSourceTriggers Value
      {
        get
        {
          return m_value;
        }
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        m_value = AutoScrollCurrentItemSourceTriggers.None;
      }

      ~SingleRestriction()
      {
        this.Dispose( false );
      }

      private AutoScrollCurrentItemSourceTriggers m_value;
    }

    #endregion
  }
}
