using Hake.Extension.ValueRecord.Mapper;
using System;
using System.IO;
using System.Windows.Input;

namespace HakeQuick.Abstraction.Base
{
    /// <summary>
    /// 加载快捷键配置
    /// 从配置文件中按照key加载属性的值
    /// 如果没有找到key,根据MissingAction采取默认值或创建实例
    /// 这样可以非常方便的从配置中初始化一个对象
    /// 默认的启动快捷键为：Alt + J
    /// </summary>
    public sealed class HotkeyConfig
    {
        [MapProperty("key", MissingAction.GivenValue, "j")]
        public string KeyString { get; set; }

        [MapProperty("flags", MissingAction.GivenValue, "alt")]
        public string FlagsString { get; set; }

        public Key Key { get { return (Key)Enum.Parse(typeof(Key), KeyString.ToUpper()); } }
        public KeyFlags KeyFlags
        {
            get
            {
                string[] keyflags = FlagsString.ToUpper().Split('+');
                KeyFlags hotkeyFlags = KeyFlags.NONE;
                if (keyflags.Length > 0)
                    foreach (string keyflag in keyflags)
                        hotkeyFlags |= (KeyFlags)Enum.Parse(typeof(KeyFlags), keyflag);
                if (hotkeyFlags == KeyFlags.NONE)
                    hotkeyFlags = KeyFlags.CONTROL;
                return hotkeyFlags;
            }
        }
    }

    public sealed class QuickConfig
    {
        [MapProperty("config", MissingAction.GivenValue, ".\\configs")]
        public string ConfigPath { get; set; }

        [MapProperty("log", MissingAction.GivenValue, ".\\log")]
        public string LogPath { get; set; }

        [MapProperty("hotkey", MissingAction.CreateInstance)]
        public HotkeyConfig Hotkey { get; set; }
    }
}
