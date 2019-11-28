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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace TheGameratorT.FileTypes.ENPG
{
    internal partial class ENPGSaveConfigWidget : SaveConfigWidget<ENPGFileType, ENPGSaveConfigToken>
    {
        protected override ENPGSaveConfigToken CreateTokenFromWidget()
        {
            ENPGSaveConfigToken token = new ENPGSaveConfigToken();

            token.DitheringLevel = (byte)ditheringLevel.Value;
            token.TransparencyThreshold = (byte)transThresh.Value;
            token.CompressLZ77 = lz77CompCheckBox.Checked;

            return token;
        }

        protected override void InitWidgetFromToken(ENPGSaveConfigToken sourceToken)
        {
            palette.Checked = true;

            ditheringLevel.Value = sourceToken.DitheringLevel;
            transThresh.Value = sourceToken.TransparencyThreshold;
            lz77CompCheckBox.Checked = sourceToken.CompressLZ77;
        }

        private void tokenChanged(object sender, EventArgs e)
        {
            UpdateToken();
        }

        private void enpgLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/TheGameratorT");
            }
            catch (Win32Exception)
            {
                // Sometimes windows says file not found when Firefox takes too long to open
            }
        }
    }
}
