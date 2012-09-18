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
#if ( XCEEDCF || SILVERLIGHT )
      throw new ArgumentException( message, innerExcept );
#else
      throw new ArgumentException( message, paramName, innerExcept );
#endif
    }

    public static void ThrowArgumentOutOfRangeException( string paramName, object value, string message )
    {
#if ( XCEEDCF || SILVERLIGHT )
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
#if ( XCEEDCF || SILVERLIGHT )
      throw new SystemException( message );
#else
#if XBAP_FRIENDLY
      // Under some circumstances, passing a type to a LicenseException will throw a 
      // FileNotFoundException on the assembly containing the type.
      throw new System.ComponentModel.LicenseException( null, instance, message );
#else
      throw new System.ComponentModel.LicenseException( type, instance, message );
#endif
#endif
    }

    #endregion PUBLIC STATIC METHODS
  }
}
