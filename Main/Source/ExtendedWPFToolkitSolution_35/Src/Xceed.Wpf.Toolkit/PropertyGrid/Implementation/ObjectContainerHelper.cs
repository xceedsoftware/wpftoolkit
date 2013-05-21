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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal class ObjectContainerHelper : ObjectContainerHelperBase
  {
    private object _selectedObject;

    public ObjectContainerHelper( IPropertyContainer propertyContainer, object selectedObject )
      : base( propertyContainer )
    {
      _selectedObject = selectedObject;
    }

    private object SelectedObject
    {
      get
      {
        return _selectedObject;
      }
    }

    protected override string GetDefaultPropertyName()
    {
      object selectedObject = SelectedObject;
      return ( selectedObject != null ) ? ObjectContainerHelperBase.GetDefaultPropertyName( SelectedObject ) : ( string )null;
    }

    protected override IEnumerable<PropertyItem> GenerateSubPropertiesCore()
    {
      var propertyItems = new List<PropertyItem>();

      if( SelectedObject != null )
      {
        try
        {
          List<PropertyDescriptor> descriptors = ObjectContainerHelperBase.GetPropertyDescriptors( SelectedObject );
          if( !PropertyContainer.AutoGenerateProperties )
          {
            List<PropertyDescriptor> specificProperties = new List<PropertyDescriptor>();
            if( PropertyContainer.PropertyDefinitions != null )
            {
              foreach( PropertyDefinition pd in PropertyContainer.PropertyDefinitions )
              {
                foreach( object targetProperty in pd.TargetProperties )
                {
                  PropertyDescriptor descriptor = null;
                  // If the target is a string, compare it with the property name.
                  var propName = targetProperty as string;
                  if( propName != null )
                  {
                    descriptor = descriptors.FirstOrDefault( d => d.Name == propName );
                  }

                  // If the target is a Type, compare it with the property type.
                  var propType = targetProperty as Type;
                  if( propType != null )
                  {
                    descriptor = descriptors.FirstOrDefault( d => d.PropertyType == propType );
                  }

                  if( descriptor != null )
                  {
                    specificProperties.Add( descriptor );
                    descriptors.Remove( descriptor );
                  }
                }
              }
            }

            descriptors = specificProperties;
          }

          foreach( PropertyDescriptor descriptor in descriptors )
          {
            if( descriptor.IsBrowsable )
            {
              propertyItems.Add( CreatePropertyItem( descriptor ) );
            }
          }
        }
        catch( Exception )
        {
          //TODO: handle this some how
        }
      }

      return propertyItems;
    }


    private PropertyItem CreatePropertyItem( PropertyDescriptor property )
    {
      DescriptorPropertyDefinition definition = new DescriptorPropertyDefinition( property, SelectedObject );
      definition.InitProperties();
      PropertyItem propertyItem = new PropertyItem( definition );
      Debug.Assert( SelectedObject != null );
      propertyItem.Instance = SelectedObject;
      return propertyItem;
    }






  }
}
