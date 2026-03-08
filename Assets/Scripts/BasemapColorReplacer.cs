using UnityEngine;

public class BasemapColorReplacer : MonoBehaviour
{
    [SerializeField] private Color targetColor = Color.red;
    [SerializeField] private int materialIndex = 1;

    private Material _instanceMaterial;

    private void Start()
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;

        // Access .materials to get instance copies (non-shared).
        var mats = renderer.materials;

        if (materialIndex < 0 || materialIndex >= mats.Length) return;

        _instanceMaterial = mats[materialIndex];
        _instanceMaterial.SetColor("_BaseColor", targetColor);

        // Write array back so the renderer uses the instanced copy.
        renderer.materials = mats;
    }

    public void SetColor(Color color)
    {
        targetColor = color;
        if (_instanceMaterial != null)
            _instanceMaterial.SetColor("_BaseColor", color);
    }

    private void OnDestroy()
    {
        if (_instanceMaterial != null)
            Destroy(_instanceMaterial);
    }

    private void OnValidate()
    {
        if (_instanceMaterial == null)
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer == null) return;

            // Access .materials to get instance copies (non-shared).
            var mats = renderer.materials;

            if (materialIndex < 0 || materialIndex >= mats.Length) return;

            _instanceMaterial = mats[materialIndex];
        }
        if (_instanceMaterial == null)
        {
            return;
        }

        _instanceMaterial.SetColor("_BaseColor", targetColor);
    }
}