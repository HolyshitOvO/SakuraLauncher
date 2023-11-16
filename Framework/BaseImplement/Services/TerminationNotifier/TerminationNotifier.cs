using System;

namespace HakeQuick.Implementation.Services.TerminationNotifier
{
    internal sealed class TerminationNotifier : ITerminationNotifier
    {
        public event EventHandler TerminationNotified;

        public void NotifyTerminate(Tray.TRAY_DOSOMETHING wantToDo)
        {
            TerminationNotified?.Invoke(wantToDo, null);
        }
    }
}
