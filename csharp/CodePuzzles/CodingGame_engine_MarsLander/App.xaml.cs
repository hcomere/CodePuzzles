using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CodingGame_engine_MarsLander
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            List<Tuple<int, int>> ground = new List<Tuple<int, int>>();
            ground.Add(new Tuple<int, int>(0, 100));
            ground.Add(new Tuple<int, int>(1000, 500));
            ground.Add(new Tuple<int, int>(1500, 100));
            ground.Add(new Tuple<int, int>(3000, 100));
            ground.Add(new Tuple<int, int>(5000, 1500));
            ground.Add(new Tuple<int, int>(6999, 1000));
            
            var window = new MainWindow(7000, 3000, ground);

            MainWindow = window;
            //window.WindowState = WindowState.Maximized;
            window.ResizeMode = ResizeMode.NoResize;
            window.Show();
        }
        
    }
}
