using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.PKGTester
{
    public class ColorRegion : BaseConfig
    {
        #region Fields
        public PointD topLeft = new PointD(0, 0);
        public PointD topRight = new PointD(1, 0);
        public PointD bottomRight = new PointD(1, 1);
        public PointD bottomLeft = new PointD(0, 1);
        #endregion

        #region Properties
        public PointD TopLeft { get => topLeft; set => topLeft = value; }
        public PointD TopRight { get => topRight; set => topRight = value; }
        public PointD BottomRight { get => bottomRight; set => bottomRight = value; }
        public PointD BottomLeft { get => bottomLeft; set => bottomLeft = value; }
        #endregion

        #region Constuctors
        public ColorRegion(string name) : base(name)
        {
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            topLeft = new PointD(0, 0);
            topRight = new PointD(1, 0);
            bottomRight = new PointD(1, 1);
            bottomLeft = new PointD(0, 1);
        }
        public override bool Validate()
        {
            // Add validation logic if needed
            if (topLeft.X < 0 || topLeft.X > 1 || topLeft.Y < 0 || topLeft.Y > 1) 
                return false;
            if (topRight.X < 0 || topRight.X > 1 || topRight.Y < 0 || topRight.Y > 1)
                return false;
            if (bottomRight.X < 0 || bottomRight.X > 1 || bottomRight.Y < 0 || bottomRight.Y > 1)
                return false;
            if (bottomLeft.X < 0 || bottomLeft.X > 1 || bottomLeft.Y < 0 || bottomLeft.Y > 1)
                return false;

            // top left check
            if (topLeft.X >= topRight.X || topLeft.Y >= bottomLeft.Y)
                return false;
            // top right check
            if (topRight.X <= topLeft.X || topRight.Y >= bottomRight.Y)
                return false;
            // bottom right check
            if (bottomRight.X <= bottomLeft.X || bottomRight.Y <= topRight.Y)
                return false;
            // bottom left check
            if (bottomLeft.X >= bottomRight.X || bottomLeft.Y <= topLeft.Y)
                return false;

            return true;
        }
        public override PropertyCollection GetPropertyCollection()
        {
            var pc = new PropertyCollection();
            pc.Add("Color Region");
            pc.Add("Top Left - X", topLeft.X);
            pc.Add("Top Left - Y", topLeft.Y);
            pc.Add("Top Right - X", topRight.X);
            pc.Add("Top Right - Y", topRight.Y);
            pc.Add("Bottom Right - X", bottomRight.X);
            pc.Add("Bottom Right - Y", bottomRight.Y);
            pc.Add("Bottom Left - X", bottomLeft.X);
            pc.Add("Bottom Left - Y", bottomLeft.Y);
            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;
            try
            {
                topLeft.X = pc.GetValue<double>("Top Left - X");
                topLeft.Y = pc.GetValue<double>("Top Left - Y");
                topRight.X = pc.GetValue<double>("Top Right - X");
                topRight.Y = pc.GetValue<double>("Top Right - Y");
                bottomRight.X = pc.GetValue<double>("Bottom Right - X");
                bottomRight.Y = pc.GetValue<double>("Bottom Right - Y");
                bottomLeft.X = pc.GetValue<double>("Bottom Left - X");
                bottomLeft.Y = pc.GetValue<double>("Bottom Left - Y");
            }
            catch (Exception ex)
            {
                // 필요시 로그 처리
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        public bool InArea(double x, double y)
        {
            // Using the ray-casting algorithm to determine if the point is inside the polygon
            int n = 4; // Number of vertices
            bool inside = false;
            PointD[] vertices = { topLeft, topRight, bottomRight, bottomLeft };
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if ((vertices[i].Y > y) != (vertices[j].Y > y) &&
                    (x < (vertices[j].X - vertices[i].X) * (y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y + 1e-10) + vertices[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }
        #endregion
    }

    public class ColorRegionCollection
    {
        #region Fields
        private List<ColorRegion> regions = new List<ColorRegion>();
        #endregion

        #region Properties
        public IReadOnlyList<ColorRegion> Items => regions.AsReadOnly();
        #endregion

        #region Constuctors
        #endregion

        #region Methods
        #endregion
    }

    public class ColorRegionClassifier
    {
        #region Fields
        private ColorRegionCollection regionCollection = new ColorRegionCollection();
        #endregion
        #region Properties
        #endregion
        #region Constructors
        public ColorRegionClassifier()
        {
        }
        #endregion
        #region Methods
        public void Clear()
        {
            // regionCollection.Clear(); // Uncomment when Clear method is implemented in ColorRegionCollection
        }
        public bool AssignRegionCollection(ColorRegionCollection collection)
        {
            if (collection == null) return false;
            regionCollection = collection;
            return true;
        }
        public ColorRegion Classify(double x, double y)
        {
            foreach (var region in regionCollection.Items) // Assuming ColorRegionCollection implements IEnumerable<ColorRegion>
            {
                if (region.InArea(x, y))
                {
                    return region;
                }
            }
            return null; // No matching region found
        }
        #endregion
    }
}