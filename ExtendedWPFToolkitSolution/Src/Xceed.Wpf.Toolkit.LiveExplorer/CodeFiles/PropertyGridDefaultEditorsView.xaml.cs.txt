/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views
{
  /// <summary>
  /// Interaction logic for PropertyGridDefaultEditorsView.xaml
  /// </summary>
  public partial class PropertyGridDefaultEditorsView : DemoView
  {
    public PropertyGridDefaultEditorsView()
    {
      InitializeComponent();

      var selectedObject = new AllEditorTypes();
      selectedObject.Boolean = true;
      selectedObject.Byte = ( byte )1;
      selectedObject.Color = Colors.Blue;
      selectedObject.DateTime = System.DateTime.Now;
      selectedObject.Decimal = ( decimal )2;
      selectedObject.Double = (double)3;
      selectedObject.Enum = HorizontalAlignment.Center;
      selectedObject.FontFamily = new FontFamily( "Arial" );
      selectedObject.FontStretch = FontStretches.Normal;
      selectedObject.FontStyle = FontStyles.Italic;
      selectedObject.FontWeight = FontWeights.Bold;
      selectedObject.Guid = new Guid( "F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4" );
      selectedObject.Char = 'T';
      selectedObject.Int16 = ( short )4;
      selectedObject.Int32 = (int)5;
      selectedObject.Int64 = (long)6;
      selectedObject.ListOfInt32 = new List<int>() { 1, 2, 3 };
      selectedObject.ListOfPerson = new List<Person>() { new Person() { Name = "John Smith" }, new Person() { Name = "Robert King" } };
      selectedObject.ListOfStrings = new List<string>() { "string1", "string2", "string3" };
      selectedObject.Dictionary = new Dictionary<int, System.Windows.Media.Color>() { { 22, System.Windows.Media.Color.FromRgb( 255, 0, 0 ) }, { 33, System.Windows.Media.Color.FromRgb( 0, 255, 0 ) } };
      selectedObject.CollectionOfPerson = new Collection<Person>() { new Person() { Name = "Tom McNeil" }, new Person() { Name = "Mike Campbell" } };
      selectedObject.Person = new Person() { Name = "John Smith" };
      selectedObject.SByte = (sbyte) 7;
      selectedObject.Single = (float)8;
      selectedObject.String = "this is a string";
      selectedObject.TimeSpan = System.TimeSpan.FromHours( 2 );      
      selectedObject.UInt16 = (ushort)9;
      selectedObject.UInt32 = (uint)10;
      selectedObject.UInt64 = (ulong)11;

      this.DataContext = selectedObject;
    }

    public class AllEditorTypes
    {
      [Category( "Non-Numeric Editors" )]
      [Description( "(C# string type) This property uses a TextBox as the default editor." )]
      public string String { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "(C# bool type) This property uses a CheckBox as the default editor." )]
      public bool Boolean { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# int type) This property uses an IntegerUpDown as the default editor." )]
      public int Int32 { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# double type) This property uses a DoubleUpDown as the default editor." )]
      public double Double { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# short type) This property uses a ShortUpDown as the default editor." )]
      public short Int16 { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# long type) This property uses a LongUpDown as the default editor." )]
      public long Int64 { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# float type) This property uses a SingleUpDown as the default editor." )]
      public float Single { get; set; }      
      [Category( "Numeric Editors" )]
      [Description( "(C# decimal type) This property uses a Decimal as the default editor." )]
      public decimal Decimal
      {
        get;
        set;
      }
      [Category( "Numeric Editors" )]
      [Description( "(C# byte type) This property uses a ByteUpDown as the default editor." )]
      public byte Byte { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# sbyte type) This property uses a SByteUpDown as the default editor. This is an internal class for CLS compliance reasons. Can only be autogenerated." )]
      public sbyte SByte { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# uint type) This property uses a UInteger as the default editor. This is an internal class for CLS compliance reasons. Can only be autogenerated." )]
      public uint UInt32 { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# ulong type) This property uses a ULongUpDown as the default editor. This is an internal class for CLS compliance reasons. Can only be autogenerated." )]
      public ulong UInt64 { get; set; }
      [Category( "Numeric Editors" )]
      [Description( "(C# ushort type) This property uses a UShortUpDown as the default editor. This is an internal class for CLS compliance reasons. Can only be autogenerated." )]
      public ushort UInt16 { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a DateTimeUpDown as the default editor." )]
      public System.DateTime DateTime { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a TimeSpanUpDown as the default editor." )]
      public System.TimeSpan TimeSpan { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a ColorPicker as the default editor." )]
      public System.Windows.Media.Color? Color { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "(C# enum type) This property uses a ComboBox as the default editor. The ComboBox is auto-populated with the enum values." )]
      public HorizontalAlignment Enum { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a ComboBox as the default editor." )]
      public FontFamily FontFamily { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a ComboBox as the default editor." )]
      public FontWeight FontWeight { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a ComboBox as the default editor." )]
      public FontStyle FontStyle { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a ComboBox as the default editor." )]
      public FontStretch FontStretch { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a Guid as the default editor." )]
      public Guid Guid { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a Char as the default editor." )]
      public Char Char { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a PrimitiveTypeCollectionEditor as the default editor." )]
      public List<string> ListOfStrings { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property uses a PrimitiveTypeCollectionEditor as the default editor." )]
      public List<int> ListOfInt32 { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "(C# IList<T> type) This property uses a CollectionEditor as the default editor." )]
      public List<Person> ListOfPerson { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "(C# IDictionary<T> type) This property uses a CollectionEditor as the default editor." )]
      public Dictionary<int, System.Windows.Media.Color> Dictionary { get; set; }
      [Category( "Non-Numeric Editors" )]
      [Description( "(C# ICollection<T> type) This property uses a CollectionEditor as the default editor." )]
      public Collection<Person> CollectionOfPerson
      {
        get; set;
      }
      [Category( "Non-Numeric Editors" )]
      [Description( "This property is a complex property and has no default editor." )]
      public Person Person { get; set; }
    }

    public class Person
    {
      public string Name { get; set; }
    }
  }
}
