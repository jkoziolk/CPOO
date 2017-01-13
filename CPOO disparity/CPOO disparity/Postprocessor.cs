using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPOO_disparity
{
    public static class Postprocessor
    {
        public static Bitmap EdgesOnImg { get; private set; }
        public static Bitmap EdgesOnDisp { get; private set; }

        public static Bitmap ColorGradCmp(Bitmap img, Bitmap disp, double treshold, double dispTreshold, int maxVal = 64)
        {
            int w = img.Width;
            int h = img.Height;
            int maskSize = 3;
            int nbhR = maskSize / 2;

            dispTreshold = 1 + dispTreshold / maxVal * 254;

            Bitmap imgEdg = new Bitmap(w, h, img.PixelFormat);
            Bitmap dispEdg = new Bitmap(w, h, img.PixelFormat);
            Bitmap res = new Bitmap(w, h, img.PixelFormat);

            Action imgLaplace = new Action(() =>
            {
                using (ProcessableBitmap inImg = new ProcessableBitmap(img))
                using (ProcessableBitmap outImg = new ProcessableBitmap(imgEdg))
                    Parallel.For(nbhR, h - nbhR, y =>
                    {
                        for (int x = nbhR; x < w - nbhR; x++)
                        {
                            //edge by Laplace
                            int edgLap = Math.Abs(
                                inImg.GetPixel(x, y - 1).R +
                                inImg.GetPixel(x - 1, y).R - 4 * inImg.GetPixel(x, y).R + inImg.GetPixel(x + 1, y).R +
                                inImg.GetPixel(x, y + 1).R
                                );
                            edgLap += Math.Abs(
                                inImg.GetPixel(x, y - 1).G +
                                inImg.GetPixel(x - 1, y).G - 4 * inImg.GetPixel(x, y).G + inImg.GetPixel(x + 1, y).G +
                                inImg.GetPixel(x, y + 1).G
                                );
                            edgLap += Math.Abs(
                                inImg.GetPixel(x, y - 1).B +
                                inImg.GetPixel(x - 1, y).B - 4 * inImg.GetPixel(x, y).B + inImg.GetPixel(x + 1, y).B +
                                inImg.GetPixel(x, y + 1).B
                                );
                            edgLap /= 3;

                            if (edgLap > treshold)
                                outImg.SetPixel(x, y, new Rgb(255));
                        }
                    });
            });

            Action dispGrad = new Action(() =>
            {
                using (ProcessableBitmap inDisp = new ProcessableBitmap(disp))
                using (ProcessableBitmap outImg = new ProcessableBitmap(dispEdg))
                    Parallel.For(nbhR, h - nbhR, y =>
                    {
                        for (int x = nbhR; x < w - nbhR; x++)
                        {
                            int preGradY = Math.Abs(-inDisp.GetPixel(x, y - 1).R + inDisp.GetPixel(x, y).R);
                            int postGradY = Math.Abs(inDisp.GetPixel(x, y).R - inDisp.GetPixel(x, y + 1).R);
                            int preGradX = Math.Abs(-inDisp.GetPixel(x - 1, y).R + inDisp.GetPixel(x, y).R);
                            int postGradX = Math.Abs(inDisp.GetPixel(x, y).R - inDisp.GetPixel(x + 1, y).R);
                            int LtC = Math.Abs(inDisp.GetPixel(x - 1, y - 1).R - inDisp.GetPixel(x, y).R);
                            int CrB = Math.Abs(inDisp.GetPixel(x, y).R - inDisp.GetPixel(x + 1, y + 1).R);
                            int LbC = Math.Abs(inDisp.GetPixel(x - 1, y + 1).R - inDisp.GetPixel(x, y).R);
                            int CrT = Math.Abs(inDisp.GetPixel(x, y).R - inDisp.GetPixel(x + 1, y - 1).R);

                            if (preGradY > dispTreshold || postGradY > dispTreshold ||
                                preGradX > dispTreshold || postGradX > dispTreshold ||
                                LtC > dispTreshold || CrB > dispTreshold ||
                                LbC > dispTreshold || CrT > dispTreshold)
                                outImg.SetPixel(x, y, new Rgb(255));
                        }
                    });
            });

            Action[] actions = new Action[] { imgLaplace, dispGrad };
            Parallel.Invoke(actions);

            EdgesOnImg = imgEdg;
            EdgesOnDisp = dispEdg;

            using (ProcessableBitmap inImg = new ProcessableBitmap(imgEdg))
            using (ProcessableBitmap inDisp = new ProcessableBitmap(dispEdg))
            using (ProcessableBitmap outImg = new ProcessableBitmap(res))
                Parallel.For(nbhR, h - nbhR, y =>
                {
                    for (int x = nbhR; x < w - nbhR; x++)
                    {
                        if (inImg.GetPixel(x, y).R == 0 && inDisp.GetPixel(x, y).R == 255)
                            outImg.SetPixel(x, y, new Rgb(255));
                    }
                });

            return res;
        }

        public static Bitmap MedianOnMask(Bitmap img, Bitmap mask, int maskSize)
        {
            int w = img.Width;
            int h = img.Height;
            int nbhR = maskSize / 2;
            Bitmap res = new Bitmap(img);

            using (ProcessableBitmap inImg = new ProcessableBitmap(img))
            using (ProcessableBitmap inMask = new ProcessableBitmap(mask))
            using (ProcessableBitmap outImg = new ProcessableBitmap(res))
                Parallel.For(nbhR, h - nbhR, y =>
                {
                    for (int x = nbhR; x < w - nbhR; x++)
                    {
                        if (inMask.GetPixel(x, y).R == 255)
                        {
                            int itr = 2;
                            List<int> vector = ElementaryMedian(inImg, inMask, x, y, w, h, maskSize);
                            while (vector.Count <= 1)
                            {
                                vector = ElementaryMedian(inImg, inMask, x, y, w, h, maskSize + itr);
                                itr += 2;
                            }

                            if (vector.Count > 1)
                                outImg.SetPixel(x, y, new Rgb(vector[vector.Count / 2]));
                        }
                    }
                });

            return res;
        }
        private static List<int> ElementaryMedian(ProcessableBitmap inImg, ProcessableBitmap inMask, int x, int y, int w, int h, int maskSize)
        {
            int nbhR = maskSize / 2;
            List<int> vector = new List<int>();
            for (int i = -nbhR; i <= nbhR; i++)
                for (int j = -nbhR; j <= nbhR; j++)
                {
                    if (x + i < 0 || y + j < 0 || x + i >= w || y + j >= h)
                        return new List<int>();
                    if (inMask.GetPixel(x + i, y + j).R == 0)
                        vector.Add(inImg.GetPixel(x + i, y + j).R);
                }
            vector.Sort();
            return vector;
        }

        public static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        public static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }

        public static Bitmap Pseudocolor(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap res = new Bitmap(img);

            using (ProcessableBitmap inImg = new ProcessableBitmap(img))
            using (ProcessableBitmap outImg = new ProcessableBitmap(res))
                Parallel.For(0, h, y =>
                {
                    for (int x = 0; x < w; x++)
                    {
                        int black = inImg.GetPixel(x, y).G;
                        double factor = black / 255.0;
                        //scale invertion
                        factor = factor * (-1) + 1;
                        double hue = 360.0 * factor;
                        //270 istead of 360 gives better results
                        hue = 270.0 * factor;

                        double sat = 0.8;
                        double val = 1;

                        int r = 0;
                        int g = 0;
                        int b = 0;

                        HsvToRgb(hue, sat, val, out r, out g, out b);
                        outImg.SetPixel(x, y, new Rgb(r, g, b));
                    }
                });

            return res;
        }
    }
}
