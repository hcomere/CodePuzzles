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

namespace CodingGame_engine_MarsLander
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Canvas Canvas { get; set; }
        private int ZoneWidth { get; set; }
        private int ZoneHeight { get; set; }
        private double WidthRatio { get; set; }
        private double HeightRatio { get; set; }

        public MainWindow(int a_zoneWidth, int a_zoneHeight)
        {
            InitializeComponent();

            Canvas = new Canvas();
            Canvas.Background = Brushes.Black;
            Content = Canvas;

            ZoneWidth = a_zoneWidth;
            ZoneHeight = a_zoneHeight;

            WidthRatio = 1;
            HeightRatio = 1;

            SizeChanged += new SizeChangedEventHandler(UpdateDrawRatio);
        }

        public void UpdateDrawRatio(object sender, SizeChangedEventArgs e)
        {
            WidthRatio = e.NewSize.Width / ZoneWidth;
            HeightRatio = e.NewSize.Height / ZoneHeight;
        }

        public void DrawFloorLine(int a_x1, int a_y1, int a_x2, int a_y2)
        {
            Line line = new Line();
            line.Stroke = System.Windows.Media.Brushes.Red;
            line.X1 = a_x1 * WidthRatio;
            line.X2 = a_x2 * WidthRatio;
            line.Y1 = a_y1 * HeightRatio;
            line.Y2 = a_y2 * HeightRatio;
            line.StrokeThickness = 1;
            Canvas.Children.Add(line);
        }

    }
}
