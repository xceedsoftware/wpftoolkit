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
