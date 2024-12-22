using CandyLauncher.Abstraction.Action;
using CandyLauncher.Abstraction.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CandyLauncher.Implementation.Base
{
    internal interface IInternalContext : IQuickContext
    {
        IInternalCancellationProvider InternalCancellationProvider { get; }

        Task WaitResults(ObservableCollection<ActionBase> list);
    }
}
