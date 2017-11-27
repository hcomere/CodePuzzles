using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodingGame_engine_MeanMax
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageSource ImageSourceForBitmap(System.Drawing.Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private class Log : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string _Value;

            public string Value
            {
                get { return _Value; }
                set { _Value = value; OnPropertyChanged("Value"); }
            }

            void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private class Looter
        {
            public int Id { get; set; }
            public double Radius { get; set; }
            public int Type { get; set; }
            public int PlayerId { get; set; }
            public Ellipse Ellipse { get; set; }

            public Looter()
            {
                PlayerId = -1;
            }
        }

        private class UnitUpdate
        {
            public int Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Vx { get; set; }
            public int Vy { get; set; }
            public int Type { get; set; }
        }

        private class Tanker
        {
            public int Id { get; set; }
            public double Radius { get; set; }
            public Ellipse Ellipse { get; set; }
        }

        private class Wreck
        {
            public int Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public double Radius { get; set; }
            public Ellipse Ellipse { get; set; }
            public bool Valid { get; set; }
        }

        private const double CANVAS_SIZE = 600;
        private const double TEXTBOX_SIZE = 150;

        private Canvas Canvas { get; set; }

        private double SizeRatio { get; set; }
        private List<TextBox> TextBoxes { get; set; }

        private List<Log> Logs { get; set; }

        private double MapRadius { get; set; }
        private double WaterTownRadius { get; set; }
        private int LooterCount { get; set; }

        private Label TurnLabel { get; set; }
        private int CurrentTurn { get; set; }
        private int TurnCount { get; set; }

        private List<List<UnitUpdate>> TurnUnitUpdates { get; set; }
        private List<List<int>> TurnWrecks { get; set; }
        private List<Looter> Looters { get; set; }
        private List<Wreck> Wrecks { get; set; }
        private List<Tanker> Tankers { get; set; }

        private Ellipse MapEllipse { get; set; }
        private Ellipse WaterTownEllipse { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;
            CurrentTurn = 0;
            TurnCount = 0;

            Canvas = new Canvas();
            Canvas.Background = Brushes.Black;

            Grid grid = new Grid();

            ColumnDefinition col1 = new ColumnDefinition();
            grid.ColumnDefinitions.Add(col1);

            ColumnDefinition col2 = new ColumnDefinition();
            grid.ColumnDefinitions.Add(col2);

            ColumnDefinition col3 = new ColumnDefinition();
            grid.ColumnDefinitions.Add(col3);

            RowDefinition row1 = new RowDefinition();
            row1.Height = new GridLength(CANVAS_SIZE);
            grid.RowDefinitions.Add(row1);

            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(30);
            grid.RowDefinitions.Add(row2);

            RowDefinition row3 = new RowDefinition();
            row3.Height = new GridLength(TEXTBOX_SIZE);
            grid.RowDefinitions.Add(row3);

            Grid.SetRow(Canvas, 0);
            Grid.SetColumnSpan(Canvas, 3);
            Grid.SetColumn(Canvas, 0);

            grid.Children.Add(Canvas);

            TextBoxes = Enumerable.Range(0, 3).Select(i => new TextBox()
            {
                IsReadOnly = true,
                DataContext = this
            }).ToList();

            foreach (int i in Enumerable.Range(0, 3))
            {
                Grid.SetRow(TextBoxes[i], 2);
                Grid.SetColumn(TextBoxes[i], i);
                grid.Children.Add(TextBoxes[i]);
            }

            Button prevButton = new Button
            {
                Content = "Previous turn"
            };
            prevButton.Click += new RoutedEventHandler(HandlePreviousButtonClick);

            Button nextButton = new Button
            {
                Content = "Next turn"
            };
            nextButton.Click += new RoutedEventHandler(HandleNextButtonClick);

            TurnLabel = new Label
            {
                Content = GetTurnLabelContent()
            };

            Grid.SetRow(prevButton, 1);
            Grid.SetColumn(prevButton, 0);

            Grid.SetRow(nextButton, 1);
            Grid.SetColumn(nextButton, 1);

            Grid.SetRow(TurnLabel, 1);
            Grid.SetColumn(TurnLabel, 2);

            grid.Children.Add(prevButton);
            grid.Children.Add(nextButton);
            grid.Children.Add(TurnLabel);


            Content = grid;

            Canvas.Width = CANVAS_SIZE;
            Canvas.Height = CANVAS_SIZE;

            Logs = Enumerable.Range(0, 3).Select(l => new Log()).ToList();
            foreach (int i in Enumerable.Range(0, 3))
            {
                Binding b = new Binding
                {
                    Source = Logs[i],
                    Path = new PropertyPath("Value"),
                    Mode = BindingMode.OneWay
                };

                TextBoxes[i].SetBinding(TextBox.TextProperty, b);
            }
        }

        private string GetTurnLabelContent()
        {
            return "" + (CurrentTurn + 1) + " / " + TurnCount;
        }

        private void HandlePreviousButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentTurn == 0)
                return;

            --CurrentTurn;
            TurnLabel.Content = GetTurnLabelContent();
            DrawTurn(CurrentTurn);
        }

        private void HandleNextButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentTurn >= TurnCount - 1)
                return;

            ++CurrentTurn;
            TurnLabel.Content = GetTurnLabelContent();
            DrawTurn(CurrentTurn);
        }

        private void InitFloor()
        {
            MapEllipse = new Ellipse()
            {
                Width = MapRadius * 2 * SizeRatio,
                Height = MapRadius * 2 * SizeRatio,
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffc756")
            };

            WaterTownEllipse = new Ellipse()
            {
                Width = WaterTownRadius * 2 * SizeRatio,
                Height = WaterTownRadius * 2 * SizeRatio,
                Margin = new Thickness((MapRadius - WaterTownRadius) * SizeRatio, (MapRadius - WaterTownRadius) * SizeRatio, 0, 0),
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#cec496")
            };
        }

        private void DrawTurn(int a_round)
        {
            Canvas.Children.Clear();
            Canvas.Children.Add(MapEllipse);
            Canvas.Children.Add(WaterTownEllipse);

            TurnWrecks[a_round].ForEach(id => Canvas.Children.Add(Wrecks.Find(w => w.Id == id).Ellipse));

            TurnUnitUpdates[a_round].ForEach(uu =>
            {
                double angle = 0;

                if (uu.Vx != 0 && uu.Vy != 0)
                {
                    double ABx = 1;
                    double ABy = 0;
                    double ACx = uu.Vx;
                    double ACy = uu.Vy;


                    double len = Math.Sqrt(ACx * ACx + ACy * ACy);
                    ACx /= len;
                    ACy /= len;

                    angle = Math.Acos(ABx * ACx + ABy * ACy) * 180 / Math.PI;

                    if (ACy < 0)
                        angle = 360 - angle;
                }

                if (uu.Type == Referee.TYPE_TANKER)
                {
                    Tanker tanker = Tankers.Find(t => t.Id == uu.Id);
                    tanker.Ellipse.Margin = new Thickness((uu.X + MapRadius - tanker.Radius) * SizeRatio, (uu.Y + MapRadius - tanker.Radius) * SizeRatio, 0, 0);
                    tanker.Ellipse.RenderTransform = new RotateTransform(angle);

                    if(! Canvas.Children.Contains(tanker.Ellipse))
                        Canvas.Children.Add(tanker.Ellipse);
                }
                else
                {
                    Looter looter = Looters.Find(l => l.Id == uu.Id);
                    looter.Ellipse.Margin = new Thickness((uu.X + MapRadius - looter.Radius) * SizeRatio, (uu.Y + MapRadius - looter.Radius) * SizeRatio, 0, 0);
                    looter.Ellipse.RenderTransform = new RotateTransform(angle);
                }
            });

            Looters.ForEach(l => Canvas.Children.Add(l.Ellipse));

        }

        private void WriteLine(int i, string msg)
        {
            Logs[i].Value += msg + "\n";
        }

        private void InitMap(double a_mapRadius, double a_waterTownRadius)
        {
            SizeRatio = CANVAS_SIZE / (a_mapRadius * 2);
            MapRadius = a_mapRadius;
            WaterTownRadius = a_waterTownRadius;
            InitFloor();
        }

        public void CreateReplay(List<TurnData> a_turnDatas)
        {
            InitMap(Referee.MAP_RADIUS, Referee.WATERTOWN_RADIUS);

            Looters = new List<Looter>();
            Wrecks = new List<Wreck>();
            Tankers = new List<Tanker>();
            TurnUnitUpdates = Enumerable.Range(0, a_turnDatas.Count).Select(updates => new List<UnitUpdate>()).ToList();
            TurnWrecks = Enumerable.Range(0, a_turnDatas.Count).Select(wrekcs => new List<int>()).ToList();
            CurrentTurn = 0;
            TurnCount = a_turnDatas.Count;

            int round = 0;

            a_turnDatas.ForEach(td =>
            {
                td.FrameDatas.ForEach(fd =>
                {
                    string[] tokens = fd.Split(' ');
                    double t = 0;

                    if (tokens[0].Contains('#'))
                    {
                        string sval = tokens[0].Remove(0, 1);
                        t = Double.Parse(sval);
                    }
                    else
                    {
                        int id = -1;
                        int comp = -1;

                        if (tokens[0].Contains('@'))
                        {
                            string[] idTokens = tokens[0].Split('@');
                            id = Int32.Parse(idTokens[0]);
                            comp = Int32.Parse(idTokens[1]);
                        }
                        else
                            id = Int32.Parse(tokens[0]);

                        if (tokens.Length >= 7) // Unit creation
                        {
                            int x = Int32.Parse(tokens[1]);
                            int y = Int32.Parse(tokens[2]);
                            int vx = Int32.Parse(tokens[3]);
                            int vy = Int32.Parse(tokens[4]);

                            int type = Int32.Parse(tokens[5]);
                            double radius = Double.Parse(tokens[6]);

                            switch (type)
                            {
                                case Referee.TYPE_WRECK:

                                    Wrecks.Add(new Wreck
                                    {
                                        Id = id,
                                        X = x,
                                        Y = y,
                                        Radius = radius,
                                        Valid = true,
                                        Ellipse = new Ellipse
                                        {
                                            Width = radius * 2 * SizeRatio,
                                            Height = radius * 2 * SizeRatio,
                                            Fill = new ImageBrush(ImageSourceForBitmap(Properties.Resources.wreck)),
                                            Margin = new Thickness((x + MapRadius - radius) * SizeRatio, (y + MapRadius - radius) * SizeRatio, 0, 0),
                                            RenderTransformOrigin = new Point(0.5, 0.5)
                                        }
                                    });

                                    if(! TurnWrecks[round].Exists(tw => tw == id))
                                        TurnWrecks[round].Add(id);

                                    break;

                                case Referee.TYPE_TANKER:
                                    
                                    Tankers.Add(new Tanker
                                    {
                                        Id = id,
                                        Radius = radius,
                                        Ellipse = new Ellipse()
                                        {
                                            Width = radius * 2 * SizeRatio,
                                            Height = radius * 2 * SizeRatio,
                                            Fill = new ImageBrush(ImageSourceForBitmap(Properties.Resources.tanker)),
                                            RenderTransformOrigin = new Point(0.5, 0.5)
                                        }
                                    });

                                    TurnUnitUpdates[round].Add(new UnitUpdate
                                    {
                                        Id = id,
                                        X = x,
                                        Y = y,
                                        Vx = vx,
                                        Vy = vy,
                                        Type = type
                                    });

                                    break;

                                case Referee.LOOTER_REAPER:
                                case Referee.LOOTER_DESTROYER:
                                case Referee.LOOTER_DOOF:
                                    int playerIndex = Int32.Parse(tokens[7]);

                                    ImageBrush reaperBrush = new ImageBrush();
                                    string image = "";

                                    switch (playerIndex)
                                    {
                                        case 0: image += "y"; break;
                                        case 1: image += "r"; break;
                                        case 2: image += "b"; break;
                                    }

                                    switch (type)
                                    {
                                        case Referee.LOOTER_REAPER: image += "reaper"; break;
                                        case Referee.LOOTER_DESTROYER: image += "destroyer"; break;
                                        case Referee.LOOTER_DOOF: image += "doof"; break;
                                    }

                                    reaperBrush.ImageSource = ImageSourceForBitmap((System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject(image));

                                    Looters.Add(new Looter
                                    {
                                        Id = id,
                                        PlayerId = playerIndex,
                                        Radius = radius,
                                        Type = type,
                                        Ellipse = new Ellipse()
                                        {
                                            Width = radius * 2 * SizeRatio,
                                            Height = radius * 2 * SizeRatio,
                                            Fill = reaperBrush,
                                            RenderTransformOrigin = new Point(0.5, 0.5)
                                        }
                                    });

                                    TurnUnitUpdates[round].Add(new UnitUpdate
                                    {
                                        Id = id,
                                        X = x,
                                        Y = y,
                                        Vx = vx,
                                        Vy = vy,
                                        Type = type
                                    });

                                    break;
                            }
                        }
                        else // Unit update
                        {
                            if (tokens.Length >= 5)
                            {
                                // HACK
                                int type = Referee.LOOTER_REAPER;
                                if (comp > -1)
                                    type = Referee.TYPE_TANKER;

                                if (tokens.Length == 5)
                                {
                                    int x = Int32.Parse(tokens[1]);
                                    int y = Int32.Parse(tokens[2]);
                                    int vx = Int32.Parse(tokens[3]);
                                    int vy = Int32.Parse(tokens[4]);

                                    TurnUnitUpdates[round].Add(new UnitUpdate
                                    {
                                        Id = id,
                                        X = x,
                                        Y = y,
                                        Vx = vx,
                                        Vy = vy,
                                        Type = type
                                    });
                                } 
                                else
                                {
                                    UnitUpdate uu = TurnUnitUpdates[round].Find(tuu => tuu.Id == id);
                                    if (uu != null)
                                        TurnUnitUpdates[round].Remove(uu);
                                }
                            }
                            else
                            {
                                if (tokens.Length == 1) // add
                                {
                                    if (!TurnWrecks[round].Exists(tw => tw == id))
                                        TurnWrecks[round].Add(id);
                                }
                                else // delete
                                {
                                    TurnWrecks[round].Remove(id);
                                }
                            }
                        }
                    }
                });

                ++round;
            });

            TurnLabel.Content = GetTurnLabelContent();
            DrawTurn(CurrentTurn);
        }
    }
}
