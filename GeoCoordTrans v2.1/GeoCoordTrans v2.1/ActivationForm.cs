using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Microsoft.Win32;

namespace GeoCoordTrans_v2._1
{
    public partial class ActivationForm : Form
    {
        public ActivationForm()
        {
            InitializeComponent();
        }
        string getpassword;
        string regPath;

        public ActivationForm(string passname, string path)
        {
            InitializeComponent();
            getpassword = passname;
            regPath = path;
        }

        public bool PasswordEntry(string originalPass, string pass)
        {
            if (originalPass == pass)
            {
                RegistryKey regkey = Registry.CurrentUser;
                regkey = regkey.CreateSubKey(regPath); //path

                if (regkey != null)
                {
                    regkey.SetValue("Password", pass); //Value Name,Value Data
                }
                return true;
            }
            else
                return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if password true then send true			
            bool value = PasswordEntry(getpassword,textBox1.Text);
            if (value ==true)
            {
                MessageBox.Show("Activation Successful! Valid product key applied","Activate",MessageBoxButtons.OK,MessageBoxIcon.Information);
                button1.DialogResult = DialogResult.OK;
                DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show("Product Key is not valid! Please Enter a valid Product Key!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
			//----------------------------------------------		
		
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Size),
                SystemColors.Control, SystemColors.ControlLightLight, (sender as Control).Tag.ToString() == "0" ? LinearGradientMode.ForwardDiagonal : LinearGradientMode.BackwardDiagonal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
        }

        private void ActivationForm_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Size),
                SystemColors.Control, SystemColors.ControlLightLight,  LinearGradientMode.BackwardDiagonal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
        }
    }
}
