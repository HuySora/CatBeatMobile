using UnityEngine;

namespace VTBeat.Extensions {
    public static class SpriteX {
        public static Sprite CreateRandomColorSprite(int width, int height) {
            Color randomColor = new Color(Random.value, Random.value, Random.value);
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = randomColor;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }
    }
}