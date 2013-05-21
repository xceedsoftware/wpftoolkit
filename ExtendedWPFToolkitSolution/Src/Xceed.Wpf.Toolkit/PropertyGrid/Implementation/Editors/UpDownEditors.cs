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
namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class UpDownEditor<TEditor, TType> : TypeEditor<TEditor> where TEditor : UpDownBase<TType>, new()
  {
    protected override void SetControlProperties()
    {
      Editor.TextAlignment = System.Windows.TextAlignment.Left;
      Editor.Style = PropertyGridUtilities.GetUpDownBaseStyle<TType>();
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
