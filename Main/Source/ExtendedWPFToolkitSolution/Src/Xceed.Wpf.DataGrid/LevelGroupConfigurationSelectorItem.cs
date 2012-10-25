/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

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
