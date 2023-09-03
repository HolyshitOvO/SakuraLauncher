using System;

namespace HakeQuick.Abstraction.Base
{
    [Flags]
    public enum KeyFlags : uint
    {
        NONE = 0x0,
        ALT = 0x1,
        CONTROL = 0x2,
        SHIFT = 0x4,
        WIN = 0x8,
        NOREPEAT = 0x4000,
    }
}
