using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using System.Drawing;
using System.Windows.Threading;


namespace paintnewVe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private BitmapSource CreateBitmapSource(System.Windows.Media.Color color)
        {
            int width = (int)paintCanvas.Width;
            int height = (int)paintCanvas.Height;
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];

            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(color);
            BitmapPalette myPalette = new BitmapPalette(colors);

            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed1,
                myPalette,
                pixels,
                stride);

            return image;
        }

        public MainWindow()
        {
            InitializeComponent();
            
        }

        #region initiator
        bool iscopy = false;
        Point startPoint;
        bool isMouseDowned = false;
        bool isMouseUp = false;
        Line lastLine;
        Ellipse ti;
        Ellipse lastEllipse;
        Rectangle lastRec, lastRecSelect;
        System.Windows.Shapes.Path Star;
        System.Windows.Shapes.Path Heart;
        System.Windows.Shapes.Path Arrow;
        System.Windows.Shapes.Path Star1;
        System.Windows.Shapes.Path Heart1;
        System.Windows.Shapes.Path Arrow1;
        int type = 0;
        FontFamily font = new FontFamily("Consolas");
        BrushConverter cv = new BrushConverter();
        string color = "Black";
        int size = 2;
        Image a = new Image();
        int dotLine = 0;
        int isFillObject = 0;
        int isFill = 0;

        

        ContentControl ctrl;
        ContentControl ctrlSelect;
        Stack<RenderTargetBitmap> backGround = new Stack<RenderTargetBitmap>();
        static Ellipse tempE = new Ellipse();
        Rectangle tempR = new Rectangle();
        Stack<RenderTargetBitmap> redo = new Stack<RenderTargetBitmap>();
        bool IsContent = false;
        UIElementCollection collection;
        Shape tempS;
        int key = 0;
        int object_ = 0;
        bool isMove = false;

        #endregion

        private void color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            color = Convert.ToString(_colorPicker.SelectedColorText);
        }

        private void lineButton_Click(object sender, RoutedEventArgs e)
        {
            type = 1;
        }

        private void ellipButton_Click(object sender, RoutedEventArgs e)
        {
            type = 2;
        }

        private void rectangleButton_Click(object sender, RoutedEventArgs e)
        {
            type = 3; 
        }

        private void freeStyle_Click(object sender, RoutedEventArgs e)
        {
            type = 0;
        }

        private void select_button(object sender, RoutedEventArgs e)
        {
            type = 4;
        }

        private void cut_button(object sender, RoutedEventArgs e)
        {
            if (type != 4 || ctrlSelect == null)
                return;

            Image b = ctrlSelect.Content as Image;

            Clipboard.SetImage(b.Source as BitmapSource);

            paintCanvas.Children.Remove(ctrlSelect);

            iscopy = false;
        }

        private void copy_button(object sender, RoutedEventArgs e)
        {
            if (type != 4 || ctrlSelect == null)
                return;

            Image c = new Image();
            c = ctrlSelect.Content as Image;
            Clipboard.SetImage(c.Source as BitmapSource);
            temprush = new ImageBrush();
            temprush.ImageSource = c.Source;
            iscopy = true;
        }

        ImageBrush temprush;
        private void paste_button(object sender, RoutedEventArgs e)
        {
           
            Rect rect = new Rect(paintCanvas.Margin.Left, paintCanvas.Margin.Top, paintCanvas.ActualWidth, paintCanvas.ActualHeight);
            double dpi = 96d;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, dpi, dpi, System.Windows.Media.PixelFormats.Default);
            rtb.Render(paintCanvas);
            
            ImageBrush br = new ImageBrush();
            br.ImageSource = rtb;
            backGround.Push(rtb);
            //paintCanvas.Children.Clear();
            //paintCanvas.Children.Add(a);
            paintCanvas.Background = br;

            Image a = new Image();
            a.Source = Clipboard.GetImage() as ImageSource;
            a.IsHitTestVisible = false;

            if (iscopy == true)
            {
                ImageBrush imgbrush = new ImageBrush();
                imgbrush.ImageSource = a.Source;

                Rectangle b = new Rectangle();
                b.Height = a.Source.Height;
                b.Width = a.Source.Width;
                b.RenderTransform = ctrlSelect.RenderTransform;
                b.RenderTransformOrigin = ctrlSelect.RenderTransformOrigin;
                b.Fill = imgbrush;

                Canvas.SetLeft(b, Canvas.GetLeft(ctrlSelect));
                Canvas.SetTop(b, Canvas.GetTop(ctrlSelect));

                paintCanvas.Children.Add(b);
                iscopy = false;
            }

            ctrlSelect = new ContentControl();
            ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("Resources/DesignerItem.xaml", UriKind.Relative));
            Style st = (Style)res["DesignerItemStyle"];
            ctrlSelect.Style = st;
            ctrlSelect.SetValue(Selector.IsSelectedProperty, true);

            ctrlSelect.Height = a.Source.Height;
            ctrlSelect.Width = a.Source.Width;

            a.Height = double.NaN;
            a.Width = double.NaN;

            ctrlSelect.Content = a;

            Canvas.SetLeft(ctrlSelect, 0);
            Canvas.SetTop(ctrlSelect, 0);

            paintCanvas.Children.Add(ctrlSelect);
        }

        bool dragInProcess = false;
        void GetOutContent(int shape)
        {
            var top = ctrl.GetValue(Canvas.TopProperty);
            var left = ctrl.GetValue(Canvas.LeftProperty);
            if (type == 6)
            {
                System.Windows.Shapes.Path r = (System.Windows.Shapes.Path)ctrl.Content;
                r.Width = ctrl.Width;
                r.Height = ctrl.Height;
                r.SetValue(Canvas.TopProperty, top);
                r.SetValue(Canvas.LeftProperty, left);
                r.Stroke = (Brush)cv.ConvertFromString(color);
                r.StrokeThickness = size;
                r.RenderTransform = ctrl.RenderTransform;
                r.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                ctrl.Content = null;
                paintCanvas.Children.Add(r);
                paintCanvas.Children.Remove(ctrl);
                ctrl = null;
                dragInProcess = false;
            }
            else if (type == 7)
            {
                System.Windows.Shapes.Path r = (System.Windows.Shapes.Path)ctrl.Content;
                r.Width = ctrl.Width;
                r.Height = ctrl.Height;
                r.SetValue(Canvas.TopProperty, top);
                r.SetValue(Canvas.LeftProperty, left);
                r.Stroke = (Brush)cv.ConvertFromString(color);
                r.StrokeThickness = size;
                r.RenderTransform = ctrl.RenderTransform;
                r.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                ctrl.Content = null;
                paintCanvas.Children.Add(r);
                paintCanvas.Children.Remove(ctrl);
                ctrl = null;
                dragInProcess = false;
            }
            else if (type == 8)
            {
                System.Windows.Shapes.Path r = (System.Windows.Shapes.Path)ctrl.Content;
                r.Width = ctrl.Width;
                r.Height = ctrl.Height;
                r.SetValue(Canvas.TopProperty, top);
                r.SetValue(Canvas.LeftProperty, left);
                r.Stroke = (Brush)cv.ConvertFromString(color);
                r.StrokeThickness = size;
                r.RenderTransform = ctrl.RenderTransform;
                r.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                ctrl.Content = null;
                paintCanvas.Children.Add(r);
                paintCanvas.Children.Remove(ctrl);
                ctrl = null;
                dragInProcess = false;

            }
            dragInProcess = false;
            IsContent = false;
        }
        void setProperties()
        {
            if (type == 6)
            {
                Star.Data = Geometry.Parse("F1 M 50,0L 34.45,33L 0,37.99L 25,64L 19.1,100L 50,83L 81,100L 75,64L 100,37.99L 65.5,32.9L 50,0 Z");
                Star.Stroke = (Brush)cv.ConvertFromString(color);
                if(isFill == 1)
                {
                    Star.Fill = (Brush)cv.ConvertFromString(color);
                }
                Star.StrokeThickness = slider2.Value;
               
                Star.Stretch = Stretch.Fill;
            }
            else if (type == 7)
            {
                Heart.Data = Geometry.Parse("F1 M 240,200 A 20,20 0 0 0 200,240 C 210,250 240,270 240,270 C 240,270 260,260 280,240 A 20,20 0 0 0 240,200 Z");
                Heart.Stroke = (Brush)cv.ConvertFromString(color);
                if (isFill == 1)
                {
                    Heart.Fill = (Brush)cv.ConvertFromString(color);
                }
                Heart.StrokeThickness = slider2.Value;
             
                Heart.Stretch = Stretch.Fill;
            }
            else if (type == 8)
            {
                Arrow.Data = Geometry.Parse("F1 M 527.311,139.851L 499.019,120.735L 499.019,130.675L 464.611,130.675L 464.611,149.027L 499.019,149.027L 499.019,158.967L 527.311,139.851 Z");
                Arrow.Stroke = (Brush)cv.ConvertFromString(color);
                if (isFill == 1)
                {
                    Heart.Fill = (Brush)cv.ConvertFromString(color);
                }
                Arrow.StrokeThickness = slider2.Value;
                Arrow.Stretch = Stretch.Fill;
               
            }
        }

        void newshape(int Obj, MouseButtonEventArgs e)
        {
            if (type == 6)
            {
                Star = new System.Windows.Shapes.Path();
                
            }
            else if (type == 7)
            {
                Heart = new System.Windows.Shapes.Path();
                
            }
            else if (type == 8)
            {
                Arrow = new System.Windows.Shapes.Path();
                
            }
            setProperties();
        }
        private void paintCanvas_MouseMove(object sender, MouseEventArgs e)
        {
             if (isMouseDowned)
            {
                switch (type)
                {
                    case 0:
                        {
                            Point endPoint = e.GetPosition((IInputElement)sender);
                            Line free = new Line();
                            free.Stroke = (Brush)cv.ConvertFromString(color);


                            free.StrokeThickness = slider.Value;
                            free.X1 = startPoint.X;
                            free.Y1 = startPoint.Y;
                            free.X2 = endPoint.X;
                            free.Y2 = endPoint.Y;
                            if (isMouseDowned == true)
                                paintCanvas.Children.Add(free);
                            startPoint.X = endPoint.X;
                            startPoint.Y = endPoint.Y;
                            //isMouseDowned = false;
                            break;
                        }
                    case 1:
                        {
                            //MessageBox.Show(Convert.ToString(dotLine));
                            Point endPoint = e.GetPosition((IInputElement)sender);
                            //if (lastLine != null)
                            //    paintCanvas.Children.Remove(lastLine);
                            bool addLine = false;
                            if (lastLine == null)
                            {
                                lastLine = new Line();
                                addLine = true;
                            }
                            lastLine.Stroke = (Brush)cv.ConvertFromString(color);
                            if (dotLine == 1)
                            {
                                lastLine.StrokeDashArray = new DoubleCollection() { 1, 1 };
                            }
                            else if (dotLine == 2)
                            {
                                lastLine.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                            }
                            else if(dotLine == 0)
                            {
                                lastLine.StrokeDashArray = null;
                            }
                            
                            lastLine.StrokeThickness = slider.Value;
                            lastLine.X1 = startPoint.X;
                            lastLine.Y1 = startPoint.Y;
                            lastLine.X2 = endPoint.X;
                            lastLine.Y2 = endPoint.Y;
                            if (addLine)
                                paintCanvas.Children.Add(lastLine);
                            if (isMouseUp == true)
                                lastLine = null;
                            break;
                        }
                    case 2:
                        {
                            tempE = new Ellipse();
                            //MessageBox.Show(Convert.ToString(dotLine));
                            Point endPoint = e.GetPosition((IInputElement)sender);
                            bool addEllipse = false;
                            if (lastEllipse == null)
                            {
                                lastEllipse = new Ellipse();
                                addEllipse = true;
                            }
                            if(dotLine == 0)
                            {
                                if(isFillObject == 1)
                                {
                                    lastEllipse.Fill = (Brush)cv.ConvertFromString(color);
                                }
                                lastEllipse.StrokeDashArray = null;
                            }
                            else if (dotLine == 1)
                            {
                                lastEllipse.StrokeDashArray = new DoubleCollection() { 1, 1 };
                            }
                            else if (dotLine == 2)
                            {
                                lastEllipse.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                            }
                            lastEllipse.Stroke = (Brush)cv.ConvertFromString(color);
                            lastEllipse.StrokeThickness = slider.Value;
                            lastEllipse.Width = Math.Abs(startPoint.X - endPoint.X);
                            lastEllipse.Height = Math.Abs(startPoint.Y - endPoint.Y);
                            var x = Math.Min(startPoint.X, endPoint.X);
                            var y = Math.Min(startPoint.Y, endPoint.Y);
                            Canvas.SetLeft(lastEllipse, x);
                            Canvas.SetTop(lastEllipse, y);
                            if (addEllipse)
                                paintCanvas.Children.Add(lastEllipse);
                          
                            break;

                        }
                    case 3:
                        {
                            
                            //MessageBox.Show(Convert.ToString(dotLine));
                            Point endPoint = e.GetPosition((IInputElement)sender);
                            bool addRec = false;
                            if (lastRec == null)
                            {
                                lastRec = new Rectangle();
                                addRec = true;
                            }

                            lastRec.Stroke = (Brush)cv.ConvertFromString(color);
                            lastRec.StrokeThickness = slider.Value;
                            if(dotLine == 0)
                            {
                                if(isFillObject == 1)
                                {
                                    lastRec.Fill = (Brush)cv.ConvertFromString(color);
                                }
                                lastRec.StrokeDashArray = null;
                            }
                            else if (dotLine == 1)
                            {
                                lastRec.StrokeDashArray = new DoubleCollection() { 1, 1 };
                            }
                            else if (dotLine == 2)
                            {
                                lastRec.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                            }
                            lastRec.Width = Math.Abs(startPoint.X - endPoint.X);
                            lastRec.Height = Math.Abs(startPoint.Y - endPoint.Y);
                            var x = Math.Min(startPoint.X, endPoint.X);
                            var y = Math.Min(startPoint.Y, endPoint.Y);
                            Canvas.SetLeft(lastRec, x);
                            Canvas.SetTop(lastRec, y);
                            if (addRec)
                                paintCanvas.Children.Add(lastRec);
                            
                            break;
                        }
                    case 4:  //SELECT
                        {
                            Point endPoint = e.GetPosition((IInputElement)sender);
                            bool addRec = false;
                            if (lastRecSelect == null)
                            {
                                lastRecSelect = new Rectangle();
                                addRec = true;
                            }

                            lastRecSelect.Stroke = (Brush)cv.ConvertFromString(color);
                            lastRecSelect.StrokeThickness = 2;
                            
                            lastRecSelect.StrokeDashArray = new DoubleCollection() { 1, 1 };
                            
                            lastRecSelect.Width = Math.Abs(startPoint.X - endPoint.X);
                            lastRecSelect.Height = Math.Abs(startPoint.Y - endPoint.Y);
                            var x = Math.Min(startPoint.X, endPoint.X);
                            var y = Math.Min(startPoint.Y, endPoint.Y);
                            Canvas.SetLeft(lastRecSelect, x);
                            Canvas.SetTop(lastRecSelect, y);
                            if (addRec)
                                paintCanvas.Children.Add(lastRecSelect);
                            break;
                        }
                    case 5:  //TEXT
                        {
                            if (isMouseUp != true)
                            {
                                Point endPoint = e.GetPosition((IInputElement)sender);
                                bool addRec = false;
                                if (lastRecSelect == null)
                                {
                                    lastRecSelect = new Rectangle();
                                    addRec = true;
                                }

                                lastRecSelect.Stroke = (Brush)cv.ConvertFromString(color);
                                lastRecSelect.StrokeThickness = 2;

                                lastRecSelect.StrokeDashArray = new DoubleCollection() { 1, 1 };

                                lastRecSelect.Width = Math.Abs(startPoint.X - endPoint.X);
                                lastRecSelect.Height = Math.Abs(startPoint.Y - endPoint.Y);
                                var x = Math.Min(startPoint.X, endPoint.X);
                                var y = Math.Min(startPoint.Y, endPoint.Y);
                                Canvas.SetLeft(lastRecSelect, x);
                                Canvas.SetTop(lastRecSelect, y);
                                if (addRec)
                                    paintCanvas.Children.Add(lastRecSelect);
                            }
                            break;
                        }
                    case 6:   // STAR
                        {
                            if (isMouseDowned)
                            {
                                isMove = true;
                                Point cur = e.GetPosition((IInputElement)sender);
                                if (shapemove == false)
                                {
                                    shapemove = true;
                                }
                                else
                                {
                                    paintCanvas.Children.Remove(Star);
                                }
                                Drawshape(cur);
                                paintCanvas.Children.Add(Star);
                            }
                            break;
                        }
                    case 7:
                        {
                            if (isMouseDowned)
                            {
                                isMove = true;
                                Point cur = e.GetPosition((IInputElement)sender);
                                if (shapemove == false)
                                {
                                    shapemove = true;
                                }
                                else
                                {
                                    paintCanvas.Children.Remove(Heart);
                                }
                                Drawshape(cur);
                                paintCanvas.Children.Add(Heart);
                            }
                            break;
                        }
                    case 8:
                        {
                            if (isMouseDowned)
                            {
                                isMove = true;
                                Point cur = e.GetPosition((IInputElement)sender);
                                if (shapemove == false)
                                {
                                    shapemove = true;
                                }
                                else
                                {
                                    paintCanvas.Children.Remove(Arrow);
                                }
                                Drawshape(cur);
                                paintCanvas.Children.Add(Arrow);
                            }
                            break;
                        }

                }
            }

            }
        private void Drawshape(Point endPoint)
        {
            if (type == 6)
            {
                var x = Math.Min(endPoint.X, startPoint.X);
                var y = Math.Min(endPoint.Y, startPoint.Y);
                var w = Math.Max(endPoint.X, startPoint.X) - x;
                var h = Math.Max(endPoint.Y, startPoint.Y) - y;
                Star.Width = w;

                Star.Height = h;
                Canvas.SetLeft(Star, x);
                Canvas.SetTop(Star, y);
            }
            else if (type == 7)
            {
                var x = Math.Min(endPoint.X, startPoint.X);
                var y = Math.Min(endPoint.Y, startPoint.Y);
                var w = Math.Max(endPoint.X, startPoint.X) - x;
                var h = Math.Max(endPoint.Y, startPoint.Y) - y;
                Heart.Width = w;
                Heart.Height = h;
                Canvas.SetLeft(Heart, x);
                Canvas.SetTop(Heart, y);
            }
            else if ( type == 8)
            {
                var x = Math.Min(endPoint.X, startPoint.X);
                var y = Math.Min(endPoint.Y, startPoint.Y);
                var w = Math.Max(endPoint.X, startPoint.X) - x;
                var h = Math.Max(endPoint.Y, startPoint.Y) - y;
                Arrow.Width = w;
                Arrow.Height = h;
                Canvas.SetLeft(Arrow, x);
                Canvas.SetTop(Arrow, y);
            }
        }
        bool shapemove = false;
        public Color GetPixel(BitmapSource bitmap, int x, int y)
{
    CroppedBitmap cb = new CroppedBitmap(bitmap, new Int32Rect(x, y, 1, 1));
    byte[] pixel = new byte[bitmap.Format.BitsPerPixel / 8];
    cb.CopyPixels(pixel, bitmap.Format.BitsPerPixel / 8, 0);
    return Color.FromRgb(pixel[2], pixel[1], pixel[0]);
}

        private void paintCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (type)
            {
                
                case 0:
                    {
                        isMouseDowned = false;
                        isMouseUp = true;
                        Rect rect = new Rect(paintCanvas.Margin.Left, paintCanvas.Margin.Top, paintCanvas.ActualWidth, paintCanvas.ActualHeight);
                        double dpi = 96d;
                        RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, dpi, dpi, System.Windows.Media.PixelFormats.Default);
                        rtb.Render(paintCanvas);
                        //Image a = new Image();
                        a = new Image();
                        a.Source = rtb;
                        backGround.Push(rtb);

                        paintCanvas.Children.Clear();
                        ImageBrush br = new ImageBrush();
                        br.ImageSource = rtb;

                        paintCanvas.Background = br;
                        
                        break;

                    }
                case 1:
                    {
                        isMouseUp = true;
                        Point endPoint = e.GetPosition((IInputElement)sender);
                        Line line = new Line();
                        line.Stroke = (Brush)cv.ConvertFromString(color);

                        if (dotLine == 1)
                        {
                            line.StrokeDashArray = new DoubleCollection() { 1, 1 };
                        }
                        else if (dotLine == 2)
                        {
                            line.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                        }
                        else if (dotLine == 0)
                        {
                            line.StrokeDashArray = null;
                        }

                        line.StrokeThickness = slider.Value;
                        line.X1 = startPoint.X;
                        line.Y1 = startPoint.Y;
                        line.X2 = endPoint.X;
                        line.Y2 = endPoint.Y;
                        Rect rect = new Rect(paintCanvas.Margin.Left, paintCanvas.Margin.Top, paintCanvas.ActualWidth, paintCanvas.ActualHeight);
                        double dpi = 96d;
                        RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, dpi, dpi, System.Windows.Media.PixelFormats.Default);
                        rtb.Render(paintCanvas);
                        
                        a = new Image();
                        a.Source = rtb;
                        backGround.Push(rtb);

                        paintCanvas.Children.Clear();
                        ImageBrush br = new ImageBrush();
                        br.ImageSource = rtb;
                        
                        paintCanvas.Background = br;
                        paintCanvas.Children.Add(line);
                        
                        isMouseDowned = false;
                        break;
                        
                    }
                case 2:
                    {
                        paintCanvas.Children.Remove(lastEllipse);
                        lastEllipse = null;
                        Point endPoint = e.GetPosition((IInputElement)sender);
                        Ellipse ellipse = new Ellipse();
                        ellipse.Stroke = (Brush)cv.ConvertFromString(color);
                        ellipse.StrokeThickness = slider.Value;
                        if (dotLine == 0)
                        {
                            if (isFillObject == 1)
                            {
                                ellipse.Fill = (Brush)cv.ConvertFromString(color);
                            }
                            ellipse.StrokeDashArray = null;
                        }
                        else if (dotLine == 1)
                        {
                            ellipse.StrokeDashArray = new DoubleCollection() { 1, 1 };
                        }
                        else if (dotLine == 2)
                        {
                            ellipse.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                        }

                        ellipse.Width = Math.Abs(startPoint.X - endPoint.X);
                        ellipse.Height = Math.Abs(startPoint.Y - endPoint.Y);
                        var x = Math.Min(startPoint.X, endPoint.X);
                        var y = Math.Min(startPoint.Y, endPoint.Y);
                        Canvas.SetLeft(ellipse, x);
                        Canvas.SetTop(ellipse, y);


                       
                        ctrl = new ContentControl();
                        ctrl.MinHeight = 1;
                        ctrl.MinWidth = 1;
                        ctrl.Height = ellipse.Height;
                        ctrl.Width = ellipse.Width;
                        var t = ellipse.GetValue(Canvas.LeftProperty);
                        ctrl.SetValue(Canvas.LeftProperty, t);
                        t = ellipse.GetValue(Canvas.TopProperty);
                        ctrl.SetValue(Canvas.TopProperty, t);
                        ctrl.SetValue(Selector.IsSelectedProperty, true);
                        ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("Resources/DesignerItem.xaml", UriKind.Relative));
                        Style st = (Style)res["DesignerItemStyle"];
                        ctrl.Style = st;
                        ellipse.Width = Double.NaN;
                        ellipse.Height = Double.NaN;
                        ellipse.IsHitTestVisible = false;
                        ctrl.Content = ellipse;
                        
                       
                        Canvas.SetLeft(tempE, x);
                        Canvas.SetTop(tempE, y);
                       // paintCanvas.Children.Add(tempE);

                        paintCanvas.Children.Add(ctrl);

                        key = 1;

                        isMouseDowned = false;
                        break;

                    }
                case 3:
                    {
                        paintCanvas.Children.Remove(lastRec);
                        lastRec = null;
                        isMouseUp = true;
                        Point endPoint = e.GetPosition((IInputElement)sender);
                        Rectangle rec = new Rectangle();
                        rec.Stroke = (Brush)cv.ConvertFromString(color);
                        if (dotLine == 0)
                        {
                            if (isFillObject == 1)
                            {
                                rec.Fill = (Brush)cv.ConvertFromString(color);
                            }
                            rec.StrokeDashArray = null;
                        }
                        else if (dotLine == 1)
                        {
                            rec.StrokeDashArray = new DoubleCollection() { 1, 1 };
                        }
                        else if (dotLine == 2)
                        {
                            rec.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                        }
                        rec.StrokeThickness = slider.Value;
                        rec.Width = Math.Abs(startPoint.X - endPoint.X);
                        rec.Height = Math.Abs(startPoint.Y - endPoint.Y);
                        var x = Math.Min(startPoint.X, endPoint.X);
                        var y = Math.Min(startPoint.Y, endPoint.Y);
                        Canvas.SetLeft(rec, x);
                        Canvas.SetTop(rec, y);
                        //ContentControl con = new ContentControl();
                      
                        ctrl = new ContentControl();
                        ctrl.MinHeight = 1;
                        ctrl.MinWidth = 1;
                        ctrl.Height = rec.Height;
                        ctrl.Width = rec.Width;
                        var t = rec.GetValue(Canvas.LeftProperty);
                        ctrl.SetValue(Canvas.LeftProperty, t);
                        t = rec.GetValue(Canvas.TopProperty);
                        ctrl.SetValue(Canvas.TopProperty, t);
                        ctrl.SetValue(Selector.IsSelectedProperty, true);
                        ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("Resources/DesignerItem.xaml", UriKind.Relative));
                        Style st = (Style)res["DesignerItemStyle"];
                        ctrl.Style = st;
                        rec.Width = Double.NaN;
                        rec.Height = Double.NaN;
                        rec.IsHitTestVisible = false;
                        ctrl.Content = rec;
                        paintCanvas.Children.Add(ctrl);
                        key = 2;
                        rec = null;
                        isMouseDowned = false;
                        break;
                        //paintCanvas.Children.Add(rec);
                    }
                case 4:
                    {
                        if(ctrlSelect != null)
                        {
                            paintCanvas.Children.Remove(ctrlSelect);
                            ctrlSelect = null;
                        }
                        paintCanvas.Children.Remove(lastRecSelect);
                        isMouseUp = true;
                        Point endPoint = e.GetPosition((IInputElement)sender);
                        
                        Rectangle position = new Rectangle();
                        ctrlSelect = new ContentControl();
                        position.Fill = paintCanvas.Background;
                        ctrlSelect.Height = position.Height;
                        ctrlSelect.Width = position.Width;
                        ctrlSelect.Width = Math.Abs(startPoint.X - endPoint.X);
                        ctrlSelect.Height = Math.Abs(startPoint.Y - endPoint.Y);
                        position.Width = Math.Abs(startPoint.X - endPoint.X);
                        position.Height = Math.Abs(startPoint.Y - endPoint.Y);
                        Rectangle rect = new Rectangle();
                        rect.Height = position.Height;
                        rect.Width = position.Width;
                        rect.Fill = paintCanvas.Background;
                        double x = Math.Min(endPoint.X, startPoint.X);
                        double y = Math.Min(endPoint.Y, startPoint.Y);
                        Canvas.SetLeft(ctrlSelect, x);
                        Canvas.SetTop(ctrlSelect, y);
                        Canvas.SetLeft(rect, x);
                        Canvas.SetTop(rect, y);
                        ctrlSelect.SetValue(Selector.IsSelectedProperty, true);
                        ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("Resources/DesignerItem.xaml", UriKind.Relative));
                        Style st = (Style)res["DesignerItemStyle"];
                        ctrlSelect.Style = st;
                        //paintCanvas.Children.Clear();
                        RenderTargetBitmap bitmap = new RenderTargetBitmap((int)paintCanvas.ActualWidth, (int)paintCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(paintCanvas);
                        ImageBrush br = new ImageBrush();
                        br.ImageSource = bitmap;
                        paintCanvas.Background = br;
                        BitmapSource bm = new CroppedBitmap(bitmap as BitmapSource, new Int32Rect((int)x, (int)y, (int)position.Width, (int)position.Height));
                        Image a = new Image();
                        Image b = new Image();
                        b.Source = bitmap;
                        a.Source = bm;
                        a.IsHitTestVisible = false;
                        paintCanvas.Children.Clear();
                        
                        
                        ctrlSelect.Content = a;
                        paintCanvas.Children.Add(ctrlSelect);
                        lastRecSelect = null;
                        isMouseDowned = false; 
                        break;
                    }
                case 5:
                    {
                        paintCanvas.Children.Remove(lastRecSelect);
                        paintCanvas.Children.Remove(ctrl);
                        isMouseUp = true;
                        Rect rect = new Rect(paintCanvas.Margin.Left, paintCanvas.Margin.Top, paintCanvas.ActualWidth, paintCanvas.ActualHeight);
                        double dpi = 96d;
                        RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, dpi, dpi, System.Windows.Media.PixelFormats.Default);
                        rtb.Render(paintCanvas);                        
                        a = new Image();
                        a.Source = rtb;
                        ImageBrush br = new ImageBrush();
                        br.ImageSource = rtb;
                        backGround.Push(rtb);
                        paintCanvas.Background = br;          
                        Point endPoint = e.GetPosition((IInputElement)sender);
                        TextBox text = new TextBox();
                        text.Height = Double.NaN;
                        text.Width = Double.NaN;
                        text.FontSize = slider3.Value;
                        text.FontFamily = (FontFamily)FONTCOMBO.SelectedItem;
                        text.Foreground =(Brush)cv.ConvertFromString(color);
                        text.FontSize = slider3.Value;
                        if(ctrlSelect != null)
                        {
                            paintCanvas.Children.Remove(ctrlSelect);
                            ctrlSelect = null;
                        }
                        ctrlSelect = new ContentControl();
                        ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(new Uri("Resources/DesignerItem.xaml", UriKind.Relative));
                        Style st = (Style)res["DesignerItemStyle"];
                        ctrlSelect.Style = st;
                        ctrlSelect.MinHeight = 1;
                        ctrlSelect.MinWidth = 1;     
                        ctrlSelect.Height = Math.Abs(startPoint.Y - endPoint.Y);
                        ctrlSelect.Width = Math.Abs(startPoint.X - endPoint.X);
                        var x = Math.Min(startPoint.X, endPoint.X);
                        var y = Math.Min(startPoint.Y, endPoint.Y);
                        Canvas.SetLeft(ctrlSelect, x);
                        Canvas.SetTop(ctrlSelect, y);
                        ctrlSelect.Content = text;                       
                        paintCanvas.Children.Add(ctrlSelect);
                        newshape(object_, e);
                        break;
                    }
                case 6:
                    {
                        isMouseDowned = false;
                        shapemove = false;
                        if (isMove == false)
                            return;
                        isMove = false;
                        Point endPoint3 = e.GetPosition((IInputElement)sender);
                        getshapeToContent(endPoint3);
                        if (ctrl != null)
                            paintCanvas.Children.Add(ctrl);
                        key = 3;
                        break;
                    }
                case 7:
                    {
                        isMouseDowned = false;
                        shapemove = false;
                        if (isMove == false)
                            return;
                        isMove = false;
                        Point endPoint3 = e.GetPosition((IInputElement)sender);
                        getshapeToContent(endPoint3);
                        if (ctrl != null)
                            paintCanvas.Children.Add(ctrl);
                        key = 4;
                        break;
                    }
                case 8:
                    {
                        isMouseDowned = false;
                        shapemove = false;
                        if (isMove == false)
                            return;
                        isMove = false;
                        Point endPoint3 = e.GetPosition((IInputElement)sender);
                        getshapeToContent(endPoint3);
                        if (ctrl != null)
                            paintCanvas.Children.Add(ctrl);
                        key = 5;
                        break;
                    }
            }
        }

        void getshapeToContent(Point endPoint)
        {
            if (type == 6)
            {
                paintCanvas.Children.Remove(Star);
                ctrl = new ContentControl();
                ctrl.Width = Star.Width;
                ctrl.Height = Star.Height;
                var t2 = Star.GetValue(Canvas.LeftProperty);
                var r2 = Star.GetValue(Canvas.TopProperty);
                ctrl.SetValue(Canvas.LeftProperty, t2);
                ctrl.SetValue(Canvas.TopProperty, r2);
                ctrl.SetValue(System.Windows.Controls.Primitives.Selector.IsSelectedProperty, true);
                ResourceDictionary res2 = (ResourceDictionary)System.Windows.Application.LoadComponent(new Uri("Resources/DesignerItem.xaml", UriKind.Relative));
                Style st2 = (Style)res2["DesignerItemStyle"];
                ctrl.Style = st2;

                Star.Width = Double.NaN;
                Star.Height = Double.NaN;
                Star.IsHitTestVisible = false;
                ctrl.Content = Star;
                Star = null;
            }
            else if (type == 7)
            {
                paintCanvas.Children.Remove(Heart);
                ctrl = new ContentControl();
                ctrl.Width = Heart.Width;
                ctrl.Height = Heart.Height;
                var t2 = Heart.GetValue(Canvas.LeftProperty);
                var r2 = Heart.GetValue(Canvas.TopProperty);
                ctrl.SetValue(Canvas.LeftProperty, t2);
                ctrl.SetValue(Canvas.TopProperty, r2);
                ctrl.SetValue(System.Windows.Controls.Primitives.Selector.IsSelectedProperty, true);
                ResourceDictionary res2 = (ResourceDictionary)System.Windows.Application.LoadComponent(new Uri("Resources/DesignerItem.xaml", UriKind.Relative));
                Style st2 = (Style)res2["DesignerItemStyle"];
                ctrl.Style = st2;

                Heart.Width = Double.NaN;
                Heart.Height = Double.NaN;
                Heart.IsHitTestVisible = false;
                ctrl.Content = Heart;
                Heart = null;
            }
            else if (type == 8)
            {
                paintCanvas.Children.Remove(Arrow);
                ctrl = new ContentControl();
                ctrl.Width = Arrow.Width;
                ctrl.Height = Arrow.Height;
                var t2 = Arrow.GetValue(Canvas.LeftProperty);
                var r2 = Arrow.GetValue(Canvas.TopProperty);
                ctrl.SetValue(Canvas.LeftProperty, t2);
                ctrl.SetValue(Canvas.TopProperty, r2);
                ctrl.SetValue(System.Windows.Controls.Primitives.Selector.IsSelectedProperty, true);
                ResourceDictionary res2 = (ResourceDictionary)System.Windows.Application.LoadComponent(new Uri("Resources/DesignerItem.xaml", UriKind.Relative));
                Style st2 = (Style)res2["DesignerItemStyle"];
                ctrl.Style = st2;

                Arrow.Width = Double.NaN;
                Arrow.Height = Double.NaN;
                Arrow.IsHitTestVisible = false;
                ctrl.Content = Arrow;
                Arrow = null;
            }
            IsContent = true;
        }

        private void paintCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            newshape(object_, e);
            if (isFill == 0)
            {
                startPoint = e.GetPosition(paintCanvas);
                isMouseDowned = true;
                isMouseUp = false;
                

                if (ctrl != null)
                {
                    if (key == 1)
                    {
                        tempE = null;
                        tempE = new Ellipse();


                        tempE.Height = ctrl.Height;
                        tempE.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        tempE.RenderTransform = rotate;
                        tempE.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        tempE.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        tempE.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        tempE.SetValue(Canvas.TopProperty, tem);
                        ctrl.RenderTransform = null;
                        ctrl.SetValue(Selector.IsSelectedProperty, false);
                        ctrl.Content = null;
                        ctrl = null;
                        if (dotLine == 0)
                        {
                            if (isFillObject == 1)
                            {
                                tempE.Fill = (Brush)cv.ConvertFromString(color);
                            }
                            tempE.StrokeDashArray = null;
                        }
                        else if (dotLine == 1)
                        {
                            tempE.StrokeDashArray = new DoubleCollection() { 1, 1 };
                        }
                        else if (dotLine == 2)
                        {
                            tempE.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                        }


                        tempE.Stroke = (Brush)cv.ConvertFromString(color);
                        tempE.StrokeThickness = slider.Value;
                        paintCanvas.Children.Add(tempE);
                    }
                    else if (key == 2)
                    {

                        tempR = null;
                        tempR = new Rectangle();


                        tempR.Height = ctrl.Height;
                        tempR.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        tempR.RenderTransform = rotate;
                        tempR.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        tempR.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        tempR.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        tempR.SetValue(Canvas.TopProperty, tem);
                        ctrl.RenderTransform = null;
                        ctrl.SetValue(Selector.IsSelectedProperty, false);
                        ctrl.Content = null;
                        ctrl = null;
                        if (dotLine == 0)
                        {
                            if (isFillObject == 1)
                            {
                                tempR.Fill = (Brush)cv.ConvertFromString(color);
                            }
                            tempR.StrokeDashArray = null;
                        }
                        else if (dotLine == 1)
                        {
                            tempR.StrokeDashArray = new DoubleCollection() { 1, 1 };
                        }
                        else if (dotLine == 2)
                        {
                            tempR.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                        }


                        tempR.Stroke = (Brush)cv.ConvertFromString(color);
                        tempR.StrokeThickness = slider.Value;
                        paintCanvas.Children.Add(tempR);

                    }
                    else if (key == 3)
                    {
                        Star1 = new System.Windows.Shapes.Path();
                        Star1.Data = Geometry.Parse("F1 M 50,0L 34.45,33L 0,37.99L 25,64L 19.1,100L 50,83L 81,100L 75,64L 100,37.99L 65.5,32.9L 50,0 Z");
                        Star1.Stroke = (Brush)cv.ConvertFromString(color);

                        Star1.StrokeThickness = slider2.Value;

                        Star1.Stretch = Stretch.Fill;

                        Star1.Height = ctrl.Height;
                        Star1.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        Star1.RenderTransform = rotate;
                        Star1.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        Star1.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        Star1.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        Star1.SetValue(Canvas.TopProperty, tem);
                        paintCanvas.Children.Remove(ctrl);
                        ctrl.Content = null;
                        paintCanvas.Children.Add(Star1);
                    }
                    else if (key == 4)
                    {
                        Heart1 = new System.Windows.Shapes.Path();
                        Heart1.Data = Geometry.Parse("F1 M 240,200 A 20,20 0 0 0 200,240 C 210,250 240,270 240,270 C 240,270 260,260 280,240 A 20,20 0 0 0 240,200 Z");
                        Heart1.Stroke = (Brush)cv.ConvertFromString(color);

                        Heart1.StrokeThickness = slider2.Value;

                        Heart1.Stretch = Stretch.Fill;

                        Heart1.Height = ctrl.Height;
                        Heart1.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        Heart1.RenderTransform = rotate;
                        Heart1.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        Heart1.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        Heart1.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        Heart1.SetValue(Canvas.TopProperty, tem);
                        paintCanvas.Children.Remove(ctrl);
                        ctrl.Content = null;
                        paintCanvas.Children.Add(Heart1);
                    }
                    else if (key == 5)
                    {
                        Arrow1 = new System.Windows.Shapes.Path();
                        Arrow1.Data = Geometry.Parse("F1 M 527.311,139.851L 499.019,120.735L 499.019,130.675L 464.611,130.675L 464.611,149.027L 499.019,149.027L 499.019,158.967L 527.311,139.851 Z");
                        Arrow1.Stroke = (Brush)cv.ConvertFromString(color);

                        Arrow1.StrokeThickness = slider2.Value;

                        Arrow1.Stretch = Stretch.Fill;

                        Arrow1.Height = ctrl.Height;
                        Arrow1.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        Arrow1.RenderTransform = rotate;
                        Arrow1.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        Arrow1.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        Arrow1.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        Arrow1.SetValue(Canvas.TopProperty, tem);
                        paintCanvas.Children.Remove(ctrl);
                        ctrl.Content = null;
                        paintCanvas.Children.Add(Arrow1);
                    }
                    Rect rect = new Rect(paintCanvas.Margin.Left, paintCanvas.Margin.Top, paintCanvas.ActualWidth, paintCanvas.ActualHeight);
                    double dpi = 96d;
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, dpi, dpi, System.Windows.Media.PixelFormats.Default);
                    rtb.Render(paintCanvas);
                    a = new Image();
                    a.Source = rtb;
                    ImageBrush br = new ImageBrush();
                    br.ImageSource = rtb;
                    backGround.Push(rtb);
                    //if (ctrl != null)
                        //paintCanvas.Children.Remove(ctrl);
                    //paintCanvas.Children.Clear();
                    //paintCanvas.Children.Add(a);
                    paintCanvas.Background = br;
                    if(ctrlSelect != null)
                    {
                        paintCanvas.Children.Remove(ctrlSelect);
                    }

                }

                //MessageBox.Show(Convert.ToString(size));
            }
            if (isFill == 1)
            {
                // PixelFormatConverter px = new PixelFormatConverter;
                isMouseUp = false;
                if (type == 6 || type == 7 || type == 8)
                {
                    newshape(object_, e);
                }

                if (ctrl != null)
                {
                    if (key == 1)
                    {
                        tempE = null;
                        tempE = new Ellipse();


                        tempE.Height = ctrl.Height;
                        tempE.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        tempE.RenderTransform = rotate;
                        tempE.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        tempE.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        tempE.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        tempE.SetValue(Canvas.TopProperty, tem);
                        ctrl.RenderTransform = null;
                        ctrl.SetValue(Selector.IsSelectedProperty, false);
                        ctrl.Content = null;
                        ctrl = null;
                        if (dotLine == 0)
                        {
                            if (isFillObject == 1)
                            {
                                tempE.Fill = (Brush)cv.ConvertFromString(color);
                            }
                            tempE.StrokeDashArray = null;
                        }
                        else if (dotLine == 1)
                        {
                            tempE.StrokeDashArray = new DoubleCollection() { 1, 1 };
                        }
                        else if (dotLine == 2)
                        {
                            tempE.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                        }


                        tempE.Stroke = (Brush)cv.ConvertFromString(color);
                        tempE.StrokeThickness = slider.Value;
                        paintCanvas.Children.Add(tempE);
                    }
                    else if (key == 2)
                    {

                        tempR = null;
                        tempR = new Rectangle();


                        tempR.Height = ctrl.Height;
                        tempR.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        tempR.RenderTransform = rotate;
                        tempR.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        tempR.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        tempR.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        tempR.SetValue(Canvas.TopProperty, tem);
                        ctrl.RenderTransform = null;
                        ctrl.SetValue(Selector.IsSelectedProperty, false);
                        ctrl.Content = null;
                        ctrl = null;
                        if (dotLine == 0)
                        {
                            if (isFillObject == 1)
                            {
                                tempR.Fill = (Brush)cv.ConvertFromString(color);
                            }
                            tempR.StrokeDashArray = null;
                        }
                        else if (dotLine == 1)
                        {
                            tempR.StrokeDashArray = new DoubleCollection() { 1, 1 };
                        }
                        else if (dotLine == 2)
                        {
                            tempR.StrokeDashArray = new DoubleCollection() { 10, 5, 10 };
                        }


                        tempR.Stroke = (Brush)cv.ConvertFromString(color);
                        tempR.StrokeThickness = slider.Value;
                        paintCanvas.Children.Add(tempR);

                    }
                    else if (key == 3)
                    {
                        Star1 = new System.Windows.Shapes.Path();
                        Star1.Data = Geometry.Parse("F1 M 50,0L 34.45,33L 0,37.99L 25,64L 19.1,100L 50,83L 81,100L 75,64L 100,37.99L 65.5,32.9L 50,0 Z");
                        Star1.Stroke = (Brush)cv.ConvertFromString(color);

                        Star1.StrokeThickness = slider2.Value;

                        Star1.Stretch = Stretch.Fill;

                        Star1.Height = ctrl.Height;
                        Star1.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        Star1.RenderTransform = rotate;
                        Star1.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        Star1.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        Star1.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        Star1.SetValue(Canvas.TopProperty, tem);
                        paintCanvas.Children.Remove(ctrl);
                        ctrl.Content = null;
                        paintCanvas.Children.Add(Star1);
                    }
                    else if (key == 4)
                    {
                        Heart1 = new System.Windows.Shapes.Path();
                        Heart1.Data = Geometry.Parse("F1 M 240,200 A 20,20 0 0 0 200,240 C 210,250 240,270 240,270 C 240,270 260,260 280,240 A 20,20 0 0 0 240,200 Z");
                        Heart1.Stroke = (Brush)cv.ConvertFromString(color);

                        Heart1.StrokeThickness = slider2.Value;

                        Heart1.Stretch = Stretch.Fill;

                        Heart1.Height = ctrl.Height;
                        Heart1.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        Heart1.RenderTransform = rotate;
                        Heart1.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        Heart1.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        Heart1.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        Heart1.SetValue(Canvas.TopProperty, tem);
                        paintCanvas.Children.Remove(ctrl);
                        ctrl.Content = null;
                        paintCanvas.Children.Add(Heart1);
                    }
                    else if (key == 5)
                    {
                        Arrow1 = new System.Windows.Shapes.Path();
                        Arrow1.Data = Geometry.Parse("F1 M 527.311,139.851L 499.019,120.735L 499.019,130.675L 464.611,130.675L 464.611,149.027L 499.019,149.027L 499.019,158.967L 527.311,139.851 Z");
                        Arrow1.Stroke = (Brush)cv.ConvertFromString(color);

                        Arrow1.StrokeThickness = slider2.Value;

                        Arrow1.Stretch = Stretch.Fill;

                        Arrow1.Height = ctrl.Height;
                        Arrow1.Width = ctrl.Width;
                        RotateTransform rotate = ctrl.RenderTransform as RotateTransform;
                        Arrow1.RenderTransform = rotate;
                        Arrow1.RenderTransformOrigin = ctrl.RenderTransformOrigin;
                        Arrow1.IsHitTestVisible = true;

                        var tem = ctrl.GetValue(Canvas.LeftProperty);
                        Arrow1.SetValue(Canvas.LeftProperty, tem);
                        tem = ctrl.GetValue(Canvas.TopProperty);
                        Arrow1.SetValue(Canvas.TopProperty, tem);
                        paintCanvas.Children.Remove(ctrl);
                        ctrl.Content = null;
                        paintCanvas.Children.Add(Arrow1);
                    }
                    Rect rect = new Rect(paintCanvas.Margin.Left, paintCanvas.Margin.Top, paintCanvas.ActualWidth, paintCanvas.ActualHeight);
                    double dpi = 96d;
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, dpi, dpi, System.Windows.Media.PixelFormats.Default);
                    rtb.Render(paintCanvas);
                    a = new Image();
                    a.Source = rtb;
                    backGround.Push(rtb);
                    ImageBrush br = new ImageBrush();
                    br.ImageSource = rtb;
                    backGround.Push(rtb);
                    //paintCanvas.Children.Clear();
                    //paintCanvas.Children.Add(a);
                    paintCanvas.Background = br;

                }
            }
        }

        private void RibbonApplicationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            /*OpenFileDialog op = new OpenFileDialog();
            string filePath = "";
            //filepath = op.FileName;
            op.Filter = "Image Files(*.jpg)|*.jpg|Image Files( *.jpeg)|*.jpeg|Image Files( *.bmp)|*.bmp";

            Nullable<bool> result = op.ShowDialog();
            if (result == true)
            {
                filePath = op.FileName;
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(@filePath));
                paintCanvas.Background = brush;
            }
            else
                return;*/
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter =
                "Image Files (*.jpg; *.jpeg; *.gif; *.bmp;)|*.jpg; *.jpeg; *.gif; *.bmp";

            if ((bool)dialog.ShowDialog())
            {
                var bitmap = new BitmapImage(new Uri(dialog.FileName));
                var image = new Image { Source = bitmap };
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = bitmap;
                paintCanvas.Background = brush;
            }
        }

        private void RibbonApplicationMenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sv = new SaveFileDialog();
            string filePath = "";
            sv.Filter = "Image Files(*.jpg)|*.jpg|Image Files( *.jpeg)|*.jpeg|Image Files( *.bmp)|*.bmp";

            Nullable<bool> result = sv.ShowDialog();
            if (result == true)
                filePath = sv.FileName;
            else
                return;
            //filePath += ".png";
            Rect rect = new Rect(paintCanvas.Margin.Left, paintCanvas.Margin.Top, paintCanvas.ActualWidth, paintCanvas.ActualHeight);
            double dpi = 96d;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, dpi, dpi, System.Windows.Media.PixelFormats.Default);
            rtb.Render(paintCanvas);
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                pngEncoder.Save(ms);
                ms.Close();
                System.IO.File.WriteAllBytes(filePath, ms.ToArray());
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TEXT_Click(object sender, RoutedEventArgs e)
        {
            type = 5;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BoxSize.Text = Convert.ToString((int)slider.Value);
        }

        private void DOT_Click(object sender, RoutedEventArgs e)
        {
            dotLine = 1;

        }

        private void DASH_Click(object sender, RoutedEventArgs e)
        {
            dotLine = 2;
        }

        private void LINE_Click(object sender, RoutedEventArgs e)
        {
            dotLine = 0;

        }

        private void fill_Click(object sender, RoutedEventArgs e)
        {
            isFill = 1;
        }

        private void fill_Object_Click(object sender, RoutedEventArgs e)
        {
            isFillObject = 1;
            dotLine = 0;

        }

        private void star_click(object sender, RoutedEventArgs e)
        {
            type = 6;
            Star = new System.Windows.Shapes.Path();
        }

        private void Arrow_click(object sender, RoutedEventArgs e)
        {
            type = 8;
            Arrow = new System.Windows.Shapes.Path();
        }

        private void Heart_click(object sender, RoutedEventArgs e)
        {

            type = 7;
            Heart = new System.Windows.Shapes.Path();
        }

        private void color_SelectedColorChanged2(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            color = Convert.ToString(_colorPicker2.SelectedColorText);
        }

        private void slider_ValueChanged2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void color_SelectedColorChanged3(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            color = Convert.ToString(_colorPicker3.SelectedColorText);
        }

        private void slider_ValueChanged3(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void undo_click(object sender, RoutedEventArgs e)
        {
            if (backGround.Count != 0)
            {
                RenderTargetBitmap temp;
                temp = backGround.Pop();
                paintCanvas.Children.Clear();
                ImageBrush br = new ImageBrush();
                br.ImageSource = temp;
                paintCanvas.Background = br;
                redo.Push(temp);
            }
        }

        private void redo_clcik(object sender, RoutedEventArgs e)
        {
            if (redo.Count != 0)
            {
                RenderTargetBitmap temp ;
                temp = redo.Pop();
                paintCanvas.Children.Clear();
                ImageBrush br = new ImageBrush();
                br.ImageSource = temp;
                paintCanvas.Background = br;
                backGround.Push(temp);
            }
        }

        private void FLOOD_FILL(object sender, RoutedEventArgs e)
        {
            type = 8;
        }

        private void RibbonGroup_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }       
    }
}
