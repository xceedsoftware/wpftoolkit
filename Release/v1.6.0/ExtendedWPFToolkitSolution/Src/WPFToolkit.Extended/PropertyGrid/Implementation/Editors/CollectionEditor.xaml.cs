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
      CollectionEditorDialog editor = new CollectionEditorDialog( _item.PropertyType );
      Binding binding = new Binding( "Value" );
      binding.Source = _item;
      binding.Mode = _item.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
      BindingOperations.SetBinding( editor, CollectionEditorDialog.ItemsSourceProperty, binding );
      editor.ShowDialog();
    }

    public FrameworkElement ResolveEditor( PropertyItem propertyItem )
    {
      _item = propertyItem;
      return this;
    }
  }
}
