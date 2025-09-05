using UnityEngine;

public static class TextureAtlas
{
    public static Vector2[] GetUVs(TextureType type)
    {
        switch (type)
        {
            case TextureType.Grass:
                return new Vector2[] { new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1), new Vector2(0, 1) };
            case TextureType.Dirt:
                return new Vector2[] { new Vector2(0.5f, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 1), new Vector2(0.5f, 1) };
            case TextureType.Stone:
                return new Vector2[] { new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f) };
            case TextureType.Sand:
                return new Vector2[] { new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f) };
            default:
                return new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        }
    }
}
