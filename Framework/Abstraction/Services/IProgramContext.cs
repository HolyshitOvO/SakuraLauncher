using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CandyLauncher.Abstraction.Services
{
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    public enum FullscreenMode
    {
        Fullscreen, // 全屏
        Borderless, // 全屏无边框
        Windowed    // 普通窗口
    }

    public interface IProgramContext
    {
        RECT WindowPosition { get; }
        Process CurrentProcess { get; }
        IntPtr WindowHandle { get; }
        IntPtr DesktopHandle { get; }
        FullscreenMode WindowScreenMode { get; }

        int ThreadId { get; }
        int ProcessId { get; }
        bool IsDesktop { get; }
    }
}
