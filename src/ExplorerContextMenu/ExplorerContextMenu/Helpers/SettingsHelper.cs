using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerContextMenu.Helpers
{
    internal static class SettingsHelper
    {
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(string))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(char))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Guid))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(bool))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(long))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ulong))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(int))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(uint))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(byte))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(sbyte))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(short))]
        //[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ushort))]
        static SettingsHelper() { }

        internal static bool DeleteKey(string registryKey)
        {
            if (string.IsNullOrEmpty(registryKey)) return false;

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(registryKey, true);
                return true;
            }
            catch { }

            return false;
        }

        internal static bool SetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(string registryKey, string name, T? value)
        {
            if (string.IsNullOrEmpty(registryKey) || string.IsNullOrEmpty(name)) return false;

            var boxValue = BoxValue(value, out var valueKind);
            if (boxValue != null)
            {
                try
                {
                    using var subKey = Registry.CurrentUser.CreateSubKey(registryKey, true);
                    if (subKey != null)
                    {
                        subKey.SetValue(name, boxValue, valueKind);
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        internal static bool UnsetValue(string registryKey, string name)
        {
            using var subKey = Registry.CurrentUser.OpenSubKey(registryKey, true);
            if (subKey != null)
            {
                try
                {
                    subKey.DeleteValue(name, true);
                    return true;
                }
                catch { }
            }
            return true;
        }

        internal static bool TryGetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(string registryKey, string name, [NotNullWhen(true)] out T? value)
        {
            value = default;

            using var subKey = Registry.CurrentUser.OpenSubKey(registryKey, true);
            if (subKey != null)
            {
                try
                {
                    var raw = subKey.GetValue(name);
                    return TryUnboxValue(raw, out value);
                }
                catch { }
            }
            return false;
        }

        private static bool TryUnboxValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(object? raw, [NotNullWhen(true)] out T? value)
        {
            value = default;

            if (raw is null) return false;
            if (raw is T _value) { value = _value; return true; }

            if (typeof(T) == typeof(bool))
            {
                if (raw is string valueString)
                {
                    value = (T)(object)(!string.IsNullOrEmpty(valueString)
                        && (string.Equals(valueString, "true", StringComparison.OrdinalIgnoreCase)
                            || !string.Equals(valueString, "0", StringComparison.OrdinalIgnoreCase)));
                    return true;
                }
                else if (raw is int valueInt) { value = (T)(object)(valueInt != 0); return true; }
                else if (raw is long valueLong) { value = (T)(object)(valueLong != 0); return true; }
                else return false;
            }


            if (typeof(T) == typeof(Guid))
            {
                if (Guid.TryParse(raw.ToString(), out var value2))
                {
                    value = (T)(object)value2;
                    return true;
                }
                return false;
            }

            if (typeof(T) == typeof(string))
            {
                var str = raw.ToString();
                if (str != null)
                {
                    value = (T)(object)str;
                    return true;
                }
            }

            if (typeof(T) == typeof(long)
                || typeof(T) == typeof(ulong)
                || typeof(T) == typeof(int)
                || typeof(T) == typeof(uint)
                || typeof(T) == typeof(byte)
                || typeof(T) == typeof(sbyte)
                || typeof(T) == typeof(short)
                || typeof(T) == typeof(ushort)
                || typeof(T) == typeof(char))
            {
                try
                {
                    var longValue = Convert.ChangeType(raw, typeof(long));
                    if (typeof(T) == typeof(long)) { value = (T)longValue; return true; }
                    else if (typeof(T) == typeof(ulong)) { value = (T)(object)unchecked((ulong)longValue); return true; }
                    else if (typeof(T) == typeof(int)) { value = (T)(object)unchecked((int)longValue); return true; }
                    else if (typeof(T) == typeof(uint)) { value = (T)(object)unchecked((uint)longValue); return true; }
                    else if (typeof(T) == typeof(byte)) { value = (T)(object)unchecked((byte)longValue); return true; }
                    else if (typeof(T) == typeof(sbyte)) { value = (T)(object)unchecked((sbyte)longValue); return true; }
                    else if (typeof(T) == typeof(short)) { value = (T)(object)unchecked((short)longValue); return true; }
                    else if (typeof(T) == typeof(ushort)) { value = (T)(object)unchecked((ushort)longValue); return true; }
                    else if (typeof(T) == typeof(char)) { value = (T)(object)unchecked((char)longValue); return true; }
                }
                catch { }

                try
                {
                    var ulongValue = Convert.ChangeType(raw, typeof(ulong));
                    if (typeof(T) == typeof(ulong)) { value = (T)ulongValue; return true; }
                    else if (typeof(T) == typeof(long)) { value = (T)(object)unchecked((long)ulongValue); return true; }
                    else if (typeof(T) == typeof(int)) { value = (T)(object)unchecked((int)ulongValue); return true; }
                    else if (typeof(T) == typeof(uint)) { value = (T)(object)unchecked((uint)ulongValue); return true; }
                    else if (typeof(T) == typeof(byte)) { value = (T)(object)unchecked((byte)ulongValue); return true; }
                    else if (typeof(T) == typeof(sbyte)) { value = (T)(object)unchecked((sbyte)ulongValue); return true; }
                    else if (typeof(T) == typeof(short)) { value = (T)(object)unchecked((short)ulongValue); return true; }
                    else if (typeof(T) == typeof(ushort)) { value = (T)(object)unchecked((ushort)ulongValue); return true; }
                    else if (typeof(T) == typeof(char)) { value = (T)(object)unchecked((char)ulongValue); return true; }
                }
                catch { }
            }

            return false;
        }

        private static object? BoxValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(T? value, out RegistryValueKind valueKind)
        {
            valueKind = RegistryValueKind.Unknown;

            if (typeof(T) == typeof(string))
            {
                valueKind = RegistryValueKind.String;
                return value;
            }
            else if (typeof(T) == typeof(long)
                || typeof(T) == typeof(ulong))
            {
                valueKind = RegistryValueKind.QWord;
                return unchecked((uint)Convert.ToInt64(value));
            }
            else if (typeof(T) == typeof(bool))
            {
                return value is true ? "1" : "0";
            }
            else if (typeof(T) == typeof(int)
                || typeof(T) == typeof(uint)
                || typeof(T) == typeof(byte)
                || typeof(T) == typeof(sbyte)
                || typeof(T) == typeof(short)
                || typeof(T) == typeof(ushort)
                || typeof(T) == typeof(char))
            {
                valueKind = RegistryValueKind.DWord;
                return unchecked((uint)Convert.ToInt32(value));
            }
            else if (typeof(T) == typeof(Guid))
            {
                valueKind = RegistryValueKind.String;

                if (value is Guid guid) return guid.ToString("B");
                else return "{00000000-0000-0000-0000-000000000000}";
            }

            return null;
        }
    }
}
