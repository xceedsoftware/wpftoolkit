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
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Text;

namespace Xceed.Wpf.DataGrid.Markup
{
  [Browsable( false )]
  [EditorBrowsable( EditorBrowsableState.Never )]
  public abstract class SharedThemeResourceDictionary : SharedResourceDictionary
  {
    #region Static Fields

    private const string UriSchemePack = "pack";
    private const string UriComponent = ";component";
    private const string UriAssemblyVersionSymbol = "V";
    private const string UriAssemblyInfoSeparator = ";";
    private const string UriPathSeparator = "/";

    private const string AssemblyFullNameSeparator = ",";
    private const string AssemblyFullNameVersionPrefix = "Version=";
    private const string AssemblyFullNamePublicKeyTokenPrefix = "PublicKeyToken=";

    private static readonly Uri SchemePackBaseUri = new Uri( "pack://application:,,,/", UriKind.Absolute );

    #endregion

    protected abstract Uri GetBaseUri();

    protected sealed override bool TryCreateAbsoluteUri( Uri baseUri, Uri sourceUri, out Uri result )
    {
      Uri packUri;

      if( sourceUri != null )
      {
        if( SharedThemeResourceDictionary.TryConvertToPackUri( sourceUri, out packUri ) )
        {
          sourceUri = packUri;
        }

        UriDescription sourceUriDescription;
        if( SharedThemeResourceDictionary.TryGetUriDescription( sourceUri, out sourceUriDescription ) )
        {
          if( SharedThemeResourceDictionary.IsAssemblyInfoFilled( sourceUriDescription ) )
          {
            result = SharedThemeResourceDictionary.CreateUri( sourceUriDescription );
            return true;
          }
        }
      }

      if( SharedThemeResourceDictionary.TryConvertToPackUri( baseUri, out packUri ) )
      {
        baseUri = packUri;
      }

      var newBaseUri = this.GetBaseUri();
      if( newBaseUri != null )
      {
        if( SharedThemeResourceDictionary.TryConvertToPackUri( newBaseUri, out packUri ) )
        {
          newBaseUri = packUri;
        }

        if( baseUri != null )
        {
          Uri combinedUri;
          if( SharedThemeResourceDictionary.TryCombineUri( newBaseUri, baseUri, out combinedUri ) )
          {
            newBaseUri = combinedUri;
          }
        }

        if( sourceUri != null )
          return SharedThemeResourceDictionary.TryCombineUri( newBaseUri, sourceUri, out result );
      }
      else
      {
        newBaseUri = baseUri;
      }

      return base.TryCreateAbsoluteUri( newBaseUri, sourceUri, out result );
    }

    protected Uri CreateBaseUri( Assembly assembly )
    {
      if( assembly == null )
        return null;

      try
      {
        return SharedThemeResourceDictionary.CreateBaseUriFromAssembly( assembly );
      }
      catch( SecurityException )
      {
        return SharedThemeResourceDictionary.CreateBaseUriFromAssemblyFullName( assembly );
      }
    }

    private static Uri CreateBaseUriFromAssembly( Assembly assembly )
    {
      Debug.Assert( assembly != null );

      var assemblyInfo = assembly.GetName();
      var assemblyName = assemblyInfo.Name;
      var assemblyVersion = SharedThemeResourceDictionary.GetAssemblyVersion( assemblyInfo.Version );
      var assemblyPublicKeyToken = SharedThemeResourceDictionary.GetAssemblyPublicKeyToken( assemblyInfo.GetPublicKeyToken() );

      return SharedThemeResourceDictionary.CreateUri( assemblyName, assemblyVersion, assemblyPublicKeyToken, string.Empty );
    }

    private static Uri CreateBaseUriFromAssemblyFullName( Assembly assembly )
    {
      Debug.Assert( assembly != null );

      var fullName = assembly.FullName;
      var assemblyName = SharedThemeResourceDictionary.GetAssemblyName( fullName );
      var assemblyVersion = SharedThemeResourceDictionary.GetAssemblyVersion( fullName );
      var assemblyPublicKeyToken = SharedThemeResourceDictionary.GetAssemblyPublicKeyToken( fullName );

      return SharedThemeResourceDictionary.CreateUri( assemblyName, assemblyVersion, assemblyPublicKeyToken, string.Empty );
    }

    private static string GetAssemblyName( string fullName )
    {
      if( fullName == null )
        return null;

      var length = fullName.IndexOf( SharedThemeResourceDictionary.AssemblyFullNameSeparator, StringComparison.InvariantCultureIgnoreCase );
      if( length <= 0 )
        return null;

      return fullName.Substring( 0, length );
    }

    private static string GetAssemblyVersion( Version version )
    {
      if( version == null )
        return null;

      return SharedThemeResourceDictionary.UriAssemblyVersionSymbol + version.ToString();
    }

