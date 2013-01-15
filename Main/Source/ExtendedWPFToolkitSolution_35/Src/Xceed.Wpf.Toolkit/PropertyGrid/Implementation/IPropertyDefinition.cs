/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal interface IPropertyDefinition
  {
    ImageSource AdvancedOptionsIcon { get; }
    object AdvancedOptionsTooltip { get; }
    string Category { get; }
    string CategoryValue { get; }
    IEnumerable<IPropertyDefinition> ChildrenDefinitions { get; }
    IEnumerable<CommandBinding> CommandBindings { get; }
    string Description { get; }
    string DisplayName { get; }
    int DisplayOrder { get; }
    bool IsExpandable { get; }
    IPropertyParent PropertyParent { get; }
    object Value { get; set; }

    FrameworkElement GenerateEditorElement( PropertyItem propertyItem );
  }
}
