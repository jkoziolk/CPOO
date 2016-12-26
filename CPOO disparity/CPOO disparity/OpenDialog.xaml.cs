using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CPOO_disparity
{
    public partial class OpenDialog : Window
    {
        public OpenDialog()
        {
            InitializeComponent();
        }

        private String _leftPath = "";
        public String leftPath
        {
            get { return _leftPath; }
        }

        private String _rightPath = "";
        public String rightPath
        {
            get { return _rightPath; }
        }

        private string initialDirectory = "C:\\";

        private void OpenLeft_Click(object sender, RoutedEventArgs e)
        {
            var imageDialog = OpenImageDialog();
            bool? userClickedOK = imageDialog.ShowDialog();

            if (userClickedOK == true)
            {
                _leftPath = imageDialog.FileName;
                LeftPath.Text = imageDialog.FileName;
                System.IO.FileInfo fInfo = new System.IO.FileInfo(_leftPath);
                initialDirectory = fInfo.DirectoryName;
            }
        }

        private void OpenRight_Click(object sender, RoutedEventArgs e)
        {
            var imageDialog = OpenImageDialog();
            bool? userClickedOK = imageDialog.ShowDialog();

            if (userClickedOK == true)
            {
                _rightPath = imageDialog.FileName;
                RightPath.Text = imageDialog.FileName;
                System.IO.FileInfo fInfo = new System.IO.FileInfo(_rightPath);
                initialDirectory = fInfo.DirectoryName;
            }
        }


        private OpenFileDialog OpenImageDialog()
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = initialDirectory;
            openFileDialog1.Filter = "Image Files (*.png)|*.png;";
            openFileDialog1.Multiselect = false;
            return openFileDialog1;
        }


        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (_leftPath != "" && _rightPath != "")
                this.DialogResult = true;
            else
                this.DialogResult = false;
        }
    }
}
