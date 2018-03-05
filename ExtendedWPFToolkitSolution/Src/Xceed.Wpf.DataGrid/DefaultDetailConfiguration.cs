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
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid
{
  public class DefaultDetailConfiguration : DependencyObject
  {
    static DefaultDetailConfiguration()
    {
      HeadersProperty = HeadersPropertyKey.DependencyProperty;
      FootersProperty = FootersPropertyKey.DependencyProperty;
    }

    public DefaultDetailConfiguration()
    {
      this.SetHeaders( new ObservableCollection<DataTemplate>() );
      this.SetFooters( new ObservableCollection<DataTemplate>() );
    }

    #region AllowDetailToggle Property

    public static readonly DependencyProperty AllowDetailToggleProperty = DataGridControl.AllowDetailToggleProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public bool AllowDetailToggle
    {
      get
      {
        return ( bool )this.GetValue( DefaultDetailConfiguration.AllowDetailToggleProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.AllowDetailToggleProperty, value );
      }
    }

    #endregion AllowDetailToggle Property

    #region DefaultGroupConfiguration Property

    public static readonly DependencyProperty DefaultGroupConfigurationProperty = DataGridControl.DefaultGroupConfigurationProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public GroupConfiguration DefaultGroupConfiguration
    {
      get
      {
        return ( GroupConfiguration )this.GetValue( DefaultDetailConfiguration.DefaultGroupConfigurationProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.DefaultGroupConfigurationProperty, value );
      }
    }

    #endregion DefaultGroupConfiguration Property

    #region GroupConfigurationSelector Property

    public static readonly DependencyProperty GroupConfigurationSelectorProperty = DataGridControl.GroupConfigurationSelectorProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public GroupConfigurationSelector GroupConfigurationSelector
    {
      get
      {
        return ( GroupConfigurationSelector )this.GetValue( DefaultDetailConfiguration.GroupConfigurationSelectorProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.GroupConfigurationSelectorProperty, value );
      }
    }

    #endregion GroupConfigurationSelector Property

    #region ItemContainerStyle Property

    public static readonly DependencyProperty ItemContainerStyleProperty = DataGridControl.ItemContainerStyleProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public Style ItemContainerStyle
    {
      get
      {
        return ( Style )this.GetValue( DefaultDetailConfiguration.ItemContainerStyleProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.ItemContainerStyleProperty, value );
      }
    }

    #endregion ItemContainerStyle Property

    #region ItemContainerStyleSelector Property

    public static readonly DependencyProperty ItemContainerStyleSelectorProperty = DataGridControl.ItemContainerStyleSelectorProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public StyleSelector ItemContainerStyleSelector
    {
      get
      {
        return ( StyleSelector )this.GetValue( DefaultDetailConfiguration.ItemContainerStyleSelectorProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.ItemContainerStyleSelectorProperty, value );
      }
    }

    #endregion ItemContainerStyleSelector Property

    #region DetailIndicatorStyle Property

    public static readonly DependencyProperty DetailIndicatorStyleProperty = DetailConfiguration.DetailIndicatorStyleProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public Style DetailIndicatorStyle
    {
      get
      {
        return ( Style )this.GetValue( DefaultDetailConfiguration.DetailIndicatorStyleProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.DetailIndicatorStyleProperty, value );
      }
    }

    #endregion DetailIndicatorStyle Property

    #region Headers Property

    private static readonly DependencyPropertyKey HeadersPropertyKey =
        DependencyProperty.RegisterReadOnly( "Headers", typeof( ObservableCollection<DataTemplate> ), typeof( DefaultDetailConfiguration ), new FrameworkPropertyMetadata( null ) );

    public static readonly DependencyProperty HeadersProperty;

    public ObservableCollection<DataTemplate> Headers
    {
      get
      {
        return ( ObservableCollection<DataTemplate> )this.GetValue( DefaultDetailConfiguration.HeadersProperty );
      }
    }

    private void SetHeaders( ObservableCollection<DataTemplate> headers )
    {
      this.SetValue( DefaultDetailConfiguration.HeadersPropertyKey, headers );
    }

    #endregion Headers Property

    #region Footers Property

    private static readonly DependencyPropertyKey FootersPropertyKey =
        DependencyProperty.RegisterReadOnly( "Footers", typeof( ObservableCollection<DataTemplate> ), typeof( DefaultDetailConfiguration ), new FrameworkPropertyMetadata( null ) );

    public static readonly DependencyProperty FootersProperty;

    public ObservableCollection<DataTemplate> Footers
    {
      get
      {
        return ( ObservableCollection<DataTemplate> )this.GetValue( DefaultDetailConfiguration.FootersProperty );
      }
    }

    private void SetFooters( ObservableCollection<DataTemplate> footers )
    {
      this.SetValue( DefaultDetailConfiguration.FootersPropertyKey, footers );
    }

    #endregion Footers Property

    #region MaxSortLevels Property

    public static readonly DependencyProperty MaxSortLevelsProperty = DataGridControl.MaxSortLevelsProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public int MaxSortLevels
    {
      get
      {
        return ( int )this.GetValue( DefaultDetailConfiguration.MaxSortLevelsProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.MaxSortLevelsProperty, value );
      }
    }

    #endregion MaxSortLevels Property

    #region MaxGroupLevels Property

    public static readonly DependencyProperty MaxGroupLevelsProperty = DataGridControl.MaxGroupLevelsProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public int MaxGroupLevels
    {
      get
      {
        return ( int )this.GetValue( DefaultDetailConfiguration.MaxGroupLevelsProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.MaxGroupLevelsProperty, value );
      }
    }

    #endregion MaxGroupLevels  Property

    #region UseDefaultHeadersFooters Property

    public static readonly DependencyProperty UseDefaultHeadersFootersProperty = DetailConfiguration.UseDefaultHeadersFootersProperty.AddOwner( typeof( DefaultDetailConfiguration ) );

    public bool UseDefaultHeadersFooters
    {
      get
      {
        return ( bool )this.GetValue( DefaultDetailConfiguration.UseDefaultHeadersFootersProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.UseDefaultHeadersFootersProperty, value );
      }
    }

    #endregion UseDefaultHeadersFooters Property

    #region IsDeleteCommandEnabled Property

    public static readonly DependencyProperty IsDeleteCommandEnabledProperty = DetailConfiguration.IsDeleteCommandEnabledProperty.AddOwner(
      typeof( DefaultDetailConfiguration ),
      new FrameworkPropertyMetadata( false, new PropertyChangedCallback( DefaultDetailConfiguration.OnIsDeleteCommandEnabledChanged ) ) );

    public bool IsDeleteCommandEnabled
    {
      get
      {
        return ( bool )this.GetValue( DefaultDetailConfiguration.IsDeleteCommandEnabledProperty );
      }
      set
      {
        this.SetValue( DefaultDetailConfiguration.IsDeleteCommandEnabledProperty, value );
      }
    }

    private static void OnIsDeleteCommandEnabledChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
    {
      CommandManager.InvalidateRequerySuggested();
    }

    #endregion IsDeleteCommandEnabled Property

    internal void AddDefaultHeadersFooters()
    {
      if( m_defaultHeadersFootersAdded )
        return;

      m_defaultHeadersFootersAdded = true;
    }

    private bool m_defaultHeadersFootersAdded = false;
  }
}
