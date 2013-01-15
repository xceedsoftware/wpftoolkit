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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.DataGrid.Markup;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Views
{
  public abstract class ViewBase : FrameworkContentElement
  {
    #region Static Members

    internal static object GetDefaultStyleKey( Type viewType, Type elementType )
    {
      return ViewBase.GetDefaultStyleKey( viewType, null, elementType );
    }

    private static object GetDefaultStyleKey( Type viewType, Theme theme, Type elementType )
    {
      Type themeType = ( theme == null ) ? null : theme.GetType();

      return new ThemeKey( viewType, themeType, elementType );
    }

    #endregion Static Members

    #region Constructors

    static ViewBase()
    {
      ViewBase.FixedHeadersProperty = ViewBase.FixedHeadersPropertyKey.DependencyProperty;
      ViewBase.HeadersProperty = ViewBase.HeadersPropertyKey.DependencyProperty;
      ViewBase.FootersProperty = ViewBase.FootersPropertyKey.DependencyProperty;
      ViewBase.FixedFootersProperty = ViewBase.FixedFootersPropertyKey.DependencyProperty;
    }

    protected ViewBase()
    {
      this.SetValue( ViewBase.HeadersPropertyKey, new ObservableCollection<DataTemplate>() );
      this.SetValue( ViewBase.FootersPropertyKey, new ObservableCollection<DataTemplate>() );
      this.SetValue( ViewBase.FixedHeadersPropertyKey, new ObservableCollection<DataTemplate>() );
      this.SetValue( ViewBase.FixedFootersPropertyKey, new ObservableCollection<DataTemplate>() );

      object newDefaultStyleKey = this.GetDefaultStyleKey( null );
      if( !object.Equals( newDefaultStyleKey, this.DefaultStyleKey ) )
      {
        this.DefaultStyleKey = newDefaultStyleKey;
      }
    }

    #endregion Constructors

    #region Theme Property

    public static readonly DependencyProperty ThemeProperty =
      DependencyProperty.Register(
      "Theme",
      typeof( Theme ),
      typeof( ViewBase ),
      new PropertyMetadata( null,
        new PropertyChangedCallback( ViewBase.ThemeChangedCallback ),
        new CoerceValueCallback( ViewBase.ThemeCoerceValueCallback ) ) );

    public Theme Theme
    {
      get
      {
        // We don't use the GetStyledValue because this property will never support styling.
        return ( Theme )this.GetValue( ViewBase.ThemeProperty );
      }
      set
      {
        this.SetValue( ViewBase.ThemeProperty, value );
      }
    }

    internal event DependencyPropertyChangedEventHandler ThemeChanged;

    private void OnThemeChanged( DependencyPropertyChangedEventArgs e )
    {
      if( this.ThemeChanged != null )
        this.ThemeChanged( this, e );
    }

    private static void ThemeChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ViewBase view = ( ViewBase )sender;

      object newDefaultStyleKey = view.GetDefaultStyleKey( null );

      if( !object.Equals( newDefaultStyleKey, view.DefaultStyleKey ) )
      {
        view.ClearValue( FrameworkContentElement.DefaultStyleKeyProperty );

        if( !object.Equals( newDefaultStyleKey, view.DefaultStyleKey ) )
        {
          view.DefaultStyleKey = newDefaultStyleKey;
        }
      }

      view.OnThemeChanged( e );
    }

    private static object ThemeCoerceValueCallback( DependencyObject sender, object newValue )
    {
      Theme newTheme = ( Theme )newValue;

      if( newTheme != null )
      {
        if( !newTheme.IsViewSupported( sender.GetType() ) )
          throw new ArgumentException( "This view is not supported by the specified theme (" + sender.GetType().Name + ")." );
      }

      return newValue;
    }

    #endregion Theme Property

    #region Headers Property

    private static readonly DependencyPropertyKey HeadersPropertyKey =
        DependencyProperty.RegisterReadOnly( "Headers", typeof( ObservableCollection<DataTemplate> ), typeof( ViewBase ), new UIPropertyMetadata( null ) );

    public static readonly DependencyProperty HeadersProperty;

    public ObservableCollection<DataTemplate> Headers
    {
      get
      {
        return ( ObservableCollection<DataTemplate> )this.GetValue( ViewBase.HeadersProperty );
      }
    }

    #endregion Headers Property

    #region Footers Property

    private static readonly DependencyPropertyKey FootersPropertyKey =
        DependencyProperty.RegisterReadOnly( "Footers", typeof( ObservableCollection<DataTemplate> ), typeof( ViewBase ), new UIPropertyMetadata( null ) );

    public static readonly DependencyProperty FootersProperty;

    public ObservableCollection<DataTemplate> Footers
    {
      get
      {
        return ( ObservableCollection<DataTemplate> )this.GetValue( ViewBase.FootersProperty );
      }
    }

    #endregion Footers Property

    #region FixedHeaders Property

    private static readonly DependencyPropertyKey FixedHeadersPropertyKey =
        DependencyProperty.RegisterReadOnly( "FixedHeaders", typeof( ObservableCollection<DataTemplate> ), typeof( ViewBase ), new UIPropertyMetadata( null ) );

    public static readonly DependencyProperty FixedHeadersProperty;

    public ObservableCollection<DataTemplate> FixedHeaders
    {
      get
      {
        return ( ObservableCollection<DataTemplate> )this.GetValue( ViewBase.FixedHeadersProperty );
      }
    }

    #endregion FixedHeaders Property

    #region FixedFooters Property

    private static readonly DependencyPropertyKey FixedFootersPropertyKey =
        DependencyProperty.RegisterReadOnly( "FixedFooters", typeof( ObservableCollection<DataTemplate> ), typeof( ViewBase ), new UIPropertyMetadata( null ) );

    public static readonly DependencyProperty FixedFootersProperty;

    public ObservableCollection<DataTemplate> FixedFooters
    {
      get
      {
        return ( ObservableCollection<DataTemplate> )this.GetValue( ViewBase.FixedFootersProperty );
      }
    }

    #endregion FixedFooters Property

    #region GroupLevelConfigurations Read-Only Property

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration property is obsolete and has been replaced by the DataGridControl.GroupConfigurationSelector and DefaultGroupConfiguration properties. ", true )]
    public static readonly DependencyProperty GroupLevelConfigurationsProperty;

    [Browsable( false )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obsolete( "The GroupLevelConfiguration property is obsolete and has been replaced by the DataGridControl.GroupConfigurationSelector and DefaultGroupConfiguration properties. ", true )]
    public GroupLevelConfigurationCollection GroupLevelConfigurations
    {
      get
      {
        return null;
      }
    }

    #endregion GroupLevelConfigurations Read-Only Property

    #region AscendingSortGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty AscendingSortGlyphProperty =
      DependencyProperty.Register( "AscendingSortGlyph",
      typeof( DataTemplate ),
      typeof( ViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate AscendingSortGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( ViewBase.AscendingSortGlyphProperty );
      }
      set
      {
        this.SetValue( ViewBase.AscendingSortGlyphProperty, value );
      }
    }

    #endregion AscendingSortGlyph Property

    #region DescendingSortGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty DescendingSortGlyphProperty =
      DependencyProperty.Register( "DescendingSortGlyph",
      typeof( DataTemplate ),
      typeof( ViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate DescendingSortGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( ViewBase.DescendingSortGlyphProperty );
      }
      set
      {
        this.SetValue( ViewBase.DescendingSortGlyphProperty, value );
      }
    }

    #endregion DescendingSortGlyph Property

    #region ExpandGroupGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty ExpandGroupGlyphProperty =
      DependencyProperty.Register( "ExpandGroupGlyph",
      typeof( DataTemplate ),
      typeof( ViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate ExpandGroupGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( ViewBase.ExpandGroupGlyphProperty );
      }
      set
      {
        this.SetValue( ViewBase.ExpandGroupGlyphProperty, value );
      }
    }

    #endregion ExpandGroupGlyph Property

    #region CollapseGroupGlyph Property

    [ViewProperty( ViewPropertyMode.ViewOnly )]
    public static readonly DependencyProperty CollapseGroupGlyphProperty =
      DependencyProperty.Register( "CollapseGroupGlyph",
      typeof( DataTemplate ),
      typeof( ViewBase ),
      new FrameworkPropertyMetadata( null ) );

    public DataTemplate CollapseGroupGlyph
    {
      get
      {
        return ( DataTemplate )this.GetValue( ViewBase.CollapseGroupGlyphProperty );
      }
      set
      {
        this.SetValue( ViewBase.CollapseGroupGlyphProperty, value );
      }
    }

    #endregion CollapseGroupGlyph Property

    #region UseDefaultHeadersFooters Property

    // This property cannot be configured differently for details and won't be used with 
    // ViewBinding: don't assign a ViewProperty attribute.
    public static readonly DependencyProperty UseDefaultHeadersFootersProperty =
        DependencyProperty.Register( "UseDefaultHeadersFooters", typeof( bool ), typeof( ViewBase ), new PropertyMetadata( true ) );

    public bool UseDefaultHeadersFooters
    {
      get
      {
        return ( bool )this.GetValue( TableView.UseDefaultHeadersFootersProperty );
      }
      set
      {
        this.SetValue( TableView.UseDefaultHeadersFootersProperty, value );
      }
    }

    #endregion UseDefaultHeadersFooters Property

    #region IsLastItem Attached Property

    public static readonly DependencyProperty IsLastItemProperty = DependencyProperty.RegisterAttached( 
      "IsLastItem", typeof( bool ), typeof( ViewBase ), 
      new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.Inherits ) );

    public static bool GetIsLastItem( DependencyObject obj )
    {
      return ( bool )obj.GetValue( ViewBase.IsLastItemProperty );
    }

    public static void SetIsLastItem( DependencyObject obj, bool value )
    {
      obj.SetValue( ViewBase.IsLastItemProperty, value );
    }

    #endregion IsLastItem Attached Property

    #region PreserveContainerSize Property

    internal static readonly DependencyProperty PreserveContainerSizeProperty = DependencyProperty.Register(
      "PreserveContainerSize",
      typeof( bool ),
      typeof( ViewBase ),
      new FrameworkPropertyMetadata(
        true,
        new PropertyChangedCallback( ViewBase.OnPreserveContainerSizeChanged ) ) );

    internal bool PreserveContainerSize
    {
      get
      {
        return m_preserveContainerSize;
      }
      set
      {
        this.SetValue( ViewBase.PreserveContainerSizeProperty, value );
      }
    }

    private static void OnPreserveContainerSizeChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ViewBase uiViewBase = sender as ViewBase;

      if( uiViewBase == null )
        return;

      uiViewBase.m_preserveContainerSize = ( bool )e.NewValue;
    }

    private bool m_preserveContainerSize = true;

    #endregion

    #region ViewTypeForThemeKey Property

    protected virtual Type ViewTypeForThemeKey
    {
      get
      {
        return this.GetType();
      }
    }

    #endregion

    #region Protected Methods

    protected virtual void AddDefaultHeadersFooters()
    {
    } 

    #endregion

    #region Internal Methods

    internal virtual ColumnVirtualizationManager CreateColumnVirtualizationManager( DataGridContext dataGridContext )
    {
      Debug.Assert( dataGridContext != null );

      return new ColumnVirtualizationManager( dataGridContext );
    }

    internal void InvokeAddDefaultHeadersFooters()
    {
      if( !m_defaultHeadersFootersAdded )
      {
        m_defaultHeadersFootersAdded = true;
        this.AddDefaultHeadersFooters();
      }
    }

    internal object GetDefaultStyleKey( Type elementType )
    {
      return ViewBase.GetDefaultStyleKey( this.ViewTypeForThemeKey, this.Theme, elementType );
    }

    internal IEnumerable<DependencyProperty> GetViewProperties()
    {
      //This will ensure that the GetViewPropertiesCore will be called only once by instance of view (optimization to avoir re-doing it all the time ).
      if( m_viewProperties == null )
      {
        List<DependencyProperty> viewProperties = new List<DependencyProperty>();

        this.GetViewPropertiesCore( viewProperties, ViewPropertyMode.ViewOnly );

        m_viewProperties = viewProperties;
      }

      return m_viewProperties;
    }

    internal IEnumerable<DependencyProperty> GetSharedProperties()
    {
      //This will ensure that the GetViewPropertiesCore will be called only once by instance of view (optimization to avoir re-doing it all the time ).
      if( m_sharedProperties == null )
      {
        List<DependencyProperty> sharedProperties = new List<DependencyProperty>();

        this.GetViewPropertiesCore( sharedProperties, ViewPropertyMode.Routed );

        m_sharedProperties = sharedProperties;
      }

      return m_sharedProperties;
    }

    internal IEnumerable<DependencyProperty> GetSharedNoFallbackProperties()
    {
      //This will ensure that the GetViewPropertiesCore will be called only once by instance of view (optimization to avoir re-doing it all the time ).
      if( m_sharedNoFallbackProperties == null )
      {
        List<DependencyProperty> sharedNoFallbackProperties = new List<DependencyProperty>();

        this.GetViewPropertiesCore( sharedNoFallbackProperties, ViewPropertyMode.RoutedNoFallback );

        m_sharedNoFallbackProperties = sharedNoFallbackProperties;
      }

      return m_sharedNoFallbackProperties;
    } 

    #endregion

    #region Private Methods

    private void GetViewPropertiesCore( List<DependencyProperty> viewProperties, ViewPropertyMode viewPropertyMode )
    {
      FieldInfo[] fields = this.GetType().GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy );

      foreach( FieldInfo field in fields )
      {
        //filter out any public static field that is not a DependencyProperty ( no examples )
        if( field.FieldType == typeof( DependencyProperty ) )
        {
          object[] viewPropertyAttributes = field.GetCustomAttributes( typeof( ViewPropertyAttribute ), true );

          //If there is no attribute set on the DP field.
          if( viewPropertyAttributes.GetLength( 0 ) == 0 )
          {
            //Do nothing, the DP shall not be used as a ViewProperty
          }
          else if( viewPropertyAttributes.GetLength( 0 ) == 1 )
          {
            //The attribute has been set on the DP field.
            ViewPropertyAttribute attrib = ( ViewPropertyAttribute )viewPropertyAttributes[ 0 ];

            if( viewPropertyMode == attrib.ViewPropertyMode )
            {
              viewProperties.Add( ( DependencyProperty )field.GetValue( null ) ); // parameter is ignored for static fields.
            }
          }
          else
          {
            //More than one ViewProperty attribute has been set on the same field.
            throw new InvalidOperationException( "An attempt was made to the ViewProperty Attribute more than once on a single DependencyProperty field." );
          }
        } // end if: FieldType is DependencyProperty
      } //end foreach: fields
    } 

    #endregion

    #region Private Fields

    private IEnumerable<DependencyProperty> m_viewProperties; // = null
    private IEnumerable<DependencyProperty> m_sharedProperties; // = null
    private IEnumerable<DependencyProperty> m_sharedNoFallbackProperties; // = null
    private bool m_defaultHeadersFootersAdded; // = false 

    #endregion
  }
}
