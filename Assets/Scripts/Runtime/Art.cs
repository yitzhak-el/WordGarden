using UnityEngine;
using UnityEngine.UI;

namespace WordGarden
{
    public static class Art
    {
        private static Sprite[] shape;
        public static Sprite Rounded
        {
            get
            {
                if (shape == null) Generate();
                return shape[0];
            }
        }
        private static void Generate()
        {
            var tex = new Texture2D(96,96,TextureFormat.RGBA32,false);
            for (int y=0;y<96;y++) for (int x=0;x<96;x++)
            {
                float cx = Mathf.Clamp(x,20,75), cy=Mathf.Clamp(y,20,75);
                var distance=Vector2.Distance(new Vector2(x,y),new Vector2(cx,cy));
                tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(21-distance)));
            }
            tex.Apply(); shape=new[]{ Sprite.Create(tex,new Rect(0,0,96,96),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(24,24,24,24)) };
        }
        public static Sprite Load(string name)
        {
            var texture=Resources.Load<Texture2D>("Art/"+name);
            if (texture == null) return null;
            return Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));
        }
    }
}
