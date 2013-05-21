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
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections;

namespace Xceed.Wpf.DataGrid.Converters
{
  internal class DefaultDataConverter : IValueConverter
  {
    public DefaultDataConverter()
    {
    }

    public object Convert( object sourceValue, Type targetType, object parameter, CultureInfo targetCulture )
    {
      Exception exception;
      return DefaultDataConverter.TryConvert( sourceValue, targetType, CultureInfo.InvariantCulture, targetCulture, out exception );
    }

    public object ConvertBack(
      object targetValue,
      Type sourceType,
      object parameter,
      CultureInfo targetCulture )
    {
      Exception exception;
      return DefaultDataConverter.TryConvert( targetValue, sourceType, targetCulture, CultureInfo.InvariantCulture, out exception );
    }

    internal static object TryConvert(
      object sourceValue,
      Type targetType,
      CultureInfo sourceCulture,
      CultureInfo targetCulture,
      out Exception exception )
    {
      if( targetCulture == null )
        targetCulture = CultureInfo.CurrentCulture;

      if( sourceCulture == null )
        sourceCulture = CultureInfo.InvariantCulture;

      exception = null;
      Type valueType = null;

      try
      {
        if( sourceValue != null )
        {
          valueType = sourceValue.GetType();

          if( targetType.IsAssignableFrom( valueType ) )
            return sourceValue;
        }
        else
        {
          if( !targetType.IsValueType )
            return sourceValue;
        }

        if( targetType == typeof( string ) )
          return string.Format( targetCulture, "{0}", sourceValue );

        if( sourceValue != null )
        {
          bool targetTypeIsNullable = false;
          Type targetConversionType = targetType;
          object resultValue;

          if( ( targetType.IsGenericType ) && ( targetType.GetGenericTypeDefinition() == typeof( Nullable<> ) ) )
          {
            targetTypeIsNullable = true;
            targetConversionType = targetType.GetGenericArguments()[ 0 ];
          }

          if( DefaultDataConverter.IsConvertibleToAndFrom( valueType, targetConversionType ) )
          {
            resultValue = System.Convert.ChangeType( sourceValue, targetConversionType, sourceCulture );

            if( !targetTypeIsNullable )
              return resultValue;

            sourceValue = resultValue;
          }
        }

        TypeConverter cachedTypeConverter;

        lock( mg_cachedTypeConverter )
        {
          cachedTypeConverter = mg_cachedTypeConverter[ targetType ] as TypeConverter;

          if( cachedTypeConverter == null )
          {
            cachedTypeConverter = TypeDescriptor.GetConverter( targetType );
            mg_cachedTypeConverter[ targetType ] = cachedTypeConverter;
          }
        }

        return cachedTypeConverter.ConvertFrom( null, sourceCulture, sourceValue );
      }
      catch( Exception localException )
      {
        exception = localException;
        return DependencyProperty.UnsetValue;
      }
    }

    private static bool IsConvertibleToAndFrom( Type sourceType, Type targetType )
    {
      if( sourceType == typeof( DateTime ) )
        return ( targetType == typeof( string ) );

      if( targetType == typeof( DateTime ) )
        return ( sourceType == typeof( string ) );

      if( sourceType == typeof( char ) )
        return DefaultDataConverter.IsConvertibleFromAndToChar( targetType );

      if( targetType == typeof( char ) )
        return DefaultDataConverter.IsConvertibleFromAndToChar( sourceType );

      bool sourceTypeFound = false;
      bool targetTypeFound = false;

      for( int i = 0; i < ConvertibleTypes.Length; i++ )
      {
        if( ( !sourceTypeFound ) && ( sourceType == ConvertibleTypes[ i ] ) )
        {
          if( targetTypeFound )
            return true;

          sourceTypeFound = true;
        }
        else if( ( !targetTypeFound ) && ( targetType == ConvertibleTypes[ i ] ) )
        {
          if( sourceTypeFound )
            return true;

          targetTypeFound = true;
        }
      }

      return false;
    }

    private static bool IsConvertibleFromAndToChar( Type type )
    {
      for( int i = 0; i < CharConvertibleTypes.Length; i++ )
      {
        if( type == CharConvertibleTypes[ i ] )
          return true;
      }

      return false;
    }

    private static readonly Type[] ConvertibleTypes = new Type[] 
      {
        typeof(string), typeof(int), typeof(long),
        typeof(float), typeof(double), typeof(decimal),
        typeof(bool), typeof(byte), typeof(short),
        typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte)
      };

    private static readonly Type[] CharConvertibleTypes = new Type[] 
      { 
        typeof(string), typeof(int), typeof(long), 
        typeof(byte), typeof(short), typeof(uint), 
        typeof(ulong), typeof(ushort), typeof(sbyte) 
      };

    private static Hashtable mg_cachedTypeConverter = new Hashtable();
  }
}
