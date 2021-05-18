using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoCoordTrans_v2._1
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Size),
                SystemColors.ControlLight, SystemColors.ControlLightLight, (sender as Control).Tag.ToString() == "0" ? LinearGradientMode.ForwardDiagonal : LinearGradientMode.BackwardDiagonal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
        }

        private void dev_click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (Control c in Helper.GetControl(Controls))
                if (c is Panel && c.Name == (sender as LinkLabel).Tag.ToString())
                    c.BringToFront();
        }

        private void mail_me(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(string.Format("mailto:{0}", (sender as LinkLabel).Text));
        }
    }
}
