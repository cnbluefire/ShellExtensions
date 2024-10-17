namespace ShellExtensions.Helpers
{
    public unsafe class ShellFolder : ShellItem
    {
        internal ShellFolder(Windows.Win32.UI.Shell.IShellItem* shellItem) : base(shellItem)
        {
        }

        public bool IsNormalFolder => Attributes.Folder && !Attributes.FileSystem && !Attributes.Stream;
    }

    public unsafe class ShellItem
    {
        private Windows.Win32.UI.Shell.IShellItem* shellItem;
        private ShellItemAttributes? attributes;

        internal ShellItem(Windows.Win32.UI.Shell.IShellItem* shellItem)
        {
            this.shellItem = shellItem;
            if (shellItem != null)
            {
                shellItem->AddRef();
            }
        }

        public nint NativeObject => (nint)shellItem;

        public string FullPath => GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEPARSING);

        public ShellItemAttributes Attributes => attributes ??= new ShellItemAttributes(shellItem);

        public string GetDisplayName(SIGDN sigdnName)
        {
            if (shellItem == null) return string.Empty;

            Windows.Win32.Foundation.PWSTR pDisplayName = default;
            try
            {
                shellItem->GetDisplayName((Windows.Win32.UI.Shell.SIGDN)(int)sigdnName, &pDisplayName);
                return pDisplayName.ToString();
            }
            finally
            {
                if (pDisplayName.Value != null)
                {
                    Windows.Win32.PInvoke.CoTaskMemFree(pDisplayName.Value);
                }
            }
        }

        public ShellItem? GetParent()
        {
            if (shellItem == null) return null;

            Windows.Win32.UI.Shell.IShellItem* parent = null;
            try
            {
                shellItem->GetParent(&parent);
                if (parent != null)
                {
                    return new ShellItem(parent);
                }
            }
            catch { }
            finally
            {
                if (parent != null) parent->Release();
            }

            return null;
        }

        ~ShellItem()
        {
            if (shellItem != null)
            {
                shellItem->Release();
                shellItem = null;
            }
        }


        public enum SIGDN
        {
            /// <summary>0x00000000. Returns the display name relative to the parent folder. In UI this name is generally ideal for display to the user.</summary>
            SIGDN_NORMALDISPLAY = 0,
            /// <summary>(int)0x80018001. Returns the parsing name relative to the parent folder. This name is not suitable for use in UI.</summary>
            SIGDN_PARENTRELATIVEPARSING = -2147385343,
            /// <summary>(int)0x80028000. Returns the parsing name relative to the desktop. This name is not suitable for use in UI.</summary>
            SIGDN_DESKTOPABSOLUTEPARSING = -2147319808,
            /// <summary>(int)0x80031001. Returns the editing name relative to the parent folder. In UI this name is suitable for display to the user.</summary>
            SIGDN_PARENTRELATIVEEDITING = -2147282943,
            /// <summary>(int)0x8004c000. Returns the editing name relative to the desktop. In UI this name is suitable for display to the user.</summary>
            SIGDN_DESKTOPABSOLUTEEDITING = -2147172352,
            /// <summary>(int)0x80058000. Returns the item's file system path, if it has one. Only items that report <a href="https://docs.microsoft.com/windows/desktop/shell/sfgao">SFGAO_FILESYSTEM</a> have a file system path. When an item does not have a file system path, a call to <a href="https://docs.microsoft.com/windows/desktop/api/shobjidl_core/nf-shobjidl_core-ishellitem-getdisplayname">IShellItem::GetDisplayName</a> on that item will fail. In UI this name is suitable for display to the user in some cases, but note that it might not be specified for all items.</summary>
            SIGDN_FILESYSPATH = -2147123200,
            /// <summary>(int)0x80068000. Returns the item's URL, if it has one. Some items do not have a URL, and in those cases a call to <a href="https://docs.microsoft.com/windows/desktop/api/shobjidl_core/nf-shobjidl_core-ishellitem-getdisplayname">IShellItem::GetDisplayName</a> will fail. This name is suitable for display to the user in some cases, but note that it might not be specified for all items.</summary>
            SIGDN_URL = -2147057664,
            /// <summary>(int)0x8007c001. Returns the path relative to the parent folder in a friendly format as displayed in an address bar. This name is suitable for display to the user.</summary>
            SIGDN_PARENTRELATIVEFORADDRESSBAR = -2146975743,
            /// <summary>(int)0x80080001. Returns the path relative to the parent folder.</summary>
            SIGDN_PARENTRELATIVE = -2146959359,
            /// <summary>(int)0x80094001. <b>Introduced in Windows 8</b>.</summary>
            SIGDN_PARENTRELATIVEFORUI = -2146877439,
        }

        public class ShellItemAttributes
        {
            private Windows.Win32.UI.Shell.IShellItem* shellItem;

            internal ShellItemAttributes(Windows.Win32.UI.Shell.IShellItem* shellItem)
            {
                this.shellItem = shellItem;
                if (shellItem != null)
                {
                    shellItem->AddRef();
                }
            }

            public bool CanCopy => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_CANCOPY);
            public bool CanMove => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_CANMOVE);
            public bool CanLink => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_CANLINK);
            public bool Storage => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_STORAGE);
            public bool CanRename => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_CANRENAME);
            public bool CanDelete => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_CANDELETE);
            public bool HasPropertySheet => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_HASPROPSHEET);
            public bool DropTarget => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_DROPTARGET);
            public bool System => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_SYSTEM);
            public bool Encrypted => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_ENCRYPTED);
            public bool IsSlow => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_ISSLOW);
            public bool Ghosted => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_GHOSTED);
            public bool Link => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_LINK);
            public bool Share => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_SHARE);
            public bool ReadOnly => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_READONLY);
            public bool Hidden => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_HIDDEN);
            public bool IsSystemFileAncestor => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_FILESYSANCESTOR);
            public bool Folder => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_FOLDER);
            public bool FileSystem => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_FILESYSTEM);
            public bool HasSubFolder => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_HASSUBFOLDER);
            public bool Removable => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_REMOVABLE);
            public bool Compressed => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_COMPRESSED);
            public bool Browsable => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_BROWSABLE);
            public bool Nonenumerated => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_NONENUMERATED);
            public bool NewContent => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_NEWCONTENT);
            public bool Stream => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_STREAM);
            public bool IsStorageAncestor => CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS.SFGAO_STORAGEANCESTOR);


            private bool CheckFlag(Windows.Win32.System.SystemServices.SFGAO_FLAGS flag)
            {
                if (shellItem == null) return false;
                try
                {
                    shellItem->GetAttributes(flag, out var result);
                    return (result & flag) != 0;
                }
                catch { }
                return false;
            }

            ~ShellItemAttributes()
            {
                if (shellItem != null)
                {
                    shellItem->Release();
                    shellItem = null;
                }
            }
        }
    }
}
