using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class MaterialFlow : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        public float scrollSpeed = 0.5f;

        void Update()
        {
            if (meshRenderer != null && meshRenderer.material != null)
            {
                float offset = Time.time * scrollSpeed;
                meshRenderer.material.mainTextureOffset = new Vector2(offset, meshRenderer.material.mainTextureOffset.y);
            }
        }
    }
}
