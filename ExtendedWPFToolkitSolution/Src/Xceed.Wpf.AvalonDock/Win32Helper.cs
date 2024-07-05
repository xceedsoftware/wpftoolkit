/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Xceed.Wpf.AvalonDock
{
  internal static class Win32Helper
  {
#pragma warning disable 618
    [DllImport( "user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode )]
    internal static extern IntPtr CreateWindowEx( int dwExStyle,
                                                  string lpszClassName,
                                                  string lpszWindowName,
                                                  int style,
                                                  int x, int y,
                                                  int width, int height,
                                                  IntPtr hwndParent,
                                                  IntPtr hMenu,
                                                  IntPtr hInst,
                                                  [MarshalAs( UnmanagedType.AsAny )] object pvParam );
#pragma warning restore 618
    internal const int
          WS_CHILD = 0x40000000,
          WS_VISIBLE = 0x10000000,
          WS_VSCROLL = 0x00200000,
          WS_BORDER = 0x00800000,
          WS_CLIPSIBLINGS = 0x04000000,
          WS_CLIPCHILDREN = 0x02000000,
          WS_TABSTOP = 0x00010000,
          WS_GROUP = 0x00020000;


    [Flags()]
    internal enum SetWindowPosFlags : uint
    {
      SynchronousWindowPosition = 0x4000,
      DeferErase = 0x2000,
      DrawFrame = 0x0020,
      FrameChanged = 0x0020,
      HideWindow = 0x0080,
      DoNotActivate = 0x0010,
      DoNotCopyBits = 0x0100,
      IgnoreMove = 0x0002,
      DoNotChangeOwnerZOrder = 0x0200,
      DoNotRedraw = 0x0008,
      DoNotReposition = 0x0200,
      DoNotSendChangingEvent = 0x0400,
      IgnoreResize = 0x0001,
      IgnoreZOrder = 0x0004,
      ShowWindow = 0x0040,
    }

    internal static readonly IntPtr HWND_TOPMOST = new IntPtr( -1 );
    internal static readonly IntPtr HWND_NOTOPMOST = new IntPtr( -2 );
    internal static readonly IntPtr HWND_TOP = new IntPtr( 0 );
    internal static readonly IntPtr HWND_BOTTOM = new IntPtr( 1 );

    [StructLayout( LayoutKind.Sequential )]
    internal class WINDOWPOS
    {
      public IntPtr hwnd;
      public IntPtr hwndInsertAfter;
      public int x;
      public int y;
      public int cx;
      public int cy;
      public int flags;
    };

    [DllImport( "user32.dll" )]
    [return: MarshalAs( UnmanagedType.Bool )]
    internal static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags );

    [DllImport( "user32.dll", CharSet = CharSet.Auto, ExactSpelling = true )]
    internal static extern bool IsChild( IntPtr hWndParent, IntPtr hwnd );

    [DllImport( "user32.dll" )]
    internal static extern IntPtr SetFocus( IntPtr hWnd );

    internal const int NCCALCSIZE = 0x0083;

    internal const int WM_WINDOWPOSCHANGED = 0x0047;
    internal const int WM_WINDOWPOSCHANGING = 0x0046;
    internal const int WM_NCMOUSEMOVE = 0xa0;
    internal const int WM_NCLBUTTONDOWN = 0xA1;
    internal const int WM_NCLBUTTONUP = 0xA2;
    internal const int WM_NCLBUTTONDBLCLK = 0xA3;
    internal const int WM_NCRBUTTONDOWN = 0xA4;
    internal const int WM_NCRBUTTONUP = 0xA5;
    internal const int WM_CAPTURECHANGED = 0x0215;
    internal const int WM_EXITSIZEMOVE = 0x0232;
    internal const int WM_ENTERSIZEMOVE = 0x0231;
    internal const int WM_MOVE = 0x0003;
    internal const int WM_MOVING = 0x0216;
    internal const int WM_KILLFOCUS = 0x0008;
    internal const int WM_SETFOCUS = 0x0007;
    internal const int WM_ACTIVATE = 0x0006;
    internal const int WM_NCHITTEST = 0x0084;
    internal const int WM_INITMENUPOPUP = 0x0117;
    internal const int WM_KEYDOWN = 0x0100;
    internal const int WM_KEYUP = 0x0101;

    internal const int WA_INACTIVE = 0x0000;

    internal const int WM_SYSCOMMAND = 0x0112;
    // These are the wParam of WM_SYSCOMMAND
    internal const int SC_MAXIMIZE = 0xF030;
    internal const int SC_RESTORE = 0xF120;

    internal const int
        WM_CREATE = 0x0001;

    [DllImport( "user32.dll", SetLastError = true )]
    public static extern IntPtr SetActiveWindow( IntPtr hWnd );

    [DllImport( "user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode )]
    internal static extern bool DestroyWindow( IntPtr hwnd );

    internal const int HT_CAPTION = 0x2;

    [DllImportAttribute( "user32.dll" )]
    internal static extern int SendMessage( IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam );
    [DllImportAttribute( "user32.dll" )]
    internal static extern int PostMessage( IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam );


    [DllImport( "user32.dll" )]
    static extern bool GetClientRect( IntPtr hWnd, out RECT lpRect );
    [DllImport( "user32.dll" )]
    internal static extern bool GetWindowRect( IntPtr hWnd, out RECT lpRect );

    // Hook Types  
    public enum HookType : int
    {
      WH_JOURNALRECORD = 0,
      WH_JOURNALPLAYBACK = 1,
      WH_KEYBOARD = 2,
      WH_GETMESSAGE = 3,
      WH_CALLWNDPROC = 4,
      WH_CBT = 5,
      WH_SYSMSGFILTER = 6,
      WH_MOUSE = 7,
      WH_HARDWARE = 8,
      WH_DEBUG = 9,
      WH_SHELL = 10,
      WH_FOREGROUNDIDLE = 11,
      WH_CALLWNDPROCRET = 12,
      WH_KEYBOARD_LL = 13,
      WH_MOUSE_LL = 14
    }

    public const int HCBT_SETFOCUS = 9;
    public const int HCBT_ACTIVATE = 5;

    [DllImport( "kernel32.dll" )]
    public static extern uint GetCurrentThreadId();

    public delegate int HookProc( int code, IntPtr wParam,
       IntPtr lParam );

    [DllImport( "user32.dll" )]
    public static extern IntPtr SetWindowsHookEx( HookType code,
        HookProc func,
        IntPtr hInstance,
        int threadID );
    [DllImport( "user32.dll" )]
    public static extern int UnhookWindowsHookEx( IntPtr hhook );
    [DllImport( "user32.dll" )]
    public static extern int CallNextHookEx( IntPtr hhook,
        int code, IntPtr wParam, IntPtr lParam );

    [Serializable, StructLayout( LayoutKind.Sequential )]
    internal struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;
      public RECT( int left_, int top_, int right_, int bottom_ )
      {
        Left = left_;
        Top = top_;
        Right = right_;
        Bottom = bottom_;
      }

      public int Height
      {
        get
        {
          return Bottom - Top;
        }
      }

      public int Width
      {
        get
        {
          return Right - Left;
        }
      }
      public Size Size
      {
        get
        {
          return new Size( Width, Height );
        }
      }
      public Point Location
      {
        get
        {
          return new Point( Left, Top );
        }
      }
      // Handy method for converting to a System.Drawing.Rectangle  
      public Rect ToRectangle()
      {
        return new Rect( Left, Top, Right, Bottom );
      }
      public static RECT FromRectangle( Rect rectangle )
      {
        return new Rect( rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom );
      }
      public override int GetHashCode()
      {
        return Left ^ ( ( Top << 13 ) | ( Top >> 0x13 ) ) ^ ( ( Width << 0x1a ) | ( Width >> 6 ) ) ^ ( ( Height << 7 ) | ( Height >> 0x19 ) );
      }

      #region Operator overloads
      public static implicit operator Rect( RECT rect )
      {
        return rect.ToRectangle();
      }
      public static implicit operator RECT( Rect rect )
      {
        return FromRectangle( rect );
      }
      #endregion
    }

    internal static RECT GetClientRect( IntPtr hWnd )
    {
      RECT result = new RECT();
      GetClientRect( hWnd, out result );
      return result;
    }
    internal static RECT GetWindowRect( IntPtr hWnd )
    {
      RECT result = new RECT();
      GetWindowRect( hWnd, out result );
      return result;
    }

    [DllImport( "user32.dll" )]
    internal static extern IntPtr GetTopWindow( IntPtr hWnd );

    internal const uint GW_HWNDNEXT = 2;
    internal const uint GW_HWNDPREV = 3;


    [DllImport( "user32.dll", SetLastError = true )]
    internal static extern IntPtr GetWindow( IntPtr hWnd, uint uCmd );

    internal enum GetWindow_Cmd : uint
    {
      GW_HWNDFIRST = 0,
      GW_HWNDLAST = 1,
      GW_HWNDNEXT = 2,
      GW_HWNDPREV = 3,
      GW_OWNER = 4,
      GW_CHILD = 5,
      GW_ENABLEDPOPUP = 6
    }

    internal static int MakeLParam( int LoWord, int HiWord )
    {
      return ( int )( ( HiWord << 16 ) | ( LoWord & 0xffff ) );
    }


    internal const int WM_MOUSEMOVE = 0x200;
    internal const int WM_LBUTTONDOWN = 0x201;
    internal const int WM_LBUTTONUP = 0x202;
    internal const int WM_LBUTTONDBLCLK = 0x203;
    internal const int WM_RBUTTONDOWN = 0x204;
    internal const int WM_RBUTTONUP = 0x205;
    internal const int WM_RBUTTONDBLCLK = 0x206;
    internal const int WM_MBUTTONDOWN = 0x207;
    internal const int WM_MBUTTONUP = 0x208;
    internal const int WM_MBUTTONDBLCLK = 0x209;
    internal const int WM_MOUSEWHEEL = 0x20A;
    internal const int WM_MOUSEHWHEEL = 0x20E;


    [DllImport( "user32.dll" )]
    [return: MarshalAs( UnmanagedType.Bool )]
    internal static extern bool GetCursorPos( ref Win32Point pt );

    [StructLayout( LayoutKind.Sequential )]
    internal struct Win32Point
    {
      public Int32 X;
      public Int32 Y;
    };
    internal static Point GetMousePosition()
    {
      Win32Point w32Mouse = new Win32Point();
      GetCursorPos( ref w32Mouse );
      return new Point( w32Mouse.X, w32Mouse.Y );
    }


    [DllImport( "user32.dll" )]
    [return: MarshalAs( UnmanagedType.Bool )]
    internal static extern bool IsWindowVisible( IntPtr hWnd );
    [DllImport( "user32.dll" )]
    [return: MarshalAs( UnmanagedType.Bool )]
    internal static extern bool IsWindowEnabled( IntPtr hWnd );

    [DllImport( "user32.dll" )]
    internal static extern IntPtr GetFocus();

    [DllImport( "user32.dll" )]
    [return: MarshalAs( UnmanagedType.Bool )]
    internal static extern bool BringWindowToTop( IntPtr hWnd );

    [DllImport( "user32.dll", SetLastError = true )]
    internal static extern IntPtr SetParent( IntPtr hWndChild, IntPtr hWndNewParent );

    [DllImport( "user32.dll", ExactSpelling = true, CharSet = CharSet.Auto )]
    internal static extern IntPtr GetParent( IntPtr hWnd );

    [DllImport( "user32.dll" )]
    static extern int SetWindowLong( IntPtr hWnd, int nIndex, int dwNewLong );

    [DllImport( "user32.dll", SetLastError = true )]
    static extern int GetWindowLong( IntPtr hWnd, int nIndex );

    public static void SetOwner( IntPtr childHandle, IntPtr ownerHandle )
    {
      SetWindowLong(
          childHandle,
          -8, // GWL_HWNDPARENT
          ownerHandle.ToInt32() );
    }

    public static IntPtr GetOwner( IntPtr childHandle )
    {
      return new IntPtr( GetWindowLong( childHandle, -8 ) );
    }


    //Monitor Patch #13440

    [DllImport( "user32.dll" )]
    public static extern IntPtr MonitorFromRect( [In] ref RECT lprc, uint dwFlags );

    [DllImport( "user32.dll" )]
    public static extern IntPtr MonitorFromWindow( IntPtr hwnd, uint dwFlags );


    [StructLayout( LayoutKind.Sequential )]
    public class MonitorInfo
    {
      public int Size = Marshal.SizeOf( typeof( MonitorInfo ) );
      public RECT Monitor;
      public RECT Work;
      public uint Flags;
    }

    [DllImport( "user32.dll" )]
    [return: MarshalAs( UnmanagedType.Bool )]
    public static extern bool GetMonitorInfo( IntPtr hMonitor, [In, Out] MonitorInfo lpmi );

  }
}
