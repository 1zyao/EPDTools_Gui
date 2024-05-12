using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        public delegate void checkonly(string file);
        public event checkonly check_only;
        public Form3()
        {
            InitializeComponent();
            this.FormClosed += Form3_FormClosed;
        }

        public void Download(string fileUrl, string savePath)
        {
            label2.Text = savePath;

            using (WebClient client = new WebClient())
            {
                // 下载进度改变事件处理
                client.DownloadProgressChanged += (s, args) =>
                {
                    // 更新进度条
                    progressBar1.Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = args.ProgressPercentage;
                    });
                    label3.Text = args.ProgressPercentage + " %";
                };

                // 下载完成事件处理
                client.DownloadFileCompleted += (s, args) =>
                {
                    if (args.Error != null)
                    {
                        Close();
                        MessageBox.Show("下载出错：" + args.Error.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        File.Delete(savePath);
                    }
                    else
                    {
                        Close();
                        MessageBox.Show("文件 " + savePath + " 下载完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                // 开始下载文件
                try
                {
                    client.DownloadFileAsync(new Uri(fileUrl), savePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("下载出错：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        // 在 Form3 中的 FormClosed 事件处理程序中调用 Form1 的 CheckOnly 方法
        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            check_only(label2.Text);
        }
    }
}
