using GeoCoordTrans_v2._1.Properties;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace GeoCoordTrans_v2._1
{
    public partial class MdbForm : Form
    {
        private static string _IDColName { get; set; }
        private string provider = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = ";
        private string constring ;
        private DataTable dt;
        private double _inputunit;
        private double _outputunit;

        private int coordtypeS;
        private int coordtypeT;

        #region Datum and Grid Properties
        private string DatumL { get; set; }
        private string GridL { get; set; }
        private string DatumR { get; set; }
        private string GridR { get; set; }
        //
        private string DatumRegionL { get; set; }
        private string GridRegionL { get; set; }
        private string DatumRegionR { get; set; }
        private string GridRegionR { get; set; }
        #endregion

        public MdbForm()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Size),
                  SystemColors.Control, SystemColors.ControlLightLight, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
            e.Graphics.DrawLine(new Pen(Color.CornflowerBlue), 0, LGB.Rectangle.Bottom - 1, LGB.Rectangle.X + LGB.Rectangle.Right, LGB.Rectangle.Bottom - 1);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Bounds.Size),
                  SystemColors.ControlLightLight, SystemColors.ControlLight, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
            e.Graphics.DrawLine(new Pen(Color.CornflowerBlue), 0, LGB.Rectangle.Top, LGB.Rectangle.X + LGB.Rectangle.Width, LGB.Rectangle.Top);
        }

        private void cancelbtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MdbForm_Load(object sender, EventArgs e)
        {
            
            CheckCoordinateTypes();
        }

        private void CheckCoordinateTypes()
        {
            #region Datums and Grids
            DatumL = mainform.DatumL; GridL = mainform.GridL;
            DatumRegionL = mainform.DatumRegionL; GridRegionL = mainform.GridRegionL;
            //
            DatumR = mainform.DatumR; GridR = mainform.GridR;
            DatumRegionR = mainform.DatumRegionR; GridRegionR = mainform.GridRegionR; 
            #endregion
            //
            //
            label31.Text = Helper.GetDatum(DatumL); label25.Text = Helper.GetGrid(GridL);
            label30.Text = Helper.GetRegion(DatumRegionL); label27.Text = Helper.GetRegion(GridRegionL);
            //
            label38.Text = Helper.GetDatum(DatumR); label42.Text = Helper.GetGrid(GridR);
            label37.Text = Helper.GetRegion(DatumRegionR); label41.Text = Helper.GetRegion(GridRegionR);
            //

            _newrb.Checked = true;
            label31.Text = Helper.GetDatum(DatumL); label27.Text = Helper.GetRegion(GridRegionL); 
            label37.Text = Helper.GetRegion(DatumRegionR); label41.Text = Helper.GetRegion(GridRegionR);
            label30.Text = Helper.GetRegion(DatumRegionL); label25.Text = Helper.GetGrid(GridL); 
            label38.Text = Helper.GetDatum(DatumR); label42.Text = Helper.GetGrid(GridR);
            //
            ValidatePage();
            cb5.DataSource = new BindingSource() { DataSource = Helper.GetTable("Unit"), Sort = "OrderNum ASC" };
            cb6.DataSource = new BindingSource() { DataSource = Helper.GetTable("Unit"), Sort = "OrderNum ASC" };
            typecb1.DataSource = new BindingSource() { DataSource = Helper.GetTable("CoordinateType"), Sort = "Value ASC" };
            typecb2.DataSource = new BindingSource() { DataSource = Helper.GetTable("CoordinateType"), Sort = "Value ASC" };
            typecb1.SelectedIndex = mainform.rbL;
            typecb2.SelectedIndex = mainform.rbR;
            cb5.SelectedIndex = mainform.UnitR;
            cb6.SelectedIndex = mainform.UnitL;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 6:
                    label34.Text = string.Format("Select unit of {0}", typecb1.SelectedIndex == 0 && height_chkbox.Checked ? "input ellipsoidal height" : "input coordinates");
                    break;
                case 10:
                    label33.Text = string.Format("Select unit of {0}", typecb2.SelectedIndex == 0 && height_chkbox.Checked ? "output ellipsoidal height" : "output coordinates");
                    break;
                case 11:
                    label12.Text = _newrb.Checked ? "click Next to Convert" : string.Format("Converted coordinates will be stored in columns {0} \n\n\nClick Next to Convert", GetString());
                    break;
                default:
                    break;
            }
            ValidatePage();
        }

        private string GetString()
        {
            string s = string.Empty;
            switch (coordtypeT)
            {
                case 0:
                    s = "Longitude_out, Latitude_out" + (typecb1.SelectedIndex != 2 && height_chkbox.Checked ? "and Height_out" : "");
                    break;
                case 1:
                    s = "Easting_out and Northing_out";
                    break;
                default:
                    break;
            }

            return s;
        }

        private void ValidatePage()
        {
            label1.Text = Settings.Default.PageInfo2[tabControl1.SelectedIndex];
            backbtn.Enabled = tabControl1.SelectedIndex != 0;
            nextbtn.Enabled = tabControl1.SelectedIndex != tabControl1.TabCount - 1;
            fbtn.Enabled = !nextbtn.Enabled;
        }

        private void nextbtn_Click(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    if (!string.IsNullOrEmpty(_filenamelbl.Text))
                        if (File.Exists(_filenamelbl.Text))
                            try
                            {
                                if (GetDatabaseTables(_filenamelbl.Text, out _) == true)
                                    tabControl1.SelectedIndex++;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        else
                            warninglbl.Text = "Invalid file";
                    else
                        warninglbl.Text = "Select file";
                    break;
                case 4:
                    EntryValidation();
                    break;
                case 5:
                    tabControl1.SelectedIndex += _newrb.Checked ? (typecb1.SelectedIndex != 1 && height_chkbox.Checked) || typecb1.SelectedIndex == 1 ? 1 : 2 :
                        (typecb1.SelectedIndex != 1 && height_chkbox.Checked) || typecb1.SelectedIndex == 1 ? 1 : 3;
                    break;
                case 6:
                    tabControl1.SelectedIndex += _newrb.Checked ? 1 : 2;
                    break;
                case 7:
                    if (!string.IsNullOrEmpty(_savetable.Text))
                    {
                        if (CheckIfTableExist(_savetable.Text) == true && checkBox1.Checked)
                        {
                            if (label15.Text != string.Empty)
                                label15.Text = string.Empty;
                            groupBox8.Enabled = false;
                            DeleteExistingTable(_savetable.Text);
                            tabControl1.SelectedIndex++;
                        }
                        else if (CheckIfTableExist(_savetable.Text) == false && (checkBox1.Checked == true || checkBox1.Checked == false))
                        {
                            if (label15.Text != string.Empty)
                                label15.Text = string.Empty;
                            tabControl1.SelectedIndex++;
                        }
                        else
                            label15.Text = "Table with name exist in database";
                    }
                    else
                        label15.Text = "Table name cannot be empty";
                    break;
                case 9:
                    tabControl1.SelectedIndex += ((typecb1.SelectedIndex != 1 && height_chkbox.Checked) || typecb2.SelectedIndex == 1) ? 1 : 2;
                    break;
                case 11:
                    nextbtn.Enabled = false;
                    panel7.Visible = !panel7.Visible;
                    backgroundWorker1.RunWorkerAsync();
                    break;
                default:
                    tabControl1.SelectedIndex++;
                    break;
            }
        }

        private void DeleteExistingTable(string v)
        {
            try
            {
                DataTable dt = new DataTable();
                List<string> tables = new List<string>();
                using (OleDbConnection con = new OleDbConnection(constring))
                {
                    string[] restrictions = new string[4];
                    restrictions[3] = "Table";
                    con.Open();
                    dt = con.GetSchema("Tables", restrictions);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                    tables.Add(dt.Rows[i][2].ToString());

                if (tables.Contains(v))
                {
                    using (OleDbConnection con = new OleDbConnection(constring))
                    {
                        con.Open();
                        using (OleDbCommand cmd = new OleDbCommand(string.Format("DROP TABLE [{0}]", v), con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CheckIfTableExist(string v)
        {
            DataTable dt = new DataTable();
            List<string> tables = new List<string>();
            using (OleDbConnection con = new OleDbConnection(constring))
            {
                string[] restrictions = new string[4];
                restrictions[3] = "Table";
                con.Open();
                dt = con.GetSchema("Tables", restrictions);
            }
            for (int i = 0; i < dt.Rows.Count; i++)
                tables.Add(dt.Rows[i][2].ToString());

            if (tables.Contains(v))
                return true;

            return false;
        }

        private void EntryValidation()
        {
            try
            {
                //Authenticate Primary key uniqueness
                if (lb3.Items.Count != 0)
                {
                    for (int i = 0; i < lb3.Items.Count; i++)
                        for (int j = 0; j < lb3.Items.Count; j++)
                            if (i != j)
                                if (lb3.Items[i].ToString() != lb3.Items[j].ToString())
                                    continue;
                                else
                                    throw new Exception("Invalid primary key format: Duplicate Keys");
                }
                else
                    return;
                //
                if (typecb1.SelectedIndex==0)
                {
                    if (lb1.Items.Count != 0)
                        for (int i = 0; i < lb1.Items.Count; i++)
                            if (double.TryParse(lb1.Items[i].ToString(), out _))
                                if (double.Parse(lb1.Items[i].ToString()) <= 180 && double.Parse(lb1.Items[i].ToString()) >= -180)
                                    continue;
                                else
                                    throw new Exception(string.Format("{0} at index {1} x {2} does not match criteria (between -180 and 180)", lb1.Items[i].ToString(), int.Parse(lb1.Tag.ToString()) + 1, i + 1));
                            else
                                throw new Exception(string.Format("Invalid entry at index: {0} x {1}", int.Parse(lb1.Tag.ToString()) + 1, i + 1));
                    else
                        return;

                    //
                    if (lb2.Items.Count != 0)
                        for (int i = 0; i < lb2.Items.Count; i++)
                            if (double.TryParse(lb2.Items[i].ToString(), out _))
                                if (double.Parse(lb2.Items[i].ToString()) <= 90 && double.Parse(lb2.Items[i].ToString()) >= -90)
                                    continue;
                                else
                                    throw new Exception(string.Format("{0} at index {1} x {2} does not match criteria (between -90 and 90)", lb2.Items[i].ToString(), int.Parse(lb2.Tag.ToString()) + 1, i + 1));
                            else
                                throw new Exception(string.Format("Invalid entry at index: {0} x {1}", int.Parse(lb2.Tag.ToString()) + 1, i + 1));
                    else
                        return;
                    
                    //
                    if (height_chkbox.Checked)
                        if (lb4.Items.Count != 0)
                            for (int i = 0; i < lb4.Items.Count; i++)
                                if (double.TryParse(lb4.Items[i].ToString(), out _))
                                    continue;
                                else
                                    throw new Exception(string.Format("Invalid entry at index: {0} x {1}", int.Parse(lb4.Tag.ToString()) + 1, i + 1));
                        else
                            return;
                }
                else
                {
                    if (lb1.Items.Count != 0)
                        for (int i = 0; i < lb1.Items.Count; i++)
                            if (double.TryParse(lb1.Items[i].ToString(), out _))
                                continue;
                            else
                                throw new Exception(string.Format("Invalid entry at index: {0}x{1}", int.Parse(lb1.Tag.ToString()) + 1, i + 1));
                    else
                        return;
                    //
                    if (lb2.Items.Count != 0)
                        for (int i = 0; i < lb2.Items.Count; i++)
                            if (double.TryParse(lb2.Items[i].ToString(), out _))
                                continue;
                            else
                                throw new Exception(string.Format("Invalid entry at index: {0}x{1}", int.Parse(lb2.Tag.ToString()) + 1, i + 1));
                    else
                        return;
                }
                if (!string.IsNullOrEmpty(label48.Text))
                    label48.Text = string.Empty;
                tabControl1.SelectedIndex++;
            }
            catch(Exception ex)
            {
                label48.Text = ex.Message;
            }
        }

        private void backbtn_Click(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 7:
                    tabControl1.SelectedIndex -= (typecb1.SelectedIndex != 1 && height_chkbox.Checked) || typecb1.SelectedIndex == 1 ? 1 : 2;
                    break;
                case 8:
                    tabControl1.SelectedIndex -= _newrb.Checked ? 1 : (typecb1.SelectedIndex != 1 && height_chkbox.Checked) || typecb1.SelectedIndex == 1 ? 2 : 3;
                    break;
                case 11:
                    tabControl1.SelectedIndex -= ((typecb1.SelectedIndex != 1 && height_chkbox.Checked) || typecb2.SelectedIndex == 1) ? 1 : 2;
                    break;
                default:
                    if (nextbtn.Enabled == false)
                        nextbtn.Enabled = true;
                    if (label15.Text != string.Empty)
                        label15.Text = string.Empty;
                    if (label48.Text != string.Empty)
                        label48.Text = string.Empty;
                    if (warninglbl.Text != string.Empty)
                        warninglbl.Text = string.Empty;
                    tabControl1.SelectedIndex--;
                    break;
            }
        }

        private void label49_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.CornflowerBlue);
            e.Graphics.DrawLine(p, 0, 0, (sender as Control).Width / 2, 0);
            e.Graphics.DrawLine(p, (sender as Control).Width / 2, 0, (sender as Control).Width / 2, (sender as Control).Height);
            e.Graphics.DrawLine(p, (sender as Control).Width / 2, (sender as Control).Height / 2, (sender as Control).Width, (sender as Control).Height / 2);
            e.Graphics.DrawLine(p, 0, (sender as Control).Height - 1, (sender as Control).Width / 2, (sender as Control).Height - 1);
        }

        private void Browsebtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select file";
                dlg.Filter = "All Files|*.*";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (new FileInfo(dlg.FileName).Extension == ".mdb")
                        {
                            DataTable dt = new DataTable();
                            if (GetDatabaseTables(dlg.FileName, out dt) == true)
                            {
                                PopulateTables(dt);
                                if (warninglbl.Text != string.Empty)
                                    warninglbl.Text = string.Empty;
                                _filenamelbl.Text = dlg.FileName;
                                sfilenamelbl.Text = dlg.SafeFileName;
                                Populate();
                            }
                        }
                        else
                            throw new Exception("You must select a Microsoft Access database with file type .mdb");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void PopulateTables(DataTable dt)
        {
            List<string> tables = new List<string>();
            if (dt.Rows.Count != 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    tables.Add(dt.Rows[i][2].ToString());
                cb1.DataSource = new BindingSource() { DataSource = tables };
            }
        }

        private bool GetDatabaseTables(string v, out DataTable tables)
        {
            tables = null;
            DataTable dt = new DataTable();
            using (OleDbConnection con = new OleDbConnection(provider + v))
            {
                con.Open();
                dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new string[] { null, null,null, "Table" });
            }
            if (dt.Rows.Count != 0)
            {
                constring = provider + v;
                tables = dt;
                return true;
            }
            else
                throw new Exception("Microsoft Access database must contain atleast one table");
        }

        private void Populate()
        {
            _savetable.Text = _newrb.Checked ? string.Format("{0}_out", cb1.SelectedItem.ToString()) : cb1.SelectedItem.ToString();
            dt = new DataTable();
            using (OleDbConnection con = new OleDbConnection(constring))
            {
                using (OleDbDataAdapter da = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", cb1.SelectedItem.ToString()), con))
                {
                    da.Fill(dt);
                }
            }
            List<string> fields = new List<string>();
            fields = dt.Columns.OfType<DataColumn>().Select(x => x.ColumnName).ToList();
            string[][] names = new string[4][];
            names[0] = Settings.Default.IDNames;
            names[1] = Settings.Default.XNames;
            names[2] = Settings.Default.YNames;
            names[3] = Settings.Default.ZNames;
            foreach (Control c in Helper.GetControl(groupBox6.Controls))
                if (c is ComboBox)
                {
                    (c as ComboBox).DataSource = new BindingSource() { DataSource = fields };
                    foreach (string v in (c as ComboBox).Items)
                        for (int j = 0; j < names[int.Parse(c.Tag.ToString())].Length; j++)
                            if (v == names[int.Parse(c.Tag.ToString())][j])
                                (c as ComboBox).SelectedIndex = (c as ComboBox).Items.IndexOf(v);
                    PopulateEntries(c);
                }
        }

        private void PopulateEntries(Control c)
        {
            if ((c as ComboBox).Name == cb7.Name)
                _IDColName = (c as ComboBox).Text;
            List<string> data = new List<string>();
            foreach (DataRow v in dt.Rows)
                data.Add(v[(c as ComboBox).SelectedIndex].ToString());
            foreach (Control d in Helper.GetControl(groupBox6.Controls))
                if (d is ListBox && d.Tag == c.Tag)
                {
                    (d as ListBox).DataSource = null;
                    (d as ListBox).DataSource = new BindingSource() { DataSource = data };
                }
                    
        }

        private void _newrb_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked == true)
            {
                panel10.Visible = panel8.Visible = (sender as RadioButton).Tag.ToString() == "0";
                pictureBox2.Image = (sender as RadioButton).Tag.ToString() == "0" ? Resources.newtable : Resources.existingtable;
            }
        }
        private void typecb1_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel9.Visible = panel11.Visible = (sender as ComboBox).SelectedIndex != 1;
            coordtypeS = tabControl2.SelectedIndex = (sender as ComboBox).SelectedIndex;
            switch ((sender as ComboBox).SelectedIndex)
            {
                case 0:
                    panel4.BringToFront();
                    panel5.BringToFront();
                    break;
                case 1:
                    panel3.BringToFront();
                    panel6.BringToFront();
                    break;
                default:
                    break;
            }
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            label51.Visible = lb4.Visible = cb8.Visible = panel11.Visible = (sender as CheckBox).Checked;
        }

        private void cb7_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateEntries(sender as ComboBox);
        }

        private void cb1_SelectedIndexChanged(object sender, EventArgs e)
        { 
            Populate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int temp = cb2.SelectedIndex;
            cb2.SelectedIndex = cb3.SelectedIndex;
            cb3.SelectedIndex = temp;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            dForm df = new dForm(DatumRegionL, DatumL);
            if (df.ShowDialog() == DialogResult.OK)
            {
                DatumRegionL = df.region;
                DatumL = df.datum;
                label30.Text = Helper.GetRegion(DatumRegionL);
                label31.Text = Helper.GetDatum(DatumL);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            gForm gf = new gForm(GridRegionL, GridL);
            if (gf.ShowDialog() == DialogResult.OK)
            {
                GridRegionL = gf.region;
                GridL = gf.grid;
                label27.Text = Helper.GetRegion(GridRegionL);
                label25.Text = Helper.GetGrid(GridL);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dForm df = new dForm(DatumRegionR, DatumR);
            if (df.ShowDialog() == DialogResult.OK)
            {
                DatumRegionR = df.region;
                DatumR = df.datum;
                label37.Text = Helper.GetRegion(DatumRegionR);
                label38.Text = Helper.GetDatum(DatumR);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            gForm gf = new gForm(GridRegionR, GridR);
            if (gf.ShowDialog() == DialogResult.OK)
            {
                GridRegionR = gf.region;
                GridR = gf.grid;
                label41.Text = Helper.GetRegion(GridRegionR);
                label42.Text = Helper.GetGrid(GridR);
            }
        }

        private void cb6_SelectedIndexChanged(object sender, EventArgs e)
        {
            _inputunit = double.Parse((sender as ComboBox).SelectedValue.ToString());
        }

        private void cb5_SelectedIndexChanged(object sender, EventArgs e)
        {
            _outputunit = double.Parse((sender as ComboBox).SelectedValue.ToString());
        }

        private void rbn4_CheckedChanged(object sender, EventArgs e)
        {
            tabControl3.SelectedIndex = int.Parse((sender as RadioButton).Tag.ToString());
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (dt.Rows != null)
            {
                try
                {
                    int k = lb1.Items.Count;
                    List<string> fields = new List<string>();
                    switch (coordtypeT)
                    {
                        case 0:
                            fields = new List<string>() { _IDColName, "Longitude", "Latitude" };
                            break;
                        case 1:
                            fields = new List<string>() { _IDColName, "Easting", "Northing" };
                            break;
                        default:
                            break;
                    }
                    if (height_chkbox.Checked && coordtypeS!=1 && coordtypeT!=1)
                        switch (coordtypeT)
                        {
                            case 0:
                                fields.Add("Height");
                                break;
                            default:
                                break;
                        }
                    DataTable data = new DataTable();
                    foreach (string s in fields)
                        data.Columns.Add(new DataColumn(s));
                    for (int i = 0; i < k; i++)
                    {
                        MPoint p = new MPoint();
                        switch (coordtypeS)
                        {
                            case 0:
                                p = new MPoint(double.Parse(lb1.Items[i].ToString()), double.Parse(lb2.Items[i].ToString()), height_chkbox.Checked ?
                                double.Parse(lb4.Items[i].ToString()) * _inputunit : 0);
                                switch (coordtypeT)
                                {
                                    case 0:
                                        p = Transform.GeoToGeo(DatumL, DatumR, p);
                                        break;
                                    case 1:
                                        p = Transform.GeoToGeo(DatumL, Helper.GetGridDatumID(GridR), p);
                                        p = Transform.GeoToPro(GridR, p);
                                        p.X /= _outputunit;
                                        p.Y /= _outputunit;
                                        p.Z /= _outputunit;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case 1:
                                p = new MPoint(double.Parse(lb1.Items[i].ToString()) * _inputunit, double.Parse(lb2.Items[i].ToString()) * _inputunit, 0);
                                switch (coordtypeT)
                                {
                                    case 0:
                                        p = Transform.ProToGeo(GridL, p);
                                        p = Transform.GeoToGeo(Helper.GetGridDatumID(GridL), DatumR, p);
                                        break;
                                    case 1:
                                        p = Transform.ProToPro(GridL, GridR, p);
                                        p.X /= _outputunit;
                                        p.Y /= _outputunit;
                                        p.Z /= _outputunit;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                        DataRow d = data.NewRow();
                        for (int j = 0; j < data.Columns.Count; j++)
                            foreach (Control c in Helper.GetControl(groupBox6.Controls))
                                if (c is ListBox && c.Tag.ToString() == j.ToString())
                                    if (j == 0)
                                        d[j] = lb3.Items[i].ToString();
                                    else
                                    {
                                        if (j == 1)
                                            d[j] = Math.Round(p.X, coordtypeT == 0 ? 7 : 2).ToString();
                                        if (j == 2)
                                            d[j] = Math.Round(p.Y, coordtypeT == 0 ? 7 : 2).ToString();
                                        if (j == 3)
                                            d[j] = Math.Round(p.Z, 2).ToString();
                                    }
                        data.Rows.Add(d);
                        backgroundWorker1.ReportProgress(i * 100 / k);
                    }
                    e.Result = data;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((DataTable)e.Result != null)
            {
                DataTable m = (DataTable)e.Result;
                using (OleDbConnection con = new OleDbConnection(constring))
                {
                    try
                    {
                        con.Open();
                        List<string> cols = new List<string>();
                        if (_newrb.Checked)
                        {
                            cols = m.Columns.OfType<DataColumn>().AsEnumerable().Select(x => x.ColumnName).ToList();
                            using (OleDbCommand cd = new OleDbCommand(GenerateQuery("Create", cols), con))
                            {
                                cd.ExecuteNonQuery();
                            }
                            for (int i = 0; i < m.Rows.Count; i++)
                            {
                                using (OleDbCommand cd = new OleDbCommand(GenerateQuery("Insert", cols), con))
                                {
                                    for (int j = 0; j < m.Columns.Count; j++)
                                        cd.Parameters.AddWithValue((j + 1).ToString(), m.Rows[i][j].ToString());
                                    cd.ExecuteNonQuery();
                                }
                            }
                        }
                        else
                        {
                            switch (coordtypeT)
                            {
                                case 0:
                                    cols = new List<string>() { _IDColName, "Longitude_out", "Latitude_out" };
                                    break;
                                case 1:
                                    cols = new List<string>() { _IDColName, "Easting_out", "Northing_out" };
                                    break;
                                default:
                                    break;
                            }
                            if (height_chkbox.Checked && coordtypeS != 1 && coordtypeT != 1)
                                switch (coordtypeT)
                                {
                                    case 0:
                                        cols.Add("Height_out");
                                        break;
                                    default:
                                        break;
                                }
                            if (CheckIfTableExist(cb1.SelectedItem.ToString()) == true)
                            {
                                using (OleDbDataAdapter da = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", cb1.SelectedItem.ToString()), con))
                                {
                                    DataTable dt = new DataTable();
                                    da.Fill(dt);
                                    List<string> Headers = dt.Columns.OfType<DataColumn>().AsEnumerable().Select(x => x.ColumnName).ToList();
                                    Headers.RemoveAt(Headers.IndexOf(_IDColName));
                                    foreach (string x in cols)
                                        foreach (string n in Headers)
                                            if (x == n)
                                                using (OleDbCommand cd = new OleDbCommand(string.Format("ALTER TABLE [{0}] DROP COLUMN {1}", cb1.SelectedItem.ToString(), x), con))
                                                    cd.ExecuteNonQuery();
                                }
                                foreach (string v in cols)
                                    if(v!=_IDColName)
                                        using (OleDbCommand cmd = new OleDbCommand(string.Format("ALTER TABLE [{0}] ADD COLUMN {1} Number", cb1.SelectedItem.ToString(), v), con))
                                            cmd.ExecuteNonQuery();
                                //
                                for (int i = 0; i < m.Rows.Count; i++)
                                {
                                    using (OleDbCommand cd = new OleDbCommand(GenerateQuery("Update",cols), con))
                                    {
                                        for (int j = m.Columns.Count-1; j >=0; j--)
                                        {
                                            cd.Parameters.AddWithValue((j + 1).ToString(), m.Rows[i][j].ToString());
                                        }
                                        cd.ExecuteNonQuery();
                                    }
                                }
                            }
                            else
                                throw new Exception("Table does not exist");
                        }
                        MessageBox.Show("Conversion Complete\t", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        progressBar1.Value = 0;
                        panel7.Visible = !panel7.Visible;
                        tabControl1.SelectedIndex++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        progressBar1.Value = 0;
                        panel7.Visible = !panel7.Visible;
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open)
                            con.Close();
                    }

                }
            }
        }

        private string GenerateQuery(string v, List<string> cols)
        {
            string r = string.Empty;
            if (v == "Create")
            {
               r= string.Format("CREATE TABLE [{0}] (", _savetable.Text);
                for (int i = 0; i < cols.Count; i++)
                {
                    if (i == 0)
                        r += string.Format("[{0}] Text,", cols[i]);
                    else if (i == cols.Count - 1)
                        r += string.Format("[{0}] Number)", cols[i]);
                    else
                        r += string.Format("[{0}] Number,", cols[i]);
                }
            }
            if (v == "Update")
            {
                r = string.Format("UPDATE [{0}] SET ", cb1.SelectedItem.ToString());
                for (int i =0; i < cols.Count ; i++)
                {
                    if (i == cols.Count - 1)
                        r += string.Format(" WHERE [{0}] = ?;", _IDColName);
                    else if (i == cols.Count - 2)
                        r += string.Format("[{0}]  = ?", cols[cols.Count - i - 1]);
                    else
                        r += string.Format(" [{0}]  = ?, ", cols[cols.Count - i - 1]);
                }
            }
            if (v == "Insert")
            {
                r = string.Format("INSERT INTO [{0}] VALUES (", _savetable.Text);
                for (int i = 0; i < cols.Count; i++)
                {
                    if (i == cols.Count - 1)
                        r += "?)";
                    else
                        r += "?,";
                }
            }

            return r;
        }

        private void fbtn_Click(object sender, EventArgs e)
        {
            if (new FileInfo(_filenamelbl.Text).Exists)
                Process.Start(_filenamelbl.Text);
            Close();
        }

        private void typecb2_SelectedIndexChanged(object sender, EventArgs e)
        {
            coordtypeT = tabControl3.SelectedIndex = (sender as ComboBox).SelectedIndex;
        }
    }
}
