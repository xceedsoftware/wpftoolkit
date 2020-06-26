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

using Xceed.Wpf.Toolkit.Primitives;
using System;
using System.Windows;
using System.Windows.Data;
#if !VS2008
using System.ComponentModel.DataAnnotations;
#endif
using System.ComponentModel;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class UpDownEditor<TEditor, TType> : TypeEditor<TEditor> where TEditor : UpDownBase<TType>, new()
  {
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = UpDownBase<TType>.ValueProperty;
    }

#if !VS2008
    internal void SetMinMaxFromRangeAttribute( PropertyDescriptor propertyDescriptor, TypeConverter converter )
    {
      if( propertyDescriptor == null )
        return;

      var rangeAttribute = PropertyGridUtilities.GetAttribute<RangeAttribute>( propertyDescriptor );
      if( rangeAttribute != null )
      {
        Editor.Maximum = ((TType)converter.ConvertFrom( rangeAttribute.Maximum.ToString() ));
        Editor.Minimum = ((TType)converter.ConvertFrom( rangeAttribute.Minimum.ToString() ));
      }
    }
#endif
  }

  public class NumericUpDownEditor<TEditor, TType> : UpDownEditor<TEditor, TType> where TEditor : UpDownBase<TType>, new()
  {
    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );

      var binding = new Binding( "IsInvalid" );
      binding.Source = this.Editor;
      binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      binding.Mode = BindingMode.TwoWay;
      BindingOperations.SetBinding( propertyItem, PropertyItem.IsInvalidProperty, binding );
    }
  }

  public class ByteUpDownEditor : NumericUpDownEditor<ByteUpDown, byte?>
  {
    protected override ByteUpDown CreateEditor()
    {
      return new PropertyGridEditorByteUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( byte ) ) );
#endif
    }
  }

  public class DecimalUpDownEditor : NumericUpDownEditor<DecimalUpDown, decimal?>
  {
    protected override DecimalUpDown CreateEditor()
    {
      return new PropertyGridEditorDecimalUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( decimal ) ) );
#endif
    }
  }

  public class DoubleUpDownEditor : NumericUpDownEditor<DoubleUpDown, double?> 
  {
    protected override DoubleUpDown CreateEditor()
    {
      return new PropertyGridEditorDoubleUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
      Editor.AllowInputSpecialValues = AllowedSpecialValues.Any;

#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( double ) ) );      
#endif
    }
  }

  public class IntegerUpDownEditor : NumericUpDownEditor<IntegerUpDown, int?>
  {
    protected override IntegerUpDown CreateEditor()
    {
      return new PropertyGridEditorIntegerUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( int ) ) );
#endif
    }
  }

  public class LongUpDownEditor : NumericUpDownEditor<LongUpDown, long?>
  {
    protected override LongUpDown CreateEditor()
    {
      return new PropertyGridEditorLongUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( long ) ) );
#endif
    }
  }

  public class ShortUpDownEditor : NumericUpDownEditor<ShortUpDown, short?>
  {
    protected override ShortUpDown CreateEditor()
    {
      return new PropertyGridEditorShortUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( short ) ) );
#endif
    }
  }

  public class SingleUpDownEditor : NumericUpDownEditor<SingleUpDown, float?> 
  {
    protected override SingleUpDown CreateEditor()
    {
      return new PropertyGridEditorSingleUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
      Editor.AllowInputSpecialValues = AllowedSpecialValues.Any;
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( float ) ) );
#endif
    }
  }

  public class DateTimeUpDownEditor : UpDownEditor<DateTimeUpDown, DateTime?>
  {
    protected override DateTimeUpDown CreateEditor()
    {
      return new PropertyGridEditorDateTimeUpDown();
    }
    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( DateTime ) ) );
#endif
    }
  }

  public class TimeSpanUpDownEditor : UpDownEditor<TimeSpanUpDown, TimeSpan?>
  {
    protected override TimeSpanUpDown CreateEditor()
    {
      return new PropertyGridEditorTimeSpanUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( TimeSpan ) ) );
#endif
    }
  }

  internal class SByteUpDownEditor : NumericUpDownEditor<SByteUpDown, sbyte?>
  {
    protected override SByteUpDown CreateEditor()
    {
      return new PropertyGridEditorSByteUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( sbyte ) ) );
#endif
    }
  }

  internal class UIntegerUpDownEditor : NumericUpDownEditor<UIntegerUpDown, uint?>
  {
    protected override UIntegerUpDown CreateEditor()
    {
      return new PropertyGridEditorUIntegerUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( uint ) ) );
#endif
    }
  }

  internal class ULongUpDownEditor : NumericUpDownEditor<ULongUpDown, ulong?>
  {
    protected override ULongUpDown CreateEditor()
    {
      return new PropertyGridEditorULongUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( ulong ) ) );
#endif
    }
  }

  internal class UShortUpDownEditor : NumericUpDownEditor<UShortUpDown, ushort?>
  {
    protected override UShortUpDown CreateEditor()
    {
      return new PropertyGridEditorUShortUpDown();
    }

    protected override void SetControlProperties( PropertyItem propertyItem )
    {
      base.SetControlProperties( propertyItem );
#if !VS2008
      this.SetMinMaxFromRangeAttribute( propertyItem.PropertyDescriptor, TypeDescriptor.GetConverter( typeof( ushort ) ) );
#endif
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

  public class PropertyGridEditorTimeSpanUpDown : TimeSpanUpDown
  {
    static PropertyGridEditorTimeSpanUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorTimeSpanUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorTimeSpanUpDown ) ) );
    }
  }

  [CLSCompliantAttribute( false )]
  public class PropertyGridEditorSByteUpDown : SByteUpDown
  {
    static PropertyGridEditorSByteUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorSByteUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorSByteUpDown ) ) );
    }
  }

  [CLSCompliantAttribute( false )]
  public class PropertyGridEditorUIntegerUpDown : UIntegerUpDown
  {
    static PropertyGridEditorUIntegerUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorUIntegerUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorUIntegerUpDown ) ) );
    }
  }

  [CLSCompliantAttribute( false )]
  public class PropertyGridEditorULongUpDown : ULongUpDown
  {
    static PropertyGridEditorULongUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorULongUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorULongUpDown ) ) );
    }
  }

  [CLSCompliantAttribute( false )]
  public class PropertyGridEditorUShortUpDown : UShortUpDown
  {
    static PropertyGridEditorUShortUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( PropertyGridEditorUShortUpDown ), new FrameworkPropertyMetadata( typeof( PropertyGridEditorUShortUpDown ) ) );
    }
  }

}
