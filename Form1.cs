using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Drawing.Drawing2D;

namespace Red_bin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //运行时直接执行btn
            button1_Click(null,null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            #region 设置读取路径 本例所用的雷达数据为9层1001*1001,特别注意本人数据存放上下相反（行是自下而上存放）

            string filename = "Z_2019062511.bin";

            #endregion

            #region 读数据
            FileStream fs = new FileStream(filename, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            //雷达数据 9层，每层1001*1001格点数。
            float[, ,] Rdata = new float[9, 1001, 1001];
            if (fs.Length == 1001 * 1001 * 9 * 4)
            {
                //层
                for (int i = 0; i < 9; i++)
                {
                    //行
                    for (int j = 0; j < 1001; j++)
                    {
                        //列
                        for (int k = 0; k < 1001; k++)
                        {
                            //写入4字节浮点数
                            Rdata[i, j, k] = br.ReadSingle();
                        }
                    }
                }
            }

            br.Close();
            fs.Close();
            #endregion

            #region 绘图
            //设置画布大小600px * 400px
            int width = 600;
            int height = 400;
            Bitmap bitmap = new Bitmap(width,height);
            Graphics g = Graphics.FromImage(bitmap);
            try
            {
                #region 绘图显示区设置、数据绘制范围设置
                //画布背景白色
                //g.Clear(Color.Coral);

                //设置绘图直角坐标原点（左下角起点）
                Point cPt = new Point(30, height-30);

                //设置横向距离和纵向距离值
                int Lx = width - 100;//右侧有70px用于画色标图

                //300px为绘制数据的高度范围，分10等分--->显示高度刻度值
                int Ly = 300;
                int cc = 10;


                #endregion

                //展示绘制数据的范围，gdx为横向绘制多少列，gdy为纵向绘制多少行。演示的数据读取所在行和起始列
                ///*-----------------------------------------*
                ///*  所谓任意角度和方位，需要把两点之间基线 *
                ///*  上的所有点坐标取出来存放到一个数组里， *
                ///*  如（行，列）坐标对数据进行绘制。       *
                ///*    此部分你需要自行研究或咨询 author    *
                ///*  为了美观，还可以先径向插值后再绘制     *
                ///*  本例任意点例子为 点1  到  点2 的连线   *
                ///*-----------------------------------------*

                //获取任意两点之间的线段
                //点1  【行，列】（1000，1000）
                int Fst01h = 200; int Fst01l = 300;
                //点2  【行，列】（1000，1000）
                int Fst02h = 500; int Fst02l = 500;


                #region 任意方位数据获取

                //1、  X横向点数 100-50=50，Y纵向点数 200-50=150  ----->  距离点数为158个
                int Fstnum = Convert.ToInt32(Math.Sqrt((Fst02h - Fst01h) * (Fst02h - Fst01h) + (Fst02l - Fst01l) * (Fst02l - Fst01l)));
                //2、  循环Fstnum次
                int[,] Numda = new int[Fstnum, 2];

                //3、  x1 != x2 时  
                if (Fst02l != Fst01l)
                {
                    // 坐标与数组方向上下反向，故斜率取反
                    double KL = -(Fst02h - Fst01h) * 1.0 / (Fst02l - Fst01l);
                    // Y = KL*（X-Fst01h）+ Fst02l;
                    double mmL = Math.Cos(Math.Atan(KL));
                    double mmH = Math.Sin(Math.Atan(KL));

                    //MessageBox.Show(KL.ToString() + " : " + mmH.ToString() + " : " + mmL.ToString());

                    //点1--->点2   点2在点1右上方
                    if (KL >= 0 & (Fst02l - Fst01l) > 0 & (Fst02h - Fst01h) <= 0)
                    {
                        mmH = -mmH;
                    }
                    //点1--->点2   点2在点1左下方
                    if (KL >= 0 & (Fst02l - Fst01l) <= 0 & (Fst02h - Fst01h) >= 0)
                    {
                        mmL = -mmL;
                    }
                    //点1--->点2   点2在点1左上方
                    if (KL < 0 & (Fst02l - Fst01l) <= 0 & (Fst02h - Fst01h) <= 0)
                    {
                        mmL = -mmL;
                    }
                    //点1--->点2   点2在点1右下方
                    if (KL < 0 & (Fst02l - Fst01l) >= 0 & (Fst02h - Fst01h) >= 0)
                    {
                        mmH = -mmH;
                    }

                    //MessageBox.Show(KL.ToString() + " : " + mmH.ToString() + " : " + mmL.ToString());

                    for (int Fs = 0; Fs < Fstnum; Fs++)
                    {
                        Numda[Fs, 0] = Convert.ToInt32(Fst01h + (Fs) * mmH); //X
                        Numda[Fs, 1] = Convert.ToInt32(Fst01l + (Fs) * mmL); //Y
                    }
                    
                }
                //4、  x1 == x2 时  
                if (Fst02l == Fst01l)
                {
                    //点2 在 点1 下方
                    if (Fst01h < Fst02h)
                    {

                        for (int Fs = 0; Fs < Fstnum; Fs++)
                        {
                            Numda[Fs, 0] = Convert.ToInt32(Fst01h + Fs); //行
                            Numda[Fs, 1] = Convert.ToInt32(Fst01l); //列  列不变
                        }

                    }
                    //点2 在 点1 上方
                    else 
                    {
                        for (int Fs = 0; Fs < Fstnum; Fs++)
                        {
                            Numda[Fs, 0] = Convert.ToInt32(Fst01h -Fs); //行
                            Numda[Fs, 1] = Convert.ToInt32(Fst01l); //列  列不变
                        }
                    }
                }



                int gdx = Fstnum;//300;//  300个格点数目
                int gdy = 9;//    9层数据

                //获取绘制数据的横向和纵向的像素分辨率
                int xL = Convert.ToInt32(Lx / gdx) + 1;//取整，为防止出现图形间隙故加1
                int yL = Convert.ToInt32(Ly / gdy) + 1;//取整，为防止出现图形间隙故加1
                
                #endregion

                #region  记录最大值和最小值（所绘制数据的高度范围）

                double Max19 = 0.0;
                double Min05 = 0.0;
                double MaxLog = 0.0;
                for (int i = 0; i < gdx; i++)
                {
                    for (int j = 0; j < gdy; j++)
                    {
                        //求0.5和19.5仰角高度
                        //double Lg = Math.Sqrt((Lh + i - 500.0) * (Lh + i - 500) + (Ln - 500.0) * (Ln - 500));
                        double Lg = Math.Sqrt((Numda[i, 0] - 500.0) * (Numda[i, 0] - 500) + (Numda[i, 1] - 500.0) * (Numda[i, 1] - 500));

                        double Lx05 = Lg * Math.Tan(0.5 * 3.1415 / 180);
                        double Lx19 = Lg * Math.Tan(19.5 * 3.1415 / 180);
                        if (Lx19>Max19)
                        {
                            Max19 = Lx19;
                            MaxLog = Lg;
                        }
                        if(Lx05<Min05)
                        {
                            Min05 = Lx05;
                        }

                    }
                }

                #endregion

                #region 将数组按色标值绘图
                //创建Brush，用于填充颜色
                Brush[] brush = new Brush[17];
                brush[0] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "000000", 16)));//-5<

                brush[1] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "0000A2", 16)));//0<
                brush[2] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "006AFC", 16)));//5<
                brush[3] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "00BAFC", 16)));//10<
                brush[4] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "6EF8FE", 16)));//15<
                brush[5] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "009632", 16)));//20<
                brush[6] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "00DC00", 16)));//25<
                brush[7] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "B4FEB4", 16)));//30<
                brush[8] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "C4A600", 16)));//35<
                brush[9] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "FEFE00", 16)));//40<
                brush[10] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "EEFE00", 16)));//45<
                brush[11] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "FE0000", 16)));//50<
                brush[12] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "FE6464", 16)));//55<
                brush[13] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "FEB4B4", 16)));//60<
                brush[14] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "9600B4", 16)));//65<
                brush[15] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "C8649A", 16)));//70<
                brush[16] = new SolidBrush(Color.FromArgb(Convert.ToInt32("FF" + "F0C6FC", 16)));//94.5<



                for (int i = 0; i < gdx; i++)
                {
                    for (int j = 0; j < gdy; j++)
                    {
                        //求0.5和19.5仰角高度  1001*1001数组，以（500，500）设为雷达中心点
                        //double Lg = Math.Sqrt((Lh + i - 500.0) * (Lh + i - 500) + (Ln - 500.0) * (Ln - 500));
                        double Lg = Math.Sqrt((1000 - Numda[i, 0] - 500.0) * (1000 - Numda[i, 0] - 500) + (Numda[i, 1] - 500.0) * (Numda[i, 1] - 500));

                        double Lx05 = Lg * Math.Tan(0.5*3.1415/180);
                        double Lx19 = Lg * Math.Tan(19.5 * 3.1415 / 180);

                        #region 将数据按所在范围取颜色值
                        //Rdata[9,1001,1001]
                        Brush brushx = brush[0];//默认为黑色
                        for (int Mj = 0; Mj < 15;Mj++ )
                        {
                            if (Rdata[j, 1000 - Numda[i, 0], Numda[i, 1]] > Mj * 5.0 & Rdata[j, 1000 - Numda[i, 0], Numda[i, 1]] <= (Mj + 1) * 5.0)
                            {
                                brushx = brush[Mj+1];
                            }
                            //大于75dBZ时显示
                            if (Rdata[j, 1000 - Numda[i, 0], Numda[i, 1]] > 75.0 & Rdata[j, 1000 - Numda[i, 0], Numda[i, 1]] < 94.5)
                            {
                                brushx = brush[16];
                            }
                        }

                        #endregion

                        //Max19 Min05    Lx19  Lx05   300px 备注：（雷达半径为500库150公里，（Lx19 * Ly / Max19）可让距离最远的点绘制至300px上边缘）
                        double Zh = (Lx19 * Ly / Max19 - Lx05) / gdy;
                        int Ld = Convert.ToInt32(Lx05);
                        int HX = Convert.ToInt32(Zh);
                        //绘制色斑 因绘制矩形时坐标点为左上角，故Y值取j+1开始
                        g.FillRectangle(brushx, 1 + cPt.X + i * Lx / gdx, cPt.Y - (j + 1) * HX - Ld, xL, HX);

                    }
                }
                #endregion

                #region 绘制坐标系及图例

                //绘制标题
                g.DrawString("天气雷达任意方位剖面图", new Font("宋体", 14), Brushes.White, new PointF(200 - 30, 20));

                //x 轴
                g.DrawLine(Pens.White, cPt.X, cPt.Y, cPt.X + Lx, cPt.Y);
                //y 轴   左、右各一条 
                g.DrawLine(Pens.White, cPt.X, cPt.Y, cPt.X, cPt.Y - Ly);
                g.DrawLine(Pens.White, cPt.X + Lx, cPt.Y, cPt.X + Lx, cPt.Y - Ly);
                //y轴单位（左侧）
                g.DrawString("单位(KM)", new Font("宋体", 12), Brushes.White, new PointF(cPt.X-20, cPt.Y - Ly-30));
                //y轴单位（右侧）
                //g.DrawString("单位(KM)", new Font("宋体", 12), Brushes.White, new PointF(cPt.X + Lx-40, cPt.Y - Ly-30));
                //高度等值线
                Pen p = new Pen(Color.White, 0.1f);//设置笔的粗细为,颜色为白色
                p.DashStyle = DashStyle.Dash;//定义虚线的样式为点
                //绘制虚线
                for (int i = 1; i <= cc; i++)
                {
                    g.DrawLine(p, cPt.X, cPt.Y - i * Ly / cc, cPt.X + Lx, cPt.Y - i * Ly / cc);
                }
                //绘制起点  终点
                g.DrawString("起始点", new Font("宋体", 10), Brushes.White, new PointF(cPt.X - 20, cPt.Y+10));
                g.DrawString("终止点", new Font("宋体", 10), Brushes.White, new PointF(cPt.X + Lx-20, cPt.Y + 10));

                //绘制y刻度  LLnum为最大的刻度数字，然后分为10等份,统一标注
                double LLnum = MaxLog * (150.0 / 500) * Math.Tan(19.5 * Math.PI / 180);
                for (int i = 1; i <= cc; i++)
                {

                    g.DrawString(((i*LLnum/cc).ToString("0.0")).ToString(), new Font("宋体", 10), Brushes.White, new PointF(cPt.X - 30, cPt.Y - i * Ly / cc - 5));
                    g.DrawLine(Pens.White, cPt.X - 3, cPt.Y - i * Ly / cc, cPt.X, cPt.Y - i * Ly / cc);
                }
                //右侧坐标单位数值，不显示
                //for (int i = 1; i <= cc; i++)
                //{
                //    g.DrawString(((i*LLnum/cc).ToString("0.0")).ToString(), new Font("宋体", 10), Brushes.White, new PointF(cPt.X + Lx + 10, cPt.Y - i * Ly / cc - 5));
                //    g.DrawLine(Pens.Black, cPt.X + Lx + 3, cPt.Y - i * Ly / cc, cPt.X + Lx, cPt.Y - i * Ly / cc);
                //}

                //绘制色标
                for (int i = 1; i < 17;i++ )
                {
                    Brush brushxk = brush[i-1];
                    g.FillRectangle(brushxk, cPt.X + Lx + 10, cPt.Y - (i) * 15 - 10, 35, 15);
                    if (i < 16)
                    {
                        g.DrawString(((i - 1) * 5).ToString(), new Font("宋体", 8), Brushes.White, new PointF(cPt.X + Lx + 45, cPt.Y - (i) * 15 - 30 + 15));
                    }
                    else
                    {
                        g.DrawString((94.5).ToString(), new Font("宋体", 8), Brushes.White, new PointF(cPt.X + Lx + 45, cPt.Y - (i) * 15 - 30 + 15));
                    }
                    
                }
                //可根据任意设定，美观即可
                g.DrawString("强度色标", new Font("宋体", 10), Brushes.White, new PointF(cPt.X + Lx+7, cPt.Y - 300+15));
                g.DrawString("单位(dBZ)", new Font("宋体", 8), Brushes.White, new PointF(cPt.X + Lx+10, cPt.Y - 285+15));

                #endregion

                //绘图
                pictureBox1.Image = bitmap;

            }
            catch (Exception ex)
            {
                MessageBox.Show("数据类型错误，请检查再试！");
            }

            #endregion

        }


    }
}
