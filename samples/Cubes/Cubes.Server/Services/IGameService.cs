namespace Cubes.Server.Services
{
    using UnityEngine;

    public interface IGameService
    {
        byte GetPlayerId();

        Vector3 GetPosition();
    }
}