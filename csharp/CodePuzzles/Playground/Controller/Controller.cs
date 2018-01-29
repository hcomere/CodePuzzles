using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace Playground
{
    class Controller
    {
        public class UnitEventArgs : EventArgs
        {
            public Unit Unit { get; set; }
            public bool IsPlayer { get; set; }
        };

        private Dispatcher dispUI = Dispatcher.CurrentDispatcher;
        private Stopwatch Stopwatch { get; set; }

        private static uint s_cellSize = 3;
        
        private List<Unit> PlayerUnits { get; set; }
        private List<Unit> EnnemyUnits { get; set; }
        private Timer UpdateTimer { get; set; }


        private AttackAction AttackAction { get; set; }
        private int Stage { get; set; }

        public delegate void UnitCreatedHandler(Controller a_controller, UnitEventArgs a_args);
        public delegate void UnitDestroyedHandler(Controller a_controller, UnitEventArgs a_args);
        public delegate void UpdateDoneHandler(Controller controller);

        public event UnitCreatedHandler UnitCreated;
        public event UnitDestroyedHandler UnitDestroyed;
        public event UpdateDoneHandler UpdateDone;

        public Controller()
        {
            UpdateTimer = new Timer(33);
            PlayerUnits = new List<Unit>();
            EnnemyUnits = new List<Unit>();
            Stopwatch = new Stopwatch();
            UpdateTimer.Elapsed += OnUpdateTimerTick;
        }

        public void Initialize()
        {
            InitializeStage(1);
            CreatePlayerUnit();
            Stopwatch.Start();
            UpdateTimer.Start();
        }

        public void CreatePlayerUnit()
        {
            Unit newUnit = new Unit(PlayerUnits.Count * s_cellSize, -5, 10, 1);
            PlayerUnits.Add(newUnit);

            UnitEventArgs args = new UnitEventArgs();
            args.Unit = newUnit;
            args.IsPlayer = true;
            UnitCreated(this, args);
        }

        private void InitializeStage(int a_stage)
        {
            Stage = a_stage;

            int ennemyCount = Stage / 10 + 1;

            EnnemyUnits.Clear();
            foreach(int i in Enumerable.Range(0, ennemyCount))
            {
                Unit newUnit = new Unit(i * s_cellSize, 5, 10, 1);

                UnitEventArgs args = new UnitEventArgs();
                args.Unit = newUnit;
                args.IsPlayer = false;

                EnnemyUnits.Add(newUnit);
                UnitCreated(this, args);
            }
        }

        private void OnUpdateTimerTick(object sender, ElapsedEventArgs e)
        {
            Stopwatch.Stop();
            int elapsedMs = (int) Stopwatch.ElapsedMilliseconds;
            Stopwatch.Restart();

            if (AttackAction == null)
                AttackAction = new AttackAction(PlayerUnits[0], EnnemyUnits[0]);
            else
                AttackAction.Forward(elapsedMs);
            
            dispUI.BeginInvoke(new Action(() =>
            {
                UpdateDone(this);
            }), null);
        }
    }
}
