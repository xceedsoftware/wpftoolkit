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
using System.Windows.Data;

namespace Microsoft.Windows.Controls.PropertyGrid.Editors
{
  public abstract class TypeEditor<T> : ITypeEditor where T : FrameworkElement, new()
  {
    #region Properties

    protected T Editor
    {
      get;
      set;
    }
    protected DependencyProperty ValueProperty
    {
      get;
      set;
    }

    #endregion //Properties

    #region ITypeEditor Members

    public virtual FrameworkElement ResolveEditor( PropertyItem propertyItem )
    {
      Editor = new T();
      SetValueDependencyProperty();
      SetControlProperties();
      ResolveValueBinding( propertyItem );
      return Editor;
    }

    #endregion //ITypeEditor Members

    #region Methods

    protected virtual IValueConverter CreateValueConverter()
    {
      return null;
    }

    protected virtual void ResolveValueBinding( PropertyItem propertyItem )
    {
      var _binding = new Binding( "Value" );
      _binding.Source = propertyItem;
      _binding.ValidatesOnExceptions = true;
      _binding.ValidatesOnDataErrors = true;
      _binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
      _binding.Converter = CreateValueConverter();
      BindingOperations.SetBinding( Editor, ValueProperty, _binding );
    }

    protected virtual void SetControlProperties()
    {
      //TODO: implement in derived class
    }

    protected abstract void SetValueDependencyProperty();

    #endregion //Methods
  }
}
