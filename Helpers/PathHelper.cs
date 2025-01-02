using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HakeQuick.Helpers
{
    public static class PathHelper
    {
        // 引入 GetLongPathNameW 函数
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetLongPathNameW(
            string lpszShortPath,
            StringBuilder lpszLongPath,
            uint cchBuffer);

        /// <summary>
        /// 标准化路径，将其转换为长路径名。
        /// </summary>
        /// <param name="path">要转换的路径。</param>
        /// <returns>转换后的长路径名。</returns>
        public static string GetLongPathName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("路径不能为空或仅包含空白字符。", nameof(path));
            }

            // 初始化缓冲区
            const int initialBufferSize = 260; // 通常的 MAX_PATH
            var buffer = new StringBuilder(initialBufferSize);

            // 调用 API
            uint result = GetLongPathNameW(path, buffer, (uint)buffer.Capacity);

            // 检查返回值
            if (result > buffer.Capacity)
            {
                // 如果缓冲区不够大，重新分配
                buffer = new StringBuilder((int)result);
                result = GetLongPathNameW(path, buffer, (uint)buffer.Capacity);
            }

            if (result == 0)
            {
                // 获取错误代码并抛出异常
                int errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"无法标准化路径。错误代码: {errorCode}");
            }

            return buffer.ToString();
        }
    }
}
