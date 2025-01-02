using Hake.Extension.ValueRecord;
using Hake.Extension.ValueRecord.Mapper;
using CandyLauncher.Abstraction.Action;
using CandyLauncher.Abstraction.Base;
using CandyLauncher.Abstraction.Plugin;
using CandyLauncher.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.IO;
using IWshRuntimeLibrary;
using System.Linq;
using static System.Collections.Specialized.BitVector32;
using System.Security.Principal;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Threading;
using HakeQuick.Helpers;

namespace RunnerPlugin
{
    /// <summary>
    /// 维护一个 actions 列表,里面是所有可运行的命令。
    /// 启动时从配置文件读取,运行时根据输入命令去匹配 actions 来返回 suggestion
    /// 通过 update 来更新 actions
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
        private static readonly string[] PREDEFINED_COMMANDS = { "ProgramData开始菜单", "Users开始菜单", "Default开始菜单" };
        private static readonly string[] PREDEFINED_FOLDERS = {
            "C:\\\\ProgramData\\\\Microsoft\\\\Windows\\\\Start Menu\\\\Programs",
            "C:\\\\Users\\\\Administrator\\\\AppData\\\\Roaming\\\\Microsoft\\\\Windows\\\\Start Menu\\\\Programs",
            "C:\\\\Users\\\\Default\\\\AppData\\\\Roaming\\\\Microsoft\\\\Windows\\\\Start Menu\\\\Programs"
        };
        private static readonly List<string> PREDEFINED_EXCLUDE_WORDS = new List<string> { "uninstall", "卸载", "帮助", "说明", "help", "Document", "Website", "更新", "使用", };
        [DllImport("kernel32")]
        public static extern uint GetTickCount();

        public ListedRunnerPlugin(ICurrentEnvironment env, ILoggerFactory loggerFactory)
        {
            if (Instance != null)
                throw new Exception($"cannot create another instance of {nameof(ListedRunnerPlugin)}");

            actions = new List<RunCommandAction>();
            logger = loggerFactory.CreateLogger("Runner");
            updateAction = new UpdateRunnerAction();
            Instance = this;
            Tick.TickSingle("UpdateConfigurations");
            UpdateConfigurations(env);
            Tick.LogSingle("UpdateConfigurations");
        }

        [Action("update")]
        public ActionUpdateResult UpdateList() => new ActionUpdateResult(updateAction, ActionPriority.Low);

        static readonly char[] CHAR_SPACE = { ' ' };

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
            // 为空则直接跳过
            if (command.Identity.Length == 0) return updateResult;
            int i = 0;
            //Debug.WriteLine(command.Identity);
            string[] wordsArray = command.Identity.Split(CHAR_SPACE, StringSplitOptions.RemoveEmptyEntries);
            // 遍历每个启动条目

            foreach (RunCommandAction action in actions)
            {
                // 遍历 空格分隔的搜索词
                foreach (string word in wordsArray)
                {
                    if (!action.RunCommand.Contains(word))
                    {
                        goto outerLoop; // 直接跳过该条目，因为搜索词中没有匹配成功
                    }
                }
                updateResult.Add(new ActionUpdateResult(action, ActionPriority.Normal));
                i++;
                // 限制展示的条目，最多展示 7 条
                if (i >= 7) break;
                outerLoop:;
            }

