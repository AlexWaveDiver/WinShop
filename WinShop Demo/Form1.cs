using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinShop;

namespace WinShop_Demo
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnUnpack_Click(object sender, EventArgs e)
        {
            var winFileDialog = new OpenFileDialog();
            winFileDialog.Filter = "GM8 Winfile (*.win) | *.win";

            var outputFolderDialog = new FolderBrowserDialog();

            if (winFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (outputFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string winFile = winFileDialog.FileName;
                    string outputFolder = outputFolderDialog.SelectedPath;

                    WinShop.Unpacking.Unpack(winFile, outputFolder);

                    var result = MessageBox.Show("Do you want to open the destination folder?", "Unpacking finished", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        Process.Start(@outputFolder);
                    }
                    System.Windows.Forms.Application.Exit();
                }
            }
        }

        private void btnRepack_Click(object sender, EventArgs e)
        {

            var winFileDialog = new SaveFileDialog();
            winFileDialog.Filter = "GM8 Winfile (*.win) | *.win";

            var outputFolderDialog = new FolderBrowserDialog();

            if (outputFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (winFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string winFile = winFileDialog.FileName;
                    string inputFolder = outputFolderDialog.SelectedPath;

                    WinShop.Packing.Pack(inputFolder, winFile);

                    var result = MessageBox.Show("WinFile created sucessfully", "Packing finished", MessageBoxButtons.OK ,MessageBoxIcon.Information);
                    System.Windows.Forms.Application.Exit();
                }
            }
        }
    }
}
