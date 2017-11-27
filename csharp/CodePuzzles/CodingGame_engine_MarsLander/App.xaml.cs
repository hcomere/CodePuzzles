using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CodingGame_engine_MarsLander
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var window = new MainWindow(7000, 3000);
            MainWindow = window;
            window.WindowState = WindowState.Maximized;
            window.ResizeMode = ResizeMode.NoResize;
            window.Show();

            List<Tuple<int, int>> ground = new List<Tuple<int, int>>();
            ground.Add(new Tuple<int, int>(0, 100));
            ground.Add(new Tuple<int, int>(1000, 500));
            ground.Add(new Tuple<int, int>(1500, 1500));
            ground.Add(new Tuple<int, int>(3000, 1000));
            ground.Add(new Tuple<int, int>(4000, 150));
            ground.Add(new Tuple<int, int>(5500, 150));
            ground.Add(new Tuple<int, int>(6999, 800));

            foreach(int i in Enumerable.Range(1, ground.Count-1))
                window.DrawFloorLine(ground[i-1].Item1, ground[i-1].Item2, ground[i].Item1, ground[i].Item2);
        }
        
    }
}
