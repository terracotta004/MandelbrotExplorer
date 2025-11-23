using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MandelbrotExplorer
{
    public partial class MainWindow : Window
    {
        private const int ImageWidth = 700;
        private const int ImageHeight = 700;

        private double _centerX = -0.5;
        private double _centerY = 0.0;
        private double _zoom = 1.0;
        private int _maxIterations = 200;

        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += MainWindow_KeyDown;

            RenderMandelbrot();
        }

        private void RenderButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(CenterXTextBox.Text, out var cx))
                _centerX = cx;

            if (double.TryParse(CenterYTextBox.Text, out var cy))
                _centerY = cy;

            if (double.TryParse(ZoomTextBox.Text, out var z) && z > 0)
                _zoom = z;

            if (int.TryParse(MaxIterationsTextBox.Text, out var it) && it > 0)
                _maxIterations = it;

            RenderMandelbrot();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            double moveFactor = 0.1 / _zoom;

            switch (e.Key)
            {
                case Key.Left:
                    _centerX -= moveFactor;
                    break;
                case Key.Right:
                    _centerX += moveFactor;
                    break;
                case Key.Up:
                    _centerY += moveFactor;
                    break;
                case Key.Down:
                    _centerY -= moveFactor;
                    break;

                case Key.OemPlus:
                case Key.Add:
                    _zoom *= 1.25;
                    break;

                case Key.OemMinus:
                case Key.Subtract:
                    _zoom /= 1.25;
                    break;

                case Key.OemOpenBrackets:  // [
                    _maxIterations = Math.Max(50, _maxIterations - 50);
                    break;

                case Key.OemCloseBrackets: // ]
                    _maxIterations += 50;
                    break;

                default:
                    return;
            }

            CenterXTextBox.Text = _centerX.ToString("0.########");
            CenterYTextBox.Text = _centerY.ToString("0.########");
            ZoomTextBox.Text = _zoom.ToString("0.###");
            MaxIterationsTextBox.Text = _maxIterations.ToString();

            RenderMandelbrot();
        }

        private void RenderMandelbrot()
        {
            var sw = Stopwatch.StartNew();

            int width = ImageWidth;
            int height = ImageHeight;

            WriteableBitmap wb = new(
                width, height, 96, 96, PixelFormats.Bgra32, null);

            byte[] pixels = new byte[width * height * 4];
            Span<byte> buffer = pixels;

            double scale = 1.0 / _zoom;
            double aspect = (double)width / height;
            double viewWidth = 3.5 * scale;
            double viewHeight = viewWidth / aspect;

            double minX = _centerX - viewWidth / 2.0;
            double minY = _centerY - viewHeight / 2.0;

            int maxIter = _maxIterations;

            int idx = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double a = minX + (x / (double)width) * viewWidth;
                    double b = minY + (y / (double)height) * viewHeight;

                    Complex c = new(a, b);
                    Complex z = Complex.Zero;

                    int iteration = 0;
                    while (iteration < maxIter && z.Magnitude <= 2.0)
                    {
                        z = z * z + c;
                        iteration++;
                    }

                    var color = GetColor(iteration, maxIter);

                    buffer[idx++] = color.B;
                    buffer[idx++] = color.G;
                    buffer[idx++] = color.R;
                    buffer[idx++] = 255;
                }
            }

            int stride = width * 4;
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            MandelbrotImage.Source = wb;

            sw.Stop();

            StatusTextBlock.Text =
                $"Center=({_centerX:0.########},{_centerY:0.########}), " +
                $"Zoom={_zoom:0.###}, MaxIter={maxIter}, " +
                $"Render time={sw.ElapsedMilliseconds} ms";
        }

        private static Color GetColor(int iteration, int maxIterations)
        {
            if (iteration == maxIterations)
                return Colors.Black;

            double t = (double)iteration / maxIterations;

            byte r = (byte)(255 * t);
            byte g = (byte)(255 * Math.Sqrt(t));
            byte b = (byte)(255 * (1.0 - t));

            return Color.FromRgb(r, g, b);
        }
    }
}
