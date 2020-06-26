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

using System;
using System.Collections.Generic;
namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyDefinition : PropertyDefinitionBase
  {
    private string _name;
    private bool? _isBrowsable = true;
    private bool? _isExpandable = null;
    private string _displayName = null;
    private string _description = null;
    private string _category = null;
    private int? _displayOrder = null;

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

    public string Category
    {
      get { return _category; }
      set 
      {
        this.ThrowIfLocked( () => this.Category );
        _category = value; 
      }
    }

    public string DisplayName
    {
      get { return _displayName; }
      set
      {
        this.ThrowIfLocked( () => this.DisplayName );
        _displayName = value;
      }
    }

    public string Description
    {
      get { return _description; }
      set
      {
        this.ThrowIfLocked( () => this.Description );
        _description = value;
      }
    }

    public int? DisplayOrder
    {
      get { return _displayOrder; }
      set
      {
        this.ThrowIfLocked( () => this.DisplayOrder );
        _displayOrder = value;
      }
    }

    public bool? IsBrowsable
    {
      get { return _isBrowsable; }
      set
      {
        this.ThrowIfLocked( () => this.IsBrowsable );
        _isBrowsable = value;
      }
    }

    public bool? IsExpandable
    {
      get { return _isExpandable; }
      set
      {
        this.ThrowIfLocked( () => this.IsExpandable );
        _isExpandable = value;
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
