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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using Xceed.Wpf.DataGrid.Converters;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class GenericContentTemplateSelector : DataTemplateSelector
  {
    #region Static Fields

    internal static readonly DataTemplate ForeignKeyCellContentTemplate;
    internal static readonly DataTemplate ForeignKeyGroupValueTemplate;
    internal static readonly DataTemplate ForeignKeyScrollTipContentTemplate;

    private static readonly DataTemplate BoolTemplate;
    private static readonly DataTemplate CommonTemplate;
    private static readonly DataTemplate ImageTemplate;

    private static readonly List<KeyValuePair<Type, WeakReference>> DefaultTemplates = new List<KeyValuePair<Type, WeakReference>>( 0 );

    private static readonly GenericContentTemplateSelectorResources GenericContentTemplateResources = new GenericContentTemplateSelectorResources();

    private static Func<DependencyObject, object, Type, object> FindTemplateResource; //null

    #endregion

    #region Constructors

    static GenericContentTemplateSelector()
    {
      // We need to initalize the ResourceDictionary before accessing since we access
      // it in a static constructor and will be called before the Layout was performed
      GenericContentTemplateSelector.GenericContentTemplateResources.InitializeComponent();

      GenericContentTemplateSelector.BoolTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "booleanDefaultContentTemplate" ] as DataTemplate;
      Debug.Assert( GenericContentTemplateSelector.BoolTemplate != null );
      GenericContentTemplateSelector.BoolTemplate.Seal();

      GenericContentTemplateSelector.CommonTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "commonDefaultContentTemplate" ] as DataTemplate;
      Debug.Assert( GenericContentTemplateSelector.CommonTemplate != null );
      GenericContentTemplateSelector.CommonTemplate.Seal();

      GenericContentTemplateSelector.ImageTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "imageDefaultContentTemplate" ] as DataTemplate;
      Debug.Assert( GenericContentTemplateSelector.ImageTemplate != null );
      GenericContentTemplateSelector.ImageTemplate.Seal();

      GenericContentTemplateSelector.ForeignKeyCellContentTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "foreignKeyDefaultContentTemplate" ] as DataTemplate;
      Debug.Assert( GenericContentTemplateSelector.ForeignKeyCellContentTemplate != null );
      GenericContentTemplateSelector.ForeignKeyCellContentTemplate.Seal();

      GenericContentTemplateSelector.ForeignKeyGroupValueTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "foreignKeyGroupValueDefaultContentTemplate" ] as DataTemplate;
      Debug.Assert( GenericContentTemplateSelector.ForeignKeyGroupValueTemplate != null );
      GenericContentTemplateSelector.ForeignKeyGroupValueTemplate.Seal();

      GenericContentTemplateSelector.ForeignKeyScrollTipContentTemplate = GenericContentTemplateSelector.GenericContentTemplateResources[ "foreignKeyScrollTipDefaultContentTemplate" ] as DataTemplate;
      Debug.Assert( GenericContentTemplateSelector.ForeignKeyScrollTipContentTemplate != null );
      GenericContentTemplateSelector.ForeignKeyScrollTipContentTemplate.Seal();
    }

    private GenericContentTemplateSelector()
    {
    }

    #endregion

    #region Instance Static Property

    public static GenericContentTemplateSelector Instance
    {
      get
      {
        return m_instance;
      }
    }

    private static GenericContentTemplateSelector m_instance = new GenericContentTemplateSelector();

    #endregion

    public override DataTemplate SelectTemplate( object item, DependencyObject container )
    {
      if( item == null )
        return base.SelectTemplate( item, container );

      DataTemplate template = null;

      if( ( item is byte[] ) || ( item is System.Drawing.Image ) )
      {
        bool useImageTemplate = false;

        try
        {
          var converter = new ImageConverter();
          useImageTemplate = ( converter.Convert( item, typeof( ImageSource ), null, CultureInfo.CurrentCulture ) != null );
        }
        catch( NotSupportedException )
        {
          //suppress the exception, the byte[] is not an image. convertedValue will remain null
        }

        if( useImageTemplate )
        {
          template = GenericContentTemplateSelector.GetImageTemplate( container );
        }
      }
      else if( item is ImageSource )
      {
        template = GenericContentTemplateSelector.GetImageTemplate( container );
      }
      else if( item is bool )
      {
        template = GenericContentTemplateSelector.BoolTemplate;
      }

      if( template == null )
      {
        template = GenericContentTemplateSelector.GetCommonTemplate( item, container );
      }

      if( template != null )
        return template;

      return base.SelectTemplate( item, container );
    }

    private static DataTemplate GetImageTemplate( DependencyObject container )
    {
      return GenericContentTemplateSelector.ImageTemplate;
    }

    private static DataTemplate GetCommonTemplate( object item, DependencyObject container )
    {
      Debug.Assert( item != null );

      var itemType = item.GetType();

      // Do not provide a template for data types that are already optimized by the framework or
      // for data types that have a default template.
      if( GenericContentTemplateSelector.IsTypeOptimized( itemType )
        || GenericContentTemplateSelector.HasImplicitTemplate( item, itemType, container ) )
        return null;

      return GenericContentTemplateSelector.GetDefaultTemplate( itemType );
    }

    private static DataTemplate GetDefaultTemplate( Type type )
    {
      Debug.Assert( type != null );

      var converter = TypeDescriptor.GetConverter( type );
      if( ( converter == null ) || ( !converter.CanConvertTo( typeof( string ) ) ) )
        return GenericContentTemplateSelector.CommonTemplate;

      var templates = GenericContentTemplateSelector.DefaultTemplates;
      lock( ( ( ICollection )templates ).SyncRoot )
      {
        for( int i = templates.Count - 1; i >= 0; i-- )
        {
          var target = templates[ i ];
          var targetTemplate = target.Value.Target as DataTemplate;

          // We have found the desired template.
          if( target.Key == type )
          {
            if( targetTemplate != null )
              return targetTemplate;

            // Unfortunately, the template has been garbage collected.
            templates.RemoveAt( i );
            break;
          }
        }

        templates.TrimExcess();

        var template = GenericContentTemplateSelector.CreateDefaultTemplate( type, converter );

        templates.Add( new KeyValuePair<Type, WeakReference>( type, new WeakReference( template ) ) );

        return template;
      }
    }

    private static DataTemplate CreateDefaultTemplate( Type type, TypeConverter converter )
    {
      Debug.Assert( type != null );
      Debug.Assert( converter != null );

      var template = new DataTemplate();
      var factory = new FrameworkElementFactory( typeof( TextBlock ) );

      var binding = new Binding();
      binding.Mode = BindingMode.OneWay;
      binding.Converter = new DefaultConverter( converter );
      binding.ConverterCulture = CultureInfo.CurrentCulture;

      factory.SetBinding( TextBlock.TextProperty, binding );

      template.VisualTree = factory;
      template.Seal();

      return template;
    }

    private static bool IsTypeOptimized( Type type )
    {
      Debug.Assert( type != null );

      if( typeof( string ).IsAssignableFrom( type )
        || typeof( UIElement ).IsAssignableFrom( type )
        || typeof( XmlNode ).IsAssignableFrom( type ) )
        return true;

      if( !typeof( Inline ).IsAssignableFrom( type ) )
      {
        var converter = TypeDescriptor.GetConverter( type );
        if( ( converter != null ) && ( converter.CanConvertTo( typeof( UIElement ) ) ) )
          return true;
      }

      return false;
    }

    private static bool HasImplicitTemplate( object item, Type type, DependencyObject container )
    {
      Debug.Assert( type != null );

      var finder = GenericContentTemplateSelector.FindTemplateResource;

      if( finder == null )
      {
        finder = GenericContentTemplateSelector.GetFindTemplateResourceInternal();

        if( finder == null )
        {
          finder = GenericContentTemplateSelector.FindTemplateResourceFallback;
        }

        GenericContentTemplateSelector.FindTemplateResource = finder;
      }

      Debug.Assert( finder != null );

      return ( finder.Invoke( container, item, type ) != null );
    }

    private static Func<DependencyObject, object, Type, object> GetFindTemplateResourceInternal()
    {
      try
      {
        var methodInfo = typeof( FrameworkElement ).GetMethod(
                           "FindTemplateResourceInternal",
                           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                           null,
                           CallingConventions.Any,
                           new Type[] { typeof( DependencyObject ), typeof( object ), typeof( Type ) },
                           null );

        if( methodInfo != null )
        {
          var finder = ( Func<DependencyObject, object, Type, object> )Delegate.CreateDelegate( typeof( Func<DependencyObject, object, Type, object> ), methodInfo );

          return ( container, item, type ) => finder.Invoke( container, item, typeof( DataTemplate ) );
        }
      }
      catch( AmbiguousMatchException )
      {
        // We swallow the exception and use a fallback method instead.
      }
      catch( MethodAccessException )
      {
        // We swallow the exception and use a fallback method instead.
      }

      return null;
    }

    private static object FindTemplateResourceFallback( DependencyObject container, object item, Type type )
    {
      var fe = container as FrameworkElement;
      if( fe == null )
        return null;

      return fe.TryFindResource( new DataTemplateKey( type ) );
    }

    #region DefaultConverter Private Class

    private sealed class DefaultConverter : IValueConverter
    {
      internal DefaultConverter( TypeConverter converter )
      {
        if( converter == null )
          throw new ArgumentNullException( "converter" );

        m_converter = converter;
      }

      public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
      {
        object result;

        if( DefaultConverter.TryConvertTo( value, targetType, culture, m_converter, out result ) )
          return result;

        if( value != null )
        {
          var valueType = value.GetType();

          if( DefaultConverter.TryConvertTo( value, targetType, culture, TypeDescriptor.GetConverter( valueType ), out result ) )
            return result;

          if( targetType.IsAssignableFrom( valueType ) )
            return value;
        }
        else if( DefaultConverter.IsNullableType( targetType ) )
        {
          return value;
        }

        throw new ArgumentException( "Cannot convert to type " + targetType.FullName + ".", "value" );
      }

      public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
      {
        throw new NotSupportedException();
      }

      private static bool TryConvertTo( object value, Type targetType, CultureInfo culture, TypeConverter converter, out object result )
      {
        if( converter != null )
        {
          try
          {
            result = converter.ConvertTo( null, culture, value, targetType );
            return true;
          }
          catch
          {
            // We'll try to convert the value another way.
          }

          if( ( value != null ) && ( converter.CanConvertFrom( value.GetType() ) ) )
          {
            try
            {
              var newValue = converter.ConvertFrom( null, culture, value );
              result = converter.ConvertTo( null, culture, newValue, targetType );

              return true;
            }
            catch
            {
            }
          }
        }

        result = null;
        return false;
      }

      private static bool IsNullableType( Type type )
      {
        return ( !type.IsValueType )
            || ( type.IsGenericType && ( type.GetGenericTypeDefinition() == typeof( Nullable<> ) ) );
      }

      private readonly TypeConverter m_converter;
    }

    #endregion
  }
}
