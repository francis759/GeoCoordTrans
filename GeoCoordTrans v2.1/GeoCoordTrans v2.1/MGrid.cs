using GeoCoordTrans_v2._1.Properties;
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
    public partial class MGrid : Form
    {
        public MGrid()
        {
            InitializeComponent();
        }

        private void MGrid_Load(object sender, EventArgs e)
        {
            Helper.PopulateRegion(treeView1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
                CreateRegion(treeView1.SelectedNode);
            else
                MessageBox.Show("Select region to which you want to add new region", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void CreateRegion(TreeNode m)
        {
            NewRegionForm rgForm = new NewRegionForm();
            if (rgForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int v;
                    Helper.InsertNewRegion(rgForm.Id, rgForm.RegionName, Helper.GetRegionID(m.Text), out v);
                    if (v == 1)
                    {
                        treeView1.Nodes.Clear();
                        Helper.RegionTable.Reset();
                        Helper.RegionTable = Helper.GetTable("Region");
                        Helper.PopulateRegion(treeView1);
                        foreach (TreeNode n in Helper.GetNode(treeView1.Nodes))
                            if (Helper.GetRegionID(n.Text) == rgForm.Id)
                                treeView1.SelectedNode = n;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView2.Nodes.Clear();
            string query = "SELECT a.[Name], b.[Name] AS Parent FROM Grid a, Datum b, DatumRegion c, Region d " +
                "WHERE d.[Name] = @p AND c.[Datum] = d.[ID] AND a.[Datum] = b.[ID] AND b.[ID] = c.[Datum]";
            using (OleDbConnection con = new OleDbConnection(Settings.Default.Constring))
            {
                using (OleDbDataAdapter da = new OleDbDataAdapter(query, con))
                {
                    da.SelectCommand.Parameters.AddWithValue(@"p", (sender as TreeView).SelectedNode.Text);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    //foreach (DataRow m in dt.Rows)
                    //{
                    //    if (!treeView2.Nodes.ContainsKey(m[1].ToString()))
                    //    {
                    //        TreeNode n = new TreeNode() { Name = m[1].ToString(), Text = m[1].ToString() };
                    //        GetChildren(dt, n);
                    //        tree2.Nodes.Add(n);
                    //    }
                    //}
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                string msg = string.Format("Are you sure you want to delete {0} and all sub regions?\n" +
                    "You will NOT be able to recover them.",treeView1.SelectedNode.Text);
                if (MessageBox.Show(msg, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    using (OleDbConnection con = new OleDbConnection(Settings.Default.Constring))
                    {
                        using (OleDbCommand cmd = new OleDbCommand("DELETE FROM Region WHERE [ID] = @p", con))
                        {
                            try
                            {
                                con.Open();
                                cmd.Parameters.AddWithValue(@"p", Helper.GetRegionID(treeView1.SelectedNode.Text));
                                int v = cmd.ExecuteNonQuery();
                                if (v == 1)
                                {
                                    treeView1.Nodes.Clear();
                                    Helper.RegionTable.Reset();
                                    Helper.RegionTable = Helper.GetTable("Region");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            finally
                            {
                                con.Close();
                            }
                        }
                    }
                }
                
            }
            else
                MessageBox.Show("Please select region", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
