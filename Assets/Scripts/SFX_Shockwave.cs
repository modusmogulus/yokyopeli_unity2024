using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
//CREDIT: https://halisavakis.com/my-take-on-shaders-shockwave-effect/

public class SFX_Shockwave : MonoBehaviour
{

    public Material shockWaveMaterial;

    void Start()
    {
        shockWaveMaterial.SetFloat("_Radius", -0.2f);
    }

    void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            Vector2 screenPos = new Vector2(Screen.width / 2, Screen.height / 2);
            shockWaveMaterial.SetFloat("_CenterX", screenPos.x);
            shockWaveMaterial.SetFloat("_CenterY", screenPos.y);
            StopAllCoroutines();
            StartCoroutine(ShockWaveEffect());
        }
    }

    IEnumerator ShockWaveEffect()
    {
        float tParam = 0;
        float waveRadius;
        while (tParam < 1)
        {
            tParam += Time.deltaTime * 2;
            waveRadius = Mathf.Lerp(-0.2f, 2, tParam);
            shockWaveMaterial.SetFloat("_Radius", waveRadius);
            yield return null;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, shockWaveMaterial, 0);
    }
}