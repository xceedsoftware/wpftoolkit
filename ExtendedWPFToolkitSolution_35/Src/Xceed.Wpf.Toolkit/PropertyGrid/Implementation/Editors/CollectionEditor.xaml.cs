/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  /// <summary>
  /// Interaction logic for CollectionEditor.xaml
  /// </summary>
  public partial class CollectionEditor : UserControl, ITypeEditor
  {
    PropertyItem _item;

    public CollectionEditor()
    {
      InitializeComponent();
    }

    private void Button_Click( object sender, RoutedEventArgs e )
    {
      CollectionControlDialog editor = new CollectionControlDialog( _item.PropertyType, _item.DescriptorDefinition.NewItemTypes );
      Binding binding = new Binding( "Value" );
      binding.Source = _item;
      binding.Mode = _item.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
      BindingOperations.SetBinding( editor, CollectionControlDialog.ItemsSourceProperty, binding );
      editor.ShowDialog();
    }

    public FrameworkElement ResolveEditor( PropertyItem propertyItem )
    {
      _item = propertyItem;
      return this;
    }
  }
}
