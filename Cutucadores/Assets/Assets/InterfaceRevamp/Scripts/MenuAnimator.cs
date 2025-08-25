using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
    [Header("Objetos da cena")]
    public GameObject escavadeira1; // Objeto 1
    public GameObject escavadeira2; // Objeto 2
    public GameObject toupeira;     // Objeto 3

    void Start()
    {
        AnimateEscavadeiras();
    }

    public void AnimateEscavadeiras()
    {
        // Posição inicial fora da tela
        escavadeira1.transform.localPosition = new Vector3(-800, escavadeira1.transform.localPosition.y, 0);
        escavadeira2.transform.localPosition = new Vector3(800, escavadeira2.transform.localPosition.y, 0);

        // Entra com efeito "pulo"
        LeanTween.moveLocalX(escavadeira1, 0, 1f).setEase(LeanTweenType.easeOutBounce);
        LeanTween.moveLocalX(escavadeira2, 0, 1f).setEase(LeanTweenType.easeOutBounce).setDelay(0.2f);
    }

    public void DropToupeira()
    {
        toupeira.SetActive(true);

        // Reset inicial da toupeira
        toupeira.transform.localPosition = new Vector3(0, 600, 0);
        toupeira.transform.localRotation = Quaternion.identity;

        // Faz cair no Y
        LeanTween.moveLocalY(toupeira, 0, 1f).setEase(LeanTweenType.easeInBounce);

        // Faz girar ao cair
        LeanTween.rotateAroundLocal(toupeira, Vector3.forward, 360, 1f)
                 .setEase(LeanTweenType.easeOutBack);
    }
}
