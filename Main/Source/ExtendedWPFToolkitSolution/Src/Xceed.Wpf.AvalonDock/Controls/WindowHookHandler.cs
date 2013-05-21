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
using System.Linq;
using System.Text;
using System.Threading;

namespace Xceed.Wpf.AvalonDock.Controls
{
    class FocusChangeEventArgs : EventArgs
    {
        public FocusChangeEventArgs(IntPtr gotFocusWinHandle, IntPtr lostFocusWinHandle)
        {
            GotFocusWinHandle = gotFocusWinHandle;
            LostFocusWinHandle = lostFocusWinHandle;
        }

        public IntPtr GotFocusWinHandle
        {
            get;
            private set;
        }
        public IntPtr LostFocusWinHandle
        {
            get;
            private set;
        }
    }

    class WindowHookHandler
    {
        public WindowHookHandler()
        { 

        }

        IntPtr _windowHook;
        Win32Helper.HookProc _hookProc;
        public void Attach()
        {
            _hookProc = new Win32Helper.HookProc(this.HookProc);
            _windowHook = Win32Helper.SetWindowsHookEx(
                Win32Helper.HookType.WH_CBT,
                _hookProc,
                IntPtr.Zero,
                (int)Win32Helper.GetCurrentThreadId());
        }


        public void Detach()
        {
            Win32Helper.UnhookWindowsHookEx(_windowHook);
        }   

        public int HookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code == Win32Helper.HCBT_SETFOCUS)
            {
                if (FocusChanged != null)
                    FocusChanged(this, new FocusChangeEventArgs(wParam, lParam));
            }
            else if (code == Win32Helper.HCBT_ACTIVATE)
            {
                if (_insideActivateEvent.CanEnter)
                {
                    using (_insideActivateEvent.Enter())
                    {
                        //if (Activate != null)
                        //    Activate(this, new WindowActivateEventArgs(wParam));
                    }
                }
            }


            return Win32Helper.CallNextHookEx(_windowHook, code, wParam, lParam);
        }

        public event EventHandler<FocusChangeEventArgs> FocusChanged;

        //public event EventHandler<WindowActivateEventArgs> Activate;

        ReentrantFlag _insideActivateEvent = new ReentrantFlag();
    }
}
