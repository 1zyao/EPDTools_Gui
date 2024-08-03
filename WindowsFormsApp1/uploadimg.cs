using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{

    public partial class uploadimg : Form
    {
        public enum Respond
        {
            RES_SUCCESS,
            RES_ERROR_FLAG,
            RES_TIME_OUT
        }

        public uploadimg()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//干掉检测 不再检测跨线程
        }

        private const int DEFAULT_TIME_OUT = 10000;

        public static SerialPort OpenComm(string comName)
        {
            SerialPort serialPort = new SerialPort(comName, 115200, Parity.None, 8, StopBits.One);

            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening {comName}: {ex.Message}");
                return null;
            }

            serialPort.ReadTimeout = 50;
            serialPort.WriteTimeout = 50;

            return serialPort;
        }

        public static int SendData(SerialPort serialPort, byte[] data)
        {
            try
            {
                serialPort.BaseStream.WriteAsync(data, 0, data.Length);
                return data.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to serial port: {ex.Message}");
                return 0;
            }
        }

        public static int ReceiveData(SerialPort serialPort, byte[] buffer, int length)
        {
            try
            {
                return serialPort.Read(buffer, 0, length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from serial port: {ex.Message}");
                return 0;
            }
        }

        public static void CloseComm(SerialPort serialPort)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        public void SendImgDataasync(SerialPort serialPort, ImgData imgData)
        {

            Thread thread = new Thread(() => SendImgDatavoid(serialPort, imgData));
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }

        public void SendImgDatavoid(SerialPort serialPort, ImgData imgData)
        {
            serialPort.Open();

            // 打开串口的函数运行后可能会导致短暂的断电，此处等待设备被重新上电
            Thread.Sleep(1000);

            ClearCommCache(serialPort);

            SendImgData(serialPort, imgData);

            serialPort.Close();//关闭串口
        }

        public bool SendImgData(SerialPort serialPort, ImgData imgData)
        {
            Console.WriteLine("Send Data: Begin...");
            label1.Text = "检查设备中";

            int sentLen = 0;

            SendData(serialPort, Encoding.ASCII.GetBytes("Beg\n\r"));

            if (CheckRespond(serialPort, "OK") != Respond.RES_SUCCESS)
            {
                Console.WriteLine("Send Data: Begin Failed");
                label1.Text = "失败";
                return false;
            }
            label1.Text = "开始发送";

            Console.WriteLine("Send Data: Begin transmit Black/White Data");

            while (sentLen < imgData.DataLen)
            {
                ushort chkSum = 0;
                for (int i = 0; i < 16; i++)
                    chkSum += imgData.BWData[sentLen + i];

                if (SendData(serialPort, imgData.BWData.AsSpan(sentLen, 16).ToArray()) == 0)
                {
                    return false;
                }

                sentLen += 16;
                Console.WriteLine($"Transmit: BW: {sentLen} / {imgData.DataLen} Bytes | {sentLen * 1.0 / imgData.DataLen * 100:0.00} %");
                label1.Text = "正在发送：黑白："+sentLen+" / "+imgData.DataLen+" Bytes";
                progressBar1.Value = (int)(sentLen * 1.0 / imgData.DataLen * 100);
                label2.Text = (sentLen * 1.0 / imgData.DataLen * 100)+ " %";

                var res = CheckRespond(serialPort, chkSum.ToString());
                if (res == Respond.RES_ERROR_FLAG || res == Respond.RES_TIME_OUT)
                {
                    return false;
                }
            }

            Console.WriteLine("Send Data: Begin transmit Red/White Data");
            sentLen = 0;

            while (sentLen < imgData.DataLen)
            {
                ushort chkSum = 0;
                for (int i = 0; i < 16; i++)
                    chkSum += imgData.RWData[sentLen + i];

                if (SendData(serialPort, imgData.RWData.AsSpan(sentLen, 16).ToArray()) == 0)
                {
                    return false;
                }

                sentLen += 16;
                Console.WriteLine($"Transmit: RW: {sentLen} / {imgData.DataLen} Bytes | {sentLen * 1.0 / imgData.DataLen * 100:0.00} %");
                label1.Text = "正在发送：红白：" + sentLen + " / " + imgData.DataLen + " Bytes";
                progressBar1.Value = (int)(sentLen * 1.0 / imgData.DataLen * 100);
                label2.Text = (sentLen * 1.0 / imgData.DataLen * 100) + " %";

                var res = CheckRespond(serialPort, chkSum.ToString());
                if (res == Respond.RES_ERROR_FLAG || res == Respond.RES_TIME_OUT)
                {
                    return false;
                }
            }

            Console.WriteLine("Send Data: Finished.");
            label1.Text = "发送完毕，屏幕正在刷新";
            return true;
        }

        public static ulong GetCurTimeStp()
        {
            return (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        public static Respond CheckRespond(SerialPort serialPort, string resFlag, int timeOutMs = DEFAULT_TIME_OUT)
        {
            DateTime startTime = DateTime.Now;
            byte[] buffer = new byte[10];
            int totalBytesRead = 0;

            while ((DateTime.Now - startTime).TotalMilliseconds < timeOutMs)
            {
                int availableBytes = serialPort.BytesToRead;
                if (availableBytes > 0)
                {
                    int bytesToRead = Math.Min(buffer.Length - totalBytesRead, availableBytes);
                    int bytesRead = serialPort.Read(buffer, totalBytesRead, bytesToRead);
                    totalBytesRead += bytesRead;

                    string recvStr = Encoding.ASCII.GetString(buffer, 0, totalBytesRead).TrimEnd('\0');
                    if (recvStr.Contains(resFlag))
                    {
                        return Respond.RES_SUCCESS;
                    }

                    if (totalBytesRead >= buffer.Length)
                    {
                        break;
                    }
                }
                Thread.Sleep(50);
            }

            Console.WriteLine("Transmit: TIME OUT");
            return Respond.RES_TIME_OUT;
        }



        public static void ClearCommCache(SerialPort serialPort)
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }
    }
}
