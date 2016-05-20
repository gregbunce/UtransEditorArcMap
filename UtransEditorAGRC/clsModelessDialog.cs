using System;

namespace UtransEditorAGRC
{
    class clsModelessDialog : System.Windows.Forms.IWin32Window
    {
        //this allows a dialog box to be visible with the host application

        private System.IntPtr hwnd;
        public System.IntPtr Handle
        {
            get { return hwnd; }
        }

        public clsModelessDialog(System.IntPtr handle)
        {
            hwnd = handle;
        }

        public clsModelessDialog(int handle)
        {
            hwnd = (IntPtr)handle;
        }

    }
}
