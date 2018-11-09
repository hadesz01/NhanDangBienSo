using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tesseract;

namespace TienXuLi
{
    public partial class Form1 : Form
    {
        Image<Bgr, byte> _src;
        Image<Bgr, byte> src;
        Image<Gray, byte> _grayImg;
        Image<Gray, byte> dst;
        Image<Gray, byte> temp;
        Image<Gray, byte> temp1;
        Image<Gray, byte> temp2;
        Rectangle location;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _src = new Image<Bgr, byte>(dlg.FileName);
                Image<Bgr, byte> _src1 = new Image<Bgr, byte>(dlg.FileName);
                src = new Image<Bgr, byte>(dlg.FileName);
                pictureBox1.Image = _src1.Bitmap;
            }
            _grayImg = new Image<Gray, byte>(_src.Width, _src.Height, new Gray(0));
            _grayImg = _src.Convert<Gray, byte>();

            #region test cmt
            //imageBox1.Image = _grayImg;
            ////Image<Gray, byte> bw = _grayImg.Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));
            //Image<Gray, byte> clone = _grayImg.Clone();
            //Mat kernel=CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle,new Size(5,5),new Point(-1,-1));
            //Image<Gray, byte> black =clone.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Blackhat, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1.0));
            //Image<Gray, float> sobel = new Image<Gray, float>(clone.Width, clone.Height, new Gray(0));
            //sobel = clone.Laplace(9);
            //imageBox2.Image = sobel;
            //imageBox3.Image=black;
            //Image<Gray, byte> normal = black.Clone();normal._EqualizeHist();
            //imageBox5.Image = normal;
            //Image<Gray, byte> pydown = clone.PyrDown();

            //imageBox4.Image = pydown;
            #endregion
            Image<Gray, byte> imgClone = _grayImg.Clone();
            Image<Gray, byte> pydown = new Image<Gray, byte>(imgClone.Width / 2, imgClone.Height / 2, new Gray(0));
            Image<Gray, byte> nomalize = new Image<Gray, byte>(imgClone.Width / 2, imgClone.Height / 2, new Gray(0));
            Image<Gray, byte> thresh = new Image<Gray, byte>(imgClone.Width / 2, imgClone.Height / 2, new Gray(0));
            pictureBox1.Image = _grayImg.Bitmap;


