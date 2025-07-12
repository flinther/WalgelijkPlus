using Newtonsoft.Json;
using System.Numerics;
using Walgelijk;
using Walgelijk.AssetManager;

namespace WalgelijkPlus.Graphics;

public class Tileset
{
    public Tileset(AssetRef<Texture> texture, int tileSize = 8)
    {
        float width = texture.Value.Size.X / tileSize;
        float height = texture.Value.Size.Y / tileSize;

        Tiles = new Texture[(int)(width * height)];

        BaseTexture = texture;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tiles[y * (int)width + x] = TextureEdit.GetSpriteRegion(new Vector2(x * tileSize, y * tileSize), new Vector2(tileSize), BaseTexture.Value);
            }
        }

        TilesetSize = new(width, height);
        Tilesize = tileSize;
    }

    [JsonIgnore]
    public IReadableTexture[] Tiles;

    public AssetRef<Texture> BaseTexture;

    public int Tilesize;

    public Vector2 TilesetSize { get; private set; }

    public static Tileset FloorTileset = new Tileset(Assets.Load<Texture>("textures/sprites/Background/tlFloors.png"), 16);

    public static Tileset WallTileset = new Tileset(Assets.Load<Texture>("textures/sprites/Background/tlWalls.png"), 8);

    public static Tileset CornerTileset = new Tileset(Assets.Load<Texture>("textures/sprites/Background/tlCorners.png"), 8);
}

