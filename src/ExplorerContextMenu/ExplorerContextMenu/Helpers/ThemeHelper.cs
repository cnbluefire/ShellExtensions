using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerContextMenu.Helpers
{
    internal static class ThemeHelper
    {
        internal enum AppTheme
        {
            Dark = 0,
            Light = 1
        }

        internal static AppTheme GetAppTheme()
        {
            try
            {
                var subKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                if (subKey != null)
                {
                    if ((int)subKey.GetValue("AppsUseLightTheme", 1) is 0)
                    {
                        return AppTheme.Dark;
                    }
                }

            }
            catch { }
            return AppTheme.Light;
        }
    }
}
