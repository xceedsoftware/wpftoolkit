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

namespace Xceed.Utils.Exceptions
{
  internal class ThrowException
  {
    private ThrowException()
    {
    }

    #region PUBLIC STATIC METHODS

    public static void ThrowArgumentException( string message, string paramName, Exception innerExcept )
    {
      #if ( XCEEDCF || SILVERLIGHT || PORTABLE )
      throw new ArgumentException( message, innerExcept );
      #else
      throw new ArgumentException( message, paramName, innerExcept );
      #endif
    }

    public static void ThrowArgumentOutOfRangeException( string paramName, object value, string message )
    {
      #if ( XCEEDCF || SILVERLIGHT || PORTABLE )
      throw new ArgumentOutOfRangeException( message );
      #else
      throw new ArgumentOutOfRangeException( paramName, value, message );
      #endif
    }

    #if !NO_CODE_ANALYSIS
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "type" )]
    #endif // !NO_CODE_ANALYSIS
    public static void ThrowLicenseException( Type type, object instance, string message )
    {
      #if PORTABLE || NETCORE
      throw new Exception( message );
      #elif ( XCEEDCF || SILVERLIGHT || XAMARIN )
      throw new SystemException( message );
      #else
      #if XBAP_FRIENDLY
      // Under some circumstances, passing a type to a LicenseException will throw a 
      // FileNotFoundException on the assembly containing the type.
      throw new System.ComponentModel.LicenseException( null, instance, message );
      #else
      throw new System.ComponentModel.LicenseException( type, instance, message );
      #endif // XBAP_FRIENDLY
      #endif // ( XCEEDCF || NETCORE )
    }

    #endregion PUBLIC STATIC METHODS
  }
}
