using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using GeoCoordTrans_v2._1.Properties;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using Microsoft.Win32;
using System.Drawing;
using System.Linq;
using System.IO;

namespace GeoCoordTrans_v2._1
{
    public partial class mainform : Form
    {
        RegistryKey Key;
        private string _url = "https://www.google.com/maps/place/";
        internal static int rbL { get; set; }
        internal static int rbR { get; set; }
        internal static int UnitL { get; set; }
        internal static int UnitR { get; set; }
        MPoint P1 = new MPoint();
        MPoint P2 = new MPoint();
        #region Datum and Grid Properties
        internal static string DatumL { get; set; }
        internal static string EllipsoidL { get; set; }
        internal static string GridL { get; set; }
        internal static string DatumR { get; set; }
        internal static string EllipsoidR { get; set; }
        internal static string GridR { get; set; }
        //
        internal static string DatumRegionL { get; set; }
        internal static string EllipsoidRegionL { get; set; }
        internal static string GridRegionL { get; set; }
        internal static string DatumRegionR { get; set; }
        internal static string EllipsoidRegionR { get; set; }
        internal static string GridRegionR { get; set; }
        #endregion

        public mainform()
        {
            InitializeComponent();
        }

        private void mainform_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            RegistryKey productkey = Registry.CurrentUser.CreateSubKey(Settings.Default.Protection);
            if (productkey != null && productkey.GetValue("Password")!= null && productkey.GetValue("Password").ToString() == Program.ProductKey)
                registerProductToolStripMenuItem.Visible = false;
            else
                registerProductToolStripMenuItem.Visible = true;

            foreach (Control c in Helper.GetControl(Controls))
                if (c is GMapControl)
                {
                    (c as GMapControl).ShowCenter = false;
                    (c as GMapControl).CacheLocation = "MapCache";
                }
            if ((Key = Registry.CurrentUser.OpenSubKey(Settings.Default.register)) != null)
                LoadLastSettings(Key);
            else
                LoadDefaultSettings();
        }

        private void LoadLastSettings(RegistryKey key)
        {
            P1 = new MPoint(); P2 = new MPoint();
            P1.X = double.Parse(key.GetValue("LongitudeL").ToString());
            P1.Y = double.Parse(key.GetValue("LatitudeL").ToString());
            P1.Z = double.Parse(key.GetValue("AltitudeL").ToString());
            //
            P2.X = double.Parse(key.GetValue("LongitudeR").ToString());
            P2.Y = double.Parse(key.GetValue("LatitudeR").ToString());
            P2.Z = double.Parse(key.GetValue("AltitudeR").ToString());

            #region Datum and Grid Values
            DatumL = key.GetValue("DatumL").ToString();
            DatumR = key.GetValue("DatumR").ToString();
            DatumRegionL = key.GetValue("DatumRegionL").ToString();
            DatumRegionR = key.GetValue("DatumRegionR").ToString();
            GridL = key.GetValue("GridL").ToString();
            GridR = key.GetValue("GridR").ToString();
            GridRegionL = key.GetValue("GridRegionL").ToString();
            GridRegionR = key.GetValue("GridRegionR").ToString();
            #endregion

            foreach (Control m in Helper.GetControl(Controls))
            {
                if (m is GroupBox)
                    foreach (Control c in Helper.GetControl(m.Controls))
                        if (c is RadioButton)
                            if ( c.Tag!=null && c.Tag.ToString() == key.GetValue("GUIMode").ToString())
                                (c as RadioButton).Checked = true;
                //
                if (m is ComboBox)
                {
                    (m as ComboBox).DisplayMember = "Name";
                    (m as ComboBox).ValueMember = "Value";
                    BindingSource d = new BindingSource() { DataSource = Helper.GetTable("Unit"), Sort = "OrderNum ASC" };
                    //
                    if (m.Tag.ToString() == "coordtype1")
                    {
                        (m as ComboBox).DataSource = new BindingSource() { DataSource = Helper.GetTable("CoordinateType"), Sort = "Value ASC" };
                        (m as ComboBox).SelectedIndex = int.Parse(key.GetValue("rbL").ToString());
                    }
                    else if (m.Tag.ToString() == "coordtype2")
                    {
                        (m as ComboBox).DataSource = new BindingSource() { DataSource = Helper.GetTable("CoordinateType"), Sort = "Value ASC" };
                        (m as ComboBox).SelectedIndex = int.Parse(key.GetValue("rbR").ToString());
                    }
                    else if (m.Tag.ToString() == "UnitM")
                    {
                        (m as ComboBox).DataSource = d;
                        (m as ComboBox).SelectedIndex = int.Parse(key.GetValue("UnitM").ToString());
                    }
                    else if (m.Tag.ToString() == "UnitL")
                    {
                        (m as ComboBox).DataSource = d;
                        (m as ComboBox).SelectedIndex = int.Parse(key.GetValue("UnitL").ToString());
                    }
                    else if (m.Tag.ToString() == "UnitR")
                    {
                        (m as ComboBox).DataSource = d;
                        (m as ComboBox).SelectedIndex = int.Parse(key.GetValue("UnitR").ToString());
                    }
                    else if (m.Tag.ToString() == "AltUnitL")
                    {
                        (m as ComboBox).DataSource = d;
                        (m as ComboBox).SelectedIndex = int.Parse(key.GetValue("AltUnitL").ToString());
                    }
                    else if (m.Tag.ToString() == "AltUnitR")
                    {
                        (m as ComboBox).DataSource = d;
                        (m as ComboBox).SelectedIndex = int.Parse(key.GetValue("AltUnitR").ToString());
                    }
                    else
                        (m as ComboBox).DataSource = new BindingSource() { DataSource = Helper.GetTable("Maps") };
                }
                //
                if (m is GMapControl)
                {
                    (m as GMapControl).Overlays.Clear();
                    GMapMarker m1 = new GMarkerGoogle(new PointLatLng(P1.Y, P1.X), new Bitmap(Resources.pic1)) { Tag = "0" };
                    GMapMarker m2 = new GMarkerGoogle(new PointLatLng(P2.Y, P2.X), new Bitmap(Resources.pic2)) { Tag = "1" };
                    GMapOverlay l = new GMapOverlay();
                    l.Markers.Add(m1); l.Markers.Add(m2);
                    (m as GMapControl).Overlays.Add(l);
                    (m as GMapControl).Position = (m as GMapControl).Tag.ToString() != "2" ? (m as GMapControl).Tag.ToString() == "0" ? new PointLatLng(P1.Y, P1.X) :
                        new PointLatLng(P2.Y, P2.X) : new PointLatLng(0, 0);
                }

                if (m is HScrollBar)
                {
                    if (m.Tag.ToString() == "0")
                        (m as HScrollBar).Value = int.Parse(key.GetValue("ZoomL").ToString());
                    else
                        (m as HScrollBar).Value = int.Parse(key.GetValue("ZoomR").ToString());
                    ZoomMap(m);
                }
            }
        }

