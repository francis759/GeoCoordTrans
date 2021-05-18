namespace GeoCoordTrans_v2._1
{
    internal class MPoint
    {
        public MPoint()
        {

        }
        public MPoint(double _x, double _y, double _z)
        {
            X = _x;
            Y = _y;
            Z = _z;
        }
        internal  double X { get; set; }
        internal  double Y { get; set; }
        internal  double Z { get; set; }
    }
}