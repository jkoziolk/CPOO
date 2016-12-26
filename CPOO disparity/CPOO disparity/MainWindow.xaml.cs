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
        private bool inWork = false;
        private int maxDepth = 64;
        private int maskSize = 5;
        private bool postprocess = true;
        private bool pseudocolor = true;
        private double imgEdg;
        private double dispEdg;

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
            if (bitmapRes != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Image Files (*.png)|*.png;"; ;
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (saveFileDialog1.ShowDialog() == true)
                {
                    bitmapRes.Save(saveFileDialog1.FileName);
                }
            }
        }

        private async void Compute_Click(object sender, RoutedEventArgs e)
        {
            if (bitmapL != null && bitmapR != null && !inWork)
            {
                inWork = true;
                GetParametersFromUI();
                IndicateStart();

                await MakeCalculationsForLeft();
                ResultImage.Source = BitmapToBitmapImageConverter.Convert(bitmapRes);

                IndicateStop();
                inWork = false;
            }
        }

        private void GetParametersFromUI()
        {
            maxDepth = Int32.Parse(MaxDepth.SelectionBoxItem as String);
            maskSize = Int32.Parse(MaskSize.SelectionBoxItem as String);
            postprocess = (bool)Postprocess.IsChecked;
            pseudocolor = (bool)Pseudocolor.IsChecked;
            imgEdg = ImgSld.Value;
            dispEdg = DispSld.Value;
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

            if(postprocess)
            {
                await Task.Run(() =>
                {
                    Bitmap mask = Postprocessor.ColorGradCmp(bitmapL, bitmapRes, imgEdg, dispEdg, maxDepth);
                    bitmapRes = Postprocessor.MedianOnMask(bitmapRes, mask);
                });
            }

            if (pseudocolor)
            {
                await Task.Run(() =>
                {
                    bitmapRes = Postprocessor.Pseudocolor(bitmapRes);
                });
            }
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
