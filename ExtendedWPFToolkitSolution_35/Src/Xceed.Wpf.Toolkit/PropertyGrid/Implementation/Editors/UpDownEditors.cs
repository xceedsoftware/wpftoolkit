/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using Xceed.Wpf.Toolkit.Primitives;
using System;
using System.Windows;
namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class UpDownEditor<TEditor, TType> : TypeEditor<TEditor> where TEditor : UpDownBase<TType>, new()
  {
    protected override void SetControlProperties()
    {
      Editor.TextAlignment = System.Windows.TextAlignment.Left;
    }
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = UpDownBase<TType>.ValueProperty;
    }
  }

  public class ByteUpDownEditor : UpDownEditor<ByteUpDown, byte?>
  {
    protected override ByteUpDown CreateEditor()
    {
      return new PropertyGridEditorByteUpDown();
    }
  }

  public class DecimalUpDownEditor : UpDownEditor<DecimalUpDown, decimal?>
  {
    protected override DecimalUpDown CreateEditor()
    {
      return new PropertyGridEditorDecimalUpDown();
    }
  }

  public class DoubleUpDownEditor : UpDownEditor<DoubleUpDown, double?> 
  {
    protected override DoubleUpDown CreateEditor()
    {
      return new PropertyGridEditorDoubleUpDown();
    }

    protected override void SetControlProperties()
    {
      base.SetControlProperties();
      Editor.AllowInputSpecialValues = AllowedSpecialValues.Any;
    }
  }

  public class IntegerUpDownEditor : UpDownEditor<IntegerUpDown, int?>
  {
    protected override IntegerUpDown CreateEditor()
    {
      return new PropertyGridEditorIntegerUpDown();
    }
  }

  public class LongUpDownEditor : UpDownEditor<LongUpDown, long?>
  {
    protected override LongUpDown CreateEditor()
    {
      return new PropertyGridEditorLongUpDown();
    }
  }

  public class ShortUpDownEditor : UpDownEditor<ShortUpDown, short?>
  {
    protected override ShortUpDown CreateEditor()
    {
      return new PropertyGridEditorShortUpDown();
    }
  }

  public class SingleUpDownEditor : UpDownEditor<SingleUpDown, float?> 
  {
    protected override SingleUpDown CreateEditor()
    {
      return new PropertyGridEditorSingleUpDown();
    }

    protected override void SetControlProperties()
    {
      base.SetControlProperties();
      Editor.AllowInputSpecialValues = AllowedSpecialValues.Any;
    }
  }

  public class DateTimeUpDownEditor : UpDownEditor<DateTimeUpDown, DateTime?>
  {
    protected override DateTimeUpDown CreateEditor()
    {
      return new PropertyGridEditorDateTimeUpDown();
    }
  }

  internal class SByteUpDownEditor : UpDownEditor<SByteUpDown, sbyte?>
  {
    protected override SByteUpDown CreateEditor()
    {
      return new PropertyGridEditorSByteUpDown();
    }
  }

  internal class UIntegerUpDownEditor : UpDownEditor<UIntegerUpDown, uint?>
  {
    protected override UIntegerUpDown CreateEditor()
    {
      return new PropertyGridEditorUIntegerUpDown();
    }
  }

  internal class ULongUpDownEditor : UpDownEditor<ULongUpDown, ulong?>
  {
    protected override ULongUpDown CreateEditor()
    {
      return new PropertyGridEditorULongUpDown();
    }
  }

  internal class UShortUpDownEditor : UpDownEditor<UShortUpDown, ushort?>
  {
    protected override UShortUpDown CreateEditor()
    {
      return new PropertyGridEditorUShortUpDown();
    }
  }



  public class PropertyGridEditorByteUpDown : ByteUpDown
  {
    static PropertyGridEditorByteUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorByteUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorByteUpDown ) ) );
    }
  }

  public class PropertyGridEditorDecimalUpDown : DecimalUpDown
  {
    static PropertyGridEditorDecimalUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorDecimalUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorDecimalUpDown ) ) );
    }
  }

  public class PropertyGridEditorDoubleUpDown : DoubleUpDown
  {
    static PropertyGridEditorDoubleUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorDoubleUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorDoubleUpDown ) ) );
    }
  }

  public class PropertyGridEditorIntegerUpDown : IntegerUpDown
  {
    static PropertyGridEditorIntegerUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorIntegerUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorIntegerUpDown ) ) );
    }
  }

  public class PropertyGridEditorLongUpDown : LongUpDown
  {
    static PropertyGridEditorLongUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorLongUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorLongUpDown ) ) );
    }
  }

  public class PropertyGridEditorShortUpDown : ShortUpDown
  {
    static PropertyGridEditorShortUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorShortUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorShortUpDown ) ) );
    }
  }

  public class PropertyGridEditorSingleUpDown : SingleUpDown
  {
    static PropertyGridEditorSingleUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorSingleUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorSingleUpDown ) ) );
    }
  }

  public class PropertyGridEditorDateTimeUpDown : DateTimeUpDown
  {
    static PropertyGridEditorDateTimeUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorDateTimeUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorDateTimeUpDown ) ) );
    }
  }

  internal class PropertyGridEditorSByteUpDown : SByteUpDown
  {
    static PropertyGridEditorSByteUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorSByteUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorSByteUpDown ) ) );
    }
  }

  internal class PropertyGridEditorUIntegerUpDown : UIntegerUpDown
  {
    static PropertyGridEditorUIntegerUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorUIntegerUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorUIntegerUpDown ) ) );
    }
  }

  internal class PropertyGridEditorULongUpDown : ULongUpDown
  {
    static PropertyGridEditorULongUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorULongUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorULongUpDown ) ) );
    }
  }

  internal class PropertyGridEditorUShortUpDown : UShortUpDown
  {
    static PropertyGridEditorUShortUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorUShortUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorUShortUpDown ) ) );
    }
  }

}
