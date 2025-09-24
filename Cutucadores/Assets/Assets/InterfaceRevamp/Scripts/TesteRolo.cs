using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesteRolo : MonoBehaviour
{
    public RectTransform scrollTransform;
    public float duracao = 1.5f;
    public CanvasGroup cg;

    private float tempo = 0f;
    private bool animando = true;

    void Start()
    {
        scrollTransform.localScale = new Vector3(0f, 1f, 1f);

        if (cg != null)
            cg.alpha = 0f;
    }

    void Update()
    {
        if (!animando) return;

        tempo += Time.deltaTime / duracao;
        float t = Mathf.Clamp01(tempo);

  
        float escalaX = Mathf.Lerp(0f, 1f, t);
        scrollTransform.localScale = new Vector3(escalaX, 1f, 1f);

        if (cg != null)
            cg.alpha = escalaX;

        if (t >= 1f)
            animando = false;
    }
}
