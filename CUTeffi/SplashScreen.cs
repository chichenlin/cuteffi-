using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace CUTeffi
{
    public partial class SplashScreen : Form
    {
        public static CUTeffiForm cut;
        public SplashScreen()
        {
            InitializeComponent();
            
        }

        private void SplashScreen_Shown(object sender, EventArgs e)
        {
            Refresh();
            Thread.Sleep(1000);
            //CUTeffiForm cut = new CUTeffiForm();
            cut = new CUTeffiForm();
            cut.FSS = this; 
            cut.Show();
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(0, 171, 230);
            this.TransparencyKey = Color.FromArgb(0, 171, 230);
        }
    }
}
