using System;

namespace CandyLauncher.Implementation.Services.TerminationNotifier
{
    public interface ITerminationNotifier
    {
        event EventHandler TerminationNotified;
        void NotifyTerminate(Tray.TRAY_DOSOMETHING wantToDo);
    }
}
