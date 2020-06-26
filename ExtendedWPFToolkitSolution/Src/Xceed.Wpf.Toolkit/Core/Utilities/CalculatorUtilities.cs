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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  static class CalculatorUtilities
  {
    public static Calculator.CalculatorButtonType GetCalculatorButtonTypeFromText( string text )
    {
      switch( text )
      {
        case "0":
          return Calculator.CalculatorButtonType.Zero;
        case "1":
          return Calculator.CalculatorButtonType.One;
        case "2":
          return Calculator.CalculatorButtonType.Two;
        case "3":
          return Calculator.CalculatorButtonType.Three;
        case "4":
          return Calculator.CalculatorButtonType.Four;
        case "5":
          return Calculator.CalculatorButtonType.Five;
        case "6":
          return Calculator.CalculatorButtonType.Six;
        case "7":
          return Calculator.CalculatorButtonType.Seven;
        case "8":
          return Calculator.CalculatorButtonType.Eight;
        case "9":
          return Calculator.CalculatorButtonType.Nine;
        case "+":
          return Calculator.CalculatorButtonType.Add;
        case "-":
          return Calculator.CalculatorButtonType.Subtract;
        case "*":
          return Calculator.CalculatorButtonType.Multiply;
        case "/":
          return Calculator.CalculatorButtonType.Divide;
        case "%":
          return Calculator.CalculatorButtonType.Percent;
        case "\b":
          return Calculator.CalculatorButtonType.Back;
        case "\r":
        case "=":
          return Calculator.CalculatorButtonType.Equal;
      }

      //the check for the decimal is not in the switch statement. To help localize we check against the current culture's decimal seperator
      if( text == CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator )
        return Calculator.CalculatorButtonType.Decimal;

      //check for the escape key
      if( text == ( ( char )27 ).ToString() )
        return Calculator.CalculatorButtonType.Clear;

      return Calculator.CalculatorButtonType.None;
    }

    public static Button FindButtonByCalculatorButtonType( DependencyObject parent, Calculator.CalculatorButtonType type )
    {
      if( parent == null )
        return null;

      for( int i = 0; i < VisualTreeHelper.GetChildrenCount( parent ); i++ )
      {
        var child = VisualTreeHelper.GetChild( parent, i );
        if( child == null )
          continue;

        object buttonType = child.GetValue( Button.CommandParameterProperty );

        if( buttonType != null && ( Calculator.CalculatorButtonType )buttonType == type )
        {
          return child as Button;
        }
        else
        {
          var result = FindButtonByCalculatorButtonType( child, type );

          if( result != null )
            return result;
        }
      }
      return null;
    }

    public static string GetCalculatorButtonContent( Calculator.CalculatorButtonType type )
    {
      string content = string.Empty;
      switch( type )
      {
        case Calculator.CalculatorButtonType.Add:
          content = "+";
          break;
        case Calculator.CalculatorButtonType.Back:
          content = "Back";
          break;
        case Calculator.CalculatorButtonType.Cancel:
          content = "CE";
          break;
        case Calculator.CalculatorButtonType.Clear:
          content = "C";
          break;
        case Calculator.CalculatorButtonType.Decimal:
          content = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
          break;
        case Calculator.CalculatorButtonType.Divide:
          content = "/";
          break;
        case Calculator.CalculatorButtonType.Eight:
          content = "8";
          break;
        case Calculator.CalculatorButtonType.Equal:
          content = "=";
          break;
        case Calculator.CalculatorButtonType.Five:
          content = "5";
          break;
        case Calculator.CalculatorButtonType.Four:
          content = "4";
          break;
        case Calculator.CalculatorButtonType.Fraction:
          content = "1/x";
          break;
        case Calculator.CalculatorButtonType.MAdd:
          content = "M+";
          break;
        case Calculator.CalculatorButtonType.MC:
          content = "MC";
          break;
        case Calculator.CalculatorButtonType.MR:
          content = "MR";
          break;
        case Calculator.CalculatorButtonType.MS:
          content = "MS";
          break;
        case Calculator.CalculatorButtonType.MSub:
          content = "M-";
          break;
        case Calculator.CalculatorButtonType.Multiply:
          content = "*";
          break;
        case Calculator.CalculatorButtonType.Nine:
          content = "9";
          break;
        case Calculator.CalculatorButtonType.None:
          break;
        case Calculator.CalculatorButtonType.One:
          content = "1";
          break;
        case Calculator.CalculatorButtonType.Percent:
          content = "%";
          break;
        case Calculator.CalculatorButtonType.Seven:
          content = "7";
          break;
        case Calculator.CalculatorButtonType.Negate:
          content = "+/-";
          break;
        case Calculator.CalculatorButtonType.Six:
          content = "6";
          break;
        case Calculator.CalculatorButtonType.Sqrt:
          content = "Sqrt";
          break;
        case Calculator.CalculatorButtonType.Subtract:
          content = "-";
          break;
        case Calculator.CalculatorButtonType.Three:
          content = "3";
          break;
        case Calculator.CalculatorButtonType.Two:
          content = "2";
          break;
        case Calculator.CalculatorButtonType.Zero:
          content = "0";
          break;
      }
      return content;
    }

    public static bool IsDigit( Calculator.CalculatorButtonType buttonType )
    {
      switch( buttonType )
      {
        case Calculator.CalculatorButtonType.Zero:
        case Calculator.CalculatorButtonType.One:
        case Calculator.CalculatorButtonType.Two:
        case Calculator.CalculatorButtonType.Three:
        case Calculator.CalculatorButtonType.Four:
        case Calculator.CalculatorButtonType.Five:
        case Calculator.CalculatorButtonType.Six:
        case Calculator.CalculatorButtonType.Seven:
        case Calculator.CalculatorButtonType.Eight:
        case Calculator.CalculatorButtonType.Nine:
        case Calculator.CalculatorButtonType.Decimal:
          return true;
        default:
          return false;
      }
    }

    public static bool IsMemory( Calculator.CalculatorButtonType buttonType )
    {
      switch( buttonType )
      {
        case Calculator.CalculatorButtonType.MAdd:
        case Calculator.CalculatorButtonType.MC:
        case Calculator.CalculatorButtonType.MR:
        case Calculator.CalculatorButtonType.MS:
        case Calculator.CalculatorButtonType.MSub:
          return true;
        default:
          return false;
      }
    }

    public static decimal ParseDecimal( string text )
    {
      decimal result; 
      var success = Decimal.TryParse( text, NumberStyles.Any, CultureInfo.CurrentCulture, out result );
      return success ? result : decimal.Zero;      
    }

    public static decimal Add( decimal firstNumber, decimal secondNumber )
    {
      return firstNumber + secondNumber;
    }

    public static decimal Subtract( decimal firstNumber, decimal secondNumber )
    {
      return firstNumber - secondNumber;
    }

    public static decimal Multiply( decimal firstNumber, decimal secondNumber )
    {
      return firstNumber * secondNumber;
    }

    public static decimal Divide( decimal firstNumber, decimal secondNumber )
    {
      return firstNumber / secondNumber;
    }

    public static decimal Percent( decimal firstNumber, decimal secondNumber )
    {
      return firstNumber * secondNumber / 100M;
    }

    public static decimal SquareRoot( decimal operand )
    {
      return Convert.ToDecimal( Math.Sqrt( Convert.ToDouble( operand ) ) );
    }

    public static decimal Fraction( decimal operand )
    {
      return 1 / operand;
    }

    public static decimal Negate( decimal operand )
    {
      return operand * -1M;
    }
  }
}
