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
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Documents;
using System.Globalization;
using Microsoft.Win32;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Automation;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Core;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Primitives
{
  public class ValueRangeTextBox : AutoSelectTextBox
  {
    static ValueRangeTextBox()
    {
      ValueRangeTextBox.TextProperty.OverrideMetadata( typeof( ValueRangeTextBox ),
        new FrameworkPropertyMetadata(
        null,
        new CoerceValueCallback( ValueRangeTextBox.TextCoerceValueCallback ) ) );

      ValueRangeTextBox.AcceptsReturnProperty.OverrideMetadata( typeof( ValueRangeTextBox ),
        new FrameworkPropertyMetadata(
        false, null, new CoerceValueCallback( ValueRangeTextBox.AcceptsReturnCoerceValueCallback ) ) );

      ValueRangeTextBox.AcceptsTabProperty.OverrideMetadata( typeof( ValueRangeTextBox ),
        new FrameworkPropertyMetadata(
        false, null, new CoerceValueCallback( ValueRangeTextBox.AcceptsTabCoerceValueCallback ) ) );
    }

    public ValueRangeTextBox()
    {
    }

    #region AcceptsReturn Property

    private static object AcceptsReturnCoerceValueCallback( DependencyObject sender, object value )
    {
      bool acceptsReturn = ( bool )value;

      if( acceptsReturn )
        throw new NotSupportedException( "The ValueRangeTextBox does not support the AcceptsReturn property." );

      return false;
    }

    #endregion AcceptsReturn Property

    #region AcceptsTab Property

    private static object AcceptsTabCoerceValueCallback( DependencyObject sender, object value )
    {
      bool acceptsTab = ( bool )value;

      if( acceptsTab )
        throw new NotSupportedException( "The ValueRangeTextBox does not support the AcceptsTab property." );

      return false;
    }

    #endregion AcceptsTab Property

    #region BeepOnError Property

    public bool BeepOnError
    {
      get
      {
        return ( bool )GetValue( BeepOnErrorProperty );
      }
      set
      {
        SetValue( BeepOnErrorProperty, value );
      }
    }

    public static readonly DependencyProperty BeepOnErrorProperty =
        DependencyProperty.Register( "BeepOnError", typeof( bool ), typeof( ValueRangeTextBox ), new UIPropertyMetadata( false ) );

    #endregion BeepOnError Property

    #region FormatProvider Property

    public IFormatProvider FormatProvider
    {
      get
      {
        return ( IFormatProvider )GetValue( FormatProviderProperty );
      }
      set
      {
        SetValue( FormatProviderProperty, value );
      }
    }

    public static readonly DependencyProperty FormatProviderProperty =
        DependencyProperty.Register( "FormatProvider", typeof( IFormatProvider ), typeof( ValueRangeTextBox ),
      new UIPropertyMetadata( null,
      new PropertyChangedCallback( ValueRangeTextBox.FormatProviderPropertyChangedCallback ) ) );

    private static void FormatProviderPropertyChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ValueRangeTextBox valueRangeTextBox = ( ValueRangeTextBox )sender;

      if( !valueRangeTextBox.IsInitialized )
        return;

      valueRangeTextBox.OnFormatProviderChanged();
    }

    internal virtual void OnFormatProviderChanged()
    {
      this.RefreshConversionHelpers();
      this.RefreshCurrentText( false );
      this.RefreshValue();
    }

    #endregion FormatProvider Property

    #region MinValue Property

    public object MinValue
    {
      get
      {
        return ( object )GetValue( MinValueProperty );
      }
      set
      {
        SetValue( MinValueProperty, value );
      }
    }

    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register( "MinValue", typeof( object ), typeof( ValueRangeTextBox ),
      new UIPropertyMetadata(
      null,
      null,
      new CoerceValueCallback( ValueRangeTextBox.MinValueCoerceValueCallback ) ) );

    private static object MinValueCoerceValueCallback( DependencyObject sender, object value )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      if( !valueRangeTextBox.IsInitialized )
        return DependencyProperty.UnsetValue;

      if( value == null )
        return value;

      Type type = valueRangeTextBox.ValueDataType;

      if( type == null )
        throw new InvalidOperationException( "An attempt was made to set a minimum value when the ValueDataType property is null." );

      if( valueRangeTextBox.IsFinalizingInitialization )
        value = ValueRangeTextBox.ConvertValueToDataType( value, valueRangeTextBox.ValueDataType );

      if( value.GetType() != type )
        throw new ArgumentException( "The value is not of type " + type.Name + ".", "MinValue" );

      IComparable comparable = value as IComparable;

      if( comparable == null )
        throw new InvalidOperationException( "MinValue does not implement the IComparable interface." );

      // ValidateValueInRange will throw if it must.
      object maxValue = valueRangeTextBox.MaxValue;

      valueRangeTextBox.ValidateValueInRange( value, maxValue, valueRangeTextBox.Value );

      return value;
    }

    #endregion MinValue Property

    #region MaxValue Property

    public object MaxValue
    {
      get
      {
        return ( object )GetValue( MaxValueProperty );
      }
      set
      {
        SetValue( MaxValueProperty, value );
      }
    }

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register( "MaxValue", typeof( object ), typeof( ValueRangeTextBox ),
      new UIPropertyMetadata(
      null,
      null,
      new CoerceValueCallback( MaxValueCoerceValueCallback ) ) );

    private static object MaxValueCoerceValueCallback( DependencyObject sender, object value )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      if( !valueRangeTextBox.IsInitialized )
        return DependencyProperty.UnsetValue;

      if( value == null )
        return value;

      Type type = valueRangeTextBox.ValueDataType;

      if( type == null )
        throw new InvalidOperationException( "An attempt was made to set a maximum value when the ValueDataType property is null." );

      if( valueRangeTextBox.IsFinalizingInitialization )
        value = ValueRangeTextBox.ConvertValueToDataType( value, valueRangeTextBox.ValueDataType );

      if( value.GetType() != type )
        throw new ArgumentException( "The value is not of type " + type.Name + ".", "MinValue" );

      IComparable comparable = value as IComparable;

      if( comparable == null )
        throw new InvalidOperationException( "MaxValue does not implement the IComparable interface." );

      object minValue = valueRangeTextBox.MinValue;

      // ValidateValueInRange will throw if it must.
      valueRangeTextBox.ValidateValueInRange( minValue, value, valueRangeTextBox.Value );

      return value;
    }

    #endregion MaxValue Property

    #region NullValue Property

    public object NullValue
    {
      get
      {
        return ( object )GetValue( NullValueProperty );
      }
      set
      {
        SetValue( NullValueProperty, value );
      }
    }

    public static readonly DependencyProperty NullValueProperty =
        DependencyProperty.Register( "NullValue", typeof( object ), typeof( ValueRangeTextBox ),
      new UIPropertyMetadata(
      null,
      new PropertyChangedCallback( ValueRangeTextBox.NullValuePropertyChangedCallback ),
      new CoerceValueCallback( NullValueCoerceValueCallback ) ) );

    private static object NullValueCoerceValueCallback( DependencyObject sender, object value )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      if( !valueRangeTextBox.IsInitialized )
        return DependencyProperty.UnsetValue;

      if( ( value == null ) || ( value == DBNull.Value ) )
        return value;

      Type type = valueRangeTextBox.ValueDataType;

      if( type == null )
        throw new InvalidOperationException( "An attempt was made to set a null value when the ValueDataType property is null." );

      if( valueRangeTextBox.IsFinalizingInitialization )
        value = ValueRangeTextBox.ConvertValueToDataType( value, valueRangeTextBox.ValueDataType );

      if( value.GetType() != type )
        throw new ArgumentException( "The value is not of type " + type.Name + ".", "NullValue" );

      return value;
    }

    private static void NullValuePropertyChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      if( e.OldValue == null )
      {
        if( valueRangeTextBox.Value == null )
          valueRangeTextBox.RefreshValue();
      }
      else
      {
        if( e.OldValue.Equals( valueRangeTextBox.Value ) )
          valueRangeTextBox.RefreshValue();
      }
    }

    #endregion NullValue Property

    #region Value Property

    public object Value
    {
      get
      {
        return ( object )GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register( "Value", typeof( object ), typeof( ValueRangeTextBox ),
      new FrameworkPropertyMetadata(
      null,
      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
      new PropertyChangedCallback( ValueRangeTextBox.ValuePropertyChangedCallback ),
      new CoerceValueCallback( ValueRangeTextBox.ValueCoerceValueCallback ) ) );

    private static object ValueCoerceValueCallback( object sender, object value )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      if( !valueRangeTextBox.IsInitialized )
        return DependencyProperty.UnsetValue;

      if( valueRangeTextBox.IsFinalizingInitialization )
        value = ValueRangeTextBox.ConvertValueToDataType( value, valueRangeTextBox.ValueDataType );

      if( !valueRangeTextBox.IsForcingValue )
        valueRangeTextBox.ValidateValue( value );

      return value;
    }

    private static void ValuePropertyChangedCallback( object sender, DependencyPropertyChangedEventArgs e )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      if( valueRangeTextBox.IsForcingValue )
        return;

      // The ValueChangedCallback can be raised even though both values are the same since the property
      // datatype is Object.
      if( object.Equals( e.NewValue, e.OldValue ) )
        return;

      valueRangeTextBox.IsInValueChanged = true;
      try
      {
        valueRangeTextBox.Text = valueRangeTextBox.GetTextFromValue( e.NewValue );
      }
      finally
      {
        valueRangeTextBox.IsInValueChanged = false;
      }
    }

    #endregion Value Property

    #region ValueDataType Property

    public Type ValueDataType
    {
      get
      {
        return ( Type )GetValue( ValueDataTypeProperty );
      }
      set
      {
        SetValue( ValueDataTypeProperty, value );
      }
    }

    public static readonly DependencyProperty ValueDataTypeProperty =
        DependencyProperty.Register( "ValueDataType", typeof( Type ), typeof( ValueRangeTextBox ),
      new UIPropertyMetadata(
      null,
      new PropertyChangedCallback( ValueRangeTextBox.ValueDataTypePropertyChangedCallback ),
      new CoerceValueCallback( ValueRangeTextBox.ValueDataTypeCoerceValueCallback ) ) );

    private static object ValueDataTypeCoerceValueCallback( DependencyObject sender, object value )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      if( !valueRangeTextBox.IsInitialized )
        return DependencyProperty.UnsetValue;

      Type valueDataType = value as Type;

      try
      {
        valueRangeTextBox.ValidateDataType( valueDataType );
      }
      catch( Exception exception )
      {
        throw new ArgumentException( "An error occured while trying to change the ValueDataType.", exception );
      }

      return value;
    }

    private static void ValueDataTypePropertyChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      Type valueDataType = e.NewValue as Type;

      valueRangeTextBox.IsNumericValueDataType = ValueRangeTextBox.IsNumericType( valueDataType );

      valueRangeTextBox.RefreshConversionHelpers();

      valueRangeTextBox.ConvertValuesToDataType( valueDataType );
    }

    internal virtual void ValidateDataType( Type type )
    {
      // Null will always be valid and will reset the MinValue, MaxValue, NullValue and Value to null.
      if( type == null )
        return;

      // We use InvariantCulture instead of the active format provider since the FormatProvider is only
      // used when the source type is String.  When we are converting from a string, we are
      // actually converting a value from XAML.  Therefore, if the string will have a period as a
      // decimal separator.  If we were using the active format provider, we could end up expecting a coma
      // as the decimal separator and the ChangeType method would throw.

      object minValue = this.MinValue;

      if( ( minValue != null ) && ( minValue.GetType() != type ) )
        minValue = System.Convert.ChangeType( minValue, type, CultureInfo.InvariantCulture );

      object maxValue = this.MaxValue;

      if( ( maxValue != null ) && ( maxValue.GetType() != type ) )
        maxValue = System.Convert.ChangeType( maxValue, type, CultureInfo.InvariantCulture );

      object nullValue = this.NullValue;

      if( ( ( nullValue != null ) && ( nullValue != DBNull.Value ) )
        && ( nullValue.GetType() != type ) )
      {
        nullValue = System.Convert.ChangeType( nullValue, type, CultureInfo.InvariantCulture );
      }

      object value = this.Value;

      if( ( ( value != null ) && ( value != DBNull.Value ) )
        && ( value.GetType() != type ) )
      {
        value = System.Convert.ChangeType( value, type, CultureInfo.InvariantCulture );
      }

      if( ( minValue != null ) || ( maxValue != null )
        || ( ( nullValue != null ) && ( nullValue != DBNull.Value ) ) )
      {
        // Value comparaisons will occur.  Therefore, the aspiring data type must implement IComparable.

        Type iComparable = type.GetInterface( "IComparable" );

        if( iComparable == null )
          throw new InvalidOperationException( "MinValue, MaxValue, and NullValue must implement the IComparable interface." );
      }
    }

    private void ConvertValuesToDataType( Type type )
    {
      if( type == null )
      {
        this.MinValue = null;
        this.MaxValue = null;
        this.NullValue = null;

        this.Value = null;

        return;
      }

      object minValue = this.MinValue;

      if( ( minValue != null ) && ( minValue.GetType() != type ) )
        this.MinValue = ValueRangeTextBox.ConvertValueToDataType( minValue, type );

      object maxValue = this.MaxValue;

      if( ( maxValue != null ) && ( maxValue.GetType() != type ) )
        this.MaxValue = ValueRangeTextBox.ConvertValueToDataType( maxValue, type );

      object nullValue = this.NullValue;

      if( ( ( nullValue != null ) && ( nullValue != DBNull.Value ) )
        && ( nullValue.GetType() != type ) )
      {
        this.NullValue = ValueRangeTextBox.ConvertValueToDataType( nullValue, type );
      }

      object value = this.Value;

      if( ( ( value != null ) && ( value != DBNull.Value ) )
        && ( value.GetType() != type ) )
      {
        this.Value = ValueRangeTextBox.ConvertValueToDataType( value, type );
      }
    }

    #endregion ValueDataType Property

    #region Text Property

    private static object TextCoerceValueCallback( object sender, object value )
    {
      ValueRangeTextBox valueRangeTextBox = sender as ValueRangeTextBox;

      if( !valueRangeTextBox.IsInitialized )
        return DependencyProperty.UnsetValue;

      if( value == null )
        return string.Empty;

      return value;
    }

    protected override void OnTextChanged( TextChangedEventArgs e )
    {
      // If in IME Composition, RefreshValue already returns without doing anything.
      this.RefreshValue();

      base.OnTextChanged( e );
    }

    #endregion Text Property

    #region HasValidationError Property

    private static readonly DependencyPropertyKey HasValidationErrorPropertyKey =
        DependencyProperty.RegisterReadOnly( "HasValidationError", typeof( bool ), typeof( ValueRangeTextBox ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty HasValidationErrorProperty = ValueRangeTextBox.HasValidationErrorPropertyKey.DependencyProperty;

    public bool HasValidationError
    {
      get
      {
        return ( bool )this.GetValue( ValueRangeTextBox.HasValidationErrorProperty );
      }
    }

    private void SetHasValidationError( bool value )
    {
      this.SetValue( ValueRangeTextBox.HasValidationErrorPropertyKey, value );
    }

    #endregion HasValidationError Property

    #region HasParsingError Property

    private static readonly DependencyPropertyKey HasParsingErrorPropertyKey =
        DependencyProperty.RegisterReadOnly( "HasParsingError", typeof( bool ), typeof( ValueRangeTextBox ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty HasParsingErrorProperty = ValueRangeTextBox.HasParsingErrorPropertyKey.DependencyProperty;

    public bool HasParsingError
    {
      get
      {
        return ( bool )this.GetValue( ValueRangeTextBox.HasParsingErrorProperty );
      }
    }

    internal void SetHasParsingError( bool value )
    {
      this.SetValue( ValueRangeTextBox.HasParsingErrorPropertyKey, value );
    }

    #endregion HasParsingError Property

    #region IsValueOutOfRange Property

    private static readonly DependencyPropertyKey IsValueOutOfRangePropertyKey =
        DependencyProperty.RegisterReadOnly( "IsValueOutOfRange", typeof( bool ), typeof( ValueRangeTextBox ), new UIPropertyMetadata( false ) );

    public static readonly DependencyProperty IsValueOutOfRangeProperty = ValueRangeTextBox.IsValueOutOfRangePropertyKey.DependencyProperty;

    public bool IsValueOutOfRange
    {
      get
      {
        return ( bool )this.GetValue( ValueRangeTextBox.IsValueOutOfRangeProperty );
      }
    }

    private void SetIsValueOutOfRange( bool value )
    {
      this.SetValue( ValueRangeTextBox.IsValueOutOfRangePropertyKey, value );
    }

    #endregion IsValueOutOfRange Property

    #region IsInValueChanged property

    internal bool IsInValueChanged
    {
      get
      {
        return m_flags[ ( int )ValueRangeTextBoxFlags.IsInValueChanged ];
      }
      private set
      {
        m_flags[ ( int )ValueRangeTextBoxFlags.IsInValueChanged ] = value;
      }
    }

    #endregion

    #region IsForcingValue property

    internal bool IsForcingValue
    {
      get
      {
        return m_flags[ ( int )ValueRangeTextBoxFlags.IsForcingValue ];
      }
      private set
      {
        m_flags[ ( int )ValueRangeTextBoxFlags.IsForcingValue ] = value;
      }
    }

    #endregion

    #region IsForcingText property

    internal bool IsForcingText
    {
      get
      {
        return m_flags[ ( int )ValueRangeTextBoxFlags.IsForcingText ];
      }
      private set
      {
        m_flags[ ( int )ValueRangeTextBoxFlags.IsForcingText ] = value;
      }
    }

    #endregion

    #region IsNumericValueDataType property

    internal bool IsNumericValueDataType
    {
      get
      {
        return m_flags[ ( int )ValueRangeTextBoxFlags.IsNumericValueDataType ];
      }
      private set
      {
        m_flags[ ( int )ValueRangeTextBoxFlags.IsNumericValueDataType ] = value;
      }
    }

    #endregion

    #region IsTextReadyToBeParsed property

    internal virtual bool IsTextReadyToBeParsed
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region IsInIMEComposition property

    internal bool IsInIMEComposition
    {
      get
      {
        return m_imePreCompositionCachedTextInfo != null;
      }
    }

    #endregion

    #region IsFinalizingInitialization Property

    private bool IsFinalizingInitialization
    {
      get
      {
        return m_flags[ ( int )ValueRangeTextBoxFlags.IsFinalizingInitialization ];
      }
      set
      {
        m_flags[ ( int )ValueRangeTextBoxFlags.IsFinalizingInitialization ] = value;
      }
    }

    #endregion

    #region TEXT FROM VALUE

    public event EventHandler<QueryTextFromValueEventArgs> QueryTextFromValue;

    internal string GetTextFromValue( object value )
    {
      string text = this.QueryTextFromValueCore( value );

      QueryTextFromValueEventArgs e = new QueryTextFromValueEventArgs( value, text );

      this.OnQueryTextFromValue( e );

      return e.Text;
    }

    protected virtual string QueryTextFromValueCore( object value )
    {
      if( ( value == null ) || ( value == DBNull.Value ) )
        return string.Empty;

      IFormatProvider formatProvider = this.GetActiveFormatProvider();

      CultureInfo cultureInfo = formatProvider as CultureInfo;

      if( cultureInfo != null )
      {
        TypeConverter converter = TypeDescriptor.GetConverter( value.GetType() );

        if( converter.CanConvertTo( typeof( string ) ) )
          return ( string )converter.ConvertTo( null, cultureInfo, value, typeof( string ) );
      }

      try
      {
        string result = System.Convert.ToString( value, formatProvider );

        return result;
      }
      catch
      {
      }

      return value.ToString();
    }

    private void OnQueryTextFromValue( QueryTextFromValueEventArgs e )
    {
      if( this.QueryTextFromValue != null )
        this.QueryTextFromValue( this, e );
    }

    #endregion TEXT FROM VALUE

    #region VALUE FROM TEXT

    public event EventHandler<QueryValueFromTextEventArgs> QueryValueFromText;

    internal object GetValueFromText( string text, out bool hasParsingError )
    {
      object value = null;
      bool success = this.QueryValueFromTextCore( text, out value );

      QueryValueFromTextEventArgs e = new QueryValueFromTextEventArgs( text, value );
      e.HasParsingError = !success;

      this.OnQueryValueFromText( e );

      hasParsingError = e.HasParsingError;

      return e.Value;
    }

    protected virtual bool QueryValueFromTextCore( string text, out object value )
    {
      value = null;

      Type validatingType = this.ValueDataType;

      text = text.Trim();

      if( validatingType == null )
        return true;

      if( !validatingType.IsValueType && ( validatingType != typeof( string ) ) )
        return false;

      try
      {
        value = ChangeTypeHelper.ChangeType( text, validatingType, this.GetActiveFormatProvider() );
      }
      catch
      {
        if( this.BeepOnError )
        {
          System.Media.SystemSounds.Beep.Play();
        }
        return false;
      }

      return true;
    }

    private void OnQueryValueFromText( QueryValueFromTextEventArgs e )
    {
      if( this.QueryValueFromText != null )
        this.QueryValueFromText( this, e );
    }

    #endregion VALUE FROM TEXT

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      if( ( e.ImeProcessedKey != Key.None ) && ( !this.IsInIMEComposition ) )
      {
        // Start of an IME Composition.  Cache all the critical infos.
        this.StartIMEComposition();
      }

      base.OnPreviewKeyDown( e );
    }

    protected override void OnGotFocus( RoutedEventArgs e )
    {
      base.OnGotFocus( e );

      this.RefreshCurrentText( true );
    }

    protected override void OnLostFocus( RoutedEventArgs e )
    {
      base.OnLostFocus( e );

      this.RefreshCurrentText( true );
    }

    protected override void OnTextInput( TextCompositionEventArgs e )
    {
      if( this.IsInIMEComposition )
        this.EndIMEComposition();

      base.OnTextInput( e );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly" )]
    protected virtual void ValidateValue( object value )
    {
      if( value == null )
        return;

      Type validatingType = this.ValueDataType;
      if( validatingType.IsGenericType && validatingType.GetGenericTypeDefinition().Equals( typeof( Nullable<> ) ) )
      {
        NullableConverter nullableConverter = new NullableConverter( validatingType );
        validatingType = nullableConverter.UnderlyingType;
      }

      if( validatingType == null )
        throw new InvalidOperationException( "An attempt was made to set a value when the ValueDataType property is null." );

      if( ( value != DBNull.Value ) && ( value.GetType() != validatingType ) )
        throw new ArgumentException( "The value is not of type " + validatingType.Name + ".", "Value" );

      this.ValidateValueInRange( this.MinValue, this.MaxValue, value );
    }

    internal static bool IsNumericType( Type type )
    {
      if( type == null )
        return false;

      if( type.IsValueType )
      {
        if( ( type == typeof( int ) ) || ( type == typeof( double ) ) || ( type == typeof( decimal ) )
          || ( type == typeof( float ) ) || ( type == typeof( short ) ) || ( type == typeof( long ) )
          || ( type == typeof( ushort ) ) || ( type == typeof( uint ) ) || ( type == typeof( ulong ) )
          || ( type == typeof( byte ) )
          )
        {
          return true;
        }
      }

      return false;
    }

    internal void StartIMEComposition()
    {
      Debug.Assert( m_imePreCompositionCachedTextInfo == null, "EndIMEComposition should have been called before another IME Composition starts." );

      m_imePreCompositionCachedTextInfo = new CachedTextInfo( this );
    }

    internal void EndIMEComposition()
    {
      CachedTextInfo cachedTextInfo = m_imePreCompositionCachedTextInfo.Clone() as CachedTextInfo;
      m_imePreCompositionCachedTextInfo = null;

      this.OnIMECompositionEnded( cachedTextInfo );
    }

    internal virtual void OnIMECompositionEnded( CachedTextInfo cachedTextInfo )
    {
    }

    internal virtual void RefreshConversionHelpers()
    {
    }

    internal IFormatProvider GetActiveFormatProvider()
    {
      IFormatProvider formatProvider = this.FormatProvider;

      if( formatProvider != null )
        return formatProvider;

      return CultureInfo.CurrentCulture;
    }

    internal CultureInfo GetCultureInfo()
    {
      CultureInfo cultureInfo = this.GetActiveFormatProvider() as CultureInfo;

      if( cultureInfo != null )
        return cultureInfo;

      return CultureInfo.CurrentCulture;
    }

    internal virtual string GetCurrentText()
    {
      return this.Text;
    }

    internal virtual string GetParsableText()
    {
      return this.Text;
    }

    internal void ForceText( string text, bool preserveCaret )
    {
      this.IsForcingText = true;
      try
      {
        int oldCaretIndex = this.CaretIndex;

        this.Text = text;

        if( ( preserveCaret ) && ( this.IsLoaded ) )
        {
          try
          {
            this.SelectionStart = oldCaretIndex;
          }
          catch( NullReferenceException )
          {
          }
        }
      }
      finally
      {
        this.IsForcingText = false;
      }
    }

    internal bool IsValueNull( object value )
    {
      if( ( value == null ) || ( value == DBNull.Value ) )
        return true;

      Type type = this.ValueDataType;
      if( type.IsGenericType && type.GetGenericTypeDefinition().Equals( typeof( Nullable<> ) ) )
      {
        NullableConverter nullableConverter = new NullableConverter( type );
        type = nullableConverter.UnderlyingType;
      }

      if( value.GetType() != type )
        value = System.Convert.ChangeType( value, type );

      object nullValue = this.NullValue;

      if( nullValue == null )
        return false;

      if( nullValue.GetType() != type )
        nullValue = System.Convert.ChangeType( nullValue, type );

      return nullValue.Equals( value );
    }

    internal void ForceValue( object value )
    {
      this.IsForcingValue = true;
      try
      {
        this.Value = value;
      }
      finally
      {
        this.IsForcingValue = false;
      }
    }

    internal void RefreshCurrentText( bool preserveCurrentCaretPosition )
    {
      string displayText = this.GetCurrentText();

      if( !string.Equals( displayText, this.Text ) )
        this.ForceText( displayText, preserveCurrentCaretPosition );
    }

    internal void RefreshValue()
    {
      if( ( this.IsForcingValue ) || ( this.ValueDataType == null ) || ( this.IsInIMEComposition ) )
        return;

      object value;
      bool hasParsingError;

      if( this.IsTextReadyToBeParsed )
      {
        string parsableText = this.GetParsableText();

        value = this.GetValueFromText( parsableText, out hasParsingError );

        if( this.IsValueNull( value ) )
          value = this.NullValue;
      }
      else
      {
        // We don't consider empty text as a parsing error.
        hasParsingError = !this.GetIsEditTextEmpty();
        value = this.NullValue;
      }

      this.SetHasParsingError( hasParsingError );

      bool hasValidationError = hasParsingError;
      try
      {
        this.ValidateValue( value );

        this.SetIsValueOutOfRange( false );
      }
      catch( Exception exception )
      {
        hasValidationError = true;

        if( this.BeepOnError )
        {
          System.Media.SystemSounds.Beep.Play();
        }

        if( exception is ArgumentOutOfRangeException )
          this.SetIsValueOutOfRange( true );

        value = this.NullValue;
      }

      if( !object.Equals( value, this.Value ) )
        this.ForceValue( value );

      this.SetHasValidationError( hasValidationError );
    }

    internal virtual bool GetIsEditTextEmpty()
    {
      return this.Text == string.Empty;
    }

    private static object ConvertValueToDataType( object value, Type type )
    {
      // We use InvariantCulture instead of the active format provider since the FormatProvider is only
      // used when the source type is String.  When we are converting from a string, we are
      // actually converting a value from XAML.  Therefore, if the string will have a period as a
      // decimal separator.  If we were using the active format provider, we could end up expecting a coma
      // as the decimal separator and the ChangeType method would throw.
      if( type == null )
        return null;

      if( ( ( value != null ) && ( value != DBNull.Value ) )
        && ( value.GetType() != type ) )
      {
        return ChangeTypeHelper.ChangeType( value, type, CultureInfo.InvariantCulture );
      }

      return value;
    }

    private void CanEnterLineBreak( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;
      e.Handled = true;
    }

    private void CanEnterParagraphBreak( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = false;
      e.Handled = true;
    }

    private void ValidateValueInRange( object minValue, object maxValue, object value )
    {
      if( this.IsValueNull( value ) )
        return;

      Type type = this.ValueDataType;
      if( type.IsGenericType && type.GetGenericTypeDefinition().Equals( typeof( Nullable<> ) ) )
      {
        NullableConverter nullableConverter = new NullableConverter( type );
        type = nullableConverter.UnderlyingType;
      }

      if( value.GetType() != type )
        value = System.Convert.ChangeType( value, type );

      // Validate the value against the range.
      if( minValue != null )
      {
        IComparable minValueComparable = ( IComparable )minValue;

        if( ( maxValue != null ) && ( minValueComparable.CompareTo( maxValue ) > 0 ) )
          throw new ArgumentOutOfRangeException( "minValue", "MaxValue must be greater than MinValue." );

        if( minValueComparable.CompareTo( value ) > 0 )
          throw new ArgumentOutOfRangeException( "minValue", "Value must be greater than MinValue." );
      }

      if( maxValue != null )
      {
        IComparable maxValueComparable = ( IComparable )maxValue;

        if( maxValueComparable.CompareTo( value ) < 0 )
          throw new ArgumentOutOfRangeException( "maxValue", "Value must be less than MaxValue." );
      }
    }

    #region ISupportInitialize

    protected override void OnInitialized( EventArgs e )
    {
      this.IsFinalizingInitialization = true;
      try
      {
        this.CoerceValue( ValueRangeTextBox.ValueDataTypeProperty );

        this.IsNumericValueDataType = ValueRangeTextBox.IsNumericType( this.ValueDataType );
        this.RefreshConversionHelpers();

        this.CoerceValue( ValueRangeTextBox.MinValueProperty );
        this.CoerceValue( ValueRangeTextBox.MaxValueProperty );

        this.CoerceValue( ValueRangeTextBox.ValueProperty );

        this.CoerceValue( ValueRangeTextBox.NullValueProperty );

        this.CoerceValue( ValueRangeTextBox.TextProperty );
      }
      catch( Exception exception )
      {
        throw new InvalidOperationException( "Initialization of the ValueRangeTextBox failed.", exception );
      }
      finally
      {
        this.IsFinalizingInitialization = false;
      }

      base.OnInitialized( e );
    }

    #endregion ISupportInitialize

    private BitVector32 m_flags;
    private CachedTextInfo m_imePreCompositionCachedTextInfo;

    [Flags]
    private enum ValueRangeTextBoxFlags
    {
      IsFinalizingInitialization = 1,
      IsForcingText = 2,
      IsForcingValue = 4,
      IsInValueChanged = 8,
      IsNumericValueDataType = 16
    }
  }
}
