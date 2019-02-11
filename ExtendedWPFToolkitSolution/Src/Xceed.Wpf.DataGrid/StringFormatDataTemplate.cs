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
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class StringFormatDataTemplate : DataTemplate
  {
    #region Static Fields

    private static readonly List<Entry> s_templates = new List<Entry>( 0 );
    private static readonly StringFormatConverter s_converter = new StringFormatConverter();
    private static readonly StringFormatComparer s_comparer = new StringFormatComparer();

    #endregion

    #region Constructor

    private StringFormatDataTemplate( DataTemplate contentTemplate )
    {
      m_contentTemplate = contentTemplate;
    }

    #endregion

    internal static StringFormatDataTemplate Get( DataTemplate contentTemplate, string format, CultureInfo culture )
    {
      if( string.IsNullOrEmpty( format ) )
        throw new ArgumentException( "The format must be non empty.", "format" );

      lock( ( ( ICollection )s_templates ).SyncRoot )
      {
        int insertionIndex = 0;
        bool cleanUp = false;
        StringFormatDataTemplate template = null;

        if( s_templates.Count > 0 )
        {
          var lookup = new Entry( format, culture );
          insertionIndex = s_templates.BinarySearch( lookup, s_comparer );

          if( insertionIndex >= 0 )
          {
            for( int i = insertionIndex; i < s_templates.Count; i++ )
            {
              var item = s_templates[ i ];
              if( s_comparer.Compare( lookup, item ) != 0 )
                break;

              var target = item.Template;
              if( target == null )
              {
                cleanUp = true;
              }
              else
              {
                Debug.Assert( object.Equals( item.Format, format ) && object.Equals( item.Culture, culture ) );

                if( target.m_contentTemplate == contentTemplate )
                {
                  template = target;
                  break;
                }
              }
            }
          }
          else
          {
            insertionIndex = ~insertionIndex;
            cleanUp = ( insertionIndex < s_templates.Count ) && ( s_templates[ insertionIndex ].Template == null );
          }
        }

        if( template == null )
        {
          template = StringFormatDataTemplate.Create( contentTemplate, format, culture );
          s_templates.Insert( insertionIndex, new Entry( template, format, culture ) );
        }

        if( cleanUp )
        {
          StringFormatDataTemplate.CleanUpCache();
        }

        Debug.Assert( template != null );

        return template;
      }
    }

    private static StringFormatDataTemplate Create( DataTemplate contentTemplate, string format, CultureInfo culture )
    {
      // We are using a converter to format the target value instead of using the
      // ContentPresenter.ContentStringFormat or Binding.StringFormat property because
      // it is not applied if a ContentPresenter.ContentTemplate is given or if the binding is
      // not targeting a string property.
      var template = new StringFormatDataTemplate( contentTemplate );
      var control = new FrameworkElementFactory( typeof( ContentPresenter ) );

      var binding = new Binding();
      binding.Path = new PropertyPath( FrameworkElement.DataContextProperty );
      binding.RelativeSource = RelativeSource.TemplatedParent;
      binding.Converter = s_converter;
      binding.ConverterParameter = format;
      binding.ConverterCulture = culture;

      control.SetValue( ContentPresenter.ContentTemplateProperty, contentTemplate );
      control.SetValue( ContentPresenter.ContentProperty, binding );

      template.VisualTree = control;
      template.Seal();

      return template;
    }

    private static void CleanUpCache()
    {
      for( int i = s_templates.Count - 1; i >= 0; i-- )
      {
        if( s_templates[ i ].Template == null )
        {
          s_templates.RemoveAt( i );
        }
      }

      s_templates.TrimExcess();
    }

    #region Private Fields

    private readonly DataTemplate m_contentTemplate;

    #endregion

    #region StringFormatConverter Private Class

    private sealed class StringFormatConverter : IValueConverter
    {
      public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
      {
        var format = parameter as string;
        if( string.IsNullOrEmpty( format ) )
          return value;

        return string.Format( culture, format, value );
      }

      public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
      {
        throw new NotSupportedException();
      }
    }

    #endregion

    #region StringFormatComparer Private Class

    private sealed class StringFormatComparer : IComparer<Entry>
    {
      public int Compare( Entry x, Entry y )
      {
        if( object.ReferenceEquals( x, y ) )
          return 0;

        if( object.ReferenceEquals( x, null ) )
          return -1;

        if( object.ReferenceEquals( y, null ) )
          return 1;

        var xf = x.Format;
        var yf = y.Format;

        if( !string.Equals( xf, yf, StringComparison.InvariantCulture ) )
        {
          if( xf == null )
            return -1;

          if( yf == null )
            return 1;

          return string.Compare( xf, yf, StringComparison.InvariantCulture );
        }

        var xc = x.Culture;
        var yc = y.Culture;

        if( !object.Equals( xc, yc ) )
        {
          if( xc == null )
            return -1;

          if( yc == null )
            return 1;

          return xc.LCID.CompareTo( yc.LCID );
        }

        return 0;
      }
    }

    #endregion

    #region Entry Private Class

    private class Entry
    {
      internal Entry( string format, CultureInfo culture )
        : this( null, format, culture )
      {
      }

      internal Entry( StringFormatDataTemplate template, string format, CultureInfo culture )
      {
        Debug.Assert( format != null );

        if( template != null )
        {
          m_template = new WeakReference( template );
        }

        m_format = format;
        m_culture = culture;
      }

      internal StringFormatDataTemplate Template
      {
        get
        {
          if( m_template == null )
            return null;

          return ( StringFormatDataTemplate )m_template.Target;
        }
      }

      internal string Format
      {
        get
        {
          return m_format;
        }
      }

      internal CultureInfo Culture
      {
        get
        {
          return m_culture;
        }
      }

      private readonly WeakReference m_template;
      private readonly string m_format;
      private readonly CultureInfo m_culture;
    }

    #endregion
  }
}
