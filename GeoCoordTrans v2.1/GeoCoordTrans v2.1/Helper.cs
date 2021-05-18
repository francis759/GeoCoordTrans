using GeoCoordTrans_v2._1.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows.Forms;

namespace GeoCoordTrans_v2._1
{
    internal class Helper
    {
        internal static DataTable GridTable = GetTable("Grid");
        internal static DataTable DatumTable = GetTable("Datum");
        internal static DataTable RegionTable = GetTable("Region");

        internal static IEnumerable<Control> GetControl(Control.ControlCollection controls)
        {
            foreach (Control m in controls)
            {
                yield return m;
                foreach (Control c in GetControl(m.Controls))
                    yield return c;
            }
        }

        internal static DataTable GetTable(string v)
        {
            DataTable dt = new DataTable();
            using (OleDbConnection con = new OleDbConnection(Settings.Default.Constring))
            {
                using(OleDbDataAdapter da=new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", v), con))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        internal static string GetGridID(string v)
        {
            return GridTable.Select(string.Format("Name = '{0}'", v.Replace("'", "''")))[0][0].ToString();
        }

        internal static void PopulateGrids(TreeView tree, TreeView tree2)
        {
            tree2.Nodes.Clear();
            string query = "SELECT a.[Name], b.[Name] AS Parent FROM Grid a, Datum b, GridRegion c, Region d " +
                "WHERE d.[Name] = @p AND c.[Region] = d.[ID] AND a.[ID] = c.[Grid] AND b.[ID] = a.[Datum]";
            using (OleDbConnection con = new OleDbConnection(Settings.Default.Constring))
            {
                using (OleDbDataAdapter da = new OleDbDataAdapter(query, con))
                {
                    da.SelectCommand.Parameters.AddWithValue(@"p", tree.SelectedNode.Text);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow m in dt.Rows)
                    {
                        if (!tree2.Nodes.ContainsKey(m[1].ToString()))
                        {
                            TreeNode n = new TreeNode() { Name = m[1].ToString(), Text = m[1].ToString() };
                            GetChildren(dt, n);
                            tree2.Nodes.Add(n);
                        }
                    }
                }
            }
            tree2.Sort();
            tree2.ExpandAll();
        }

        internal static void InsertNewRegion(string id, string name, string ParentId, out int v)
        {
            using (OleDbConnection con=new OleDbConnection(Settings.Default.Constring))
            {
                using (OleDbCommand cmd = new OleDbCommand("INSERT INTO [Region] ([ID], [Name], [ParentID]) VALUES([p1],[p2],[p3])", con))
                {
                    con.Open();
                    cmd.Parameters.AddWithValue(@"p1", id);
                    cmd.Parameters.AddWithValue(@"p2", name);
                    cmd.Parameters.AddWithValue(@"p3", ParentId);
                    v = cmd.ExecuteNonQuery();
                }
            }
        }
        private static void GetChildren(DataTable dt, TreeNode n)
        {
            foreach (DataRow m in dt.Rows)
            {
                if (m[1].ToString() == n.Text)
                {
                    TreeNode c = new TreeNode() { Name = m[0].ToString(), Text = m[0].ToString() };
                    n.Nodes.Add(c);
                }
            }
        }

        internal static void PopulateRegion(TreeView tree)
        {
            DataTable dt = GetTable("Region");
            foreach (DataRow m in dt.Rows)
            {
                if (m[2].ToString() == "None")
                {
                    TreeNode n = new TreeNode(m[1].ToString());
                    string ParentId = m[0].ToString();
                    GetChildren(dt, ParentId, n);
                    tree.Nodes.Add(n);
                }
            }
            foreach (TreeNode n in GetNode(tree.Nodes))
                if (n.Text == "The Earth")
                    n.Expand();
            tree.Sort();
        }
        internal static void GridFormLoad(TreeView tree, string region, TreeView tree2, string grid)
        {
            DataTable dt = RegionTable;
            foreach (DataRow m in dt.Rows)
            {
                if (m[2].ToString() == "None")
                {
                    TreeNode n = new TreeNode(m[1].ToString());
                    string ParentId = m[0].ToString();
                    GetChildren(dt, ParentId, n);
                    tree.Nodes.Add(n);
                }
            }
            tree.Sort();
            foreach (TreeNode n in GetNode(tree.Nodes))
                if (n.Text == "The Earth")
                    n.Expand();
            foreach (TreeNode n in GetNode(tree.Nodes))
                if (n.Text == GetRegion(region))
                    tree.SelectedNode = n;

            foreach (TreeNode n in GetNode(tree2.Nodes))
                if (n.Text == GetGridName(grid) && n.Parent.Text == GetGridDatum(grid))
                    tree2.SelectedNode = n;
        }

        internal static string GetGridDatum(string v)
        {
            string datumid = GridTable.Select(string.Format("ID = '{0}'", v))[0][3].ToString();

            return GetDatum(datumid);
        }

        internal static string GetGridDatumID(string v)
        {
            return GridTable.Select(string.Format("ID = '{0}'", v))[0][3].ToString();
        }

        internal static string GetGridName(string v)
        {
            return GridTable.Select(string.Format("ID = '{0}'", v))[0][2].ToString();
        }

        internal static void PopulateDatums(TreeView tree, ListBox dbox)
        {
            dbox.Items.Clear();
            string query = "SELECT a.[Name] FROM Datum a, DatumRegion b, Region c " +
                "WHERE c.[Name] = @p AND b.[Region] = c.[ID] AND a.[ID] = b.[Datum]";
            using (OleDbConnection con = new OleDbConnection(Settings.Default.Constring))
            {
                using (OleDbDataAdapter da = new OleDbDataAdapter(query, con))
                {
                    da.SelectCommand.Parameters.AddWithValue(@"p", tree.SelectedNode.Text);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dbox.Items.AddRange(dt.AsEnumerable().Select(x => x[0].ToString()).ToArray());
                }
            }
        }

        internal static string GetDatumID(string v)
        {
            return DatumTable.Select(string.Format("Name LIKE '%{0}%'", v.Replace("'", "''")))[0][0].ToString();
        }

        internal static string GetRegionID(string v)
        {
            return RegionTable.Select(string.Format("Name like '%{0}%'", v.Replace("'","''")))[0][0].ToString();
        }

        internal static void DatumFormLoad(TreeView tree, string region, ListBox dbox, string datum)
        {
            DataTable dt = GetTable("Region");
            foreach (DataRow m in dt.Rows)
            {
                if (m[2].ToString() == "None")
                {
                    TreeNode n = new TreeNode(m[1].ToString());
                    string ParentId = m[0].ToString();
                    GetChildren(dt, ParentId, n);
                    tree.Nodes.Add(n);
                }
            }
            tree.Sort();
            foreach (TreeNode n in Helper.GetNode(tree.Nodes))
                if (n.Text == "The Earth")
                    n.Expand();
            foreach (TreeNode n in GetNode(tree.Nodes))
                if (n.Text == GetRegion(region))
                    tree.SelectedNode = n;
            if (dbox.Items != null && dbox.Items.Contains(GetDatum(datum)))
                dbox.SelectedItem = GetDatum(datum);
        }

        private static void GetChildren(DataTable dt, string parentId, TreeNode n)
        {
            foreach (DataRow m in dt.Rows)
            {
                if (m[2].ToString() == parentId)
                {
                    TreeNode c = new TreeNode(m[1].ToString());
                    string ChildID = m[0].ToString();
                    GetChildren(dt, ChildID, c);
                    n.Nodes.Add(c);
                }
            }
        }

        internal static IEnumerable<TreeNode> GetNode(TreeNodeCollection nodes)
        {
            foreach (TreeNode c in nodes)
            {
                yield return c;
                foreach (TreeNode m in GetNode(c.Nodes))
                    yield return m;
            }
        }

        internal static string GetRegion(string v)
        {
            return RegionTable.Select(string.Format("ID LIKE '%{0}%'", v))[0][1].ToString();
        }

        internal static string GetDatum(string v)
        {
            return DatumTable.Select(string.Format("ID LIKE '%{0}%'", v))[0][1].ToString();
        }

        internal static string GetGrid(string v)
        {
            DataTable dt = new DataTable();
            using (OleDbConnection con = new OleDbConnection(Settings.Default.Constring))
            {
                using (OleDbDataAdapter da = new OleDbDataAdapter("SELECT a.[Name], b.[Name] AS datum FROM Grid a, Datum b " +
                    "WHERE a.[Datum] = b.[ID] AND a.[ID] = @p", con))
                {
                    da.SelectCommand.Parameters.AddWithValue(@"p", v);
                    da.Fill(dt);
                }
            }
            return string.Format("{0} [{1}]", dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString());
        }

        internal static double Bearing(MPoint p1, MPoint p2)
        {
            /* 
             * Bearing is from p1 to p2 
             * φ1, φ2 are the latitude of point 1 and latitude of point 2 (in radians)
             * λ1, λ2 are the longitude of point 1 and longitude of point 2 (in radians)
             * 
             *  bearing  = arctan(X,Y)
             *  X = cos θb * sin ∆L
             *  Y = cos θa * sin θb – sin θa * cos θb * cos ∆L
             */
            double a1 = p1.X;
            double a2 = p2.X;
            //
            double b1 = p1.Y;
            double b2 = p2.Y;

            double λ1 = Math.Round(ToRadians(a1), 7);
            double λ2 = Math.Round(ToRadians(a2), 7);

            double φ1 = Math.Round(ToRadians(b1), 7);
            double φ2 = Math.Round(ToRadians(b2), 7);

            double X = Math.Cos(φ2) * Math.Sin(λ2 - λ1);
            double Y = Math.Cos(φ1) * Math.Sin(φ2) - (Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(λ2 - λ1));

            double b = Math.Atan2(X, Y);

            return (ToDegree(b) + 360) % 360; // Whole Circle Bearing
        }

        private static double ToDegree(double angle)
        {
            return angle * 180 / Math.PI;
        }

        private static double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }

        internal static double Distance(MPoint p1, MPoint p2)
        {
            //φ1, φ2 are the latitude of point 1 and latitude of point 2(in radians)
            //λ1, λ2 are the longitude of point 1 and longitude of point 2(in radians)

            double λ1 = Math.Round(ToRadians(p1.X), 6);
            double λ2 = Math.Round(ToRadians(p2.X), 6);

            double φ1 = Math.Round(ToRadians(p1.Y), 6);
            double φ2 = Math.Round(ToRadians(p2.Y), 6);

            double hav_φ2_φ1 = Math.Pow(Math.Sin((φ2 - φ1) / 2), 2);
            double hav_λ2_λ1 = Math.Pow(Math.Sin((λ2 - λ1) / 2), 2);

            double x = Math.Sqrt(hav_φ2_φ1 + (Math.Cos(φ1) * Math.Cos(φ2) * hav_λ2_λ1));

            double r = 6366739;  //Radius of earth in metres

            return 2 * r * Math.Asin(x);
        }
    }
}