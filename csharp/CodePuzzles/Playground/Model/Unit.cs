using devDept.Geometry;
using System.ComponentModel;

namespace Playground
{
    class Unit : INotifyPropertyChanged
    {
        private static uint NextUnitUId = 0;

        private Point2D m_position;

        public uint UId { get; private set; }
        
        public uint HitPoint { get; private set; }
        public uint Attack { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Unit(double a_x, double a_y, uint a_hp, uint a_attack)
        {
            m_position = new Point2D(a_x, a_y);

            UId = NextUnitUId++;
            HitPoint = a_hp;
            Attack = a_attack;
        }

        public (double X, double Y) Position
        {
            get { return (m_position.X, m_position.Y); }
            set
            {
                m_position.X = value.X;
                m_position.Y = value.Y;
                OnPropertyChanged("Position");
            }
        }

        public override string ToString()
        {
            return "Unit" + UId;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
