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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Xceed.Wpf.Toolkit.Core.Input
{
  [TypeConverter( typeof( KeyModifierCollectionConverter ) )]
  public class KeyModifierCollection : Collection<KeyModifier>
  {
    #region AreActive Property

    public bool AreActive
    {
      get
      {
        if( this.Count == 0 )
          return true;

        // if the Blocked modifier is present, then the action is not allowed
        // so simply return false
        if( this.Contains( KeyModifier.Blocked ) )
          return false;

        if( this.Contains( KeyModifier.Exact ) )
          return this.IsExactMatch();

        return this.MatchAny();
      }
    }

    #endregion

    private static bool IsKeyPressed( KeyModifier modifier, ICollection<Key> keys )
    {
      switch( modifier )
      {
        case KeyModifier.Alt:
          return keys.Contains( Key.LeftAlt )
              || keys.Contains( Key.RightAlt );

        case KeyModifier.LeftAlt:
          return keys.Contains( Key.LeftAlt );

        case KeyModifier.RightAlt:
          return keys.Contains( Key.RightAlt );

        case KeyModifier.Ctrl:
          return keys.Contains( Key.LeftCtrl )
              || keys.Contains( Key.RightCtrl );

        case KeyModifier.LeftCtrl:
          return keys.Contains( Key.LeftCtrl );

        case KeyModifier.RightCtrl:
          return keys.Contains( Key.RightCtrl );

        case KeyModifier.Shift:
          return keys.Contains( Key.LeftShift )
              || keys.Contains( Key.RightShift );

        case KeyModifier.LeftShift:
          return keys.Contains( Key.LeftShift );

        case KeyModifier.RightShift:
          return keys.Contains( Key.RightShift );

        case KeyModifier.None:
          return true;

        default:
          throw new NotSupportedException( "Unknown modifier" );
      }
    }

    private static bool HasModifier( Key key, ICollection<KeyModifier> modifiers )
    {
      switch( key )
      {
        case Key.LeftAlt:
          return modifiers.Contains( KeyModifier.Alt )
              || modifiers.Contains( KeyModifier.LeftAlt );

        case Key.RightAlt:
          return modifiers.Contains( KeyModifier.Alt )
              || modifiers.Contains( KeyModifier.RightAlt );

        case Key.LeftCtrl:
          return modifiers.Contains( KeyModifier.Ctrl )
              || modifiers.Contains( KeyModifier.LeftCtrl );

        case Key.RightCtrl:
          return modifiers.Contains( KeyModifier.Ctrl )
              || modifiers.Contains( KeyModifier.RightCtrl );

        case Key.LeftShift:
          return modifiers.Contains( KeyModifier.Shift )
              || modifiers.Contains( KeyModifier.LeftShift );

        case Key.RightShift:
          return modifiers.Contains( KeyModifier.Shift )
              || modifiers.Contains( KeyModifier.RightShift );

        default:
          throw new NotSupportedException( "Unknown key" );
      }
    }

    private bool IsExactMatch()
    {
      HashSet<KeyModifier> modifiers = this.GetKeyModifiers();
      HashSet<Key> keys = this.GetKeysPressed();

      // No key must be pressed for the modifier None.
      if( this.Contains( KeyModifier.None ) )
        return ( modifiers.Count == 0 )
            && ( keys.Count == 0 );

      // Make sure every modifier has a matching key pressed.
      foreach( KeyModifier modifier in modifiers )
      {
        if( !KeyModifierCollection.IsKeyPressed( modifier, keys ) )
          return false;
      }

      // Make sure every key pressed has a matching modifier.
      foreach( Key key in keys )
      {
        if( !KeyModifierCollection.HasModifier( key, modifiers ) )
          return false;
      }

      return true;
    }

    private bool MatchAny()
    {
      if( this.Contains( KeyModifier.None ) )
        return true;

      HashSet<KeyModifier> modifiers = this.GetKeyModifiers();
      HashSet<Key> keys = this.GetKeysPressed();

      foreach( KeyModifier modifier in modifiers )
      {
        if( KeyModifierCollection.IsKeyPressed( modifier, keys ) )
          return true;
      }

      return false;
    }

    private HashSet<KeyModifier> GetKeyModifiers()
    {
      HashSet<KeyModifier> modifiers = new HashSet<KeyModifier>();

      foreach( KeyModifier modifier in this )
      {
        switch( modifier )
        {
          case KeyModifier.Alt:
          case KeyModifier.LeftAlt:
          case KeyModifier.RightAlt:
          case KeyModifier.Ctrl:
          case KeyModifier.LeftCtrl:
          case KeyModifier.RightCtrl:
          case KeyModifier.Shift:
          case KeyModifier.LeftShift:
          case KeyModifier.RightShift:
            {
              if( !modifiers.Contains( modifier ) )
              {
                modifiers.Add( modifier );
              }
            }
            break;

          default:
            break;
        }
      }

      return modifiers;
    }

    private HashSet<Key> GetKeysPressed()
    {
      HashSet<Key> keys = new HashSet<Key>();

      if( Keyboard.IsKeyDown( Key.LeftAlt ) )
        keys.Add( Key.LeftAlt );
      if( Keyboard.IsKeyDown( Key.RightAlt ) )
        keys.Add( Key.RightAlt );
      if( Keyboard.IsKeyDown( Key.LeftCtrl ) )
        keys.Add( Key.LeftCtrl );
      if( Keyboard.IsKeyDown( Key.RightCtrl ) )
        keys.Add( Key.RightCtrl );
      if( Keyboard.IsKeyDown( Key.LeftShift ) )
        keys.Add( Key.LeftShift );
      if( Keyboard.IsKeyDown( Key.RightShift ) )
        keys.Add( Key.RightShift );

      return keys;
    }
  }
}
