
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
    /// 内容摘要: 本类是导轨PLC类,包括主要……模块、……函数及功能是…….
    /// 完成日期: 2016年5月23日
    /// 版    本:
    /// 作    者: 陈巍
    /// 
    /// 修改记录1: 
    /// 修改日期:2016年5月26日
    /// 版 本 号:
    /// 修 改 人:陈巍
    /// 修改内容:添加一台激光传感器，地址为VD225,laser_Sensor2_Value
    /// </summary>
    /// 
    public class RailPLC
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
        #region bool类型
        public bool Touch_Sensor_SG;//触摸传感器测点信号（R）
        public bool TeleCylin_Max_On;//伸缩机构伸到位信号（R）
        public bool TeleCylin_Min_On;//伸缩机构缩到位信号（R）
        public bool Laser_Sensor_CH1_On;//激光传感器测点（CH1）信号（R）
        public bool Laser_Sensor_CH2_On;//激光传感器危险点（CH2）信号（R）
        public bool FlapWheel_RotaDrect;//百叶轮转动方向反馈（R）
        public bool Rail_MotionDrect;//导轨运动方向反馈（R）
        public bool Rail_Position_Match_Done;//导轨位置加载完成信号（R）
        public bool Rail_Search_OriginPT_Done;//导轨参考点搜索完成信号（R）
        public bool rail_To_Desti_Done=true;
        /// <summary>
        /// 导轨定位运动完成信号
        /// </summary>
        public bool Rail_To_Desti_Done
        {
            get { return rail_To_Desti_Done; }
            set 
            {
               if (value!=rail_To_Desti_Done&&value==true)//定位运动到位后自动将定位控制位置0
               {
                   PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:定位运动到位", rail_To_Desti_Done, value);
                   if (this.RailMoveDone != null)
                    {
                        this.RailMoveDone(this, e);
                    }
               }
                rail_To_Desti_Done = value; 
            }
        }
        private bool device_Elec_Off;
        /// <summary>
        /// 设备断电（急停）标志位（R）
        /// </summary>
        public bool Device_Elec_Off
        {
            get { return device_Elec_Off; }
            set
            {
                if (value == true && device_Elec_Off == false)//若初始状态下没有上电，则无法检测到
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:设备断电（急停）", Device_Elec_Off, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                device_Elec_Off = value;
            }
        }
        private bool local_Communication_OK=true;
        /// <summary>
        /// 本地设备通信正常标志位（R）,PLC判断上位机远程计算机通信正常标志位PC_Communication_OK置0时间超过0.5秒，则置0
        /// </summary>
        public bool Local_Communication_OK
        {
            get { return local_Communication_OK; }
            set
            {
                if (value == false && local_Communication_OK==true)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:通讯失败", local_Communication_OK, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                local_Communication_OK = value;
            }
        }
        #endregion
        #region float 类型
        private float laser_Sensor_Value;
        /// <summary>
        /// 激光传感器测距值（R）
        /// </summary>
        public float Laser_Sensor_Value
        {
            get { return laser_Sensor_Value; }
            set { laser_Sensor_Value = value; }
        }
        private float laser_Sensor2_Value;
        /// <summary>
        /// 激光传感器2测距值（R）
        /// </summary>
        public float Laser_Sensor2_Value
        {
            get { return laser_Sensor2_Value; }
            set { laser_Sensor2_Value = value; }
        }
        private float flapWheel_SpeedOrTorque;
        /// <summary>
        /// 百叶轮速度反馈（实际转矩反馈）（R）
        /// </summary>
        public float FlapWheel_SpeedOrTorque
        {
            get { return flapWheel_SpeedOrTorque; }
            set
            {
                if (torqueOrDisFB)//转矩反馈-true
                {
                    if (Math.Abs(value) > grindingTorque + torqueTolerance | Math.Abs(value) < grindingTorque - torqueTolerance)
                    {
                        PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:百叶轮电机转矩超限", flapWheel_SpeedOrTorque, value);
                        if (this.TorqueWarningRaise != null)
                        {
                            this.TorqueWarningRaise(this, e);
                        }
                    }
                }
                flapWheel_SpeedOrTorque = value;
            }
        }
        private float rail_Robot_Position;
        /// <summary>
        /// 导轨位置反馈（R）
        /// </summary>
        public float Rail_Robot_Position
        {
            get { return rail_Robot_Position; }
            set 
            { 
                if (value>robotMaxPos||value<robotMinPos)
                {
                    StopMove();
                }
                rail_Robot_Position = value;
            }
        }
        private float rail_Robot_Speed;
        /// <summary>
        /// //导轨速度反馈（R）
        /// </summary>
        public float Rail_Robot_Speed
        {
            get { return rail_Robot_Speed; }
            set { rail_Robot_Speed = value; }
        }
        #endregion
        #endregion
        #region 写信号地址
        #region bool类型
        public string ACF_Source_Gas_Ctrl_Address;//柔顺装置气源开闭控制
        public string Dust_Collector_Ctrl_Address;//独立吸尘装置启停控制
        /// <summary>
        /// 测点传感器选择：0-探针，1-激光
        /// </summary>
        public string Select_Sensor_Ctrl_Address;//测点传感器选择
        public string TeleCylin_Ctrl_Address;//伸缩机构伸缩控制
        public string Laser_Sensor_CH1_Set_Address;//激光传感器CH1设置
        public string Laser_Sensor_CH2_Set_Address;//激光传感器CH2设置
        public string Laser_Sensor_Zero_Set_Address;//激光传感器归零设置
        public string Laser_Sensor_Light_Off_Address;//激光传感器激光亮灭设置
        public string Polish_Tool_Select_Address;//打磨工具选择
        public string Head_Face_Polish_Tool_Ctrl_Address;//气动端面打磨工具启停控制
        public string FlapWheel_Servo_Warn_Reset_Address;//百叶轮伺服故障复位控制
        public string FlapWheel_Start_Ctrl_Address;//百叶轮启停控制
        public string FlapWheel_RotaDerect_Ctrl_Address;//百叶轮方向控制
        public string Rail_Servo_Warn_Reset_Address;//导轨伺服故障复位控制
        public string Rail_Position_Match_Ctrl_Address;//导轨加载位置指令
        public string Rail_Search_OriginPT_Ctrl_Address;//导轨参考点搜索指令
        public string Rail_Jog_Ctrl_Address;//导轨点动控制
        public string Rail_MotionDrect_Ctrl_Address;//导轨运动方向控制
        public string Rail_To_Desti_Ctrl_Address;//导轨定位运动控制
        public string Rail_To_Desti_Stop_Address;//导轨停止进行中的定位运动指令
        public string PC_Communication_OK_Address;//远程计算机通信正常标志位
        public string Robot_Emergency_Stop_Address;//远程机器人急停标志位
        public string System_Reset_SG_Address;//远程复位信号
        #endregion
        #region byte类型
        /// <summary>
        /// 导轨运动模式-绝对0 相对1
        /// </summary>
        public string Rail_Motion_Mode_Selcet_Address;
        #endregion
        #region float类型
        /// <summary>
        /// 上位机气动端面打磨工具转速设置（W）
        /// </summary>
        public string HFPT_Speed_Set_Address;
        /// <summary>
        /// 上位机百叶轮速度设置（W）
        /// </summary>
        public string FlapWheel_Speed_Set_Address;
        /// <summary>
        /// 上位机导轨速度设置（W）
        /// </summary>
        public string Rail_Robot_Speed_Set_Address;
        /// <summary>
        /// 上位机导轨位置（绝对运动）或距离（相对运动）设置（W）
        /// </summary>
        public string Rail_Robot_Position_Set_Address;
        #endregion
        #endregion
        #region 错误及警告
        private byte flapWheel_Error_Code;//百叶轮控制错误代码（R）
        public byte FlapWheel_Error_Code
        {
            get { return flapWheel_Error_Code; }
            set
            {
                if (value == (byte)18)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:百叶轮控制错误", FlapWheel_Error_Code, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                flapWheel_Error_Code = value;
            }
        }
        private byte rail_Read_Coder_ErrCode;//导轨读取绝对值编码器错误代码（R）
        public byte Rail_Read_Coder_ErrCode
        {
            get { return rail_Read_Coder_ErrCode; }
            set
            {
                if (value == (byte)22)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:导轨读取绝对值编码器错误", Rail_Read_Coder_ErrCode, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                rail_Read_Coder_ErrCode = value;
            }
        }
        private byte rail_Position_Match_ErrCode;//导轨位置加载错误代码（R）
        public byte Rail_Position_Match_ErrCode
        {
            get { return rail_Position_Match_ErrCode; }
            set
            {
                if (value == (byte)23)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:导轨位置加载错误", Rail_Position_Match_ErrCode, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                rail_Position_Match_ErrCode = value;
            }
        }
        private byte rail_Search_OriginPT_ErrCode;//导轨参考点搜索错误代码（R）
        public byte Rail_Search_OriginPT_ErrCode
        {
            get { return rail_Search_OriginPT_ErrCode; }
            set
            {
                if (value == (byte)24)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:导轨参考点搜索错误", Rail_Search_OriginPT_ErrCode, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                rail_Search_OriginPT_ErrCode = value;
            }
        }
        private byte rail_Jog_ErrCode;//导轨点动运动错误代码（R）
        public byte Rail_Jog_ErrCode
        {
            get { return rail_Jog_ErrCode; }
            set
            {
                if (value == (byte)25)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:导轨参考点搜索错误", Rail_Jog_ErrCode, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                rail_Jog_ErrCode = value;
            }
        }
        private byte rail_To_Desti_ErrCode;//导轨定位运动错误代码（R）
        public byte Rail_To_Desti_ErrCode
        {
            get { return rail_To_Desti_ErrCode; }
            set
            {
                if (value == (byte)26)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:导轨定位运动错误", Rail_To_Desti_ErrCode, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                rail_To_Desti_ErrCode = value;
            }
        }
        private bool local_PLC_ERR;//本地PLC故障标志位（R）
        public bool Local_PLC_ERR
        {
            get { return local_PLC_ERR; }
            set
            {
                if (value == true&&value!=local_PLC_ERR)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:本地PLC故障", Local_PLC_ERR, value);
                    if (this.RailPLCErrRaise != null)
                    {
                        this.RailPLCErrRaise(this, e);
                    }
                }
                local_PLC_ERR = value;
            }
        }
        private bool local_Device_ERR;//本地设备故障标志位（R）
        public bool Local_Device_ERR
        {
            get { return local_Device_ERR; }
            set
            {
                if (value == true && local_Device_ERR==false)
                  {
                      PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:本地设备故障", Local_Device_ERR, value);
                      if (this.RailPLCErrRaise != null)
                     {
                         this.RailPLCErrRaise(this, e);
                     }
                  }
                local_Device_ERR = value;
            }
        }
        private bool flapWheel_Servo_Warn_SG;
        /// <summary>
        /// 百叶轮伺服驱动器报警信号（R）
        /// </summary>
        public bool FlapWheel_Servo_Warn_SG
        {
            get { return flapWheel_Servo_Warn_SG; }
            set
            {
                if (value == true && value != flapWheel_Servo_Warn_SG)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:百叶轮伺服驱动器报警", FlapWheel_Servo_Warn_SG, value);
                    if (this.RailPLCWarningRaise != null)
                    {
                        this.RailPLCWarningRaise(this, e);
                    }
                }
                flapWheel_Servo_Warn_SG = value;
            }
        }
        private bool rail_Servo_Warn_SG;
        /// <summary>
        /// 导轨伺服驱动器报警信号（R）
        /// </summary>
        public bool Rail_Servo_Warn_SG
        {
            get { return rail_Servo_Warn_SG; }
            set
            {
                if (value == true && value != rail_Servo_Warn_SG)
                {
                    PropertyChangedEventArgs e = new PropertyChangedEventArgs("导轨PLC:导轨伺服驱动器报警", Rail_Servo_Warn_SG, value);
                    if (this.RailPLCWarningRaise != null)
                    {
                        this.RailPLCWarningRaise(this, e);
                    }
                }
                rail_Servo_Warn_SG = value;
            }
        }
        #endregion
        #region 事件
        public event EventHandler<PropertyChangedEventArgs> RailPLCWarningRaise;
        public event EventHandler<PropertyChangedEventArgs> RailPLCErrRaise;
        public event EventHandler<PropertyChangedEventArgs> RailPLCEStopRaise;
        public event EventHandler<PropertyChangedEventArgs> RailMoveDone;//导轨运动到位
        public event EventHandler<PropertyChangedEventArgs> TorqueWarningRaise;//转矩过大或过小（和转速共用一位，只是模式不同）
        #endregion
        
        private float robotMinPos = 100;//导轨最小位置
        private float robotMaxPos = 5500;
        /// <summary>
        /// 导轨最大位置
        /// </summary>
        public float RobotMaxPos
        {
            get { return robotMaxPos; }
        }
        private float maxTorque = 13;//电机堵转转矩（绝对值）
        private float grindingTorque = 1.5f;
        /// <summary>
        /// 期望转矩
        /// </summary>
        public float GrindingTorque
        {
            get { return grindingTorque; }
        }
        private float torqueTolerance = 0.3F;
        /// <summary>
        /// 转矩容差
        /// </summary>
        public float TorqueTolerance
        {
            get { return torqueTolerance; }
        }
        private float moveSpeed = 50;
        /// <summary>
        /// 自动模式移动速度
        /// </summary>
        public float MoveSpeed
        {
            get { return moveSpeed; }
        }
        private const float maxMoveSpeed = 100;//导轨运动最大速度
        private bool torqueOrDisFB = false;//反馈模式：距离反馈-false 转矩反馈-true
        private float wheelSpeed = 500;//打磨工具转速
        private float maxWheelSpeed = 800;//百叶轮最大转速800 端面最大30000
        private bool grinderNum;//打磨工具编号 0-端面 1-百叶轮 默认是百叶轮
        public bool GrinderNum 
        { 
            get 
            {
                return grinderNum;
            }
          set 
          {
              if (value)
              {
                  maxWheelSpeed = 800;
              }
              else
              {
                  maxWheelSpeed = 0;
              }
              grinderNum=value;
          }
        }

        /// <summary>
        /// 自定义构造函数
        /// </summary>
        /// <param name="startAddr">起始地址</param>
        /// <param name="cp_desc"></param>
        /// <param name="cre"></param>
        /// <param name="ordeid"></param>
        public RailPLC(int startAddr, UInt32 cp_desc, UInt16 cre, UInt16 ordeid)
        {
            startAddress = startAddr;
            cp_descr = cp_desc;
            cref = cre;
            orderid = ordeid;
            GrinderNum = true;
            InitialPLCAddress();
        }

        /// <summary>
        /// 地址初始化
        /// </summary>
        public void InitialPLCAddress()
        {
            int[] I_Add = new int[8];
            string[] S_Add = new string[8];
            for (int i = 0; i <= 7; i++)
            {
                if (i <= 2)
                {
                    I_Add[i] = startAddress + i;
                    S_Add[i] = I_Add[i].ToString();
                }
                else
                {
                    I_Add[i] = startAddress + (i - 2) * 4;
                    S_Add[i] = I_Add[i].ToString();
                }
            };
            string S1 = "DB1,";
            string S_B = "B";
            string S_X = "X";
            ACF_Source_Gas_Ctrl_Address = S1 + S_X + S_Add[0] + ".0";
            Dust_Collector_Ctrl_Address = S1 + S_X + S_Add[0] + ".1";
            Select_Sensor_Ctrl_Address = S1 + S_X + S_Add[0] + ".2";
            TeleCylin_Ctrl_Address = S1 + S_X + S_Add[0] + ".3";
            Laser_Sensor_CH1_Set_Address = S1 + S_X + S_Add[0] + ".4";
            Laser_Sensor_CH2_Set_Address = S1 + S_X + S_Add[0] + ".5";
            Laser_Sensor_Zero_Set_Address = S1 + S_X + S_Add[0] + ".6";
            Laser_Sensor_Light_Off_Address = S1 + S_X + S_Add[0] + ".7";
            Polish_Tool_Select_Address = S1 + S_X + S_Add[1] + ".0";
            Head_Face_Polish_Tool_Ctrl_Address = S1 + S_X + S_Add[1] + ".1";
            FlapWheel_Servo_Warn_Reset_Address = S1 + S_X + S_Add[1] + ".2";
            FlapWheel_Start_Ctrl_Address = S1 + S_X + S_Add[1] + ".3";
            FlapWheel_RotaDerect_Ctrl_Address = S1 + S_X + S_Add[1] + ".4";
            Rail_Servo_Warn_Reset_Address = S1 + S_X + S_Add[1] + ".5";
            Rail_Position_Match_Ctrl_Address = S1 + S_X + S_Add[1] + ".6";
            Rail_Search_OriginPT_Ctrl_Address = S1 + S_X + S_Add[1] + ".7";
            Rail_Jog_Ctrl_Address = S1 + S_X + S_Add[2] + ".0";
            Rail_MotionDrect_Ctrl_Address = S1 + S_X + S_Add[2] + ".1";
            Rail_To_Desti_Ctrl_Address = S1 + S_X + S_Add[2] + ".2";
            Rail_To_Desti_Stop_Address = S1 + S_X + S_Add[2] + ".3";
            PC_Communication_OK_Address = S1 + S_X + S_Add[2] + ".4";
            Robot_Emergency_Stop_Address = S1 + S_X + S_Add[2] + ".5";
            System_Reset_SG_Address = S1 + S_X + S_Add[2] + ".6";
            HFPT_Speed_Set_Address = S1 + S_B + S_Add[3] + ",4";
            FlapWheel_Speed_Set_Address = S1 + S_B + S_Add[4] + ",4";
            Rail_Robot_Speed_Set_Address = S1 + S_B + S_Add[5] + ",4";
            Rail_Robot_Position_Set_Address = S1 + S_B + S_Add[6] + ",4";
            Rail_Motion_Mode_Selcet_Address = S1 + S_B + S_Add[7] + ",1";
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="direc">移动方向</param>
        /// <param name="distance">移动距离</param>
        /// <param name="relOrAbsPosition">定位方式</param>
        /// <param name="speed">移动速度</param>
        /// <param name="IsJog"></param>
        /// <returns></returns>
        public bool Move(bool direc, float speed, float distance, bool relOrAbsPosition,bool IsJog)
        {
            if (!local_PLC_ERR)
            {
                bool ret;
                speed = speed < maxMoveSpeed ? speed : maxMoveSpeed;
                ret = Write(Rail_MotionDrect_Ctrl_Address, direc);//设置方向
                ret &= Write(Rail_Motion_Mode_Selcet_Address, (byte)(relOrAbsPosition ? 1 : 0));//设置定位方式
                ret &= Write(Rail_Robot_Speed_Set_Address, speed);//设置运动速度
                if (IsJog)//点动
                {
                    ret &= Write(Rail_Jog_Ctrl_Address, true);//点动控制位1
                    ret &= Write(Rail_To_Desti_Ctrl_Address, false);//写定位控制位0
                }
                else//定位控制
                {
                    if (Rail_To_Desti_Done)//到位之后才接受指令
                    {
                        ret &= Write(Rail_Robot_Position_Set_Address, distance);//运动距离或位置
                        ret &= SetReset(Rail_To_Desti_Ctrl_Address, 500);//定位控制位1 500ms 后置0
                    }
                }
                return ret;
            }
            Console.WriteLine("导轨移动失败：Local_PLC_ERR故障,请排除故障并复位." + startAddress.ToString());
            return false;
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public bool StopMove()
        {
            //定位控制位false,停止进行中的定位运动指令
            return SetReset(Rail_To_Desti_Stop_Address, 500)
                && Write(Rail_To_Desti_Ctrl_Address, false);
        }

        /// <summary>
        /// 点动停止
        /// </summary>
        /// <returns></returns>
        public bool StopJogMove()
        {
            return Write(Rail_Jog_Ctrl_Address, false);
        }

        /// <summary>
        /// 导轨长动位复位
        /// </summary>
        /// <returns></returns>
        public bool ResetRailToDesti()
        {
            if (!local_PLC_ERR)
            {
                return Write(Rail_To_Desti_Ctrl_Address, false);
            }
            Console.WriteLine("导轨定位运动复位失败：Local_PLC_ERR故障,请先排除故障并复位." + startAddress.ToString());
            return false;    
        }

        /// <summary>
        /// 回参考点（标定原点使用） 持续一秒
        /// </summary> 
        public bool BackHome()
        {
            if (!local_PLC_ERR)
            {
                return SetReset(Rail_Search_OriginPT_Ctrl_Address);
            }
            Console.WriteLine("导轨回参考点失败：Local_PLC_ERR故障,请先排除故障并复位." + startAddress.ToString());
            return false;
        }

        /// <summary>
        /// 启动百叶轮
        /// </summary>
        public bool RotateFlapWheel(bool direction)
        {
            if (!local_PLC_ERR)
            {
                return Write(FlapWheel_Speed_Set_Address, wheelSpeed)
                           & Write(FlapWheel_RotaDerect_Ctrl_Address, direction)
                           & Write(FlapWheel_Start_Ctrl_Address, true);
            }
            Console.WriteLine("启动百叶轮失败：导轨Local_PLC_ERR故障,请先排除故障并复位." + startAddress.ToString());
            return false;
        }

        /// <summary>
        /// 启动端面打磨头(不需要写速度)
        /// </summary>
        public bool RotateHeadFacePolishTool( )
        {
            if (!local_PLC_ERR)
            {
                return Write(Head_Face_Polish_Tool_Ctrl_Address, true);
            }
            Console.WriteLine("启动端面打磨头失败：导轨Local_PLC_ERR故障,请先排除故障并复位." + startAddress.ToString());
            return false;
        }

        /// <summary>
        /// 设置集尘器
        /// </summary>
        /// <param name="open"></param>
        /// <returns></returns>
        public bool SetDustCollector(bool open)
        {
            return Write(Dust_Collector_Ctrl_Address, open);
        }

        /// <summary>
        /// 停止百叶轮 速度写0 启停控制位false
        /// </summary>
        private bool StopFlapWheel()
        {
            return Write(FlapWheel_Speed_Set_Address, 0.0f) && Write(FlapWheel_Start_Ctrl_Address, false);
        }

        /// <summary>
        /// 停止端面打磨头 速度写0 启停控制位false
        /// </summary>
        private bool StopHFPTWheel()
        {
            return Write(HFPT_Speed_Set_Address, 0.0f) && Write(Head_Face_Polish_Tool_Ctrl_Address, false);
        }

        /// <summary>
        /// 停止打磨头
        /// </summary>
        /// <returns></returns>
        public bool StopWheel()
        {
            if (GrinderNum)
            {
                return StopFlapWheel();
            }
            else
            {
                return StopHFPTWheel();
            }
        }

        /// <summary>
        /// 百叶轮伺服故障复位，持续置1 1S然后置0(端面打磨头不需要故障复位)
        /// </summary>
        public bool ResetWheelErr()
        {
            if (GrinderNum)
            {
                return SetReset(FlapWheel_Servo_Warn_Reset_Address, 500);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 导轨伺服故障复位,持续置1 1S然后置0
        /// </summary>
        public bool ResetRailErr()
        {
            return SetReset(Rail_Servo_Warn_Reset_Address, 500);
        }

        /// <summary>
        /// 导轨PLC复位
        /// </summary>
        public bool ResetRail()
        {
            return SetReset(System_Reset_SG_Address, 500);
        }

        public bool CloseLaser()
        {
            //熄灭激光传感器
            //return  write(Laser_Sensor_Light_Off_Address, true);
            return true;
        }

        public bool OpenLaser()
        {
            return Write(Laser_Sensor_Light_Off_Address, false);
        }

        /// <summary>
        /// 加载导轨位置
        /// </summary>
        /// <returns></returns>
        public bool LoadRailPositon()
        {
            if (!local_PLC_ERR)
            {
                if (SetReset(Rail_Position_Match_Ctrl_Address, 500))
                {
                    if (Rail_Robot_Position > 0)
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("导轨位置为0，请检查");
                    }
                }
            }
            //Console.WriteLine("加载导轨位置失败：导轨Local_PLC_ERR故障,请先排除故障并复位." + startAddress.ToString(), MessageType.Error);
            return false;
        }

        /// <summary>
        /// 设置导轨最大行程
        /// </summary>
        public void SetMaxDis(float maxDis)
        {
            robotMaxPos = maxDis;
        }

        public bool RailEstop()
        {
            if (Write(Robot_Emergency_Stop_Address, true))
            {
                Console.WriteLine("导轨已急停,重新启动前请先系统复位." + startAddress.ToString());
                return true;
            }
            Console.WriteLine("导轨急停失败,请按急停按钮." + startAddress.ToString());
            return false;
        }

        /// <summary>
        /// 复位指定信号
        /// </summary>
        /// <param name="address"></param>
        private bool SetReset(string address)
        {
            return SetReset(address, 500);
        }

        /// <summary>
        /// 将信号先置1time时间再置0
        /// </summary>
        /// <param name="address"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool SetReset(string address, long time)
        {
            bool ret1=false, ret2=false;
            ret1 = Write(address, true);
            Thread.Sleep((int)time);
            ret2 = Write(address, false);//置0 间隔时间长一点
            if (ret1&&ret2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 写bool信号
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool Write(string address, bool value)
        {
            if (!local_PLC_ERR)
            {
                return S7_COM_FUN.Write_Req(cp_descr, cref, orderid, address, value) == 0;
            }
            Console.WriteLine("导轨Local_PLC_ERR故障,请排除故障并复位." + startAddress.ToString());
            return false;
        }

        /// <summary>
        /// 写byte信号
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool Write(string address, byte value)
        {
            if (!local_PLC_ERR)
            {
                return S7_COM_FUN.Write_Req(cp_descr, cref, orderid, address, value) == 0;
            }
            Console.WriteLine("导轨Local_PLC_ERR故障,请排除故障并复位." + startAddress.ToString());
            return false;
        }

        /// <summary>
        /// 写float信号
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool Write(string address, float value)
        {
            if (!local_PLC_ERR)
            {
                return S7_COM_FUN.Write_Req(cp_descr, cref, orderid, address, value) == 0;
            }
            Console.WriteLine("导轨Local_PLC_ERR故障,请排除故障并复位." + startAddress.ToString());
            return false;
        }

        /// <summary>
        /// 设置反馈模式()
        /// </summary>
        /// <param name="torqueOrDis">距离或转矩反馈</param>
        public void SetFBMode(bool torqueOrDis)
        {
            torqueOrDisFB = torqueOrDis;
        }

        /// <summary>
        /// 设置运动范围
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public void SetRailRange(float min, float max)
        {
            robotMaxPos = max;
            robotMinPos = min;
        }

        /// <summary>
        /// 设置运动速度
        /// </summary>
        /// <param name="speed">导轨移动速度</param>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed<maxMoveSpeed?speed:maxMoveSpeed;
        }

        /// <summary>
        /// 设置最大转矩
        /// </summary>
        /// <param name="maxT"></param>
        public void SetMaxTorque(float maxT)
        {
            maxTorque = maxT;
        }

        /// <summary>
        /// 设置打磨期望转矩
        /// </summary>
        /// <param name="torque"></param>
        public void SetGrindTorque(float torque)
        {
            grindingTorque = torque<maxTorque?torque:maxTorque;
        }

        /// <summary>
        /// 设置转矩容差
        /// </summary>
        /// <param name="toleranceT"></param>
        public void SetTorqueTol(float toleranceT)
        {
            torqueTolerance = toleranceT;
        }

        /// <summary>
        /// 设置百叶轮转速
        /// </summary>
        /// <param name="speed">转速</param>
        public void SetWheelSpeed(float speed)
        {
            speed = Math.Abs(speed);
            wheelSpeed = speed < maxWheelSpeed ? speed : maxWheelSpeed;
        }

        /// <summary>
        /// 设置砂轮最大转速
        /// </summary>
        /// <param name="speed">最大转速</param>
        public void SetMaxWheelSpeed(float speed)
        {
            maxWheelSpeed = Math.Abs(speed);
        }

        /// <summary>
        /// 选择打磨工具(0-端面 1-百叶轮 默认是百叶轮)
        /// </summary>
        /// <param name="HFPTOrFlap"></param>
        /// <returns></returns>
        public bool SetGrinder(bool HFPTOrFlap)
        {
            if (!local_PLC_ERR)
            {
                if (Write(Polish_Tool_Select_Address, HFPTOrFlap))
                {
                    GrinderNum = HFPTOrFlap;
                    return true;
                }
            }
            Console.WriteLine("选择打磨工具失败，导轨Local_PLC_ERR故障,请排除故障并复位." + startAddress.ToString());
            return false;
        }
    }
}
