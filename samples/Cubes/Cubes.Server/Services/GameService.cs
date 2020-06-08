namespace Cubes.Server.Services
{
    using System.Collections.Generic;
    using UnityEngine;

    public class GameService : IGameService
    {
        private readonly Queue<byte> _ids = new Queue<byte>(collection: new byte[] { 1, 2 });
        private readonly Queue<Vector3> _positions = new Queue<Vector3>(
            collection:
                new[]
                {
                    new Vector3
                    {
                        x = 5,
                        y = 5,
                        z = 5,
                    },
                    new Vector3
                    {
                        x = -5,
                        y = 5,
                        z = -5,
                    },
                });

        public byte GetPlayerId()
        {
            var id = _ids.Dequeue();
            return id;
        }

        public Vector3 GetPosition()
        {
            var pos = _positions.Dequeue();
            return pos;
        }
    }
}