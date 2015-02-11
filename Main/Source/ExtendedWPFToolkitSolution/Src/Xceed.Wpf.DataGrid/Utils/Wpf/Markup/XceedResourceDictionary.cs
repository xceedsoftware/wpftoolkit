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
using System.Windows;

namespace Xceed.Utils.Wpf.Markup
{
  [Browsable( false )]
  [EditorBrowsable( EditorBrowsableState.Never )]
  public sealed class XceedResourceDictionary : ResourceDictionary
  {
    #region XceedSource Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public string XceedSource
    {

      get
      {
        return m_xceedSource;
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "XceedSource" );

        m_xceedSource = value;

        string[] parsedArguments = m_xceedSource.Split( new char[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries );

        if( parsedArguments.Length != 2 )
          throw new ArgumentException( "Invalid URI format.", "XceedSource" );

        string uriString =  parsedArguments[ 0 ] + 
          ";;;" + parsedArguments[ 1 ];

        this.Source = new Uri( uriString, UriKind.RelativeOrAbsolute );
      }
    }

    private string m_xceedSource = string.Empty;

    #endregion
  }
}
