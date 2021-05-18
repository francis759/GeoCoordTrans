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
    public partial class gForm : Form
    {
        public string region { get; set; }
        public string grid { get; set; }

        public gForm(string _region, string _grid)
        {
            InitializeComponent();
            region = _region;
            grid = _grid;
        }

        private void gForm_Load(object sender, EventArgs e)
        {
            Helper.GridFormLoad(_regiontree, region, _gridtree, grid);
        }

        private void _regiontree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Helper.PopulateGrids(_regiontree, _gridtree);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_gridtree.SelectedNode == null)
                MessageBox.Show("No Grid Selected");
            else
            {
                if (_gridtree.SelectedNode.Parent != null)
                {
                    region = Helper.GetRegionID(_regiontree.SelectedNode.Text);
                    grid = Helper.GetGridID(_gridtree.SelectedNode.Text);
                    DialogResult = DialogResult.OK;
                }
            }  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
