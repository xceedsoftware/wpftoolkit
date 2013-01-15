/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace Xceed.Wpf.DataGrid
{
  static class Log
  {
#if LOG
    static Log()
    {
      m_path = Environment.GetEnvironmentVariable( "AstoriaLogs" );

      if( string.IsNullOrEmpty( m_path ) )
        m_path = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), @"XceedLog" );

      Directory.CreateDirectory( m_path );
      Log.WriteLine( "" );
      Log.WriteLine( null, "*** App starting - Version : " + typeof( Log ).Assembly.GetName().Version.ToString() + " - Start time : " + DateTime.Now.ToString( "yy-MM-dd HH:mm:ss" ) );
      AppDomain.CurrentDomain.FirstChanceException += new EventHandler<FirstChanceExceptionEventArgs>( CurrentDomain_FirstChanceException );
    }

    static void CurrentDomain_FirstChanceException( object sender, FirstChanceExceptionEventArgs e )
    {
      Exception exception = e.Exception;

      if( exception != null )
      {
        StackTrace stackTrace = new StackTrace( 1 );
        string stackTraceString = stackTrace.ToString();

        if( stackTraceString.IndexOf( "xceed", StringComparison.InvariantCultureIgnoreCase ) >= 0 )
        {
          lock( m_unknowSource )
          {
            Log.WriteLine( m_unknowSource, exception.ToString() );
            Log.WriteLine( "--------------------" );
            Log.WriteLine( stackTraceString );
          }
        }
      }
      else
      {
        Log.WriteLine( "FirstChanceException with a null/no exception." );
      }
    }

    private static string m_path;
    private static string m_fileName = Environment.UserName + "-" + Environment.MachineName + "-" + DateTime.Now.ToString( "yyMMdd-HHmmss-" ) + Guid.NewGuid().ToString();
    private static int m_indentation = 0;
    private static UnknowSource m_unknowSource = new UnknowSource();

    public static void Assert( object source, bool condition, string message )
    {
      if( !condition )
      {
        Log.WriteLine( source, "# - " + message );

        StackTrace stackTrace = new StackTrace();
        Log.WriteLine( stackTrace.ToString() );
      }
    }

    public static void Start( object source, string message )
    {
      if( source == null )
        source = m_unknowSource;

      message = DateTime.Now.ToString( "HHmmss" ) + " | " +
        Thread.CurrentThread.GetHashCode().ToString() + " : " +
        source.GetHashCode().ToString( "0000000000" ) + " : " +
        new string( ' ', m_indentation * 2 ) +
        message + " - Start";

      Log.WriteLine( message );
      m_indentation++;
    }

    public static void End( object source, string message )
    {
      if( source == null )
        source = m_unknowSource;

      m_indentation--;

      if( m_indentation < 0 )
        m_indentation = 0;

      message = DateTime.Now.ToString( "HHmmss" ) + " | " +
        Thread.CurrentThread.GetHashCode().ToString() + " : " +
        source.GetHashCode().ToString( "0000000000" ) + " : " +
        new string( ' ', m_indentation * 2 ) +
        message + " - End";

      Log.WriteLine( message );
    }

    public static void WriteLine( object source, string message )
    {
      if( source == null )
        source = m_unknowSource;

      message = DateTime.Now.ToString( "HHmmss" ) + " | " +
        Thread.CurrentThread.GetHashCode().ToString() + " : " +
        source.GetHashCode().ToString( "0000000000" ) + " : " +
        new string( ' ', m_indentation * 2 ) +
        message;

      Log.WriteLine( message );
    }

    private static void WriteLine( string message )
    {
      lock( m_unknowSource )
      {
        using( TextWriter tw = new StreamWriter( m_path + "\\" + m_fileName + ".txt", true ) )
        {
          tw.WriteLine( message );
        }
      }
    }

    private class UnknowSource
    {
      public override int GetHashCode()
      {
        return -1;
      }

      public override bool Equals( object obj )
      {
        return base.Equals( obj );
      }
    }
#endif

    public static void NotToolkit( string featureName )
    {
      Console.WriteLine( Log.NotToolkitStr( featureName ) );
    }

    public static string NotToolkitStr( string featureName )
    {
      return string.Format( "{0} feature not available in the toolkit version of the DataGrid", featureName );
    }

  }
}