            Mat kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(6, 3), new Point(1, 1));
            pydown = imgClone.Clone();
            //CvInvoke.PyrDown(pydown, pydown, Emgu.CV.CvEnum.BorderType.Default);
            //pydown.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Blackhat,kernel,new Point(-1,-1),1,Emgu.CV.CvEnum.BorderType.Default,new MCvScalar(1.0));


            CvInvoke.MorphologyEx(pydown, pydown, Emgu.CV.CvEnum.MorphOp.Blackhat, kernel, new Point(1, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1.0));
            //giản lược chi tiết ảnh

            nomalize = new Image<Gray, byte>(pydown.Width, pydown.Height, new Gray(0));
            CvInvoke.Normalize(pydown, nomalize, 255, 0, Emgu.CV.CvEnum.NormType.MinMax);


            thresh = nomalize.Clone();
            //
            CvInvoke.Threshold(thresh, thresh, 500, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);
            dst = new Image<Gray, byte>(_grayImg.Width, _grayImg.Height);
            dst.SetZero();

            //dùng ma trận điểm gồm 4 ma trận con trượt trên ảnh xét số điểm trắng lớn hơn ngưỡng nhất định
            //xác định các vùng nghi ngờ là biển số
            int count;
            int diem1, diem2, diem3, diem4;
            Rectangle rect;
            for (int i = 0; i < thresh.Width - 16; i += 4)
            {
                for (int j = 0; j < thresh.Height - 32; j += 4)
                {
                    rect = new Rectangle(i, j, 16, 8);
                    CvInvoke.cvSetImageROI(thresh, rect);
                    diem1 = CvInvoke.CountNonZero(thresh);
                    CvInvoke.cvResetImageROI(thresh);

                    rect = new Rectangle(i + 16, j, 16, 8);
                    CvInvoke.cvSetImageROI(thresh, rect);
                    diem2 = CvInvoke.CountNonZero(thresh);
                    CvInvoke.cvResetImageROI(thresh);

                    rect = new Rectangle(i, j + 8, 16, 8);
                    CvInvoke.cvSetImageROI(thresh, rect);
                    diem3 = CvInvoke.CountNonZero(thresh);
                    CvInvoke.cvResetImageROI(thresh);

                    rect = new Rectangle(i + 8, j + 16, 16, 8);
                    CvInvoke.cvSetImageROI(thresh, rect);
                    diem4 = CvInvoke.CountNonZero(thresh);
                    CvInvoke.cvResetImageROI(thresh);
                    count = 0;

                    if (diem1 > 15) count++;
                    if (diem2 > 15) count++;
                    if (diem3 > 15) count++;
                    if (diem4 > 15) count++;
                    if (count > 2)
                    {
                        rect = new Rectangle(i, j, 32, 16);
                        CvInvoke.cvSetImageROI(thresh, rect);
                        CvInvoke.cvSetImageROI(dst, rect);
                        CvInvoke.cvCopy(thresh, dst, IntPtr.Zero);// copy ảnh nhị phân vào ảnh đích với ROI là i j 
                        CvInvoke.cvResetImageROI(dst);
                        CvInvoke.cvResetImageROI(thresh);
                    }
                }
            }

            temp = dst.Clone();
            temp = temp.Dilate(4); temp = temp.Erode(9);


            imageBox1.Image = imgClone;
            imageBox2.Image = pydown;
            imageBox3.Image = nomalize;
            imageBox4.Image = thresh;
            imageBox5.Image = temp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Image<Gray, byte> blackimg = _src.Convert<Gray, byte>();
            VectorOfVectorOfPoint vect = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(temp, vect, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(blackimg, vect, -1, new MCvScalar(255, 255, 0));

            Dictionary<int, double> dict = new Dictionary<int, double>();
            if (vect.Size > 0)
            {
                for (int i = 0; i < vect.Size; i++)
                {
                    double area = CvInvoke.ContourArea(vect[i]);
                    dict.Add(i, area);

                }
            }
            var item = dict.OrderByDescending(v => v.Value);
            foreach (var it in item)
            {
                int key = int.Parse(it.Key.ToString());
                Rectangle rect = CvInvoke.BoundingRectangle(vect[key]);
                rect.Width = rect.Width + 20;
                rect.Height = rect.Height + 10;
                if (rect.Height / rect.Width <= 1.2 && rect.Height / rect.Width >= 0.3)
                {
                    CvInvoke.Rectangle(_src, rect, new MCvScalar(100, 0, 0), 4);
                    CvInvoke.cvSetImageROI(_grayImg, rect);
                    temp = _grayImg;
                    CvInvoke.Threshold(_grayImg, temp, 500, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);
                    CvInvoke.Resize(temp, temp, new Size(640, 480));
                    imageBox1.Image = temp;
                    //MessageBox.Show(temp.Height.ToString() + temp.Width.ToString());
                    location = rect;
                    
                }
                CvInvoke.Rectangle(_src, rect, new MCvScalar(0, 0, 255), 1);
                
            }
         
            pictureBox1.Image = _src.Bitmap;
            label1.Text = "contours";

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //string path = "d:\\Untitled.png";
            //Image<Bgr, byte> iii = new Image<Bgr, byte>(path);
            //Image<Gray, byte> ii = iii.Convert<Gray, byte>();
            ////Image<Gray, byte> ii = temp1;
            //TesseractEngine engine = new TesseractEngine("./tessdata", "eng");
            //Page pag = engine.Process(ii.Bitmap, PageSegMode.Auto);
            //string text = pag.GetText();
            //MessageBox.Show(text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> color1 = new Image<Bgr,byte>(temp.Width,temp.Height);
            color1.SetZero();
            CvInvoke.cvSetImageROI(src, location);
            CvInvoke.cvCopy(src, color1, IntPtr.Zero);
            Mat kernel=CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross,new Size(4,4),new Point(-1,-1));
            temp1 = temp.Clone();
            CvInvoke.Erode(temp, temp1, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, default(MCvScalar));
            //CvInvoke.Dilate(temp, temp2, null, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, default(MCvScalar));
            temp2 = temp1.Clone();
            temp2._Dilate(1);
            temp2._Not();
            imageBox4.Image = temp2;
            imageBox5.Image = temp1;

            //Tìm contours của số trên ảnh temp2//

            Image<Gray,byte>temp3=temp2.Clone();
            VectorOfVectorOfPoint vect = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(temp3, vect, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(temp3, vect, -1, new MCvScalar(255, 255, 0));

            Dictionary<int, double> dict = new Dictionary<int, double>();
            if (vect.Size > 0)
            {
                for (int i = 0; i < vect.Size; i++)
                {
                    double area = CvInvoke.ContourArea(vect[i]);
                    dict.Add(i, area);
                }
            }
            var item = dict.OrderByDescending(v => v.Value);
            foreach (var it in item)
            {
                int key = int.Parse(it.Key.ToString());
                Rectangle rect = CvInvoke.BoundingRectangle(vect[key]);
                //CvInvoke.Rectangle(temp3, rect, new MCvScalar(255, 0, 0), 1);    
                if (rect.Height/rect.Width>0.3)
                {
                    CvInvoke.Rectangle(color1, rect, new MCvScalar(0, 255, 0), 1);
                }
                
            }
            CvInvoke.cvResetImageROI(src);
            imageBox2.Image = color1;
            imageBox3.Image = temp3;
        }
    }
}
