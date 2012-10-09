using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Box2D.XNA;
using System.Diagnostics;

namespace NLAS_game
{
    class PhysicsBox : DynamicPhysicsGameObject
    {
        public PhysicsBox(Texture2D sprite_texture, World physicsWorld, float box_width, float box_height, 
            float positionX, float positionY, float rot, float density) : base(sprite_texture)
        {
            this.scaleToFitTheseDimensions(box_width, box_height);
            this.position.X = positionX;
            this.position.Y = positionY;

            BodyDef dynamicBodyDef = new BodyDef();
            dynamicBodyDef.type = BodyType.Dynamic;
            dynamicBodyDef.position = new Vector2(positionX/DynamicPhysicsGameObject.ScreenPixelsPerMeter, positionY/DynamicPhysicsGameObject.ScreenPixelsPerMeter);
            dynamicBodyDef.angle = rot;
            Body dynamicBody = physicsWorld.CreateBody(dynamicBodyDef);

            PolygonShape dynamicBoxShape = new PolygonShape();
            dynamicBoxShape.SetAsBox((box_width/ScreenPixelsPerMeter) / 2.0f, (box_height/ScreenPixelsPerMeter) / 2.0f);   //experiment with / 2.0f
            FixtureDef dynamicBoxFixtureDef = new FixtureDef();
            dynamicBoxFixtureDef.shape = dynamicBoxShape;
            dynamicBoxFixtureDef.density = density;
            dynamicBoxFixtureDef.friction = 0.3f;
            dynamicBody.CreateFixture(dynamicBoxFixtureDef);

            this.physicsBody = dynamicBody;
        }
    }
}
