using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace CPOO_disparity
{
    public class DisparityLeft : Disparity
    {       
        protected override void GenerateSADMap()
        {
            _disparityMap = new double[ImageWidth, ImageHeight];

            using (ProcessableBitmap prLeft = new ProcessableBitmap(leftBitmap))
            using (ProcessableBitmap prRight = new ProcessableBitmap(rightBitmap))
            {
                int widthL = prLeft.Width;
                int widthR = prRight.Width;
                int maxDepthByte = depthForCalc;

                int step = 1;

                Parallel.For(0, heightInPixels, y =>
                {
                    for (int x = 0; x < widthL; x++)
                    {
                        int error = Int32.MaxValue;
                        double tempDisp = 0;
                        int tempAmb = 0;

                        int xOnR = x;

                        for (int xScan = Math.Max(xOnR - maxDepthByte, 0); xScan < xOnR; xScan += step)
                        {
                            int[,] errorsArr = new int[options.MaskSize, options.MaskSize];
                            int tmpErr = 0;
                            //y block for
                            for (int yi = Math.Max(y - nbhRadius, 0); yi <= Math.Min(y + nbhRadius, heightInPixels - 1); yi++)
                            {
                                //x block for
                                for (int xi = 0 - nbhRadius; xi <= nbhRadius; xi++)
                                {
                                    var diff = xi;
                                    //in interpolation it's needed to compare only chosen pixels in mask, not to widen the mask
                                    var diffR = diff;


                                    if ((x + diff >= 0) && (xScan + diffR >= 0) && (x + diff < widthL) && (xScan + diffR < widthR))
                                    {
                                        var left = prLeft.GetPixel(x + diff, yi);
                                        var right = prRight.GetPixel(xScan + diffR, yi);
                                        errorsArr[options.MaskSize / 2 + xi, options.MaskSize / 2 + yi - y] = Math.Abs(left.B - right.B) + Math.Abs(left.G - right.G) + Math.Abs(left.R - right.R);                                   
                                    }                                 
                                }
                            }

                            tmpErr = SumElements(errorsArr, options.MaskSize);

                            if (tmpErr == error)
                                tempAmb++;
                            if (tmpErr < error)
                                tempAmb = 0;
                            if (tmpErr <= error) //The last best is taken - lower disparity value
                            {
                                double theScannedX = xScan;
                                error = tmpErr;
                                tempDisp = (x - theScannedX);
                            }
                        }

                        _disparityMap[x, y] = tempDisp;
                        if (tempDisp > maxValue)
                            maxValue = tempDisp;
                    }
                });
            }
        }

        protected override void SetBitmaps()
        {

            leftBitmap = options.LeftBitmap;
            rightBitmap = options.RightBitmap;
        }
    }
}
