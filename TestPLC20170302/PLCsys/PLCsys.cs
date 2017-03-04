using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Diagnostics;

namespace BIMI.RGWTB
{
    /// <summary>
    /// 版权所有: 版权所有(C) 2016，华中科技大学无锡研究院叶片智能制造研究所
    /// 内容摘要: 本类是PLC类
    /// 完成日期: 2016年5月23日
    /// 版    本:
    /// 作    者: 陈巍
    /// 
    /// 修改记录1: 
    /// 修改日期:2016年6月20日
    /// 版 本 号:
    /// 修 改 人:陈巍
    /// 修改内容:添加静态成员变量
    /// </summary>
    public class PLCsys
    {
        public static UInt16 i = 1;//s7Initial
        //private static UInt32 cp_descr;//连接1
        public static UInt32 Cp_descr;
       // private static UInt16 cref;
        public static UInt16 Cref;
        //private static UInt16 orderid;
        public static UInt16 Orderid;
        //private static UInt16 cref2;//连接2
        public static UInt16 Cref2;
        //private static UInt16 orderid2;
        public static UInt16 Orderid2;

        public System.Byte[] Value_store = new System.Byte[150];//单次从PLC读取的字节数
        /// <summary>
        /// PLC可读
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ReadyToReadEvent;//PLC返回可读参数
        public int read_CNF;
        public int Read_CNF
        {
            get
            {
                return read_CNF;
            }
            private set
            {
                if (value == S7_COM_FUN.S7_READ_CNF && value != read_CNF)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("PLC值已更改", Read_CNF, value);
                    if (ReadyToReadEvent!=null)
                    {
                        this.ReadyToReadEvent(this,e);
                    }
                }
                read_CNF = value;
            }
        }
        public static S7_COM_FUN.S7_READ_PARA ReadPara = new S7_COM_FUN.S7_READ_PARA();
        
        public static StringBuilder ConnName{ get; set; }// 连接名称
        public static StringBuilder ConnName2{ get;set;}
        public static string IniReadPara { get; set; }

        #region PLC起始地址
        public static int[] RailStartAddress = new int[2];//static 可提前设置
        private static int fixtureStartAddress;
        private static int comStartAddress;
        #endregion
        #region 系统中PLC成员 根据实际配置更改
        public RailPLC Rail1 { get; private set; }
        public RailPLC Rail2 { get; private set; }
        public FixturePLC Fixture { get;private set; }//应该为共用 static
        public ComPLC Com { get; private set; }//应该为共用 static
        #endregion
        /// <summary>
        /// 自定义构造函数
        /// </summary>
        /// <param name="railStartAdd"></param>
        /// <param name="fixtureStartAdd"></param>
        /// <param name="comStartAdd"></param>
        /// <param name="conName"></param>
        /// <param name="conName2"></param>
        public PLCsys(int []railStartAdd, int fixtureStartAdd, int comStartAdd, StringBuilder conName, StringBuilder conName2)
        {
            //500,550,600,610,"S7 connection_1"
            RailStartAddress[0] = railStartAdd[0];
            RailStartAddress[1] = railStartAdd[1];
            fixtureStartAddress = fixtureStartAdd;
            comStartAddress = comStartAdd;
            ConnName = conName;
            ConnName2 = conName2;
        }

        public void Initial()
        {
            Fixture = new FixturePLC(fixtureStartAddress, Cp_descr, Cref, Orderid);
            Com = new ComPLC(comStartAddress, Cp_descr, Cref, Orderid);
            Rail1 = new RailPLC(RailStartAddress[0], Cp_descr, Cref, Orderid);
            Rail2 = new RailPLC(RailStartAddress[1], Cp_descr, Cref, Orderid); 
            InitialReadPara();
        }

        /// <summary>
        /// PLC连接是否成功
        /// </summary>
        /// <returns></returns>
        public bool Connected()
        {
            if (S7Initial()&&SetConnect())
            {
                Initial();//初始化PLC读写参数
                return true;
            }
            return false;       
        }

        /// <summary>
        /// s7初始化
        /// </summary>
        /// <returns></returns>
        private bool S7Initial()
        {
            return S7_COM_FUN.Initial(i, ref Cp_descr) == 0;
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <returns></returns>
        private bool SetConnect()
        {
            //两个连接
            bool res1=(S7_COM_FUN.Set_Connection(Cp_descr, ConnName,ref Cref, ref Orderid)==0);
            Thread.Sleep(100);
            res1 &= (S7_COM_FUN.Set_Connection(Cp_descr, ConnName2, ref Cref2, ref Orderid2) == 0);
            return res1;
        }

        /// <summary>
        /// 初始化读参数,起始地址 字节
        /// </summary>
        private void InitialReadPara()
        {
            S7_COM_FUN.R_Struct_Inital(ref ReadPara, IniReadPara);//读取数据块1,V200开始150个字节
        }

        /// <summary>
        /// 发起读请求
        /// </summary>
        /// <returns></returns>
        public bool RequestRead()
        {
            return S7_COM_FUN.s7_read_req(Cp_descr, Cref2, Orderid2, ref ReadPara)==0;
        }

        /// <summary>
        /// 接收PLC数据
        /// </summary>
        /// <returns></returns>
        public void Receive()
        {
            while (true)
            {
                Thread.Sleep(3);
                int resw = S7_COM_FUN.Receive(Cp_descr, Cref, Orderid, ref ReadPara, Value_store);
                Thread.Sleep(3);
                while (!S7_COM_FUN.IsWriting)
                {
                    Read_CNF = S7_COM_FUN.Receive(Cp_descr, Cref2, Orderid2, ref ReadPara, Value_store);
                }
            }
        }

        public void ResetReadCNF()
        {
            Read_CNF = -2;
        }

        /// <summary>
        /// 复位plc和伺服故障复位
        /// </summary>
        /// <returns></returns>
        public bool ResetPLCsys()
        {
            bool ret1 = Com.ResetAllReset();
            Thread.Sleep(100);
            ret1 &= Com.SetAllReset();
            Thread.Sleep(100);
            ret1 &= Com.ResetAllReset();
            if (ret1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// (系统设置)设置PLC系统参数
        /// </summary>
        /// <param name="railStartAdd"></param>
        /// <param name="fixtureStartAdd"></param>
        /// <param name="comStartAdd"></param>
        /// <param name="conname1"></param>
        /// <param name="conname2"></param>
        public static void SetPLCPara(int[] railStartAdd, int fixtureStartAdd, int comStartAdd, string conname1, string conname2)
        {
            RailStartAddress[0] = railStartAdd[0];
            RailStartAddress[1] = railStartAdd[1];
            fixtureStartAddress = fixtureStartAdd;
            comStartAddress = comStartAdd;
            ConnName = new StringBuilder(conname1);
            ConnName2 = new StringBuilder(conname2);
        }

        /// <summary>
        /// 复位PLC系统参数
        /// </summary>
        public static void ReseetPLCPara()
        {
            RailStartAddress[0] = 500;
            RailStartAddress[1] = 550;
            fixtureStartAddress = 600;
            comStartAddress = 610;
            ConnName = new StringBuilder("S7 connection_1");
            ConnName2 = new StringBuilder("S7 connection_2");
        }
    }
}
