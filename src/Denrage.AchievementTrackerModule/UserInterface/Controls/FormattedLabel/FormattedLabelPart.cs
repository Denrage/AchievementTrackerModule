﻿using System;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls.FormattedLabel
{
    internal class FormattedLabelPart
    {
        public BitmapFont Font { get; }

        public bool IsItalic { get; }

        public bool IsStrikeThrough { get; }

        public bool IsUnderlined { get; }

        public string Text { get; }

        public Action Link { get; }

        public Texture2D PrefixImage { get; }

        public Texture2D SuffixImage { get; }

        public Point PrefixImageSize { get; }

        public Point SuffixImageSize { get; }

        public ContentService.FontSize FontSize { get; }

        public ContentService.FontFace FontFace { get; }

        public Color TextColor { get; }

        public Color HoverColor { get; }

        public FormattedLabelPart(
            bool isItalic,
            bool isStrikeThrough,
            bool isUnderlined,
            string text,
            Action link,
            Texture2D prefixImage,
            Texture2D suffixImage,
            Point prefixImageSize,
            Point suffixImageSize,
            Color textColor,
            Color hoverColor,
            ContentService.FontSize fontSize,
            ContentService.FontFace fontFace)
        {
            IsItalic = isItalic;
            IsStrikeThrough = isStrikeThrough;
            IsUnderlined = isUnderlined;
            Text = text;
            Link = link;
            PrefixImage = prefixImage;
            SuffixImage = suffixImage;
            PrefixImageSize = prefixImageSize;
            SuffixImageSize = suffixImageSize;
            HoverColor = hoverColor;
            FontSize = fontSize;
            FontFace = fontFace;
            TextColor = textColor == default ? Color.White : textColor;

            var style = ContentService.FontStyle.Regular;

            if (IsItalic)
            {
                style = ContentService.FontStyle.Italic;
            }

            Font = GameService.Content.GetFont(FontFace, FontSize, style);
        }
    }
}