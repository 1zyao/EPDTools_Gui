using QRCoder;
using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        private Point lastLocation;
        private bool isMoving = false;

        public Form2()
        {
            InitializeComponent();

            // 画布 Panel 控件
            canvasPanel.MouseDown += CanvasPanel_MouseDown;
            canvasPanel.MouseMove += CanvasPanel_MouseMove;
            canvasPanel.MouseUp += CanvasPanel_MouseUp;

            // PictureBox 控件
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;

            // 生成 QR Code
            GenerateQRCode(textBox1.Text, (int)numericUpDown1.Value);
        }

        private void GenerateQRCode(string text, int size)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(5, Color.Black, Color.White, false);
            pictureBox.Image = qrCodeImage;
            pictureBox.Size = new Size(size, size);
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastLocation = e.Location;
                isMoving = true;
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isMoving)
            {
                int deltaX = e.X - lastLocation.X;
                int deltaY = e.Y - lastLocation.Y;

                // 移动 PictureBox
                pictureBox.Left += deltaX;
                pictureBox.Top += deltaY;

                // 限制 PictureBox 不能移出画布
                pictureBox.Left = Math.Max(0, Math.Min(pictureBox.Left, canvasPanel.Width - pictureBox.Width));
                pictureBox.Top = Math.Max(0, Math.Min(pictureBox.Top, canvasPanel.Height - pictureBox.Height));

                lastLocation = e.Location;
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMoving = false;
            }
        }

        private void CanvasPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastLocation = e.Location;
            }
        }

        private void CanvasPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int deltaX = e.X - lastLocation.X;
                int deltaY = e.Y - lastLocation.Y;

                // 移动 PictureBox
                pictureBox.Left += deltaX;
                pictureBox.Top += deltaY;

                // 限制 PictureBox 不能移出画布
                pictureBox.Left = Math.Max(0, Math.Min(pictureBox.Left, canvasPanel.Width - pictureBox.Width));
                pictureBox.Top = Math.Max(0, Math.Min(pictureBox.Top, canvasPanel.Height - pictureBox.Height));

                lastLocation = e.Location;
            }
        }

        private void CanvasPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastLocation = Point.Empty;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "图片文件 (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    using (Bitmap bmp = new Bitmap(canvasPanel.Width, canvasPanel.Height))
                    {
                        canvasPanel.DrawToBitmap(bmp, new Rectangle(0, 0, canvasPanel.Width, canvasPanel.Height));
                        bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    MessageBox.Show("已导出到：" + fileName, "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (numericUpDown1.Value > 0)
                {
                    GenerateQRCode(textBox1.Text, (int)numericUpDown1.Value);
                }
                else
                {
                    MessageBox.Show("大小错误！");
                }
            }
            else
            {
                MessageBox.Show("请先输入文本");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(canvasPanel.Width, canvasPanel.Height);

            canvasPanel.DrawToBitmap(bmp, new Rectangle(0, 0, canvasPanel.Width, canvasPanel.Height));

            Form1 form1 = Application.OpenForms["Form1"] as Form1;
            if (form1 != null)
            {
                form1.updateimg(bmp);
            }
        }
    }
}
