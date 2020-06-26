/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal sealed class GeneralUtilities : DependencyObject
  {
    private GeneralUtilities() { }

    #region StubValue attached property

    internal static readonly DependencyProperty StubValueProperty = DependencyProperty.RegisterAttached(
      "StubValue",
      typeof( object ),
      typeof( GeneralUtilities ),
      new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

    internal static object GetStubValue( DependencyObject obj )
    {
      return ( object )obj.GetValue( GeneralUtilities.StubValueProperty );
    }

    internal static void SetStubValue( DependencyObject obj, object value )
    {
      obj.SetValue( GeneralUtilities.StubValueProperty, value );
    }

    #endregion StubValue attached property

    public static object GetPathValue( object sourceObject, string path )
    {
      var targetObj = new GeneralUtilities();
      BindingOperations.SetBinding( targetObj, GeneralUtilities.StubValueProperty, new Binding( path ) { Source = sourceObject } );
      object value = GeneralUtilities.GetStubValue( targetObj );
      BindingOperations.ClearBinding( targetObj, GeneralUtilities.StubValueProperty );
      return value;
    }

    public static object GetBindingValue( object sourceObject, Binding binding )
    {
      Binding bindingClone = new Binding()
      {
        BindsDirectlyToSource = binding.BindsDirectlyToSource,
        Converter = binding.Converter,
        ConverterCulture = binding.ConverterCulture,
        ConverterParameter = binding.ConverterParameter,
        FallbackValue = binding.FallbackValue,
        Mode = BindingMode.OneTime, 
        Path = binding.Path,
        StringFormat = binding.StringFormat,
        TargetNullValue = binding.TargetNullValue,
        XPath = binding.XPath
      };

      bindingClone.Source = sourceObject;

      var targetObj = new GeneralUtilities();
      BindingOperations.SetBinding( targetObj, GeneralUtilities.StubValueProperty, bindingClone );
      object value = GeneralUtilities.GetStubValue( targetObj );
      BindingOperations.ClearBinding( targetObj, GeneralUtilities.StubValueProperty );
      return value;
    }

    internal static bool CanConvertValue( object value, object targetType )
    {
      return ( ( value != null )
              && ( !object.Equals( value.GetType(), targetType ) )
              && ( !object.Equals( targetType, typeof( object ) ) ) );
    }
  }
}