            return updateResult;
        }

        /// <summary>
        /// 读取配置文件 runner.json,如果不存在则创建默认配置。
        /// 解析成CommandData对象列表,然后转成RunCommandAction添加到 actions 列表中
        /// </summary>
        /// <param name="env"></param>
        internal void UpdateConfigurations(ICurrentEnvironment env)
        {
            // todo: 重新加载列表的时候，内存会越来越大，不知道为什么
            actions.ForEach(action =>
            {
                if (action.Icon != null)
                {
                    action.Icon.Freeze();
                    action.Icon = null;
                }
            });
            actions.Clear();
            actions.TrimExcess();

            string filename = "runner.json";
            //string configPath = Path.Combine(env.ConfigDirectory.FullName, "runner");
            string configPath = env.ConfigDirectory.FullName;
            if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
            filename = Path.Combine(configPath, filename);

            // 如果不存在配置文件，则创建配置
            if (!System.IO.File.Exists(filename))
            {
                List<CommandData> data = new List<CommandData>();
                for (int i = 0; i < PREDEFINED_COMMANDS.Length; i++)
                {
                    string command = PREDEFINED_COMMANDS[i];
                    data.Add(new CommandData()
                    {
                        Command = command,
                        ExcludeNameWordArr = PREDEFINED_EXCLUDE_WORDS,
                        IsSearchSubFolder = true,
                        FolderPath = PREDEFINED_FOLDERS[i]
                    });
                    //actions.Add(new RunCommandAction(command, null, null, false, null, null));
                }
                FileStream stream2 = System.IO.File.Create(filename);
                ListRecord record2 = GetCommandsRecord(data);
                string json = Hake.Extension.ValueRecord.Json.Converter.Json(record2);
                StreamWriter writer = new StreamWriter(stream2);
                writer.Write(json);
                writer.Flush();
                writer.Close();
                writer.Dispose();
                stream2.Close();
                stream2.Dispose();
                Debug.WriteLine("runner.json not exists, write default configuration to new file");
            }


            // 读取 json
            FileStream stream = System.IO.File.Open(filename, FileMode.Open);
            ListRecord record = Hake.Extension.ValueRecord.Json.Converter.ReadJson(stream) as ListRecord;
            stream.Close();
            stream.Dispose();
            try
            {
                List<CommandData> data = ObjectMapper.ToObject<List<CommandData>>(record);
                Tick.TickSingle("TraverseFiles");
                foreach (CommandData cmd in data)
                {

                    if (!string.IsNullOrEmpty(cmd.FolderPath) && cmd.FolderPath.Length > 0)
                    {
                        // 标准化文件夹路径，转换相对路径为绝对路径
                        if (cmd.FolderPath.StartsWith("\\") || cmd.FolderPath.StartsWith("."))
                        {
                            cmd.FolderPath = Path.Combine(configPath, cmd.FolderPath);
                        }
                        // 转换路径里的变量，和长路径转换
                        cmd.FolderPath  = PathHelper.GetLongPathName(cmd.FolderPath);
                        // 文件夹配置
                        if (Directory.Exists(cmd.FolderPath))
                        {
                            // 先添加此文件夹到列表
                            actions.Add(new RunCommandAction(cmd.Command, cmd.FolderPath, admin: cmd.Admin, workingDirectory: cmd.FolderPath, isUwpItem: false));
                            // 遍历文件夹里的文件，添加到列表
                            TraverseFiles(cmd.FolderPath, cmd.IsSearchSubFolder, cmd.Exts, cmd.ExcludeNameWordArr, cmd.ExcludeNameArr, cmd.RenameSource, cmd.RenameTarget);
                        }
                    }
                    else
                    {
                        // 普通的快捷方式配置
                        actions.Add(new RunCommandAction(cmd.Command, cmd.ExePath, admin: cmd.Admin, workingDirectory: cmd.WorkingDirectory, argsStr: cmd.ArgStr, isUwpItem: false));
                    }
                }
                Tick.LogSingle("TraverseFiles");
                // 获取 UWP 应用
                if (true)
                {
                    Tick.TickSingle("UwpList");
                    List<ShellObject> UwpList = UwpUtil.SpecificallyForGetCurrentUwpName2();
                    Tick.LogSingle("UwpList");
                    // 这里使用并行没有优势
                    UwpList.ForEach(uwp =>
                    {
                        if (string.IsNullOrEmpty(uwp.Name))
                        {
                            Debug.WriteLine("遇到空的了");
                        }
                        // 获取应用图片，除了这个，其他大小的图都会有黑背景和锯齿感，不知为啥
                        //System.Drawing.Bitmap temp = uwp.Thumbnail.ExtraLargeBitmap;
                        //SystemIcon.ToBitmapImage(temp)
                        ImageSource icon = uwp.Thumbnail.ExtraLargeBitmapSource;
                        // 使用 `using` 确保 `MemoryStream` 被释放
                        BitmapImage bitmapImage = null;
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            try
                            {
                                var bitmapSource = (BitmapSource)icon;

                                // 使用 PngBitmapEncoder 将 BitmapSource 编码到 MemoryStream
                                var encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                                encoder.Save(memoryStream);

                                memoryStream.Seek(0, SeekOrigin.Begin);

                                // 初始化 BitmapImage
                                bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = memoryStream;
                                bitmapImage.EndInit();

                                // 清除 MemoryStream 中的数据，虽然其生命周期由 `using` 管理
                                memoryStream.SetLength(0);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"处理图片时出错: {ex.Message}");
                            }
                        }

                        // 添加到 actions
                        if (bitmapImage != null)
                        {
                            actions.Add(new RunCommandAction(uwp.Name, "shell:AppsFolder\\" + uwp.ParsingName, true, bitmapImage));
                        }

                    });

                }
            }
            catch (Exception ex)
            {
                logger.LogExceptionAsync(ex);
            }
            // 获取当前线程的 SynchronizationContext
            SynchronizationContext syncContext = SynchronizationContext.Current;
            Task.Run(() =>
            {
                List<BitmapImage> tempIcon = new List<BitmapImage>();
                actions.ForEach(wp =>
                {
                    if (!wp.IsUwpItem)
                    {
                        try
                        {
                            // 某些情况会闪退
                            //Icon = SystemIcon.ToBitmapImage(WindowsThumbnailProvider.GetThumbnail(tempIconPath, 48, 48, ThumbnailOptions.IconOnly));
                            tempIcon.Add(SystemIcon.ToBitmapImage(WindowsThumbnailProvider.GetThumbnail(wp.targetFilePath, 48, 48, ThumbnailOptions.None)));
                        }
                        catch (Exception)
                        {
                            try
                            {
                                tempIcon.Add(SystemIcon.ToBitmapImage(SystemIcon.GetIcon(wp.targetFilePath, true).ToBitmap()));// 这个也可以，但是某些图标不行
                            }
                            catch (Exception)
                            {
                                tempIcon.Add(null);
                            }
                        }
                    }
                });
                // 在 UI 线程中更新 Icon
                if (syncContext != null)
                {
                    Debug.WriteLine("syncContext");
                    syncContext.Send(_ =>
                    {
                        for (int i = 0; i < actions.Count; i++)
                        {
                            if (!actions[i].IsUwpItem) actions[i].Icon = tempIcon[i];
                            //if (!actions[i].IsUwpItem) actions[i].Iconbyte = BitmapImageToByteArray(tempIcon[i]);
                            //tempIcon[i].Freeze();
                            //tempIcon[i]=null;
                        }
                    }, null);
                }
                else
                {
                    for (int i = 0; i < actions.Count; i++)
                    {
                        if (!actions[i].IsUwpItem) actions[i].Icon = tempIcon[i];
                        //if (!actions[i].IsUwpItem) actions[i].Iconbyte = BitmapImageToByteArray(tempIcon[i]);
                        //tempIcon[i].Freeze();
                        //tempIcon[i] = null;
                    }
                }
            });
            // todo: 在这里更新图标，不要在主索引进行的时候更新
        }

        public BitmapImage ByteArrayToBitmapImage(byte[] byteArray)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        public byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // 创建一个PngEncoder或者JpegEncoder，视需要选择格式
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(ms);
                return ms.ToArray();
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
        /// 遍历文件夹里的内容，并添加到启动条目列表
        /// </summary>
        /// <param name="folderPath">文件夹全路径</param>
        /// <param name="isSearchSubFolder">是否搜索子文件夹</param>
        public void TraverseFiles(string folderPath, bool isSearchSubFolder, List<string> exts, List<string> excludeWords, List<string> excludes, List<string> renameSource, List<string> renameTarget)
        {
            // 修正 null 值的情况
            if (exts == null || exts.Count == 0) exts = Constants.DEF_EXT_VALUES;
            if (excludes == null) excludes = new List<string>();
            if (excludeWords == null) excludeWords = new List<string>();
            if (renameSource == null) renameSource = new List<string>();
            if (renameTarget == null) renameTarget = new List<string>();

            // 排除词语，全部转小写
            var excludeWordsHashSet = new HashSet<string>(excludeWords.ConvertAll(s => s.ToLower()));
            var excludesHashSet = new HashSet<string>(excludes.ConvertAll(s => s.ToLower()));

            // 创建重命名映射
            var renameMap = new Dictionary<string, string>();
            for (int i = 0; i < renameSource.Count; i++)
            {
                renameMap[renameSource[i]] = renameTarget[i];
            }
            // 获取当前文件夹中的所有指定文件格式（exts） 的文件路径数组（一次性加载所有文件）
            List<string> filePathList = new List<string>();
            foreach (var ext in exts)
            {
                filePathList.AddRange(Directory.GetFiles(folderPath, ext, isSearchSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            }
            Tick.TickSingle("ProcessFiles");
            if (filePathList.Count > 10)
            {
                // 使用 ConcurrentBag 来处理线程安全的文件添加
                var actionTempList = new ConcurrentBag<RunCommandAction>();
                // 使用 Parallel.ForEach 进行并行处理
                var options = new ParallelOptions { MaxDegreeOfParallelism = 4 }; // 控制最大并行度为 4
                Parallel.ForEach(filePathList, options, filePath =>
                {
                    RunCommandAction temp = addActionCommand(filePath, excludeWordsHashSet, excludesHashSet, renameMap);
                    if (temp == null) return;
                    actionTempList.Add(temp);
                });
                actions.AddRange(actionTempList.ToList());
            }
            else
            {
                foreach (string filePath in filePathList)
                {
                    RunCommandAction temp = addActionCommand(filePath, excludeWordsHashSet, excludesHashSet, renameMap);
                    if (temp == null) continue;
                    actions.Add(temp);
                }
            }
            Tick.LogSingle("ProcessFiles");
        }

        private static RunCommandAction addActionCommand(string filePath, HashSet<string> excludeWordsHashSet, HashSet<string> excludesHashSet, Dictionary<string, string> renameMap)
        {
            string justName = Path.GetFileNameWithoutExtension(filePath);

            // 是否需要排除，完整匹配
            if (excludesHashSet.Count != 0 && excludesHashSet.Contains(justName.ToLower())) return null;

            // 是否需要排除，部分匹配
            if (excludeWordsHashSet.Any(word => justName.ToLower().Contains(word))) return null;

            // 是否需要重命名该项
            if (renameMap.ContainsKey(justName))
            {
                justName = renameMap[justName];
            }

            // 添加到启动条目列表
            return new RunCommandAction(justName, filePath, false, admin: true);
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
