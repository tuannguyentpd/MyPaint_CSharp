using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace _1612774
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(500, 500);

            Draw_Oxy(bmp);

            //test ve nhieu duong thang
            int[] x1 = new int[500];
            int[] x2 = new int[500];
            int[] y1 = new int[500];
            int[] y2 = new int[500];
            for (int i = 0; i < 500; i++)
            {
                Random ran = new Random();
                x1[i] = ran.Next(0, 499);
                Thread.Sleep(2);
                x2[i] = ran.Next(0, 499);
                Thread.Sleep(2);
                y1[i] = ran.Next(0, 499);
                Thread.Sleep(2);
                y2[i] = ran.Next(0, 499);
                Thread.Sleep(2);
            }
            //for (int i = 0; i < 500; i++)
            //Draw_Line_Presenham(bmp, x1[i], x2[i], y1[i], y2[i], Color.Red);

            /*
             * (10,10)-(100,10)-(10,100)-(100,100)
            */
            //Draw_Line_DDA(bmp, 100, 10, 10, 100, Color.Red);
            //Draw_Line_MidPoint(bmp, 10, 100, 10, 10, Color.Red);
            //Draw_Line_MidPoint(bmp, 499, 0, 0, 499, Color.Red);
            //Draw_Circle_MidPoint(bmp, 100, 100, 90, Color.Red);
            //Draw_Circle_Presenham(bmp, 301, 301, 100, Color.Red);
            //Draw_Line_MidPoint(bmp, 100, 10, 10, 100, Color.Red);
            //Draw_Ellipse_MidPoint(bmp, 100, 100, 90, Color.Red);
            Draw_Ellipse_Presenham(bmp, 100, 100, 80,50, Color.Red);


            show_plot.Image = bmp;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
           
        }

        private void Draw_Oxy(Bitmap bmp)
        {
            int mid = show_plot.Size.Width / 2;

            Draw_Line_DDA(bmp, 0, mid, 499, mid,Color.Gold); //Truc Ox
            Draw_Line_DDA(bmp, mid, 0, mid, 499, Color.Gold); //Truc Oy
        }

        private void MovePoint(int x,int y)
        {
            int mid_Rid = show_plot.Size.Width / 2;
            if (x > 0) x = mid_Rid - x;
            else x = mid_Rid + x;
            if (y > 0) y = mid_Rid - y;
            else y = mid_Rid + y;
        }

        private void Draw_Line_DDA(Bitmap bmp, int x1, int y1, int x2, int y2, Color c)
        {
            if (x1 == x2 && y1 == y2)
            {
                bmp.SetPixel(x1, y1, c);
                return;
            }
            int Dx = x2 - x1, Dy = y2 - y1, x, y;
            float delta, temp;
            if (Math.Abs(Dx) > Math.Abs(Dy))
            {
                delta = Dy / Dx;
                if (x1 > x2) {
                    int tem = x1;x1 = x2;x2 = tem;
                    tem = y1;y1 = y2;y2 = tem;
                }
                temp = y1;x = x1;
                do
                {
                    bmp.SetPixel(x, (int)Math.Round(temp), c);
                    ++x;
                    temp += delta;
                } while (x <= x2);
            }
            else
            {
                delta = Dx / Dy;
                if (y1 > y2) {
                    int tem = x1;x1 = x2;x2 = tem;
                    tem = y1;y1 = y2;y2 = tem;
                }
                y = y1;temp = x1;
                do
                {
                    bmp.SetPixel((int)Math.Round(temp), y, c);
                    ++y;
                    temp += delta;
                } while (y <= y2);
            }
        }

        private void Draw_Line_Presenham(Bitmap bmp,int x1,int y1,int x2,int y2,Color c)
        {
            int x, y, Dx, Dy, p;
            if (x1 > x2)
            {
                int temp = x1;x1 = x2;x2 = temp;
                temp = y1;y1 = y2;y2 = temp;
            }
            bmp.SetPixel(x1, y1, c);
            Dx = Math.Abs(x2 - x1);
            Dy = Math.Abs(y2 - y1);
            x = x1;
            y = y1;
            if (Dy < Dx)
            {
                p = 2 * Dy - Dx;
                bmp.SetPixel(x, y, c);
                while (x <= x2)
                {
                    ++x;
                    if (p < 0)
                        p += 2 * Dy;
                    else
                    {
                        if (y1 < y2)
                        {
                            ++y;
                            p += 2 * (Dy - Dx);
                        }
                        else
                        {
                            --y;
                            p += 2 * (Dy - Dx);
                        }
                    }
                    bmp.SetPixel(x, y, c);
                }
            }
            else
            {
                p = 2 * Dx - Dy;
                bmp.SetPixel(x, y, c);
                if (y1 < y2)
                {
                    while (y <= y2)
                    {
                        y += 1;
                        if (p < 0)
                            p += 2 * Dx;
                        else
                        {
                            x += 1;
                            p += 2 * (Dx - Dy);
                        }
                        bmp.SetPixel(x, y, c);
                    }
                }
                else
                {
                    while (y >= y2)
                    {
                        y -= 1;
                        if (p < 0)
                            p += 2 * Dx;
                        else
                        {
                            x += 1;
                            p += 2 * (Dx - Dy);
                        }
                        bmp.SetPixel(x, y, c);
                    }
                }
            }
        }

        private void Draw_Line_MidPoint(Bitmap bmp,int x1,int y1,int x2,int y2,Color c)
        {
            int Dx = x2 - x1, Dy = y2 - y1, x, y;
            float delta;
            if (Math.Abs(Dx) < Math.Abs(Dy))
            {
                delta = Dx - (Dy / 2);
                if (x1 > x2)
                {
                    int tem = x1; x1 = x2; x2 = tem;
                    tem = y1; y1 = y2; y2 = tem;
                    delta = -delta;
                }
                y = y1; x = x1;
                while (y <= y2)
                {
                    ++y;
                    if (delta < 0) delta += Dx;
                    else
                    {
                        delta += Dx - Dy ;
                        ++x;
                    }
                    bmp.SetPixel(x, y, c);
                }
            }
            else
            {
                delta = Dy - (Dx / 2);
                if (y1 > y2)
                {
                    int tem = x1; x1 = x2; x2 = tem;
                    tem = y1; y1 = y2; y2 = tem;
                    delta = -delta;
                }
                y = y1; x = x1;
                while (x <= x2)
                {
                    ++x;
                    if (delta < 0) delta += Dy;
                    else
                    {
                        delta += Dy - Dx;
                        ++y;
                    }
                    bmp.SetPixel(x, y, c);
                }
            }
        }

        //Ve duong tron
        private void Draw_Point_Circle(Bitmap bmp, int x0,int y0,int x,int y,Color c)
        {
            bmp.SetPixel(x0 + x, y0 + y, c);
            bmp.SetPixel(x0 - x, y0 + y, c);
            bmp.SetPixel(x0 + x, y0 - y, c);
            bmp.SetPixel(x0 - x, y0 - y, c);
            bmp.SetPixel(x0 + y, y0 + x, c);
            bmp.SetPixel(x0 - y, y0 + x, c);
            bmp.SetPixel(x0 + y, y0 - x, c);
            bmp.SetPixel(x0 - y, y0 - x, c);
        }

        private void Draw_Circle_DDA(Bitmap bmp, int x0, int y0, int r, Color c)
        {

        }

        private void Draw_Circle_Presenham(Bitmap bmp,int x0,int y0,int r, Color c)
        {
            int x = 0, y = r;
            int p = 3 - 2 * r;
            while (x <= y)
            {
                Draw_Point_Circle(bmp, x0, y0, x, y, c);
                if (p < 0)
                    p += 4 * x + 6;
                else
                {
                    p += 4 * (x - y) + 10;
                    --y;
                }
                ++x;
            }
        }

        private void Draw_Circle_MidPoint(Bitmap bmp, int x0,int y0,int r,Color c)
        {
            int x = 0, y = r;
            int p = 5 - 4 * r;
            while (x <= y)
            {
                Draw_Point_Circle(bmp, x0, y0, x, y, c);
                if (p < 0)
                    p += 4*(2 * x + 3);
                else
                {
                    p += 4*(2 * (x - y) + 5);
                    --y;
                }
                ++x;
            }
        }

        //Ve ellipse
        private void Draw_Point_Ellipse(Bitmap bmp, int xc, int yc, int a, int b, Color c)
        {
            bmp.SetPixel(xc + a, yc + b, c);
            bmp.SetPixel(xc - a, yc + b, c);
            bmp.SetPixel(xc - a, yc - b, c);
            bmp.SetPixel(xc + a, yc - b, c);
        }

        private void Draw_Ellipse_Presenham(Bitmap bmp, int xc, int yc, int a, int b, Color c)
        {
            int a2 = a * a, b2 = b * b;

            //Nhanh 1
            int x = 0, y = b, p = 2 * b2 / a2 - 2 * b + 1;
            while ((b2 / a2) * (x / y) < 1)
            {
                Draw_Point_Ellipse(bmp, xc, yc, x, y, c);
                if (p < 0) p += 2 * (b2 / a2) * (2 * x + 3);
                else
                {
                    p = p - 4 * y + 2 * (b2 / a2) * (2 * x + 3);
                    --y;
                }
                ++x;
            }

            //Nhanh 2
            y = 0; x = a; p = 2 * a2 / b2 - 2 * a + 1;
            while ((a2 / b2) * (y / x) < 1)
            {
                Draw_Point_Ellipse(bmp, xc, yc, x, y, c);
                if (p < 0) p += 2 * (a2 / b2) * (2 * y + 3);
                else
                {
                    p = p - 4 * x + 2 * (a2 / b2) * (2 * y + 3);
                    --x;
                }
                ++y;
            }
        }

        private void Draw_Ellipse_MidPoint(Bitmap bmp,int xc, int yc, int rx,int ry,Color c)
        {
            int x = 0, y = ry, p = (ry * ry) - (rx * rx * ry) + ((rx * rx) / 4);
            while ((2 * x * ry * ry) < (2 * y * rx * rx))
            {
                bmp.SetPixel(xc + x, yc - y, c);
                bmp.SetPixel(xc - x, yc + y, c);
                bmp.SetPixel(xc + x, yc + y, c);
                bmp.SetPixel(xc - x, yc - y, c);

                if (p < 0)
                {
                    x = x + 1;
                    p = p + (2 * ry * ry * x) + (ry * ry);
                }
                else
                {
                    x = x + 1;
                    y = y - 1;
                    p = p + (2 * ry * ry * x + ry * ry) - (2 * rx * rx * y);
                }
            }

            double temp = ry * ry + (y - 1) * (y - 1) * rx * rx - rx * rx * ry * ry;
            temp *= ((Convert.ToDouble(x) + 0.5)) * ((Convert.ToDouble(x) + 0.5));
            p = (int)Math.Round(temp);
            

            while (y >= 0)
            {
                bmp.SetPixel(xc + x, yc - y, c);
                bmp.SetPixel(xc - x, yc + y, c);
                bmp.SetPixel(xc + x, yc + y, c);
                bmp.SetPixel(xc - x, yc - y, c);

                if (p > 0)
                {
                    y = y - 1;
                    p = p - (2 * rx * rx * y) + (rx * rx);

                }
                else
                {
                    y = y - 1;
                    x = x + 1;
                    p = p + (2 * ry * ry * x) - (2 * rx * rx * y) - (rx * rx);
                }
            }
        }
    }
}