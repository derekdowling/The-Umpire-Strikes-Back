using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;

namespace NLAS_game
{
    /// <summary>
    /// Represent's a button in the game. Button's contain a texture and a position and can calculate if they have been touched on update.
    /// <author>churchmf</author>
    /// </summary>
    class Button
    {
        // texture of the button
        private Texture2D texture;
        // position of the button
        private Vector2 position;
        // action of the button
        private Action action;

        public Button(ContentManager Content, Vector2 position, String name, Action action)
        {
            // Load the button's texture
            texture = Content.Load<Texture2D>(name);
            // Set the button's action
            this.action = action;
            // Set the button's position
            this.position = position;

        }

        public void Update(TouchLocation location)
        {
            switch (location.State)
            {
                // only handle release actions
                case TouchLocationState.Released:
                    Vector2 touch = location.Position;
                    // bound check the button
                    if (touch.X >= position.X && 
                        touch.Y >= position.Y &&
                        touch.X <= position.X + texture.Width &&
                        touch.Y <= position.Y + texture.Height)
                    {
                        // perform the button's action
                        action.Invoke();
                    }
                    break;
                default:
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.Draw(texture, position, color);
        }
    }
}
