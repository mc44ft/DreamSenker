using UnityEngine;

[DisallowMultipleComponent]
public class PlayerMirrorEffect : MonoBehaviour
{
    [SerializeField] private GameObject _mirrorGameObject;
    [SerializeField] private SpriteRenderer _shadowRenderer;

    private Material _shadowMaterial;

    private void Awake()
    {
        if (_shadowRenderer != null)
            _shadowMaterial = _shadowRenderer.material;
    }

    public void SetMirrorActive(bool isActive)
    {
        if (_mirrorGameObject != null)
            _mirrorGameObject.SetActive(isActive);
    }

    public void SetShadowDarknessStrength(float strength)
    {
        //该组件是可选特效组件，未配置阴影时直接跳过
        if (_shadowMaterial != null)
            _shadowMaterial.SetFloat("_DarknessStrength", strength);
    }
}
