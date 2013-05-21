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
using System.Text;
using System.Windows.Markup;

namespace Xceed.Wpf.DataGrid
{
  [ContentProperty("GroupConfiguration" )]
  public class LevelGroupConfigurationSelectorItem
  {
    #region Level Property

    public int Level
    {
      get
      {
        return m_level;
      }
      set
      {
        if( m_isSealed == true )
          throw new InvalidOperationException( "An attempt was made to modify the level of a LevelGroupConfigurationSelectorItem that has already been added to a LevelGroupConfigurationSelector." );

        m_level = value;
      }
    }

    private int m_level;

    #endregion

    #region GroupConfiguration Property

    public GroupConfiguration GroupConfiguration
    {
      get
      {
        return m_groupConfig;
      }
      set
      {
        m_groupConfig = value;
      }
    }

    private GroupConfiguration m_groupConfig;

    #endregion

    #region IsSealed Property

    private bool m_isSealed = false;

    public bool IsSealed
    {
      get
      {
        return m_isSealed;
      }
    }

    internal void Seal()
    {
      m_isSealed = true;
    }

    #endregion

  }
}
