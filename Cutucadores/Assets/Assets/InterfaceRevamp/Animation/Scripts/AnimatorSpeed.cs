using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimatorSpeed : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

   private Animator anim;

    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float hoverSpeed = 2f;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        anim.speed = hoverSpeed; // acelera
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.speed = normalSpeed; // volta ao normal
    }
}
