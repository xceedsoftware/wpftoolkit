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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace Xceed.Wpf.DataGrid
{
  internal class Log
  {
    internal Log()
    {
      this.CreateBackgroungWorker();
    }

    #region SyncRoot Private Property

    private object SyncRoot
    {
      get
      {
        if( m_syncRoot == null )
        {
          Interlocked.CompareExchange( ref m_syncRoot, new object(), null );
        }

        return m_syncRoot;
      }
    }

    private object m_syncRoot;

    #endregion

    #region Indentation Methods

    private int GetIndentLevel()
    {
      return m_indentLevel;
    }

    private void IncreaseIndentLevel()
    {
      Interlocked.Increment( ref m_indentLevel );
    }

    private void DecreaseIndentLevel()
    {
      Interlocked.Decrement( ref m_indentLevel );
    }

    private int m_indentLevel;

    #endregion

    [Conditional( "LOG" )]
    private void CreateBackgroungWorker()
    {
      m_queueSyncRoot = new object();
      m_messageQueue = new Queue<string>( 100 );
      m_backgroundWorker = new BackgroundWorker();
      m_backgroundWorker.DoWork += new DoWorkEventHandler( DoWriteLine );
    }

    internal void Fail( string message )
    {
      this.Fail( UnknownSource.Instance, message );
    }

    internal void Fail( object source, string message )
    {
      this.Assert( source, false, message );
    }

    internal void Assert( bool condition, string message )
    {
      this.Assert( UnknownSource.Instance, condition, message );
    }

    internal void Assert( object source, bool condition, string message )
    {
      Debug.Assert( condition, message );
      this.AssertCore( source, condition, message );
    }

    [Conditional( "LOG" )]
    internal void WriteLine( object source, string message )
    {
      var sb = new StringBuilder();

      lock( this.SyncRoot )
      {
        sb.Append( this.GetHeader( source ) );
        sb.Append( this.GetMessage( string.Empty, message, string.Empty ) );

        this.QueueWriteLine( sb.ToString() );
      }
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal Block BeginBlock( object source, string message )
    {
      var location = new StackFrame( 1 ).GetMethod().Name;

      return new Block( this, source, location, message );
    }

    [Conditional( "LOG" )]
    private void Start( object source, string location, string message )
    {
      lock( this.SyncRoot )
      {
        var sb = new StringBuilder();
        sb.Append( this.GetHeader( source ) );
        sb.Append( this.GetMessage( location, "Start", message ) );

        this.QueueWriteLine( sb.ToString() );
        this.IncreaseIndentLevel();
      }
    }

    [Conditional( "LOG" )]
    private void End( object source, string location, string message )
    {
      lock( this.SyncRoot )
      {
        this.DecreaseIndentLevel();

        var sb = new StringBuilder();
        sb.Append( this.GetHeader( source ) );
        sb.Append( this.GetMessage( location, "End", message ) );

        this.QueueWriteLine( sb.ToString() );
      }
    }

    private string GetHeader( object source )
    {
      var sb = new StringBuilder();
      sb.AppendFormat( "{0} | {1}", DateTime.Now.ToString( "HHmmss" ), Thread.CurrentThread.GetHashCode() );

      if( ( source == null ) || ( source == UnknownSource.Instance ) )
      {
        sb.AppendFormat( " : {0,10}", "Unknown" );
      }
      else
      {
        sb.AppendFormat( " : {0:0000000000}", source.GetHashCode() );
      }

      sb.Append( " : " );

      var indentLevel = this.GetIndentLevel();
      if( indentLevel > 0 )
      {
        sb.Append( ' ', indentLevel * 2 );
      }

      return sb.ToString();
    }

    private string GetMessage( string prefix, string message, string suffix )
    {
      var sb = new StringBuilder();

      foreach( var part in new string[] { prefix, message, suffix } )
      {
        if( string.IsNullOrEmpty( part ) )
          continue;

        if( sb.Length > 0 )
        {
          sb.Append( " - " );
        }

        sb.Append( part );
      }

      return sb.ToString();
    }

    [Conditional( "LOG" )]
    private void QueueWriteLine( string message )
    {
      lock( m_queueSyncRoot )
      {
        m_messageQueue.Enqueue( message );
      }

      if( !m_backgroundWorker.IsBusy )
      {
        m_backgroundWorker.RunWorkerAsync();
      }
    }

    private void DoWriteLine( object sender, DoWorkEventArgs e )
    {
      string message;

      while( m_messageQueue.Count > 0 )
      {
        lock( m_queueSyncRoot )
        {
          message = m_messageQueue.Dequeue();
        }

        lock( this.SyncRoot )
        {
          using( TextWriter tw = new StreamWriter( m_defaultPath + "\\" + m_defaultFileName, true ) )
          {
            tw.WriteLine( message );
          }
        }
      }
    }

    [Conditional( "LOG" )]
    internal void AssertCore( object source, bool condition, string message )
    {
      if( condition )
        return;

      var stackTrace = new StackTrace();
      var sb = new StringBuilder();

      lock( this.SyncRoot )
      {
        sb.Append( this.GetHeader( source ) );
        sb.Append( this.GetMessage( "# - ", message, string.Empty ) );

        this.QueueWriteLine( sb.ToString() );
        this.QueueWriteLine( stackTrace.ToString() );
      }
    }

    [Conditional( "LOG" )]
    internal void StartUp( string gridUniqueName = "" )
    {
      m_defaultPath = Environment.GetEnvironmentVariable( "AstoriaLogs" );
      if( string.IsNullOrEmpty( m_defaultPath ) )
      {
        m_defaultPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), @"XceedLog" );
      }

      var now = DateTime.Now;
      m_defaultFileName = Environment.UserName + "-" + Environment.MachineName + "-" + now.ToString( "yyMMdd-HHmmss-" ) + Guid.NewGuid().ToString() + ".txt";

      this.OverridePaths( gridUniqueName );

      // Make sure the destination folder exists.
      if( !Directory.Exists( m_defaultPath ) )
      {
        Directory.CreateDirectory( m_defaultPath );
      }

      this.QueueWriteLine( "*** App starting - Version : " + typeof( Log ).Assembly.GetName().Version.ToString() + " - Start time : " + now.ToString( "yy-MM-dd HH:mm:ss" ) );

      AppDomain.CurrentDomain.FirstChanceException += new EventHandler<FirstChanceExceptionEventArgs>( this.OnFirstChanceException );
    }

    [Conditional( "CUSTOMLOG" )]
    private void OverridePaths( string gridUniqueName )
    {
      m_defaultPath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), @"..\Local Settings\Application Data\WMC\Gtseq\Logs\" );

      var now = DateTime.Now;
      m_defaultFileName = gridUniqueName + "-TID_" + Thread.CurrentThread.ManagedThreadId + "-" + now.ToString( "yyMMdd-HHmmss" ) + ".log";
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    private void OnFirstChanceException( object sender, FirstChanceExceptionEventArgs e )
    {
      this.WriteException( e.Exception as Exception );
    }

    [Conditional( "LOG" )]
    [MethodImpl( MethodImplOptions.NoInlining )]
    private void WriteException( Exception e )
    {
      if( e != null )
      {
        StackTrace stackTrace = new StackTrace( 2 );
        string stackTraceString = stackTrace.ToString();

        if( stackTraceString.IndexOf( "xceed", StringComparison.InvariantCultureIgnoreCase ) >= 0 )
        {
          lock( this.SyncRoot )
          {
            this.WriteLine( UnknownSource.Instance, e.ToString() );
            this.QueueWriteLine( "--------------------" );
            this.QueueWriteLine( stackTraceString );
          }
        }
      }
      else
      {
        this.QueueWriteLine( "FirstChanceException with a null/no exception." );
      }
    }

    private string m_defaultPath;
    private string m_defaultFileName;
    private object m_queueSyncRoot;
    private Queue<string> m_messageQueue;
    private BackgroundWorker m_backgroundWorker;

    #region UnknownSource Private Class

    private sealed class UnknownSource
    {
      internal static readonly UnknownSource Instance = new UnknownSource();

      private UnknownSource()
      {
      }

      public override int GetHashCode()
      {
        return 0;
      }

      public override bool Equals( object obj )
      {
        return base.Equals( obj );
      }

      public override string ToString()
      {
        return "Unknown source";
      }
    }

    #endregion

    #region Block Private Class

    internal sealed class Block : IDisposable
    {
      internal Block( Log log, object source, string location, string message )
      {
        if( source == null )
          throw new ArgumentNullException( "source" );

        m_log = log;
        m_source = source;
        m_location = location;

        m_log.Start( source, location, message );
      }

      internal void Break( string message )
      {
        if( Interlocked.CompareExchange( ref m_source, null, null ) == null )
          return;

        m_message = message;
      }

      void IDisposable.Dispose()
      {
        this.Dispose( true );
        GC.SuppressFinalize( this );
      }

      private void Dispose( bool disposing )
      {
        var source = m_source;
        if( Interlocked.CompareExchange( ref m_source, null, source ) == null )
          return;

        if( disposing )
        {
          m_log.End( source, m_location, m_message );
        }

        m_location = null;
        m_message = null;
      }

      ~Block()
      {
        this.Dispose( false );
      }

      private Log m_log;
      private object m_source;
      private string m_location;
      private string m_message;
    }

    #endregion
  }
}
