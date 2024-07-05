/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Standard
{
  using System;
  using System.ComponentModel;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;
  using System.Reflection;
  using System.Runtime.InteropServices;

  [StructLayout( LayoutKind.Explicit )]
  internal struct Win32Error
  {
    [FieldOffset( 0 )]
    private readonly int _value;

    // NOTE: These public static field declarations are automatically
    // picked up by (HRESULT's) ToString through reflection.

    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_SUCCESS = new Win32Error( 0 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_INVALID_FUNCTION = new Win32Error( 1 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_FILE_NOT_FOUND = new Win32Error( 2 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_PATH_NOT_FOUND = new Win32Error( 3 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_TOO_MANY_OPEN_FILES = new Win32Error( 4 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_ACCESS_DENIED = new Win32Error( 5 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_INVALID_HANDLE = new Win32Error( 6 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_OUTOFMEMORY = new Win32Error( 14 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_NO_MORE_FILES = new Win32Error( 18 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_SHARING_VIOLATION = new Win32Error( 32 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_INVALID_PARAMETER = new Win32Error( 87 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_INSUFFICIENT_BUFFER = new Win32Error( 122 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_NESTING_NOT_ALLOWED = new Win32Error( 215 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_KEY_DELETED = new Win32Error( 1018 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_NOT_FOUND = new Win32Error( 1168 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_NO_MATCH = new Win32Error( 1169 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_BAD_DEVICE = new Win32Error( 1200 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_CANCELLED = new Win32Error( 1223 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_CLASS_ALREADY_EXISTS = new Win32Error( 1410 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly Win32Error ERROR_INVALID_DATATYPE = new Win32Error( 1804 );

    public Win32Error( int i )
    {
      _value = i;
    }

    public static explicit operator HRESULT( Win32Error error )
    {
      // #define __HRESULT_FROM_WIN32(x) 
      //     ((HRESULT)(x) <= 0 ? ((HRESULT)(x)) : ((HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000)))
      if( error._value <= 0 )
      {
        return new HRESULT( ( uint )error._value );
      }
      return HRESULT.Make( true, Facility.Win32, error._value & 0x0000FFFF );
    }

    // Method version of the cast operation
    public HRESULT ToHRESULT()
    {
      return ( HRESULT )this;
    }

    [SuppressMessage( "Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands" )]
    public static Win32Error GetLastError()
    {
      return new Win32Error( Marshal.GetLastWin32Error() );
    }

    public override bool Equals( object obj )
    {
      try
      {
        return ( ( Win32Error )obj )._value == _value;
      }
      catch( InvalidCastException )
      {
        return false;
      }
    }

    public override int GetHashCode()
    {
      return _value.GetHashCode();
    }

    public static bool operator ==( Win32Error errLeft, Win32Error errRight )
    {
      return errLeft._value == errRight._value;
    }

    public static bool operator !=( Win32Error errLeft, Win32Error errRight )
    {
      return !( errLeft == errRight );
    }
  }

  internal enum Facility
  {
    Null = 0,
    Rpc = 1,
    Dispatch = 2,
    Storage = 3,
    Itf = 4,
    Win32 = 7,
    Windows = 8,
    Control = 10,
    Ese = 0xE5E,
    WinCodec = 0x898,
  }

  [StructLayout( LayoutKind.Explicit )]
  internal struct HRESULT
  {
    [FieldOffset( 0 )]
    private readonly uint _value;

    // NOTE: These public static field declarations are automatically
    // picked up by ToString through reflection.
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT S_OK = new HRESULT( 0x00000000 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT S_FALSE = new HRESULT( 0x00000001 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_PENDING = new HRESULT( 0x8000000A );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_NOTIMPL = new HRESULT( 0x80004001 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_NOINTERFACE = new HRESULT( 0x80004002 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_POINTER = new HRESULT( 0x80004003 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_ABORT = new HRESULT( 0x80004004 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_FAIL = new HRESULT( 0x80004005 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_UNEXPECTED = new HRESULT( 0x8000FFFF );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT STG_E_INVALIDFUNCTION = new HRESULT( 0x80030001 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT REGDB_E_CLASSNOTREG = new HRESULT( 0x80040154 );

    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT DESTS_E_NO_MATCHING_ASSOC_HANDLER = new HRESULT( 0x80040F03 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT DESTS_E_NORECDOCS = new HRESULT( 0x80040F04 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT DESTS_E_NOTALLCLEARED = new HRESULT( 0x80040F05 );

    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_ACCESSDENIED = new HRESULT( 0x80070005 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_OUTOFMEMORY = new HRESULT( 0x8007000E );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT E_INVALIDARG = new HRESULT( 0x80070057 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT INTSAFE_E_ARITHMETIC_OVERFLOW = new HRESULT( 0x80070216 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT COR_E_OBJECTDISPOSED = new HRESULT( 0x80131622 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT WC_E_GREATERTHAN = new HRESULT( 0xC00CEE23 );
    [SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
    public static readonly HRESULT WC_E_SYNTAX = new HRESULT( 0xC00CEE2D );

    public HRESULT( uint i )
    {
      _value = i;
    }

    public static HRESULT Make( bool severe, Facility facility, int code )
    {
      // #define MAKE_HRESULT(sev,fac,code) \
      //    ((HRESULT) (((unsigned long)(sev)<<31) | ((unsigned long)(fac)<<16) | ((unsigned long)(code))) )

      // Severity has 1 bit reserved.
      // bitness is enforced by the boolean parameter.

      // Facility has 11 bits reserved (different than SCODES, which have 4 bits reserved)
      // MSDN documentation incorrectly uses 12 bits for the ESE facility (e5e), so go ahead and let that one slide.
      // And WIC also ignores it the documented size...
      Assert.Implies( ( int )facility != ( int )( ( int )facility & 0x1FF ), facility == Facility.Ese || facility == Facility.WinCodec );
      // Code has 4 bits reserved.
      Assert.AreEqual( code, code & 0xFFFF );

      return new HRESULT( ( uint )( ( severe ? ( 1 << 31 ) : 0 ) | ( ( int )facility << 16 ) | code ) );
    }

    public Facility Facility
    {
      get
      {
        return GetFacility( ( int )_value );
      }
    }

    public static Facility GetFacility( int errorCode )
    {
      // #define HRESULT_FACILITY(hr)  (((hr) >> 16) & 0x1fff)
      return ( Facility )( ( errorCode >> 16 ) & 0x1fff );
    }

    public int Code
    {
      get
      {
        return GetCode( ( int )_value );
      }
    }

    public static int GetCode( int error )
    {
      // #define HRESULT_CODE(hr)    ((hr) & 0xFFFF)
      return ( int )( error & 0xFFFF );
    }

    #region Object class override members

    public override string ToString()
    {
      // Use reflection to try to name this HRESULT.
      // This is expensive, but if someone's ever printing HRESULT strings then
      // I think it's a fair guess that they're not in a performance critical area
      // (e.g. printing exception strings).
      // This is less error prone than trying to keep the list in the function.
      // To properly add an HRESULT's name to the ToString table, just add the HRESULT
      // like all the others above.
      //
      // CONSIDER: This data is static.  It could be cached 
      // after first usage for fast lookup since the keys are unique.
      //
      foreach( FieldInfo publicStaticField in typeof( HRESULT ).GetFields( BindingFlags.Static | BindingFlags.Public ) )
      {
        if( publicStaticField.FieldType == typeof( HRESULT ) )
        {
          var hr = ( HRESULT )publicStaticField.GetValue( null );
          if( hr == this )
          {
            return publicStaticField.Name;
          }
        }
      }

      // Try Win32 error codes also
      if( Facility == Facility.Win32 )
      {
        foreach( FieldInfo publicStaticField in typeof( Win32Error ).GetFields( BindingFlags.Static | BindingFlags.Public ) )
        {
          if( publicStaticField.FieldType == typeof( Win32Error ) )
          {
            var error = ( Win32Error )publicStaticField.GetValue( null );
            if( ( HRESULT )error == this )
            {
              return "HRESULT_FROM_WIN32(" + publicStaticField.Name + ")";
            }
          }
        }
      }

      // If there's no good name for this HRESULT,
      // return the string as readable hex (0x########) format.
      return string.Format( CultureInfo.InvariantCulture, "0x{0:X8}", _value );
    }

    public override bool Equals( object obj )
    {
      try
      {
        return ( ( HRESULT )obj )._value == _value;
      }
      catch( InvalidCastException )
      {
        return false;
      }
    }

    public override int GetHashCode()
    {
      return _value.GetHashCode();
    }

    #endregion

    public static bool operator ==( HRESULT hrLeft, HRESULT hrRight )
    {
      return hrLeft._value == hrRight._value;
    }

    public static bool operator !=( HRESULT hrLeft, HRESULT hrRight )
    {
      return !( hrLeft == hrRight );
    }

    public bool Succeeded
    {
      get
      {
        return ( int )_value >= 0;
      }
    }

    public bool Failed
    {
      get
      {
        return ( int )_value < 0;
      }
    }

    public void ThrowIfFailed()
    {
      ThrowIfFailed( null );
    }

    [
        SuppressMessage(
            "Microsoft.Usage",
            "CA2201:DoNotRaiseReservedExceptionTypes",
            Justification = "Only recreating Exceptions that were already raised." ),
        SuppressMessage(
            "Microsoft.Security",
            "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands" )
    ]
    public void ThrowIfFailed( string message )
    {
      if( Failed )
      {
        if( string.IsNullOrEmpty( message ) )
        {
          message = ToString();
        }
#if DEBUG
        else
        {
          message += " (" + ToString() + ")";
        }
#endif
        // Wow.  Reflection in a throw call.  Later on this may turn out to have been a bad idea.
        // If you're throwing an exception I assume it's OK for me to take some time to give it back.
        // I want to convert the HRESULT to a more appropriate exception type than COMException.
        // Marshal.ThrowExceptionForHR does this for me, but the general call uses GetErrorInfo
        // if it's set, and then ignores the HRESULT that I've provided.  This makes it so this
        // call works the first time but you get burned on the second.  To avoid this, I use
        // the overload that explicitly ignores the IErrorInfo.
        // In addition, the function doesn't allow me to set the Message unless I go through
        // the process of implementing an IErrorInfo and then use that.  There's no stock
        // implementations of IErrorInfo available and I don't think it's worth the maintenance
        // overhead of doing it, nor would it have significant value over this approach.
        Exception e = Marshal.GetExceptionForHR( ( int )_value, new IntPtr( -1 ) );
        Assert.IsNotNull( e );
        // ArgumentNullException doesn't have the right constructor parameters,
        // (nor does Win32Exception...)
        // but E_POINTER gets mapped to NullReferenceException,
        // so I don't think it will ever matter.
        Assert.IsFalse( e is ArgumentNullException );

        // If we're not getting anything better than a COMException from Marshal,
        // then at least check the facility and attempt to do better ourselves.
        if( e.GetType() == typeof( COMException ) )
        {
          switch( Facility )
          {
            case Facility.Win32:
              e = new Win32Exception( Code, message );
              break;
            default:
              e = new COMException( message, ( int )_value );
              break;
          }
        }
        else
        {
          ConstructorInfo cons = e.GetType().GetConstructor( new[] { typeof( string ) } );
          if( null != cons )
          {
            e = cons.Invoke( new object[] { message } ) as Exception;
            Assert.IsNotNull( e );
          }
        }
        throw e;
      }
    }

    public static void ThrowLastError()
    {
      ( ( HRESULT )Win32Error.GetLastError() ).ThrowIfFailed();
      // Only expecting to call this when we're expecting a failed GetLastError()
      Assert.Fail();
    }
  }
}