        private void LoadDefaultSettings()
        {
            P1.X = 0; P1.Y = 0; P1.Z = 0;
            P2 = P1;

            #region Datum and Grid Properties
            DatumL = "GAMF";
            EllipsoidL = "GAMF";
            GridL = "CCJB";
            DatumR = "BADU";
            EllipsoidR = "BADU";
            GridR = "UZUM";
            //
            DatumRegionL = "WUBA";
            EllipsoidRegionL = "WUBA";
            GridRegionL = "YMMX";
            DatumRegionR = "YMMX";
            EllipsoidRegionR = "YMMX";
            GridRegionR = "YMMX";
            #endregion

            foreach (Control c in Helper.GetControl(Controls))
            {
                if (c is GroupBox)
                    foreach (Control m in Helper.GetControl(c.Controls))
                        if (m is RadioButton)
                            if (m.Tag != null && m.Tag.ToString() == "0")
                                (m as RadioButton).Checked = true;

                if (c is ComboBox)
                {
                    (c as ComboBox).DisplayMember = "Name";
                    (c as ComboBox).ValueMember = "Value";
                    if (c.Tag.ToString() == "coordtype1"|| c.Tag.ToString() == "coordtype2")
                        (c as ComboBox).DataSource = new BindingSource() { DataSource = Helper.GetTable("CoordinateType")};
                    else if(c.Tag.ToString()=="UnitL"|| c.Tag.ToString() == "UnitR"||c.Tag.ToString() == "UnitM"||c.Tag.ToString() == "AltUnitL" 
                        || c.Tag.ToString() == "AltUnitR")
                        (c as ComboBox).DataSource = new BindingSource() { DataSource = Helper.GetTable("Unit"), Sort = "OrderNum ASC" };
                    else
                        (c as ComboBox).DataSource = new BindingSource() { DataSource = Helper.GetTable("Maps") };
                }
                if(c is HScrollBar)
                {
                    (c as HScrollBar).Value = 50;
                    ZoomMap(c);
                }
                if(c is GMapControl)
                {
                    (c as GMapControl).Overlays.Clear();
                    GMapMarker m1 = new GMarkerGoogle(new PointLatLng(P1.Y, P1.X), Resources.pic1) { Tag = 0 };
                    GMapMarker m2 = new GMarkerGoogle(new PointLatLng(P2.Y, P2.X), Resources.pic2) { Tag = 1 };
                    GMapOverlay layer = new GMapOverlay();
                    layer.Markers.Add(m1);
                    layer.Markers.Add(m2);
                    (c as GMapControl).Overlays.Add(layer);
                    (c as GMapControl).Position = c.Tag.ToString() == "Map2" ? new PointLatLng(0, 0) : m1.Position;
                }
            }
            ShowBearingAndDistance();
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rbL = (sender as ComboBox).SelectedIndex;
            MPoint p;
            foreach (Control m in groupBox1.Controls)
                if (m is Panel && m.Tag.ToString() == (sender as ComboBox).SelectedValue.ToString())
                    (m as Panel).BringToFront();
            switch((sender as ComboBox).SelectedIndex)
            {
                case 0:
                    RegionlblL.Text = Helper.GetRegion(DatumRegionL);
                    geodatumlblL.Text = Helper.GetDatum(DatumL);
                    p = Transform.GeoToGeo("GAMF", DatumL, P1);
                    st1.Text = lngL.Text = Math.Round(p.X, 7).ToString();
                    st2.Text = latL.Text = Math.Round(p.Y, 7).ToString();
                    p.Z /= double.Parse(cb4.SelectedValue.ToString());
                    //
                    hL.Text = Math.Round(p.Z, 2).ToString();
                    break;
                case 1:
                    RegionlblL.Text = Helper.GetRegion(GridRegionL);
                    progridlblL.Text = Helper.GetGrid(GridL);
                    p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), P1);
                    p = Transform.GeoToPro(GridL, p);
                    p.X /= double.Parse(cb2.SelectedValue.ToString());
                    p.Y /= double.Parse(cb2.SelectedValue.ToString());
                    p.Z /= double.Parse(cb2.SelectedValue.ToString());
                    //
                    st1.Text = eastL.Text = Math.Round(p.X, 2).ToString();
                    st2.Text = northL.Text = Math.Round(p.Y, 2).ToString();
                    break;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked == true)
                panel8.Visible = panel4.Visible = (sender as RadioButton).Tag.ToString() == "1";
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Control c in Helper.GetControl(Controls))
                if (c is GMapControl && c.Tag == (sender as ComboBox).Tag)
                    (c as GMapControl).MapProvider = GMapProviders.List[int.Parse((sender as ComboBox).SelectedValue.ToString())];
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            ZoomMap(sender);
        }

        private void ZoomMap(object sender)
        {
            foreach(Control c in (sender as HScrollBar).Parent.Controls)
            {
                if (c is GMapControl)
                    (c as GMapControl).Zoom = (0.2 * (sender as HScrollBar).Value) - 2;
                if (c is Label)
                    c.Text = string.Format("{0}%", (sender as HScrollBar).Value);
            }
        }

        private void resetToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "Are you sure you want to reset to defaults?";
            if (MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                LoadDefaultSettings();
        }

        private void exitApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            rbR = (sender as ComboBox).SelectedIndex;
            MPoint p;
            foreach (Control m in groupBox5.Controls)
                if (m is Panel && m.Tag.ToString() == (sender as ComboBox).SelectedValue.ToString())
                    (m as Panel).BringToFront();
            switch ((sender as ComboBox).SelectedIndex)
            {
                case 0:
                    RegionlblR.Text = Helper.GetRegion(DatumRegionR);
                    geodatumlblR.Text = Helper.GetDatum(DatumR);
                    p = Transform.GeoToGeo("GAMF", DatumR, P2);
                    st3.Text = lngR.Text = Math.Round(p.X, 7).ToString();
                    st4.Text = latR.Text = Math.Round(p.Y, 7).ToString();
                    p.Z /= double.Parse(cb7.SelectedValue.ToString());
                    //
                    hR.Text = Math.Round(p.Z, 2).ToString();
                    break;
                case 1:
                    RegionlblR.Text = Helper.GetRegion(GridRegionR);
                    progridlblR.Text = Helper.GetGrid(GridR);
                    p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), P2);
                    p = Transform.GeoToPro(GridR, p);
                    p.X /= double.Parse(cb6.SelectedValue.ToString());
                    p.Y /= double.Parse(cb6.SelectedValue.ToString());
                    p.Z /= double.Parse(cb6.SelectedValue.ToString());
                    //
                    st3.Text = eastR.Text = Math.Round(p.X, 2).ToString();
                    st4.Text = northR.Text = Math.Round(p.Y, 2).ToString();
                    break;
            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About abt = new About();
            abt.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dForm df = new dForm(DatumRegionL, DatumL);
            if (df.ShowDialog() == DialogResult.OK)
            {
                MPoint p;
                DatumRegionL = df.region;
                DatumL = df.datum;
                RegionlblL.Text = Helper.GetRegion(DatumRegionL);
                geodatumlblL.Text = Helper.GetDatum(DatumL);
                p = Transform.GeoToGeo("GAMF", DatumL, P1);
                //
                st1.Text = lngL.Text = Math.Round(p.X, 7).ToString();
                st2.Text = latL.Text = Math.Round(p.Y, 7).ToString();
                p.Z /= double.Parse(cb4.SelectedValue.ToString());
                //
                hL.Text = Math.Round(p.Z, 2).ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            gForm gf = new gForm(GridRegionL, GridL);
            if (gf.ShowDialog() == DialogResult.OK)
            {
                MPoint p;
                GridRegionL = gf.region;
                GridL = gf.grid;
                RegionlblL.Text = Helper.GetRegion(GridRegionL);
                progridlblL.Text = Helper.GetGrid(GridL);
                p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), P1);
                p = Transform.GeoToPro(GridL, p);
                p.X /= double.Parse(cb2.SelectedValue.ToString());
                p.Y /= double.Parse(cb2.SelectedValue.ToString());
                p.Z /= double.Parse(cb2.SelectedValue.ToString());
                //
                st1.Text = eastL.Text = Math.Round(p.X, 2).ToString();
                st2.Text = northL.Text = Math.Round(p.Y, 2).ToString();
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            gForm gf = new gForm(GridRegionR, GridR);
            if (gf.ShowDialog() == DialogResult.OK)
            {
                MPoint p;
                GridRegionR = gf.region;
                GridR = gf.grid;
                RegionlblR.Text = Helper.GetRegion(GridRegionR);
                progridlblR.Text = Helper.GetGrid(GridR);
                p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), P2);
                p = Transform.GeoToPro(GridR, p);
                p.X /= double.Parse(cb6.SelectedValue.ToString());
                p.Y /= double.Parse(cb6.SelectedValue.ToString());
                p.Z /= double.Parse(cb6.SelectedValue.ToString());
                //
                st3.Text = eastR.Text = Math.Round(p.X, 2).ToString();
                st4.Text = northR.Text = Math.Round(p.Y, 2).ToString();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dForm df = new dForm(DatumRegionR, DatumR);
            if (df.ShowDialog() == DialogResult.OK)
            {
                MPoint p;
                DatumRegionR = df.region;
                DatumR = df.datum;
                RegionlblR.Text = Helper.GetRegion(DatumRegionR);
                geodatumlblR.Text = Helper.GetDatum(DatumR);
                p = Transform.GeoToGeo("GAMF", DatumR, P2);
                st3.Text = lngR.Text = Math.Round(p.X, 7).ToString();
                st4.Text = latR.Text = Math.Round(p.Y, 7).ToString();
                p.Z /= double.Parse(cb7.SelectedValue.ToString());
                //
                hR.Text = Math.Round(p.Z, 2).ToString();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (Control c in Helper.GetControl(Controls))
                if (c is GMapControl && c.Tag == (sender as Button).Tag)
                    (c as GMapControl).Position = new PointLatLng(0, 0);
        }

        private void cb3_SelectedIndexChanged(object sender, EventArgs e)
        {
            MPoint p;
            switch ((sender as Control).Tag.ToString())
            {
                case "AltUnitL":
                    p = Transform.GeoToGeo("GAMF", DatumL, P1);
                    st1.Text = lngL.Text = Math.Round(p.X, 7).ToString();
                    st2.Text = latL.Text = Math.Round(p.Y, 7).ToString();
                    p.Z /= double.Parse((sender as ComboBox).SelectedValue.ToString());
                    //
                    hL.Text = Math.Round(p.Z, 2).ToString();
                    break;
                case "UnitL":
                    UnitL = (sender as ComboBox).SelectedIndex;
                    p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), P1);
                    p = Transform.GeoToPro(GridL, p);
                    p.X /= double.Parse((sender as ComboBox).SelectedValue.ToString());
                    p.Y /= double.Parse((sender as ComboBox).SelectedValue.ToString());
                    p.Z /= double.Parse((sender as ComboBox).SelectedValue.ToString());
                    //
                    st1.Text = eastL.Text = Math.Round(p.X, 2).ToString();
                    st2.Text = northL.Text = Math.Round(p.Y, 2).ToString();
                    break;
                default:
                    break;
            }
        }

        private void cb6_SelectedIndexChanged(object sender, EventArgs e)
        {
            MPoint p;
            switch ((sender as Control).Tag.ToString())
            {
                case "AltUnitR":
                    p = Transform.GeoToGeo("GAMF", DatumR, P2);
                    st3.Text = lngR.Text = Math.Round(p.X, 7).ToString();
                    st4.Text = latR.Text = Math.Round(p.Y, 7).ToString();
                    p.Z /= double.Parse((sender as ComboBox).SelectedValue.ToString());
                    //
                    hR.Text = Math.Round(p.Z, 2).ToString();
                    break;
                case "UnitR":
                    UnitR = (sender as ComboBox).SelectedIndex;
                    p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), P2);
                    p = Transform.GeoToPro(GridR, p);
                    p.X /= double.Parse((sender as ComboBox).SelectedValue.ToString());
                    p.Y /= double.Parse((sender as ComboBox).SelectedValue.ToString());
                    p.Z /= double.Parse((sender as ComboBox).SelectedValue.ToString());
                    //
                    st3.Text = eastR.Text = Math.Round(p.X, 2).ToString();
                    st4.Text = northR.Text = Math.Round(p.Y, 2).ToString();
                    break;
                default:
                    break;
            }
        }

        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                int v;
                PointLatLng t = (sender as GMapControl).FromLocalToLatLng(e.X, e.Y);
                v = (sender as GMapControl).Tag.ToString() != "2" ? int.Parse((sender as GMapControl).Tag.ToString()) : 2 - (e.Button == MouseButtons.Left ? 2 : 1);
                //
                foreach (Control c in Helper.GetControl(Controls))
                    if (c is GMapControl)
                        foreach (GMapOverlay l in (c as GMapControl).Overlays)
                            foreach (GMapMarker m in l.Markers)
                                if (m.Tag.ToString() == v.ToString())
                                    m.Position = t;
                foreach (Control c in Helper.GetControl(Controls))
                    if (c is GMapControl && (c as GMapControl).Tag.ToString() == v.ToString())
                        (c as GMapControl).Position = t;
                if (v == 0)
                    P1 = new MPoint(t.Lng, t.Lat, 0);
                else
                    P2 = new MPoint(t.Lng, t.Lat, 0);

                PointCoordinates(v.ToString());
                ShowBearingAndDistance();
            }
        }

        private void PointCoordinates(string v)
        {
            MPoint p;
            if (v == "0")
            {
                switch (cb1.SelectedIndex)
                {
                    case 0:
                        p = Transform.GeoToGeo("GAMF", DatumL, P1);
                        st1.Text = lngL.Text = Math.Round(p.X, 7).ToString();
                        st2.Text = latL.Text = Math.Round(p.Y, 7).ToString();
                        p.Z /= double.Parse(cb4.SelectedValue.ToString());
                        //
                        hL.Text = Math.Round(p.Z, 2).ToString();
                        break;
                    case 1:
                        p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), P1);
                        p = Transform.GeoToPro(GridL, p);
                        p.X /= double.Parse(cb2.SelectedValue.ToString());
                        p.Y /= double.Parse(cb2.SelectedValue.ToString());
                        p.Z /= double.Parse(cb2.SelectedValue.ToString());
                        //
                        st1.Text = eastL.Text = Math.Round(p.X, 2).ToString();
                        st2.Text = northL.Text = Math.Round(p.Y, 2).ToString();
                        break;
                }
            }
            else
            {
                switch (cb5.SelectedIndex)
                {
                    case 0:
                        p = Transform.GeoToGeo("GAMF", DatumR, P2);
                        st3.Text = lngR.Text = Math.Round(p.X, 7).ToString();
                        st4.Text = latR.Text = Math.Round(p.Y, 7).ToString();
                        p.Z /= double.Parse(cb7.SelectedValue.ToString());
                        //
                        hR.Text = Math.Round(p.Z, 2).ToString();
                        break;
                    case 1:
                        p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), P2);
                        p = Transform.GeoToPro(GridR, p);
                        p.X /= double.Parse(cb6.SelectedValue.ToString());
                        p.Y /= double.Parse(cb6.SelectedValue.ToString());
                        p.Z /= double.Parse(cb6.SelectedValue.ToString());
                        //
                        st3.Text = eastR.Text = Math.Round(p.X, 2).ToString();
                        st4.Text = northR.Text = Math.Round(p.Y, 2).ToString();
                        break;
                }
            }
        }

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowBearingAndDistance();
        }

        private void ShowBearingAndDistance()
        {
            brnglbl.Text = string.Format("{0} Deg",Math.Round(Helper.Bearing(P1, P2), 1).ToString());
            distlbl.Text = Math.Round(Helper.Distance(P1, P2) / double.Parse(cb12.SelectedValue.ToString()), 2).ToString();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            P2 = P1;
            MPoint p;
            switch (cb5.SelectedIndex)
            {
                case 0:
                    RegionlblR.Text = Helper.GetRegion(DatumRegionR);
                    geodatumlblR.Text = Helper.GetDatum(DatumR);
                    p = Transform.GeoToGeo("GAMF", DatumR, P2);
                    st3.Text = lngR.Text = Math.Round(p.X, 7).ToString();
                    st4.Text = latR.Text = Math.Round(p.Y, 7).ToString();
                    p.Z /= double.Parse(cb7.SelectedValue.ToString());
                    //
                    hR.Text = Math.Round(p.Z, 2).ToString();
                    break;
                case 1:
                    RegionlblR.Text = Helper.GetRegion(GridRegionR);
                    progridlblR.Text = Helper.GetGrid(GridR);
                    p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), P2);
                    p = Transform.GeoToPro(GridR, p);
                    p.X /= double.Parse(cb6.SelectedValue.ToString());
                    p.Y /= double.Parse(cb6.SelectedValue.ToString());
                    p.Z /= double.Parse(cb6.SelectedValue.ToString());
                    //
                    st3.Text = eastR.Text = Math.Round(p.X, 2).ToString();
                    st4.Text = northR.Text = Math.Round(p.Y, 2).ToString();
                    break;
            }
            FixMarker("1");
            ShowBearingAndDistance();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            P1 = P2;
            MPoint p;
            switch (cb1.SelectedIndex)
            {
                case 0:
                    RegionlblL.Text = Helper.GetRegion(DatumRegionL);
                    geodatumlblL.Text = Helper.GetDatum(DatumL);
                    p = Transform.GeoToGeo("GAMF", DatumL, P1);
                    st1.Text = lngL.Text = Math.Round(p.X, 7).ToString();
                    st2.Text = latL.Text = Math.Round(p.Y, 7).ToString();
                    p.Z /= double.Parse(cb4.SelectedValue.ToString());
                    //
                    hL.Text = Math.Round(p.Z, 2).ToString();
                    break;
                case 1:
                    RegionlblL.Text = Helper.GetRegion(GridRegionL);
                    progridlblL.Text = Helper.GetGrid(GridL);
                    p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), P1);
                    p = Transform.GeoToPro(GridL, p);
                    p.X /= double.Parse(cb2.SelectedValue.ToString());
                    p.Y /= double.Parse(cb2.SelectedValue.ToString());
                    p.Z /= double.Parse(cb2.SelectedValue.ToString());
                    //
                    st1.Text = eastL.Text = Math.Round(p.X, 2).ToString();
                    st2.Text = northL.Text = Math.Round(p.Y, 2).ToString();
                    break;
            }
            FixMarker("0");
            ShowBearingAndDistance();
        }

        private void FixMarker(string v)
        {
            PointLatLng temp = v == "0" ? new PointLatLng(P1.Y, P1.X) : new PointLatLng(P2.Y, P2.X);

            foreach (Control m in Helper.GetControl(Controls))
                if (m is GMapControl)
                    foreach (GMapOverlay n in (m as GMapControl).Overlays)
                        foreach (GMapMarker s in n.Markers)
                            if (s.Tag.ToString() == v)
                                s.Position = temp;
            foreach (Control m in Helper.GetControl(Controls))
                if (m is GMapControl && (m as GMapControl).Tag.ToString() == v)
                    (m as GMapControl).Position = temp;
        }

        private void transformationParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TransformParam transformParam = new TransformParam();
            transformParam.ShowDialog();
        }

        private void wIzardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Wizard wizard = new Wizard();
            wizard.ShowDialog();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            double lng = (sender as Button).Tag.ToString() == "0" ? P1.X : P2.X;
            double lat = (sender as Button).Tag.ToString() == "0" ? P1.Y : P2.Y;
            StringBuilder builder = new StringBuilder(_url);
            _ = builder.Append(string.Format("{0},{1}", lat, lng));
            Process.Start(builder.ToString());
        }

        private void mainform_FormClosing(object sender, FormClosingEventArgs e)
        {
            Key = Registry.CurrentUser.CreateSubKey(Settings.Default.register);
            if (Key != null)
            {
                Key.SetValue("LongitudeL", P1.X.ToString());
                Key.SetValue("LatitudeL", P1.Y.ToString());
                Key.SetValue("AltitudeL", P1.Z.ToString());

                Key.SetValue("LongitudeR", P2.X.ToString());
                Key.SetValue("LatitudeR", P2.Y.ToString());
                Key.SetValue("AltitudeR", P2.Z.ToString());

                #region Datum and Grid Values
                Key.SetValue("DatumL", DatumL);
                Key.SetValue("DatumR", DatumR);
                Key.SetValue("DatumRegionL", DatumRegionL);
                Key.SetValue("DatumRegionR", DatumRegionR);
                Key.SetValue("GridL", GridL);
                Key.SetValue("GridR", GridR);
                Key.SetValue("GridRegionL", GridRegionL);
                Key.SetValue("GridRegionR", GridRegionR);
                #endregion


                foreach (Control m in Helper.GetControl(Controls))
                {
                    if (m is GroupBox)
                        foreach (Control c in (m as GroupBox).Controls)
                            if (c is RadioButton && (c as RadioButton).Checked == true)
                                    Key.SetValue("GUIMode", c.Tag.ToString());
                    //
                    if (m is ComboBox)
                    {
                        if ((m as ComboBox).Tag.ToString() == "coordtype1")
                            Key.SetValue("rbL", (m as ComboBox).SelectedIndex.ToString());
                        if ((m as ComboBox).Tag.ToString() == "coordtype2")
                            Key.SetValue("rbR", (m as ComboBox).SelectedIndex.ToString());
                        if ((m as ComboBox).Tag.ToString() == "UnitM")
                            Key.SetValue("UnitM", (m as ComboBox).SelectedIndex.ToString());
                        if ((m as ComboBox).Tag.ToString() == "UnitL")
                            Key.SetValue("UnitL", (m as ComboBox).SelectedIndex.ToString());
                        if ((m as ComboBox).Tag.ToString() == "UnitR")
                            Key.SetValue("UnitR", (m as ComboBox).SelectedIndex.ToString());
                        if ((m as ComboBox).Tag.ToString() == "AltUnitL")
                            Key.SetValue("AltUnitL", (m as ComboBox).SelectedIndex.ToString());
                        if ((m as ComboBox).Tag.ToString() == "AltUnitR")
                            Key.SetValue("AltUnitR", (m as ComboBox).SelectedIndex.ToString());
                    }
                    //
                    if (m is HScrollBar)
                        if (m.Tag.ToString() == "0")
                            Key.SetValue("ZoomL", (m as HScrollBar).Value.ToString());
                        else
                            Key.SetValue("ZoomR", (m as HScrollBar).Value.ToString());
                }
            }
        }

        private void lngL_Leave(object sender, EventArgs e)
        {
            TextBox c = sender as TextBox;
            if (string.IsNullOrEmpty(c.Text))
                c.Text = 0.ToString();
            try
            {
                if (double.TryParse(c.Text, out _))
                {
                    if (c.Name == lngL.Name)
                    {
                        if (double.Parse(c.Text) <= 180 && double.Parse(c.Text) >= -180)
                        {
                            if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 7)
                                c.Text = Math.Round(double.Parse(c.Text), 7).ToString();
                            else
                                c.Text = double.Parse(c.Text).ToString();
                        }
                        else
                            throw new Exception("Invalid Longitude: Must be between -180 and 180");
                    }
                    if (c.Name == latL.Name)
                    {
                        if (double.Parse(c.Text) <= 90 && double.Parse(c.Text) >= -90)
                        {
                            if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 7)
                                c.Text = Math.Round(double.Parse(c.Text), 7).ToString();
                            else
                                c.Text = double.Parse(c.Text).ToString();
                        }
                        else
                            throw new Exception("Invalid Latitude: Must be between -90 and 90");
                    }
                    if (c.Name == hL.Name)
                    {
                        if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 2)
                            c.Text = Math.Round(double.Parse(c.Text), 2).ToString();
                        else
                            c.Text = double.Parse(c.Text).ToString();
                    }
                    //
                    st1.Text = lngL.Text;
                    st2.Text = latL.Text;
                    MPoint p = new MPoint(double.Parse(lngL.Text), double.Parse(latL.Text), double.Parse(hL.Text) * double.Parse(cb6.SelectedValue.ToString()));
                    P1 = Transform.GeoToGeo(DatumL, "GAMF", p);
                    FixMarker("0");
                    ShowBearingAndDistance();
                }
                else
                    throw new Exception(string.Format("{0} is not a number", c.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                c.Focus();
            }
        }

        private void lngR_Leave(object sender, EventArgs e)
        {
            TextBox c = sender as TextBox;
            if (string.IsNullOrEmpty(c.Text))
                c.Text = 0.ToString();
            try
            {
                if (double.TryParse(c.Text, out _))
                {
                    if (c.Name == lngR.Name)
                    {
                        if (double.Parse(c.Text) <= 180 && double.Parse(c.Text) >= -180)
                        {
                            if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 7)
                                c.Text = Math.Round(double.Parse(c.Text), 7).ToString();
                            else
                                c.Text = double.Parse(c.Text).ToString();
                        }
                        else
                            throw new Exception("Invalid Longitude: Must be between -180 and 180");
                    }
                    if (c.Name == latR.Name)
                    {
                        if (double.Parse(c.Text) <= 90 && double.Parse(c.Text) >= -90)
                        {
                            if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 7)
                                c.Text = Math.Round(double.Parse(c.Text), 7).ToString();
                            else
                                c.Text = double.Parse(c.Text).ToString();
                        }
                        else
                            throw new Exception("Invalid Latitude: Must be between -90 and 90");
                    }
                    if (c.Name == hR.Name)
                    {
                        if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 2)
                            c.Text = Math.Round(double.Parse(c.Text), 2).ToString();
                        else
                            c.Text = c.Text;
                    }
                    //
                    st3.Text = lngR.Text;
                    st4.Text = latR.Text;
                    MPoint p = new MPoint(double.Parse(lngR.Text), double.Parse(latR.Text), double.Parse(hR.Text) * double.Parse(cb7.SelectedValue.ToString()));
                    P2 = Transform.GeoToGeo(DatumR, "GAMF", p);
                    FixMarker("1");
                    ShowBearingAndDistance();
                }
                else
                    throw new Exception(string.Format("{0} is not a number", c.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                c.Focus();
            }
        }

        private void eastL_Leave(object sender, EventArgs e)
        {
            TextBox c = sender as TextBox;
            if (string.IsNullOrEmpty(c.Text))
                c.Text = 0.ToString();
            try
            {
                if (double.TryParse(c.Text, out _))
                {
                    if (c.Name == eastL.Name)
                    {
                        if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 2)
                            c.Text = Math.Round(double.Parse(c.Text), 2).ToString();
                        else
                            c.Text = double.Parse(c.Text).ToString();
                    }
                    if (c.Name == northL.Name)
                    {
                        if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 2)
                            c.Text = Math.Round(double.Parse(c.Text), 2).ToString();
                        else
                            c.Text = double.Parse(c.Text).ToString();
                    }
                    st1.Text = eastL.Text;
                    st2.Text = northL.Text;
                    MPoint p = new MPoint(double.Parse(eastL.Text) * double.Parse(cb1.SelectedValue.ToString()),
                        double.Parse(northL.Text) * double.Parse(cb1.SelectedValue.ToString()), 0);
                    p = Transform.ProToGeo(GridL, p);
                    p = Transform.GeoToGeo(Helper.GetGridDatumID(GridL), "GAMF", p);
                    P1.X = Math.Round(p.X, 7);
                    P1.Y = Math.Round(p.Y, 7);
                    P1.Z = Math.Round(p.Z, 2);
                    FixMarker("0");
                    ShowBearingAndDistance();
                }
                else
                    throw new Exception(string.Format("{0} is not a number", c.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                c.Focus();
            }
        }

        private void eastR_Leave(object sender, EventArgs e)
        {
            TextBox c = sender as TextBox;
            if (string.IsNullOrEmpty(c.Text))
                c.Text = 0.ToString();
            try
            {
                if (double.TryParse(c.Text, out _))
                {
                    if (c.Name == eastR.Name)
                    {
                        if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 2)
                            c.Text = Math.Round(double.Parse(c.Text), 2).ToString();
                        else
                            c.Text = double.Parse(c.Text).ToString();
                    }
                    if (c.Name == northR.Name)
                    {
                        if (c.Text.Contains('.') && c.Text.Substring(c.Text.IndexOf('.') + 1).Length > 2)
                            c.Text = Math.Round(double.Parse(c.Text), 2).ToString();
                        else
                            c.Text = double.Parse(c.Text).ToString();
                    }
                    st3.Text = eastR.Text;
                    st4.Text = northR.Text;
                    MPoint p = new MPoint(double.Parse(eastR.Text) * double.Parse(cb5.SelectedValue.ToString()),
                        double.Parse(northR.Text) * double.Parse(cb5.SelectedValue.ToString()), 0);
                    p = Transform.ProToGeo(GridR, p);
                    p = Transform.GeoToGeo(Helper.GetGridDatumID(GridR), "GAMF", p);
                    P2.X = Math.Round(p.X, 7);
                    P2.Y = Math.Round(p.Y, 7);
                    P2.Z = Math.Round(p.Z, 2);
                    FixMarker("1");
                    ShowBearingAndDistance();
                }
                else
                    throw new Exception(string.Format("{0} is not a number", c.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                c.Focus();
            }
        }

        private void gMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            MPoint p = new MPoint((sender as GMapControl).FromLocalToLatLng(e.X, e.Y).Lng, (sender as GMapControl).FromLocalToLatLng(e.X, e.Y).Lat, 0);
            //
            switch ((sender as GMapControl).Tag.ToString())
            {
                case "0":
                    switch (cb1.SelectedIndex)
                    {
                        case 0:
                            p = Transform.GeoToGeo("GAMF", DatumL, p);
                            st1.Text = Math.Round(p.X, 7).ToString();
                            st2.Text = Math.Round(p.Y, 7).ToString();
                            break;
                        case 1:
                            p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), p);
                            p = Transform.GeoToPro(GridL, p);
                            p.X /= double.Parse(cb2.SelectedValue.ToString());
                            p.Y /= double.Parse(cb2.SelectedValue.ToString());
                            p.Z /= double.Parse(cb2.SelectedValue.ToString());
                            //
                            st1.Text = Math.Round(p.X, 2).ToString();
                            st2.Text = Math.Round(p.Y, 2).ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case "1":
                    switch (cb5.SelectedIndex)
                    {
                        case 0:
                            p = Transform.GeoToGeo("GAMF", DatumR, p);
                            st3.Text = Math.Round(p.X, 7).ToString();
                            st4.Text = Math.Round(p.Y, 7).ToString();
                            break;
                        case 1:
                            p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), p);
                            p = Transform.GeoToPro(GridR, p);
                            p.X /= double.Parse(cb6.SelectedValue.ToString());
                            p.Y /= double.Parse(cb6.SelectedValue.ToString());
                            p.Z /= double.Parse(cb6.SelectedValue.ToString());
                            //
                            st3.Text = Math.Round(p.X, 2).ToString();
                            st4.Text = Math.Round(p.Y, 2).ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case "2":
                    switch (cb1.SelectedIndex)
                    {
                        case 0:
                            p = Transform.GeoToGeo("GAMF", DatumL, p);
                            st1.Text = Math.Round(p.X, 7).ToString();
                            st2.Text = Math.Round(p.Y, 7).ToString();
                            break;
                        case 1:
                            p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), p);
                            p = Transform.GeoToPro(GridL, p);
                            p.X /= double.Parse(cb2.SelectedValue.ToString());
                            p.Y /= double.Parse(cb2.SelectedValue.ToString());
                            p.Z /= double.Parse(cb2.SelectedValue.ToString());
                            //
                            st1.Text = Math.Round(p.X, 2).ToString();
                            st2.Text = Math.Round(p.Y, 2).ToString();
                            break;
                        default:
                            break;
                    }
                    switch (cb5.SelectedIndex)
                    {
                        case 0:
                            p = Transform.GeoToGeo("GAMF", DatumR, p);
                            st3.Text = Math.Round(p.X, 7).ToString();
                            st4.Text = Math.Round(p.Y, 7).ToString();
                            break;
                        case 1:
                            p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), p);
                            p = Transform.GeoToPro(GridR, p);
                            p.X /= double.Parse(cb6.SelectedValue.ToString());
                            p.Y /= double.Parse(cb6.SelectedValue.ToString());
                            p.Z /= double.Parse(cb6.SelectedValue.ToString());
                            //
                            st3.Text = Math.Round(p.X, 2).ToString();
                            st4.Text = Math.Round(p.Y, 2).ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void gMapControl1_MouseLeave(object sender, EventArgs e)
        {
            MPoint p;
            switch ((sender as GMapControl).Tag.ToString())
            {
                case "0":
                    switch (cb1.SelectedIndex)
                    {
                        case 0:
                            p = Transform.GeoToGeo("GAMF", DatumL, P1);
                            st1.Text = Math.Round(p.X, 7).ToString();
                            st2.Text = Math.Round(p.Y, 7).ToString();
                            break;
                        case 1:
                            p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), P1);
                            p = Transform.GeoToPro(GridL, p);
                            p.X /= double.Parse(cb2.SelectedValue.ToString());
                            p.Y /= double.Parse(cb2.SelectedValue.ToString());
                            p.Z /= double.Parse(cb2.SelectedValue.ToString());
                            //
                            st1.Text = Math.Round(p.X, 2).ToString();
                            st2.Text = Math.Round(p.Y, 2).ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case "1":
                    switch (cb5.SelectedIndex)
                    {
                        case 0:
                            p = Transform.GeoToGeo("GAMF", DatumR, P2);
                            st3.Text = Math.Round(p.X, 7).ToString();
                            st4.Text = Math.Round(p.Y, 7).ToString();
                            break;
                        case 1:
                            p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), P2);
                            p = Transform.GeoToPro(GridR, p);
                            p.X /= double.Parse(cb6.SelectedValue.ToString());
                            p.Y /= double.Parse(cb6.SelectedValue.ToString());
                            p.Z /= double.Parse(cb6.SelectedValue.ToString());
                            //
                            st3.Text = Math.Round(p.X, 2).ToString();
                            st4.Text = Math.Round(p.Y, 2).ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case "2":
                    switch (cb1.SelectedIndex)
                    {
                        case 0:
                            p = Transform.GeoToGeo("GAMF", DatumL, P1);
                            st1.Text = Math.Round(p.X, 7).ToString();
                            st2.Text = Math.Round(p.Y, 7).ToString();
                            break;
                        case 1:
                            p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridL), P1);
                            p = Transform.GeoToPro(GridL, p);
                            p.X /= double.Parse(cb2.SelectedValue.ToString());
                            p.Y /= double.Parse(cb2.SelectedValue.ToString());
                            p.Z /= double.Parse(cb2.SelectedValue.ToString());
                            //
                            st1.Text = Math.Round(p.X, 2).ToString();
                            st2.Text = Math.Round(p.Y, 2).ToString();
                            break;
                        default:
                            break;
                    }
                    switch (cb5.SelectedIndex)
                    {
                        case 0:
                            p = Transform.GeoToGeo("GAMF", DatumR, P2);
                            st3.Text = Math.Round(p.X, 7).ToString();
                            st4.Text = Math.Round(p.Y, 7).ToString();
                            break;
                        case 1:
                            p = Transform.GeoToGeo("GAMF", Helper.GetGridDatumID(GridR), P2);
                            p = Transform.GeoToPro(GridR, p);
                            p.X /= double.Parse(cb6.SelectedValue.ToString());
                            p.Y /= double.Parse(cb6.SelectedValue.ToString());
                            p.Z /= double.Parse(cb6.SelectedValue.ToString());
                            //
                            st3.Text = Math.Round(p.X, 2).ToString();
                            st4.Text = Math.Round(p.Y, 2).ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void manageGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MGrid mGrid = new MGrid();
            mGrid.ShowDialog();
        }

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (new FileInfo("Manual.pdf").Exists)
                Process.Start("Manual.pdf");
        }

        private void registerProductToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(Settings.Default.Protection);
            ActivationForm activationForm = new ActivationForm(Program.ProductKey,Settings.Default.Protection);
            if (activationForm.ShowDialog() == DialogResult.OK)
                if (key.GetValue("Password") != null && key.GetValue("Password").ToString() == Program.ProductKey)
                    registerProductToolStripMenuItem.Visible = false;
        }
    }
}
