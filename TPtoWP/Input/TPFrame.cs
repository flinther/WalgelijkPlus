namespace TPtoWP;

public class TPFrame
{
    public string Filename;
    public TPFrameInfo Frame;
}

public class TPFrameInfo
{
    public int X;
    public int Y;
    public int W;
    public int H;
}

public class TPJson
{
    public List<TPFrame> Frames = new();
}
