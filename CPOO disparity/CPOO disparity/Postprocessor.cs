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


        public static Bitmap MedianOnMask(Bitmap img, Bitmap mask)
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap res = new Bitmap(img);

            using (ProcessableBitmap inImg = new ProcessableBitmap(img))
            using (ProcessableBitmap inMask = new ProcessableBitmap(mask))
            using (ProcessableBitmap outImg = new ProcessableBitmap(res))
                Parallel.For(2, h - 2, y =>
                {
                    for (int x = 2; x < w - 2; x++)
                    {
                        if (inMask.GetPixel(x, y).R == 255)
                        {
                            List<int> vector = new List<int>();
                            for (int i = -2; i <= 2; i++)
                                for (int j = -2; j <= 2; j++)
                                    vector.Add(inImg.GetPixel(x + i, y + j).R);

                            vector.Sort();
                            if (vector.Count > 0)
                                outImg.SetPixel(x, y, new Rgb(vector[vector.Count / 2]));
                        }
                    }
                });

            return res;
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
                    for (int x = 0; x < w ; x++)
                    {
                        //real pseudocolor function is needed
                        //outImg.SetPixel(x, y, new Rgb(inImg.GetPixel(x,y).R, 0,0));

                        int black = inImg.GetPixel(x, y).G;
                      

                        double factor = black / 255.0;
                        int red = 0;
                        int green = 0;
                        int blue = 0;
                        double aux = 0;
                        if (factor < 0.5)
                        {
                            factor = factor * 2.0;
                            if (factor == 0)
                            {
                                red = 255;
                            }
                            else
                            {
                                aux = ((factor*(-1.0))+1.0) * 255.0;
                                red = Convert.ToInt32(aux);
                            }
                            aux = 255.0 * factor;
                            green = Convert.ToInt32(aux);

                        }
                        else if(factor > 0.5)
                        {
                            factor = (factor - 0.5)*2.0;
                            aux = ((factor * (-1.0)) + 1.0) * 255.0;
                            green = Convert.ToInt32(aux);
                            aux = 255.0 * factor;
                            blue = Convert.ToInt32(aux);
                        }
                        else
                        {
                            green = 255;
                        }
                        outImg.SetPixel(x, y, new Rgb(red, green, blue));
                    }
                });

            return res;
        }
    }
}
