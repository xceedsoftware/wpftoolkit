/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  public class GroupConfiguration : Freezable
  {
    static GroupConfiguration()
    {
      GroupConfiguration.HeadersProperty = GroupConfiguration.HeadersPropertyKey.DependencyProperty;
      GroupConfiguration.FootersProperty = GroupConfiguration.FootersPropertyKey.DependencyProperty;

      DefaultHeaderTemplate = new GroupHeaderFooterItemTemplate();
      DefaultHeaderTemplate.VisibleWhenCollapsed = true;
      DefaultHeaderTemplate.Template = new DataTemplate();
      DefaultHeaderTemplate.Template.VisualTree = new FrameworkElementFactory( typeof( GroupHeaderControl ) );
      DefaultHeaderTemplate.Template.Seal();
      DefaultHeaderTemplate.Seal();

      DefaultGroupConfiguration = new GroupConfiguration();
      DefaultGroupConfiguration.AddDefaultHeadersFooters();
      DefaultGroupConfiguration.Freeze();
    }

    internal static readonly GroupConfiguration DefaultGroupConfiguration;

    public GroupConfiguration()
    {
      this.SetHeaders( new GroupHeaderFooterCollection() );
      this.SetFooters( new GroupHeaderFooterCollection() );
    }

    #region Headers Read-Only Property

    private static readonly DependencyPropertyKey HeadersPropertyKey =
        DependencyProperty.RegisterReadOnly( "Headers", typeof( ObservableCollection<object> ), typeof( GroupConfiguration ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty HeadersProperty;

    public ObservableCollection<object> Headers
    {
      get
      {
        return ( ObservableCollection<object> )this.GetValue( GroupConfiguration.HeadersProperty );
      }
    }

    private void SetHeaders( ObservableCollection<object> value )
    {
      this.SetValue( GroupConfiguration.HeadersPropertyKey, value );
    }

    #endregion Headers Read-Only Property

    #region Footers Read-Only Property

    private static readonly DependencyPropertyKey FootersPropertyKey =
        DependencyProperty.RegisterReadOnly( "Footers", typeof( ObservableCollection<object> ), typeof( GroupConfiguration ), new PropertyMetadata( null ) );

    public static readonly DependencyProperty FootersProperty;

    public ObservableCollection<object> Footers
    {
      get
      {
        return ( ObservableCollection<object> )this.GetValue( GroupConfiguration.FootersProperty );
      }
    }

    private void SetFooters( ObservableCollection<object> value )
    {
      this.SetValue( GroupConfiguration.FootersPropertyKey, value );
    }

    #endregion Footers Read-Only Property

    #region InitiallyExpanded Property

    public static readonly DependencyProperty InitiallyExpandedProperty =
        DependencyProperty.Register( "InitiallyExpanded", typeof( bool ), typeof( GroupConfiguration ), new UIPropertyMetadata( true ) );

    public bool InitiallyExpanded
    {
      get
      {
        return ( bool )this.GetValue( GroupConfiguration.InitiallyExpandedProperty );
      }
      set
      {
        this.SetValue( GroupConfiguration.InitiallyExpandedProperty, value );
      }
    }

    #endregion InitiallyExpanded Property

    #region ItemContainerStyle Property

    public static readonly DependencyProperty ItemContainerStyleProperty = DataGridControl.ItemContainerStyleProperty.AddOwner( typeof( GroupConfiguration ) );

    public Style ItemContainerStyle
    {
      get
      {
        return ( Style )this.GetValue( GroupConfiguration.ItemContainerStyleProperty );
      }
      set
      {
        this.SetValue( GroupConfiguration.ItemContainerStyleProperty, value );
      }
    }

    #endregion ItemContainerStyle Property

    #region ItemContainerStyleSelector Property

    public static readonly DependencyProperty ItemContainerStyleSelectorProperty = DataGridControl.ItemContainerStyleSelectorProperty.AddOwner( typeof( GroupConfiguration ) );

    public StyleSelector ItemContainerStyleSelector
    {
      get
      {
        return ( StyleSelector )this.GetValue( GroupConfiguration.ItemContainerStyleSelectorProperty );
      }
      set
      {
        this.SetValue( GroupConfiguration.ItemContainerStyleSelectorProperty, value );
      }
    }

    #endregion ItemContainerStyleSelector Property

    #region GroupLevelIndicatorStyle Property

    public static readonly DependencyProperty GroupLevelIndicatorStyleProperty =
        DependencyProperty.Register( "GroupLevelIndicatorStyle", typeof( Style ), typeof( GroupConfiguration ), new UIPropertyMetadata( null ) );

    public Style GroupLevelIndicatorStyle
    {
      get
      {
        return ( Style )this.GetValue( GroupConfiguration.GroupLevelIndicatorStyleProperty );
      }
      set
      {
        this.SetValue( GroupConfiguration.GroupLevelIndicatorStyleProperty, value );
      }
    }

    #endregion GroupLevelIndicatorStyle Property

    #region UseDefaultHeadersFooters Property

    public static readonly DependencyProperty UseDefaultHeadersFootersProperty =
        DependencyProperty.Register( "UseDefaultHeadersFooters", typeof( bool ), typeof( GroupConfiguration ), new PropertyMetadata( true ) );

    public bool UseDefaultHeadersFooters
    {
      get
      {
        return ( bool )this.GetValue( GroupConfiguration.UseDefaultHeadersFootersProperty );
      }
      set
      {
        this.SetValue( GroupConfiguration.UseDefaultHeadersFootersProperty, value );
      }
    }

    #endregion UseDefaultHeadersFooters Property

    internal void AddDefaultHeadersFooters()
    {
      if( m_defaultHeadersFootersAdded )
        return;

      m_defaultHeadersFootersAdded = true;
      this.Headers.Insert( 0, GroupConfiguration.DefaultHeaderTemplate );
    }

    internal static GroupConfiguration GetGroupConfiguration( DataGridContext dataGridContext, ObservableCollection<GroupDescription> groupDescriptions, GroupConfigurationSelector groupConfigSelector, int groupLevel, CollectionViewGroup collectionViewGroup )
    {
      if( dataGridContext == null )
        throw new ArgumentNullException( "dataGridContext" );

      if( groupDescriptions == null )
        throw new ArgumentNullException( "groupDescriptions" );

      if( groupLevel >= groupDescriptions.Count )
        throw new ArgumentException( "The specified group level is greater than the number of GroupDescriptions in the DataGridContext.", "groupLevel" );

      GroupDescription groupDescription = groupDescriptions[ groupLevel ];

      GroupConfiguration retval = null;
      DataGridGroupDescription dataGridGroupDescription = groupDescription as DataGridGroupDescription;

      if( ( dataGridGroupDescription != null ) && ( dataGridGroupDescription.GroupConfiguration != null ) )
      {
        retval = dataGridGroupDescription.GroupConfiguration;
      }
      else if( groupConfigSelector != null )
      {
        retval = groupConfigSelector.SelectGroupConfiguration( groupLevel, collectionViewGroup, groupDescription );
      }

      if( retval == null )
      {
        retval = dataGridContext.DefaultGroupConfiguration;
      }

      if( retval == null )
      {
        retval = GroupConfiguration.DefaultGroupConfiguration;
      }

      return retval;
    }

    protected override Freezable CreateInstanceCore()
    {
      return new GroupConfiguration();
    }


    private bool m_defaultHeadersFootersAdded; // = false
    private static readonly GroupHeaderFooterItemTemplate DefaultHeaderTemplate;
  }
}
