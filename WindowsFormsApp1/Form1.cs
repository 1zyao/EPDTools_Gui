using Dithering;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//串口参数结构体
struct COMPORT_ATTRIBUTE
{
    public int bandrate;
    public int data_bit;
    public Parity parity_check_bit;
    public StopBits stop_bit;
    public string comport_number;
};

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Dithering.Dithering Dithering = new Dithering.Dithering();
        ManagementEventWatcher watcher;

        // 声明一个委托
        delegate void InitializePortsDelegate();

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            // 设备插入时执行 InitializePorts 方法
            InitializePorts();
        }

        //初始化串口
        public void InitializeSerialSet()
        {

            // 初始化扫描串口
            InitializePorts();
            // 初始化波特率
            serialPort1.BaudRate = 115200;
            // 初始化数据位
            serialPort1.DataBits = 8;
            // 初始化停止位
            serialPort1.StopBits = (StopBits)1;

            serialPort1.Parity = Parity.None;
            // 无校验位  

        }

        /// <summary>
        /// 扫描串口
        /// </summary>
        public void InitializePorts()
        {
            if (InvokeRequired)
            {
                // 如果当前线程不是 UI 线程，则通过 Invoke 方法在 UI 线程上执行 InitializePorts 方法
                Invoke(new InitializePortsDelegate(InitializePorts));
            }
            else
            {
                comport.Items.Clear();//清除数据
                string[] ports = System.IO.Ports.SerialPort.GetPortNames();//获取电脑上可用串口号
                comport.Items.AddRange(ports);//给comboBox1添加数据
                comport.SelectedIndex = comport.Items.Count > 0 ? 0 : -1;//如果里面有数据,显示第0个
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeSerialSet();

            // 创建设备变化事件监听器
            watcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Query = query;

            // 启动监听器
            watcher.Start();
        }

        private void comtext_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭监听器
            if (watcher != null)
            {
                watcher.Stop();
                watcher.Dispose();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
                pictureBox1.Refresh();
                pictureBox2.Image.Dispose();
                pictureBox2.Image = null;
                pictureBox2.Refresh();
            }
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "图片|*.gif;*.jpg;*.jpeg;*.bmp;*.jfif;*.png;";//限制只能选择这几种图片格式

            if (openfile.ShowDialog() == DialogResult.OK && (openfile.FileName != ""))
            {
                Image image = Image.FromFile(openfile.FileName);
                if (image.Width.Equals(122) && image.Height.Equals(250))
                {
                }
                else if (image.Width.Equals(250) && image.Height.Equals(122))
                {
                    image.RotateFlip(RotateFlipType.Rotate90FlipX);
                }
                else
                {
                    DialogResult result = MessageBox.Show("您的图片分辨率不正确，请选择缩放或拉伸，是为缩放，否为拉伸", "选择操作", MessageBoxButtons.YesNoCancel);

                    if (image.Width > image.Height)
                    {
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                    }

                    if (result == DialogResult.Yes)
                    {
                        // 定义目标 Bitmap 的大小
                        int targetWidth = 122;
                        int targetHeight = 250;

                        // 计算绘制图像在目标 Bitmap 中的位置和大小，保持原始比例不变
                        int width, height, x, y;

                        if ((float)image.Width / image.Height > (float)targetWidth / targetHeight)
                        {
                            width = targetWidth;
                            height = (int)((float)image.Height * targetWidth / image.Width);
                            x = 0;
                            y = (targetHeight - height) / 2;
                        }
                        else
                        {
                            width = (int)((float)image.Width * targetHeight / image.Height);
                            height = targetHeight;
                            x = (targetWidth - width) / 2;
                            y = 0;
                        }

                        // 创建目标 Bitmap 对象
                        Bitmap resizedImage = new Bitmap(targetWidth, targetHeight);

                        // 创建 Graphics 对象，并在目标 Bitmap 上绘制原始图像
                        using (Graphics g = Graphics.FromImage(resizedImage))
                        {
                            g.Clear(Color.White); // 使用白色填充整个图像区域
                            g.DrawImage(image, x, y, width, height);
                        }

                        image = resizedImage;
                    }
                    else if (result == DialogResult.No)
                    {
                        // 创建一个新的Bitmap对象，用于存储拉伸后的图像
                        Bitmap stretchedImage = new Bitmap(122, 250);
                        // 创建一个Graphics对象，用于绘制图像
                        using (Graphics g = Graphics.FromImage(stretchedImage))
                        {
                            // 将源图像拉伸绘制到自身的指定位置上，实现图像的拉伸
                            g.DrawImage(image, 0, 0, 122, 250);
                        }
                        image = stretchedImage;
                    }
                    else
                    {
                        MessageBox.Show("你取消了选择，请自行修改图片分辨率到122*250");
                        return;
                    }
                }
                Bitmap bitmap = new Bitmap(image);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.Image = bitmap;

                pictureBox2.Image = Dithering.ConvertToBitmap(new Bitmap(pictureBox1.Image));
                trackBar1.MouseUp +=
                    new MouseEventHandler(TrackBar1_MouseUp);
                trackBar1.ValueChanged +=
                    new EventHandler(TrackBar1_ValueChanged);
            }

        }
        private void TrackBar1_MouseUp(object sender, System.EventArgs e)
        {
            pictureBox2.Image = Dithering.ConvertToBitmap(new Bitmap(pictureBox1.Image));
        }
        private void TrackBar1_ValueChanged(object sender, System.EventArgs e)
        {
            label4.Text = "当前扩散值："+trackBar1.Value.ToString();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                uploadimg uploadimg = new uploadimg();

                serialPort1.PortName = comport.SelectedItem.ToString();

                ImgData imgData = Dithering.ConvertToImgData(new Bitmap(pictureBox1.Image));
                uploadimg.Show();
                uploadimg.SendImgDataasync(serialPort1, imgData);
            }
            else
            {
                MessageBox.Show("没有加载任何图片！");
            }
        }

        public void updateimg(Bitmap img)
        {
            uploadimg uploadimg = new uploadimg();

            serialPort1.PortName = comport.SelectedItem.ToString();

            ImgData imgData = Dithering.ConvertToImgData(img);
            uploadimg.Show();
            uploadimg.SendImgDataasync(serialPort1, imgData);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }
    }

    public static class ProcessExtensions
    {
        public static Task<bool> WaitForExitAsync(this Process process)
        {
            var tcs = new TaskCompletionSource<bool>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(true);
            if (process.HasExited) tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}
