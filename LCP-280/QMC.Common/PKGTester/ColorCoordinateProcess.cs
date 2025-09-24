using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            if (x < 0 || x > 1 || y < 0 || y > 1)
                return false;

            using (var path = GetPolygon())
            {
                return path.IsVisible((float)x, (float)y);
            }
        }
        public bool IsOverlap(ColorRegion other)
        {
            if (other == null)
                return false;
            
            using (var pathA = GetPolygon())
            using (var pathB = other.GetPolygon())
            {
                return PolygonsOverlap(pathA, pathB);
            }
        }
        private GraphicsPath GetPolygon()
        {
            GraphicsPath path = new GraphicsPath();
            PointF[] points = new PointF[]
            {
                new PointF((float)topLeft.X, (float)topLeft.Y),
                new PointF((float)topRight.X, (float)topRight.Y),
                new PointF((float)bottomRight.X, (float)bottomRight.Y),
                new PointF((float)bottomLeft.X, (float)bottomLeft.Y)
            };
            path.AddPolygon(points);
            return path;
        }
        private bool PolygonsOverlap(GraphicsPath a, GraphicsPath b)
        {
            using (var regA = new Region(a))
            using (var regB = new Region(b))
            {
                regA.Intersect(regB);
                return !regA.IsEmpty(Graphics.FromHwnd(IntPtr.Zero));
            }
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
        public ColorRegionCollection()
        {
        }
        #endregion

        #region Methods
        public void Clear()
        {
            regions.Clear();
        }
        public bool AddRegion(ColorRegion region)
        {
            if (region == null)     
                return false;

            regions.Add(region);
            return true;
        }
        public bool Validate()
        {
            // 모든 멤버가 유효한지 확인
            for (int i = 0; i < regions.Count; i++)
            {
                if (regions[i] == null)
                    return false;
                if (!regions[i].Validate())
                    return false;
            }

            // 멤버 간에 겹치는 영역이 있는지 확인
            for (int i = 0; i < regions.Count; i++)
            {
                var regionA = regions[i];
                for (int j = i + 1; j < regions.Count; j++)
                {
                    var regionB = regions[j];
                    if (regionA.IsOverlap(regionB))
                        return false;
                }
            }
            return true;
        }
        public bool LoadFromFile(string filePath)
        {
            try
            {
                Clear();
                using (var reader = new System.IO.StreamReader(filePath, System.Text.Encoding.UTF8))
                {
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool SaveToFile(string filePath)
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                using (var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
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