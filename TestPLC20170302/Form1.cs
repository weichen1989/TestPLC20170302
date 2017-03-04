using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using BIMI.RGWTB;
using System.Threading;

namespace TestPLC20170302
{
    public partial class Form1 : Form
    {
        private PLCsys PLC;
        private Thread receivePLCThread;//Receive PLC
        private Thread readReqThread;//发送读请求
        private Thread readVerifyThread;
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void buttonInitial_Click(object sender, EventArgs e)
        {
            PLCsys.IniReadPara = "DB1,B200,150";
            PLC = new PLCsys(new int[] { 500, 550 }, 600, 610, new StringBuilder("S7 connection_1"), new StringBuilder("S7 connection_2"));
            if (PLC.Connected())
            {
                OutPutTextBox.AppendText("\n"+"PLC连接成功！" + "\n");
                buttonReset.Enabled = true;
                buttonInitial.Enabled = false;
            }
            else
            {
                OutPutTextBox.AppendText("\n"+"PLC连接失败！" + "\n");
            }

            //接收PLC返回参数
            receivePLCThread = new Thread(PLC.Receive);
            receivePLCThread.Name = "PLC";
            receivePLCThread.Start();
            if (receivePLCThread.ThreadState != System.Threading.ThreadState.Running)
            {
                OutPutTextBox.AppendText("receivePLCThread启动失败!" + "\n");
            }

            //验证与PLC通信是否正常
            readVerifyThread = new Thread(PLCComVerify);
            readVerifyThread.Start();

            //发送读PLC请求
            readReqThread = new Thread(RequestReadPLC);
            readReqThread.Priority = ThreadPriority.AboveNormal;
            readReqThread.Start();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            //OutPutTextBox.Clear();
            //对系统PLC进行复位
            if (PLC.ResetPLCsys())
            {
                if (ResetPLCError())
                {
                   // AddPLCSubscription();
                    if (LoadRailPosition())
                    {
                        foreach (Control ctr in this.Controls)
                        {
                            ctr.Enabled = true;
                        }
                        textBoxRail1Pos.Text = PLC.Rail1.Rail_Robot_Position.ToString();
                        textBoxRail2Pos.Text = PLC.Rail2.Rail_Robot_Position.ToString();

                        OutPutTextBox.AppendText("系统复位成功." + "\n");
                        return;
                    }
                }
            }
            else
            {
                OutPutTextBox.AppendText("PLC系统复位失败,请确认系统无误后重新点击复位按钮."+"\n");
            }
        }

        /// <summary>
        /// 加载导轨位置
        /// </summary>
        /// <returns></returns>
        private bool LoadRailPosition()
        {
            bool result = true;

                if (PLC != null)
                {
                    //系统无故障且上电才可以加载导轨位置
                    if (!PLC.Rail1.Device_Elec_Off && !PLC.Fixture.Fixture_Stop_SG && !PLC.Com.Enmergency_Stop_SG)
                    {
                        bool tempResult = PLC.Rail1.LoadRailPositon();
                        result &= tempResult;
                        if (!tempResult)
                        {
                            OutPutTextBox.AppendText("导轨1位置加载失败,请确认系统无误后点击复位按钮." + "\n");
                        }
                        else
                        {
                            OutPutTextBox.AppendText("导轨1位置加载成功！" + "\n");
                        }
                    }
                    else
                        OutPutTextBox.AppendText("导轨1PLC系统故障或者设备急停（断电）-未加载导轨位置，请确认系统无误后点击复位按钮！" + "\n");

                //系统无故障且上电才可以加载导轨位置
                if (!PLC.Rail2.Device_Elec_Off && !PLC.Fixture.Fixture_Stop_SG && !PLC.Com.Enmergency_Stop_SG)
                {
                    bool tempResult = PLC.Rail2.LoadRailPositon();
                    result &= tempResult;
                    if (!tempResult)
                    {
                        OutPutTextBox.AppendText("导轨2位置加载失败,请确认系统无误后点击复位按钮." + "\n");
                    }
                    else
                    {
                        OutPutTextBox.AppendText("导轨2位置加载成功！" + "\n");
                    }
                }
                else
                    OutPutTextBox.AppendText("导轨2PLC系统故障或者设备急停（断电）-未加载导轨位置，请确认系统无误后点击复位按钮！" + "\n");
            }
            return result;
        }

        /// <summary>
        /// 百叶轮和导轨伺服故障复位
        /// </summary>
        /// <returns></returns>
        private bool ResetPLCError()
        {
            bool result = true;
            PLC.Rail1.ResetRailErr();
            PLC.Rail2.ResetWheelErr();
            if (PLC.Rail1.Rail_Servo_Warn_SG)
            {
                if (!PLC.Rail1.ResetWheelErr())
                {
                    result = false;
                    OutPutTextBox.AppendText("导轨1伺服故障复位失败,请确认系统无误后点击复位按钮.！" + "\n");
                }
            }
            if (PLC.Rail2.Rail_Servo_Warn_SG)
            {
                if (!PLC.Rail2.ResetWheelErr())
                {
                    result = false;
                    OutPutTextBox.AppendText("导轨2伺服故障复位失败,请确认系统无误后点击复位按钮.！" + "\n");
                }
            }
            if (PLC.Rail1.FlapWheel_Servo_Warn_SG)
            {
                if (!PLC.Rail1.ResetRailErr())
                {
                    result = false;
                    OutPutTextBox.AppendText("导轨1百叶轮伺服故障复位失败,请确认系统无误后点击复位按钮.！" + "\n");
                }
            }
            if (PLC.Rail2.FlapWheel_Servo_Warn_SG)
            {
                if (!PLC.Rail2.ResetRailErr())
                {
                    result = false;
                    OutPutTextBox.AppendText("导轨2百叶轮伺服故障复位失败,请确认系统无误后点击复位按钮.！" + "\n");
                }
            }
            return result;
        }

        /// <summary>
        /// 发送通讯成功标志 测试
        /// </summary>
        private void PLCComVerify()
        {
            while (true)
            {
                if (PLC.Fixture != null && PLC.Com != null)
                {
                    //发通讯成功标志位
                    PLC.Com.SetAllComOK();
                    Thread.Sleep(300);
                }
            }
        }

        /// <summary>
        /// 请求读取PLC数据
        /// </summary>
        private void RequestReadPLC()
        {
            while (true)
            {
                if (PLC != null)
                {
                    PLC.RequestRead();
                    DATA_CONVERTER.ByteToPLCsys(PLC.Value_store, PLC.Com, PLC.Fixture, PLC.Rail1 != null ? PLC.Rail1 : null, PLC.Rail2 != null ? PLC.Rail2 : null);
                    PLC.ResetReadCNF();//以便重新触发读取函数ByteToPLC
                }
                
                Thread.Sleep(50);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);//强制退出整个程序
            this.Dispose();
        }

    }
}
