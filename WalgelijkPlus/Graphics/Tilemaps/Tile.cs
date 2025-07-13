using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Walgelijk.AssetManager.Deserialisers;

namespace WalgelijkPlus.Graphics;

public class Tile
{
    public Tile(int tileId = 0, int tileset = 0)
    {
        id = tileId;
        tilesetId = tileset;
    }

    public int id;
    public int tilesetId;
}