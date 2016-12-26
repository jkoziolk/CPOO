using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.ExceptionServices;


namespace CPOO_disparity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Bitmap bitmapL;
        private Bitmap bitmapR;
        private Bitmap bitmapRes;
        private bool InWork = false;
        private int maxDepth = 64;
        private int maskSize = 5;

        private void Open_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenDialog openDialog = new OpenDialog();
            if (openDialog.ShowDialog() == true)
            {
                OpenBitmaps(openDialog);
                BitmapsToUI();
                Reset();
            }
        }

        private void OpenBitmaps(OpenDialog dlg)
        {
            bitmapL = OpenBmpFromDir(dlg.leftPath);
            bitmapR = OpenBmpFromDir(dlg.rightPath);
        }

        private Bitmap OpenBmpFromDir(String dir)
        {
            if (dir.EndsWith(".png"))
                return new Bitmap(dir);
            else
                throw new InvalidDataException("Only png images are valid.");
        }

        private void BitmapsToUI()
        {
            BitmapImage left = BitmapToBitmapImageConverter.Convert(bitmapL);
            LeftImage.Source = left;

            BitmapImage right = BitmapToBitmapImageConverter.Convert(bitmapR);
            RightImage.Source = right;
        }

        private void Reset()
        {
            ResultImage.Source = null;
            bitmapRes = null;
        }


        private void Save_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private async void Compute_Click(object sender, RoutedEventArgs e)
        {
            if (bitmapL != null && bitmapR != null && !InWork)
            {
                InWork = true;
                //GetParametersFromUI();
                IndicateStart();

                await MakeCalculationsForLeft();
                ResultImage.Source = BitmapToBitmapImageConverter.Convert(bitmapRes);

                IndicateStop();
                InWork = false;
            }
        }

        private void IndicateStart()
        {
            IndicatorText.Text = "Computations in progress";
            IndicatorReady.Visibility = System.Windows.Visibility.Collapsed;
            IndicatorProcess.Visibility = System.Windows.Visibility.Visible;
        }

        private void IndicateStop()
        {
            IndicatorText.Text = "Ready";
            IndicatorReady.Visibility = System.Windows.Visibility.Visible;
            IndicatorProcess.Visibility = System.Windows.Visibility.Collapsed;
        }

        private async Task MakeCalculationsForLeft()
        {
            DisparityCalculationOptions options = SetDisparityCalculationOptions();

            var dsp = new DisparityLeft();
            bitmapRes = await CalculateDisparityAsync(dsp, options);
        }

        private DisparityCalculationOptions SetDisparityCalculationOptions()
        {
            DisparityCalculationOptions options = new DisparityCalculationOptions();
            options.LeftBitmap = bitmapL;
            options.RightBitmap = bitmapR;
            options.MaxDepth = maxDepth;
            options.MaskSize = maskSize;

            return options;
        }

        private async Task<Bitmap> CalculateDisparityAsync(Disparity disp, DisparityCalculationOptions options)
        {
            return await Task.Run(() =>
            {
                return disp.CalculateDisparity(options);

            });
        }
    }
}
