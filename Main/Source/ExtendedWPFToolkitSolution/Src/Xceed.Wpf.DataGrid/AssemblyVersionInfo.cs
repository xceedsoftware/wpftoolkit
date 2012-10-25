/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

[assembly: System.Reflection.AssemblyVersion( _XceedVersionInfo.Version )]

internal static class _XceedVersionInfo
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
  public const string BaseVersion = "1.8";
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
