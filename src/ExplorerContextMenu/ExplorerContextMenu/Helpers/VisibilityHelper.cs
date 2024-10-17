using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerContextMenu.Helpers
{
    internal class VisibilityHelper
    {
        private readonly ModelFactory factory;

        private string registryKey;

        public VisibilityHelper(ModelFactory factory)
        {
            this.factory = factory;

            registryKey = string.Empty;

            var tmpKey = factory.RootModel.ConfigRegistryKey;
            if (!string.IsNullOrEmpty(tmpKey))
            {
                tmpKey = tmpKey.Replace('/', '\\');
                var context = new ParameterContext();
                if (context.TryFormat(tmpKey, out var value))
                {
                    registryKey = value;
                }
            }
        }

        public bool TryGetValue(Guid guid, string name, out bool value)
        {
            return SettingsHelper.TryGetValue(Path.Combine(registryKey, guid.ToString("B")), name, out value);
        }
    }
}
