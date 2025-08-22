namespace QMC.Common
{
    /// <summary>
    /// Position(도메인) <-> PropertyPosition(UI) 어댑터
    /// </summary>
    public static class PositionUiAdapter
    {
        public static PropertyPosition ToUi(Position pos, string axisName)
        {
            var pp = new PropertyPosition(pos.Name, pos.Name + " position", pos.AxisKey, pos.Unit, true)
            {
                Key = pos.Key
            };
            pp.AddDoubleProperty(axisName, pos.Value);
            pp.AddDoubleProperty("Velocity", pos.Velocity);
            pp.AddDoubleProperty("Acceleration", pos.Acceleration);
            pp.AddDoubleProperty("Deceleration", pos.Deceleration);
            pp.AddDoubleProperty("TimeoutMs", pos.TimeoutMs);
            return pp;
        }

        public static Position FromUi(PropertyPosition pp, string axisName)
        {
            var pos = new Position
            {
                Key = pp.Key,
                Name = pp.Title,
                AxisKey = pp.Category,
                Unit = pp.Unit
            };

            var dp = pp.GetDoubleProperty(axisName);
            if (dp != null) pos.Value = dp.Value;
            if (pp.TryGetDouble("Velocity", out var vel)) pos.Velocity = vel;
            if (pp.TryGetDouble("Acceleration", out var acc)) pos.Acceleration = acc;
            if (pp.TryGetDouble("Deceleration", out var dec)) pos.Deceleration = dec;
            if (pp.TryGetDouble("TimeoutMs", out var to)) pos.TimeoutMs = (int)to;
            return pos;
        }
    }
}
