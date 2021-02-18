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
using System.ComponentModel;
using System.Diagnostics;

namespace Xceed.Wpf.Toolkit.Core
{
  public class ResourceDictionary : System.Windows.ResourceDictionary, ISupportInitialize
  {
    private int _initializingCount;
    private string _assemblyName;
    private string _sourcePath;


    public ResourceDictionary() { }

    public ResourceDictionary( string assemblyName, string sourcePath )
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

    protected virtual Uri BuildUri()
    {
      // Build a pack uri relative to the root of the supplied assembly name
      string uriStr = PackUriExtension.BuildRelativePackUriString( this.AssemblyName, this.SourcePath );
      return new Uri( uriStr, UriKind.Relative );
    }

    private void EnsureInitialization()
    {
      if( _initializingCount <= 0 )
        throw new InvalidOperationException( this.GetType().Name + " properties can only be set while initializing." );
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
          throw new InvalidOperationException( "Source property cannot be initialized on the " + this.GetType().Name );

        if( string.IsNullOrEmpty( this.AssemblyName ) || string.IsNullOrEmpty( this.SourcePath ) )
          throw new InvalidOperationException( "AssemblyName and SourcePath must be set during initialization" );

        // Build the pack uri based on the value of our properties
        Uri uri = this.BuildUri();

        // Load the resources
        this.Source = uri;
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
