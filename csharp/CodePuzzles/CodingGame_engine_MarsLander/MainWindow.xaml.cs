using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodingGame_engine_MarsLander
{
    public partial class MainWindow : Window
    {
        private Canvas Canvas { get; set; }
        private int ZoneWidth { get; set; }
        private int ZoneHeight { get; set; }
        private double WidthRatio { get; set; }
        private double HeightRatio { get; set; }

        private List<Tuple <Line, System.Windows.Shapes.Line>> Ground { get; set; }

        public MainWindow(int a_zoneWidth, int a_zoneHeight, List<Tuple<int, int>> a_ground)
        {
            InitializeComponent();

            Canvas = new Canvas();
            Canvas.Background = Brushes.Black;
            Content = Canvas;

            ZoneWidth = a_zoneWidth;
            ZoneHeight = a_zoneHeight;

            double whratio = ZoneWidth / ZoneHeight;
            Width = 1000;
            Height = Width / whratio;

            Ground = Enumerable.Range(1, a_ground.Count - 1).Select(i => new Tuple < Line, System.Windows.Shapes.Line >(
                new Line(a_ground[i - 1].Item1, a_ground[i - 1].Item2, a_ground[i].Item1, a_ground[i].Item2),
                new System.Windows.Shapes.Line
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 3
                }
                )).ToList();

            Ground.ForEach(g => Canvas.Children.Add(g.Item2));

            Canvas.SizeChanged += new SizeChangedEventHandler(UpdateDrawRatio);   
        }

        public void UpdateDrawRatio(object sender, SizeChangedEventArgs e)
        {
            WidthRatio = e.NewSize.Width / ZoneWidth;
            HeightRatio = e.NewSize.Height / ZoneHeight;
            Redraw((int) e.NewSize.Width, (int) e.NewSize.Height);
        }

        private void Redraw(int a_canvasWidth, int a_canvasHeight)
        {
            Ground.ForEach(g =>
            {
                g.Item2.X1 = g.Item1.StartX * WidthRatio;
                g.Item2.Y1 = a_canvasHeight - g.Item1.StartY * HeightRatio;
                g.Item2.X2 = g.Item1.EndX * WidthRatio;
                g.Item2.Y2 = a_canvasHeight - g.Item1.EndY * HeightRatio;
            }); 
        }
    }
}
