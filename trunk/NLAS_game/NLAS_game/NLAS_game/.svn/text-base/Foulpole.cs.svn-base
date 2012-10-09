using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace NLAS_game
{
    public class Foulpole : GameObject
    {
        public static Texture2D foulpoleTexture;
        private bool flip;

        public Foulpole(Vector2 position, int width, int height, bool flip)
            : base(foulpoleTexture)
        {
            this.position = position;
            this.flip = flip;
            this.scaleToFitTheseDimensions(width, height);
        }

        public override void draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (spriteTexture != null)
            {
                SpriteEffects effects;
                if (flip)
                    effects = SpriteEffects.FlipHorizontally;
                else
                    effects = SpriteEffects.None;
                spriteBatch.Draw(this.spriteTexture, this.position, null, Color.White, this.rotation, this.origin, this.scale, effects, 0.0f);
            }
            else
            {
                Debug.WriteLine("WARNING! Trying to render a gameObject that is missing a texture!");
            }
        }
    }
}
