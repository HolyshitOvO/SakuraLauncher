using System;

namespace HakeQuick.Abstraction.Base
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
    /// </summary>
    [Flags]
    public enum KeyFlags : uint
    {
        NONE = 0x0,
        NOREPEAT = 0x4000,

        CTRL = 0x2,
        CONTROL = 0x2,
        SHIFT = 0x4,
        ALT = 0x1,
        WIN = 0x8,
    }
}