    private static string GetAssemblyVersion( string fullName )
    {
      if( fullName == null )
        return null;

      var start = fullName.IndexOf( SharedThemeResourceDictionary.AssemblyFullNameVersionPrefix, StringComparison.InvariantCultureIgnoreCase );
      if( start < 0 )
        return null;

      start += SharedThemeResourceDictionary.AssemblyFullNameVersionPrefix.Length;

      var end = fullName.IndexOf( SharedThemeResourceDictionary.AssemblyFullNameSeparator, start, StringComparison.InvariantCultureIgnoreCase );
      var value = ( end > 0 )
                    ? fullName.Substring( start, end - start )
                    : fullName.Substring( start );

      if( string.IsNullOrEmpty( value ) )
        return null;

      return SharedThemeResourceDictionary.UriAssemblyVersionSymbol + value;
    }

    private static string GetAssemblyPublicKeyToken( byte[] publicKeyToken )
    {
      if( publicKeyToken == null )
        return null;

      var sb = new StringBuilder();
      for( int i = 0; i < publicKeyToken.Length; i++ )
      {
        sb.AppendFormat( "{0:x2}", publicKeyToken[ i ] );
      }

      return sb.ToString();
    }

    private static string GetAssemblyPublicKeyToken( string fullName )
    {
      if( fullName == null )
        return null;

      var start = fullName.IndexOf( SharedThemeResourceDictionary.AssemblyFullNamePublicKeyTokenPrefix, StringComparison.InvariantCultureIgnoreCase );
      if( start < 0 )
        return null;

      start += SharedThemeResourceDictionary.AssemblyFullNamePublicKeyTokenPrefix.Length;

      var end = fullName.IndexOf( SharedThemeResourceDictionary.AssemblyFullNameSeparator, start, StringComparison.InvariantCultureIgnoreCase );
      var value = ( end > 0 )
                    ? fullName.Substring( start, end - start )
                    : fullName.Substring( start );

      return value;
    }

    private static string ParseAssemblyVersion( string version )
    {
      var symbol = SharedThemeResourceDictionary.UriAssemblyVersionSymbol;
      if( string.IsNullOrEmpty( version ) || !version.StartsWith( symbol, StringComparison.InvariantCultureIgnoreCase ) )
        return version;

      Version ver;
      if( !Version.TryParse( version.Substring( symbol.Length ), out ver ) )
        return version;

      return SharedThemeResourceDictionary.GetAssemblyVersion( ver );
    }

    private static Uri CreateUri( UriDescription description )
    {
      Debug.Assert( description != null );

      return SharedThemeResourceDictionary.CreateUri(
               description.AssemblyName,
               description.AssemblyVersion,
               description.AssemblyPublicKeyToken,
               description.RelativePath );
    }

    private static Uri CreateUri( string assemblyName, string assemblyVersion, string assemblyPublicKeyToken, string relativePath )
    {
      var sb = new StringBuilder( SharedThemeResourceDictionary.UriPathSeparator );

      sb.Append( assemblyName ?? string.Empty );

      sb.Append( SharedThemeResourceDictionary.UriAssemblyInfoSeparator );
      sb.Append( assemblyVersion ?? string.Empty );
      sb.Append( SharedThemeResourceDictionary.UriAssemblyInfoSeparator );
      sb.Append( assemblyPublicKeyToken ?? string.Empty );

      sb.Append( SharedThemeResourceDictionary.UriComponent );

      if( string.IsNullOrEmpty( relativePath ) || !relativePath.StartsWith( SharedThemeResourceDictionary.UriPathSeparator, StringComparison.InvariantCultureIgnoreCase ) )
      {
        sb.Append( SharedThemeResourceDictionary.UriPathSeparator );
      }

      sb.Append( relativePath ?? string.Empty );

      return new Uri( SharedThemeResourceDictionary.SchemePackBaseUri, sb.ToString() );
    }

    private static bool TryConvertToPackUri( Uri sourceUri, out Uri result )
    {
      if( SharedThemeResourceDictionary.IsPackUri( sourceUri ) )
      {
        result = new Uri( SharedThemeResourceDictionary.SchemePackBaseUri, sourceUri );
      }
      else
      {
        result = null;
      }

      return ( result != null );
    }

    private static bool IsPackUri( Uri sourceUri )
    {
      if( sourceUri == null )
        return false;

      var path = sourceUri.ToString();

      return ( path.IndexOf( SharedThemeResourceDictionary.UriComponent, StringComparison.InvariantCultureIgnoreCase ) >= 0 );
    }

    private static bool IsAssemblyInfoFilled( UriDescription description )
    {
      if( string.IsNullOrEmpty( description.AssemblyName ) )
        return false;

      return !string.IsNullOrEmpty( description.AssemblyVersion )
          || !string.IsNullOrEmpty( description.AssemblyPublicKeyToken );
    }

