/*
 * OptiPNG file type
 * Copyright (C) 2008 ilikepi3142@gmail.com
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using PaintDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TheGameratorT.FileTypes.ENPG
{
    public sealed class ENPGFileTypeFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            return new[] { new ENPGFileType() };
        }
    }

    internal class ENPGFileType : FileType<ENPGSaveConfigToken, ENPGSaveConfigWidget>
    {
        internal ENPGFileType()
            //: base("Nintendo ENPG", FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving, new[] { ".enpg" })
            : base("Nintendo ENPG", new FileTypeOptions() {
                LoadExtensions = new string[] { ".enpg" },
                SaveExtensions = new string[] { ".enpg" }
            })
        {
        }

        private byte[] ConvertStreamToByteArray(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private int NCG_GetColorForPixel(byte[] data, int x, int y)
        {
            int offs = x + y * 256;
            if (offs >= data.Length) return 0;
            return data[offs];
        }

        private Color[] NCL_ByteArrayToPalette(byte[] data)
        {
            ByteArrayInputStream ii = new ByteArrayInputStream(data);
            Color[] pal = new Color[data.Length / 2];
            for (int i = 0; i < pal.Length; i++)
            {
                int c = ii.readUShort();
                int cR = (c & 31) * 8;
                int cG = ((c >> 5) & 31) * 8;
                int cB = ((c >> 10) & 31) * 8;
                pal[i] = Color.FromArgb(i == 0 ? 0 : 255, cR, cG, cB);
            }
            return pal;
        }

        private byte[] NCL_PaletteToByteArray(Color[] pal)
        {
            ByteArrayOutputStream oo = new ByteArrayOutputStream();
            for (int i = 0; i < pal.Length; i++)
            {
                Color c = pal[i];

                byte r = (byte)(c.R >> 3);
                byte g = (byte)(c.G >> 3);
                byte b = (byte)(c.B >> 3);

                ushort val = 0;

                val |= r;
                val |= (ushort)(g << 5);
                val |= (ushort)(b << 10);

                oo.writeUShort(val);
            }

            return oo.getArray();
        }

        private Image ENPG_StreamToImage(Stream stream)
        {
            byte[] bytes = ConvertStreamToByteArray(stream);
            try { bytes = LZ77.LZ77_Decompress(bytes); } catch (Exception) { }

            if (bytes.Length != 0x10200)
                throw new Exception("Not a valid ENPG file!\nFile size did not match a ENPG.");

            byte[] bmpBytes = new byte[0x10000];
            Array.Copy(bytes, 0, bmpBytes, 0, 0x10000);
            byte[] palBytes = new byte[0x200];
            Array.Copy(bytes, 0x10000, palBytes, 0, 0x200);

            Color[] pal = NCL_ByteArrayToPalette(palBytes);
            Bitmap bmp = new Bitmap(256, 256);
            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 256; y++)
                    bmp.SetPixel(x, y, pal[NCG_GetColorForPixel(bmpBytes, x, y)]);

            return bmp;
        }

        private byte[] ENPG_ImageToByteArray(Bitmap bmp)
        {
            byte[] bmpBytes = new byte[bmp.Width * bmp.Height];

            List<Color> registeredColors = new List<Color>();

            //Search for transparent pixel and add it (do this because it needs to be the first in the list)
            for(int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    if (color.A == 0)
                    {
                        registeredColors.Add(color);
                        goto TransparentColorFound;
                    }
                }
            }
            registeredColors.Add(Color.FromArgb(0, 0, 0, 0)); //If there are no transparent colors, add one
            TransparentColorFound:

            //Create palette and get colors for pixel at the same time
            int i = 0;
            for (int y = 0; y < bmp.Width; y++)
            {
                for (int x = 0; x < bmp.Height; x++)
                {
                    Color color = bmp.GetPixel(x, y);
                    if (!registeredColors.Contains(color))
                    {
                        registeredColors.Add(color);
                    }
                    bmpBytes[i] = (byte)registeredColors.IndexOf(color);

                    i++;
                }
            }

            byte[] palBytes = NCL_PaletteToByteArray(registeredColors.ToArray());

            byte[] result = new byte[0x10200];
            Array.Copy(bmpBytes, result, bmpBytes.Length);
            Array.Copy(palBytes, 0, result, bmpBytes.Length, palBytes.Length);

            return result;
        }

        protected override Document OnLoad(Stream input)
        {
            using (Image image = ENPG_StreamToImage(input))
            {
                return Document.FromImage(image);
            }
        }

        protected override void OnSaveT(Document input, Stream output, ENPGSaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {
            using (RenderArgs ra = new RenderArgs(scratchSurface))
            {
                input.Render(ra, true);
            }

            Bitmap final = reduceToPalette(scratchSurface, token.DitheringLevel, token.TransparencyThreshold, progressCallback);

            if (final.Size != new Size(256, 256))
            {
                byte[] invalidENPG = Properties.Resources.invalidENPGsize;
                output.Write(invalidENPG, 0, invalidENPG.Length);
                return;
            }

            byte[] finalBytes = ENPG_ImageToByteArray(final);
            if (token.CompressLZ77)
                finalBytes = LZ77.LZ77_Compress(finalBytes);
            output.Write(finalBytes, 0, finalBytes.Length);
            final.Dispose();
        }

        private unsafe Bitmap reduceToPalette(Surface surface, byte ditheringLevel, byte threshold, ProgressEventHandler progressCallback)
        {
            BinaryPixelOp blendOp = new UserBlendOps.NormalBlendOp();

            for (int y = 0; y < surface.Height; y++)
            {
                ColorBgra* ptr = surface.GetRowAddressUnchecked(y);

                for (int x = 0; x < surface.Width; x++)
                {
                    if (ptr->A < threshold)
                    {
                        ptr->Bgra = 0x00000000;
                    }
                    else
                    {
                        ptr->Bgra = blendOp.Apply(ColorBgra.White, *ptr).Bgra;
                    }
                    ptr++;
                }
            }

            int ColorCount = 256;
            //Search for transparent pixel and add it (do this because if there is no transparent pixel, quantize with -1 color)
            for (int x = 0; x < surface.Width; x++)
            {
                for (int y = 0; y < surface.Height; y++)
                {
                    Color color = surface.GetPoint(x, y);
                    if (color.A == 0)
                    {
                        goto TransparentColorFound;
                    }
                }
            }
            ColorCount = 255;
            TransparentColorFound:

            return Quantize(surface, ditheringLevel, ColorCount, true, progressCallback);
        }

        ~ENPGFileType()
        {
            //File.Delete(tempFile);
        }

        protected override ENPGSaveConfigToken OnCreateDefaultSaveConfigTokenT()
        {
            return new ENPGSaveConfigToken();
        }

        protected override ENPGSaveConfigWidget OnCreateSaveConfigWidgetT()
        {
            return new ENPGSaveConfigWidget();
        }
    }
}
