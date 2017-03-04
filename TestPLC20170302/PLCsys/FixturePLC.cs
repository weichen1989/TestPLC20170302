using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using System.Diagnostics;
namespace BIMI.RGWTB
{
    public class FixturePLC
    {
        /// <summary>
        /// 自定义构造函数
        /// </summary>
        /// <param name="startAddr">PLC起始地址</param>
        /// <param name="cp_desc"></param>
        /// <param name="cre"></param>
        /// <param name="ordeid"></param>
        public FixturePLC(int startAddr, UInt32 cp_desc, UInt16 cre, UInt16 ordeid)
        {
            startAddress = startAddr;
            cp_descr = cp_desc;
            cref = cre;
            orderid = ordeid;
            Initial();
        }
        #region 读信号
        //bool类型变量，共15个
        public bool Fixture_Local_Reset_SG;//翻转小车本地复位信号(R)
        private bool fixture_Jog_Rota_CW_SG;//点动正转(顺时针)标志位(R)
        public bool Fixture_Jog_Rota_CW_SG
        {
            get { return fixture_Jog_Rota_CW_SG; }
            set
            {
                if (value == false && fixture_Jog_Rota_CW_SG == true && fixture_Jog_Rota_CWW_SG == false)//正转反转标志位均为false
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("工装正向翻转到位", fixture_Jog_Rota_CW_SG, value);
                    if (this.FixtureRotateDone != null)
                    {
                        this.FixtureRotateDone(this, e);
                    }
                }
                fixture_Jog_Rota_CW_SG = value;
            }
        }
        private bool fixture_Jog_Rota_CWW_SG;//点动反转(逆时针)标志位(R)
        public bool Fixture_Jog_Rota_CWW_SG
        {
            get { return fixture_Jog_Rota_CWW_SG; }
            set
            {
                if (value == false && fixture_Jog_Rota_CWW_SG == true && fixture_Jog_Rota_CW_SG == false)//正转反转标志位均为false
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("工装反向翻转到位", fixture_Jog_Rota_CWW_SG, value);
                    if (this.FixtureRotateDone != null)
                    {
                        this.FixtureRotateDone(this, e);
                    }
                }
                fixture_Jog_Rota_CWW_SG = value;
            }
        }
        public bool Local_PLC_ERR;//上电标志位(R)
        private bool fixture_Rota_CW_SG;//正转反馈标志位(R)  不使用

        public bool Fixture_Rota_CW_SG
        {
            get { return fixture_Rota_CW_SG; }
            set 
            {
                fixture_Rota_CW_SG = value; 
            }
        }

        private bool fixture_Rota_CWW_SG;//反转反馈标志位(R)不使用

        public bool Fixture_Rota_CWW_SG
        {
            get { return fixture_Rota_CWW_SG; }
            set 
            {
                fixture_Rota_CWW_SG = value; 
            }
        }
        public bool Fixture1_Rotation_Done;//1号翻转小车转动到位标志位(R)--未安装传感器
        public bool Fixture2_Rotation_Done;//2号翻转小车转动到位标志位(R)--未安装传感器
        public bool Fixture_Rotation_Done;//翻转小车转动到位标志位（R）--未安装传感器
        public bool Fixture_LRota_CW_SG;//长动正转标志位(R)
        private bool fixture_LRota_CWW_SG;//长动反转标志位（R）
        public bool Fixture_LRota_CWW_SG
        {
            get { return fixture_LRota_CWW_SG; }
            set { fixture_LRota_CWW_SG = value; }
        }
        public bool Fixture_Current_Mode;//翻转小车当前运行模式（R）
        private bool fixture_Stop_SG;//翻转小车急停(断电)标志位（R）
        public bool Fixture_Stop_SG
        {
            get { return fixture_Stop_SG; }
            set
            {
                if (value == true && value != fixture_Stop_SG)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("翻转小车急停(断电)", Fixture_Stop_SG, value);
                    if (this.FixturePLCEstopRaise != null)
                    {
                        this.FixturePLCEstopRaise(this, e);
                    }
                }
                fixture_Stop_SG = value;
            }
        }
        private bool local_Communication_OK = true;//本地设备通信正常标志位（R）
        public bool Local_Communication_OK
        {
            get { return local_Communication_OK; }
             set
            {
                if (value == false && local_Communication_OK==true)////复位也会报错
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("工装PLC:通讯失败", local_Communication_OK, value);
                    if (this.FixturePLCErrRaise != null)
                    {
                        this.FixturePLCErrRaise(this, e);
                    }
                }
                local_Communication_OK = value;
            }
        }
        #region FLOAT类型
        public float Fixture_XAngle;
        private float fixture_YAngle;
        /// <summary>
        /// 上位机倾角传感器Y轴倾角(R)
        /// </summary>
        public float Fixture_YAngle
        {
            get { return fixture_YAngle; }
            set 
            {
                if (value!=fixture_YAngle&& fixture_YAngle!=0.0)//排除初始状态
                {
                    if (value > maxAngle || value < minAngle || (value - angleToRotate) * (fixture_YAngle - angleToRotate) < 0 || (value - angleToRotate) * (fixture_YAngle - angleToRotate) == 0)//限制最大角度,判断是否转动到位(前后两次值与angleToRotate的关系)
                    {
                        if (!StopRotate())
                        {
                            if (!StopRotate())
                                Console.WriteLine("叶片翻转停止失败，请停止运行");
                        }
                    }
                }
                fixture_YAngle = value; 
            }
        }
        #endregion
        #endregion
        #region 写信号地址
        #region bool类型
        /// <summary>
        /// 点动正转(顺时针)按钮位
        /// </summary>
        public string Fixture_Jog_Rota_CW_Address;//
        /// <summary>
        /// 点动反转(逆时针)按钮位
        /// </summary>
        public string Fixture_Jog_Rota_CWW_Address;
        /// <summary>
        /// 长动正转按钮位
        /// </summary>
        public string Fixture_LRota_CW_Address;
        /// <summary>
        /// 长动反转按钮位
        /// </summary>
        public string Fixture_LRota_CWW_Address;
        /// <summary>
        /// 上位机远程复位信号
        /// </summary>
        public string PC_Reset_Address;
        /// <summary>
        /// 上位机远程急停信号
        /// </summary>
        public string PC_Enmergency_Stop_Address;
        /// <summary>
        /// 远程计算机通信正常标志位
        /// </summary>
        public string PC_Communication_OK_Address;
        /// <summary>
        /// 远程机器人急停标志位
        /// </summary>
        public string Robot_Enmergengcy_Stop_Address;
        /// <summary>
        /// 上位机倾角传感器Y轴倾角
        /// </summary>
        public string Fixture_YAngle_Stop_Address;
        /// <summary>
        /// 上位机倾角传感器X轴倾角
        /// </summary>
        public string Fixture_XAngle_Stop_Address;
        #endregion

        #endregion
        #region 错误及警告信号
        private bool fixture_ERR_SG;
        /// <summary>
        /// 翻转小车故障标志位(R)
        /// </summary>
        public bool Fixture_ERR_SG
        {
            get { return fixture_ERR_SG; }
            set
            {
                if (value == true && fixture_ERR_SG==false)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("翻转小车故障", Fixture_ERR_SG, value);
                    if (this.FixturePLCErrRaise != null)
                    {
                        this.FixturePLCErrRaise(this, e);
                    }
                }
                fixture_ERR_SG = value;
            }
        }
        #endregion
        #region 事件
        public event EventHandler<PropertyChangedEventArgs> FixturePLCWarningRaise;
        public event EventHandler<PropertyChangedEventArgs> FixturePLCErrRaise;
        public event EventHandler<PropertyChangedEventArgs> FixturePLCEstopRaise;
        public event EventHandler<PropertyChangedEventArgs> FixtureRotateDone;//叶片翻转完成
        #endregion
        /// <summary>
        /// PLC起始地址
        /// </summary>
        private int startAddress;

        private float angleToRotate=0.0f;//翻转角度，用于判断是否已转到位

        private static float maxAngle = 38, minAngle = -38;//极限旋转角度

        /// <summary>
        /// 最大正向翻转角度
        /// </summary>
        public static float MinAngle
        {
            get { return FixturePLC.minAngle; }
            //set { FixturePLC.minAngle = value; }
        }

        /// <summary>
        /// 最大反向翻转角度
        /// </summary>
        public static float MaxAngle
        {
            get { return FixturePLC.maxAngle; }
            //set { FixturePLC.maxAngle = value; }
        }   
        #region PLC读写参数
        private UInt32 cp_descr = 0;
        private UInt16 cref = 0;
        private UInt16 orderid = 0;
        #endregion
        /// <summary>
        /// 初始化-分配写信号地址
        /// </summary>
        public void Initial()
        {
            string S_Add = startAddress.ToString();
            string S1 = "DB1,";
            string S_X = "X";
            Fixture_Jog_Rota_CW_Address = S1 + S_X + S_Add + ".0";
            Fixture_Jog_Rota_CWW_Address = S1 + S_X + S_Add + ".1";
            Fixture_LRota_CW_Address = S1 + S_X + S_Add + ".2";
            Fixture_LRota_CWW_Address = S1 + S_X + S_Add + ".3";
            PC_Reset_Address = S1 + S_X + S_Add + ".4";
            PC_Enmergency_Stop_Address = S1 + S_X + S_Add + ".5";
            PC_Communication_OK_Address = S1 + S_X + S_Add + ".6";
            Robot_Enmergengcy_Stop_Address = S1 + S_X + S_Add + ".7";
        }

        /// <summary>
        /// 转动
        /// </summary>
        /// <param name="direction">转动方向</param>
        /// <param name="speed">转动速度</param>
        /// <param name="angle">转动角度</param>
        /// <param name="IsJog">点动</param>
        /// <returns></returns>
        public bool Rotate(bool direction, float speed, float angle,bool IsJog)
        {
            bool result = true;
            if (!fixture_ERR_SG)
            {
                if (Fixture_Current_Mode)//远程模式才能转动
                {
                    //目前只支持点动，根据角度传感器来判断旋转角度
                    if (IsJog)
                    {
                        //写转向
                        if (direction)//正转
                        {
                            result &= Write(Fixture_Jog_Rota_CW_Address, true);
                            result &= Write(Fixture_Jog_Rota_CWW_Address, false);
                        }
                        else
                        {
                            result &= Write(Fixture_Jog_Rota_CWW_Address, true);
                            result &= Write(Fixture_Jog_Rota_CW_Address, false);
                        }
                        //写速度（暂不支持）
                        return result;
                    }//end if (IsJog)
                    else
                    {
                        return false;
                    }
                }// end if (Fixture_Current_Mode)
                else
                {
                    Console.WriteLine("工装转动失败:目前为本地模式，请设置为远程模式后重试.");
                    return false;
                }
            }//end if (!fixture_ERR_SG)
            else
            {
                Console.WriteLine("工装Fixture_ERR故障，转动工装失败，请排除故障复位后重试.");
                return false;
            }
        }

        /// <summary>
        /// 点动--单参数多线程
        /// </summary>
        /// <param name="direc">转动方向</param>
        public bool JogRotate(bool direc)
        {
            angleToRotate = maxAngle;//点动时设置为最大角
            return Rotate(direc, 0, 0, true);
        }

        /// <summary>
        ///  持续运动（按旋转角度（绝对）旋转）
        /// </summary>
        /// <param name="angle"></param>
        public bool ContinuousRotate(float angle)
        {
            angleToRotate = angle;//设置要到达的角度
            bool direc = angle - fixture_YAngle > 0;//通过相对关系来决定转向
            return Rotate(direc, 0, 0, true);//只有点动
        }

        /// <summary>
        /// 回零
        /// </summary>
        public void BackHome()
        {
             ContinuousRotate(0);
        }

        /// <summary>
        /// 停止旋转
        /// </summary>
        public bool StopRotate()
        {
            bool ret1 = true, ret2 = true;
            //正反转转向位 置0
            if (Fixture_Jog_Rota_CWW_SG)
            {
                ret1=Write(Fixture_Jog_Rota_CWW_Address, false);
            }
            if (Fixture_Jog_Rota_CW_SG)
            {
                ret2=Write(Fixture_Jog_Rota_CW_Address, false);
            }
            return (ret1&&ret2);
        }

        /// <summary>
        /// 旋转完成
        /// </summary>
        public bool DoneRotation()
        {
            return Fixture_Rotation_Done;//标志位
        }

        /// <summary>
        /// 工装PLC复位
        /// </summary>
        public bool ResetFicxture()
        {
            bool ret;
            ret = Write(PC_Reset_Address, true);
            Thread.Sleep(200);
            ret&=Write(PC_Reset_Address, false);
            return ret;
        }

        /// <summary>
        /// 写通讯成功标志位--以PLC验证通讯是否成功
        /// </summary>
        public bool SetComOK()
        {
            return S7_COM_FUN.Write_Req(cp_descr, PLCsys.Cref2, PLCsys.Orderid2, PC_Communication_OK_Address, true) == 0;
        }
        /// <summary>
        /// 写bool信号
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private bool Write(string address, bool value)
        {
            return S7_COM_FUN.Write_Req(cp_descr, cref, orderid, address, value) == 0;
        }

        /// <summary>
        /// 写byte信号
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private bool Write(string address, byte value)
        {
            return S7_COM_FUN.Write_Req(cp_descr, cref, orderid, address, value) == 0;
        }

        /// <summary>
        /// 写float信号
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        private bool Write(string address, float value)
        {
            return S7_COM_FUN.Write_Req(cp_descr, cref, orderid, address, value) == 0;
        }

        /// <summary>
        /// 设置工装最大旋转角度
        /// </summary>
        /// <param name="maxYAngle">正向最大角度</param>
        /// <param name="minYAngle">负向最大角度</param>
        public void SetMaxAngle(float maxYAngle, float minYAngle)
        {
            maxAngle = maxYAngle;
            minAngle = minYAngle;
        }

        /// <summary>
        /// 工装急停
        /// </summary>
        /// <returns></returns>
        public bool FixtureEStop()
        {
            return Write(PC_Enmergency_Stop_Address, true);
        }
    }
}
