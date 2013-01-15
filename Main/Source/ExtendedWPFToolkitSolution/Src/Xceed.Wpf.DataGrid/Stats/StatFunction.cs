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
using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

using Xceed.Wpf.DataGrid.Markup;
using Xceed.Utils;
using Xceed.Utils.Math;

namespace Xceed.Wpf.DataGrid.Stats
{
  internal abstract class StatFunction
  {
    static StatFunction()
    {
    }

    protected StatFunction()
    {
    }

    protected StatFunction( string resultPropertyName, string sourcePropertyName )
      : this()
    {
      m_resultPropertyName = resultPropertyName;
      m_sourcePropertyName = sourcePropertyName;
      this.ExtractSourcePropertyNames();
    }

    #region ResultPropertyName Property

    private string m_resultPropertyName;

    public string ResultPropertyName
    {
      get
      {
        return m_resultPropertyName;
      }
      set
      {
        this.CheckSealed();
        m_resultPropertyName = value;
      }
    }

    #endregion ResultPropertyName Property

    #region SourcePropertyName Property

    private string m_sourcePropertyName;

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public string SourcePropertyName
    {
      get
      {
        return m_sourcePropertyName;
      }

      set
      {
        if( value == null )
          throw new ArgumentNullException( "SourcePropertyName" );

        if( value.Trim().Length == 0 )
          throw new ArgumentException( "SourcePropertyName cannot be empty.", "SourcePropertyName" );

        this.CheckSealed();
        m_sourcePropertyName = value;
        this.ExtractSourcePropertyNames();
      }
    }

    #endregion SourcePropertyName Property

    #region PrerequisiteFunctions Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays" )]
    protected internal virtual StatFunction[] PrerequisiteFunctions
    {
      get
      {
        return null;
      }
    }

    #endregion PrerequisiteFunctions Property

    #region SourcePropertyNames Property

    private string[] m_sourcePropertyNames = new string[ 0 ];

    internal string[] SourcePropertyNames
    {
      get
      {
        return m_sourcePropertyNames;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    private void ExtractSourcePropertyNames()
    {
      if( m_sourcePropertyName != null )
      {
        m_sourcePropertyNames = m_sourcePropertyName.Split( ',' );

        for( int i = 0; i < m_sourcePropertyNames.Length; i++ )
        {
          m_sourcePropertyNames[ i ] = m_sourcePropertyNames[ i ].Trim();

          if( m_sourcePropertyNames[ i ].Length == 0 )
            throw new ArgumentException( "A multi-part SourcePropertyName cannot include an empty property name.", "SourcePropertyName" );
        }
      }
    }

    #endregion SourcePropertyNames Property

    #region IsSealed Property

    private bool m_sealed = false;

    protected bool IsSealed
    {
      get
      {
        return m_sealed;
      }
    }

    #endregion IsSealed Property

    #region InCalculation Property

    private bool m_inCalculation;

    internal bool InCalculation
    {
      get
      {
        return m_inCalculation;
      }
    }

    internal void StartCalculation()
    {
      m_inCalculation = true;
    }

    internal void EndCalculation()
    {
      m_inCalculation = false;
    }

    #endregion InCalculation Property

    protected internal virtual bool RequiresAccumulation
    {
      get
      {
        return true;
      }
    }

    protected internal virtual bool IsInitialized
    {
      get
      {
        return m_initialized;
      }
    }

    protected internal virtual void Validate()
    {
      if( ( m_resultPropertyName == null ) || ( m_resultPropertyName.Length == 0 ) )
        throw new InvalidOperationException( "The ResultPropertyName property must be set." );
    }

    protected internal virtual void InitializeFromInstance( StatFunction source )
    {
      this.SourcePropertyName = source.SourcePropertyName;
    }

    protected virtual void Initialize( Type[] sourcePropertyTypes )
    {
    }

    internal void InitializeAccumulationTypes( Type[] sourcePropertyTypes )
    {
      this.Initialize( sourcePropertyTypes );
      m_initialized = true;
    }

    protected internal virtual void Reset()
    {
    }

    protected internal virtual void InitializePrerequisites( StatResult[] prerequisitesValues )
    {
      throw new NotImplementedException( "InitializePrerequisites must also be overridden when PrerequisiteFunctions has been overridden." ); 
    }

    protected internal virtual void Accumulate( object[] values )
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
    protected internal virtual StatResult GetResult()
    {
      return new StatResult( new InvalidOperationException( Log.NotToolkitStr( "Statistical functions" ) ) );
    }

    protected void CheckSealed()
    {
      if( m_sealed )
        throw new InvalidOperationException( "The StatFunction cannot be changed once it has been added to the DataGridCollectionView." );
    }

    protected internal virtual bool IsEquivalent( StatFunction statFunction )
    {
      if( statFunction == null )
        return false;

      return ( this.GetType() == statFunction.GetType() ) &&
             ( this.SourcePropertyName == statFunction.SourcePropertyName );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate" )]
    protected internal virtual int GetEquivalenceKey()
    {
      return this.GetType().GetHashCode() ^ this.SourcePropertyName.GetHashCode();
    }

    internal static bool AreEquivalents( StatFunction statFunctionA, StatFunction statFunctionB )
    {
      if( ( statFunctionA != null ) && ( statFunctionB != null ) )
      {
        return statFunctionA.IsEquivalent( statFunctionB );
      }
      else
      {
        return ( statFunctionA == null ) && ( statFunctionB == null );
      }
    }

    internal void ValidateSourcePropertyName( int expectedPropertyCount )
    {
      if( this.SourcePropertyNames.Length != expectedPropertyCount )
        throw new InvalidOperationException( string.Format( "The SourcePropertyName property must be set to {0} field{1} with the Statistical function {2}.",
          expectedPropertyCount, ( expectedPropertyCount > 1 ) ? "s" : "", this.GetType().Name ) );
    }

    internal void Seal()
    {
      m_sealed = true;
    }

    internal static bool CanProcessValues( object[] values, int count )
    {
      if( count > values.Length )
        return false;

      for( int i = 0; i < count; i++ )
      {
        if( ( values[ i ] == null ) || ( values[ i ] is DBNull ) )
          return false;
      }

      return true;
    }

    internal static TypeCode GetDefaultNumericalAccumulationType( Type dataType )
    {
      TypeCode accType = TypeCode.Empty;

      switch( Type.GetTypeCode( dataType ) )
      {
        case TypeCode.Char:
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          accType = TypeCode.Int64;
          break;

        case TypeCode.Single:
        case TypeCode.Double:
          accType = TypeCode.Double;
          break;

        case TypeCode.Decimal:
          accType = TypeCode.Decimal;
          break;
      }

      return accType;
    }

    private bool m_initialized; // = false
  }
}
