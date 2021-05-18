using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoCoordTrans_v2._1
{
    public partial class NewRegionForm : Form
    {
        Random rn = new Random();
        private const int counter = 4;
        public string RegionName { get; set; }
        public string Id { get; set; }
        public NewRegionForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    Id = GenerateId();
                    RegionName = textBox1.Text;
                    DialogResult = DialogResult.OK;
                }
                else
                    throw new Exception("Invalid region name, Please enter valid region name");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string GenerateId()
        {
            string _id = string.Empty;
            for (int i = 0; i < counter; i++)
                _id += (char)rn.Next(65, 90);
            //
            if (IDExist(_id) == true)
                GenerateId();

            return _id;
        }

        private bool IDExist(string id)
        {
            DataTable dt = Helper.GetTable("Region");
            List<string> Ids = dt.AsEnumerable().Select(x => x[0].ToString()).ToList();
            foreach (string v in Ids)
                if (v == id)
                    return true;

            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
