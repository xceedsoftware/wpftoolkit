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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xceed.Wpf.DataGrid.Markup;

namespace Xceed.Wpf.DataGrid.Views
{
  public abstract class ViewBase : FrameworkContentElement
  {
    #region Static Members

    internal static object GetDefaultStyleKey( Type viewType, Type elementType )
    {
      return new ThemeKey( viewType, null, elementType );
    }

    private static object GetDefaultStyleKey( Type viewType, Theme theme, Type elementType )
    {
      if( theme == null )
        return ViewBase.GetDefaultStyleKey( viewType, elementType );

      return theme.GetDefaultStyleKey( viewType, elementType );
    }

    #endregion Static Members

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
      var handler = this.ThemeChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
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

    #endregion SortOrderGlyph Property

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
      new FrameworkPropertyMetadata( true ) );

    internal bool PreserveContainerSize
    {
      get
      {
        return ( bool )this.GetValue( ViewBase.PreserveContainerSizeProperty );
      }
      set
      {
        this.SetValue( ViewBase.PreserveContainerSizeProperty, value );
      }
    }

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

    internal IEnumerable<ViewPropertyStruct> GetViewProperties()
    {
      //This will ensure that the GetViewPropertiesCore will be called only once by instance of view (optimization to avoir re-doing it all the time ).
      if( m_viewProperties == null )
      {
        m_viewProperties = this.GetViewPropertiesCore( ViewPropertyMode.ViewOnly ).ToArray();
      }

      return m_viewProperties;
    }

    internal IEnumerable<ViewPropertyStruct> GetSharedProperties()
    {
      //This will ensure that the GetViewPropertiesCore will be called only once by instance of view (optimization to avoir re-doing it all the time ).
      if( m_sharedProperties == null )
      {
        m_sharedProperties = this.GetViewPropertiesCore( ViewPropertyMode.Routed ).ToArray();
      }

      return m_sharedProperties;
    }

    internal IEnumerable<ViewPropertyStruct> GetSharedNoFallbackProperties()
    {
      //This will ensure that the GetViewPropertiesCore will be called only once by instance of view (optimization to avoir re-doing it all the time ).
      if( m_sharedNoFallbackProperties == null )
      {
        m_sharedNoFallbackProperties = this.GetViewPropertiesCore( ViewPropertyMode.RoutedNoFallback ).ToArray();
      }

      return m_sharedNoFallbackProperties;
    }

    #endregion

    #region Private Methods

    private static ViewPropertyAttribute GetViewPropertyAttribute( MemberInfo memberInfo )
    {
      if( memberInfo == null )
        return null;

      var attributes = memberInfo.GetCustomAttributes( typeof( ViewPropertyAttribute ), true );
      if( ( attributes == null ) || ( attributes.GetLength( 0 ) != 1 ) )
        return null;

      return ( ViewPropertyAttribute )attributes[ 0 ];
    }

    private IEnumerable<ViewPropertyStruct> GetViewPropertiesCore( ViewPropertyMode viewPropertyMode )
    {
      var fields = this.GetType().GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy );
      foreach( FieldInfo field in fields )
      {
        //Filter out any public static field that is not a DependencyProperty ( no examples )
        if( field.FieldType != typeof( DependencyProperty ) )
          continue;

        //Filter out all DependencyProperty that doesn't match the ViewPropertyMode.
        var attribute = ViewBase.GetViewPropertyAttribute( field );
        if( ( attribute == null ) || ( attribute.ViewPropertyMode != viewPropertyMode ) )
          continue;

        var dependencyProperty = ( DependencyProperty )field.GetValue( null ); // parameter is ignored for static fields.
        Debug.Assert( dependencyProperty != null );

        if( dependencyProperty.ReadOnly )
          throw new InvalidOperationException( "An attempt was made to return a read-only property. Dependency properties returned by ViewBase.GetViewPropertiesCore() cannot be read-only." );

        yield return new ViewPropertyStruct( dependencyProperty, attribute.ViewPropertyMode, attribute.FlattenDetailBindingMode );
      }
    }

    #endregion

    #region Private Fields

    private IEnumerable<ViewPropertyStruct> m_viewProperties; // = null
    private IEnumerable<ViewPropertyStruct> m_sharedProperties; // = null
    private IEnumerable<ViewPropertyStruct> m_sharedNoFallbackProperties; // = null
    private bool m_defaultHeadersFootersAdded; // = false 

    #endregion

    #region ViewPropertyStruct Nested Type

    internal struct ViewPropertyStruct
    {
      internal ViewPropertyStruct( DependencyProperty dependencyProperty )
      {
        if( dependencyProperty == null )
          throw new ArgumentNullException( "dependencyProperty" );

        this.DependencyProperty = dependencyProperty;
        this.ViewPropertyMode = ViewPropertyMode.None;
        this.FlattenDetailBindingMode = FlattenDetailBindingMode.Default;
      }

      internal ViewPropertyStruct(
        DependencyProperty dependencyProperty,
        ViewPropertyMode viewPropertyMode,
        FlattenDetailBindingMode flattenDetailBindingMode )
      {
        if( dependencyProperty == null )
          throw new ArgumentNullException( "dependencyProperty" );

        this.DependencyProperty = dependencyProperty;
        this.ViewPropertyMode = viewPropertyMode;
        this.FlattenDetailBindingMode = flattenDetailBindingMode;
      }

      internal readonly DependencyProperty DependencyProperty;
      internal readonly ViewPropertyMode ViewPropertyMode;
      internal readonly FlattenDetailBindingMode FlattenDetailBindingMode;

      public override bool Equals( object obj )
      {
        if( !( obj is ViewPropertyStruct ) )
          return false;

        return ( this.DependencyProperty == ( ( ViewPropertyStruct )obj ).DependencyProperty );
      }

      public override int GetHashCode()
      {
        return this.DependencyProperty.GetHashCode();
      }
    }

    #endregion
  }
}
