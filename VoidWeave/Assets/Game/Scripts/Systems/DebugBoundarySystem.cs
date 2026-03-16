namespace Game.Scripts.Systems
{
    using Components;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct DebugBoundarySystem : ISystem
    {
        public void OnUpdate(ref SystemState systemState)
        {
            if(!SystemAPI.TryGetSingleton<BoundaryOffsetComponent>(out var offset) || !SystemAPI.TryGetSingleton<CameraOrthographicSizeComponent>(out var camera)) return;

            float aspect = (float)Screen.width / Screen.height;
            float bX = camera.Value * aspect - offset.Value;
            float bY = camera.Value - offset.Value;
            
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