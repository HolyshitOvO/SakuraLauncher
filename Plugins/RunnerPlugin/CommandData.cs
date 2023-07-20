using Hake.Extension.ValueRecord;
using Hake.Extension.ValueRecord.Mapper;
using System.Collections.Generic;

namespace RunnerPlugin
{
    /// <summary>
    /// 解析 json 的 model 类
    /// </summary>
    internal sealed class CommandData
    {
        /// <summary>
        /// 名称，或运行命令
        /// </summary>
        [MapProperty("command", MissingAction.Throw)]
        public string Command { get; set; }
        /// <summary>
        /// EXE 运行路径
        /// </summary>
        [MapProperty("path", MissingAction.GivenValue, null)]
        public string ExePath { get; set; }

        /// <summary>
        /// 图标路径
        /// </summary>
        [MapProperty("icon", MissingAction.GivenValue, null)]
        public string IconPath { get; set; }

        /// <summary>
        /// 是否以管理员启动
        /// </summary>
        [MapProperty("admin", MissingAction.GivenValue, false)]
        public bool Admin { get; set; }

        /// <summary>
        /// 工作路径
        /// </summary>
        [MapProperty("workingdir", MissingAction.GivenValue, null)]
        public string WorkingDirectory { get; set; }
        [MapProperty("args", MissingAction.CreateInstance)]
        public List<string> Args { get; set; }

        /// <summary>
        /// 批量命令，索引文件夹路径里的文件
        /// </summary>
        [MapProperty("folder", MissingAction.GivenValue, null)]
        public string FolderPath { get; set; }

        /// <summary>
        /// 批量命令，索引文件夹路径里的文件
        /// </summary>
        [MapProperty("arg", MissingAction.GivenValue, null)]
        public string ArgStr { get; set; }

        /// <summary>
        /// 需要索引的文件格式
        /// </summary>
        [MapProperty("exts", MissingAction.CreateInstance, null)]
        public List<string> Exts { get; set; }

        /// <summary>
        /// 需要排除的文件名
        /// </summary>
        [MapProperty("exclude", MissingAction.CreateInstance, null)]
        public List<string> ExcludeNameArr{ get; set; }

        /// <summary>
        /// 是否包括子文件夹
        /// </summary>
        [MapProperty("subfolder", MissingAction.GivenValue, false)]
        public bool IsSearchSubFolder { get; set; }
    }

    
}
