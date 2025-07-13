using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Walgelijk;

namespace WalgelijkPlus.Graphics;

public static class TextureEdit
{
    public static IReadableTexture GetSpriteRegion(Vector2 topLeft, Vector2 scale, IReadableTexture texture)
    {
        Color[] colors = new Color[(int)(scale.X * scale.Y)];

        for (int x = 0; x < scale.X; x++)
        {
            for (int y = 0; y < scale.Y; y++)
            {
                int pixelX = (int)(topLeft.X + x);
                int pixelY = texture.Height - 1 - (int)(topLeft.Y + y);

                colors[y * (int)scale.X + x] = texture.GetPixel(pixelX, pixelY);
            }
        }

        for (int y = 0; y < scale.Y / 2; y++)
        {
            for (int x = 0; x < scale.X; x++)
            {
                int topIndex = y * (int)scale.X + x;
                int bottomIndex = (int)(scale.Y - 1 - y) * (int)scale.X + x;

                Color temp = colors[topIndex];
                colors[topIndex] = colors[bottomIndex];
                colors[bottomIndex] = temp;
            }
        }

        return new Texture((int)scale.X, (int)scale.Y, colors);
    }

}