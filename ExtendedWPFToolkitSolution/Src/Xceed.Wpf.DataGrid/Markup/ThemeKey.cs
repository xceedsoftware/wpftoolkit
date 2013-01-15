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
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid.Markup
{
  [MarkupExtensionReturnType( typeof( ThemeKey ) )]
  public class ThemeKey : ResourceKey
  {
    public ThemeKey()
    {
    }

    public ThemeKey( Type targetViewType, Type themeType, Type targetElementType )
    {
      if( targetViewType == null )
        throw new ArgumentNullException( "targetViewType" );

      if( !typeof( ViewBase ).IsAssignableFrom( targetViewType ) )
        throw new ArgumentException( "The specified view type must derive from ViewBase.", "targetViewType" );

      m_targetViewType = targetViewType;
      m_targetViewTypeInitialized = true;
      m_themeType = themeType;
      m_themeTypeInitialized = true;
      m_targetElementType = targetElementType;
      m_targetElementTypeInitialized = true;
    }

    public ThemeKey( Type targetViewType, Type targetElementType )
      : this( targetViewType, null, targetElementType )
    {
    }

    #region TargetViewType Property

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    public Type TargetViewType
    {
      get
      {
        return m_targetViewType;
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "TargetViewType" );

        if( !typeof( ViewBase ).IsAssignableFrom( value ) )
          throw new ArgumentException( "The specified view type must derive from ViewBase.", "TargetViewType" );

        if( m_targetViewTypeInitialized )
          throw new InvalidOperationException( "An attempt was made to set the TargetViewType property when it has already been initialized." );

        m_targetViewType = value;
        m_targetViewTypeInitialized = true;
      }
    }

    private Type m_targetViewType; // = null;
    private bool m_targetViewTypeInitialized; // = false;

    #endregion TargetViewType Property

    #region ThemeType Property

    public Type ThemeType
    {
      get
      {
        return m_themeType;
      }
      set
      {
        if( m_themeTypeInitialized )
          throw new InvalidOperationException( "An attempt was made to set the ThemeType property when it has already been initialized." );

        m_themeType = value;
        m_themeTypeInitialized = true;
      }
    }

    private Type m_themeType; // = null;
    private bool m_themeTypeInitialized; // = false;

    #endregion ThemeType Property

    #region TargetElementType Property

    public Type TargetElementType
    {
      get
      {
        return m_targetElementType;
      }
      set
      {
        if( m_targetElementTypeInitialized )
          throw new InvalidOperationException( "An attempt was made to set the TargetElementType property when it has already been initialized." );

        m_targetElementType = value;
        m_targetElementTypeInitialized = true;
      }
    }

    private Type m_targetElementType; // = null;
    private bool m_targetElementTypeInitialized; // = false;

    #endregion TargetElementType Property

    public override Assembly Assembly
    {
      get
      {
        if( m_themeType != null )
          return m_themeType.Assembly;

        if( m_targetViewType != null )
          return m_targetViewType.Assembly;

        return null;
      }
    }

    public override bool Equals( object obj )
    {
      ThemeKey key = obj as ThemeKey;

      if( key == null )
        return false;

      if( !( ( key.TargetViewType != null ) ? key.TargetViewType.Equals( this.TargetViewType ) : ( this.TargetViewType == null ) ) )
        return false;

      if( !( ( key.ThemeType != null ) ? key.ThemeType.Equals( this.ThemeType ) : ( this.ThemeType == null ) ) )
        return false;

      if( !( ( key.TargetElementType != null ) ? key.TargetElementType.Equals( this.TargetElementType ) : ( this.TargetElementType == null ) ) )
        return false;

      return true;
    }

    public override int GetHashCode()
    {
      return ( ( ( this.TargetViewType != null ) ? this.TargetViewType.GetHashCode() : 0 )
        ^ ( ( this.ThemeType != null ) ? this.ThemeType.GetHashCode() : 0 )
        ^ ( ( this.TargetElementType != null ) ? this.TargetElementType.GetHashCode() : 0 ) );
    }
  }
}
