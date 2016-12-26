using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace CPOO_disparity
{
    public unsafe class ProcessableBitmap : IDisposable
    {
        private Bitmap _bitmap;
        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }
        private BitmapData _bitmapData;
        public int Width
        {
            get { return _bitmap.Width; }
        }
        public int Height
        {
            get { return _bitmap.Height; }
        }
        private int bytesPerPixel;
        private byte* PtrFirstPixel;

        bool disposed = false;

        public ProcessableBitmap(Bitmap bitmap)
        {
            _bitmap = bitmap;
            _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadWrite, _bitmap.PixelFormat);
            bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(_bitmap.PixelFormat) / 8;
            PtrFirstPixel = (byte*)_bitmapData.Scan0;
        }

        [HandleProcessCorruptedStateExceptions]
        public Rgb GetPixel(int x, int y)
        {
            try
            {
                byte* currentLine = PtrFirstPixel + (y * _bitmapData.Stride);
                int XInPixelsMap = x * bytesPerPixel;
                int b = currentLine[XInPixelsMap];
                int g = currentLine[XInPixelsMap + 1];
                int r = currentLine[XInPixelsMap + 2];
                return new Rgb(r, g, b);
            }
            catch (AccessViolationException ex)
            {
                Dispose();
                return new Rgb(0, 0, 0);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public void SetPixel(int x, int y, Rgb rgb)
        {
            try
            {
                byte* currentLine = PtrFirstPixel + (y * _bitmapData.Stride);
                int XInPixelsMap = x * bytesPerPixel;
                currentLine[XInPixelsMap] = (byte)rgb.B;
                currentLine[XInPixelsMap + 1] = (byte)rgb.G;
                currentLine[XInPixelsMap + 2] = (byte)rgb.R;
            }
            catch (AccessViolationException ex)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _bitmap.UnlockBits(_bitmapData);
            }
            disposed = true;
        }
    }

    public class Rgb
    {
        public int R;
        public int G;
        public int B;
        public Rgb(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Rgb(int val)
        {
            R = val;
            G = val;
            B = val;
        }
    }
}
