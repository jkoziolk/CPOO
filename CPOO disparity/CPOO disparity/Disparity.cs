using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Runtime.CompilerServices;

namespace CPOO_disparity
{
    public unsafe class Disparity
    {
        protected double[,] _disparityMap;
        public double[,] disparityMap
        {
            get { return _disparityMap; }
        }
        protected int _imageWidth;
        public int ImageWidth
        {
            get { return _imageWidth; }
        }
        protected int _imageHeight;
        public int ImageHeight
        {
            get { return _imageHeight; }
        }
        private PixelFormat _imagePixelFormat;
        public PixelFormat ImagePixelFormat
        {
            get { return _imagePixelFormat; }
        }
        private Bitmap _resultImage;
        public Bitmap ResultImage
        {
            get { return _resultImage; }
        }
        protected double maxValue = 1;
        public DisparityCalculationOptions options;
        protected Bitmap leftBitmap;
        protected Bitmap rightBitmap;
        protected int depthForCalc;
        protected int bytesPerPixel;
        protected int heightInPixels;
        protected int widthInPixels;
        protected int widthInBytesR;
        protected int widthInBytesL;
        protected BitmapData bitmapDataL;
        protected BitmapData bitmapDataR;
        protected byte* PtrFirstPixelL;
        protected byte* PtrFirstPixelR;
        protected int nbhRadius;


        public Disparity()
        {
            _imageWidth = 100;
            _imageHeight = 100;
            _imagePixelFormat = PixelFormat.Undefined;
        }


        public Bitmap CalculateDisparity(DisparityCalculationOptions passedOptions)
        {
            InitializeFieldsFromOptions(passedOptions);
            GenerateDisparityMap();
            MapToBitmapConverter mapToBmp = new MapToBitmapConverter(ImageWidth, ImageHeight, ImagePixelFormat);
            _resultImage = mapToBmp.GenerateRangedImageFromMap(disparityMap, maxValue);

            return ResultImage;
        }

        private void InitializeFieldsFromOptions(DisparityCalculationOptions passedOptions)
        {
            options = passedOptions;
            _imageWidth = passedOptions.LeftBitmap.Width;
            _imageHeight = passedOptions.LeftBitmap.Height;
            _imagePixelFormat = passedOptions.LeftBitmap.PixelFormat;
            depthForCalc = passedOptions.MaxDepth;
            SetBitmaps();
            bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(leftBitmap.PixelFormat) / 8;
            heightInPixels = leftBitmap.Height;
            widthInPixels = leftBitmap.Width;
            widthInBytesR = rightBitmap.Width * bytesPerPixel;
            widthInBytesL = leftBitmap.Width * bytesPerPixel;
            nbhRadius = (int)Math.Floor((double)options.MaskSize / 2);
        }

        private void GenerateDisparityMap()
        {
                GenerateSADMap();
        }

        protected virtual void GenerateSADMap()
        {
            throw new NotImplementedException();
        }

        protected int SumElements(int[,] arr, int size)
        {
            int sum = 0;
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++ )
                    sum += arr[i,j];
            
            return sum;
        }

        protected virtual void SetBitmaps()
        {
            throw new NotImplementedException("You must override this method in the child class");
        }

        private Rgb GetRgb(byte* PtrFirst, BitmapData bmpData, int x, int y)
        {
            int Blue = (PtrFirst + (y * bmpData.Stride))[x];
            int Green = (PtrFirst + (y * bmpData.Stride))[x + 1];
            int Red = (PtrFirst + (y * bmpData.Stride))[x + 2];
            return new Rgb(Red, Green, Blue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int CensusComparison(int main, int nbh)
        {
            if (nbh < main)
                return 1;
            else
                return 0;
        }
    }
}
