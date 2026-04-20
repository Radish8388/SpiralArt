using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpiralArt
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public Brush brush = Brushes.Brown;
        public int colorNumber = 11;
        static Brush[] brushes = {
                Brushes.AliceBlue,
                Brushes.AntiqueWhite,
                Brushes.Aqua,
                Brushes.Aquamarine,
                Brushes.Azure,
                Brushes.Beige,
                Brushes.Bisque,
                Brushes.Black,
                Brushes.BlanchedAlmond,
                Brushes.Blue,
                Brushes.BlueViolet,
                Brushes.Brown,
                Brushes.BurlyWood,
                Brushes.CadetBlue,
                Brushes.Chartreuse,
                Brushes.Chocolate,
                Brushes.Coral,
                Brushes.CornflowerBlue,
                Brushes.Cornsilk,
                Brushes.Crimson,
                Brushes.Cyan,
                Brushes.DarkBlue,
                Brushes.DarkCyan,
                Brushes.DarkGoldenrod,
                Brushes.DarkGray,
                Brushes.DarkGreen,
                Brushes.DarkKhaki,
                Brushes.DarkMagenta,
                Brushes.DarkOliveGreen,
                Brushes.DarkOrange,
                Brushes.DarkOrchid,
                Brushes.DarkRed,
                Brushes.DarkSalmon,
                Brushes.DarkSeaGreen,
                Brushes.DarkSlateBlue,
                Brushes.DarkSlateGray,
                Brushes.DarkTurquoise,
                Brushes.DarkViolet,
                Brushes.DeepPink,
                Brushes.DeepSkyBlue,
                Brushes.DimGray,
                Brushes.DodgerBlue,
                Brushes.Firebrick,
                Brushes.FloralWhite,
                Brushes.ForestGreen,
                Brushes.Fuchsia,
                Brushes.Gainsboro,
                Brushes.GhostWhite,
                Brushes.Gold,
                Brushes.Goldenrod,
                Brushes.Gray,
                Brushes.Green,
                Brushes.GreenYellow,
                Brushes.Honeydew,
                Brushes.HotPink,
                Brushes.IndianRed,
                Brushes.Indigo,
                Brushes.Ivory,
                Brushes.Khaki,
                Brushes.Lavender,
                Brushes.LavenderBlush,
                Brushes.LawnGreen,
                Brushes.LemonChiffon,
                Brushes.LightBlue,
                Brushes.LightCoral,
                Brushes.LightCyan,
                Brushes.LightGoldenrodYellow,
                Brushes.LightGray,
                Brushes.LightGreen,
                Brushes.LightPink,
                Brushes.LightSalmon,
                Brushes.LightSeaGreen,
                Brushes.LightSkyBlue,
                Brushes.LightSlateGray,
                Brushes.LightSteelBlue,
                Brushes.LightYellow,
                Brushes.Lime,
                Brushes.LimeGreen,
                Brushes.Linen,
                Brushes.Magenta,
                Brushes.Maroon,
                Brushes.MediumAquamarine,
                Brushes.MediumBlue,
                Brushes.MediumOrchid,
                Brushes.MediumPurple,
                Brushes.MediumSeaGreen,
                Brushes.MediumSlateBlue,
                Brushes.MediumSpringGreen,
                Brushes.MediumTurquoise,
                Brushes.MediumVioletRed,
                Brushes.MidnightBlue,
                Brushes.MintCream,
                Brushes.MistyRose,
                Brushes.Moccasin,
                Brushes.NavajoWhite,
                Brushes.Navy,
                Brushes.OldLace,
                Brushes.Olive,
                Brushes.OliveDrab,
                Brushes.Orange,
                Brushes.OrangeRed,
                Brushes.Orchid,
                Brushes.PaleGoldenrod,
                Brushes.PaleGreen,
                Brushes.PaleTurquoise,
                Brushes.PaleVioletRed,
                Brushes.PapayaWhip,
                Brushes.PeachPuff,
                Brushes.Peru,
                Brushes.Pink,
                Brushes.Plum,
                Brushes.PowderBlue,
                Brushes.Purple,
                Brushes.Red,
                Brushes.RosyBrown,
                Brushes.RoyalBlue,
                Brushes.SaddleBrown,
                Brushes.Salmon,
                Brushes.SandyBrown,
                Brushes.SeaGreen,
                Brushes.SeaShell,
                Brushes.Sienna,
                Brushes.Silver,
                Brushes.SkyBlue,
                Brushes.SlateBlue,
                Brushes.SlateGray,
                Brushes.Snow,
                Brushes.SpringGreen,
                Brushes.SteelBlue,
                Brushes.Tan,
                Brushes.Teal,
                Brushes.Thistle,
                Brushes.Tomato,
                Brushes.Turquoise,
                Brushes.Violet,
                Brushes.Wheat,
                Brushes.White,
                Brushes.WhiteSmoke,
                Brushes.Yellow,
                Brushes.YellowGreen
            };

        public ColorPicker(Brush prevBrush, int prevColor)
        {
            InitializeComponent();
            Selected.Background = prevBrush;
            colorNumber = prevColor;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {

            brush = brushes[colorNumber];
            DialogResult = true;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border btn && btn.Tag != null)
            {
                if (!int.TryParse(btn.Tag.ToString(), out colorNumber))
                {
                    Debug.WriteLine($"Tag={btn.Tag.ToString()}, num={colorNumber}");
                    colorNumber = 7;
                }
                Selected.Background = brushes[colorNumber];
            }
        }

        public static Brush GetBrushColor(int color)
        {
            return brushes[color];
        }
    }
}
