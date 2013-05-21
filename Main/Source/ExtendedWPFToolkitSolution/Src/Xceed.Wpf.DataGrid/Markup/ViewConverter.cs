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
using System.ComponentModel;
using System.Globalization;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid.Markup
{
  public class ViewConverter : TypeConverter
  {
    public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
    {
      if( sourceType == typeof( string ) )
        return true;

      return base.CanConvertFrom( context, sourceType );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1807:AvoidUnnecessaryStringCreation", MessageId = "viewName" )]
    public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
    {
      string stringValue = value as string;

      if( stringValue != null )
      {
        ViewBase toReturn = null;

        string completeName = stringValue;
        int pointIndex = completeName.IndexOf( "." );
        string viewName = string.Empty;
        string themeName = string.Empty;

        if(pointIndex != -1)
        {
          viewName = completeName.Substring( 0, pointIndex );
          themeName = completeName.Substring( pointIndex + 1 );
        }
        else
        {
          viewName = completeName;
        }

        viewName = viewName.ToLowerInvariant();

        switch( viewName )
        {
          case "tableview":
            toReturn = new TableView();
            break;
          case "tableflowview":
            toReturn = new TableflowView();
            break;
        }

        if( toReturn == null )
        {
          throw new ArgumentException( "The specified view is invalid.", "value" );
        }
        else
        {
          if( !string.IsNullOrEmpty( themeName ) )
          {
            ThemeConverter themeConverter = new ThemeConverter();
            toReturn.Theme = (Theme)themeConverter.ConvertFromString(themeName);
          }
        }

        return toReturn;
      }

      return base.ConvertFrom( context, culture, value );
    }

    public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
    {
      if( ( destinationType != null ) && ( value is ViewBase ) )
      {
        if( destinationType == typeof( string ) )
        {
          return value.GetType().Name;
        }
      }

      return base.ConvertTo( context, culture, value, destinationType );
    }

  }
}
