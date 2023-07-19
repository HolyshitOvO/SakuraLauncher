using Hake.Extension.ValueRecord;
using Hake.Extension.ValueRecord.Mapper;
using HakeQuick.Abstraction.Action;
using HakeQuick.Abstraction.Base;
using HakeQuick.Abstraction.Plugin;
using HakeQuick.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.IO;
using IWshRuntimeLibrary;
using System.Linq;

namespace RunnerPlugin
{
    /// <summary>
    /// 维护一个actions列表,里面是所有可运行的命令。
    /// 启动时从配置文件读取,运行时根据输入命令去匹配actions来返回suggestion
    /// 通过update来更新actions
    /// </summary>
    [Identity("runner")]
    public sealed class ListedRunnerPlugin : QuickPlugin
    {
        internal static ListedRunnerPlugin Instance { get; private set; }

        private readonly List<RunCommandAction> actions;
        private readonly UpdateRunnerAction updateAction;
        private readonly ILogger logger;

        /// <summary>
        /// 默认条目
        /// </summary>
        private static string[] PREDEFINED_COMMANDS = {
            "regedit", "winword", "excel", "powerpnt", "code", "explorer"
        };

        public ListedRunnerPlugin(ICurrentEnvironment env, ILoggerFactory loggerFactory)
        {
            if (Instance != null)
                throw new Exception($"cannot create another instance of {nameof(ListedRunnerPlugin)}");

            actions = new List<RunCommandAction>();
            logger = loggerFactory.CreateLogger("Runner");
            updateAction = new UpdateRunnerAction();
            Instance = this;
            UpdateConfigurations(env);
        }

        [Action("update")]
        public ActionUpdateResult UpdateList() => new ActionUpdateResult(updateAction, ActionPriority.Low);

        /// <summary>
        /// 输入框，查找条目的逻辑
        /// 当输入文本时,根据文本的去匹配 actions 列表中的命令,返回匹配的 ActionUpdateResult
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [ExplicitCall]
        public IEnumerable<ActionUpdateResult> OnUpdate(ICommand command)
        {
            List<ActionUpdateResult> updateResult = new List<ActionUpdateResult>();

            string identity = command.Identity;
            int i = 0;
            foreach (RunCommandAction action in actions)
            {
                if (action.RunCommand.Contains(identity))
                {
                    updateResult.Add(new ActionUpdateResult(action, ActionPriority.Normal));
                    i++;
                }
                // 限制展示的条目，最多展示 5+1 条
                if (i > 5) break;
            }

            return updateResult;
        }

