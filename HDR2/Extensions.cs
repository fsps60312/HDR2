﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace HDR2
{
    public static class Extensions
    {
        public static byte ClampByte(this double v)
        {
            return (byte)Math.Max(0, Math.Min(255, v));
        }
        public static void Save(this BitmapSource image, Stream stream)
        {
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(stream);
        }
        public static Button Set(this Button button, Action action)
        {
            button.Click += delegate { action(); };
            return button;
        }
        public static UIElement Set(this UIElement uIElement,int row,int column)
        {
            Grid.SetRow(uIElement, row);
            Grid.SetColumn(uIElement, column);
            return uIElement;
        }
        public static UIElement SetSpan(this UIElement uIElement, int rowSpan, int columnSpan)
        {
            Grid.SetRowSpan(uIElement, rowSpan);
            Grid.SetColumnSpan(uIElement, columnSpan);
            return uIElement;
        }
    }
}