    private static bool TryGetUriDescription( Uri uri, out UriDescription description )
    {
      description = null;
      if( ( uri == null ) || !uri.IsAbsoluteUri )
        return false;

      if( !string.Equals( uri.Scheme, SharedThemeResourceDictionary.UriSchemePack, StringComparison.InvariantCultureIgnoreCase ) )
        return false;

      var segments = uri.Segments;
      if( ( segments == null ) || ( segments.Length < 2 ) )
        return false;

      var componentPart = segments[ 1 ];
      var index = componentPart.IndexOf( SharedThemeResourceDictionary.UriComponent, StringComparison.InvariantCultureIgnoreCase );
      if( index < 0 )
        return false;

      string assemblyName = null;
      string assemblyVersion = null;
      string assemblyPublicKeyToken = null;

      var assemblyInfo = componentPart.Substring( 0, index );
      if( assemblyInfo.Length > 0 )
      {
        var assemblyParts = assemblyInfo.Split( new string[] { SharedThemeResourceDictionary.UriAssemblyInfoSeparator }, StringSplitOptions.None );
        Debug.Assert( assemblyParts.Length <= 3 );

        assemblyName = ( assemblyParts.Length > 0 ) ? assemblyParts[ 0 ] : null;
        assemblyVersion = ( assemblyParts.Length > 1 ) ? SharedThemeResourceDictionary.ParseAssemblyVersion( assemblyParts[ 1 ] ) : null;
        assemblyPublicKeyToken = ( assemblyParts.Length > 2 ) ? assemblyParts[ 2 ] : null;
      }

      string relativePath = null;

      if( segments.Length > 2 )
      {
        var sb = new StringBuilder();
        for( int i = 2; i < segments.Length; i++ )
        {
          sb.Append( segments[ i ] );
        }

        relativePath = sb.ToString();
      }

      description = new UriDescription( assemblyName, assemblyVersion, assemblyPublicKeyToken, relativePath );

      return true;
    }

    private static bool TryCombineUri( Uri baseUri, Uri relativeUri, out Uri result )
    {
      result = null;

      Debug.Assert( ( baseUri != null ) && ( relativeUri != null ) );
      Debug.Assert( baseUri.IsAbsoluteUri );

      UriDescription baseUriDescription;
      if( !SharedThemeResourceDictionary.TryGetUriDescription( baseUri, out baseUriDescription ) )
        return false;

      UriDescription relativeUriDescription;
      if( SharedThemeResourceDictionary.TryGetUriDescription( relativeUri, out relativeUriDescription ) )
      {
        if( !string.IsNullOrEmpty( relativeUriDescription.AssemblyName ) && !string.Equals( relativeUriDescription.AssemblyName, baseUriDescription.AssemblyName, StringComparison.InvariantCultureIgnoreCase ) )
          return false;
      }
      else if( relativeUri.IsAbsoluteUri )
      {
        return false;
      }
      else
      {
        relativeUriDescription = null;
      }

      var baseUriPath = baseUriDescription.RelativePath;
      var relativeUriPath = ( relativeUriDescription != null ) ? relativeUriDescription.RelativePath : Uri.EscapeUriString( relativeUri.ToString() );

      if( relativeUriPath.StartsWith( SharedThemeResourceDictionary.UriPathSeparator, StringComparison.InvariantCultureIgnoreCase ) )
      {
        baseUriPath = string.Empty;
        relativeUriPath = relativeUriPath.Substring( SharedThemeResourceDictionary.UriPathSeparator.Length );
      }

      var assemblyDescription = ( ( relativeUriDescription != null ) && !SharedThemeResourceDictionary.IsAssemblyInfoFilled( baseUriDescription ) )
                                  ? relativeUriDescription
                                  : baseUriDescription;
      var newBaseUri = SharedThemeResourceDictionary.CreateUri(
                         assemblyDescription.AssemblyName,
                         assemblyDescription.AssemblyVersion,
                         assemblyDescription.AssemblyPublicKeyToken,
                         baseUriPath );
      var newRelativeUri = new Uri( relativeUriPath, UriKind.Relative );

      return Uri.TryCreate( newBaseUri, newRelativeUri, out result );
    }

    #region UriDescription Private Class

    private sealed class UriDescription
    {
      internal UriDescription( string assemblyName, string assemblyVersion, string assemblyPublicKeyToken, string relativePath )
      {
        this.AssemblyName = assemblyName ?? string.Empty;
        this.AssemblyVersion = assemblyVersion ?? string.Empty;
        this.AssemblyPublicKeyToken = assemblyPublicKeyToken ?? string.Empty;
        this.RelativePath = relativePath ?? string.Empty;
      }

      internal readonly string AssemblyName;
      internal readonly string AssemblyVersion;
      internal readonly string AssemblyPublicKeyToken;
      internal readonly string RelativePath;
    }

    #endregion
  }
}
