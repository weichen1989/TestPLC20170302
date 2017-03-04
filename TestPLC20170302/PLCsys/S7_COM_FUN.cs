using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

using System.Windows.Forms;

namespace BIMI.RGWTB
{
    /// <summary>
    /// 版权所有: 版权所有(C) 2016，华中科技大学无锡研究院叶片智能制造研究所
    /// 内容摘要: 本类功能是实现PC和PLC通讯、数据读写
    /// 完成日期: 2016年3月
    /// 版    本:
    /// 作    者: 连学军
    /// 
    /// 修改记录1: 
    /// 修改日期: 
    /// 版 本 号: 
    /// 修 改 人: 
    /// 修改内容:
    /// </summary>
    public class S7_COM_FUN
    {
        #region 对s732.dll的函数进行重新声明，以符合C#的类型和语法，只声明了部分函数
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_device(UInt16 i, ref UInt16 number_ptr, StringBuilder dev_name);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_vfd(StringBuilder dev_name, UInt16 i, ref UInt16 number_ptr, StringBuilder vfd_name);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_init(StringBuilder dev_name, StringBuilder vfd_name, ref UInt32 cp_descr_ptr);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_cref(UInt32 cp_descr, StringBuilder conn_name, ref UInt16 cref_ptr);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_initiate_req(UInt32 cp_descr, UInt16 cref);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_receive(UInt32 cp_descr, ref UInt16 cref_ptr, ref UInt16 orderid_ptr);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_initiate_cnf();
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_read_req(UInt32 cp_descr, UInt16 cref, UInt16 orderid, ref S7_READ_PARA read_para);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_write_req(UInt32 cp_descr, UInt16 cref, UInt16 orderid, ref S7_WRITE_PARA write_para, IntPtr od_ptr);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_read_cnf(IntPtr od_ptr, ref UInt16 var_length_ptr, Byte[] value_ptr);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_write_cnf();
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_multiple_read_req(UInt32 cp_descr, UInt16 cref, UInt16 orderid, UInt16 number,[In,Out] S7_READ_PARA[]read_para);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
         public static extern int s7_get_multiple_read_cnf(IntPtr od_ptr, ref UInt16 result_array, [In,Out] UInt16 []var_length_array,IntPtr[] value_array);//不确定这种声明方式是否正确
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_multiple_write_req(UInt32 cp_descr, UInt16 cref, UInt16 orderid, UInt16 number, [In, Out] S7_WRITE_PARA[] write_para, IntPtr od_ptr);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_multiple_write_cnf(ref UInt16 result_array);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_cycl_read_init_req(UInt32 cp_descr, UInt16 cref, UInt16 orderid, UInt16 cycl_time, UInt16 number, ref S7_READ_PARA[] read_para);
        //cycl_time只能是1s，10s，100s，若取其他值会被舍去变为默认值，2-9会被认为是1s，11-99会被认为是10s
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_cycl_read_init_cnf();
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_cycl_read_start_req(UInt32 cp_descr, UInt16 cref, UInt16 orderid);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_cycl_read_start_cnf();
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_cycl_read_ind(IntPtr od_ptr, StringBuilder result_array, ref UInt16[] var_length_array, ref Byte[] value_array);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_cycl_read_abort_ind();
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_cycl_read_stop_req(UInt32 cp_descr, UInt16 cref, UInt16 orderid);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_cycl_read_stop_cnf();
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_cycl_read_delete_req(UInt32 cp_descr, UInt16 cref, UInt16 orderid);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_cycl_read_delete_cnf();
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_cycl_read(UInt32 cp_descr, UInt16 cref, UInt16 orderid, UInt16 cycl_time, UInt16 number, [In, Out] S7_READ_PARA[] read_para);
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_abort(UInt32 cp_descr, UInt16 cref);//断开连接；
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_get_abort_ind();
        [DllImport("s732", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int s7_shut(UInt32 cp_descr);//清除初始化数据；
 #endregion
        #region 对s7函数的返回值进行定义
        // Important MSG
        public const int S7_OK = 0;
        public const int S7_ERR_RETRY = -1;
        public const int S7_NO_MSG = 0;
        public const int S7_UNKNOWN_MSG = 1;

        /* initiate, abort */
        public const int S7_INITIATE_CNF = 0x0101;
        public const int S7_INITIATE_IND = 0x0102;
        public const int S7_ABORT_IND = 0x0103;
        public const int S7_AWAIT_INITIATE_CNF = 0x0104;

        /* VFD */
        public const int S7_VFD_STATE_CNF = 0x0201;
        public const int S7_VFD_USTATE_IND = 0x0202;

        /* variables */
        public const int S7_READ_CNF = 0x0301;
        public const int S7_WRITE_CNF = 0x0302;
        public const int S7_MULTIPLE_READ_CNF = 0x0303;
        public const int S7_MULTIPLE_WRITE_CNF = 0x0304;

        /* cyclic variables */
        public const int S7_CYCL_READ_ABORT_IND = 0x0401;
        public const int S7_CYCL_READ_DELETE_CNF = 0x0402;
        public const int S7_CYCL_READ_IND = 0x0403;
        public const int S7_CYCL_READ_INIT_CNF = 0x0404;
        public const int S7_CYCL_READ_START_CNF = 0x0405;
        public const int S7_CYCL_READ_STOP_CNF = 0x0406;

        /* PBK variables */
        public const int S7_BSEND_CNF = 0x0601;
        public const int S7_BRCV_IND = 0x0602;
        public const int S7_BRCV_CANCEL_IND = 0x0603;
        public const int S7_BRCV_STOP = 0x0604;

        /* DIAG variables */
        public const int S7_DIAG_IND = 0x0701;

        /* Domain variables */
        public const int S7_BLOCK_READ_CNF = 0x0801;
        public const int S7_BLOCK_WRITE_CNF = 0x0802;
        public const int S7_BLOCK_DELETE_CNF = 0x0803;
        public const int S7_BLOCK_LINK_IN_CNF = 0x0804;
        public const int S7_BLOCK_COMPRESS_CNF = 0x0805;
        public const int S7_PASSWORD_CNF = 0x0806;

        /* PMC variables */
        public const int S7_MSG_INITIATE_CNF = 0x4401;
        public const int S7_MSG_ABORT_CNF = 0x4402;
        public const int S7_SCAN_IND = 0x4403;
        public const int S7_ALARM_IND = 0x4404;
        /*defines for access*/
        public const int S7_ACCESS_NAME = 1;
        public const int S7_ACCESS_SYMB_ADDRESS = 2;
        public const int S7_ACCESS_ADDRESS = 3;

        #endregion
        public static bool IsWriting = false;//写过程中关闭读
        public static bool IsReading = false;//读过程中关闭写
        #region 读写结构体重新声明 初始化
        //读参数结构体重新声明
        [StructLayout(LayoutKind.Sequential)]
        public struct S7_READ_PARA
        {
            public UInt16 access;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
            public String var_name;
            public UInt16 index;
            public UInt16 subindex;
            public UInt16 address_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public String address;
        };
        //读参数结构体初始化函数
        public static void R_Struct_Inital(ref S7_READ_PARA R_Struct,String Mvar_name) 
        {
            R_Struct.access = S7_ACCESS_SYMB_ADDRESS;
            R_Struct.var_name = Mvar_name;
            R_Struct.index = 0;
            R_Struct.subindex = 0;
            R_Struct.address_len = 0;
            R_Struct.address = null;
        }
        //写参数结构体重新声明
        [StructLayout(LayoutKind.Sequential)]
        public struct S7_WRITE_PARA
        {
            public UInt16 access;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
            public String var_name;
            public UInt16 index;
            public UInt16 subindex;
            public UInt16 address_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public String address;
            public UInt16 var_length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public Byte[] value;
        };
        //写参数结构体初始化函数
        public static void W_Struct_Initial(ref S7_WRITE_PARA W_Struct, String Mvar_name, Byte[] Mvalue)
        {
            W_Struct.access = S7_ACCESS_SYMB_ADDRESS;
            W_Struct.var_name = Mvar_name;
            W_Struct.index = 0;
            W_Struct.subindex = 0;
            W_Struct.address_len = 0;
            W_Struct.address = null;
            W_Struct.var_length =(UInt16)Mvalue.Length;
            W_Struct.value = new Byte[256];
            for (int i = 0; i <= (Mvalue.Length-1);i++ )
            {
                W_Struct.value[i] = Mvalue[i];
            }
        }
 #endregion
        //自己利用s732.dll函数集成的函数
        //初始化函数，主要功能是获取访问PLC网卡的位置，以及在STEP7组态的虚拟设备（VFD）信息
        public static int Initial(UInt16 i, ref UInt32 cp_de)
        {
            int[] ret = new int[3];
            UInt16 num1 = 0;
            UInt16 num2 = 0;
            StringBuilder dev_name = new StringBuilder();
            StringBuilder vfd_name = new StringBuilder();
            ret[0] = s7_get_device(i, ref num1, dev_name);//第i个已安装的通讯处理器（CP）,num1，dev_name为返回值
            ret[1] = s7_get_vfd(dev_name, 0, ref num2, vfd_name);//配置的组态虚拟设备VFD
            ret[2] = s7_init(dev_name, vfd_name, ref cp_de);//登录通讯系统
            if (ret[2] == 0)
            {
                Console.WriteLine("Initial OK!");
                Console.WriteLine("dev_name is " + dev_name);
                Console.WriteLine("vfd_name is " + vfd_name);
                return 0;
            }
            else
            {
                Console.WriteLine("Initial ERR!");
                return -1;
            }
        }
  
        //2016年3月24日新版本  
        //如果初始化成功，则应用程序可以向PLC发起建立连接的请求，建立连接是之后一切操作的基础
        public static int Set_Connection(UInt32 cp_descr, StringBuilder con_name, ref UInt16 cref, ref  UInt16 orderid)
        {
            int[] ret = new int[3];
            Int16 i = 0;
            ret[0] = s7_get_cref(cp_descr, con_name, ref cref);
            ret[0] = s7_initiate_req(cp_descr, cref);
            if (ret[0] != 0)
            {
                Console.WriteLine("Set Connection ERR!(-1)");
                return -1;
            }
            else
            {
                do
                {
                    Thread.Sleep(5);
                    i++;
                    ret[1] = s7_receive(cp_descr, ref cref, ref orderid);
                    switch (ret[1])
                    {
                        case S7_NO_MSG:
                            Console.WriteLine("NO MESSAGE");
                            break;
                        case S7_INITIATE_CNF:
                            ret[2] = s7_get_initiate_cnf();
                            Console.WriteLine("INTITIATE CNF");
                            break;
                        default:
                            break;
                    }
                } while (ret[1] != S7_INITIATE_CNF && i < 200);

                if ((ret[1] == S7_INITIATE_CNF && ret[2] == 0))
                {
                    Console.WriteLine(i);
                    Console.WriteLine("Set Connection OK!");
                    return 0;
                }
                else
                {
                    Console.WriteLine("Set Connection ERR!(-2)");
                    return -2;
                }
            }
        }
        //Write_Req函数重载了3个方法，可以实现bool、byte、float的写操作请求
        //写100次，若不成功则停止
        #region Write_Req函数重载了3个方法，可以实现bool、byte、float的写操作请求
        public static int Write_Req(UInt32 cp_descr, UInt16 cref, UInt16 orderid, string Address, bool Value)
        {
            IsWriting = true;
            int ret=-1;
            IntPtr Null_Ptr = new IntPtr();
            S7_WRITE_PARA write_para = new S7_WRITE_PARA();
            Byte[] BValue = new Byte[1];
            if (Value == true)
            {
                BValue[0] = 1;
            }
            else
            {
                BValue[0] = 0;
            }
            W_Struct_Initial(ref write_para, Address, BValue);
            int num = 0;
            ret = s7_write_req(cp_descr, cref, orderid, ref write_para, Null_Ptr);
            while (ret != 0 && num<20)
            {
                ret = s7_write_req(cp_descr, cref, orderid, ref write_para, Null_Ptr);
                num++;
                Thread.Sleep(50);
            }
            if (ret == -2)
            {
                //MessageBox.Show(Address + "写入失败.", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            IsWriting = false;
            return ret;
        }
        public static int Write_Req(UInt32 cp_descr, UInt16 cref, UInt16 orderid, string Address, byte Value)
        {
            IsWriting = true;
            int ret=-1;
            IntPtr Null_Ptr = new IntPtr();
            S7_WRITE_PARA write_para = new S7_WRITE_PARA();
            Byte[] BValue = new Byte[1];//保证write_para和BValue的大小相等？
            BValue[0] = Value;
            W_Struct_Initial(ref write_para, Address, BValue);
            int num = 0;
            ret = s7_write_req(cp_descr, cref, orderid, ref write_para, Null_Ptr);
            while (ret != 0 && num < 20)
            {
                ret = s7_write_req(cp_descr, cref, orderid, ref write_para, Null_Ptr);
                num++;
                Thread.Sleep(10);
            }
            if (ret == -2)
            {
                //MessageBox.Show(Address + "写入失败.","错误",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            IsWriting = false;
            return ret;
        }
        public static int Write_Req(UInt32 cp_descr, UInt16 cref, UInt16 orderid, string Address, float Value)
        {
            IsWriting = true;
            int ret=-1;
            IntPtr Null_Ptr = new IntPtr();
            S7_WRITE_PARA write_para = new S7_WRITE_PARA();
            Byte[] BValue = new Byte[4];
            DATA_CONVERTER.Float_To_Byte(Value, ref BValue);
            DATA_CONVERTER.Byte_ExRank(ref BValue, 4);
            W_Struct_Initial(ref write_para, Address, BValue);
            int num = 0;
            ret = s7_write_req(cp_descr, cref, orderid, ref write_para, Null_Ptr);
            while (ret != 0 && num < 20)
            {
                ret = s7_write_req(cp_descr, cref, orderid, ref write_para, Null_Ptr);
                num++;
                Thread.Sleep(10);
            }
            if (ret == -2)
            {
                //MessageBox.Show(Address + "写入失败.", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            IsWriting = false;
            return ret;
        }
        #endregion
        /// <summary>
        /// 接收数据,如果返回nomesssge 则不会对value_ptr进行修改
        /// </summary>
        /// <param name="cp_descr"></param>
        /// <param name="cref"></param>
        /// <param name="orderid"></param>
        /// <param name="read_para"></param>
        /// <param name="value_ptr"></param>
        /// <returns></returns>
        public static int Receive(UInt32 cp_descr, UInt16 cref, UInt16 orderid, ref S7_READ_PARA read_para, Byte[] value_ptr)
        {
            int ret;
            UInt16 value_length=(UInt16)value_ptr.Length;
            IntPtr NULL_PTR = new IntPtr();
            UInt16 result=0;
            ret=s7_receive(cp_descr, ref cref, ref orderid);
                switch(ret)
                {
                    case S7_NO_MSG:
                        break;
                    case S7_INITIATE_CNF:
                        s7_get_initiate_cnf();
                        break;
                    case S7_READ_CNF:
                        s7_get_read_cnf(NULL_PTR, ref value_length, value_ptr);
              //          S7_read_req(cp_descr, cref, orderid, ref read_para);
                        break ;
                    case S7_WRITE_CNF:
                        s7_get_write_cnf();
                        break;
                    case S7_MULTIPLE_READ_CNF:
                        break;
                    case S7_MULTIPLE_WRITE_CNF:
                        s7_get_multiple_write_cnf(ref result);
                        break;
                    case S7_ABORT_IND:
                        s7_get_abort_ind();
                        break;
                    default:
                        break;
                }
                return ret;
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="cp_descr"></param>
        /// <param name="cref"></param>
        /// <returns></returns>
        public static int Abort_Connection(UInt32 cp_descr, UInt16 cref)
        {
            int ret;
            ret = s7_abort(cp_descr, cref);
            if (ret == 0)
            {
                //Console.WriteLine("Abort_Connection OK!");
                return 0;
            }
            else
            {
                //Console.WriteLine("Abort_Connection ERR!(-1)");
                return -1;
            }
        }
    }
}
