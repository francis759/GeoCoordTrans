using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoCoordTrans_v2._1
{
    public partial class LogForm : Form
    {
        int _export = 0;
        List<string> exporttype = new List<string>() { "Complete Log", "Parameters only" };
        private List<string> Log { get; set; }
        private string filepath { get; set; }

        public LogForm(string _filepath, List<string>_log)
        {
            InitializeComponent();
            label2.Text = filepath = _filepath;
            Log = _log;
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = exporttype;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog save = new SaveFileDialog())
            {
                save.Title = "Select file";
                save.Filter = "TextFile .txt|*.txt";
                save.FileName = new FileInfo(filepath).Name;
                if (save.ShowDialog() == DialogResult.OK)
                    label2.Text = save.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (BackgroundWorker work = new BackgroundWorker())
            {
                try
                {
                    if (Log.Count != 0)
                    {
                        work.DoWork += Work_DoWork;
                        work.RunWorkerCompleted += Work_RunWorkerCompleted;
                        work.RunWorkerAsync();
                    }
                    else
                        throw new Exception("Log file write error");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (checkBox1.Checked)
                if(new FileInfo(label2.Text).Exists)
                    Process.Start(label2.Text);

            Close();
        }

        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            using (StreamWriter ws = new StreamWriter(label2.Text))
            {
                for (int i = _export == 1 ? 3 : 0; i < Log.Count; i++)
                    ws.WriteLine(Log[i]);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _export = (sender as ComboBox).SelectedIndex;
        }
    }
}
