using System.Collections;

namespace ShellExtensions.Helpers
{
    public unsafe class ShellItemArray : IReadOnlyList<ShellItem>
    {
        private Windows.Win32.UI.Shell.IShellItemArray* shellItemArray;
        private ShellItem[]? managedArray;

        internal ShellItemArray(Windows.Win32.UI.Shell.IShellItemArray* shellItemArray)
        {
            this.shellItemArray = shellItemArray;
            if (shellItemArray != null)
            {
                shellItemArray->AddRef();
            }
        }


        public unsafe ShellItem this[int index]
        {
            get
            {
                if (shellItemArray == null) throw new IndexOutOfRangeException();

                var count = Count;
                if (index < 0 || index >= count) throw new IndexOutOfRangeException();

                if (managedArray == null) managedArray = new ShellItem[Count];

                if (managedArray[index] == null)
                {
                    Windows.Win32.UI.Shell.IShellItem* shellItem = default;
                    shellItemArray->GetItemAt((uint)index, &shellItem);
                    try
                    {
                        managedArray[index] = new ShellItem(shellItem);
                    }
                    finally
                    {
                        shellItem->Release();
                    }
                }

                return managedArray[index];
            }
        }

        public int Count
        {
            get
            {
                if (shellItemArray == null) return 0;

                shellItemArray->GetCount(out var count);
                return (int)count;
            }
        }

        public IEnumerator<ShellItem> GetEnumerator()
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ShellItem>)this).GetEnumerator();
        }

        ~ShellItemArray()
        {
            if (shellItemArray != null)
            {
                shellItemArray->Release();
                shellItemArray = null;
            }
        }
    }
}
