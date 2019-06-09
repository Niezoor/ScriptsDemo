using UnityEngine;
using System.Collections;
using TMPro;

public class KeepParentRenderLayer : MonoBehaviour {
    public int LayerOffset = 1;
    public bool KeepingActive = true;
	public bool KeepAlpha = true;
	public bool KeepEnable = false;
    private SpriteRenderer parentRenderer;
    private SpriteRenderer childSpriteRenderer;
    private Renderer childRenderer;
    private TextMesh childTextMesh;
	private TextMeshPro childTextMeshPro;
    private bool ThisIsTextMesh;

    void Start()
	{
		if (transform.parent == null) {
			KeepingActive = false;
			return;
		}
		if (transform.parent.GetComponent<SpriteRenderer> () != null) {
			parentRenderer = transform.parent.GetComponent<SpriteRenderer> ();
		} else if (transform.parent.parent != null) {
			if (transform.parent.parent.GetComponent<SpriteRenderer> () != null) {
				parentRenderer = transform.parent.parent.GetComponent<SpriteRenderer> ();
			} else if (transform.parent.parent.parent != null && transform.parent.parent.parent.GetComponent<SpriteRenderer> () != null) {
				parentRenderer = transform.parent.parent.parent.GetComponent<SpriteRenderer> ();
			}
		} else {
			KeepingActive = false;
		}
        childRenderer = GetComponent<Renderer>();
        childSpriteRenderer = GetComponent<SpriteRenderer>();
        if (childSpriteRenderer == null)
        {
            ThisIsTextMesh = true;
            childTextMesh = GetComponent<TextMesh>();
			if (childTextMesh == null) {
				childTextMeshPro = GetComponent<TextMeshPro>();
			}
        }
    }

    void Update()
    {
        if (KeepingActive)
        {
			if (parentRenderer != null) {
				if (childRenderer.sortingOrder != parentRenderer.sortingOrder + LayerOffset)
					childRenderer.sortingOrder = parentRenderer.sortingOrder + LayerOffset;
				if (KeepAlpha) {
					if (!ThisIsTextMesh) {
						if (childSpriteRenderer != null) {
							if (childSpriteRenderer.color != parentRenderer.color) {
								Color color = parentRenderer.color;
								Color color2 = childSpriteRenderer.color;

								color2.a = color.a;

								childSpriteRenderer.color = color2;
								if (KeepEnable) {
									if (color2.a == 0) {
										childSpriteRenderer.enabled = false;
									} else {
										childSpriteRenderer.enabled = true;
									}
								}
							}
						}
					} else {
						if (childTextMesh != null) {
							if (childTextMesh.color != parentRenderer.color) {
								Color color = parentRenderer.color;
								Color color2 = childTextMesh.color;

								color2.a = color.a;

								childTextMesh.color = color2;
							}
						} else if (childTextMeshPro != null) {
							if (childTextMeshPro.color != parentRenderer.color) {
								Color color = parentRenderer.color;
								Color color2 = childTextMeshPro.color;

								color2.a = color.a;

								childTextMeshPro.color = color2;
							}
						}
					}
				}
			}
        }
    }
}
