using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Text;

namespace ExplorerContextMenu.ResourceHelper
{
    public static class IconHelper
    {
        public static void ConvertImagesToIcon(ImageData[] images, Stream outputStream)
        {
            if (outputStream.CanSeek) outputStream.Seek(0, SeekOrigin.Begin);

            using (var iconWriter = new BinaryWriter(outputStream, Encoding.ASCII, leaveOpen: true))
            using (var imagesBuffer = new MemoryStream())
            using (var tmpStream = new MemoryStream())
            {
                // Write header
                // 0-1 reserved
                iconWriter.Write((byte)0);
                iconWriter.Write((byte)0);

                // 2-3 image type, 1 = icon, 2 = cursor
                iconWriter.Write((short)1);

                // 4-5 number of images
                iconWriter.Write((short)images.Length);

                // image data offset
                // ico header (6 bytes) + image directory (16 bytes per image)
                long offset = 6 + (16 * images.Length);

                // Write image directory
                foreach (var image in images)
                {
                    tmpStream.Position = 0;
                    tmpStream.SetLength(0);

                    var size = (int)image.Size;

                    if (image.Image.Width != image.Image.Height
                        || image.Image.Width != image.Size
                        || image.Image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb
                        || image.Size > 256)
                    {
                        if (size > 256) size = 256;

                        using (var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                        {
                            using (var g = System.Drawing.Graphics.FromImage(bitmap))
                            {
                                g.DrawImage(image.Image, 0, 0, size, size);
                            }
                            bitmap.Save(tmpStream, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                    else
                    {
                        image.Image.Save(tmpStream, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    tmpStream.Position = 0;
                    tmpStream.CopyTo(imagesBuffer);

                    var dataLength = tmpStream.Length;

                    // 0 image width
                    iconWriter.Write((byte)(size));

                    // 1 image height
                    iconWriter.Write((byte)(size));

                    // 2 number of colors, 0 if the image does not use a color palette
                    iconWriter.Write((byte)0);

                    // 3 reserved
                    iconWriter.Write((byte)0);

                    // 4-5 color planes
                    iconWriter.Write((short)0);

                    // 6-7 bits per pixel, png use 4 bytes per pixel (argb)
                    iconWriter.Write((short)32);

                    // 8-11 size of image data
                    iconWriter.Write((uint)dataLength);

                    // 12-15 offset of image data
                    iconWriter.Write((uint)offset);

                    offset += dataLength;
                }

                imagesBuffer.Position = 0;
                imagesBuffer.CopyTo(iconWriter.BaseStream);
            }
        }


        internal static unsafe bool GetIconGroupResourceData(
            Stream stream,
            Func<ICONDIRENTRY, ushort> idGetter,
            [NotNullWhen(true)] out byte[]? iconGroupData,
            [NotNullWhen(true)] out IconResourceData[]? iconData)
        {
            iconGroupData = null;
            iconData = null;

            var iconDir = GetIconHeader(stream);
            if (iconDir.idCount <= 0)
            {
                return false;
            }

            iconData = new IconResourceData[iconDir.idCount];
            for (var i = 0; i < iconDir.idCount; i++)
            {
                iconData[i].Id = idGetter.Invoke(iconDir.idEntries[i]);
            }

            iconGroupData = GetIconGroupData(iconDir, iconData);

            if (iconGroupData != null)
            {
                for (var i = 0; i < iconDir.idCount; i++)
                {
                    iconData[i].Data = new byte[iconDir.idEntries[i].dwBytesInRes];
                    stream.Seek(iconDir.idEntries[i].dwImageOffset, SeekOrigin.Begin);
                    stream.Read(iconData[i].Data, 0, iconDir.idEntries[i].dwBytesInRes);
                }

                return true;
            }


            iconGroupData = null;
            iconData = null;

            return false;
        }


        private static unsafe ICONDIR GetIconHeader(Stream stream)
        {
            if (!stream.CanSeek) return default;

            stream.Seek(0, SeekOrigin.Begin);

            var iconDir = new ICONDIR();
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                iconDir.idReserved = reader.ReadUInt16();
                iconDir.idType = reader.ReadUInt16();
                if (iconDir.idType == 1)
                {
                    iconDir.idCount = reader.ReadUInt16();
                    if (iconDir.idCount > 0)
                    {
                        iconDir.idEntries = new ICONDIRENTRY[iconDir.idCount];
                        var buffer = new byte[sizeof(ICONDIRENTRY)];
                        fixed (byte* ptr = buffer)
                        {
                            for (int i = 0; i < iconDir.idCount; i++)
                            {
                                reader.Read(buffer, 0, buffer.Length);
                                iconDir.idEntries[i] = *(ICONDIRENTRY*)ptr;
                            }
                        }
                    }
                }
            }

            return iconDir;
        }

        private static unsafe byte[]? GetIconGroupData(ICONDIR iconDir, IconResourceData[] iconData)
        {
            if (iconDir.idCount <= 0) return null;
            var length = 6 + 14 * iconDir.idCount;
            var bytes = new byte[length];
            fixed (byte* ptr = bytes)
            {
                *((ushort*)ptr) = iconDir.idReserved;
                *((ushort*)(ptr + 2)) = iconDir.idType;
                *((ushort*)(ptr + 4)) = iconDir.idCount;

                var offset = 6;
                for (int i = 0; i < iconDir.idCount; i++)
                {
                    MEMICONDIRENTRY* pEntry = (MEMICONDIRENTRY*)(ptr + offset);

                    pEntry->bWidth = iconDir.idEntries[i].bWidth;
                    pEntry->bHeight = iconDir.idEntries[i].bHeight;
                    pEntry->bColorCount = iconDir.idEntries[i].bColorCount;
                    pEntry->bReserved = iconDir.idEntries[i].bReserved;
                    pEntry->wPlanes = iconDir.idEntries[i].wPlanes;
                    pEntry->wBitCount = iconDir.idEntries[i].wBitCount;
                    pEntry->dwBytesInRes = (uint)iconDir.idEntries[i].dwBytesInRes;
                    pEntry->nID = iconData[i].Id;

                    offset += 14;
                }
            }

            return bytes;
        }

        public class ImageData : IDisposable
        {
            private bool disposedValue;

            public ImageData(uint size, Image image)
            {
                Size = size;
                Image = image;
            }

            public uint Size { get; }

            public System.Drawing.Image Image { get; }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    Image.Dispose();
                    disposedValue = true;
                }
            }

            ~ImageData()
            {
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        internal struct IconResourceData
        {
            public ushort Id;

            public byte[] Data;
        }

        #region structs

        internal struct ICONDIR
        {
            public ushort idReserved;   // Reserved
            public ushort idType;       // resource type (1 for icons)
            public ushort idCount;      // how many images?
            public ICONDIRENTRY[] idEntries;   // the entries for each image
        };

        internal struct ICONDIRENTRY
        {
            public byte bWidth;         // Width of the image
            public byte bHeight;        // Height of the image (times 2)
            public byte bColorCount;    // Number of colors in image (0 if >=8bpp)
            public byte bReserved;      // Reserved
            public ushort wPlanes;      // Color Planes
            public ushort wBitCount;    // Bits per pixel
            public int dwBytesInRes;    // how many bytes in this resource?
            public int dwImageOffset;   // where in the file is this image
        }

        private struct MEMICONDIRENTRY
        {
            public byte bWidth;         // Width of the image
            public byte bHeight;        // Height of the image (times 2)
            public byte bColorCount;    // Number of colors in image (0 if >=8bpp)
            public byte bReserved;      // Reserved
            public ushort wPlanes;      // Color Planes
            public ushort wBitCount;    // Bits per pixel
            public uint dwBytesInRes;   // how many bytes in this resource?
            public ushort nID;          // the ID
        }


        #endregion structs
    }
}
