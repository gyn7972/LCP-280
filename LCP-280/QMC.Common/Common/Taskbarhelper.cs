using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

public static class TaskbarHelper
{
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid, out IPropertyStore propertyStore);

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    private interface IPropertyStore
    {
        int GetCount([Out] out uint cProps);
        int GetAt([In] uint iProp, out PropertyKey pkey);
        int GetValue([In] ref PropertyKey key, out PropVariant pv);
        int SetValue([In] ref PropertyKey key, [In] ref PropVariant pv);
        int Commit();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct PropertyKey
    {
        public Guid fmtid;
        public uint pid;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct PropVariant
    {
        [FieldOffset(0)] ushort vt;
        [FieldOffset(8)] IntPtr pointerValue;

        public static PropVariant FromString(string value)
        {
            var pv = new PropVariant();
            pv.vt = 31; // VT_LPWSTR
            pv.pointerValue = Marshal.StringToCoTaskMemUni(value);
            return pv;
        }
    }

    public static void SetAppId(IntPtr hwnd, string appId)
    {
        Guid iid = new Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");
        IPropertyStore propStore;
        SHGetPropertyStoreForWindow(hwnd, ref iid, out propStore);

        var key = new PropertyKey
        {
            fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"),
            pid = 5
        };
        var value = PropVariant.FromString(appId);
        propStore.SetValue(ref key, ref value);
        propStore.Commit();
    }
}
