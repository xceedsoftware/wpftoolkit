/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;

namespace Samples.Modules.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for BindingToStructs.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class BindingToStructs : DemoView
  {
    public BindingToStructs()
    {
      InitializeComponent();
      _propertyGrid.SelectedObject = Person.CreatePerson();
    }

    public class Person
    {
      [Category( "Information" )]
      [DisplayName( "First Name" )]
      [Description( "This property uses a TextBox as the default editor." )]
      public string FirstName
      {
        get;
        set;
      }

      [Category( "Information" )]
      [DisplayName( "Last Name" )]
      [Description( "This property uses a TextBox as the default editor." )]
      public string LastName
      {
        get;
        set;
      }

      public Dimension Dimensions
      {
        get;
        set;
      }

      public static Person CreatePerson()
      {
        var person = new Person();
        person.FirstName = "John";
        person.LastName = "Doe";
        person.Dimensions = new Dimension()
        {
          Height = 75.0,
          Weight = 185.76
        };
        return person;
      }
    }
  }

  public struct Dimension
  {
    public double Height;
    public double Weight;

    public Dimension( double height, double weight )
    {
      this.Height = height;
      this.Weight = weight;
    }
  }

  public class DimensionsConverter : IValueConverter
  {
    static Dimension _originalValue; // the static struct that stores original value at the start of editing

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      _originalValue = ( ( Dimension )value );

      if( parameter.ToString() == "Height" )
        return ( ( Dimension )value ).Height;
      if( parameter.ToString() == "Weight" )
        return ( ( Dimension )value ).Weight;

      return _originalValue;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( parameter.ToString() == "Height" )
        _originalValue = new Dimension( double.Parse( value.ToString() ), _originalValue.Weight );
      if( parameter.ToString() == "Weight" )
        _originalValue = new Dimension( _originalValue.Height, double.Parse( value.ToString() ) );

      return _originalValue;
    }
  }
}
