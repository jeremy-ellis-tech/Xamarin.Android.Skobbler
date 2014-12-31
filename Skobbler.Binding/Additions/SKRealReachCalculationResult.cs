using System;

namespace Skobbler.Additions
{
    public sealed class SKRealReachCalculationResult
    {
        internal SKRealReachCalculationResult(int xMin, int xMax, int yMin, int yMax)
        {
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
        }

        public int XMin { get; private set; }
        public int XMax { get; private set; }
        public int YMin { get; private set; }
        public int YMax { get; private set; }

        public override string ToString()
        {
            return String.Format("Skobbler.Additions.SKRealReachCalculationResult - XMin: {0} XMax: {1} YMin: {2} YMax: {3}", XMin, XMax, YMin, YMax);
        }

        public override bool Equals(object obj)
        {
            if(Object.ReferenceEquals(obj, this))
            {
                return true;
            }

            SKRealReachCalculationResult other = obj as SKRealReachCalculationResult;

            return other != null && other.XMin == XMin && other.XMax == XMax && other.YMin == YMin && other.YMax == YMax;
        }

        public override int GetHashCode()
        {
            return XMin.GetHashCode() + XMax.GetHashCode() + YMin.GetHashCode() + YMax.GetHashCode();
        }
    }
}