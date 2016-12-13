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
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;

namespace Xceed.Wpf.Toolkit.Core
{
  public class VersionResourceDictionary : ResourceDictionary, ISupportInitialize
  {
    private int _initializingCount;
    private string _assemblyName;
    private string _sourcePath;


    public VersionResourceDictionary() { }

    public VersionResourceDictionary(string assemblyName, string sourcePath)
    {
      ( ( ISupportInitialize )this ).BeginInit();
      this.AssemblyName = assemblyName;
      this.SourcePath = sourcePath;
      ( ( ISupportInitialize )this ).EndInit();
    }

    public string AssemblyName
    {
      get { return _assemblyName; }
      set 
      {
        this.EnsureInitialization();
        _assemblyName = value; 
      }
    }

    public string SourcePath
    {
      get { return _sourcePath; }
      set 
      {
        this.EnsureInitialization();
        _sourcePath = value; 
      }
    }

    private void EnsureInitialization()
    {
      if( _initializingCount <= 0 )
        throw new InvalidOperationException( "VersionResourceDictionary properties can only be set while initializing." );
    }

    void ISupportInitialize.BeginInit()
    {
      base.BeginInit();
      _initializingCount++;
    }

    void ISupportInitialize.EndInit()
    {
      _initializingCount--;
      Debug.Assert( _initializingCount >= 0 );

      if( _initializingCount <= 0 )
      {
        if( this.Source != null )
          throw new InvalidOperationException( "Source property cannot be initialized on the VersionResourceDictionary" );

        if( string.IsNullOrEmpty( this.AssemblyName ) || string.IsNullOrEmpty( this.SourcePath ) )
          throw new InvalidOperationException( "AssemblyName and SourcePath must be set during initialization" );

        //Using an absolute path is necessary in VS2015 for themes different than Windows 8.
        string uriStr = string.Format( @"pack://application:,,,/{0};v{1};component/{2}", this.AssemblyName, _XceedVersionInfo.Version, this.SourcePath );
        this.Source = new Uri( uriStr, UriKind.Absolute );
      }

      base.EndInit();
    }


    private enum InitState
    {
      NotInitialized,
      Initializing,
      Initialized
    };
  }
}
