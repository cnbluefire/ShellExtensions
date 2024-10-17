using ShellExtensions.ComObjects;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace ShellExtensions.Helpers
{
    internal static class ComHelpers
    {
        internal static StrategyBasedComWrappers ComWrappers { get; } = new StrategyBasedComWrappers();
    }
}
