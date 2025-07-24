using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesteRolo : MonoBehaviour
{
    public RectTransform scrollTransform;
    public float duracao = 1.5f;
    public CanvasGroup cg;

    public Vector2 posFinal;
    public Vector2 posInicial;

    private float tempo = 0f;
    private bool animando = true;

    void Start()
    {
        //posFinal = scrollTransform.anchoredPosition;
        //posInicial = new Vector2(posFinal.x - 200f, posFinal.y); // comece mais à esquerda
        //scrollTransform.anchoredPosition = posInicial;

        scrollTransform.localScale = new Vector3(0f, 1f, 1f);
    }

    void Update()
    {
        if (!animando) return;

        tempo += Time.deltaTime / duracao;
        float t = Mathf.Clamp01(tempo);

        // Atualiza escala
        float escala = Mathf.Lerp(0f, 1f, t);
        scrollTransform.localScale = new Vector3(escala, 1f, 1f);
        cg.alpha = escala;

        // Atualiza posição
        Vector2 pos = Vector2.Lerp(posInicial, posFinal, t);
        scrollTransform.anchoredPosition = pos;

        if (t >= 1f)
            animando = false;
    }


}
