/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid
{
  public class CellEditor: Freezable
  {
    static CellEditor()
    {
      CellEditor.ActivationGesturesProperty = CellEditor.ActivationGesturesPropertyKey.DependencyProperty;
    }

    public CellEditor()
    {
      this.SetActivationGestures( new ActivationGestureCollection() );
    }

    public static CellEditor TextBoxEditor
    {
      get
      {
        return DefaultCellEditorSelector.TextBoxEditor;
      }
    }

    public static CellEditor CheckBoxEditor
    {
      get
      {
        return DefaultCellEditorSelector.CheckBoxEditor;
      }
    }

    public static CellEditor DatePickerEditor
    {
      get
      {
        return DefaultCellEditorSelector.DateTimeEditor;
      }
    }

    #region EditTemplate Property

    public static readonly DependencyProperty EditTemplateProperty =
        DependencyProperty.Register( "EditTemplate", typeof( DataTemplate ), typeof( CellEditor ), new UIPropertyMetadata( null ) );

    public DataTemplate EditTemplate
    {
      get
      {
        return ( DataTemplate )this.GetValue( CellEditor.EditTemplateProperty );
      }
      set
      {
        this.SetValue( CellEditor.EditTemplateProperty, value );
      }
    }

    #endregion EditTemplate Property

    #region ActivationGestures Property

    private static readonly DependencyPropertyKey ActivationGesturesPropertyKey = DependencyProperty.RegisterReadOnly(
      "ActivationGestures",
      typeof( ActivationGestureCollection ), 
      typeof( CellEditor ), 
      new FrameworkPropertyMetadata( ( ActivationGestureCollection )null ) );

    public static readonly DependencyProperty ActivationGesturesProperty;

    public ActivationGestureCollection ActivationGestures
    {
      get 
      { 
        return (ActivationGestureCollection)this.GetValue( CellEditor.ActivationGesturesProperty ); 
      }
    }

    private void SetActivationGestures( ActivationGestureCollection value )
    {
      this.SetValue( CellEditor.ActivationGesturesPropertyKey, value );
    }

    #endregion

    #region HasError Attached Property

    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.RegisterAttached( "HasError", typeof( bool ), typeof( CellEditor ), new UIPropertyMetadata( false ) );

    public static bool GetHasError( DependencyObject obj )
    {
      return ( bool )obj.GetValue( HasErrorProperty );
    }

    public static void SetHasError( DependencyObject obj, bool value )
    {
      obj.SetValue( HasErrorProperty, value );
    }

    #endregion HasError Attached Property

    protected override Freezable CreateInstanceCore()
    {
      CellEditor editorClone = new CellEditor();

      foreach( ActivationGesture gesture in this.ActivationGestures )
      {
        editorClone.ActivationGestures.Add( gesture.Clone() as ActivationGesture );
      }

      return editorClone;
    }

    internal KeyActivationGesture GetMatchingKeyActivationGesture( Key key, Key systemKey, ModifierKeys modifier)
    {
      foreach( ActivationGesture gesture in this.ActivationGestures )
      {
        KeyActivationGesture keyGesture = gesture as KeyActivationGesture;

        if( keyGesture != null )
        {
          if( keyGesture.IsActivationKey( key, systemKey, modifier ) == true )
          {
            return keyGesture;
          }
        }
      }

      return null; 
    }

    internal virtual bool IsTextInputActivation()
    {
      foreach( ActivationGesture gesture in this.ActivationGestures )
      {
        if( gesture is TextInputActivationGesture )
        {
          return true;
        }
      }
      return false;
    }
  }
}
