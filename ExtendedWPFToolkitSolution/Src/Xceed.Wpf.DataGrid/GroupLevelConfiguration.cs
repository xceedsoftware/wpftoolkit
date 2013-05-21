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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  [EditorBrowsable( EditorBrowsableState.Never )]
  [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
  public class GroupLevelConfiguration : DependencyObject
  {
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public GroupLevelConfiguration()
    {
    }

    #region Headers Read-Only Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public static readonly DependencyProperty HeadersProperty;

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public ObservableCollection<object> Headers
    {
      get
      {
        return null;
      }
    }

    #endregion Headers Read-Only Property

    #region Footers Read-Only Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public static readonly DependencyProperty FootersProperty;

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public ObservableCollection<object> Footers
    {
      get
      {
        return null;
      }
    }

    #endregion Footers Read-Only Property

    #region InitiallyExpanded Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public static readonly DependencyProperty InitiallyExpandedProperty;

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value" ), System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" ), Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public bool InitiallyExpanded
    {
      get
      {
        return false;
      }
      set
      {
      }
    }

    #endregion InitiallyExpanded Property

    #region GroupLevelIndicatorStyle Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public static readonly DependencyProperty GroupLevelIndicatorStyleProperty;

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value" ), System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" ), Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration class is obsolete and has been replaced by the LevelGroupConfigurationSelector class.", true )]
    public Style GroupLevelIndicatorStyle
    {
      get
      {
        return null;
      }
      set
      {
      }
    }

    #endregion GroupLevelIndicatorStyle Property
  }
}
