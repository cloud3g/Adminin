using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;

namespace Adminin
{
    public class AdmininHelper : IDisposable
    {
        #region 字段

        private const int winCount = 9;
        private List<IntPtr>[] winGroup = new List<IntPtr>[winCount];//窗体组
        private int currentID = 0;//当前虚拟桌面ID
        private delegate bool CallBack(IntPtr hwnd, int lParam);

        #endregion

        public int CurrentID
        {
            get { return this.currentID; }
        }


        #region 属性

        private IntPtr selfHWND;
        public IntPtr SelfHWND
        {
            get { return this.selfHWND; }
        }

        private static int hotKeyID = 80000;
        public static int HotKeyID
        {
            get { return hotKeyID; }
        }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hostWin">虚拟桌面程序宿主窗体</param>
        public AdmininHelper(Form hostForm)
        {
            this.selfHWND = hostForm.Handle;
            //初始化窗体组
            for (int index = 0; index < winGroup.Length; ++index)
            {
                winGroup[index] = new List<IntPtr>();
            }
        }

        /// <summary>
        /// 枚举窗体的回调函数
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private bool EnumWindowsProc(IntPtr hwnd, int lParam)
        {
            //添加非隐藏窗体句柄
            if (API.IsWindowVisible(hwnd) != 0 && !winGroup[currentID].Contains(hwnd))
            {
                winGroup[currentID].Add(hwnd);
            }
            return true;
        }

        /// <summary>
        /// 注册系统热键
        /// </summary>
        /// <param name="modifyKey"></param>
        /// <param name="hotKey"></param>
        public void RegisterHotKey(ModifyKeys modifyKey)
        {
            int id = hotKeyID;
            API.RegisterHotKey(selfHWND, id++, (uint)modifyKey, Keys.Enter);
            API.RegisterHotKey(selfHWND, id++, 0, Keys.F1);
            API.RegisterHotKey(selfHWND, id++, 0, Keys.F3);
            /*
            API.RegisterHotKey(selfHWND, id++, (uint)modifyKey, Keys.D3);
            API.RegisterHotKey(selfHWND, id++, (uint)modifyKey, Keys.D4);
            API.RegisterHotKey(selfHWND, id++, (uint)modifyKey, Keys.D5);
            API.RegisterHotKey(selfHWND, id++, (uint)modifyKey, Keys.D6);
            API.RegisterHotKey(selfHWND, id++, (uint)modifyKey, Keys.D7);
            API.RegisterHotKey(selfHWND, id++, (uint)modifyKey, Keys.D8);
            API.RegisterHotKey(selfHWND, id++, (uint)modifyKey, Keys.D9);

            //注册隐藏程序窗体热键
            API.RegisterHotKey(selfHWND, hotKeyID - 1, (uint)ModifyKeys.Alt, Keys.Oemtilde);

            //API.RegisterHotKey(selfHWND, hotKeyID - 2, (uint)ModifyKeys.Ctrl, Keys.F1);
            //API.RegisterHotKey(selfHWND, hotKeyID - 3, (uint)ModifyKeys.Ctrl, Keys.F2);
            */
        }

        /// <summary>
        /// 缷载系统热键
        /// </summary>
        public void UnRegisterHotKey()
        {
            int id = hotKeyID;
            for (int i = 0; i < winCount; ++i)
            {
                API.UnregisterHotKey(selfHWND, id + i);
            }
            //缷载隐藏程序窗体热键
            API.UnregisterHotKey(selfHWND, hotKeyID - 1);
            //API.UnregisterHotKey(selfHWND, hotKeyID - 2);
            //API.UnregisterHotKey(selfHWND, hotKeyID - 3);
        }

        /// <summary>
        /// 切换组
        /// </summary>
        /// <param name="groupID">组号（从0开始）</param>
        public void SwitchGroup(int groupID)
        {
            if (currentID == groupID)
            {
                return;
            }
            //清空原有数据
            winGroup[currentID].Clear();
            //保存当前桌面窗体
            API.EnumWindows(EnumWindowsProc, 0);

            //获取桌面图标窗体句柄
            IntPtr deskTopHwnd = API.FindWindowEx(API.GetDesktopWindow(), (IntPtr)null, "Progman", null);

            foreach (IntPtr hwnd in winGroup[currentID])
            {
                //忽略特殊窗体
                if (selfHWND.Equals(hwnd)//本程序窗体
                    || API.FindWindow("Shell_TrayWnd", "").Equals(hwnd)//任务栏
                    || deskTopHwnd.Equals(hwnd)//桌面
                    )
                {
                    continue;
                }
                //隐藏窗体
                API.ShowWindow(hwnd, (int)WindowAction.Hide);
            }
            //显示新组窗体
            foreach (IntPtr hwnd in winGroup[groupID])
            {
                API.ShowWindow(hwnd, (int)WindowAction.ShowNoActivate);
            }
            //设置当前组号
            currentID = groupID;
        }

