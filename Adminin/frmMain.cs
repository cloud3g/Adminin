using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Adminin
{
    public partial class frmMain : Form
    {
        private AdmininHelper vdtHelper;
        public frmMain()
        {
            InitializeComponent();

            #region 只能运行一个客户端程序
            bool flag = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Test", out flag);
            //第一个参数:true--给调用线程赋予互斥体的初始所属权  
            //第一个参数:互斥体的名称  
            //第三个参数:返回值,如果调用线程已被授予互斥体的初始所属权,则返回true  
            if (!flag)
            {
                MessageBox.Show("只能运行一个客户端程序", "请确定", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(1);//退出程序  
            }
            #endregion  

            vdtHelper = new AdmininHelper(this);

            //注册系统热键
            vdtHelper.RegisterHotKey(AdmininHelper.ModifyKeys.Ctrl);

            this.Opacity = 0f;
        }

        private void btnClick(object sender, EventArgs e)
        {
            int targetGroupIndex = 0;
            if (sender is Button)
            {
                Button clickedBtn = sender as Button;
                switch (clickedBtn.Text)
                {
                    case "休眠":
                        Application.SetSuspendState(PowerState.Hibernate, true, false);
                        break;
                    case "待机":
                        Application.SetSuspendState(PowerState.Suspend, true, false);
                        break;
                    default:
                        targetGroupIndex = Int32.Parse(clickedBtn.Text) - 1;
                        break;
                }
            }
            else if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem tsmItem = sender as ToolStripMenuItem;
                targetGroupIndex = Int32.Parse(tsmItem.Text.Replace("桌面","")) - 1;
            }

            vdtHelper.SwitchGroup(targetGroupIndex);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //缷载系统热键
            vdtHelper.UnRegisterHotKey();
            //恢复显示所有虚拟桌面组内的窗体
            vdtHelper.Dispose();
        }

        protected override void WndProc(ref Message m)//监视Windows消息
        {
            const int WM_HOTKEY = 0x0312;//按快捷键
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    int pc = m.WParam.ToInt32() - AdmininHelper.HotKeyID;
                    if (pc >= 0 && pc < 9)
                    {
                        vdtHelper.SwitchGroup(pc);
                    }
                    else if (pc == -1)
                    {
                        if (this.Opacity==1f)
                        {
                            this.Visible = false;
                            this.Opacity = 0f;
                        }
                        else
                        {
                            this.Visible = true;
                            this.Opacity = 1f;
                        }   
                    }
                    //else if (pc == -2)
                    //{
                    //    Application.SetSuspendState(PowerState.Hibernate, true, false);
                    //}
                    //else if (pc == -3)
                    //{
                    //    Application.SetSuspendState(PowerState.Suspend, true, false);
                    //}
                    break;
            }
            base.WndProc(ref m);
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            this.桌面1ToolStripMenuItem.Checked = vdtHelper.CurrentID == 0 ? true : false;
            this.桌面2ToolStripMenuItem.Checked = vdtHelper.CurrentID == 1 ? true : false;
            this.桌面3ToolStripMenuItem.Checked = vdtHelper.CurrentID == 2 ? true : false;
            this.桌面4ToolStripMenuItem.Checked = vdtHelper.CurrentID == 3 ? true : false;
            this.桌面5ToolStripMenuItem.Checked = vdtHelper.CurrentID == 4 ? true : false;
            this.桌面6ToolStripMenuItem.Checked = vdtHelper.CurrentID == 5 ? true : false;
            this.桌面7ToolStripMenuItem.Checked = vdtHelper.CurrentID == 6 ? true : false;
            this.桌面8ToolStripMenuItem.Checked = vdtHelper.CurrentID == 7 ? true : false;
            this.桌面9ToolStripMenuItem.Checked = vdtHelper.CurrentID == 8 ? true : false;
        }

        private void ShowFormMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Opacity == 1f)
            {
                this.Visible = false;
                this.Opacity = 0f;
            }
            else
            {
                this.Visible = true;
                this.Opacity = 1f;
            }   
        }

    }
}