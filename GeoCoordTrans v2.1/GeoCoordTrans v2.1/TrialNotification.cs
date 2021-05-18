using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoCoordTrans_v2._1
{
    public partial class TrialNotification : Form
    {
        public TrialNotification(string message, string _status)
        {
            InitializeComponent();
            label3.Text = message;
            label1.Text = _status;
            if (_status == "EXPIRED")
                label1.ForeColor = Color.IndianRed;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Size),
                SystemColors.ControlLight, SystemColors.ControlLightLight, LinearGradientMode.ForwardDiagonal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void label6_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
