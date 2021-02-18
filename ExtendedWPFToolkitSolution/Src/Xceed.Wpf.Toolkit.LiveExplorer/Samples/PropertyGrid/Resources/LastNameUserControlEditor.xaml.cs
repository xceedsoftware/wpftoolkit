/************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Toolkit for WPF also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid
{
  /// <summary>
  /// Interaction logic for LastNameUserControlEditor.xaml
  /// </summary>
  public partial class LastNameUserControlEditor : UserControl, ITypeEditor
  {
    public LastNameUserControlEditor()
    {
      InitializeComponent();
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( string ), typeof( LastNameUserControlEditor ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );
    public string Value
    {
      get
      {
        return ( string )GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private void Button_Click( object sender, RoutedEventArgs e )
    {
      Value = string.Empty;
    }

    public FrameworkElement ResolveEditor( Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem )
    {
      Binding binding = new Binding( "Value" );
      binding.Source = propertyItem;
      binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
      BindingOperations.SetBinding( this, LastNameUserControlEditor.ValueProperty, binding );
      return this;
    }
  }
}
