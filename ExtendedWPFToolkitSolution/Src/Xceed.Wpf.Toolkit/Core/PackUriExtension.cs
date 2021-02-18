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
using System.Windows.Markup;

namespace Xceed.Wpf.Toolkit.Core
{
  [MarkupExtensionReturnType( typeof( Uri ) )]
  public class PackUriExtension : MarkupExtension
  {
    #region Constructors
    public PackUriExtension()
      :this( UriKind.Relative )
    {

    }

    public PackUriExtension( UriKind uriKind )
    {
      this.m_uriKind = uriKind;
    }

    #endregion // Constructors

    #region AssemblyName Property

    public string AssemblyName
    {
      get { return this.m_assemblyName; }
      set { this.m_assemblyName = value; }
    }

    #endregion // AssemblyName Property

    #region Path Property

    public string Path
    {
      get { return this.m_path; }
      set { this.m_path = value; }
    }

    #endregion // Path Property

    #region Kind Property

    public UriKind Kind
    {
      get { return this.m_uriKind; }
      set { this.m_uriKind = value; }
    }

    #endregion // Kind Property

    #region PUBLIC METHODS

    public override object ProvideValue( IServiceProvider serviceProvider )
    {
      if( string.IsNullOrEmpty( this.m_path ) )
        throw new InvalidOperationException( "Path must be set during initialization" );

      string uriString;

      switch( this.m_uriKind )
      {
        case UriKind.RelativeOrAbsolute:
        case UriKind.Relative:
          uriString = BuildRelativePackUriString( this.m_assemblyName, this.m_path );
          break;

        case UriKind.Absolute:
          uriString = BuildAbsolutePackUriString( this.m_assemblyName, this.m_path );
          break;

        default:
          throw new NotSupportedException();
      }

      return new Uri( uriString, this.m_uriKind );
    }

    #endregion PUBLIC METHODS

    #region INTERNAL METHODS

    internal static string BuildRelativePackUriString( string assemblyName, string path )
    {
      return BuildRelativePackUriString( assemblyName, String.Empty, path );
    }

    internal static string BuildRelativePackUriString( string assemblyName, string version, string path )
    {
      if( string.IsNullOrEmpty( assemblyName ) )
        throw new ArgumentException( "assemblyName cannot be null or empty", assemblyName );

      string platformSuffix = String.Empty;



      // If we have version information
      if( !String.IsNullOrEmpty( version ) )
      {
        // Format it for the pack uri
        version = ";v" + version;
      }

      // Format a relative pack uri string
      string uriString = string.Format( "/{0}{1}{2};component/{3}", assemblyName, platformSuffix, version, path );

      return uriString;
    }

    internal static string BuildAbsolutePackUriString( string assemblyName, string path )
    {
      return BuildAbsolutePackUriString( assemblyName, String.Empty, path );
    }

    internal static string BuildAbsolutePackUriString( string assemblyName, string version, string path )
    {
      string platformSuffix = String.Empty;
      bool hasAssemblyName = !String.IsNullOrEmpty( assemblyName );



      // If we have an assembly name and version information
      if( hasAssemblyName && !String.IsNullOrEmpty( version ) )
      {
        // Format it for the pack uri
        version = ";v" + version;
      }

      // Format an absolute pack uri string
      string uriString = string.Format( "pack://application:,,,/{0}{1}{2};component/{3}", assemblyName, platformSuffix, version, path );

      return uriString;
    }

    #endregion INTERNAL METHODS

    #region PRIVATE FIELDS

    private string m_assemblyName;
    private string m_path;
    private UriKind m_uriKind;

    #endregion PRIVATE FIELDS
  }
}
