using CandyLauncher.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CandyLauncher.Implementation.Base
{
    internal interface IInternalCancellationProvider : ICancellationProvider, IDisposable
    {
        void Cancel();
    }
}
