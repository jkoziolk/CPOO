using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPOO_disparity
{
    public class DisparityCalculationOptions
    {        
        public Bitmap LeftBitmap { get; set; }
        public Bitmap RightBitmap { get; set; }
        public int MaxDepth { get; set; }
        public int MaskSize { get; set; }

        public DisparityCalculationOptions()
        {
            MaxDepth = 64;
            MaskSize = 5;
        }

        public DisparityCalculationOptions(DisparityCalculationOptions opts)
        {
            MaxDepth = opts.MaxDepth;
            MaskSize = opts.MaskSize;
            LeftBitmap = new Bitmap(opts.LeftBitmap);
            RightBitmap = new Bitmap(opts.RightBitmap);
        }
    }
}
