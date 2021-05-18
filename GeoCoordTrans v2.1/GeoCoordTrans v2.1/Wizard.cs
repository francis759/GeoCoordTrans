using GeoCoordTrans_v2._1.Properties;
using Microsoft.Win32;
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
    public partial class Wizard : Form
    {
        RegistryKey Key;

        public Wizard()
        {
            InitializeComponent();
            listBox1.DataSource = Helper.GetTable("Wizard");
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Size),
                SystemColors.ControlLight, SystemColors.ControlLightLight, (sender as Control).Tag.ToString() == "0" ? LinearGradientMode.ForwardDiagonal : LinearGradientMode.BackwardDiagonal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
        }

        private void Wizard_Load(object sender, EventArgs e)
        {
            Key = Registry.CurrentUser.CreateSubKey(Settings.Default.register);
            if (Key != null)
                if (Key.GetValue("SelectedWizard") != null)
                    listBox1.SelectedIndex = int.Parse(Key.GetValue("SelectedWizard").ToString());
                else
                    listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label7.Text = listBox1.GetItemText(listBox1.SelectedItem);
            label6.Text = listBox1.SelectedValue.ToString();
        }

        private void Wizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            Key = Registry.CurrentUser.CreateSubKey(Settings.Default.register);
            Key.SetValue("SelectedWizard", listBox1.SelectedIndex.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch (listBox1.SelectedIndex)
            {
                case 0:
                    Csvform csvform = new Csvform();
                    csvform.ShowDialog();
                    break;
                case 1:
                    MdbForm mdbForm = new MdbForm();
                    mdbForm.ShowDialog();
                    break;
                default:
                    break;
            }
        }
    }
}
