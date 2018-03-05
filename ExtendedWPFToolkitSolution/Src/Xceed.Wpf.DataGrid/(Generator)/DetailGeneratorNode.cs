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

namespace Xceed.Wpf.DataGrid
{
  internal partial class DetailGeneratorNode : GeneratorNode
  {
    internal DetailGeneratorNode( DataGridContext dataGridContext, CustomItemContainerGenerator generator )
      : base( null )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      if( generator == null )
        throw new ArgumentNullException( "generator" );

      m_dataGridContext = dataGridContext;
      m_generator = generator;
      m_itemCount = generator.ItemCount;
    }

    public CustomItemContainerGenerator DetailGenerator
    {
      get
      {
        return m_generator;
      }
    }

    public DataGridContext DetailContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    internal override int ItemCount
    {
      get
      {
        return m_itemCount;
      }
    }

    internal override void CleanGeneratorNode()
    {
      base.CleanGeneratorNode();

      m_generator.CleanupGenerator( true );
      m_dataGridContext.CleanDataGridContext();

      m_generator = null;
    }

    public void UpdateItemCount()
    {
      m_itemCount = m_generator.ItemCount;
    }

    private readonly DataGridContext m_dataGridContext;
    private CustomItemContainerGenerator m_generator;
    private int m_itemCount;
  }
}
