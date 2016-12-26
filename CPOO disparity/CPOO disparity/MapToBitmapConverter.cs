using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPOO_disparity
{
    public class MapToBitmapConverter
    {
        private int Width;
        private int Height;
        private PixelFormat Format;
        public MapToBitmapConverter(int width, int height, PixelFormat format)
        {
            Width = width;
            Height = height;
            Format = format;
        }

        public Bitmap GenerateRangedImageFromMap(double[,] disparityMap, double maxValue)
        {
            Bitmap result = new Bitmap(Width, Height, Format);

            using (ProcessableBitmap Result = new ProcessableBitmap(result))
            {
                Parallel.For(0, Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int disparityVal = (int)(1 + disparityMap[x,y] / maxValue * 254);
                        Result.SetPixel(x, y, new Rgb(disparityVal));
                    }
                });
            }
            return result;
        }
    }
    
}
