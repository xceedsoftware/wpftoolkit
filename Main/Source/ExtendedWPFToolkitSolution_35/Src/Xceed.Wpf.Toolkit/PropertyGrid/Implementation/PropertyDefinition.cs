/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyDefinition : PropertyDefinitionBase
  {
    private string _name;

    [Obsolete(@"Use 'TargetProperties' instead of 'Name'")]
    public string Name
    {
      get { return _name; }
      set 
      {
        const string usageError = "{0}: \'Name\' property is obsolete. Instead use \'TargetProperties\'. (XAML example: <t:PropertyDefinition TargetProperties=\"FirstName,LastName\" />)";
        System.Diagnostics.Trace.TraceWarning( usageError, typeof( PropertyDefinition ) );
        _name = value; 
      }
    }

    internal override void Lock()
    {
      if( _name != null
        && this.TargetProperties != null
        && this.TargetProperties.Count > 0 )
      {
        throw new InvalidOperationException(
          string.Format(
            @"{0}: When using 'TargetProperties' property, do not use 'Name' property.", 
            typeof( PropertyDefinition ) ) );
      }

      if( _name != null )
      {
        this.TargetProperties = new List<object>() { _name };
      }
      base.Lock();
    }
  }
}
