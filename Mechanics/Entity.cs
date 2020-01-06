using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace craftinggame.Mechanics
{
    public class Entity
    {
        public Entity(Vector3 position)
        {
            Position = position;
            Yaw = -MathHelper.PiOver2;
        }

        public Vector3 Position { get; set; }
        private float _pitch;

        public float Pitch
        {
            get => _pitch;
            set
            {
                var angle = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2);
                _pitch = angle;
            }
        }

        public float Yaw { get; set; }
    }
}
