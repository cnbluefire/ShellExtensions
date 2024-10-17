using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions.Helpers
{
    internal unsafe class UIAutomationHelper
    {
        public static bool IsTreeItemFocused(int millisecondsTimeout = 2000)
        {
            var result = new bool[1] { false };
            var thread = new Thread(_result =>
            {
                Windows.Win32.UI.Accessibility.IUIAutomation* uia = null;
                Windows.Win32.UI.Accessibility.IUIAutomationElement* ele = null;
                try
                {
                    Windows.Win32.PInvoke.CoInitializeEx(Windows.Win32.System.Com.COINIT.COINIT_MULTITHREADED);

                    uia = CreateUIAutomation();
                    if (uia != null)
                    {
                        ele = uia->GetFocusedElement();
                        if (ele != null)
                        {
                            ((bool[])_result!)[0] = (ele->CurrentControlType) == Windows.Win32.UI.Accessibility.UIA_CONTROLTYPE_ID.UIA_TreeItemControlTypeId;
                            return;
                        }
                    }
                    ((bool[])_result!)[0] = false;
                }
                catch { }
                finally
                {
                    lock (_result!)
                    {
                        Monitor.Pulse(_result);
                    }

                    if (ele != null) ele->Release();
                    if (uia != null) uia->Release();

                    Windows.Win32.PInvoke.CoUninitialize();
                }
            });

            thread.Name = "Managed UIAutomation Thread";
            thread.IsBackground = true;
            thread.Start(result);

            lock (result)
            {
                Monitor.Wait(result, millisecondsTimeout);
            }
            thread.Join();
            return result[0];
        }

        private static Windows.Win32.UI.Accessibility.IUIAutomation* CreateUIAutomation()
        {
            Windows.Win32.UI.Accessibility.IUIAutomation* uiAutomation;

            var clsid = new Guid("ff48dba4-60ef-4201-aa87-54103eef594e");
            var iid = Windows.Win32.UI.Accessibility.IUIAutomation.IID_Guid;

            var hr = Windows.Win32.PInvoke.CoCreateInstance(
                &clsid,
                null,
                Windows.Win32.System.Com.CLSCTX.CLSCTX_INPROC_SERVER,
                &iid,
                (void**)&uiAutomation);

            if (hr.Succeeded) return uiAutomation;
            return null;
        }
    }
}
