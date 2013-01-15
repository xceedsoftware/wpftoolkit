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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  public class GroupLevelDescription : DependencyObject, INotifyPropertyChanged
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

    #endregion

    #region GroupDescription Property

    internal GroupDescription GroupDescription
    {
      get
      {
        return m_groupDescription;
      }
    }

    #endregion

    #region Title Property

    internal static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register( "Title", typeof( object ), typeof( GroupLevelDescription ), new UIPropertyMetadata( null, new PropertyChangedCallback( GroupPropertiesChangedHandler ) ) );

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

    internal static readonly DependencyProperty TitleTemplateProperty =
        DependencyProperty.Register( "TitleTemplate", typeof( DataTemplate ), typeof( GroupLevelDescription ), new UIPropertyMetadata( null, new PropertyChangedCallback( GroupPropertiesChangedHandler ) ) );

    public DataTemplate TitleTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( GroupLevelDescription.TitleTemplateProperty );
      }
    }

    internal void SetTitleTemplate( DataTemplate titleTemplate )
    {
      this.SetValue( GroupLevelDescription.TitleTemplateProperty, titleTemplate );
    }

    #endregion TitleTemplate Property

    #region TitleTemplateSelector Property

    internal static readonly DependencyProperty TitleTemplateSelectorProperty =
        DependencyProperty.Register( "TitleTemplateSelector", typeof( DataTemplateSelector ), typeof( GroupLevelDescription ), new UIPropertyMetadata( null, new PropertyChangedCallback( GroupPropertiesChangedHandler ) ) );

    public DataTemplateSelector TitleTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( GroupLevelDescription.TitleTemplateSelectorProperty );
      }
    }

    internal void SetTitleTemplateSelector( DataTemplateSelector titleTemplateSelector )
    {
      this.SetValue( GroupLevelDescription.TitleTemplateSelectorProperty, titleTemplateSelector );
    }

    #endregion TitleTemplateSelector Property

    #region ValueTemplate Property

    public static readonly DependencyProperty ValueTemplateProperty =
        DependencyProperty.Register( "ValueTemplate", typeof( DataTemplate ), typeof( GroupLevelDescription ), new UIPropertyMetadata( null, new PropertyChangedCallback( GroupPropertiesChangedHandler ) ) );

    public DataTemplate ValueTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( GroupLevelDescription.ValueTemplateProperty );
      }
      set
      {
        this.SetValue( GroupLevelDescription.ValueTemplateProperty, value );
      }
    }

    #endregion ValueTemplate Property

    #region ValueTemplateSelector Property

    public static readonly DependencyProperty ValueTemplateSelectorProperty =
        DependencyProperty.Register( "ValueTemplateSelector", typeof( DataTemplateSelector ), typeof( GroupLevelDescription ), new UIPropertyMetadata( null, new PropertyChangedCallback( GroupPropertiesChangedHandler ) ) );

    public DataTemplateSelector ValueTemplateSelector
    {
      get
      {
        return ( DataTemplateSelector )this.GetValue( GroupLevelDescription.ValueTemplateSelectorProperty );
      }
      set
      {
        this.SetValue( GroupLevelDescription.ValueTemplateSelectorProperty, value );
      }
    }

    #endregion ValueTemplateSelector Property

    #region PRIVATE METHODS

    private static void GroupPropertiesChangedHandler( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      GroupLevelDescription info = ( GroupLevelDescription )sender;

      if( info.PropertyChanged != null )
      {
        info.PropertyChanged( info, new PropertyChangedEventArgs( e.Property.Name ) );
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region PRIVATE FIELDS

    private string m_fieldName;
    private GroupDescription m_groupDescription;

    #endregion
  }
}
