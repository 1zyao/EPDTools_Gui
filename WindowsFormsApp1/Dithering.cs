using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1;

public class ImgData
{
    public byte[] BWData { get; set; }
    public byte[] RWData { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public ushort DataLen { get; set; }
}

namespace Dithering
{
    public class Dithering
    {
        private readonly int[][] colorPalette =
        {
            new[] { 0, 0, 0, 255 },  // Black
            new[] { 255, 255, 255, 255 },  // White
            new[] { 255, 0, 0, 255 }  // Red
        };

        public ImgData ConvertToImgData(Bitmap image)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipX);

            int width = image.Width;
            int height = image.Height;

            string type = "floydsteinberg";
            int threshold = 128;
            
            if (width % 8 != 0)
            {
                width += 8 - (width % 8);
            }

            byte[] bwData = new byte[(height * width) / 8];
            byte[] rwData = new byte[(height * width) / 8];
            int index = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 8)
                {
                    byte currentBWByte = 0;
                    byte currentRWByte = 0;

                    for (int bit = 0; bit < 8; bit++)
                    {
                        int adjustedX = x + bit;
                        if (adjustedX < image.Width)
                        {
                            Color pixel = image.GetPixel(adjustedX, y);
                            int luminance = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                            if (type == "none")
                            {
                                luminance = luminance < threshold ? 0 : 255;
                            }
                            else if (type == "bayer")
                            {
                                int bayerValue = GetBayerValue(adjustedX, y);
                                luminance = (luminance + bayerValue) / 2 < threshold ? 0 : 255;
                            }
                            else if (type == "floydsteinberg")
                            {
                                luminance = ApplyFloydSteinbergDithering(image, adjustedX, y, luminance);
                            }

                            int closestColorIndex = FindClosestColor(new[] { luminance, luminance, luminance, pixel.A });
                            int closestColorIndex1 = FindClosestColor(new[] { (int)pixel.R, pixel.G, pixel.B, pixel.A });
                            bool isBlackWhite = closestColorIndex != 0;
                            bool isRedWhite = closestColorIndex1 != 2;

                            currentBWByte |= (byte)((isBlackWhite ? 1 : 0) << (7 - bit));
                            currentRWByte |= (byte)((isRedWhite ? 1 : 0) << (7 - bit));
                        }
                    }

                    bwData[index] = currentBWByte;
                    rwData[index] = currentRWByte;
                    index++;
                }
            }

            return new ImgData
            {
                BWData = bwData,
                RWData = rwData,
                Width = (ushort)image.Width,
                Height = (ushort)height,
                DataLen = (ushort)(index)
            };
        }

        private int GetBayerValue(int x, int y)
        {
            int[,] bayerThresholdMap = {
                { 15, 135, 45, 165 },
                { 195, 75, 225, 105 },
                { 60, 180, 30, 150 },
                { 240, 120, 210, 90 }
            };
            return bayerThresholdMap[x % 4, y % 4];
        }

        private int ApplyFloydSteinbergDithering(Bitmap image, int x, int y, int oldPixel)
        {
            //int newPixel = oldPixel < 129? 0 : 255;
            Form1 form1 = Application.OpenForms["Form1"] as Form1;
            int newPixel = oldPixel < form1.trackBar1.Value ? 0 : 255;
            int quantError = oldPixel - newPixel;
            SetPixelWithError(image, x + 1, y, quantError * 7 / 16);
            SetPixelWithError(image, x - 1, y + 1, quantError * 3 / 16);
            SetPixelWithError(image, x, y + 1, quantError * 5 / 16);
            SetPixelWithError(image, x + 1, y + 1, quantError * 1 / 16);
            return newPixel;
        }

        private void SetPixelWithError(Bitmap image, int x, int y, int error)
        {
            if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
            {
                Color pixel = image.GetPixel(x, y);
                int newR = Clamp(pixel.R + error);
                int newG = Clamp(pixel.G + error);
                int newB = Clamp(pixel.B + error);
                image.SetPixel(x, y, Color.FromArgb(pixel.A, newR, newG, newB));
            }
        }

        private int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        private int FindClosestColor(int[] pixelColor)
        {
            double minDistance = double.MaxValue;
            int closestColorIndex = 0;

            for (int i = 0; i < colorPalette.Length; i++)
            {
                double distance = CalculateColorDistance(pixelColor, colorPalette[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestColorIndex = i;
                }
            }
            return closestColorIndex;
        }

        private double CalculateColorDistance(int[] color1, int[] color2)
        {
            int rMean = (color1[0] + color2[0]) / 2;
            int r = color1[0] - color2[0];
            int g = color1[1] - color2[1];
            int b = color1[2] - color2[2];
            return Math.Sqrt(((512 + rMean) * r * r) / 256 + 4 * g * g + ((767 - rMean) * b * b) / 256);
        }
        public Bitmap ConvertToBitmap(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            // Create a new Bitmap to hold the dithered image
            Bitmap ditheredImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int luminance = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                    // Apply Floyd-Steinberg dithering
                    int newPixel = ApplyFloydSteinbergDithering(image, x, y, luminance);


                    int closestColorIndex = FindClosestColor(new[] { (int)pixel.R, pixel.G, pixel.B, pixel.A });

                    // Set the pixel in the new image to the new value
                    bool isBlackWhite = closestColorIndex == 0;
                    bool isRedWhite = closestColorIndex == 2;
                    if (isBlackWhite && isRedWhite)
                    {
                        ditheredImage.SetPixel(x, y, Color.FromArgb(128,0,0));
                    }
                    else if (isBlackWhite)
                    {
                        ditheredImage.SetPixel(x, y, Color.Black);
                    }
                    else if (isRedWhite)
                    {
                        ditheredImage.SetPixel(x, y, Color.Red);
                    }
                    else
                    {
                        ditheredImage.SetPixel(x, y, Color.White);
                    }
                }
            }

            return ditheredImage;
        }
    }
}
