using Microsoft.Win32;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace SpiralArt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Ellipse ring = new Ellipse();
        Ellipse wheel = new Ellipse();
        Ellipse pen = new Ellipse();
        Polyline? polyline;
        Polyline? previewpolyline;
        Brush PenColor = Brushes.Black;
        int PenColorNumber = 7;
        Point RingCenter, WheelCenter, PenCenter;
        double PenArmLength = 50; // in percent of WheelRadius
        double PenWidth = 1;
        double RingRadius = 100; // in pixels
        double WheelRadius = 50; // in pixels
        double PenRadius = 2; // in pixels
        double RingThickness = 25; // in percent of RingRadius
        bool IsWheelInside = false;
        bool IsHideDisks = false;
        bool IsMovingWheel = false;
        bool IsMovingRing = false;
        bool IsMovingPaper = false;
        double LastMouseAngle = 0;
        double LastWheelAngle = 0;
        double LastPenAngle = Math.PI;
        double WheelDistance = 0;
        Point LastMousePos;
        List<Stroke> strokeList = new List<Stroke>();
        Stroke? currentStroke;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            // Capture the mouse to ensure we track it even if it leaves the window
            canvas.CaptureMouse();

            // check if wheel was clicked
            double distance = Math.Sqrt((p.X - WheelCenter.X) * (p.X - WheelCenter.X) +
                (p.Y - WheelCenter.Y) * (p.Y - WheelCenter.Y));
            if (distance < WheelRadius && !IsHideDisks) // if wheel was clicked
            {
                IsMovingWheel = true;
                IsMovingRing = false;
                IsMovingPaper = false;
                LastMouseAngle = GetMouseAngle(p); // in radians, clockwise from right
                WheelDistance = Math.Sqrt((WheelCenter.X - RingCenter.X) * (WheelCenter.X - RingCenter.X) +
                    (WheelCenter.Y - RingCenter.Y) * (WheelCenter.Y - RingCenter.Y));
                LastWheelAngle = GetWheelAngle();

                // create polyline
                polyline = new Polyline();
                polyline.Stroke = PenColor;
                polyline.StrokeThickness = PenWidth;
                polyline.StrokeLineJoin = PenLineJoin.Round;
                polyline.StrokeStartLineCap = PenLineCap.Round;
                polyline.StrokeEndLineCap = PenLineCap.Round;
                polyline.Points.Add(PenCenter);
                canvas.Children.Add(polyline);

                // create Stroke
                currentStroke = new Stroke();
                currentStroke.color = PenColor;
                currentStroke.width = PenWidth;
                currentStroke.offset.X = 0;
                currentStroke.offset.Y = 0;
                currentStroke.P.Add(PenCenter);
                //Debug.WriteLine($"MouseDown, wheel clicked, #strokes={strokeList.Count}");
                return;
            }

            // check if ring was clicked
            distance = Math.Sqrt((p.X - RingCenter.X) * (p.X - RingCenter.X) +
                (p.Y - RingCenter.Y) * (p.Y - RingCenter.Y));
            if ((distance < RingRadius) && (distance > RingRadius*(1-RingThickness/100)) && !IsHideDisks) // if ring was clicked
            {
                IsMovingWheel = false;
                IsMovingRing = true;
                IsMovingPaper = false;
                LastMousePos = p;
                //Debug.WriteLine($"MouseDown, ring clicked, #strokes={strokeList.Count}");
                return;
            }

            // moving the paper
            IsMovingWheel = false;
            IsMovingRing = false;
            IsMovingPaper = true;
            LastMousePos = p;
            //Debug.WriteLine($"MouseDown, paper clicked, #strokes={strokeList.Count}");
        }

        private double GetWheelAngle()
        {
            return Math.Atan2((WheelCenter.Y - RingCenter.Y), (WheelCenter.X - RingCenter.X));
        }

        private double GetMouseAngle(Point p)
        {
            return Math.Atan2((p.Y - RingCenter.Y), (p.X - RingCenter.X));
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(canvas);

            if (IsMovingWheel)
            {
                // calculate angles
                double newMouseAngle = GetMouseAngle(p);
                double deltaMouseAngle = GetDeltaMouseAngle(newMouseAngle, LastMouseAngle);
                if (Math.Abs(deltaMouseAngle) > 0.017) // greater than 1 degree
                    InterpolateWheelAndPen(deltaMouseAngle);
                else
                    MoveWheelAndPen(deltaMouseAngle);
                /*
                double newWheelAngle = LastWheelAngle + deltaMouseAngle;
                double newPenAngle = LastPenAngle + AddPenAngle(deltaMouseAngle);

                // calculate new wheel and pen positions
                WheelCenter.X = RingCenter.X + WheelDistance * Math.Cos(newWheelAngle);
                WheelCenter.Y = RingCenter.Y + WheelDistance * Math.Sin(newWheelAngle);
                PenCenter.X = WheelCenter.X + PenArmLength / 100 * WheelRadius * Math.Cos(newPenAngle);
                PenCenter.Y = WheelCenter.Y + PenArmLength / 100 * WheelRadius * Math.Sin(newPenAngle);

                // move wheel and pen
                Canvas.SetLeft(wheel, WheelCenter.X - WheelRadius);
                Canvas.SetTop(wheel, WheelCenter.Y - WheelRadius);
                Canvas.SetLeft(pen, PenCenter.X - PenRadius);
                Canvas.SetTop(pen, PenCenter.Y - PenRadius);

                // draw a line
                if (polyline != null)
                    polyline.Points.Add(PenCenter);
                if (currentStroke != null)
                    currentStroke.P.Add(PenCenter);

                // set values for the next call of this method
                LastWheelAngle = newWheelAngle;
                LastPenAngle = newPenAngle;
                */
                LastMouseAngle = newMouseAngle;
            }
            else if (IsMovingRing)
            {
                double deltaX = p.X - LastMousePos.X;
                double deltaY = p.Y - LastMousePos.Y;
                RingCenter.X += deltaX;
                RingCenter.Y += deltaY;
                Redraw();
                LastMousePos = p;
                //Debug.WriteLine($"MouseMove, ring clicked, #strokes={strokeList.Count}");
            }
            else if (IsMovingPaper)
            {
                double deltaX = p.X - LastMousePos.X;
                double deltaY = p.Y - LastMousePos.Y;
                //Debug.WriteLine($"p={p.X:F1},{p.Y:F1} last={LastMousePos.X:F1},{LastMousePos.Y:F1} delta={deltaX:F1},{deltaY:F1}");
                for (int i=0; i<strokeList.Count; i++)
                {
                    strokeList[i].offset.X += deltaX;
                    strokeList[i].offset.Y += deltaY;
                    //Debug.WriteLine($"stroke {i} offset={strokeList[i].offset.X:F1},{strokeList[i].offset.Y:F1}");
                }
                Redraw();
                LastMousePos = p;
                //Debug.WriteLine($"MouseMove, paper clicked, #strokes={strokeList.Count}");
            }
        }

        private void InterpolateWheelAndPen(double totalMouseAngle)
        {
            double mouseAngle;
            double cummulativeMouseAngle;

            if (totalMouseAngle > 0)
            {
                mouseAngle = 0.017;
                cummulativeMouseAngle = mouseAngle;
                while (cummulativeMouseAngle < totalMouseAngle)
                {
                    MoveWheelAndPen(mouseAngle);
                    cummulativeMouseAngle += mouseAngle;
                    Debug.WriteLine($"total={totalMouseAngle * 180 / Math.PI},angle={mouseAngle * 180 / Math.PI}");
                }
                MoveWheelAndPen(totalMouseAngle - cummulativeMouseAngle + mouseAngle);
            }
            else
            {
                mouseAngle = -0.017;
                cummulativeMouseAngle = mouseAngle;
                while (cummulativeMouseAngle > totalMouseAngle)
                {
                    MoveWheelAndPen(mouseAngle);
                    cummulativeMouseAngle += mouseAngle;
                    Debug.WriteLine($"total={totalMouseAngle * 180 / Math.PI},angle={mouseAngle * 180 / Math.PI}");
                }
                MoveWheelAndPen(totalMouseAngle - cummulativeMouseAngle + mouseAngle);
            }
        }

        private void MoveWheelAndPen(double deltaMouseAngle)
        {
            double newWheelAngle = LastWheelAngle + deltaMouseAngle;
            double newPenAngle = LastPenAngle + AddPenAngle(deltaMouseAngle);

            // calculate new wheel and pen positions
            WheelCenter.X = RingCenter.X + WheelDistance * Math.Cos(newWheelAngle);
            WheelCenter.Y = RingCenter.Y + WheelDistance * Math.Sin(newWheelAngle);
            PenCenter.X = WheelCenter.X + PenArmLength / 100 * WheelRadius * Math.Cos(newPenAngle);
            PenCenter.Y = WheelCenter.Y + PenArmLength / 100 * WheelRadius * Math.Sin(newPenAngle);

            // move wheel and pen
            Canvas.SetLeft(wheel, WheelCenter.X - WheelRadius);
            Canvas.SetTop(wheel, WheelCenter.Y - WheelRadius);
            Canvas.SetLeft(pen, PenCenter.X - PenRadius);
            Canvas.SetTop(pen, PenCenter.Y - PenRadius);

            // draw a line
            if (polyline != null)
                polyline.Points.Add(PenCenter);
            if (currentStroke != null)
                currentStroke.P.Add(PenCenter);

            // set values for the next call of this method
            LastWheelAngle = newWheelAngle;
            LastPenAngle = newPenAngle;
        }

        private double GetDeltaMouseAngle(double angle1, double angle2)
        {
            double deltaMouseAngle = angle1 - angle2;

            // to handle the jump from 180 deg to -180 deg
            while (deltaMouseAngle < Math.PI)
                deltaMouseAngle += 2 * Math.PI;
            while (deltaMouseAngle > Math.PI)
                deltaMouseAngle -= 2 * Math.PI;

            return deltaMouseAngle;
        }

        private double AddPenAngle(double deltaWheelAngle)
        {
            double deltaPenAngle, radius, arc;
            /*
            // to handle the jump from 180 deg to -180 deg
            while (deltaWheelAngle < Math.PI)
                deltaWheelAngle += 2 * Math.PI;
            while (deltaWheelAngle > Math.PI)
                deltaWheelAngle -= 2 * Math.PI;
            */
            if (IsWheelInside)
                radius = RingRadius * (RingThickness / 100 - 1);
            else
                radius = RingRadius;
            arc = deltaWheelAngle * radius;
            deltaPenAngle = arc / WheelRadius;
            return deltaPenAngle;
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            canvas.ReleaseMouseCapture();
            polyline = null;
            if (IsMovingWheel)
            {
                double newMouseAngle = GetMouseAngle(p);
                double deltaMouseAngle = GetDeltaMouseAngle(newMouseAngle, LastMouseAngle);
                if (Math.Abs(deltaMouseAngle) > 0.017) // greater than 1 degree
                    InterpolateWheelAndPen(deltaMouseAngle);
                else
                    MoveWheelAndPen(deltaMouseAngle);
                LastMouseAngle = newMouseAngle;
                if (currentStroke != null)
                    strokeList.Add(currentStroke);
            }
            IsMovingWheel = false;
            IsMovingRing = false;
            IsMovingPaper = false;
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double maxSize = Math.Min(canvas.ActualWidth, canvas.ActualHeight) / 2.0;
            //Debug.WriteLine($"max size = {maxSize}");
            RingSize.Maximum = maxSize;
            WheelSize.Maximum = maxSize;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // load the properties from disk
            Properties.Settings.Default.Reload();
            RingRadius = Properties.Settings.Default.RingSize;
            WheelRadius = Properties.Settings.Default.WheelSize;
            PenArmLength = Properties.Settings.Default.PenPos;
            PenWidth = Properties.Settings.Default.LineWidth;
            PenColorNumber = Properties.Settings.Default.Color;
            IsWheelInside = Properties.Settings.Default.WheelInside;
            IsHideDisks = Properties.Settings.Default.HideDisks;

            RingSize.Value = RingRadius;
            WheelSize.Value = WheelRadius;
            PenPos.Value = PenArmLength;
            LineWidth.Value = PenWidth;
            PenColor = ColorPicker.GetBrushColor(PenColorNumber);
            if (IsWheelInside)
            {
                InvertWheel.Inlines.Clear();
                InvertWheel.Inlines.Add("Wheel");
                InvertWheel.Inlines.Add(new LineBreak());
                InvertWheel.Inlines.Add("Outside");
            }
            else
            {
                InvertWheel.Inlines.Clear();
                InvertWheel.Inlines.Add("Wheel");
                InvertWheel.Inlines.Add(new LineBreak());
                InvertWheel.Inlines.Add("Inside");
            }
            if (IsHideDisks)
            {
                HideDisks.Inlines.Clear();
                HideDisks.Inlines.Add("Show");
                HideDisks.Inlines.Add(new LineBreak());
                HideDisks.Inlines.Add("Disks");
            }
            else
            {
                HideDisks.Inlines.Clear();
                HideDisks.Inlines.Add("Hide");
                HideDisks.Inlines.Add(new LineBreak());
                HideDisks.Inlines.Add("Disks");
            }

            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;

            // ensure window size doesn't exceed screen size
            if (this.Width > screenWidth) this.Width = screenWidth;
            if (this.Height > screenHeight) this.Height = screenHeight;

            // ensure window is not off the left or top
            if (this.Left < 0) this.Left = 0;
            if (this.Top < 0) this.Top = 0;

            // ensure window is not off the right or bottom
            if (this.Left + this.Width > screenWidth)
                this.Left = screenWidth - this.Width;
            if (this.Top + this.Height > screenHeight)
                this.Top = screenHeight - this.Height;

            RingCenter.X = canvas.ActualWidth / 2.0;
            RingCenter.Y = canvas.ActualHeight / 2.0;
            if (IsWheelInside)
                WheelDistance = RingRadius * (1 - RingThickness / 100) - WheelRadius;
            else
                WheelDistance = RingRadius + WheelRadius;

            LastWheelAngle = 0;
            LastPenAngle = Math.PI;
            PenRadius = (PenWidth + 3) / 2.0;
            Redraw();
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            strokeList.Clear();
            Redraw();
        }

        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsWheelInside)
                WheelDistance = RingRadius * (1 - RingThickness / 100) - WheelRadius;
            else
                WheelDistance = RingRadius + WheelRadius;
            LastWheelAngle = 0;
            LastPenAngle = Math.PI;
            Redraw();
        }

        private void ColorButtonClick(object sender, RoutedEventArgs e)
        {
            ColorPicker dialog = new ColorPicker(PenColor, PenColorNumber);
            dialog.Owner = this;
            bool? result = dialog.ShowDialog(); // Modal dialog
            if (result == true)
            {
                PenColor = dialog.brush;
                PenColorNumber = dialog.colorNumber;
                pen.Fill = PenColor;
            }
        }

        private void InvertButtonClick(object sender, RoutedEventArgs e)
        {
            IsWheelInside = !IsWheelInside;
            if (IsWheelInside)
            {
                InvertWheel.Inlines.Clear();
                InvertWheel.Inlines.Add("Wheel");
                InvertWheel.Inlines.Add(new LineBreak());
                InvertWheel.Inlines.Add("Outside");
            }
            else
            {
                InvertWheel.Inlines.Clear();
                InvertWheel.Inlines.Add("Wheel");
                InvertWheel.Inlines.Add(new LineBreak());
                InvertWheel.Inlines.Add("Inside");
            }
            if (IsWheelInside)
                WheelDistance = RingRadius * (1 - RingThickness / 100) - WheelRadius;
            else
                WheelDistance = RingRadius + WheelRadius;
            Redraw();
        }

        private void PreviewButtonClick(object sender, RoutedEventArgs e)
        {
            double radius, deltaWheelAngle, deltaPenAngle;
            Point wheelCenter, penCenter;

            // Remove previous preview if exists
            if (previewpolyline != null)
                canvas.Children.Remove(previewpolyline);

            deltaWheelAngle = 1.0 * Math.PI / 180.0;
            if (IsWheelInside)
                radius = RingRadius * (RingThickness / 100 - 1);
            else
                radius = RingRadius;
            deltaPenAngle = deltaWheelAngle * radius / WheelRadius;

            double newWheelAngle = LastWheelAngle;
            double newPenAngle = LastPenAngle;

            // create polyline
            previewpolyline = new Polyline();
            previewpolyline.Stroke = PenColor;
            previewpolyline.Opacity = 0.25;
            previewpolyline.StrokeThickness = PenWidth;
            previewpolyline.StrokeLineJoin = PenLineJoin.Round;
            previewpolyline.StrokeStartLineCap = PenLineCap.Round;
            previewpolyline.StrokeEndLineCap = PenLineCap.Round;
            canvas.Children.Add(previewpolyline);

            for (int i = 0; i <= 18000; i++)
            {
                wheelCenter.X = RingCenter.X + WheelDistance * Math.Cos(newWheelAngle);
                wheelCenter.Y = RingCenter.Y + WheelDistance * Math.Sin(newWheelAngle);
                penCenter.X = wheelCenter.X + PenArmLength / 100 * WheelRadius * Math.Cos(newPenAngle);
                penCenter.Y = wheelCenter.Y + PenArmLength / 100 * WheelRadius * Math.Sin(newPenAngle);
                if (previewpolyline != null)
                    previewpolyline.Points.Add(penCenter);

                // set values for the next loop
                newWheelAngle += deltaWheelAngle;
                newPenAngle += deltaPenAngle;
            }
        }

        private void HideDisksButtonClick(object sender, RoutedEventArgs e)
        {
            IsHideDisks = !IsHideDisks;
            if (IsHideDisks)
            {
                HideDisks.Inlines.Clear();
                HideDisks.Inlines.Add("Show");
                HideDisks.Inlines.Add(new LineBreak());
                HideDisks.Inlines.Add("Disks");
            }
            else
            {
                HideDisks.Inlines.Clear();
                HideDisks.Inlines.Add("Hide");
                HideDisks.Inlines.Add(new LineBreak());
                HideDisks.Inlines.Add("Disks");
            }
            Redraw();
        }

        private void RingSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RingRadius = RingSize.Value;

            if (IsWheelInside)
                WheelDistance = RingRadius * (1 - RingThickness / 100) - WheelRadius;
            else
                WheelDistance = RingRadius + WheelRadius;

            Redraw();
        }

        private void WheelSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            WheelRadius = WheelSize.Value;

            if (IsWheelInside)
                WheelDistance = RingRadius * (1 - RingThickness / 100) - WheelRadius;
            else
                WheelDistance = RingRadius + WheelRadius;

            Redraw();
        }

        private void PenPos_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PenArmLength = PenPos.Value;
            Redraw();
        }

        private void LineWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PenWidth = LineWidth.Value;
            PenRadius = (PenWidth + 3) / 2.0;
            Redraw();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            // save the properties to disk
            Properties.Settings.Default.RingSize = RingRadius;
            Properties.Settings.Default.WheelSize = WheelRadius;
            Properties.Settings.Default.PenPos = PenArmLength;
            Properties.Settings.Default.LineWidth = PenWidth;
            Properties.Settings.Default.Color = PenColorNumber;
            Properties.Settings.Default.WheelInside = IsWheelInside;
            Properties.Settings.Default.HideDisks = IsHideDisks;
            Properties.Settings.Default.Save();

        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            // create a bitmap
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight, 
                96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(canvas);
                dc.DrawRectangle(vb, null, new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight));
            }
            rtb.Render(dv);

            // Copy to clipboard
            Clipboard.SetImage(rtb);

            // Restore window layout (copying can mess it up)
            //InvalidateWindowLayout();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // create a bitmap
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight,
                96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(canvas);
                dc.DrawRectangle(vb, null, new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight));
            }
            rtb.Render(dv);

            // save to file
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "PNG Image|*.png";
            dlg.DefaultExt = ".png";
            if (dlg.ShowDialog() == true)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            // create a bitmap
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight,
                96, 96, PixelFormats.Pbgra32);

            // render the canvas into the bitmap
            DrawingVisual dvCapture = new DrawingVisual();
            using (DrawingContext dc = dvCapture.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(canvas);
                dc.DrawRectangle(vb, null, new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight));
            }
            rtb.Render(dvCapture);

            // print the bitmap
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                DrawingVisual dvPrint = new DrawingVisual();

                // 1 inch = 25.4mm, 1 inch = 96 pixels
                double mmToPixels = 96.0 / 25.4;

                double marginTop = 5 * mmToPixels;
                double marginBottom = 15 * mmToPixels;
                double marginLeft = 5 * mmToPixels;
                double marginRight = 5 * mmToPixels;

                double printableWidth = dlg.PrintableAreaWidth - marginLeft - marginRight;
                double printableHeight = dlg.PrintableAreaHeight - marginTop - marginBottom;

                double canvasAspect = canvas.ActualWidth / canvas.ActualHeight;
                double pageAspect = printableWidth / printableHeight;

                double printWidth, printHeight;

                if (canvasAspect > pageAspect)
                {
                    printWidth = printableWidth;
                    printHeight = printWidth / canvasAspect;
                }
                else
                {
                    printHeight = printableHeight;
                    printWidth = printHeight * canvasAspect;
                }

                // center within the margins
                double offsetX = marginLeft + (printableWidth - printWidth) / 2;
                double offsetY = marginTop + (printableHeight - printHeight) / 2;

                using (DrawingContext dc = dvPrint.RenderOpen())
                {
                    dc.DrawImage(rtb, new Rect(offsetX, offsetY, printWidth, printHeight));
                }

                dlg.PrintVisual(dvPrint, "SpiralArt");
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Redraw()
        {
            canvas.Children.Clear();
            WheelCenter.X = RingCenter.X + WheelDistance * Math.Cos(LastWheelAngle);
            WheelCenter.Y = RingCenter.Y + WheelDistance * Math.Sin(LastWheelAngle);
            PenCenter.X = WheelCenter.X + PenArmLength / 100 * WheelRadius * Math.Cos(LastPenAngle);
            PenCenter.Y = WheelCenter.Y + PenArmLength / 100 * WheelRadius * Math.Sin(LastPenAngle);

            if (!IsHideDisks)
            {

                // draw ring
                ring.Width = 2 * RingRadius;
                ring.Height = 2 * RingRadius;
                ring.StrokeThickness = RingThickness / 100 * RingRadius;
                ring.Stroke = Brushes.LightGray;
                ring.Opacity = 0.25;
                ring.Fill = Brushes.Transparent;
                Canvas.SetZIndex(ring, 1);
                Canvas.SetLeft(ring, RingCenter.X - RingRadius);
                Canvas.SetTop(ring, RingCenter.Y - RingRadius);
                canvas.Children.Add(ring);

                // draw wheel
                wheel.Width = 2 * WheelRadius;
                wheel.Height = 2 * WheelRadius;
                wheel.Stroke = Brushes.Transparent;
                wheel.Fill = Brushes.LightGray;
                wheel.Opacity = 0.25;
                Canvas.SetZIndex(wheel, 1);
                Canvas.SetLeft(wheel, WheelCenter.X - WheelRadius);
                Canvas.SetTop(wheel, WheelCenter.Y - WheelRadius);
                canvas.Children.Add(wheel);

                // draw pen
                pen.Width = 2 * PenRadius;
                pen.Height = 2 * PenRadius;
                pen.Stroke = Brushes.Transparent;
                pen.Fill = PenColor;
                Canvas.SetZIndex(pen, 2);
                Canvas.SetLeft(pen, PenCenter.X - PenRadius);
                Canvas.SetTop(pen, PenCenter.Y - PenRadius);
                canvas.Children.Add(pen);
            }

            // draw all strokes
            DrawStrokes();
        }

        private void DrawStrokes()
        {
            Point p;
            for (int i = 0; i < strokeList.Count; i++)
            {
                // create polyline
                polyline = new Polyline();
                polyline.Stroke = strokeList[i].color;
                polyline.StrokeThickness = strokeList[i].width;
                polyline.StrokeLineJoin = PenLineJoin.Round;
                polyline.StrokeStartLineCap = PenLineCap.Round;
                polyline.StrokeEndLineCap = PenLineCap.Round;
                for (int j = 0; j < strokeList[i].P.Count; j++)
                {
                    p = strokeList[i].P[j];
                    p.X += strokeList[i].offset.X;
                    p.Y += strokeList[i].offset.Y;
                    polyline.Points.Add(p);
                }
                canvas.Children.Add(polyline);
                /*
                Canvas.SetLeft(polyline, Canvas.GetLeft(polyline) + strokeList[i].offset.X);
                Canvas.SetTop(polyline, Canvas.GetTop(polyline) + strokeList[i].offset.Y);
                */
                //Debug.WriteLine($"offsetX={strokeList[i].offset.X}, offsetY={strokeList[i].offset.Y}");
                //Debug.WriteLine($"GetLeft={Canvas.GetLeft(polyline)}, GetTop={Canvas.GetTop(polyline)}");
                /*
                TranslateTransform? tt = polyline.RenderTransform as TranslateTransform;
                if (tt == null)
                {
                    tt = new TranslateTransform();
                    polyline.RenderTransform = tt;
                }
                tt.X += strokeList[i].offset.X;
                tt.Y += strokeList[i].offset.Y;
                */
            }
        }
    }

    public class Stroke
    {
        public Brush color = Brushes.Black;
        public double width = 1;
        public Point offset = new Point(0, 0);
        public List<Point> P = new List<Point>();
    }
}