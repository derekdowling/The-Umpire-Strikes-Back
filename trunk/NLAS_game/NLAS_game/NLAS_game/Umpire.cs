using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NLAS_game
{
    class Umpire : GameObject
    {
        public static Texture2D umpireTexture;

        public Umpire(Vector2 position, int width, int height)
            : base(umpireTexture)
        {
            this.position = position;
            this.scaleToFitTheseDimensions(width, height);
        }
    }
}
