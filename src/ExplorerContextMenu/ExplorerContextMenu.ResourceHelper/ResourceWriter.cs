using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExplorerContextMenu.ResourceHelper
{
    public unsafe static class ResourceWriter
    {
        private const ushort RT_STRING = 6;
        private const ushort RT_ICON = 3;
        private const ushort RT_GROUP_ICON = RT_ICON + 11;

        public static void WriteResource(IntPtr hResource, string resourceFilePath)
        {
            var json = System.IO.File.ReadAllText(resourceFilePath, Encoding.UTF8);
            var model = JsonSerializer.Deserialize<Models.ExplorerContextMenuModel>(json, JsonModelContext.Default.ExplorerContextMenuModel);

            if (model != null
                && model.MenuItems != null
                && model.MenuItems.Count > 0
                && NormalizeItems(model.MenuItems, out var icons))
            {
                if (string.IsNullOrEmpty(model.ConfigRegistryKey))
                {
                    model.ConfigRegistryKey = Models.ExplorerContextMenuModel.DefaultConfigRegistryKey;
                }

                if (icons != null && icons.Count > 0)
                {
                    ushort iconId = 20;

                    for (int i = 0; i < model.MenuItems.Count; i++)
                    {
                        var item = model.MenuItems[i];
                        if (item.Icon != null)
                        {
                            var _id = iconId;
                            try
                            {
                                WriteIconModelResource(hResource, item.Guid, item.Icon, icons, false, ref iconId);
                            }
                            catch
                            {
                                iconId = _id;
                                item.Icon = null;
                                throw;
                            }
                        }
                        if (item.DarkIcon != null)
                        {
                            var _id = iconId;
                            try
                            {
                                WriteIconModelResource(hResource, item.Guid, item.DarkIcon, icons, true, ref iconId);
                            }
                            catch
                            {
                                iconId = _id;
                                item.DarkIcon = null;
                                throw;
                            }
                        }
                    }
                }

                var json2 = System.Text.Json.JsonSerializer.Serialize(model, JsonModelContext.Default.ExplorerContextMenuModel);

                if (string.IsNullOrEmpty(json2))
                {
                    throw new ArgumentException("Serialize failed.");
                }
                WriteJsonResource(hResource, json2, 1);
            }
        }

        private static bool ProcessModel(
            Models.ExplorerContextMenuModel model,
            [NotNullWhen(true)] out Dictionary<string, (ushort icon, ushort darkIcon)>? icons)
        {
            icons = null;

            if (model != null
                && model.MenuItems != null
                && model.MenuItems.Count > 0
                && NormalizeItems(model.MenuItems, out icons))
            {
                return true;
            }
            return false;
        }

        private static bool NormalizeItems(
            List<Models.ExplorerContextMenuItemModel> items,
            [NotNullWhen(true)] out Dictionary<string, (ushort icon, ushort darkIcon)>? icons)
        {
            icons = null;
            ushort globalId = 200;

            var dict = new Dictionary<string, Models.ExplorerContextMenuItemModel>();
            foreach (var item in items)
            {
                if (!Guid.TryParse(item?.Guid, out var guid)) return false;
                var nGuid = guid.ToString("N");
                if (dict.ContainsKey(nGuid)) return false;
                dict.Add(nGuid, item);
                item.Guid = nGuid;

                if (item.SubMenuItems != null)
                {
                    for (int i = 0; i < item.SubMenuItems.Count; i++)
                    {
                        if (Guid.TryParse(item.SubMenuItems[i], out var subGuid))
                        {
                            item.SubMenuItems[i] = subGuid.ToString("N");
                        }
                    }
                }
            }

            var parents = new HashSet<string>();
            foreach (var item in dict)
            {
                parents.Clear();
                if (CheckRecursion(item.Value, dict, parents)) return false;
            }

            icons = new Dictionary<string, (ushort icon, ushort darkIcon)>();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                ushort iconId = 0;
                ushort darkIconId = 0;

                NormalizeIcon(item.Icon, ref globalId, out iconId);
                NormalizeIcon(item.DarkIcon, ref globalId, out darkIconId);

                if (iconId != 0 || darkIconId != 0)
                {
                    icons[item.Guid] = (iconId, darkIconId);
                }
            }

            return true;

            void NormalizeIcon(Models.ExplorerContextMenuItemIconModel _iconModel, ref ushort _globalId, out ushort _iconId)
            {
                _iconId = 0;

                if (_iconModel == null) return;

                if (string.IsNullOrEmpty(_iconModel.IconFile))
                {
                    if (!string.IsNullOrEmpty(_iconModel.EmbeddedIconFile)
                        && string.Equals(System.IO.Path.GetExtension(_iconModel.EmbeddedIconFile), ".ico", StringComparison.OrdinalIgnoreCase)
                        && System.IO.File.Exists(_iconModel.EmbeddedIconFile))
                    {
                        _iconId = _globalId++;
                    }
                    else if (_iconModel.EmbeddedPngFiles != null
                        && _iconModel.EmbeddedPngFiles.Count > 0)
                    {
                        var embeddedPngs = new Dictionary<string, string>();
                        foreach (var item in _iconModel.EmbeddedPngFiles)
                        {
                            if (item.Key == "256"
                                || item.Key == "64"
                                || item.Key == "48"
                                || item.Key == "40"
                                || item.Key == "32"
                                || item.Key == "24"
                                || item.Key == "20"
                                || item.Key == "16")
                            {
                                if (!string.IsNullOrEmpty(item.Value)
                                    && string.Equals(System.IO.Path.GetExtension(item.Value), ".png", StringComparison.OrdinalIgnoreCase)
                                    && System.IO.File.Exists(item.Value))
                                {
                                    embeddedPngs[item.Key] = item.Value;
                                }
                            }
                        }
                        if (embeddedPngs.Count > 0)
                        {
                            _iconModel.EmbeddedPngFiles = embeddedPngs;
                            _iconId = _globalId++;
                        }
                        else
                        {
                            _iconModel.EmbeddedPngFiles = null;
                        }
                    }

                    if (_iconId != 0)
                    {
                        _iconModel.IconFile = $"\"{{dllPath}}\",{_iconId * -1}";
                    }
                }
                else
                {
                    _iconModel.EmbeddedIconFile = null;
                    _iconModel.EmbeddedPngFiles = null;
                }
            }

            bool CheckRecursion(
                Models.ExplorerContextMenuItemModel _model,
                Dictionary<string, Models.ExplorerContextMenuItemModel> _dict,
                HashSet<string> _parents)
            {
                if (_model == null || _model.SubMenuItems == null || _model.SubMenuItems.Count == 0) return false;

                _parents.Add(_model.Guid);
                for (int i = _model.SubMenuItems.Count - 1; i >= 0; i--)
                {
                    if (_dict.TryGetValue(_model.SubMenuItems[i], out var _subItem))
                    {
                        if (_parents.Contains(_subItem.Guid)) return true;

                        if (CheckRecursion(_subItem, _dict, _parents)) return true;
                    }
                    else if (!IsSeparatorGuid(_model.SubMenuItems[i]))
                    {
                        _model.SubMenuItems.RemoveAt(i);
                    }
                }

                return false;
            }

            bool IsSeparatorGuid(string _guid)
            {
                if (_guid != null && _guid.Length >= 3)
                {
                    var first = _guid[0];
                    bool flag = IsSeparatorChar(first);
                    for (int j = 1; flag && j < _guid.Length; j++)
                    {
                        flag = _guid[j] == first && IsSeparatorChar(_guid[j]);
                    }

                    if (flag) return flag;
                }

                return _guid == "00000000-0000-0000-0000-000000000000"
                    || _guid == "11111111-1111-1111-1111-111111111111"
                    || _guid == "22222222-2222-2222-2222-222222222222"
                    || _guid == "33333333-3333-3333-3333-333333333333"
                    || _guid == "44444444-4444-4444-4444-444444444444"
                    || _guid == "55555555-5555-5555-5555-555555555555"
                    || _guid == "66666666-6666-6666-6666-666666666666"
                    || _guid == "77777777-7777-7777-7777-777777777777"
                    || _guid == "88888888-8888-8888-8888-888888888888"
                    || _guid == "99999999-9999-9999-9999-999999999999"
                    || _guid == "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
                    || _guid == "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
                    || _guid == "cccccccc-cccc-cccc-cccc-cccccccccccc"
                    || _guid == "dddddddd-dddd-dddd-dddd-dddddddddddd"
                    || _guid == "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"
                    || _guid == "ffffffff-ffff-ffff-ffff-ffffffffffff";
            }

            bool IsSeparatorChar(char _ch) => _ch == '*' || _ch == '-' || _ch == '_';
        }

        private static bool WriteJsonResource(IntPtr hResource, string json, ushort sectionId)
        {
            var bytes = new byte[json.Length * sizeof(char) + sizeof(ushort)];

            var span = bytes.AsSpan();

            MemoryMarshal.Cast<byte, ushort>(span)[0] = (ushort)json.Length;
            json.AsSpan().CopyTo(MemoryMarshal.Cast<byte, char>(span.Slice(sizeof(ushort))));

            fixed (byte* lpData = span)
            {
                return NativeMethods.UpdateResource(
                    hResource,
                    RT_STRING,
                    sectionId,
                    0,
                    lpData,
                    (uint)bytes.Length);
            }
        }

        private static void WriteIconModelResource(IntPtr hResource, string guid, Models.ExplorerContextMenuItemIconModel icon, Dictionary<string, (ushort iconId, ushort darkIconId)> icons, bool isDarkIcon, ref ushort iconId)
        {
            if (icon == null || icons == null || string.IsNullOrEmpty(guid)) throw new ArgumentException("Write icon resource failed.");

            ushort iconGroupId = 0;

            if (icons.TryGetValue(guid, out var ids))
            {
                if (isDarkIcon) iconGroupId = ids.darkIconId;
                else iconGroupId = ids.iconId;
            }

            if (iconGroupId == 0) throw new ArgumentException("Write icon resource failed.");

            var embeddedIcon = icon.EmbeddedIconFile;
            var embeddedPngs = icon.EmbeddedPngFiles;

            icon.EmbeddedIconFile = null;
            icon.EmbeddedPngFiles = null;

            if (!string.IsNullOrEmpty(embeddedIcon))
            {
                using (var fileStream = new System.IO.FileStream(embeddedIcon, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var _id = iconId;
                    try
                    {
                        WriteIconResource(hResource, iconGroupId, ref iconId, fileStream);
                    }
                    catch
                    {
                        iconId = _id;
                        throw;
                    }
                }
            }
            else if (embeddedPngs != null && embeddedPngs.Count > 0)
            {
                var imageData = new IconHelper.ImageData[embeddedPngs.Count];
                int idx = 0;

                { if (embeddedPngs.TryGetValue("256", out var path)) { imageData[idx++] = new IconHelper.ImageData(256, System.Drawing.Image.FromFile(path)); } }
                { if (embeddedPngs.TryGetValue("64", out var path)) { imageData[idx++] = new IconHelper.ImageData(64, System.Drawing.Image.FromFile(path)); } }
                { if (embeddedPngs.TryGetValue("48", out var path)) { imageData[idx++] = new IconHelper.ImageData(48, System.Drawing.Image.FromFile(path)); } }
                { if (embeddedPngs.TryGetValue("40", out var path)) { imageData[idx++] = new IconHelper.ImageData(40, System.Drawing.Image.FromFile(path)); } }
                { if (embeddedPngs.TryGetValue("32", out var path)) { imageData[idx++] = new IconHelper.ImageData(32, System.Drawing.Image.FromFile(path)); } }
                { if (embeddedPngs.TryGetValue("24", out var path)) { imageData[idx++] = new IconHelper.ImageData(24, System.Drawing.Image.FromFile(path)); } }
                { if (embeddedPngs.TryGetValue("20", out var path)) { imageData[idx++] = new IconHelper.ImageData(20, System.Drawing.Image.FromFile(path)); } }
                { if (embeddedPngs.TryGetValue("16", out var path)) { imageData[idx++] = new IconHelper.ImageData(16, System.Drawing.Image.FromFile(path)); } }

                using (var stream = new System.IO.MemoryStream())
                {
                    var _id = iconId;
                    try
                    {
                        IconHelper.ConvertImagesToIcon(imageData, stream);
                        stream.Position = 0;
                        WriteIconResource(hResource, iconGroupId, ref iconId, stream);
                    }
                    catch
                    {
                        iconId = _id;
                        throw;
                    }
                }
            }
        }

        private static void WriteIconResource(IntPtr hResource, ushort iconGroupId, ref ushort iconId, System.IO.Stream iconStream)
        {
            var id = iconId;
            if (IconHelper.GetIconGroupResourceData(iconStream, entry => id++, out var groupData, out var iconData))
            {
                iconId = id;

                bool res = false;
                fixed (byte* lpData = groupData)
                {
                    res = NativeMethods.UpdateResource(
                        hResource,
                        RT_GROUP_ICON,
                        iconGroupId,
                        0,
                        lpData,
                        (uint)groupData.Length);

                    if (!res)
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }
                }

                for (int i = 0; i < iconData.Length; i++)
                {
                    fixed (byte* lpData = iconData[i].Data)
                    {
                        res = NativeMethods.UpdateResource(
                            hResource,
                            RT_ICON,
                            iconData[i].Id,
                            0,
                            lpData,
                            (uint)iconData[i].Data.Length);

                        if (!res)
                        {
                            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException("Write icon resource failed.");
            }
        }

        private static string? GetJsonText(string resourceFilePath)
        {
            //var model = new Models.ExplorerContextMenuModel()
            //{
            //    MenuItems = new List<Models.ExplorerContextMenuItemModel>()
            //    {
            //        new Models.ExplorerContextMenuItemModel()
            //        {
            //            Guid = "67691CB3-DB2C-4141-B168-E8110DCDB236",
            //            ExecuteOptions = new Models.ExplorerContextMenuItemExecuteOptions()
            //            {
            //                Command = "notepad.exe {item}",
            //            },
            //            HasMultipleItemsSupport = false,
            //            Title = new Dictionary<string, string>()
            //            {
            //                ["en"] = "Test 123",
            //                ["zh-Hans"] = "测试 123"
            //            },
            //            Icon = new Models.ExplorerContextMenuItemIconModel()
            //            {
            //                IconFile = "C:\\Users\\blue-fire\\Downloads\\1\\test.ico",
            //            },
            //            //SubMenuItems = new List<string>()
            //            //{
            //            //    "27A1F671-03D9-43B1-8991-CBC24D79B385",
            //            //    "50E01E94-ACA0-4624-B666-8ECF08B1CEA6",
            //            //    "A78879BA-CDE7-452C-A003-BBEEE9A8E450"
            //            //},
            //        },
            //        new Models.ExplorerContextMenuItemModel()
            //        {
            //            Guid = "27A1F671-03D9-43B1-8991-CBC24D79B385",
            //            Title = new Dictionary<string, string>()
            //            {
            //                ["en"] = "Test1"
            //            },
            //            CheckOptions = new Models.ExplorerContextMenuItemCheckOptions()
            //            {
            //                IsCheckable = true,
            //                CheckType = Models.ExplorerContextMenuItemCheckType.RadioCheck,
            //                GroupName = "Test",
            //                DefaultChecked = true,
            //            },
            //            HasMultipleItemsSupport = false
            //        },
            //        new Models.ExplorerContextMenuItemModel()
            //        {
            //            Guid = "50E01E94-ACA0-4624-B666-8ECF08B1CEA6",
            //            Title = new Dictionary<string, string>()
            //            {
            //                ["en"] = "Test2"
            //            },
            //            CheckOptions = new Models.ExplorerContextMenuItemCheckOptions()
            //            {
            //                IsCheckable = true,
            //                CheckType = Models.ExplorerContextMenuItemCheckType.Check,
            //                GroupName = "Test"
            //            },
            //            HasFolderSupport = false
            //        },
            //        new Models.ExplorerContextMenuItemModel()
            //        {
            //            Guid = "A78879BA-CDE7-452C-A003-BBEEE9A8E450",
            //            Title = new Dictionary<string, string>()
            //            {
            //                ["en"] = "Test3"
            //            },
            //            CheckOptions = new Models.ExplorerContextMenuItemCheckOptions()
            //            {
            //                IsCheckable = true,
            //                CheckType = Models.ExplorerContextMenuItemCheckType.Check,
            //                GroupName = "Test"
            //            },
            //        }
            //    }
            //};



            return null;
        }

        //private static JsonSerializerOptions GetJsonSerializerOptions() =>
        //    new JsonSerializerOptions()
        //    {
        //        AllowTrailingCommas = true,
        //        ReadCommentHandling = JsonCommentHandling.Skip,
        //        PropertyNameCaseInsensitive = true,
        //        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        //        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        //        PreferredObjectCreationHandling = System.Text.Json.Serialization.JsonObjectCreationHandling.Populate,
        //        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip,
        //        WriteIndented = false,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        //        Converters =
        //        {
        //            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        //        }
        //    };
    }
}
