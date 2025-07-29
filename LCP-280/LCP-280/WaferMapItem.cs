public class WaferMapItem
{
    public int X { get; set; }
    public int Y { get; set; }
    public int BinRank { get; set; }
    public bool IsProcessed { get; set; }

    public WaferMapItem(int x, int y, int binRank, bool isProcessed)
    {
        X = x;
        Y = y;
        BinRank = binRank;
        IsProcessed = isProcessed;
    }
}