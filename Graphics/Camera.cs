using System;
using OpenTK;
using craftinggame.Mechanics;

namespace craftinggame.Graphics
{
    public class Camera
    {
        private Entity _entity;

        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        public Matrix4 view;
        public Matrix4 projection;

        private float _fov = (float)MathHelper.DegreesToRadians(45.0);

        public Camera(Entity entity, float aspectRatio)
        {
            _entity = entity;
            AspectRatio = aspectRatio;
        }
        public float AspectRatio { private get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 45f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(_entity.Position, _entity.Position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 500f);
        }

        public void UpdateVectors()
        {
            _front.X = (float)Math.Cos(_entity.Pitch) * (float)Math.Cos(_entity.Yaw);
            _front.Y = (float)Math.Sin(_entity.Pitch);
            _front.Z = (float)Math.Cos(_entity.Pitch) * (float)Math.Sin(_entity.Yaw);
            _front = Vector3.Normalize(_front);

            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        }
    }
}