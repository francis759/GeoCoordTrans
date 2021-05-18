using GeoCoordTrans_v2._1.Properties;
using System;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GeoCoordTrans_v2._1
{
    public partial class Csvform : Form
    {
        RegistryKey Key;
        private char Delimiter { get; set; }
        #region Datum and Grid Properties
        private  string DatumL { get; set; }
        private  string GridL { get; set; }
        private  string DatumR { get; set; }
        private  string GridR { get; set; }
        //
        private  string DatumRegionL { get; set; }
        private  string GridRegionL { get; set; }
        private  string DatumRegionR { get; set; }
        private  string GridRegionR { get; set; }
        #endregion
        private List<string> Data = new List<string>();
        private double _inputunit;
        private double _outputunit;

        private int coordtypeS;
        private int coordtypeT;

        public Csvform()
        {
            InitializeComponent();
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Size),
                 SystemColors.Control, SystemColors.ControlLightLight, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
            e.Graphics.DrawLine(new Pen(Color.CornflowerBlue), 0, LGB.Rectangle.Bottom - 1, LGB.Rectangle.X + LGB.Rectangle.Right, LGB.Rectangle.Bottom - 1);
        }

        private void Panel2_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush LGB = new LinearGradientBrush(new Rectangle(new Point(0, 0), (sender as Control).Bounds.Size),
                  SystemColors.ControlLightLight, SystemColors.ControlLight, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(LGB, LGB.Rectangle);
            e.Graphics.DrawLine(new Pen(Color.CornflowerBlue), 0, LGB.Rectangle.Top, LGB.Rectangle.X + LGB.Rectangle.Width, LGB.Rectangle.Top);
        }

        private void Csvform_Load(object sender, EventArgs e)
        {
            if ((Key = Registry.CurrentUser.OpenSubKey(Settings.Default.register)) != null)
                if(Key.GetValue("FieldNamesOnFirstRow")!=null)
                    fieldnameschkbx.Checked = bool.Parse(Key.GetValue("FieldNamesOnFirstRow").ToString());
            EnableAndDisableButton();
            CheckCoordinateTypes();
            cb1.DataSource = Helper.GetTable("Delimiter");
            cb4.DataSource = new BindingSource() { DataSource = Helper.GetTable("Unit"), Sort = "OrderNum ASC" };
            cb5.DataSource = new BindingSource() { DataSource = Helper.GetTable("Unit"), Sort = "OrderNum ASC" };
            cb4.SelectedIndex = mainform.UnitL;
            cb5.SelectedIndex = mainform.UnitR;
            typecb1.DataSource=new BindingSource() { DataSource = Helper.GetTable("CoordinateType"), Sort = "Value ASC" };
            typecb2.DataSource = new BindingSource() { DataSource = Helper.GetTable("CoordinateType"), Sort = "Value ASC" };
            typecb1.SelectedIndex = mainform.rbL;
            typecb2.SelectedIndex = mainform.rbR;
            
        }

        private void CheckCoordinateTypes()
        {
            #region Datums and Grids
            DatumL = mainform.DatumL;GridL = mainform.GridL; 
            DatumRegionL = mainform.DatumRegionL ;GridRegionL = mainform.GridRegionL;
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
        }

        private void EnableAndDisableButton()
        {
            label1.Text = Settings.Default.pageInfo[tabControl1.SelectedIndex];
            backbtn.Enabled = tabControl1.SelectedIndex != 0;
            nextbtn.Enabled = tabControl1.SelectedIndex != tabControl1.TabCount - 1;
            fbtn.Enabled = !nextbtn.Enabled;
        }

        private void nextbtn_Click(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 1:
                    if (_filenamelbl.Text != string.Empty)
                        if (File.Exists(_filenamelbl.Text))
                            if (new FileInfo(_filenamelbl.Text).Length > 2)
                            {
                                if (warninglbl.Text != string.Empty)
                                    warninglbl.Text = string.Empty;
                                tabControl1.SelectedIndex++;
                            }
                            else
                                warninglbl.Text = "Empty File";
                        else
                            warninglbl.Text = "Invalid file";
                    else
                        warninglbl.Text = "Select file";
                    break;
                case 5:
                    try
                    {
                        if (typecb1.SelectedIndex==0)
                        {
                            if (lb1.Items.Count != 0)
                            {
                                for (int i = 0; i < lb1.Items.Count; i++)
                                    if (double.TryParse(lb1.Items[i].ToString(), out _))
                                        if (double.Parse(lb1.Items[i].ToString()) <= 180 && double.Parse(lb1.Items[i].ToString()) >= -180)
                                            continue;
                                        else
                                            throw new Exception(string.Format("{0} at index {1} x {2} does not match criteria (between -180 and 180)", lb1.Items[i].ToString(), int.Parse(lb1.Tag.ToString()) + 1, i + 1));
                                    else
                                        throw new Exception(string.Format("Invalid entry at index: {0} x {1}", int.Parse(lb1.Tag.ToString()) + 1, i + 1));
                            }
                            else
                                return;
                            //
                            if (lb2.Items.Count != 0)
                            {
                                for (int i = 0; i < lb2.Items.Count; i++)
                                    if (double.TryParse(lb2.Items[i].ToString(), out _))
                                        if (double.Parse(lb2.Items[i].ToString()) <= 90 && double.Parse(lb2.Items[i].ToString()) >= -90)
                                            continue;
                                        else
                                            throw new Exception(string.Format("{0} at index {1} x {2} does not match criteria (between -90 and 90)", lb2.Items[i].ToString(), int.Parse(lb2.Tag.ToString()) + 1, i + 1));
                                    else
                                        throw new Exception(string.Format("Invalid entry at index: {0} x {1}", int.Parse(lb2.Tag.ToString()) + 1, i + 1));
                            }
                            else
                                return;
                            //
                            if (height_chkbox.Checked)
                                if (lb3.Items.Count != 0)
                                    for (int i = 0; i < lb3.Items.Count; i++)
                                        if (double.TryParse(lb3.Items[i].ToString(), out _))
                                            continue;
                                        else
                                            throw new Exception(string.Format("Invalid entry at index: {0} x {1}", int.Parse(lb3.Tag.ToString()) + 1, i + 1));
                                else
                                    return;
                        }
                        else
                        {
                            if (lb1.Items.Count != 0)
                            {
                                for (int i = 0; i < lb1.Items.Count; i++)
                                    if (double.TryParse(lb1.Items[i].ToString(), out _))
                                        continue;
                                    else
                                        throw new Exception(string.Format("Invalid entry at index: {0}x{1}", int.Parse(lb1.Tag.ToString()) + 1, i + 1));
                            }
                            //
                            if (lb2.Items.Count != 0)
                            {
                                for (int i = 0; i < lb2.Items.Count; i++)
                                    if (double.TryParse(lb2.Items[i].ToString(), out _))
                                        continue;
                                    else
                                        throw new Exception(string.Format("Invalid entry at index: {0}x{1}", int.Parse(lb2.Tag.ToString()) + 1, i + 1));
                            }
                        }
                        if (!string.IsNullOrEmpty(label48.Text))
                            label48.Text = string.Empty;
                        tabControl1.SelectedIndex++;
                    }
                    catch(Exception ex)
                    {
                        label48.Text = ex.Message;
                    }
                    break;
                case 6:
                    tabControl1.SelectedIndex += (typecb1.SelectedIndex!=1 && height_chkbox.Checked) || typecb1.SelectedIndex==1 ? 1 : 2;
                    break;
                case 10:
                    tabControl1.SelectedIndex += (typecb1.SelectedIndex != 1 && height_chkbox.Checked && typecb2.SelectedIndex !=1) || typecb2.SelectedIndex == 1 ? 1 : 2;
                    break;
                case 12:
                    try
                    {
                        nextbtn.Enabled = false;
                        panel7.Visible = !panel7.Visible;
                        backgroundWorker1.RunWorkerAsync();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                default:
                    tabControl1.SelectedIndex++;
                    break;
            }
        }

        private void backbtn_Click(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 8:
                    tabControl1.SelectedIndex -= (typecb1.SelectedIndex != 1 && height_chkbox.Checked) || typecb1.SelectedIndex == 1 ? 1 : 2;
                    break;
                case 12:
                    tabControl1.SelectedIndex -= (typecb1.SelectedIndex != 1 && height_chkbox.Checked && typecb2.SelectedIndex != 1) || typecb2.SelectedIndex == 1 ? 1 : 2;
                    break;
                default:
                    tabControl1.SelectedIndex--;
                    if (nextbtn.Enabled == false)
                        nextbtn.Enabled = true;
                    if (label48.Text != string.Empty)
                        label48.Text = string.Empty;
                    if (warninglbl.Text != string.Empty)
                        warninglbl.Text = string.Empty;
                    break;
            }
            EnableAndDisableButton();
        }

        private void typecb1_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel9.Visible = panel8.Visible = (sender as ComboBox).SelectedIndex != 1;
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

        private void typecb2_SelectedIndexChanged(object sender, EventArgs e)
        {
            coordtypeT = tabControl3.SelectedIndex = (sender as ComboBox).SelectedIndex;
        }

        private void label49_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.CornflowerBlue);
            e.Graphics.DrawLine(p, 0, 0, (sender as Control).Width / 2, 0);
            e.Graphics.DrawLine(p, (sender as Control).Width / 2, 0, (sender as Control).Width / 2, (sender as Control).Height);
            e.Graphics.DrawLine(p, (sender as Control).Width / 2, (sender as Control).Height / 2, (sender as Control).Width, (sender as Control).Height / 2);
            e.Graphics.DrawLine(p, 0, (sender as Control).Height - 1, (sender as Control).Width / 2, (sender as Control).Height - 1);
        }

        private void height_chkbox_CheckedChanged(object sender, EventArgs e)
        {
            label52.Visible = lb3.Visible = cb6.Visible = (sender as CheckBox).Checked;
        }

        private void cancelbtn_Click(object sender, EventArgs e)
        {
            Close();
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

        private void fbtn_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                if (new FileInfo(label34.Text).Exists)
                    Process.Start(label34.Text);
            Close();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label50.Text = string.Format("Select unit of {0}", typecb1.SelectedIndex == 0 && height_chkbox.Checked ? "ellipsoidal height" : "input coordinates");
            label51.Text = string.Format("Select unit of {0}", typecb2.SelectedIndex == 0 && height_chkbox.Checked ? "output ellipsoidal height" : "output coordinates");
            EnableAndDisableButton();
        }

        private void Browsebtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.RestoreDirectory = true;
                dlg.Title = "Select file";
                dlg.Filter = "All Files|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                    try
                    {
                        if (new FileInfo(dlg.FileName).Extension == ".csv" || new FileInfo(dlg.FileName).Extension == ".txt")
                            if (new FileInfo(dlg.FileName).Length > 2)
                            {
                                Data = File.ReadAllLines(dlg.FileName).ToList();
                                if (warninglbl.Text != string.Empty)
                                    warninglbl.Text = string.Empty;
                                _filenamelbl.Text = dlg.FileName;
                                sfilenamelbl.Text = dlg.SafeFileName;
                                FindDelimiter();
                                label33.Text = Path.GetFileNameWithoutExtension(dlg.FileName) + "_out" + Path.GetExtension(dlg.SafeFileName);
                                label34.Text = Path.GetFullPath(_filenamelbl.Text).Replace(dlg.SafeFileName, label33.Text);
                            }
                            else
                                throw new Exception("Empty File!\t\t");
                        else
                            throw new Exception(label2.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
            }
        }

        private void FindDelimiter()
        {
            string[] delimiters = Helper.GetTable("Delimiter").AsEnumerable().Select(x => x[1].ToString()).ToArray();
            for (int i = 0; i < delimiters.Length; i++)
                if (delimiters[i] == "\\t")
                    delimiters[i] = "\t";

            // test file against delimiters
            for (int i = 0; i < delimiters.Length; i++)
            {
                using (TextFieldParser parser = new TextFieldParser(_filenamelbl.Text))
                {
                    // setup parser
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(delimiters[i]);
                    parser.HasFieldsEnclosedInQuotes = true;
                    if (!parser.EndOfData)
                    {
                        // parse
                        string[] fields = parser.ReadFields();
                        // if we get more than one field, we have found the correct delimiter
                        if (fields.Length > 1)
                        {
                            cb1.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            PopulateFields();
        }

        private void cb1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Delimiter = Convert.ToChar((sender as ComboBox).SelectedValue.ToString() == "\\t" ? "\t" : (sender as ComboBox).SelectedValue);
            PopulateFields();
        }

        private void PopulateFields()
        {
            if (Data.Count != 0)
            {
                label15.Text = fieldnameschkbx.Checked == true ? Data[0] : "Preview: No field names";
                //
                string[] fields = new string[Data[0].Split(Delimiter).Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fieldnameschkbx.Checked == true)
                        fields[i] = Data[0].Split(Delimiter)[i];
                    else
                        fields[i] = "Field" + (i + 1).ToString();
                }
                string[][] names = new string[3][];
                names[0] = Settings.Default.XNames;
                names[1] = Settings.Default.YNames;
                names[2] = Settings.Default.ZNames;
                foreach (Control c in Helper.GetControl(groupBox6.Controls))
                    if (c is ComboBox)
                    {
                        (c as ComboBox).DataSource = new BindingSource() { DataSource = fields };
                        foreach (string v in (c as ComboBox).Items)
                            for (int j = 0; j < names[int.Parse(c.Tag.ToString())].Length; j++)
                                if (v == names[int.Parse(c.Tag.ToString())][j])
                                    (c as ComboBox).SelectedIndex = (c as ComboBox).Items.IndexOf(v);
                    }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            PopulateFields();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int temp = cb2.SelectedIndex;
            cb2.SelectedIndex = cb3.SelectedIndex;
            cb3.SelectedIndex = temp;
        }

        private void cb2_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> d = new List<string>();
            for (int i = fieldnameschkbx.Checked == true ? 1 : 0; i < Data.Count; i++)
                d.Add(Data[i].Split(Delimiter)[(sender as ComboBox).SelectedIndex]);
            foreach (Control c in Helper.GetControl(groupBox6.Controls))
                if (c is ListBox && c.Tag == (sender as ComboBox).Tag)
                {
                    (c as ListBox).DataSource = null;
                    (c as ListBox).DataSource = d;
                }
                    
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<string> r = new List<string>();
                if (Data.Count != 0)
                {
                    int k = lb1.Items.Count;
                    for (int i = 0; i < k; i++)
                    {
                        string s = string.Empty;
                        MPoint p = new MPoint();
                        switch (coordtypeS)
                        {
                            case 0:
                                p = new MPoint(double.Parse(lb1.Items[i].ToString()), double.Parse(lb2.Items[i].ToString()), height_chkbox.Checked ?
                                double.Parse(lb3.Items[i].ToString()) * _inputunit : 0);
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
                        for (int j = 0; j < Data[0].Split(Delimiter).Length; j++)
                            s += Data[fieldnameschkbx.Checked ? i + 1 : i].Split(Delimiter)[j] + Delimiter;
                        s += Math.Round(p.X, coordtypeT == 0 ? 7 : 2).ToString() + Delimiter;
                        s += Math.Round(p.Y, coordtypeT == 0 ? 7 : 2).ToString();
                        if (coordtypeS != 1 && height_chkbox.Checked && coordtypeT != 1)
                            s += Delimiter + Math.Round(p.Z, 2).ToString();
                        r.Add(s);
                        backgroundWorker1.ReportProgress(i * 100 / k);
                    }
                    e.Result = r;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cb4_SelectedIndexChanged(object sender, EventArgs e)
        {
            _inputunit = double.Parse((sender as ComboBox).SelectedValue.ToString());
        }

        private void cb5_SelectedIndexChanged(object sender, EventArgs e)
        {
            _outputunit = double.Parse((sender as ComboBox).SelectedValue.ToString());
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                List<string> s = new List<string>();
                if (fieldnameschkbx.Checked == true)
                {
                    string v = string.Empty;
                    switch (coordtypeT)
                    {
                        case 0:
                            v = "Longitude" + Delimiter + "Latitude" + (typecb1.SelectedIndex != 1 && height_chkbox.Checked ? Delimiter + "Height" : "");
                            break;
                        case 1:
                            v = "Easting" + Delimiter + "Northing";
                            break;
                        default:
                            break;
                    }
                    s.Add(Data[0] + Delimiter + v);
                    s.AddRange(((List<string>)e.Result).ToArray());
                }
                else
                    s = (List<string>)e.Result;
                using (StreamWriter ws = new StreamWriter(label34.Text))
                {
                    foreach (string v in s)
                        ws.WriteLine(v);
                }
                MessageBox.Show("Conversion complete!");
                progressBar1.Value = 0;
                panel7.Visible = !panel7.Visible;
                tabControl1.SelectedIndex++;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Select File";
                dlg.FileName = label33.Text;
                dlg.Filter = "CSV Demiliter|*.csv|Text File |*.txt";
                if (new FileInfo(label34.Text).Extension==".txt")
                    dlg.FilterIndex = 2;
                dlg.InitialDirectory = Path.GetFullPath(label34.Text);
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    label34.Text = dlg.FileName;
                    label33.Text = Path.GetFileName(dlg.FileName);
                }
            }
        }

        private void Csvform_FormClosing(object sender, FormClosingEventArgs e)
        {
            Key = Registry.CurrentUser.CreateSubKey(Settings.Default.register);
            if (Key != null)
                Key.SetValue("FieldNamesOnFirstRow", fieldnameschkbx.Checked);
        }
    }
}
