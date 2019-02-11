/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid
{
  public class GroupLevelDescription : DependencyObject, IGroupLevelDescription, INotifyPropertyChanged
  {
    #region CONSTRUCTORS

    internal GroupLevelDescription( GroupDescription groupDescription, string fieldName )
    {
      if( groupDescription == null )
        throw new DataGridInternalException( "GroupDescription cannot be null." );

      m_groupDescription = groupDescription;
      m_fieldName = fieldName;
    }

    #endregion

    #region FieldName Property

    public string FieldName
    {
      get
      {
        return m_fieldName;
      }
    }

    private readonly string m_fieldName;

    #endregion

    #region GroupDescription Property

    internal GroupDescription GroupDescription
    {
      get
      {
        return m_groupDescription;
      }
    }

    private readonly GroupDescription m_groupDescription;

    #endregion

    #region Title Property

    internal static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
      "Title",
      typeof( object ),
      typeof( GroupLevelDescription ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( GroupLevelDescription.OnPropertyChanged ) ) );

    public object Title
    {
      get
      {
        return ( object )this.GetValue( GroupLevelDescription.TitleProperty );
      }
    }

    internal void SetTitle( object title )
    {
      this.SetValue( GroupLevelDescription.TitleProperty, title );
    }

    #endregion Title Property

    #region TitleTemplate Property

    internal static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
      "TitleTemplate",
      typeof( DataTemplate ),
      typeof( GroupLevelDescription ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( GroupLevelDescription.OnPropertyChanged ) ) );

    public DataTemplate TitleTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( GroupLevelDescription.TitleTemplateProperty );
      }
    }

    #endregion TitleTemplate Property

    #region TitleTemplateSelector Property

    internal static readonly DependencyProperty TitleTemplateSelectorProperty = DependencyProperty.Register(
      "TitleTemplateSelector",
      typeof( DataTemplateSelector ),
      typeof( GroupLevelDescription ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( GroupLevelDescription.OnPropertyChanged ) ) );

    public DataTemplateSelector TitleTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( GroupLevelDescription.TitleTemplateSelectorProperty );
      }
    }

    #endregion TitleTemplateSelector Property

    #region ValueStringFormat Property

    internal static readonly DependencyProperty ValueStringFormatProperty = DependencyProperty.Register(
      "ValueStringFormat",
      typeof( string ),
      typeof( GroupLevelDescription ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( GroupLevelDescription.OnPropertyChanged ) ) );

    public string ValueStringFormat
    {
      get
      {
        return ( string )this.GetValue( GroupLevelDescription.ValueStringFormatProperty );
      }
    }

    #endregion ValueStringFormat Property

    #region ValueStringFormatCulture Property

    internal static readonly DependencyProperty ValueStringFormatCultureProperty = DependencyProperty.Register(
      "ValueStringFormatCulture",
      typeof( CultureInfo ),
      typeof( GroupLevelDescription ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( GroupLevelDescription.OnPropertyChanged ) ) );

    public CultureInfo ValueStringFormatCulture
    {
      get
      {
        return ( CultureInfo )this.GetValue( GroupLevelDescription.ValueStringFormatCultureProperty );
      }
    }

    #endregion ValueStringFormatCulture Property

    #region ValueTemplate Property

    internal static readonly DependencyProperty ValueTemplateProperty = DependencyProperty.Register(
      "ValueTemplate",
      typeof( DataTemplate ),
      typeof( GroupLevelDescription ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( GroupLevelDescription.OnPropertyChanged ) ) );

    public DataTemplate ValueTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( GroupLevelDescription.ValueTemplateProperty );
      }
    }

    #endregion ValueTemplate Property

    #region ValueTemplateSelector Property

    internal static readonly DependencyProperty ValueTemplateSelectorProperty = DependencyProperty.Register(
      "ValueTemplateSelector",
      typeof( DataTemplateSelector ),
      typeof( GroupLevelDescription ),
      new UIPropertyMetadata( null, new PropertyChangedCallback( GroupLevelDescription.OnPropertyChanged ) ) );

    public DataTemplateSelector ValueTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( GroupLevelDescription.ValueTemplateSelectorProperty );
      }
    }

    #endregion ValueTemplateSelector Property

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private static void OnPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = ( GroupLevelDescription )sender;
      var handler = self.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( self, new PropertyChangedEventArgs( e.Property.Name ) );
    }

    #endregion
  }
}
