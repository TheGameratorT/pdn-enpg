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
using System.Windows.Forms;

namespace TheGameratorT.FileTypes.ENPG
{
    internal partial class ENPGSaveConfigWidget
    {
        private static readonly Padding firstIndent = new Padding(0, 0, 0, 0);
        private static readonly Padding secondIndent = new Padding(16, 0, 0, 0);
        private readonly int NUD_WIDTH = 50;

        private RadioButton palette;
        private Label ditheringLabel;
        private NumericUpDown ditheringLevel;
        private Label transThreshLabel;
        private NumericUpDown transThresh;

        private Label lz77CompLabel;
        private CheckBox lz77CompCheckBox;

        private LinkLabel enpgLinkLabel;

        public ENPGSaveConfigWidget()
            : base(new ENPGFileType())
        {
            AutoSize = true;

            NUD_WIDTH = (int)(NUD_WIDTH * this.AutoScaleDimensions.Width / 96f);

            TableLayoutPanel main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            ToolTip toolTip = new ToolTip();
            initColors(toolTip, main);
            initCompression(toolTip, main);

            enpgLinkLabel = new LinkLabel
            {
                AutoSize = true,
                LinkArea = new LinkArea(3, 13),
                Margin = new Padding(3, 10, 0, 0),
                Text = "By TheGameratorT"
            };
            enpgLinkLabel.LinkClicked += enpgLinkLabel_LinkClicked;
            main.Controls.Add(enpgLinkLabel);

            InitWidgetFromToken(new ENPGSaveConfigToken());

            Controls.Add(main);
        }

        private void initColors(ToolTip toolTip, TableLayoutPanel main)
        {
            HeaderLabel colorHeader = newHeader("Color settings");
            main.Controls.Add(colorHeader);

            palette = createBaseRadioBtn();
            palette.Text = "&Palette";
            palette.Margin = firstIndent;
            toolTip.SetToolTip(palette, "No more than 256 distinct colors");
            palette.CheckedChanged += tokenChanged;
            main.Controls.Add(palette);

            TableLayoutPanel dithering = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = secondIndent
            };

            ditheringLabel = new Label
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Text = "&Dithering level:",
                Margin = Padding.Empty
            };
            dithering.Controls.Add(ditheringLabel, 0, 0);

            ditheringLevel = new NumericUpDown
            {
                Width = NUD_WIDTH,
                Margin = Padding.Empty,
                Maximum = 8
            };
            ditheringLevel.ValueChanged += tokenChanged;
            dithering.Controls.Add(ditheringLevel, 1, 0);

            transThreshLabel = new Label
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Text = "&Transparency threshold:",
                Margin = Padding.Empty
            };
            dithering.Controls.Add(transThreshLabel, 0, 1);

            transThresh = new NumericUpDown
            {
                Width = NUD_WIDTH,
                Margin = new Padding(0, 3, 0, 0),
                Maximum = 255
            };
            toolTip.SetToolTip(transThresh, "Pixels with an alpha value less than the threshold will be fully transparent.");
            transThresh.ValueChanged += tokenChanged;
            dithering.Controls.Add(transThresh, 1, 1);

            main.Controls.Add(dithering);
        }

        private void initCompression(ToolTip toolTip, TableLayoutPanel main)
        {
            HeaderLabel colorHeader = newHeader("Compression settings");
            main.Controls.Add(colorHeader);

            TableLayoutPanel lz77Comp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = secondIndent
            };

            lz77CompLabel = new Label
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Text = "&LZ77 Compressed:",
                Margin = Padding.Empty
            };
            lz77Comp.Controls.Add(lz77CompLabel, 0, 0);

            lz77CompCheckBox = new CheckBox
            {
                Width = NUD_WIDTH,
                Margin = Padding.Empty
            };
            lz77CompCheckBox.CheckedChanged += tokenChanged;
            lz77Comp.Controls.Add(lz77CompCheckBox, 1, 0);

            main.Controls.Add(lz77Comp);
        }

        private RadioButton createBaseRadioBtn()
        {
            return new RadioButton
            {
                AutoSize = true
            };
        }

        private HeaderLabel newHeader(string text)
        {
            return new HeaderLabel
            {
                Dock = DockStyle.Fill,
                Text = text
            };
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            enpgLinkLabel.LinkColor = ForeColor;
        }
    }
}
