using System;
using MatrixClass;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace GeoCoordTrans_v2._1
{
    public enum Algoritm
    {
        CASSINI_SOLDNER,
        LAMBERT_SP1,
        LAMBERT_SP2,
        TRANSVERSE_MERCATOR
    }
    public enum Equation
    {
        Affine,
        Conformal
    }
    class Transform
    {
        internal static MPoint GeoToCen(string source, string target, MPoint p)
        {

            MPoint r = new MPoint();
            //X = (ν + h) cos ϕ cos λ
            //Y = (ν + h) cos ϕ sin λ
            //Z = ((1 – e2) ν + h) sin ϕ
            //ν = a /(1 – e2sin2ϕ)0.5,
            double ϕ = ToRadians(p.Y);
            double λ = ToRadians(p.X);
            double h = p.Z;

            double a = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][4].ToString());
            double e2 = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][5].ToString());

            double v = a / Math.Sqrt(1 - (e2 * Math.Pow(Math.Sin(ϕ), 2)));

            r.X = (v + h) * Math.Cos(ϕ) * Math.Cos(λ);
            r.Y = (v + h) * Math.Cos(ϕ) * Math.Sin(λ);
            r.Z = (((1 - e2) * v) + h) * Math.Sin(ϕ);

            r = CartToCart(source, target, r);

            return r;
        }

        internal static MPoint CartToGeo(string source, MPoint Cen)
        {
            //
            MPoint r = new MPoint();

            double a = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][4].ToString());
            double e2 = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][5].ToString());
            double ee2 = e2 / (1 - e2);
            double P = Math.Sqrt(Math.Pow(Cen.X, 2) + Math.Pow(Cen.Y, 2));
            //
            r.X = Math.Atan2(Cen.Y, Cen.X);
            r.Y = Math.Atan(Cen.Z * (1 + ee2) / P);
            double v = a / Math.Sqrt(1 - (e2 * Math.Pow(Math.Sin(r.Y), 2)));
            r.Y = Math.Atan((Cen.Z + (e2 * v * Math.Sin(r.Y))) / P);

            for (int i = 0; i < 15; i++)
            {
                v = a / Math.Sqrt(1 - (e2 * Math.Pow(Math.Sin(r.Y), 2)));
                r.Y = Math.Atan((Cen.Z + (e2 * v * Math.Sin(r.Y))) / P);
            }
            //
            r.Z = (Cen.X * (1 / Math.Cos(r.X)) * (1 / Math.Cos(r.Y))) - v;

            r.X = ToDegree(r.X);
            r.Y = ToDegree(r.Y);

            return r;
        }

        private static MPoint CenToCen(string source, string target, MPoint pt)
        {
            MPoint r = new MPoint();

            double dxs = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][6].ToString());
            double dys = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][7].ToString());
            double dzs = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][8].ToString());
            double Rxs = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][9].ToString());
            double Rys = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][10].ToString());
            double Rzs = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][11].ToString());
            double ks = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", source))[0][12].ToString());
            //
            double dxt = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", target))[0][6].ToString());
            double dyt = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", target))[0][7].ToString());
            double dzt = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", target))[0][8].ToString());
            double Rxt = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", target))[0][9].ToString());
            double Ryt = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", target))[0][10].ToString());
            double Rzt = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", target))[0][11].ToString());
            double kt = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", target))[0][12].ToString());

            double[][] A = new double[3][];
            A[0] = new double[] { 1, Rzs + Rzt, -(Rys + Ryt) };
            A[1] = new double[] { -(Rzs + Rzt), 1, Rxs + Rxt };
            A[2] = new double[] { Rys + Ryt, -(Rxs + Rxt), 1 };

            //
            double[][] T = new double[3][];
            T[0] = new double[] { pt.X };
            T[1] = new double[] { pt.Y };
            T[2] = new double[] { pt.Z };

            //
            A = Matrix.Mul(A, T);
            for (int i = 0; i < A.Length; i++)
                for (int j = 0; j < A[0].Length; j++)
                    A[i][j] *= 1 + (ks + kt);
            //
            A[0][0] = source == "GAMF" ? A[0][0] - (dxs + dxt) : A[0][0] + (dxs + dxt);
            A[1][0] = source == "GAMF" ? A[1][0] - (dys + dyt) : A[1][0] + (dys + dyt);
            A[2][0] = source == "GAMF" ? A[2][0] - (dzs + dzt) : A[2][0] + (dzs + dzt);

            r.X = A[0][0];
            r.Y = A[1][0];
            r.Z = A[2][0];

            return r;
        }

        internal static MPoint GeoToGeo(string source, string target, MPoint p)
        {
            return CartToGeo(target, GeoToCen(source, target, p));
        }

        internal static MPoint CartToCart(string source, string target, MPoint p)
        {
            return CenToCen("GAMF", target, CenToCen(source, "GAMF", p));
        }

        internal static MPoint GeoToPro(string source, MPoint p)
        {
            MPoint r = new MPoint();
            try
            {
                double λ = ToRadians(p.X);
                double φ = ToRadians(p.Y);
                //
                DataRow[] gv = Helper.GridTable.Select(string.Format("ID = '{0}'", source));
                //
                double a = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", gv[0][3]))[0][4].ToString());
                double e2 = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", gv[0][3]))[0][5].ToString());
                Algoritm algorithm = (Algoritm)Enum.Parse(typeof(Algoritm), gv[0][4].ToString(), false);
                double FE = double.Parse(gv[0][5].ToString());
                double FN = double.Parse(gv[0][6].ToString());
                double λo = ToRadians(double.Parse(gv[0][7].ToString()));
                double φo = ToRadians(double.Parse(gv[0][8].ToString()));
                double ko = double.Parse(gv[0][9].ToString());
                double Sp1 = double.Parse(gv[0][10].ToString());
                double Sp2 = double.Parse(gv[0][11].ToString());

                switch (algorithm)
                {
                    case Algoritm.CASSINI_SOLDNER:
                        double Ac = Math.Cos(φ) * (λ - λo);
                        double Tc = Math.Pow(Math.Tan(φ), 2);
                        double Cc = e2 * Math.Pow(Math.Cos(φ), 2) / (1 - e2);
                        double vc = a / Math.Sqrt(1 - (e2 * Math.Pow(Math.Sin(φ), 2)));
                        double Mc = a * Conjugate(φ, e2);
                        double Mco = a * Conjugate(φo, e2);
                        //
                        r.X = FE + (vc * (Ac - (Tc * Math.Pow(Ac, 3) / 6) - ((8 - Tc + (8 * Tc)) * (Tc * Math.Pow(Ac, 5) / 120))));
                        r.Y = FN + Mc - Mco + (vc * Math.Tan(φ) * ((Math.Pow(Ac, 2) / 2) + ((5 - Tc + (6 * Cc)) * Math.Pow(Ac, 4) / 24)));
                        break;
                    case Algoritm.LAMBERT_SP1:
                        break;
                    case Algoritm.LAMBERT_SP2:
                        break;
                    case Algoritm.TRANSVERSE_MERCATOR:
                        double Atm = Math.Cos(φ) * (λ - λo);
                        double vtm = a / Math.Sqrt(1 - (e2 * Math.Pow(Math.Sin(φ), 2)));
                        double Ctm = e2 * Math.Pow(Math.Cos(φ), 2) / (1 - e2);
                        double ee2 = e2 / (1 - e2);
                        double Ttm = Math.Pow(Math.Tan(φ), 2);
                        double M = a * Conjugate(φ, e2);
                        double Mo = a * Conjugate(φo, e2);
                        //
                        r.X = FE + (ko * vtm * (Atm + ((1 - Ttm + Ctm) * Math.Pow(Atm, 3) / 6) + ((5 - (18 * Ttm) + Math.Pow(Ttm, 2) + (72 * Ctm) - (58 * ee2)) * Math.Pow(Atm, 5) / 120)));
                        r.Y = FN + (ko * (M - Mo + (vtm * Math.Tan(φ) * ((Math.Pow(Atm, 2) / 2) + ((5 - Ttm + (9 * Ctm) + (4 * Math.Pow(Ctm, 2))) * Math.Pow(Atm, 4) / 24) +
                            ((61 - (58 * Ttm) + Math.Pow(Ttm, 2) + (600 * Ctm) - (330 * ee2)) * Math.Pow(Atm, 6) / 720)))));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            return r;
        }

        internal static MPoint ProToGeo(string source, MPoint p)
        {
            MPoint r = new MPoint();

            double E = p.X;
            double N = p.Y;
            //
            DataRow[] gv = Helper.GridTable.Select(string.Format("ID = '{0}'", source));
            //
            double a = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", gv[0][3]))[0][4].ToString());
            double e2 = double.Parse(Helper.DatumTable.Select(string.Format("ID = '{0}'", gv[0][3]))[0][5].ToString());
            Algoritm algorithm = (Algoritm)Enum.Parse(typeof(Algoritm), gv[0][4].ToString(), false);
            double FE = double.Parse(gv[0][5].ToString());
            double FN = double.Parse(gv[0][6].ToString());
            double λo = ToRadians(double.Parse(gv[0][7].ToString()));
            double φo = ToRadians(double.Parse(gv[0][8].ToString()));
            double ko = double.Parse(gv[0][9].ToString());
            double Sp1 = double.Parse(gv[0][10].ToString());
            double Sp2 = double.Parse(gv[0][11].ToString());

            switch (algorithm)
            {
                case Algoritm.CASSINI_SOLDNER:
                    double Moc = a * Conjugate(φo, e2);
                    double M1c = Moc + (N - FN);
                    double e1c = (1 - Math.Sqrt(1 - e2)) / (1 + Math.Sqrt(1 - e2));
                    double μ1c = M1c / (a * (1 - (e2 / 4) - (3 * Math.Pow(e2, 2) / 64) - (5 * Math.Pow(e2, 4) / 256)));
                    double ϕ1c = μ1c + ((((3 * e1c / 2) - (27 * Math.Pow(e1c, 3) / 32)) * Math.Sin(2 * μ1c)) + (Math.Sin(4 * μ1c) * ((21 * Math.Pow(e1c, 2) / 16) - (55 * Math.Pow(e1c, 4) / 32))) +
                        (Math.Sin(6 * μ1c) * (151 * Math.Pow(e1c, 3) / 96)) + (Math.Sin(8 * μ1c) * (1097 * Math.Pow(e1c, 4) / 512)));
                    double T1c = Math.Pow(Math.Tan(ϕ1c), 2);
                    double v1c = a / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(ϕ1c), 2));
                    double Dc = (E - FE) / v1c;
                    double ρ1c = a * (1 - e2) / Math.Pow(1 - (e2 * Math.Pow(Math.Sin(ϕ1c), 2)), 1.5);
                    //
                    double λc = λo + ((Dc - (T1c * Math.Pow(Dc, 3) / 3) + ((1 + 3) * (T1c * Math.Pow(Dc, 5) / 15))) / Math.Cos(ϕ1c));
                    double ϕc = ϕ1c - (v1c * Math.Tan(ϕ1c) / ρ1c * ((Math.Pow(Dc, 2) / 2) - (1 + 3 * T1c) * Math.Pow(Dc, 4) / 24));
                    //
                    r.X = ToDegree(λc);
                    r.Y = ToDegree(ϕc);
                    break;
                case Algoritm.LAMBERT_SP1:
                    break;
                case Algoritm.LAMBERT_SP2:
                    break;
                case Algoritm.TRANSVERSE_MERCATOR:
                    double ee2 = e2 / (1 - e2);
                    double Mo = a * Conjugate(φo, e2);
                    //
                    double M1 = Mo + ((N - FN) / ko);
                    double μ1 = M1 / (a * (1 - (e2 / 4) - (3 * Math.Pow(e2, 2) / 64) - (5 * Math.Pow(e2, 4) / 256)));
                    double e1 = (1 - Math.Sqrt(1 - e2)) / (1 + Math.Sqrt(1 - e2));
                    double ϕ1 = μ1 + ((((3 * e1 / 2) - (27 * Math.Pow(e1, 3) / 32)) * Math.Sin(2 * μ1)) + (Math.Sin(4 * μ1) * ((21 * Math.Pow(e1, 2) / 16) - (55 * Math.Pow(e1, 4) / 32))) +
                        (Math.Sin(6 * μ1) * (151 * Math.Pow(e1, 3) / 96)) + (Math.Sin(8 * μ1) * (1097 * Math.Pow(e1, 4) / 512)));
                    double v1 = a / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(ϕ1), 2));
                    double ρ1 = a * (1 - e2) / Math.Pow(1 - (e2 * Math.Pow(Math.Sin(ϕ1), 2)), 1.5);
                    double T1 = Math.Pow(Math.Tan(ϕ1), 2);
                    double C1 = ee2 * Math.Pow(Math.Cos(ϕ1), 2);
                    double D = (E - FE) / (v1 * ko);
                    //
                    double λ = λo + ((D - (Math.Pow(D, 3) / 6 * (1 + (2 * T1) + C1)) + ((5 - (2 * C1) + (28 * T1) - (3 * Math.Pow(C1, 2)) + (8 * ee2) + (24 * Math.Pow(T1, 2))) * Math.Pow(D, 5) / 120)) / Math.Cos(ϕ1));
                    double ϕ = ϕ1 - (v1 * Math.Tan(ϕ1) / ρ1 * ((Math.Pow(D, 2) / 2) - ((5 + (3 * T1) + (10 * C1) - (4 * Math.Pow(C1, 2)) - (9 * ee2)) * Math.Pow(D, 4) / 24) +
                        ((61 + (90 * T1) + (298 * C1) + (45 * Math.Pow(T1, 2)) - (252 * ee2) - (3 * Math.Pow(C1, 2))) * Math.Pow(D, 6) / 720)));
                    r.X = ToDegree(λ);
                    r.Y = ToDegree(ϕ);
                    break;
                default:
                    break;
            }

            return r;
        }

        internal static MPoint ProToPro(string source, string target, MPoint p)
        {
            return GeoToPro(target, GeoToGeo(Helper.GetGridDatumID(source), Helper.GetGridDatumID(target), ProToGeo(source, p)));
        }

        private static double Conjugate(double φ, double e2)
        {
            double Ao = 1 - (e2 / 4) - (3 * Math.Pow(e2, 2) / 64) - (5 * Math.Pow(e2, 4) / 256);
            double A2 = (3 * e2 / 8) + (3 * Math.Pow(e2, 2) / 32) + (45 * Math.Pow(e2, 4) / 1024);
            double A4 = (15 * Math.Pow(e2, 2) / 256) + (45 * Math.Pow(e2, 4) / 1024);
            double A6 = 35 * Math.Pow(e2, 4) / 3072;
            //
            return (Ao * φ) - (A2 * Math.Sin(2 * φ)) + (A4 * Math.Sin(4 * φ)) - (A6 * Math.Sin(6 * φ));
        }

        internal static double ToDegree(double v)
        {
            return v * 180 / Math.PI;
        }

        internal static double ToRadians(double v)
        {
            return v * Math.PI / 180;
        }

        internal static double[][] Parameters(Equation equation, string source, string target)
        {
            double[][] x = new double[File.ReadAllLines(source).ToList().Count * 2][];
            List<string> DataA, DataL;
            try
            {
                DataA = File.ReadAllLines(source).ToList();
                DataL = File.ReadAllLines(target).ToList();
                //

                #region Forming A matrix
                double[][] A = new double[DataA.Count * 2][];
                int a = 0;
                switch (equation)
                {
                    case Equation.Conformal:
                        for (int i = 0; i < A.Length; i++)
                        {
                            if (i % 2 == 0 && i != 0)
                                a++;
                            string[] coord = DataA[a].Split(',', '\t');
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
                            string[] coord = DataA[a].Split(',', '\t');
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
                double[][] _L = new double[DataL.Count * 2][];
                for (int i = 0; i < _L.Length; i++)
                {
                    if (i % 2 == 0 && i != 0)
                        k++;
                    string[] _coord = DataL[k].Split(',', '\t');
                    _L[i] = new double[1];
                    for (int j = 0; j < _L[0].Length; j++)
                    {
                        _L[i][j] = (i % 2 == 0) ? double.Parse(_coord[0].ToString()) : double.Parse(_coord[1].ToString());
                    }
                }
                #endregion

                //
                x = Matrix.Mul(Matrix.Inv(Matrix.Mul(Matrix.Trans(A), A)), Matrix.Mul(Matrix.Trans(A), _L));

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return x;
        }

        internal static MPoint Solve(Equation equation, string source, string target, MPoint p)
        {
            MPoint r = new MPoint();
            try
            {
                double[][] x = Parameters(equation, source, target);
                switch (equation)
                {
                    case Equation.Conformal:
                        r.X = x[0][0] * p.X - x[1][0] * p.Y + x[2][0];
                        r.Y = x[0][0] * p.Y + x[1][0] * p.X + x[3][0];
                        r.Z = 0;
                        break;
                    case Equation.Affine:
                        r.X = x[0][0] * p.X + x[1][0] * p.Y + x[2][0];
                        r.Y = x[3][0] * p.X + x[4][0] * p.Y + x[5][0];
                        r.Z = 0;
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return r;
        }
    }
}