        /// <summary>
        /// 读取配置文件runner.json,如果不存在则创建默认配置。
        /// 解析成CommandData对象列表,然后转成RunCommandAction添加到actions列表中
        /// </summary>
        /// <param name="env"></param>
        internal void UpdateConfigurations(ICurrentEnvironment env)
        {
            actions.Clear();
            logger.LogMessageAsync("A");
            string filename = "runner.json";
            string iconPath = "icons";
            string configPath = Path.Combine(env.ConfigDirectory.FullName, "runner");
            if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
            iconPath = Path.Combine(configPath, iconPath);
            filename = Path.Combine(configPath, filename);
            logger.LogMessageAsync("B");
            // 是否存在配置文件
            if (System.IO.File.Exists(filename))
            {
                // 读取json
                FileStream stream = System.IO.File.Open(filename, FileMode.Open);
                ListRecord record = Hake.Extension.ValueRecord.Json.Converter.ReadJson(stream) as ListRecord;
                stream.Close();
                stream.Dispose();
                logger.LogMessageAsync("C");
                try
                {
                    List<CommandData> data = ObjectMapper.ToObject<List<CommandData>>(record);
                    foreach (CommandData cmd in data)
                    {
                        
                        if (!string.IsNullOrEmpty(cmd.FolderPath) && cmd.FolderPath.Length > 0)
                        {
                            // 文件夹配置
                            if (Directory.Exists(cmd.FolderPath))
                            {
                                // 先添加此文件夹到列表
                                actions.Add(new RunCommandAction(cmd.Command, cmd.FolderPath, null, cmd.Admin, cmd.FolderPath, null));
                                // 遍历文件夹里的文件，添加到列表
                                TraverseFiles(cmd.FolderPath,cmd.IsSearchSubFolder,cmd.Exts,cmd.ExcludeNameArr);
                            }
                        }
                        else
                        {
                            // 普通的快捷方式配置
                            if (cmd.IconPath != null)
                                actions.Add(new RunCommandAction(cmd.Command, cmd.ExePath, Path.Combine(iconPath, cmd.IconPath), cmd.Admin, cmd.WorkingDirectory, cmd.ArgStr));
                            else
                                actions.Add(new RunCommandAction(cmd.Command, cmd.ExePath, null, cmd.Admin, cmd.WorkingDirectory, cmd.ArgStr));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogExceptionAsync(ex);
                }
            }
            else
            {
                List<CommandData> data = new List<CommandData>();
                foreach (string command in PREDEFINED_COMMANDS)
                {
                    data.Add(new CommandData()
                    {
                        Command = command,
                        ExePath = null,
                        IconPath = null
                    });
                    actions.Add(new RunCommandAction(command, null, null, false, null, null));
                }
                FileStream stream = System.IO.File.Create(filename);
                ListRecord record = GetCommandsRecord(data);
                string json = Hake.Extension.ValueRecord.Json.Converter.Json(record);
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(json);
                writer.Flush();
                writer.Close();
                writer.Dispose();
                stream.Close();
                stream.Dispose();
                logger.LogWarningAsync("runner.json not exists, write default configuration to new file");
            }
        }

        /// <summary>
        /// 将CommandData对象列表转成ListRecord,用于序列化成json
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        private ListRecord GetCommandsRecord(List<CommandData> commands)
        {
            ListRecord commandRecords = (ListRecord)ObjectMapper.ToRecord(commands);
            return commandRecords;
        }

        /// <summary>
        /// 遍历文件夹里的内容，并添加到列表
        /// </summary>
        /// <param name="folderPath">文件夹全路径</param>
        /// <param name="isSearchSubFolder">是否搜索子文件夹</param>
        public void TraverseFiles(string folderPath,bool isSearchSubFolder, List<string> exts, List<string> excludes)
        {
            // 获取当前文件夹中的所有.lnk和 文件
            // TODO: 当前只支持lnk
            string[] fileArr = {};
            if (exts.Count == 0)
                exts = Constants.DEF_EXT_VALUES;
            if (excludes.Count == 0)
                excludes = Constants.DEF_EXCLUDE_VALUES;
            

            foreach (string ext in exts)
            {
                fileArr = fileArr.Concat(Directory.GetFiles(folderPath, ext, isSearchSubFolder? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)).ToArray();
            }

            // 遍历当前文件夹中的.lnk文件
            foreach (string file in fileArr)
            {
                string justName = Path.GetFileNameWithoutExtension(file);
                bool isNeedContinue = false;
                foreach (string exclude in excludes)
                {
                    if (justName.ToLower().Contains(exclude))
                    {
                        isNeedContinue = true;
                        break;
                    }
                }
                if (isNeedContinue) continue;

                actions.Add(new RunCommandAction(
                    justName,
                    file,
                    null,
                    true,
                    null,
                    null,
                    file
                ));
            }
        }

        // 解析快捷方式目标路径
        private static string GetLnkTargetPath(string lnkFilePath)
        {
            WshShell shell = new WshShell();
            IWshShortcut lnkPath = (IWshShortcut)shell.CreateShortcut(lnkFilePath);
            //if(lnkPath.TargetPath.StartsWith("explorer.exe shell:AppsFolder\\")){
            return lnkPath.TargetPath;
        }

        public static class Constants
        {
            public static readonly List<string> DEF_EXT_VALUES = new List<string> { "*.*" };
            public static readonly List<string> DEF_EXCLUDE_VALUES = new List<string> { "uninstall", "卸载" };
        }
    }
}
