using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoCoordTrans_v2._1
{
    public partial class dForm : Form
    {
        public string region { get; set; }
        public string datum { get; set; }

        public dForm(string _region, string _datum)
        {
            InitializeComponent();
            region = _region;
            datum = _datum;
        }

        private void dForm_Load(object sender, EventArgs e)
        {
            Helper.DatumFormLoad(_regiontree, region, dbox, datum);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void _regiontree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Helper.PopulateDatums(_regiontree, dbox);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dbox.SelectedItem != null)
            {
                region = Helper.GetRegionID(_regiontree.SelectedNode.Text);
                datum = Helper.GetDatumID(dbox.GetItemText(dbox.SelectedItem));
                DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show("No Datum Selected");
        }
    }
}
