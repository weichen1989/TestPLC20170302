using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
namespace BIMI.RGWTB
{
   public class ComPLC
    {
        /// <summary>
        /// 起始地址
        /// </summary>
        private int startAddress;
        #region PLC读写参数
        private UInt32 cp_descr = 0;
        private UInt16 cref = 0;
        private UInt16 orderid = 0;
        #endregion
        #region 读信号
        #region bool类型 4个
        public bool PLC_Communication_OK;//操作台PLC通信正常标志位（R）
        private bool enmergency_Stop_SG;//操作台急停信号（R）
        /// <summary>
        /// 操作台急停信号（R）
        /// </summary>
        public bool Enmergency_Stop_SG
        {
            get { return enmergency_Stop_SG; }
            set
            {
                if (value == true && value != enmergency_Stop_SG)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("操作台急停", Enmergency_Stop_SG, value);
                    if (this.ComPLCEstopRaise != null)
                    {
                        this.ComPLCEstopRaise(this, e);
                    }
                }
                enmergency_Stop_SG = value;
            }
        }
        private bool pLC_ERR_SG;//操作台PLC故障标志位（R）
        /// <summary>
        /// 操作台PLC故障标志位（R）
        /// </summary>
        public bool PLC_ERR_SG
        {
            get { return pLC_ERR_SG; }
            set
            {
                if (value == true)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("操作台PLC故障", PLC_ERR_SG, value);
                    if (this.ComPLCErrRaise != null)
                    {
                        this.ComPLCErrRaise(this, e);
                    }
                }
                pLC_ERR_SG = value;
            }
        }
        private bool system_ERR_SG;//系统故障汇总标志位（R）
        /// <summary>
        /// 系统故障汇总标志位（R）
        /// </summary>
        public bool System_ERR_SG
        {
            get { return system_ERR_SG; }
            set
            {
                if (value == true&&system_ERR_SG==false)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("通讯PLC系统故障", System_ERR_SG, value);
                    if (this.ComPLCErrRaise != null)
                    {
                        this.ComPLCErrRaise(this, e);
                    }
                }
                system_ERR_SG = value;
            }
        }
        #endregion
        #endregion
        #region 写信号
        private string pC_Reset_SG_Address;
        /// <summary>
        ///上位机远程复位信号
        /// </summary>
        public string PC_Reset_SG_Address
        {
            get { return pC_Reset_SG_Address; }
            set { pC_Reset_SG_Address = value; }
        }
        private string sys_Com_OK_Address;
        /// <summary>
        /// 总通讯成功标志位（持续写true）
        /// </summary>
        public string Sys_Com_OK_Address
        {
            get { return sys_Com_OK_Address; }
            set { sys_Com_OK_Address = value; }
        }
        private string sys_Reset_SG_Address;
       /// <summary>
       /// 总复位信号（点击伺服错误需单独复位）
       /// </summary>
        public string Sys_Reset_SG_Address
        {
            get { return sys_Reset_SG_Address; }
            set { sys_Reset_SG_Address = value; }
        }
       
        #endregion
        #region 事件
        public event EventHandler<PropertyChangedEventArgs> ComPLCErrRaise;
        public event EventHandler<PropertyChangedEventArgs> ComPLCWarningRaise;
        public event EventHandler<PropertyChangedEventArgs> ComPLCEstopRaise;
        #endregion
        public ComPLC(int startAddr, UInt32 cp_desc, UInt16 cre, UInt16 ordeid)
        {
            startAddress = startAddr;
            cp_descr = cp_desc;
            cref = cre;
            orderid = ordeid;
            Initial();
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initial()
        {
            string S_Add = startAddress.ToString();
            string S1 = "DB1,";
            string S_X = "X";
            PC_Reset_SG_Address = S1 + S_X + S_Add + ".0";
            Sys_Com_OK_Address = S1 + S_X + S_Add + ".1";
            Sys_Reset_SG_Address = S1 + S_X + S_Add + ".2";
        }
       /// <summary>
       /// 通讯PLC复位
       /// </summary>
       public bool resetComPLC()
        {
            return write(PC_Reset_SG_Address, true);
        }
       /// <summary>
       /// 通讯PLC复位置0
       /// </summary>
       /// <returns></returns>
       public bool setComPLC()
       {
           return write(PC_Reset_SG_Address, false);
       }
       /// <summary>
       /// PLC系统复位
       /// </summary>
       public bool resetPLCSys()
       {
           return write(PC_Reset_SG_Address, true);
       }
       /// <summary>
       /// PLC系统复位置0
       /// </summary>
       /// <returns></returns>
       public bool setPLCSys()
       {
           return write(PC_Reset_SG_Address, false);
       }
       /// <summary>
       /// PLC系统通讯成功标志位置1
       /// </summary>
       /// <returns></returns>
       public bool SetPLComOK()
       {
           return write(Sys_Com_OK_Address, true);
       }
       /// <summary>
       /// 设置总通讯成功标志位
       /// </summary>
       /// <returns></returns>
       public bool SetAllComOK()
       {
           return write(Sys_Com_OK_Address, true);
       }
       /// <summary>
       /// 设置总系统复位（置1）
       /// </summary>
       /// <returns></returns>
       public bool SetAllReset()
       {
           return write(Sys_Reset_SG_Address, true);
       }
       /// <summary>
       /// 设置总系统复位（置0）
       /// </summary>
       /// <returns></returns>
       public bool ResetAllReset()
       {
           return write(Sys_Reset_SG_Address, false);
       }
       /// <summary>
       /// 写bool信号
       /// </summary>
       /// <param name="address"></param>
       /// <param name="value"></param>
       /// <returns></returns>
       private bool write(string address, bool value)
       {
           return (S7_COM_FUN.Write_Req(cp_descr, cref, orderid, address, value) == 0);
       }
    }
}
