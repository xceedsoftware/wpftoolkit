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
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid.Stats
{
  internal class StatFunctionCollection : ObservableCollection<StatFunction>
  {
    internal StatFunctionCollection()
      :base()
    {
    }

    public StatFunction this[ string resultPropertyName ]
    {
      get
      {
        StatFunction statFunction;

        for( int i = 0; i < this.Count; i++ )
        {
          statFunction = this[ i ];

          if( string.Equals( statFunction.ResultPropertyName, resultPropertyName ) )
            return statFunction;
        }

        return null;
      }
    }

    public bool Contains( string resultPropertyName )
    {
      return this[ resultPropertyName ] != null;
    }

    protected override void InsertItem( int index, StatFunction item )
    {
      this.PrepareStatFunction( item );
      base.InsertItem( index, item );
    }

    protected override void SetItem( int index, StatFunction item )
    {
      this.PrepareStatFunction( item );
      base.SetItem( index, item );
    }

    internal StatFunction GetEquivalentOf( StatFunction statFunction )
    {
      int count = this.Count;

      for( int i = 0; i < count; i++ )
      {
        if( StatFunction.AreEquivalents( statFunction, this[ i ] ) )
          return this[ i ];
      }

      return null;
    }

    private void PrepareStatFunction( StatFunction item )
    {
      if( this.Contains( item.ResultPropertyName ) )
        throw new ArgumentException( "A StatFunction with the same ResultPropertyName already exists in the StatFunctionCollection." ); 

      item.Validate();
      item.Seal();
    }
  }
}
