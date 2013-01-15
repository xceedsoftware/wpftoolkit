/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

[assembly: System.Reflection.AssemblyVersion( _XceedVersionInfo.Version )]

internal static class _XceedVersionInfo
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
  public const string BaseVersion = "1.9";
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
  public const string Version = BaseVersion + ".0.0";
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
  public const string PublicKeyToken = "ba83ff368b7563c6";

  public const string FrameworkVersion = "4.0.0.0";

  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
  public const string DesignFullName =
    "Xceed.Wpf.DataGrid,Version=" +
    Version +
    ",Culture=neutral,PublicKeyToken=" + PublicKeyToken;

  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
  public const string CurrentAssemblyPackUri =
    "pack://application:,,,/Xceed.Wpf.DataGrid,Version=" + Version;


}
