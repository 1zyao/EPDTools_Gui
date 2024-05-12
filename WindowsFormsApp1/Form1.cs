using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        ManagementEventWatcher watcher;

        // 声明一个委托
        delegate void InitializePortsDelegate();

        // 串口参数
        private COMPORT_ATTRIBUTE uart_port;

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
            uart_port.bandrate = 115200;
            // 初始化数据位
            uart_port.data_bit = 8;
            // 初始化停止位
            uart_port.stop_bit = (StopBits)1;
            // 初始化校验位
            uart_port.parity_check_bit = 0;//Parity.None
            if (uart_port.parity_check_bit == (Parity)1)//  Parity.Odd
            {
            }
            else if (uart_port.parity_check_bit == (Parity)2) //Parity.Even
            {
            }
            else
            {
            }


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

                string[] port_names = SerialPort.GetPortNames();
                string last_name = "";

                comport.Items.Clear();//清除数据
                if (port_names == null)
                {
                    MessageBox.Show("本机没有串口！", "Error");
                    return;
                }
                foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                {
                    //获取有多少个COM口就添加进COMBOX项目列表  
                    comport.Items.Add(s);
                    last_name = s;//保存最新的一个
                }
                comport.Text = last_name;//显示最新的一个串口
                uart_port.comport_number = last_name;//赋值变量

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

            // 检查文件
            check();
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
                string file = "temp.png";
                image.Save(file);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.ImageLocation = file;
                pictureBox1.Tag = file;

                pictureBox2.LoadCompleted += PictureBox2_LoadCompleted;
                string outfile = "dith.png";
                loadimg(file, outfile, "dith", pictureBox2);

                pictureBox3.LoadCompleted += PictureBox3_LoadCompleted;
                outfile = "mod.png";
                loadimg(file, outfile, "mod", pictureBox3);
            }

        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        static void PictureBox2_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // 图片加载完成后执行的操作
            if (e.Error != null)
            {
                // 加载出错
                Console.WriteLine("Error loading image: " + e.Error.Message);
            }
            else
            {
                // 加载成功
                Console.WriteLine("Image loaded successfully.");
                // 在此处执行你需要的操作
                File.Delete("dith.png");
            }
        }
        static void PictureBox3_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // 图片加载完成后执行的操作
            if (e.Error != null)
            {
                // 加载出错
                Console.WriteLine("Error loading image: " + e.Error.Message);
            }
            else
            {
                // 加载成功
                Console.WriteLine("Image loaded successfully.");
                // 在此处执行你需要的操作
                File.Delete("mod.png");
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null && pictureBox1.Tag != null)
            {
                string imagePath = pictureBox1.Tag.ToString();
                Image image = Image.FromFile(imagePath);
                image.RotateFlip(RotateFlipType.Rotate90FlipX);
                image.RotateFlip(RotateFlipType.Rotate180FlipX);
                image.Save(imagePath);

                updateimg(imagePath);

                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
                pictureBox2.Image.Dispose();
                pictureBox2.Image = null;
                pictureBox3.Image.Dispose();
                pictureBox3.Image = null;
            }
            else
            {
                MessageBox.Show("没有加载任何图片！");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private bool CheckFileStatus(string fileName)
        {
            // 获取当前目录
            string currentDirectory = Directory.GetCurrentDirectory();

            // 构建文件路径
            string filePath = Path.Combine(currentDirectory, fileName);

            // 检查文件是否存在
            if (File.Exists(filePath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            check();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void checkonly(string file)
        {
            bool EPD = CheckFileStatus("EPDTools.exe");
            bool cv = CheckFileStatus("opencv_world452.dll");
            bool FSD = CheckFileStatus("FloydSteinbergDithering.exe");
            if (file.Equals("EPDTools.exe"))
            {
                if (EPD)
                {
                    label6.Text = "EPDTools.exe：存在";
                    label6.ForeColor = Color.Green;
                }
                else
                {
                    label6.Text = "EPDTools.exe：丢失";
                    label6.ForeColor = Color.Red;
                }
            }
            else if (file.Equals("opencv_world452.dll"))
            {
                if (cv)
                {
                    label7.Text = "opencv_world452.dll：存在";
                    label7.ForeColor = Color.Green;
                }
                else
                {
                    label7.Text = "opencv_world452.dll：丢失";
                    label7.ForeColor = Color.Red;
                }
            }
            else if (file.Equals("FloydSteinbergDithering.exe"))
            {
                if (FSD)
                {
                    label8.Text = "FloydSteinbergDithering.exe：存在";
                    label8.ForeColor = Color.Green;
                }
                else
                {
                    label8.Text = "FloydSteinbergDithering.exe：丢失";
                    label8.ForeColor = Color.Red;
                }
            }
        }
        private void check()
        {
            bool EPD = CheckFileStatus("EPDTools.exe");
            bool cv = CheckFileStatus("opencv_world452.dll");
            bool FSD = CheckFileStatus("FloydSteinbergDithering.exe");
            checkonly("EPDTools.exe");
            checkonly("opencv_world452.dll");
            checkonly("FloydSteinbergDithering.exe");
            if (!EPD || !cv || !FSD)
            {
                DialogResult result = MessageBox.Show("文件存在丢失，是否修复？", "修复", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    if (!EPD)
                    {
                        // 实例化 Form3 对象
                        Form3 form3 = new Form3();
                        // 调用 Form3 的 DoSomething 方法
                        form3.Download("https://cn-sy1.rains3.com/zhilianxiazai/EPDTools.exe", "EPDTools.exe");
                        form3.check_only += new Form3.checkonly(DateCheck);

                        // 显示 Form3 窗口
                        form3.Show();
                    }
                    if (!cv)
                    {
                        // 实例化 Form3 对象
                        Form3 form31 = new Form3();

                        // 调用 Form3 的 DoSomething 方法
                        form31.Download("https://cn-sy1.rains3.com/zhilianxiazai/opencv_world452.dll", "opencv_world452.dll");
                        form31.check_only += new Form3.checkonly(DateCheck);

                        // 显示 Form3 窗口
                        form31.Show();
                    }
                    if (!FSD)
                    {
                        // 实例化 Form3 对象
                        Form3 form32 = new Form3();

                        // 调用 Form3 的 DoSomething 方法
                        form32.Download("https://cn-sy1.rains3.com/zhilianxiazai/FloydSteinbergDithering.exe", "FloydSteinbergDithering.exe");
                        form32.check_only += new Form3.checkonly(DateCheck);

                        // 显示 Form3 窗口
                        form32.Show();
                    }
                }
                else
                {
                    MessageBox.Show("你取消了不修复！请自行补全文件。");
                }


            }
        }

        void DateCheck(string file)
        {
            checkonly(file);
        }

        public async void updateimg(string imagePath)
        {
            if (comport.SelectedItem != null)
            {
                if (comport.SelectedItem != null)
                {
                    string selectedPortName = comport.SelectedItem.ToString();
                    string method = methodsel.SelectedItem.ToString();
                    DialogResult result = MessageBox.Show("当前选择的串行端口是：" + selectedPortName + "，当前选择的模式是：" + method + "，是否确认？", "确认选择", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        if (method.Equals("色彩抖动"))
                        {
                            method = "dith";
                        }
                        else
                        {
                            method = "mod";
                        }

                        // 要执行的程序的路径
                        string programPath = "EPDTools.exe";

                        // 要传递给程序的参数
                        string arguments = "-f " + imagePath + " -m " + method + " -w serial -p " + selectedPortName;

                        // 创建进程启动信息对象
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = programPath;
                        startInfo.Arguments = arguments;

                        // 设置进程启动信息
                        startInfo.UseShellExecute = false;  // 必须设置为false以便可以指定Arguments

                        if (!checkBox1.Checked)
                        {
                            startInfo.RedirectStandardOutput = true; // 如果需要捕获程序的输出，可以设置为true
                            startInfo.CreateNoWindow = true;
                            startInfo.RedirectStandardOutput = true;
                            startInfo.RedirectStandardError = true;
                            startInfo.RedirectStandardInput = true; // Is a MUST!
                        }

                        // 创建进程对象
                        Process process = new Process();
                        process.StartInfo = startInfo;

                        process.StartInfo.UseShellExecute = false;
                        process.EnableRaisingEvents = true;

                        try
                        {
                            // 启动进程
                            process.Start();

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();



                            // 等待进程退出，如果你需要的话
                            await process.WaitForExitAsync();

                            if (!checkBox1.Checked)
                            {
                                // 可以读取程序的输出
                                string output = process.StandardOutput.ReadToEnd();
                                Console.WriteLine(output);
                                MessageBox.Show(output);
                            }

                            // 检查文件是否存在
                            if (File.Exists("temp.png"))
                            {
                                // 如果文件存在，则删除它
                                File.Delete("temp.png");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("An error occurred: " + ex.Message);
                        }
                    }
                    else
                    {
                        // 用户取消选择，可以执行取消操作
                        MessageBox.Show("你取消了选择！");
                    }
                }
                else
                {
                    MessageBox.Show("请选择一个模式！");
                }
            }
            else
            {
                MessageBox.Show("请选择一个串行端口！");
            }
        }
        private void loadimg(string file, string outfile, string method, PictureBox pictureBox)
        {
            // 要执行的程序的路径
            string programPath = "FloydSteinbergDithering.exe";

            // 要传递给程序的参数
            string arguments = file + " " + outfile + " " + method;

            // 创建进程启动信息对象
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = programPath;
            startInfo.Arguments = arguments;

            // 设置进程启动信息
            startInfo.UseShellExecute = false;  // 必须设置为false以便可以指定Arguments
            startInfo.RedirectStandardOutput = true; // 如果需要捕获程序的输出，可以设置为true

            // 创建进程对象
            Process process = new Process();
            process.StartInfo = startInfo;

            try
            {
                // 启动进程
                process.Start();

                // 等待进程退出
                process.WaitForExit();

                pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // 设置 PictureBox 的缩放模式为 Zoom
                pictureBox.ImageLocation = outfile;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            switch (checkBox1.CheckState)
            {
                case CheckState.Checked:
                    // Code for checked state.  
                    break;
                case CheckState.Unchecked:
                    DialogResult res = MessageBox.Show("关闭显示命令行请通过底座led判断状况且无法观察上传进度，是否继续？", "温馨提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        break;
                    }
                    else
                    {
                        checkBox1.Checked = true;
                    }
                    break;
                case CheckState.Indeterminate:
                    // Code for indeterminate state.  
                    break;
            }
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
