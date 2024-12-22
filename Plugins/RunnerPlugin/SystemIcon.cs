using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.IO;
using static RunnerPlugin.WindowsThumbnailProvider;
using System.Drawing.Imaging;

namespace RunnerPlugin
{
    /// <summary>
    /// 通过全路径获取文件图标
    /// https://www.jb51.net/article/81646.htm
    /// </summary>
    static class SystemIcon
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
        [DllImport("Shell32.dll", EntryPoint = "SHGetFileInfo", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);
        [DllImport("User32.dll", EntryPoint = "DestroyIcon")]
        public static extern int DestroyIcon(IntPtr hIcon);
        #region API 参数的常量定义
        public enum FileInfoFlags : uint
        {
            SHGFI_ICON = 0x000000100, // get icon
            SHGFI_DISPLAYNAME = 0x000000200, // get display name
            SHGFI_TYPENAME = 0x000000400, // get type name
            SHGFI_ATTRIBUTES = 0x000000800, // get attributes
            SHGFI_ICONLOCATION = 0x000001000, // get icon location
            SHGFI_EXETYPE = 0x000002000, // return exe type
            SHGFI_SYSICONINDEX = 0x000004000, // get system icon index
            SHGFI_LINKOVERLAY = 0x000008000, // put a link overlay on icon
            SHGFI_SELECTED = 0x000010000, // show icon in selected state
            SHGFI_ATTR_SPECIFIED = 0x000020000, // get only specified attributes
            SHGFI_LARGEICON = 0x000000000, // get large icon
            SHGFI_SMALLICON = 0x000000001, // get small icon
            SHGFI_OPENICON = 0x000000002, // get open icon
            SHGFI_SHELLICONSIZE = 0x000000004, // get shell size icon
            SHGFI_PIDL = 0x000000008, // pszPath is a pidl
            SHGFI_USEFILEATTRIBUTES = 0x000000010, // use passed dwFileAttribute
            SHGFI_ADDOVERLAYS = 0x000000020, // apply the appropriate overlays
            SHGFI_OVERLAYINDEX = 0x000000040 // Get the index of the overlay
        }
        public enum FileAttributeFlags : uint
        {
            FILE_ATTRIBUTE_READONLY = 0x00000001,
            FILE_ATTRIBUTE_HIDDEN = 0x00000002,
            FILE_ATTRIBUTE_SYSTEM = 0x00000004,
            FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
            FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
            FILE_ATTRIBUTE_DEVICE = 0x00000040,
            FILE_ATTRIBUTE_NORMAL = 0x00000080,
            FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
            FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,
            FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,
            FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
            FILE_ATTRIBUTE_OFFLINE = 0x00001000,
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
            FILE_ATTRIBUTE_ENCRYPTED = 0x00004000
        }
        #endregion
        /// <summary>
        /// 获取文件类型的关联图标
        /// </summary>
        /// <param name="fileName">文件类型的扩展名或文件的绝对路径</param>
        /// <param name="isLargeIcon">是否返回大图标</param>
        /// <returns>获取到的图标</returns>
        public static Icon GetIcon(string fileName, bool isLargeIcon)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            IntPtr hI;
            if (isLargeIcon)
                hI = SHGetFileInfo(fileName, 0, ref shfi, (uint)Marshal.SizeOf(shfi), (uint)FileInfoFlags.SHGFI_ICON | (uint)FileInfoFlags.SHGFI_USEFILEATTRIBUTES | (uint)0x4);
                //hI = SHGetFileInfo(fileName, 0, ref shfi, (uint)Marshal.SizeOf(shfi), (uint)FileInfoFlags.SHGFI_ICON | (uint)FileInfoFlags.SHGFI_USEFILEATTRIBUTES | (uint)FileInfoFlags.SHGFI_LARGEICON);
            else
                hI = SHGetFileInfo(fileName, 0, ref shfi, (uint)Marshal.SizeOf(shfi), (uint)FileInfoFlags.SHGFI_ICON | (uint)FileInfoFlags.SHGFI_USEFILEATTRIBUTES | (uint)FileInfoFlags.SHGFI_SMALLICON);
            Icon icon = Icon.FromHandle(shfi.hIcon).Clone() as Icon;
            DestroyIcon(shfi.hIcon); //释放资源
            return icon;
        }
        /// <summary> 
        /// 获取文件夹图标
        /// </summary> 
        /// <returns>图标</returns> 
        public static Icon GetDirectoryIcon(bool isLargeIcon)
        {
            SHFILEINFO _SHFILEINFO = new SHFILEINFO();
            IntPtr _IconIntPtr;
            if (isLargeIcon)
            {
                _IconIntPtr = SHGetFileInfo(@"", 0, ref _SHFILEINFO, (uint)Marshal.SizeOf(_SHFILEINFO), ((uint)FileInfoFlags.SHGFI_ICON | (uint)FileInfoFlags.SHGFI_LARGEICON));
            }
            else
            {
                _IconIntPtr = SHGetFileInfo(@"", 0, ref _SHFILEINFO, (uint)Marshal.SizeOf(_SHFILEINFO), ((uint)FileInfoFlags.SHGFI_ICON | (uint)FileInfoFlags.SHGFI_SMALLICON));
            }
            if (_IconIntPtr.Equals(IntPtr.Zero)) return null;
            Icon _Icon = System.Drawing.Icon.FromHandle(_SHFILEINFO.hIcon);
            return _Icon;
        }