        #region IDisposable Members

        public void Dispose()
        {
            for (int index = 0; index < winGroup.Length; ++index)
            {
                foreach (IntPtr hwnd in winGroup[index])
                {
                    //如果隐藏则显示
                    if (API.IsWindowVisible(hwnd) == 0)
                    {
                        API.ShowWindow(hwnd, (int)WindowAction.ShowNoActivate);
                    }
                }
            }
        }

        #endregion

        private static class API
        {
            #region Import API

            /// <summary>
            /// 注册热键
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="id"></param>
            /// <param name="control"></param>
            /// <param name="vk"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint control, Keys vk);

            /// <summary>
            /// 缷载热键
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            /// <summary>
            /// 该函数枚举所有屏幕上的顶层窗口
            /// </summary>
            /// <param name="lpEnumFunc"></param>
            /// <param name="lParam"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern bool EnumWindows(CallBack lpEnumFunc, int lParam);

            //[DllImport("user32")]
            //public static extern string GetWindowText(IntPtr hWnd, IntPtr lpString, int nMaxCount);//取得一个窗体的标题（caption）文字，或者一个控件的内容

            /// <summary>
            /// 在指定的设备场景中描绘桌面墙纸图案
            /// </summary>
            /// <param name="hdc"></param>
            /// <returns></returns>
            [DllImport("user32", EntryPoint = "PaintDesktop")]
            public static extern int PaintDesktop(
                    int hdc
            );

            /// <summary>
            /// 该函数返回桌面窗口的句柄
            /// </summary>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern IntPtr GetDesktopWindow();

            /// <summary>
            /// 判断窗口是否可见
            /// </summary>
            /// <param name="hWnd"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern int IsWindowVisible(IntPtr hWnd);

            /// <summary>
            /// 获取指定窗体句柄
            /// </summary>
            /// <param name="LpClassName"></param>
            /// <param name="LpWindowName"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern IntPtr FindWindow(string LpClassName, string LpWindowName);

            /// <summary>
            /// 该函数获得一个窗口的句柄，该窗口的类名和窗口名与给定的字符串相匹配。这个函数查找子窗口，从排在给定的子窗口后面的下一个子窗口开始。在查找时不区分大小写。
            /// </summary>
            /// <param name="hwndParent"></param>
            /// <param name="hwndChildAfter"></param>
            /// <param name="lpszClass"></param>
            /// <param name="lpszWindow"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            /// <summary>
            /// 该函数设置指定窗口的显示状态
            /// </summary>
            /// <param name="hWnd">目标窗口句柄</param>
            /// <param name="nCmdShow">状态参数</param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            //[DllImport("user32")]
            //public static extern int GetClassName(IntPtr hWnd, string sClassName, int nMaxCount);
            #endregion
        }


        private enum WindowAction
        {
            /// <summary>
            /// 隐藏窗口并激活其他窗口
            /// </summary>
            Hide = 0x00,

            /// <summary>
            /// 在窗口原来的位置以原来的尺寸激活和显示窗口
            /// </summary>
            Show = 0x04,

            /// <summary>
            /// 激活并显示窗口。如果窗口最小化或最大化，则系统将窗口恢复到原来的尺寸和位置。在恢复最小化窗口时，应用程序应该指定这个标志。
            /// </summary>
            Restore = 0x03,

            /// <summary>
            /// 以窗口最近一次的大小和状态显示窗口。激活窗口仍然维持激活状态。
            /// </summary>
            ShowNoActivate = 0x08
        }

        [Flags]
        public enum ModifyKeys
        {
            None = 0x00,
            Alt = 0x01,
            Ctrl = 0x02,
            Shift = 0x04,
            Windows = 0x08
        }

    }
}