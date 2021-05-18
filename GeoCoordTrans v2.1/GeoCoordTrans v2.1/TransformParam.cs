using GeoCoordTrans_v2._1.Properties;
using MatrixClass;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoCoordTrans_v2._1
{
    public partial class TransformParam : Form
    {
        List<string> Data = new List<string>();
        List<string> Data1 = new List<string>();
        double[][] x { get; set; }
        char Delimiter { get; set; }
        char Delimiter2 { get; set; }
        Equation equation = 0;
        public TransformParam()
        {
            InitializeComponent();
        }

        private void label49_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.CornflowerBlue);
            e.Graphics.DrawLine(p, 0, 0, (sender as Control).Width / 2, 0);
            e.Graphics.DrawLine(p, (sender as Control).Width / 2, 0, (sender as Control).Width / 2, (sender as Control).Height);
            e.Graphics.DrawLine(p, (sender as Control).Width / 2, (sender as Control).Height / 2, (sender as Control).Width, (sender as Control).Height / 2);
            e.Graphics.DrawLine(p, 0, (sender as Control).Height - 1, (sender as Control).Width / 2, (sender as Control).Height - 1);
        }

        private void TransformParam_Load(object sender, EventArgs e)
        {
            cb1.DataSource = Enum.GetValues(typeof(Equation)).Cast<Equation>().ToList();
        }

        private void cb1_SelectedIndexChanged(object sender, EventArgs e)
        {
            equation = (Equation)Enum.Parse(typeof(Equation), (sender as ComboBox).SelectedValue.ToString(), true);
            pictureBox1.Image = cb1.SelectedIndex == 0 ? Resources.affine : Resources.conf;
            textBox1.Text = string.Empty;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetData((sender as Button).Tag.ToString());
        }

        private void GetData(string v)
        {
            try
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    if (new FileInfo(dlg.FileName).Extension == ".csv" || new FileInfo(dlg.FileName).Extension == ".txt")
                        if (new FileInfo(dlg.FileName).Length > 2)
                        {
                            if (v == "0")
                            {
                                Data = File.ReadAllLines(dlg.FileName).ToList();
                                label2.Text = dlg.FileName;
                                label8.Text = dlg.SafeFileName;
                            }
                            else
                            {
                                Data1 = File.ReadAllLines(dlg.FileName).ToList();
                                label15.Text = dlg.FileName;
                                label10.Text = dlg.SafeFileName;
                            } 
                            FindDelimiter(dlg.FileName, v);
                        }
                        else
                            throw new Exception("Empty File!\t\t");
                    else
                        throw new Exception("The input file must be in CSV format. That is a text file with columns of data, " +
                                    "where each column is separated by a special character.(i.e. comma, etc)If you have your coordinates in Microsoft Access, " +
                                    "Excel or any other file format, you need to save them as a CSV file.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                dlg.Dispose();
            }
        }

        private void FindDelimiter(string field, string v)
        {
            string[] delimiters = new string[] { "\t", "," };

            // test file against delimiters
            for (int i = 0; i < delimiters.Length; i++)
            {
                using (TextFieldParser parser = new TextFieldParser(field))
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
                            if (v == "0")
                                Delimiter = Convert.ToChar(delimiters[i]);
                            else
                                Delimiter2 = Convert.ToChar(delimiters[i]);
                            break;
                        }

                    }
                }
            }
            PopulateFields(v);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GetData((sender as Button).Tag.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int temp = comboBox2.SelectedIndex;
            comboBox2.SelectedIndex = comboBox3.SelectedIndex;
            comboBox3.SelectedIndex = temp;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> d = new List<string>();
            for (int i = ckbox1.Checked == true ? 1 : 0; i < Data.Count; i++)
                d.Add(Data[i].Split(Delimiter)[(sender as ComboBox).SelectedIndex]);
            foreach (Control c in Helper.GetControl(groupBox2.Controls))
                if (c is ListBox && c.Tag == (sender as ComboBox).Tag)
                {
                    (c as ListBox).DataSource = null;
                    (c as ListBox).DataSource = d;
                }
            textBox1.Text = string.Empty;
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> d = new List<string>();
            for (int i = ckbox2.Checked == true ? 1 : 0; i < Data1.Count; i++)
                d.Add(Data1[i].Split(Delimiter2)[(sender as ComboBox).SelectedIndex]);
            foreach (Control c in Helper.GetControl(groupBox3.Controls))
                if (c is ListBox && c.Tag == (sender as ComboBox).Tag)
                {
                    (c as ListBox).DataSource = null;
                    (c as ListBox).DataSource = d;
                }
            textBox1.Text = string.Empty;
        }

        private void ckbox1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Tag.ToString() == "0")
            {
                if (Data.Count != 0)
                    PopulateFields("0");
            }
            else
                if (Data1.Count != 0)
                PopulateFields("1");
        }

        private void PopulateFields(string v)
        {
            if (v == "0")
            {
                string[] fields = new string[Data[0].Split(Delimiter).Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    if (ckbox1.Checked == true)
                        fields[i] = Data[0].Split(Delimiter)[i];
                    else
                        fields[i] = "Field" + (i + 1).ToString();
                }
                foreach (Control c in Helper.GetControl(groupBox2.Controls))
                    if (c is ComboBox)
                        (c as ComboBox).DataSource = new BindingSource() { DataSource = fields };
            }
            else
            {
                string[] fields = new string[Data1[0].Split(Delimiter2).Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    if (ckbox2.Checked == true)
                        fields[i] = Data1[0].Split(Delimiter2)[i];
                    else
                        fields[i] = "Field" + (i + 1).ToString();
                }
                foreach (Control c in Helper.GetControl(groupBox3.Controls))
                    if (c is ComboBox)
                        (c as ComboBox).DataSource = new BindingSource() { DataSource = fields };
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int temp = comboBox5.SelectedIndex;
            comboBox5.SelectedIndex = comboBox4.SelectedIndex;
            comboBox4.SelectedIndex = temp;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ComputeParameters();
        }

        private void ComputeParameters()
        {
            try
            {
                foreach (Control n in Helper.GetControl(Controls))
                    if (n is ListBox && (n as ListBox).DataSource == null)
                        throw new Exception("Please ensure all coordinate entries are provided");
                foreach (Control c in Helper.GetControl(Controls))
                    if (c is ListBox)
                        foreach (Control m in Helper.GetControl(Controls))
                            if (m is ListBox && m != c)
                                if ((c as ListBox).Items.Count != (m as ListBox).Items.Count)
                                    throw new Exception("Non conformal input coordinates. " +
                                        "\nPlease ensure the number of Source coordinates and target coordinates are equal");
                foreach (Control c in Helper.GetControl(groupBox2.Controls))
                    if (c is ComboBox)
                        foreach (Control m in Helper.GetControl(groupBox2.Controls))
                            if (m is ComboBox && m != c)
                                if ((c as ComboBox).SelectedIndex == (m as ComboBox).SelectedIndex)
                                    throw new Exception("Source coordinate fields cannot be the same!");
                foreach (Control c in Helper.GetControl(groupBox3.Controls))
                    if (c is ComboBox)
                        foreach (Control m in Helper.GetControl(groupBox3.Controls))
                            if (m is ComboBox && m != c)
                                if ((c as ComboBox).SelectedIndex == (m as ComboBox).SelectedIndex)
                                    throw new Exception("Target coordinate fields cannot be the same!");
                foreach (Control c in Helper.GetControl(Controls))
                    if (c is ListBox)
                        foreach (string v in (c as ListBox).Items)
                            if (double.TryParse(v, out _))
                                continue;
                            else
                                throw new Exception(string.Format("Invalid entry {0} at {1}x{2}", v, int.Parse(c.Tag.ToString()) +
                                    int.Parse((c as ListBox).Parent.Tag.ToString()), (c as ListBox).Items.IndexOf(v)));
                x = DefineParameters();
                textBox1.Text = string.Empty;
                for (int i = 0; i < x.Length; i++)
                    textBox1.Text += string.Format("{0} = {1}", (char)(i + 97), x[i][0].ToString()) + (i == x.Length - 1 ? "" : Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private double[][] DefineParameters()
        {
            #region Forming A matrix
            double[][] A = new double[lb1.Items.Count * 2][];
            int a = 0;
            switch (equation)
            {
                case Equation.Conformal:
                    for (int i = 0; i < A.Length; i++)
                    {
                        if (i % 2 == 0 && i != 0)
                            a++;
                        string[] coord = new string[] { lb1.Items[a].ToString(), lb2.Items[a].ToString() };
                        A[i] = new double[4];  // 4 parameters equation
                        for (int j = 0; j < A[0].Length; j++)
                        {
                            if (i % 2 == 0)
                            {
                                if (j <= 1)
                                    A[i][j] = (j == 0) ? double.Parse(coord[0].ToString()) : -double.Parse(coord[1].ToString());
                                else
                                    A[i][j] = (j == 2) ? 1 : 0;
                            }
                            else
                            {
                                if (j <= 1)
                                    A[i][j] = (j == 0) ? double.Parse(coord[1].ToString()) : double.Parse(coord[0].ToString());
                                else
                                    A[i][j] = (j == 2) ? 0 : 1;
                            }
                        }
                    }
                    break;
                case Equation.Affine:
                    for (int i = 0; i < A.Length; i++)
                    {
                        if (i % 2 == 0 && i != 0)
                            a++;
                        string[] coord = new string[] { lb1.Items[a].ToString(), lb2.Items[a].ToString() };
                        A[i] = new double[6];  //parameters equation
                        for (int j = 0; j < A[0].Length; j++)
                        {
                            if (i % 2 == 0)
                            {
                                if (j <= 1)
                                    A[i][j] = (j == 0) ? double.Parse(coord[0].ToString()) : double.Parse(coord[1].ToString());
                                else
                                    A[i][j] = (j == 2) ? 1 : 0;
                            }
                            else
                            {
                                if (j >= 3 && j <= 4)
                                    A[i][j] = (j == 3) ? double.Parse(coord[0].ToString()) : double.Parse(coord[1].ToString());
                                else
                                    A[i][j] = (j == A[0].Length - 1) ? 1 : 0;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            #endregion

            #region Forming L matrix
            int k = 0;
            double[][] _L = new double[lb3.Items.Count * 2][];
            for (int i = 0; i < _L.Length; i++)
            {
                if (i % 2 == 0 && i != 0)
                    k++;
                string[] _coord = new string[] { lb3.Items[k].ToString(), lb4.Items[k].ToString() };
                _L[i] = new double[1];
                for (int j = 0; j < _L[0].Length; j++)
                {
                    _L[i][j] = (i % 2 == 0) ? double.Parse(_coord[0].ToString()) : double.Parse(_coord[1].ToString());
                }
            }
            #endregion

            //
            double[][] x = Matrix.Mul(Matrix.Inv(Matrix.Mul(Matrix.Trans(A), A)), Matrix.Mul(Matrix.Trans(A), _L));

            return x;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBox1.Text))
                    PrepareConversionLog();
                else
                    throw new Exception("Compute parameters first");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PrepareConversionLog()
        {
            string path = Path.GetFullPath(label2.Text).Replace(label8.Text, "");
            string sourcefile = Path.GetFileName(label2.Text).Replace(Path.GetExtension(label2.Text), "");
            string targetfile = Path.GetFileName(label15.Text).Replace(Path.GetExtension(label15.Text), "");
            List<string> log = new List<string>();
            log.Capacity = x.Length + 2;
            log.Add(string.Format("\t\t\t{0}", Application.ProductName));
            log.Add(string.Format("Transformation Equation:{0}\t\t\tLog Time{1}", cb1.SelectedItem.ToString(), DateTime.Now.ToString()));
            log.Add(string.Format("Parameters for converting from {0} to {1}", sourcefile, targetfile));
            log.Add(textBox1.Text);
            LogForm fm = new LogForm(path + "Parameters_log.txt", log);
            fm.ShowDialog();
        }
    }
}
