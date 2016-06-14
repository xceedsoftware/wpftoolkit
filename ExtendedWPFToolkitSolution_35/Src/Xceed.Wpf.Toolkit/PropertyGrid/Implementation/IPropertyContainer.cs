/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/


using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Windows;
namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal interface IPropertyContainer
  {






    ContainerHelperBase ContainerHelper { get; }

    Style PropertyContainerStyle { get; }

    EditorDefinitionCollection EditorDefinitions { get; }

    PropertyDefinitionCollection PropertyDefinitions { get; }

    bool IsCategorized { get; }

    bool IsSortedAlphabetically { get; }

    bool AutoGenerateProperties { get; }

    bool HideInheritedProperties { get; }

    FilterInfo FilterInfo { get; }

    bool? IsPropertyVisible( PropertyDescriptor pd );
  }
}