        /// <summary>
        /// Bitmap 转为 BitmapImage 的高效方法
        /// https://www.jb51.net/article/229605.htm
        /// </summary>
        /// <param name="ImageOriginal"></param>
        /// <returns></returns>
        public static BitmapImage ToBitmapImage(Bitmap ImageOriginal)
        {

            Bitmap ImageOriginalBase = new System.Drawing.Bitmap(ImageOriginal);
            BitmapImage bitmapImage = new BitmapImage();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                ImageOriginalBase.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }

        public static ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        /// <summary>
        /// gpt 给的垃圾
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Icon GetFileIcon(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    // 获取文件的图标
                    //Icon icon = Icon.ExtractAssociatedIcon(filePath);
                    return Icon.ExtractAssociatedIcon(filePath);
                    // 缩放图标到指定尺寸
                    //return new Icon(icon, new Size(size, size));
                }
                catch (Exception ex)
                {
                    // 处理异常
                    Console.WriteLine("获取图标时出错：" + ex.Message);
                }
            }
            return null;
        }

        /// <summary>
        /// https://pythonjishu.com/cibjtsjrjs/
        /// </summary>
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public UInt32 cbSize;
            public IntPtr hIcon;
            public int iSysImageIndex;
            public int iIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;

            public SHSTOCKICONINFO(UInt32 size) : this()
            {
                cbSize = size;
            }
        }

        public enum SHSTOCKICONID : uint
        {
            SIID_DOCNOASSOC = 0,
            SIID_DOCASSOC = 1,
            SIID_APPLICATION = 2,
            SIID_FOLDER = 3,
            SIID_FOLDEROPEN = 4,
            SIID_DRIVE525 = 5,
            SIID_DRIVE35 = 6,
            SIID_DRIVEREMOVE = 7,
            SIID_DRIVEFIXED = 8,
            SIID_DRIVENET = 9,
            SIID_DRIVENETDISABLED = 10,
            SIID_DRIVECD = 11,
            SIID_DRIVERAM = 12,
            SIID_WORLD = 13,
            SIID_SERVER = 15,
            SIID_PRINTER = 16,
            SIID_MYNETWORK = 17,
            SIID_FIND = 22,
            SIID_HELP = 23,
            SIID_SHARE = 28,
            SIID_LINK = 29,
            SIID_SLOWFILE = 30,
            SIID_RECYCLER = 31,
            SIID_RECYCLERFULL = 32,
            SIID_MEDIACDAUDIO = 40,
            SIID_LOCK = 47,
            SIID_AUTOLIST = 49,
            SIID_PRINTERNET = 50,
            SIID_SERVERSHARE = 51,
            SIID_PRINTERFAX = 52,
            SIID_PRINTERFAXNET = 53,
            SIID_PRINTERFILE = 54,
            SIID_STACK = 55,
            SIID_MEDIASVCD = 56,
            SIID_STUFFEDFOLDER = 57,
            SIID_DRIVEUNKNOWN = 58,
            SIID_DRIVEDVD = 59,
            SIID_MEDIADVD = 60,
            SIID_MEDIADVDRAM = 61,
            SIID_MEDIADVDRW = 62,
            SIID_MEDIADVDR = 63,
            SIID_MEDIADVDROM = 64,
            SIID_MEDIACDAUDIOPLUS = 65,
            SIID_MEDIACDRW = 66,
            SIID_MEDIACDR = 67,
            SIID_MEDIACDBURN = 68,
            SIID_MEDIABLANKCD = 69,
            SIID_MEDIACDROM = 70,
            SIID_AUDIOFILES = 71,
            SIID_IMAGEFILES = 72,
            SIID_VIDEOFILES = 73,
            SIID_MIXEDFILES = 74,
            SIID_FOLDERBACK = 75,
            SIID_FOLDERFRONT = 76,
            SIID_SHIELD = 77,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80,
            SIID_KEY = 81,
            SIID_SOFTWARE = 82,
            SIID_RENAME = 83,
            SIID_DELETE = 84,
            SIID_MEDIAAUDIODVD = 85,
            SIID_MEDIAMOVIEDVD = 86,
            SIID_MEDIAENHANCEDCD = 87,
            SIID_MEDIAENHANCEDDVD = 88,
            SIID_MEDIAHDDVD = 89,
            SIID_MEDIABLURAY = 90,
            SIID_MEDIAVCD = 91,
            SIID_MEDIADVDPLUSR = 92,
            SIID_MEDIADVDPLUSRW = 93,
            SIID_DESKTOPPC = 94,
            SIID_MOBILEPC = 95,
            SIID_USERS = 96,
            SIID_MEDIANET = 97,
            SIID_UAVIDEO = 98,
            SIID_UATV = 99,
            SIID_MEDIAVIDEOCD = 100,
            SIID_MEDIAVIDEODVD = 101,
            SIID_MEDIATVCOMPUTER = 102,
            SIID_MEDIATV = 103,
            SIID_MEDIAAUDIOCD = 104,
            SIID_MEDIAAUDIOBOOK = 105,
            SIID_MEDIAMUSIC = 106,
            SIID_DEVICECELLPHONE = 107,
            SIID_DEVICECAMERA = 108,
            SIID_DEVICEVIDEOCAMERA = 109,
            SIID_DEVICEAUDIOPLAYER = 110,
            SIID_NETWORKCONNECT = 112,
            SIID_INTERNET = 113,
            SIID_ZIPFILE = 114,
            SIID_SETTINGS = 117,
            SIID_DRIVEHDDVD = 118,
            SIID_DRIVEBD = 119,
            SIID_MEDIAHDDVDROM = 120,
            SIID_MEDIAHDDVDR = 121,
            SIID_MEDIAHDDVDRAM = 122,
            SIID_MEDIABDROM = 123,
            SIID_MEDIABDR = 124,
            SIID_MEDIABDRE = 125,
            SIID_CLUSTEREDDRIVE = 126,
            SIID_MAX_ICONS = 175
        }

        [DllImport("Shell32.dll", SetLastError = true)]
        public static extern int SHGetStockIconInfo(SHSTOCKICONID siid, uint uFlags, ref SHSTOCKICONINFO psii);
        [ComImportAttribute()]
        [GuidAttribute("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItem
        {
            void BindToHandler(IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(uint sigdnName, out IntPtr ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        [ComImportAttribute()]
        [GuidAttribute("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItemImageFactory
        {
            [PreserveSig]
            HResult GetImage(
            [In, MarshalAs(UnmanagedType.Struct)] SIZE size,
            [In] ThumbnailOptions flags,
            [Out] out IntPtr phbm);
        }


        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string file, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

        public static BitmapSource GetLargeIcon(string fileName)
        {
            SHCreateItemFromParsingName(fileName, IntPtr.Zero, typeof(IShellItem).GUID, out IShellItem nativeShellItem);
            IntPtr shellItemPtr = Marshal.GetComInterfaceForObject(nativeShellItem, typeof(IShellItem));
            IntPtr shellItemImageFactoryPtr = IntPtr.Zero;
            Guid shellItemImageFactoryGuid = new Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b");

            try
            {
                Marshal.QueryInterface(shellItemPtr, ref shellItemImageFactoryGuid, out shellItemImageFactoryPtr);
                IShellItemImageFactory shellItemImageFactory = (IShellItemImageFactory)Marshal.GetObjectForIUnknown(shellItemImageFactoryPtr);

                uint iconSize = 256;
                //System.Runtime.InteropServices.ComTypes.FILETIME ft = new System.Runtime.InteropServices.ComTypes.FILETIME();
                IntPtr hBitmap = IntPtr.Zero;

                shellItemImageFactory.GetImage(new SIZE()
                {
                    Width = (int)iconSize,
                    Height = (int)iconSize
                },
                0x0,
                //ref ft,
                out hBitmap);

                if (hBitmap != IntPtr.Zero)
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight((int)iconSize, (int)iconSize));
                }

                return null;
            }
            finally
            {
                Marshal.ReleaseComObject(nativeShellItem);
                if (shellItemImageFactoryPtr != IntPtr.Zero)
                {
                    Marshal.Release(shellItemImageFactoryPtr);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SIZE
        {
            private int width;
            private int height;

            public int Width { set { width = value; } }
            public int Height { set { height = value; } }
        };

        //https://stackoverflow.com/questions/70156160/c-sharp-converting-bitmapsource-to-bitmapimage
        public static BitmapImage ConvertBitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            // before encoding/decoding, check if bitmapSource is already a BitmapImage
            if (!(bitmapSource is BitmapImage bitmapImage))
            {
                bitmapImage = new BitmapImage();
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    encoder.Save(memoryStream);
                    memoryStream.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                }
            }
            return bitmapImage;
        }
        /// <summary>
        /// 获取程序的图标，只支持exe
        /// </summary>
        /// <param name="filePath">文件全路径</param>
        public static BitmapImage GetExeIcon(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return null;

            Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
            if (icon == null)
                return null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                icon.ToBitmap().Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }
}

// 其他：https://blog.csdn.net/lzl_li/article/details/117038323