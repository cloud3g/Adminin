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
        #region �ֶ�

        private const int winCount = 9;
        private List<IntPtr>[] winGroup = new List<IntPtr>[winCount];//������
        private int currentID = 0;//��ǰ��������ID
        private delegate bool CallBack(IntPtr hwnd, int lParam);

        #endregion

        public int CurrentID
        {
            get { return this.currentID; }
        }


        #region ����

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
        /// ���캯��
        /// </summary>
        /// <param name="hostWin">�������������������</param>
        public AdmininHelper(Form hostForm)
        {
            this.selfHWND = hostForm.Handle;
            //��ʼ��������
            for (int index = 0; index < winGroup.Length; ++index)
            {
                winGroup[index] = new List<IntPtr>();
            }
        }

        /// <summary>
        /// ö�ٴ���Ļص�����
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private bool EnumWindowsProc(IntPtr hwnd, int lParam)
        {
            //��ӷ����ش�����
            if (API.IsWindowVisible(hwnd) != 0 && !winGroup[currentID].Contains(hwnd))
            {
                winGroup[currentID].Add(hwnd);
            }
            return true;
        }

        /// <summary>
        /// ע��ϵͳ�ȼ�
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

            //ע�����س������ȼ�
            API.RegisterHotKey(selfHWND, hotKeyID - 1, (uint)ModifyKeys.Alt, Keys.Oemtilde);

            //API.RegisterHotKey(selfHWND, hotKeyID - 2, (uint)ModifyKeys.Ctrl, Keys.F1);
            //API.RegisterHotKey(selfHWND, hotKeyID - 3, (uint)ModifyKeys.Ctrl, Keys.F2);
            */
        }

        /// <summary>
        /// ����ϵͳ�ȼ�
        /// </summary>
        public void UnRegisterHotKey()
        {
            int id = hotKeyID;
            for (int i = 0; i < winCount; ++i)
            {
                API.UnregisterHotKey(selfHWND, id + i);
            }
            //�������س������ȼ�
            API.UnregisterHotKey(selfHWND, hotKeyID - 1);
            //API.UnregisterHotKey(selfHWND, hotKeyID - 2);
            //API.UnregisterHotKey(selfHWND, hotKeyID - 3);
        }

        /// <summary>
        /// �л���
        /// </summary>
        /// <param name="groupID">��ţ���0��ʼ��</param>
        public void SwitchGroup(int groupID)
        {
            if (currentID == groupID)
            {
                return;
            }
            //���ԭ������
            winGroup[currentID].Clear();
            //���浱ǰ���洰��
            API.EnumWindows(EnumWindowsProc, 0);

            //��ȡ����ͼ�괰����
            IntPtr deskTopHwnd = API.FindWindowEx(API.GetDesktopWindow(), (IntPtr)null, "Progman", null);

            foreach (IntPtr hwnd in winGroup[currentID])
            {
                //�������ⴰ��
                if (selfHWND.Equals(hwnd)//��������
                    || API.FindWindow("Shell_TrayWnd", "").Equals(hwnd)//������
                    || deskTopHwnd.Equals(hwnd)//����
                    )
                {
                    continue;
                }
                //���ش���
                API.ShowWindow(hwnd, (int)WindowAction.Hide);
            }
            //��ʾ���鴰��
            foreach (IntPtr hwnd in winGroup[groupID])
            {
                API.ShowWindow(hwnd, (int)WindowAction.ShowNoActivate);
            }
            //���õ�ǰ���
            currentID = groupID;
        }

        #region IDisposable Members

        public void Dispose()
        {
            for (int index = 0; index < winGroup.Length; ++index)
            {
                foreach (IntPtr hwnd in winGroup[index])
                {
                    //�����������ʾ
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
            /// ע���ȼ�
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="id"></param>
            /// <param name="control"></param>
            /// <param name="vk"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint control, Keys vk);

            /// <summary>
            /// �����ȼ�
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            /// <summary>
            /// �ú���ö��������Ļ�ϵĶ��㴰��
            /// </summary>
            /// <param name="lpEnumFunc"></param>
            /// <param name="lParam"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern bool EnumWindows(CallBack lpEnumFunc, int lParam);

            //[DllImport("user32")]
            //public static extern string GetWindowText(IntPtr hWnd, IntPtr lpString, int nMaxCount);//ȡ��һ������ı��⣨caption�����֣�����һ���ؼ�������

            /// <summary>
            /// ��ָ�����豸�������������ǽֽͼ��
            /// </summary>
            /// <param name="hdc"></param>
            /// <returns></returns>
            [DllImport("user32", EntryPoint = "PaintDesktop")]
            public static extern int PaintDesktop(
                    int hdc
            );

            /// <summary>
            /// �ú����������洰�ڵľ��
            /// </summary>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern IntPtr GetDesktopWindow();

            /// <summary>
            /// �жϴ����Ƿ�ɼ�
            /// </summary>
            /// <param name="hWnd"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern int IsWindowVisible(IntPtr hWnd);

            /// <summary>
            /// ��ȡָ��������
            /// </summary>
            /// <param name="LpClassName"></param>
            /// <param name="LpWindowName"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern IntPtr FindWindow(string LpClassName, string LpWindowName);

            /// <summary>
            /// �ú������һ�����ڵľ�����ô��ڵ������ʹ�������������ַ�����ƥ�䡣������������Ӵ��ڣ������ڸ������Ӵ��ں������һ���Ӵ��ڿ�ʼ���ڲ���ʱ�����ִ�Сд��
            /// </summary>
            /// <param name="hwndParent"></param>
            /// <param name="hwndChildAfter"></param>
            /// <param name="lpszClass"></param>
            /// <param name="lpszWindow"></param>
            /// <returns></returns>
            [DllImport("user32")]
            public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            /// <summary>
            /// �ú�������ָ�����ڵ���ʾ״̬
            /// </summary>
            /// <param name="hWnd">Ŀ�괰�ھ��</param>
            /// <param name="nCmdShow">״̬����</param>
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
            /// ���ش��ڲ�������������
            /// </summary>
            Hide = 0x00,

            /// <summary>
            /// �ڴ���ԭ����λ����ԭ���ĳߴ缤�����ʾ����
            /// </summary>
            Show = 0x04,

            /// <summary>
            /// �����ʾ���ڡ����������С������󻯣���ϵͳ�����ڻָ���ԭ���ĳߴ��λ�á��ڻָ���С������ʱ��Ӧ�ó���Ӧ��ָ�������־��
            /// </summary>
            Restore = 0x03,

            /// <summary>
            /// �Դ������һ�εĴ�С��״̬��ʾ���ڡ��������Ȼά�ּ���״̬��
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