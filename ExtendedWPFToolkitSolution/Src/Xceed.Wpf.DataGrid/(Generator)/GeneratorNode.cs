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
  internal partial class GeneratorNode
  {
    internal GeneratorNode( GeneratorNode parent )
    {
      this.Parent = parent;
    }

    //-------------
    // Events
    internal event EventHandler<ExpansionStateChangedEventArgs> ExpansionStateChanged;

    //-------------
    // Properties 

    internal GeneratorNode Previous;

    internal GeneratorNode Next;

    internal GeneratorNode Parent;

    internal int Level
    {
      get
      {
        int count = 0;

        GeneratorNode runningPtr = this;

        while( runningPtr.Parent != null )
        {
          runningPtr = runningPtr.Parent;
          count++;
        }

        return count;
      }
    }

    internal virtual int ItemCount
    {
      get
      {
        return 0;
      }
    }

    internal bool IsComputedExpanded
    {
      get
      {
        bool expanded = true;

        GeneratorNode runningPtr = this;

        while( runningPtr != null )
        {
          expanded = runningPtr.IsComputedExpandedOverride;
          if( expanded == false )
          {
            break;
          }

          runningPtr = runningPtr.Parent;
        }

        return expanded;
      }
    }

    protected virtual bool IsComputedExpandedOverride
    {
      get
      {
        //default implementation... 
        return true;
      }
    }

    //-------------
    // Methods

    internal virtual void CleanGeneratorNode()
    {
      this.Previous = null;
      this.Parent = null;
      this.Next = null;
    }

    internal void AdjustItemCount( int delta )
    {
      delta = this.AdjustItemCountOverride( delta );

      if( ( this.Parent != null ) && ( delta != 0 ) )
      {
        this.Parent.AdjustItemCount( delta );
      }
    }

    internal void AdjustLeafCount( int delta )
    {
      delta = this.AdjustLeafCountOverride( delta );

      if( ( this.Parent != null ) && ( delta != 0 ) )
      {
        this.Parent.AdjustLeafCount( delta );
      }
    }

    protected virtual int AdjustItemCountOverride( int delta )
    {
      return delta;
    }

    protected virtual int AdjustLeafCountOverride( int delta )
    {
      return delta;
    }

    internal virtual void NotifyExpansionStateChanged( bool isParentExpanded )
    {
    }

    protected void OnExpansionStateChanged( bool newExpansionState, int itemOffset, int count )
    {
      var handler = this.ExpansionStateChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new ExpansionStateChangedEventArgs( newExpansionState, itemOffset, count ) );
    }
  }
}
