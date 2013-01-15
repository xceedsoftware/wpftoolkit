/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.PropertyGrid.Commands;
using System.Windows.Media;
using System.Collections;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Markup.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Implementation
{
  internal class DescriptorPropertyDefinitionBase : DependencyObject, IPropertyDefinition, IPropertyParent
  {
    #region Members

    internal readonly IPropertyParent _propertyParent;
    private string _category;
    private string _categoryValue;
    private string _description;
    private string _displayName;
    private int _displayOrder;
    private bool _isExpandable;
    private IEnumerable<CommandBinding> _commandBindings;
    private IEnumerable<IPropertyDefinition> _children;

    #endregion

    #region Constructors

    internal DescriptorPropertyDefinitionBase( IPropertyParent propertyParent )
    {
      if( propertyParent == null )
        throw new ArgumentNullException( "propertyParent" );

      _propertyParent = propertyParent;
    }

    #endregion

    #region Virtual Methods

    protected virtual string ComputeCategory()
    {
      return null;
    }

    protected virtual string ComputeCategoryValue()
    {
      return null;
    }

    protected virtual string ComputeDescription()
    {
      return null;
    }

    protected virtual int ComputeDisplayOrder()
    {
      return int.MaxValue;
    }

    protected virtual bool ComputeIsExpandable()
    {
      return false;
    }

    protected virtual void ResetValue()
    {
    }

    protected virtual void SetBinding()
    {
    }

    protected virtual bool ComputeCanResetValue()
    {
      return false;
    }

    protected virtual IEnumerable<IPropertyDefinition> GenerateChildrenProperties()
    {
      return null;
    }

    protected virtual AdvancedOptionsValues ComputeAdvancedOptionsValues()
    {
      return new AdvancedOptionsValues()
      {
        ImageSource = null,
        Tooltip = null
      };
    }

    protected virtual ITypeEditor GetAttributeEditor()
    {
      return null;
    }

    protected virtual string GetPropertyName()
    {
      return null;
    }

    protected virtual Type GetPropertyType()
    {
      return null;
    }

    protected virtual string GetPropertyDisplayName()
    {
      return null;
    }

    protected virtual bool IsPropertyDescriptorReadOnly()
    {
      return false;
    }

    protected virtual ITypeEditor CreateDefaultEditor()
    {
      return null;
    }

    internal virtual PropertyDescriptor GetPropertyDescriptor()
    {
      return null;
    }


    #endregion

    #region Internal Methods

    internal void UpdateAdvanceOptionsForItem( MarkupObject markupObject, DependencyObject dependencyObject, DependencyPropertyDescriptor dpDescriptor, 
                                                out string imageName, out object tooltip )
    {
      imageName = "AdvancedProperties11";
      tooltip = "Advanced Properties";

      bool isResource = false;
      bool isDynamicResource = false;

      var markupProperty = markupObject.Properties.Where( p => p.Name == GetPropertyName() ).FirstOrDefault();
      if( markupProperty != null )
      {
        //TODO: need to find a better way to determine if a StaticResource has been applied to any property not just a style
        isResource = ( markupProperty.Value is Style );
        isDynamicResource = ( markupProperty.Value is DynamicResourceExtension );
      }

      if( isResource || isDynamicResource )
      {
        imageName = "Resource11";
        tooltip = "Resource";
      }
      else
      {
        if( ( dependencyObject != null ) && ( dpDescriptor != null ) )
        {
          if( BindingOperations.GetBindingExpressionBase( dependencyObject, dpDescriptor.DependencyProperty ) != null )
          {
            imageName = "Database11";
            tooltip = "Databinding";
          }
          else
          {
            BaseValueSource bvs =
              DependencyPropertyHelper
              .GetValueSource( dependencyObject, dpDescriptor.DependencyProperty )
              .BaseValueSource;

            switch( bvs )
            {
              case BaseValueSource.Inherited:
              case BaseValueSource.DefaultStyle:
              case BaseValueSource.ImplicitStyleReference:
                imageName = "Inheritance11";
                tooltip = "Inheritance";
                break;
              case BaseValueSource.DefaultStyleTrigger:
                break;
              case BaseValueSource.Style:
                imageName = "Style11";
                tooltip = "Style Setter";
                break;

              case BaseValueSource.Local:
                imageName = "Local11";
                tooltip = "Local";
                break;
            }
          }
        }
      }
    }

    internal AdvancedOptionsValues CreateAdvanceOptionValues( string imageName, object tooltip )
    {
      string uriPrefix = "/Xceed.Wpf.Toolkit;component/PropertyGrid/Images/";

      AdvancedOptionsValues values = new AdvancedOptionsValues();
      values.Tooltip = tooltip;
      values.ImageSource = new BitmapImage( new Uri( String.Format( "{0}{1}.png", uriPrefix, imageName ), UriKind.RelativeOrAbsolute ) );

      return values;
    }

    internal void UpdateAdvanceOptions()
    {
      AdvancedOptionsValues advancedOptions = ComputeAdvancedOptionsValues();
      AdvancedOptionsIcon = advancedOptions.ImageSource;
      AdvancedOptionsTooltip = advancedOptions.Tooltip;
    }




    internal object ComputeDescriptionForItem( object item )
    {
      PropertyDescriptor pd = item as PropertyDescriptor;

      //We do not simply rely on the "Description" property of PropertyDescriptor
      //since this value is cached by PropertyDescriptor and the localized version 
      //(e.g., LocalizedDescriptionAttribute) value can dynamicaly change.
      DescriptionAttribute descriptionAtt = PropertyGridUtilities.GetAttribute<DescriptionAttribute>( pd );
      return ( descriptionAtt != null )
              ? descriptionAtt.Description
              : pd.Description;
    }

    internal object ComputeDisplayOrderForItem( object item )
    {
      PropertyDescriptor pd = item as PropertyDescriptor;
      var attribute = PropertyGridUtilities.GetAttribute<PropertyOrderAttribute>( pd );

      // Max Value. Properties with no order will be displayed last.
      return ( attribute != null )
              ? attribute.Order
              : int.MaxValue;
    }

    #endregion

    #region Private Methods

    private void ExecuteResetValueCommand( object sender, ExecutedRoutedEventArgs e )
    {
      if( ComputeCanResetValue() )
        ResetValue();
    }

    private void CanExecuteResetValueCommand( object sender, CanExecuteRoutedEventArgs e )
    {
      e.CanExecute = ComputeCanResetValue();
    }

    private string ComputeDisplayName()
    {
      string displayName = GetPropertyDisplayName();
      var attribute = PropertyGridUtilities.GetAttribute<ParenthesizePropertyNameAttribute>( GetPropertyDescriptor() );
      if( ( attribute != null ) && attribute.NeedParenthesis )
      {
        displayName = "(" + displayName + ")";
      }

      return displayName;
    }

     private FrameworkElement CreateCustomEditor( EditorDefinition customEditor )
    {
      return ( customEditor != null && customEditor.EditorTemplate != null )
        ? customEditor.EditorTemplate.LoadContent() as FrameworkElement
        : null;
    }

    #endregion

    #region IPropertyDefinition Properties

    #region AdvancedOptionsIcon (DP)

    public static readonly DependencyProperty AdvancedOptionsIconProperty =
        DependencyProperty.Register( "AdvancedOptionsIcon", typeof( ImageSource ), typeof( DescriptorPropertyDefinitionBase ), new UIPropertyMetadata( null ) );

    public ImageSource AdvancedOptionsIcon
    {
      get
      {
        return ( ImageSource )GetValue( AdvancedOptionsIconProperty );
      }
      set
      {
        SetValue( AdvancedOptionsIconProperty, value );
      }
    }

    #endregion

    #region AdvancedOptionsTooltip (DP)

    public static readonly DependencyProperty AdvancedOptionsTooltipProperty =
        DependencyProperty.Register( "AdvancedOptionsTooltip", typeof( object ), typeof( DescriptorPropertyDefinitionBase ), new UIPropertyMetadata( null ) );

    public object AdvancedOptionsTooltip
    {
      get
      {
        return ( object )GetValue( AdvancedOptionsTooltipProperty );
      }
      set
      {
        SetValue( AdvancedOptionsTooltipProperty, value );
      }
    }

    #endregion //AdvancedOptionsTooltip

    public string Category
    {
      get
      {
        return _category;
      }
    }

    public string CategoryValue
    {
      get
      {
        return _categoryValue;
      }
    }

    public IEnumerable<IPropertyDefinition> ChildrenDefinitions
    {
      get
      {
        if( _children == null )
        {
          _children = GenerateChildrenProperties();
        }

        return _children;
      }
    }

    public IEnumerable<CommandBinding> CommandBindings
    {
      get
      {
        return _commandBindings;
      }
    }

    public string DisplayName
    {
      get
      {
        return _displayName;
      }
    }

    public string Description
    {
      get
      {
        return _description;
      }
    }

    public int DisplayOrder
    {
      get
      {
        return _displayOrder;
      }
    }

    public bool IsExpandable
    {
      get
      {
        return _isExpandable;
      }
    }

    public IPropertyParent PropertyParent
    {
      get
      {
        return _propertyParent;
      }
    }

    #region Value Property (DP)

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( object ), typeof( DescriptorPropertyDefinitionBase ), new UIPropertyMetadata( null, OnValueChanged ) );
    public object Value
    {
      get
      {
        return GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ( ( DescriptorPropertyDefinitionBase )o ).OnValueChanged( e.OldValue, e.NewValue );
    }

    private void OnValueChanged( object oldValue, object newValue )
    {
      UpdateAdvanceOptions();

      // Reset command also affected.
      CommandManager.InvalidateRequerySuggested();
    }

    #endregion //Value Property

    #endregion IPropertyDefinition Properties

    #region IPropertyParent Properties

    public object ValueInstance
    {
      get
      {
        return this.Value;
      }
    }

    public EditorDefinitionCollection EditorDefinitions
    {
      get
      {
        return PropertyParent.EditorDefinitions;
      }
    }

    #endregion

    #region IPropertyDefinition Methods

    public FrameworkElement GenerateEditorElement( PropertyItem propertyItem )
    {
      FrameworkElement editorElement = null;

      // Priority #1: CustomEditors based on the Attribute.
      ITypeEditor editor = GetAttributeEditor();
      if( editor != null )
        editorElement = editor.ResolveEditor( propertyItem );

      if( _propertyParent.EditorDefinitions != null )
      {
        // Priority #2: Custom Editors based on the name (same for all PropertyDescriptors).
        if( editorElement == null )
          editorElement = CreateCustomEditor( _propertyParent.EditorDefinitions[ GetPropertyName() ] );

        // Priority #3: Custom Editors based on the type (same for all PropertyDescriptors).
        if( editorElement == null )
          editorElement = CreateCustomEditor( _propertyParent.EditorDefinitions[ GetPropertyType() ] );
      }

      if( editorElement == null )
      {
        // Priority #4: Default Editor. Read-only properties use a TextBox.
        if( IsPropertyDescriptorReadOnly() )
          editor = new TextBlockEditor();

        // Fallback: Use a default type editor.
        if( editor == null )
        {
          editor = CreateDefaultEditor();
        }

        Debug.Assert( editor != null );

        editorElement = editor.ResolveEditor( propertyItem );
      }

      return editorElement;
    }

    public void InitProperties()
    {
      _category = ComputeCategory();
      _categoryValue = ComputeCategoryValue();
      _description = ComputeDescription();
      _displayName = ComputeDisplayName();
      _displayOrder = ComputeDisplayOrder();
      _isExpandable = ComputeIsExpandable();
      _commandBindings = new CommandBinding[] { new CommandBinding( PropertyItemCommands.ResetValue, ExecuteResetValueCommand, CanExecuteResetValueCommand ) };

      UpdateAdvanceOptions();

      SetBinding();
    }

    #endregion IPropertyDefinition Methods




    #region AdvancedOptionsValues struct (Internal )

    internal struct AdvancedOptionsValues
    {
      public ImageSource ImageSource;
      public object Tooltip;
    }

    #endregion
  }
}
