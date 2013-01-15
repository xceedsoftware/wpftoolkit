/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using Xceed.Wpf.Toolkit.Primitives;
using System;
namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class UpDownEditor<TEditor, TType> : TypeEditor<TEditor> where TEditor : UpDownBase<TType>, new()
  {
    protected override void SetControlProperties()
    {
      Editor.BorderThickness = new System.Windows.Thickness( 0 );
    }
    protected override void SetValueDependencyProperty()
    {
      ValueProperty = UpDownBase<TType>.ValueProperty;
    }
  }

  public class ByteUpDownEditor : UpDownEditor<ByteUpDown, byte?> { }

  public class DecimalUpDownEditor : UpDownEditor<DecimalUpDown, decimal?> { }

  public class DoubleUpDownEditor : UpDownEditor<DoubleUpDown, double?> 
  {
    protected override void SetControlProperties()
    {
      base.SetControlProperties();
      Editor.AllowInputSpecialValues = AllowedSpecialValues.Any;
    }
  }

  public class IntegerUpDownEditor : UpDownEditor<IntegerUpDown, int?> { }

  public class LongUpDownEditor : UpDownEditor<LongUpDown, long?> { }

  public class ShortUpDownEditor : UpDownEditor<ShortUpDown, short?> { }

  public class SingleUpDownEditor : UpDownEditor<SingleUpDown, float?> 
  {
    protected override void SetControlProperties()
    {
      base.SetControlProperties();
      Editor.AllowInputSpecialValues = AllowedSpecialValues.Any;
    }
  }

  public class DateTimeUpDownEditor : UpDownEditor<DateTimeUpDown, DateTime?> { }

}
