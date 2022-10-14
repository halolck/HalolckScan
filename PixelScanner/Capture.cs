using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HalolckScan
{
    class CaptureImage
    {
        bool Start = false;
        bool detectcheck = false;
        int fov = 1;
        int imagemode = 0;
        int w = Screen.PrimaryScreen.Bounds.Width;
        int h = Screen.PrimaryScreen.Bounds.Height;
        int minarena = 0;
        int minvertex = 0;
        int maxarena = 0;
        int maxvertex = 0;

        public void AIO(bool detectcheck1,int Mode, int minare, int minvert,int maxvert, Color pcolor, Color rcolor,int width,int height)
        {
            Start = true;
            detectcheck = detectcheck1;
            imagemode = Mode;
            minarena = minare;
            minvertex = minvert;
            maxvertex = maxvert;
            Previous = pcolor;
            Rear = rcolor;
            Rerect(width, height);
        }
        public void CapStart()
        {
            Start = true;
        }
        public void CapStop()
        {
            Start = false;
        }
        public void StateChange()
        {
            Start = !Start;
        }
        public void CheckChange(bool check)
        {
            detectcheck = check;
        }
        public void SetImageMode(int Mode)
        {
            imagemode = Mode;
        }
        public void SetMinArena(int are)
        {
            minarena = are;
        }
        public void SetMinVertex(int vert)
        {
            minvertex = vert;
        }
        public void SetMaxVertex(int vert)
        {
            maxvertex = vert;
        }
        public void SetFov(int fo)
        {
            fov = fo;
        }
        Rect CutRect = new Rect(0, 0, 1, 1);
        Bitmap memoryImage;
        Rect rect;
        Rect targetrect;
        double area;
        Scalar greenscaler = new Scalar(0, 255, 0, 255);
        Scalar bluescaler = new Scalar(255, 0, 0, 255);
        Scalar blackscaler = new Scalar(0, 0, 0, 255);
        Mat originmat;
        Graphics memoryGraphics;
        System.Drawing.Size s = new System.Drawing.Size(1,1);
        Color Previous = Color.Black;
        Color Rear = Color.Black;
        public void SetPrevious(Color color)
        {
            Previous = color;
        }

        public void SetRear(Color color)
        {
            Rear = color;
        }


        public void Rerect(int width,int height)
        {
            CutRect = new Rect(w/2 - width/2, h/2 - height/2, width, height);
            s = new System.Drawing.Size(CutRect.Width, CutRect.Height);
        }
        unsafe Bitmap DetectColorPointWithUnsafe(Bitmap image, byte searchedR, byte searchedG, int searchedB, byte backsearchedR, byte backsearchedG, int backsearchedB)
        {
            BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width,
              image.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int bytesPerPixel = 3;

            byte* scan0 = (byte*)imageData.Scan0.ToPointer();
            int stride = imageData.Stride;

            byte unmatchingValue = 255;
            byte matchingValue = 0;
            for (int y = 0; y < imageData.Height; y++)
            {
                byte* row = scan0 + (y * stride);

                for (int x = 0; x < imageData.Width; x++)
                {
                    // Watch out for actual order (BGR)!
                    int bIndex = x * bytesPerPixel;
                    int gIndex = bIndex + 1;
                    int rIndex = bIndex + 2;
                    byte pixelR = row[rIndex];
                    byte pixelG = row[gIndex];
                    byte pixelB = row[bIndex];
                    row[rIndex] = row[bIndex] = row[gIndex] =
                    (searchedR <= pixelR  && pixelR <= backsearchedR) && (searchedG <= pixelG && pixelG <= backsearchedG) && (searchedB <= pixelB && pixelB <= backsearchedB) ?  matchingValue : unmatchingValue;

                }
            }

            image.UnlockBits(imageData);

            return (image);
            //pictureBox1.Image = image;
        }
        public Bitmap DisplayScan()
        {
            memoryImage = new Bitmap(s.Width, s.Height);

            memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(CutRect.X, CutRect.Y, 0, 0, s);
            originmat = BitmapConverter.ToMat(memoryImage);
            memoryImage = DetectColorPointWithUnsafe(memoryImage, Previous.R, Previous.G, Previous.B, Rear.R, Rear.G, Rear.B);

            if (true)
            {
                Mat mat = new Mat();
                mat = BitmapConverter.ToMat(memoryImage);
                //Cv2.Dilate(mat, mat, noise);
                Mat grayMat = mat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Mat mat2 = grayMat.Threshold(230, 255, ThresholdTypes.BinaryInv);
                OpenCvSharp.Point[][] contours;
                OpenCvSharp.HierarchyIndex[] hierarchyIndexes;
                mat2.FindContours(out contours, out hierarchyIndexes, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                rect = new Rect(mat.Width / 2 - fov / 2, mat.Height / 2 - fov / 2, fov, fov);
                //mat.DrawContours(contours, -1,new Scalar(255,0,0,255), 2);
                foreach (var contour in contours)
                {
                    area = Cv2.ContourArea(contour,false);
                    //修正ポイント
                    if (contour.Length < maxvertex && contour.Length > minvertex && area > minarena && area < 270000) 
                    {
                        targetrect = Cv2.BoundingRect(contour);
                        Cv2.Rectangle(imagemode == 0 ? originmat : mat, targetrect, getDistance(mat.Width / 2, mat.Height / 2, targetrect.Left + targetrect.Width / 2, targetrect.Top + targetrect.Height / 2) < fov ? greenscaler : bluescaler, 1);
                        Cv2.Line(imagemode == 0 ? originmat : mat, mat.Width / 2, 0, targetrect.Left + targetrect.Width / 2, targetrect.Top + targetrect.Height / 2, getDistance(mat.Width / 2, mat.Height / 2, targetrect.Left + targetrect.Width / 2, targetrect.Top + targetrect.Height / 2) < fov ? greenscaler : bluescaler, 1);

                        Cv2.PutText(imagemode == 0 ? originmat : mat, "vertex:" + contour.Length.ToString(), new OpenCvSharp.Point(targetrect.Left, targetrect.Top + 10 + 15), (HersheyFonts)2, 1, blackscaler);
                        Cv2.PutText(imagemode == 0 ? originmat : mat, "Size:" + area.ToString(), new OpenCvSharp.Point(targetrect.Left, targetrect.Top + 20 + 35), (HersheyFonts)2, 1, blackscaler);

                    }
                    //面積を算出
                    if (true)
                    {
                        Cv2.PutText(imagemode == 0 ? originmat : mat, "vertex:" + contour.Length.ToString(), new OpenCvSharp.Point(targetrect.Left, targetrect.Top + targetrect.Height + 15), (HersheyFonts)2, 1, blackscaler);
                        Cv2.PutText(imagemode == 0 ? originmat : mat, "Size:" + area.ToString(), new OpenCvSharp.Point(targetrect.Left, targetrect.Top + targetrect.Height + 35), (HersheyFonts)2, 1, blackscaler);

                    }
                }
                Cv2.Circle(imagemode == 0 ? originmat : mat, mat.Width / 2, mat.Height / 2, fov, bluescaler, 1);
                memoryImage = BitmapConverter.ToBitmap(imagemode == 0 ? originmat : mat);
                mat.Dispose();
                mat2.Dispose();
                grayMat.Dispose();
                memoryGraphics.Dispose();
            }
            return memoryImage;
        }

        protected int getDistance(double x, double y, double x2, double y2)
        {
            double distance = Math.Sqrt((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y));

            return (int)distance;
        }
    }
}
