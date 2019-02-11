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
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
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
      if( object.ReferenceEquals( obj, this ) )
        return true;

      var key = obj as ThemeKey;
      if( object.ReferenceEquals( key, null ) )
        return false;

      return ( object.Equals( key.TargetViewType, this.TargetViewType ) )
          && ( object.Equals( key.ThemeType, this.ThemeType ) )
          && ( object.Equals( key.TargetElementType, this.TargetElementType ) );
    }

    public override int GetHashCode()
    {
      var hashCode = 0;

      var elementType = this.TargetElementType;
      if( elementType != null )
      {
        hashCode += elementType.GetHashCode();
      }

      var viewType = this.TargetViewType;
      if( viewType != null )
      {
        hashCode *= 11;
        hashCode += viewType.GetHashCode();
      }

      var themeType = this.ThemeType;
      if( themeType != null )
      {
        hashCode *= 17;
        hashCode += themeType.GetHashCode();
      }

      return hashCode;
    }
  }
}
