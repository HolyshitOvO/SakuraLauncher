using CandyLauncher.Abstraction.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CandyLauncher.Implementation.Services.HotKey
{
    public interface IHotKeyBuilder
    {
        void SetBinding(Key key, KeyFlags flags);
        IHotKey Build();
    }
}
