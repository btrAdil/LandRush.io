using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleFootStep : MonoBehaviour
{
    private Material material;
    public float fadeTime ;

    private void OnEnable()
    {
        material=GetComponent<MeshRenderer>().material;
        StartCoroutine(FadeAndDestroy());
    }
    IEnumerator FadeAndDestroy()
    {
        float timeElapsed = 0;

        while (timeElapsed < fadeTime)
        {
            timeElapsed += Time.deltaTime;
            material.color = new Color(material.color.r, material.color.g, material.color.b, 1f - timeElapsed / fadeTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
