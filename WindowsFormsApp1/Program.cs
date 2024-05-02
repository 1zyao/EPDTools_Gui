using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
    internal static class Program
    {
        // 声明 C++ 函数原型
        [DllImport("ImageProcessor.dll", EntryPoint = "ImageTo", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ImageTo(string filePath);
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

    }
}
