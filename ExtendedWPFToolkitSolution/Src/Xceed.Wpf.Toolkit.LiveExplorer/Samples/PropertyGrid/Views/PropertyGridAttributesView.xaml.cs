/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridEditorAttributeView.xaml
  /// </summary>
  public partial class PropertyGridAttributesView : DemoView
  {
    public PropertyGridAttributesView()
    {
      InitializeComponent();

      var person = new Man();
      person.FirstName = "John";
      person.LastName = "Doe";
      person.WritingFontSize = 12;
      person.RangeDouble = 5d;
      person.Friends = new List<Person>() { new Man() { FirstName = "First", LastName = "Friend", RangeDouble = 5d }, new Woman() { FirstName = "Second", LastName = "Friend", RangeDouble = 5d } };
      person.Spouse = new Woman()
      {
        FirstName = "Jane",
        LastName = "Doe",
        RangeDouble = 5d
      };

      this.DataContext = person;
    }
  }

  [CategoryOrder("Information", 0)]
  [CategoryOrder( "Conections", 1 )]
  [CategoryOrder( "Other", 2 )]
  public abstract class Person : INotifyPropertyChanged
  {
    // All properties have their own "[Category(...)]" attribute to specify which category they
    // belong to when the "Categorized" layout is used by the PropertyGrid.

    [Category( "Information" )]
    [Description( "This property uses the [DisplayName(\"Is a Men\")] attribute to customize the name of this property." )]
    [DisplayName( "Is male" )]
    public bool IsMale { get; set; }

    [Category( "Information" )]
    [Description( "This property uses the [Editor(..)] attribute to provide a custom editor using the 'FirstNameEditor' class. In the Plus version, it also depends on the IsMale property to change its foreground and source." )]
    [Editor( typeof( FirstNameEditor ), typeof( FirstNameEditor ) )]
    public string FirstName
    {
      get{ return _firstName; }
      set{ _firstName = value; OnPropertyChanged( "FirstName" );}
    }
    private string _firstName;

    [Category( "Information" )]
    [Description( "This property uses the [Editor(..)] attribute to provide a custom editor using the 'LastNameUserControlEditor' user control." )]
    [Editor( typeof( LastNameUserControlEditor ), typeof( LastNameUserControlEditor ) )]
    [DefaultValue("Friend")]
    public string LastName
    {
      get{ return _lastName; }
      set{ _lastName = value; OnPropertyChanged( "LastName" );}
    }
    private string _lastName;

    [Category( "Conections" )]
    [Description( "This property uses the [NewItemTypes(...)] attribute to provide the underlying CollectionEditor with class types (eg. Man, Woman) that can be inserted in the collection." )]
    [NewItemTypes( typeof( Man ), typeof( Woman ) )]
    public List<Person> Friends { get; set; }

    [Category( "Information" )]
    [DisplayName( "Writing Font Size" )]
    [Description( "This property defines the [ItemsSource(..)] attribute that allows you to specify a ComboBox editor and control its items." )]
    [ItemsSource( typeof( FontSizeItemsSource ) )]
    [RefreshProperties( RefreshProperties.All )]    //This will reload the PropertyGrid
    public double WritingFontSize { get; set; }

    [Category( "Conections" )]
    [Description( "This property defines the [ExpandableObject()] attribute. This allows you to expand this property and drill down through its values." )]
    [ExpandableObject()]
    public Person Spouse { get; set; }

    [Category( "Other" )]
    [Description( "This property uses the [PropertyOrder(1)] attribute to control its position in the categorized and non-categorized views. Otherwise, alphabetical order is used." )]
    [PropertyOrder( 1 )]
    public string A_SecondProperty { get; set; }

    [Category( "Other" )]
    [Description( "This property uses the [PropertyOrder(0)] attribute to control its position in the categorized and non-categorized view. Otherwise, alphabetical order is used." )]
    [PropertyOrder( 0 )]
    public string B_FirstProperty { get; set; }

    [Category( "Other" )]
    [Description( "This property uses the [ParenthesizePropertyName()] attribute to force the name to be displayed within round brackets." )]
    [ParenthesizePropertyNameAttribute(true)]
    public string NameInParentheses { get; set; }

    [Category( "Other" )]
    [Description( "This property uses the [Browsable(false)] attribute to not display the property" )]
    [BrowsableAttribute( false )]
    public string InvisibleProperty
    {
      get;
      set;
    }

    [Range(0d, 10d)]
    [Category( "Other" )]
    [DefaultValue(5d)]
    [Description( "This property uses the [Range(0,10)] and DefaultValue attributes to set the Minimum, Maximum and default properties." )]
    public double RangeDouble
    {
      get;
      set;
    }

    public override string ToString()
    {
      return FirstName + " " + LastName;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged( string propertyName )
    {
      if( PropertyChanged != null )
      {
        PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
      }
    }
  }

  // The "Man" and "Woman" classes are referenced by the "NewItemTypesAttribute"
  // of the "Friends" property.
  //
  // Theses are the types that can be instantiated in the CollectionEditor
  // of the property.
  public class Man : Person
  {
    [Display( Name = "Favorite Sport"
            , Prompt = "Enter your favorite sport"
            , Description = "This property uses the Display attribute to set different PropertyItem fields."
            , GroupName = "Information"
            , AutoGenerateField = true
            , AutoGenerateFilter = false )]
    public string FavoriteSport { get; set; }
    public Man() { this.IsMale = true; }
  }
  public class Woman : Person
  {
    [Category( "Information" )]
    [Description( "This property has no special attribute besides [Categroy(\"Information\")] and [Description(...)]" )]
    public string FavoriteRestaurant { get; set; }
    public Woman() { this.IsMale = false; }
  }

  // This is the custom editor referenced by the "EditorAttribute"
  // of the "FirstName" property.
  //
  // This class must implement the
  //   Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor 
  // interface
  public class FirstNameEditor : Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
  {
    public FrameworkElement ResolveEditor( Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem )
    {
      TextBox textBox = new TextBox();
      textBox.Background = new SolidColorBrush( Colors.Red );
      //create the binding from the bound property item to the editor
      var _binding = new Binding( "Value" ); //bind to the Value property of the PropertyItem
      _binding.Source = propertyItem;
      _binding.ValidatesOnExceptions = true;
      _binding.ValidatesOnDataErrors = true;
      _binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
      BindingOperations.SetBinding( textBox, TextBox.TextProperty, _binding );
      return textBox;
    }
  }

  // This is the ItemsSource provider referenced by the "ItemsSourceAttribute"
  // of the "WritingFontSize" property.
  //
  // This class must implement the
  //   Xceed.Wpf.Toolkit.PropertyGrid.Attributes.IItemsSource 
  // interface
  public class FontSizeItemsSource : IItemsSource
  {
    public Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection GetValues()
    {
      var sizes = new Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ItemCollection();
      sizes.Add( 5.0, "Five" );
      sizes.Add( 5.5 );
      sizes.Add( 6.0, "Six" );
      sizes.Add( 6.5 );
      sizes.Add( 7.0, "Seven" );
      sizes.Add( 7.5 );
      sizes.Add( 8.0, "Eight" );
      sizes.Add( 8.5 );
      sizes.Add( 9.0, "Nine" );
      sizes.Add( 9.5 );
      sizes.Add( 10.0 );
      sizes.Add( 12.0, "Twelve" );
      sizes.Add( 14.0 );
      sizes.Add( 16.0 );
      sizes.Add( 18.0 );
      sizes.Add( 20.0 );
      return sizes;
    }
  }
}
