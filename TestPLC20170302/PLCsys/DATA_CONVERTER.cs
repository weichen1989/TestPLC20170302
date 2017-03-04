using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

namespace BIMI.RGWTB
{
    /// <summary>
    /// 版权所有: 版权所有(C) 2016，华中科技大学无锡研究院叶片智能制造研究所
    /// 内容摘要: 本类功能是PC和PLC之间的数据转换
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
    class DATA_CONVERTER
    {
        /*由于PC与PLC中的字和双字的字节位置不同，因此需要相互转换，字的字节转换：byte[0]<-->byte[1]*/
        /*双字的字节转换：byte[0],byte[1],byte[2],byte[3]<-->byte[3],byte[2],byte[1],byte[0]*/
        public static int Byte_ExRank(ref Byte[] DATA, UInt16 i)
        {
            Int32 a = DATA.Length;
            if (i != 2 && i != 4)
            {
                return -1;
            }
            if (i == 2)
            {
                if (a % 2 == 0 && a != 0)
                {
                    for (int b = 0; b <= (a - 2); b = b + 2)
                    {
                        Byte C = DATA[b];
                        DATA[b] = DATA[b + 1];
                        DATA[b + 1] = C;
                    }
                    return 0;
                }
                else
                {
                    return -1;
                }

            }
            if (i == 4)
            {
                if (a % 2 == 0 && a != 0)
                {
                    for (int d = 0; d <= (a - 4); d = d + 4)
                    {
                        Byte E = DATA[d];
                        DATA[d] = DATA[d + 3];
                        DATA[d + 3] = E;
                        E = DATA[d + 1];
                        DATA[d + 1] = DATA[d + 2];
                        DATA[d + 2] = E;
                    }
                    return 0;
                }
                else
                {
                    return -1;
                }

            }
            return 0;
        }
        /*将4字节转换为浮点类型数据*/
        /*上位机从PLC读取浮点类型数值时，读取结果存放于4个字节中，交换了高低字节后，再转换为浮点类型才能得到与PLC相同的数据结果*/
        public static int Byte_To_Float(Byte[] DATA1, ref float[] DATA2)
        {
            Int32 D1_len = DATA1.Length;
            Int32 D2_len = DATA2.Length;
            if (D1_len == (4 * D2_len))
            {
                for (int i = 0; i <= (D1_len - 4); i = i + 4)
                {
                    DATA2[i / 4] = BitConverter.ToSingle(DATA1, i);
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*对Byte_To_Float函数进行重载，使其能够将长度为4的字节数组直接转化为一个浮点数*/
        public static int Byte_To_Float(Byte[] DATA1, ref float DATA2)
        {
            Int32 D1_len = DATA1.Length;
            if (D1_len == 4)
            {
                DATA2 = BitConverter.ToSingle(DATA1, 0);
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*将浮点类型转换为4个字节*/
        /*上位机对PLC进行浮点类型写操作时，需要将浮点类型转换为4个字节，交换高低字节，再写入PLC才能得到预期结果*/
        public static int Float_To_Byte(float[] DATA1, ref Byte[] DATA2)
        {
            Int32 D1_len = DATA1.Length;
            Int32 D2_len = DATA2.Length;
            Byte[] Temp = new Byte[4];
            if (D2_len == (4 * D1_len))
            {
                for (int i = 0; i <= (D1_len - 1); i++)
                {
                    Temp = BitConverter.GetBytes(DATA1[i]);
                    DATA2[4 * i] = Temp[0];
                    DATA2[4 * i + 1] = Temp[1];
                    DATA2[4 * i + 2] = Temp[2];
                    DATA2[4 * i + 3] = Temp[3];
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*对Float_To_Byte函数进行重载，将一个浮点数转化为长度为4的字节数组*/
        public static int Float_To_Byte(float DATA1, ref Byte[] DATA2)
        {
            Int32 D2_len = DATA2.Length;
            if (D2_len == 4)
            {
                DATA2 = BitConverter.GetBytes(DATA1);
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*将字节型数组每2字节转换为16位有符号整型，得到整型数组*/
        public static int Byte_To_Int16(Byte[] DATA1, ref Int16[] DATA2)
        {
            Int32 D1_len = DATA1.Length;
            Int32 D2_len = DATA2.Length;
            if (D1_len == (2 * D2_len))
            {
                for (int i = 0; i <= (D1_len - 2); i = i + 2)
                {
                    DATA2[i / 2] = BitConverter.ToInt16(DATA1, i);
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*对Byte_To_Int16进行重载，将2字节转换为16位有符号整型*/
        public static int Byte_To_Int16(Byte[] DATA1, ref Int16 DATA2)
        {
            Int32 D1_len = DATA1.Length;
            if (D1_len == 2)
            {
                DATA2 = BitConverter.ToInt16(DATA1, 0);
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*将16位有符号整型数组转换为字节数组*/
        public static int Int16_To_Byte(Int16[] DATA1, ref Byte[] DATA2)
        {
            Int32 D1_len = DATA1.Length;
            Int32 D2_len = DATA2.Length;
            Byte[] Temp = new Byte[2];
            if (D2_len == (2 * D1_len))
            {
                for (int i = 0; i <= D1_len; i++)
                {
                    Temp = BitConverter.GetBytes(DATA1[i]);
                    DATA2[2 * i] = Temp[0];
                    DATA2[2 * i + 1] = Temp[1];
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*对Int16_To_Byte进行重载，使得其能将一个16位有符号整型数转化为两个字节*/
        public static int Int16_To_Byte(Int16 DATA1, ref Byte[] DATA2)
        {
            Int32 D2_len = DATA2.Length;
            if (D2_len == 2)
            {
                DATA2 = BitConverter.GetBytes(DATA1);
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*将字节数组每4个字节转换为32位有符号整型，得到一个32位有符号的整型数组*/
        public static int Byte_To_Int32(Byte[] DATA1, ref Int32[] DATA2)
        {
            Int32 D1_len = DATA1.Length;
            Int32 D2_len = DATA2.Length;
            if (D1_len == (4 * D2_len))
            {
                for (int i = 0; i <= D1_len - 4; i = i + 4)
                {
                    DATA2[i / 4] = BitConverter.ToInt32(DATA1, i);
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*对Byte_To_Int32进行重载，将4个字节转化为一个32位有符号的整型数*/
        public static int Byte_To_Int32(Byte[] DATA1, ref Int32 DATA2)
        {
            Int32 D1_len = DATA1.Length;
            if (D1_len == 4)
            {
                DATA2 = BitConverter.ToInt32(DATA1, 0);
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*将32位有符号整型数组转换为字节数组*/
        public static int Int32_To_Byte(Int32[] DATA1, ref Byte[] DATA2)
        {
            Int32 D1_len = DATA1.Length;
            Int32 D2_len = DATA2.Length;
            Byte[] Temp = new Byte[4];
            if (D2_len == (4 * D1_len))
            {
                for (int i = 0; i <= (D1_len - 1); i++)
                {
                    Temp = BitConverter.GetBytes(DATA1[i]);
                    DATA2[4 * i] = Temp[0];
                    DATA2[4 * i + 1] = Temp[1];
                    DATA2[4 * i + 2] = Temp[2];
                    DATA2[4 * i + 3] = Temp[3];
                }
                return 0;
            }
            else
            {
                return -1;
            }
        }
        /*对Int32_To_Byte进行重载，能够将一个32位的有符号整型数，转换为4个字节*/
        public static int Int32_To_Byte(Int32 DATA1, ref Byte[] DATA2)
        {
            Int32 D2_len = DATA2.Length;
            if (D2_len == 4)
            {
                DATA2 = BitConverter.GetBytes(DATA1);
                return 0;
            }
            else
            {
                return -1;
            }
        }
       
        /// <summary>
        /// 将读取的数据存至各PLC（导轨PLC包含于机器人类）
        /// </summary>
        /// <param name="Read_Var"></param>
        /// <param name="comPLC"></param>
        /// <param name="fixurePLC"></param>
        /// <param name="railPLC1"></param>
        /// <param name="railPLC2"></param>
        public static void ByteToPLCsys(Byte[] Read_Var, ComPLC comPLC, FixturePLC fixurePLC, RailPLC railPLC1, RailPLC railPLC2)
        {
            //将字节转化为bool值，存入到对应的plc中

            //读取Rail1 PLC中的值
            if (railPLC1 != null)
            {
                BitArray TEMP1 = new BitArray(new Byte[1] { Read_Var[0] });
                BitArray TEMP2 = new BitArray(new Byte[1] { Read_Var[1] });
                Byte[] BTEMP1 = new Byte[4];
                float FTEMP1 = 0;
                railPLC1.Touch_Sensor_SG = TEMP1[0];
                railPLC1.TeleCylin_Max_On = TEMP1[1];
                railPLC1.TeleCylin_Min_On = TEMP1[2];
                railPLC1.Laser_Sensor_CH1_On = TEMP1[3];
                railPLC1.Laser_Sensor_CH2_On = TEMP1[4];
                railPLC1.FlapWheel_RotaDrect = TEMP1[5];
                railPLC1.FlapWheel_Servo_Warn_SG = TEMP1[6];
                railPLC1.Rail_Servo_Warn_SG = TEMP1[7];
                railPLC1.Rail_MotionDrect = TEMP2[0];
                railPLC1.Rail_Position_Match_Done = TEMP2[1];
                railPLC1.Rail_Search_OriginPT_Done = TEMP2[2];
                railPLC1.Rail_To_Desti_Done = TEMP2[3];
                railPLC1.Device_Elec_Off = TEMP2[4];
                railPLC1.Local_Communication_OK = TEMP2[5];
                railPLC1.Local_PLC_ERR = TEMP2[6];
                railPLC1.Local_Device_ERR = TEMP2[7];
                //读取错误代码
                railPLC1.FlapWheel_Error_Code = Read_Var[7];
                railPLC1.Rail_Read_Coder_ErrCode = Read_Var[20];
                railPLC1.Rail_Position_Match_ErrCode = Read_Var[21];
                railPLC1.Rail_Search_OriginPT_ErrCode = Read_Var[22];
                railPLC1.Rail_Jog_ErrCode = Read_Var[23];
                railPLC1.Rail_To_Desti_ErrCode = Read_Var[24];
                //读取实数

                //激光器1
                for (int i = 0; i <= 3; i++)
                {
                    BTEMP1[i] = Read_Var[3 + i];
                }
                Byte_ExRank(ref BTEMP1, 4);
                Byte_To_Float(BTEMP1, ref FTEMP1);
                railPLC1.Laser_Sensor_Value = FTEMP1;

                //激光器2
                for (int i = 0; i <= 3; i++)
                {
                    BTEMP1[i] = Read_Var[25 + i];
                }
                Byte_ExRank(ref BTEMP1, 4);
                Byte_To_Float(BTEMP1, ref FTEMP1);
                railPLC1.Laser_Sensor2_Value = FTEMP1;

                //百叶轮转速（或转矩）
                for (int i = 0; i <= 3; i++)
                {
                    BTEMP1[i] = Read_Var[8 + i];
                }
                Byte_ExRank(ref BTEMP1, 4);
                Byte_To_Float(BTEMP1, ref FTEMP1);
                railPLC1.FlapWheel_SpeedOrTorque = FTEMP1;

                for (int i = 0; i <= 3; i++)
                {
                    BTEMP1[i] = Read_Var[12 + i];
                }
                Byte_ExRank(ref BTEMP1, 4);
                Byte_To_Float(BTEMP1, ref FTEMP1);
                railPLC1.Rail_Robot_Position = FTEMP1;

                for (int i = 0; i <= 3; i++)
                {
                    BTEMP1[i] = Read_Var[16 + i];
                }
                Byte_ExRank(ref BTEMP1, 4);
                Byte_To_Float(BTEMP1, ref FTEMP1);
                railPLC1.Rail_Robot_Speed = FTEMP1;
            }

            //读取Rail2 PLC中的值
            if (railPLC2 != null)
            {
                BitArray TEMP1 = new BitArray(new Byte[1] { Read_Var[0] });
                BitArray TEMP2 = new BitArray(new Byte[1] { Read_Var[1] });
                Byte[] BTEMP1 = new Byte[4];
                float FTEMP1 = 0;
                //读取bool值
                BitArray TEMP3 = new BitArray(new Byte[1] { Read_Var[50] });
                BitArray TEMP4 = new BitArray(new Byte[1] { Read_Var[51] });
                railPLC2.Touch_Sensor_SG = TEMP3[0];
                railPLC2.TeleCylin_Max_On = TEMP3[1];
                railPLC2.TeleCylin_Min_On = TEMP3[2];
                railPLC2.Laser_Sensor_CH1_On = TEMP3[3];
                railPLC2.Laser_Sensor_CH2_On = TEMP3[4];
                railPLC2.FlapWheel_RotaDrect = TEMP3[5];
                railPLC2.FlapWheel_Servo_Warn_SG = TEMP3[6];
                railPLC2.Rail_Servo_Warn_SG = TEMP3[7];
                railPLC2.Rail_MotionDrect = TEMP4[0];
                railPLC2.Rail_Position_Match_Done = TEMP4[1];
                railPLC2.Rail_Search_OriginPT_Done = TEMP4[2];
                railPLC2.Rail_To_Desti_Done = TEMP4[3];
                railPLC2.Device_Elec_Off = TEMP4[4];
                railPLC2.Local_Communication_OK = TEMP4[5];
                railPLC2.Local_PLC_ERR = TEMP4[6];
                railPLC2.Local_Device_ERR = TEMP4[7];
                //读取错误代码
                railPLC2.FlapWheel_Error_Code = Read_Var[57];
                railPLC2.Rail_Read_Coder_ErrCode = Read_Var[70];
                railPLC2.Rail_Position_Match_ErrCode = Read_Var[71];
                railPLC2.Rail_Search_OriginPT_ErrCode = Read_Var[72];
                railPLC2.Rail_Jog_ErrCode = Read_Var[73];
                railPLC2.Rail_To_Desti_ErrCode = Read_Var[74];
                //读取实数
                //激光器1
                Byte[] BTEMP2 = new Byte[4];
                float FTEMP2 = 0;
                for (int i = 0; i <= 3; i++)
                {
                    BTEMP2[i] = Read_Var[53 + i];
                }
                Byte_ExRank(ref BTEMP2, 4);
                Byte_To_Float(BTEMP2, ref FTEMP2);
                railPLC2.Laser_Sensor_Value = FTEMP2;

                //激光器2
                for (int i = 0; i <= 3; i++)
                {
                    BTEMP1[i] = Read_Var[75 + i];
                }
                Byte_ExRank(ref BTEMP1, 4);
                Byte_To_Float(BTEMP1, ref FTEMP1);
                railPLC2.Laser_Sensor2_Value = FTEMP1;

                for (int i = 0; i <= 3; i++)
                {
                    BTEMP2[i] = Read_Var[58 + i];
                }
                Byte_ExRank(ref BTEMP2, 4);
                Byte_To_Float(BTEMP2, ref FTEMP2);
                railPLC2.FlapWheel_SpeedOrTorque = FTEMP2;

                for (int i = 0; i <= 3; i++)
                {
                    BTEMP2[i] = Read_Var[62 + i];
                }
                Byte_ExRank(ref BTEMP2, 4);
                Byte_To_Float(BTEMP2, ref FTEMP2);
                railPLC2.Rail_Robot_Position = FTEMP2;

                for (int i = 0; i <= 3; i++)
                {
                    BTEMP2[i] = Read_Var[66 + i];
                }
                Byte_ExRank(ref BTEMP2, 4);
                Byte_To_Float(BTEMP2, ref FTEMP2);
                railPLC2.Rail_Robot_Speed = FTEMP2;
            }

            //读取叶片夹持小车PLC中的值
            if (fixurePLC != null)
            {
                BitArray TEMP1 = new BitArray(new Byte[1] { Read_Var[0] });
                BitArray TEMP2 = new BitArray(new Byte[1] { Read_Var[1] });
                Byte[] BTEMP1 = new Byte[4];
                float FTEMP1 = 0;
                //读取bool值
                BitArray TEMP5 = new BitArray(new Byte[1] { Read_Var[100] });
                BitArray TEMP6 = new BitArray(new Byte[1] { Read_Var[101] });
                fixurePLC.Fixture_Local_Reset_SG = TEMP5[0];
                fixurePLC.Fixture_Jog_Rota_CW_SG = TEMP5[1];
                fixurePLC.Fixture_Jog_Rota_CWW_SG = TEMP5[2];
                fixurePLC.Fixture_LRota_CW_SG = TEMP5[3];
                fixurePLC.Fixture_LRota_CWW_SG = TEMP5[4];
                fixurePLC.Fixture_Rota_CW_SG = TEMP5[5];
                fixurePLC.Fixture_Rota_CWW_SG = TEMP5[6];
                fixurePLC.Fixture1_Rotation_Done = TEMP5[7];
                fixurePLC.Fixture2_Rotation_Done = TEMP6[0];
                fixurePLC.Fixture_Rotation_Done = TEMP6[1];
                fixurePLC.Fixture_Current_Mode = TEMP6[2];
                fixurePLC.Fixture_Stop_SG = TEMP6[3];
                fixurePLC.Local_Communication_OK = TEMP6[4];
                fixurePLC.Local_PLC_ERR = TEMP6[5];
                fixurePLC.Fixture_ERR_SG = TEMP6[6];
                //读角度传感器
                for (int i = 0; i <= 3; i++)
                {
                    BTEMP1[i] = Read_Var[102 + i];
                }
                Byte_ExRank(ref BTEMP1, 4);
                Byte_To_Float(BTEMP1, ref FTEMP1);
                fixurePLC.Fixture_XAngle = FTEMP1;

                for (int i = 0; i <= 3; i++)
                {
                    BTEMP1[i] = Read_Var[106 + i];
                }
                Byte_ExRank(ref BTEMP1, 4);
                Byte_To_Float(BTEMP1, ref FTEMP1);
                fixurePLC.Fixture_YAngle = FTEMP1;
            }
            //读取通信PLC中的数值           
            if (comPLC != null)
            {
                BitArray TEMP7 = new BitArray(new Byte[1] { Read_Var[110] });
                comPLC.PLC_Communication_OK = TEMP7[0];
                comPLC.PLC_ERR_SG = TEMP7[1];
                comPLC.System_ERR_SG = TEMP7[2];
                comPLC.Enmergency_Stop_SG = TEMP7[3];
            }
        }
    } 
}
