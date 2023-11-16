using System;

namespace HakeQuick.Implementation.Services.TerminationNotifier
{
    public interface ITerminationNotifier
    {
        event EventHandler TerminationNotified;
        void NotifyTerminate(Tray.TRAY_DOSOMETHING wantToDo);
    }
}
