using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CandyLauncher.Implementation.Base;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace UnitTest
{
	[TestClass]
    public class CommandTest
    {
        [TestMethod]
        public void TestCommandParse()
        {
            string input = "";
            Command command;
            command = new Command(input);
            Assert.AreEqual("", command.Identity);
            Assert.AreEqual("", command.Action);
            Assert.AreEqual("", command.ActionPost);
            Assert.AreEqual(0, command.NamedArguments.Count);
            Assert.AreEqual(0, command.UnnamedArguments.Count);

            input = "test";
            command = new Command(input);
            Assert.AreEqual("test", command.Identity);
            Assert.AreEqual("", command.Action);
            Assert.AreEqual("", command.ActionPost);
            Assert.AreEqual(0, command.NamedArguments.Count);
            Assert.AreEqual(0, command.UnnamedArguments.Count);

            input = "test action";
            command = new Command(input);
            Assert.AreEqual("test", command.Identity);
            Assert.AreEqual("action", command.Action);
            Assert.AreEqual("action", command.ActionPost);
            Assert.AreEqual(1, command.NamedArguments.Count);
            Assert.AreEqual("action", command.NamedArguments["action"]);
            Assert.AreEqual(0, command.UnnamedArguments.Count);

            input = "test \"action act\"";
            command = new Command(input);
            Assert.AreEqual("test", command.Identity);
            Assert.AreEqual("", command.Action);
            Assert.AreEqual("\"action act\"", command.ActionPost);
            Assert.AreEqual(0, command.NamedArguments.Count);
            Assert.AreEqual(1, command.UnnamedArguments.Count);
            Assert.AreEqual("action act", command.UnnamedArguments[0]);

            input = "test \"action act\";act";
            command = new Command(input);
            string[] comp = new string[] { "action act", "act" };
            Assert.AreEqual("test", command.Identity);
            Assert.AreEqual("", command.Action);
            Assert.AreEqual(0, command.NamedArguments.Count);
            Assert.AreEqual(1, command.UnnamedArguments.Count);
            Assert.AreEqual(comp.Length, (command.UnnamedArguments[0] as string[]).Length);
            for (int i = 0; i < comp.Length; i++)
            {
                Assert.AreEqual(comp[i], (command.UnnamedArguments[0] as string[])[i]);
            }

            input = "test \"action act\"; act";
            command = new Command(input);
            Assert.AreEqual("test", command.Identity);
            Assert.AreEqual("", command.Action);
            Assert.AreEqual(0, command.NamedArguments.Count);
            Assert.AreEqual(2, command.UnnamedArguments.Count);
            Assert.AreEqual("action act", command.UnnamedArguments[0]);
            Assert.AreEqual("act", command.UnnamedArguments[1]);

            input = "test \"action act\"; act -sub";
            command = new Command(input);
            Assert.AreEqual("test", command.Identity);
            Assert.AreEqual("", command.Action);
            Assert.AreEqual(1, command.NamedArguments.Count);
            Assert.AreEqual(true, command.NamedArguments["sub"]);
            Assert.AreEqual(2, command.UnnamedArguments.Count);
            Assert.AreEqual("action act", command.UnnamedArguments[0]);
            Assert.AreEqual("act", command.UnnamedArguments[1]);

            input = "test \"action act\"; act -sub text;test";
            command = new Command(input);
            comp = new string[] { "text", "test" };
            Assert.AreEqual("test", command.Identity);
            Assert.AreEqual("", command.Action);
            Assert.AreEqual(1, command.NamedArguments.Count);
            Assert.AreEqual(comp.Length, (command.NamedArguments["sub"] as string[]).Length);
            for (int i = 0; i < comp.Length; i++)
            {
                Assert.AreEqual(comp[i], (command.NamedArguments["sub"] as string[])[i]);
            }
            Assert.AreEqual(2, command.UnnamedArguments.Count);
            Assert.AreEqual("action act", command.UnnamedArguments[0]);
            Assert.AreEqual("act", command.UnnamedArguments[1]);
        }

        [TestMethod]
        public void YourTestMethod()
        {
            // 打印的方法
            Console.WriteLine("23");
            //Debug.WriteLine("123");
            //MessageBox.Show("123", "警告");
        }

        [TestMethod]
        public void YourTestMethod2()
        {
			try
			{
                ContextMenuUtil.ShellContextMenu scm = new ContextMenuUtil.ShellContextMenu();
				FileInfo[] files2 = new FileInfo[1];
				//files2[0] = new FileInfo(@"c:\windows\notepad.exe");
				files2[0] = new FileInfo(@"C:\Users\Administrator\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Windows PowerShell\Windows PowerShell.lnk");
				//string filepath2 = ;
				//files2[0] = new FileInfo(filepath2);
				scm.ShowContextMenu(files2, new System.Drawing.Point(200, 200));
			}
			catch (Exception ex)
			{
				Debug.Print("显示菜单出错: {0}", ex);
			}
			return;

		}

		[TestMethod]
        public void YourTestMethod3()
        {
			Console.WriteLine(PinyinHelper.GetPinyinLongStr("崩坏3"));
            Console.WriteLine(PinyinHelper.GetPinyinLongStr("网易云音乐"));
            Console.WriteLine(PinyinHelper.GetPinyinLongStr("Microsoft Visual Studio 2022"));
            Console.WriteLine(PinyinHelper.GetPinyinLongStr("360安全中心"));
            Console.WriteLine(PinyinHelper.GetPinyinLongStr("控制面板 control panel"));
            Console.WriteLine(PinyinHelper.GetPinyinLongStr("Developer Command Prompt for VS 2022"));
		}

	}
}
