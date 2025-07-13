namespace WalgelijkPlus.Graphics;

public class TileMap
{
    public const int Width = 150;
    public const int Height = 150;

    public Tile[] Tiles = new Tile[Width * Height];

    public List<TileLayer> Layers = new();

    public List<Tileset> Tilesets = new();
}
