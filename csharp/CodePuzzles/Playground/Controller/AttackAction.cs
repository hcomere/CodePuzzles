using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    class AttackAction
    {
        private enum State
        {
            GO,
            BACK
        };

        private Unit Source { get; set; }
        private Unit Target { get; set; }
        private State State { get; set; }

        public delegate void ActionTerminatedHandler(AttackAction a_action);
        public event ActionTerminatedHandler ActionTerminated;

        public AttackAction(Unit a_source, Unit a_target)
        {
            Source = a_source;
            Target = a_target;
            State = State.GO;
        }

        public void Forward(int a_elapsedMs)
        {
        }
    }
}
