namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DebugBoundarySystem : ISystem
    {
        public void OnCreate(ref SystemState systemState)
        {
            systemState.RequireForUpdate<BoundaryOffsetComponent>();
            systemState.RequireForUpdate<CameraOrthographicSizeComponent>();
        }

        public void OnUpdate(ref SystemState systemState)
        {
            float camera = SystemAPI.GetSingleton<CameraOrthographicSizeComponent>().Value;
            float offset = SystemAPI.GetSingleton<BoundaryOffsetComponent>().Value;

            float aspect = (float)Screen.width / Screen.height;
            float bX = camera * aspect - offset;
            float bY = camera - offset;

            Vector3 topLeft = new Vector3(-bX , bY , 0);
            Vector3 topRight = new Vector3(bX , bY , 0);
            Vector3 bottomLeft = new Vector3(-bX , -bY , 0);
            Vector3 bottomRight = new Vector3(bX , -bY , 0);

            Debug.DrawLine(topLeft , topRight , Color.magenta);
            Debug.DrawLine(topRight , bottomRight , Color.magenta);
            Debug.DrawLine(bottomRight , bottomLeft , Color.magenta);
            Debug.DrawLine(bottomLeft , topLeft , Color.magenta);
        }
    }
}