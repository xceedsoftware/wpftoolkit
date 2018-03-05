/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

#region Using directives

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Markup;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Xceed Toolkit for WPF - DataGrid" )]
[assembly: AssemblyDescription( "This assembly implements the Xceed.Wpf.DataGrid namespace, a data grid control for the Windows Presentation Framework." )]

[assembly: AssemblyCompany( "Xceed Software Inc." )]
[assembly: AssemblyProduct( "Xceed Toolkit for WPF - DataGrid" )]
[assembly: AssemblyCopyright( "Copyright (C) Xceed Software Inc. 2007-2017" )]


[assembly: CLSCompliant( true )]
[assembly: ComVisible( false )]

[assembly: XmlnsPrefix( "http://schemas.xceed.com/wpf/xaml/datagrid", "xcdg" )]
[assembly: XmlnsDefinition( "http://schemas.xceed.com/wpf/xaml/datagrid", "Xceed.Wpf.DataGrid")]
[assembly: XmlnsDefinition( "http://schemas.xceed.com/wpf/xaml/datagrid", "Xceed.Wpf.DataGrid.Converters")]
[assembly: XmlnsDefinition( "http://schemas.xceed.com/wpf/xaml/datagrid", "Xceed.Wpf.DataGrid.Markup")]
[assembly: XmlnsDefinition( "http://schemas.xceed.com/wpf/xaml/datagrid", "Xceed.Wpf.DataGrid.Views")]
[assembly: XmlnsDefinition( "http://schemas.xceed.com/wpf/xaml/datagrid", "Xceed.Wpf.DataGrid.ValidationRules")]


[assembly: SecurityRules( SecurityRuleSet.Level1 )]
[assembly: AllowPartiallyTrustedCallers]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.SourceAssembly, //where theme specific resource dictionaries are located
  //(used if a resource is not found in the page, 
  // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
  //(used if a resource is not found in the page, 
  // app, or any theme specific resource dictionaries)
)]

#pragma warning disable 1699
[assembly: AssemblyDelaySign( false )]
[assembly: AssemblyKeyFile( @"..\..\sn.snk" )]
[assembly: AssemblyKeyName( "" )]
#pragma warning restore 1699
