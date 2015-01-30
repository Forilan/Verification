using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Verification
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bit = new Bitmap(Properties.Resources._1);

            for (int i = 0; i < bit.Height; ++i)
            {
                for (int j = 0; j < bit.Width; ++j)
                {
                    int pix = GetGrayNumColor(bit.GetPixel(j, i));
                    bit.SetPixel(j, i, Color.FromArgb(255-pix, 255-pix, 255-pix));
                }
            }

            //pictureBox2.Image = bit;
            pictureBox3.Image = ImageTwoValue(bit);
            pictureBox2.Image = RemoveEdge(bit);
        }

        /// <summary>
        /// 根据RGB,计算灰度值
        /// </summary>
        /// <param name="posClr"></param>
        /// <returns></returns>
        private int GetGrayNumColor(Color posClr)
        {
            return (posClr.R * 19595 + posClr.G * 38469 + posClr.B * 7472) >> 16;
        }

        /// <summary>
        /// 颜色二值化
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        private Bitmap ImageTwoValue(Bitmap bit)
        {
            int bgGrayValue = 128; //灰度背景分界值
            int CharCount = 4;  //有效字符
            int posX1 = bit.Width, posY1 = bit.Height;
            int posX2 = 0, posY2 = 0;
            for (int i = 0; i < bit.Height; ++i)
            {
                for (int j = 0; j < bit.Width; ++j)
                {
                    int pixValue = bit.GetPixel(j, i).R;
                    if (pixValue < bgGrayValue)
                    {
                        if (posX1 > j) posX1 = j;
                        if (posY1 > i) posY1 = i;
                        if (posX2 < j) posX2 = j;
                        if (posY2 < i) posY2 = i;
                    }
                }
            }

            int Span = CharCount - (posX2 - posX1 + 1) % CharCount;//确保能整除

            //可整除的差额数
            if (Span < CharCount)
            {
                int leftSpan = Span / 2;

                //分配到左边的空列，如span为单数，则右边比左边大1
                if (posX1 > leftSpan)
                {
                    posX1 = posX1 - leftSpan;
                }

                if (posX2 + Span - leftSpan < bit.Width)
                {
                    posX2 = posX2 - Span + leftSpan;
                }
            }

            Rectangle rect = new Rectangle(posX1, posY1, posX2 - posX1 + 1, posY2 - posY1 + 1);
            bit = bit.Clone(rect, bit.PixelFormat);

            return bit;
        }

        //private Bitmap[] GetSplitPics(Bitmap bit, int RowNum, int ColNum)
        //{
        //    if (RowNum == 0 || ColNum == 0)
        //        return null;


        //}

        /// <summary>
        /// 去边
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        private Bitmap RemoveEdge(Bitmap bit)
        {
            int bgGrayValue = 128;//灰度背景分界值
            for (int i = 0; i < bit.Height; ++i)
            {
                for (int j = 1; j < bit.Width; ++j)
                {
                    int pixValue = bit.GetPixel(j, i).R;
                    if (pixValue > bgGrayValue && bit.GetPixel(j-1, i).R > bgGrayValue)
                    {
                        bit.SetPixel(j, i, Color.FromArgb(255, 255, 255));
                    }
                }
            }

            return bit;
        }

    }
}

