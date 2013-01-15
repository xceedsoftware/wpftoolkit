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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  [DebuggerStepThrough]
  internal sealed class NotifyPropertyChangedHelper
  {
    #region Constructor

    internal NotifyPropertyChangedHelper(
      INotifyPropertyChanged owner,
      Action<string> notifyPropertyChangedDelegate )
    {
      if( owner == null )
        throw new ArgumentNullException( "owner" );

      if( notifyPropertyChangedDelegate == null )
        throw new ArgumentNullException( "notifyPropertyChangedDelegate" );

      m_owner = owner;
      m_delegate = notifyPropertyChangedDelegate;
    }

    #endregion

    internal static bool PropertyChanged( string propertyName, PropertyChangedEventArgs e, bool targetPropertyOnly )
    {
      string target = e.PropertyName;
      if( target == propertyName )
        return true;

      return ( !targetPropertyOnly )
          && ( string.IsNullOrEmpty( target ) );
    }

    internal static bool PropertyChanged<TOwner, TMember>(
      Expression<Func<TMember>> expression,
      PropertyChangedEventArgs e,
      bool targetPropertyOnly )
    {
      var body = expression.Body as MemberExpression;
      if( body == null )
        throw new ArgumentException( "The expression must target a property or field.", "expression" );

      return NotifyPropertyChangedHelper.PropertyChanged( body, typeof( TOwner ), e, targetPropertyOnly );
    }

    internal static bool PropertyChanged<TOwner, TMember>(
      Expression<Func<TOwner, TMember>> expression,
      PropertyChangedEventArgs e,
      bool targetPropertyOnly )
    {
      var body = expression.Body as MemberExpression;
      if( body == null )
        throw new ArgumentException( "The expression must target a property or field.", "expression" );

      return NotifyPropertyChangedHelper.PropertyChanged( body, typeof( TOwner ), e, targetPropertyOnly );
    }

    internal void RaisePropertyChanged( string propertyName )
    {
      ReflectionHelper.ValidatePropertyName( m_owner, propertyName );

      this.InvokeDelegate( propertyName );
    }

    internal void RaisePropertyChanged<TMember>( Expression<Func<TMember>> expression )
    {
      if( expression == null )
        throw new ArgumentNullException( "expression" );

      var body = expression.Body as MemberExpression;
      if( body == null )
        throw new ArgumentException( "The expression must target a property or field.", "expression" );

      var propertyName = NotifyPropertyChangedHelper.GetPropertyName( body, m_owner.GetType() );

      this.InvokeDelegate( propertyName );
    }

    internal void HandleReferenceChanged<TMember>( Expression<Func<TMember>> expression, ref TMember localReference, TMember newValue ) where TMember : class
    {
      if( localReference != newValue )
      {
        this.ExecutePropertyChanged( expression, ref localReference, newValue );
      }
    }

    internal void HandleEqualityChanged<TMember>( Expression<Func<TMember>> expression, ref TMember localReference, TMember newValue )
    {
      if( !object.Equals( localReference, newValue ) )
      {
        this.ExecutePropertyChanged( expression, ref localReference, newValue );
      }
    }

    private void ExecutePropertyChanged<TMember>( Expression<Func<TMember>> expression, ref TMember localReference, TMember newValue )
    {
      TMember oldValue = localReference;
      localReference = newValue;
      this.RaisePropertyChanged( expression );
    }

    internal static string GetPropertyName<TMember>( Expression<Func<TMember>> expression, Type ownerType )
    {
      var body = expression.Body as MemberExpression;
      if( body == null )
        throw new ArgumentException( "The expression must target a property or field.", "expression" );

      return NotifyPropertyChangedHelper.GetPropertyName( body, ownerType );
    }

    private static bool PropertyChanged( MemberExpression expression, Type ownerType, PropertyChangedEventArgs e, bool targetPropertyOnly )
    {
      var propertyName = NotifyPropertyChangedHelper.GetPropertyName( expression, ownerType );

      return NotifyPropertyChangedHelper.PropertyChanged( propertyName, e, targetPropertyOnly );
    }

    private static string GetPropertyName( MemberExpression expression, Type ownerType )
    {
      var targetType = expression.Expression.Type;
      if( !targetType.IsAssignableFrom( ownerType ) )
        throw new ArgumentException( "The expression must target a property or field on the appropriate owner.", "expression" );

      return ReflectionHelper.GetPropertyOrFieldName( expression );
    }

    private void InvokeDelegate( string propertyName )
    {
      m_delegate.Invoke( propertyName );
    }

    #region Private Fields

    private readonly INotifyPropertyChanged m_owner;
    private readonly Action<string> m_delegate;

    #endregion
  }
}
