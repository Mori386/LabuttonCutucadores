using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public GameObject cursorObject; // O objeto 3D que substituir� o cursor do mouse
    public Transform targetObject, targetObject2; // O objeto 3D para onde o cursor 3D ser� movido
    public Animator anim; // anima��es da m�o

    private bool isMoving = false;
    private float moveTime = 2f; // Tempo total (tempo que o cursor fica na posi��o de destino + tempo de deslocamento)
    private float pauseTime = 1.8f; // Tempo que o cursor fica na posi��o de destino
    private float moveDuration; // Tempo de deslocamento
    private float timer = 0.0f; // verifica��o de tempo max

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        Cursor.visible = false; // Esconde o cursor do mouse
        Cursor.lockState = CursorLockMode.Confined; // Mant�m o cursor dentro da janela do jogo.

        startPosition = cursorObject.transform.position; //para retorno da posi��o inicial
        moveDuration = moveTime - pauseTime; // valor do tempo de deslocamento
        //anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isMoving)
        {
            // Atualiza a posi��o do cursor 3D para seguir o mouse
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 2.3f; // Dist�ncia do cursor em rela��o � c�mera
            cursorObject.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);

        }
        else
        {
            // O cursor 3D est� se movendo para a posi��o de destino para realizar a anima��o
            timer += Time.deltaTime;

            if (timer < moveDuration)
            {
                // Interpole suavemente a posi��o do cursor 3D durante o tempo de deslocamento
                float t = timer / moveDuration;
                cursorObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }
            else if (timer >= moveDuration && timer < moveTime)
            {
                // O tempo de deslocamento acabou; o cursor fica parado
                cursorObject.transform.position = targetPosition;
            }
            else
            {
                // O tempo de espera acabou, anima��o executada, retornar � posi��o do mouse --- verificar calculo para retorno suave
                cursorObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                isMoving = false;
            }
        }
    }

    public void NextPage(bool kitten) // chamando esse void atrav�s dos bot�es em cena, caso o parametro do metodo seja verdadeiro, a sequ�ncia do update � executada
    {
        // Inicializa a posi��o de in�cio e destino
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayNextPage());
        timer = 0.0f;
    }

    public void ReturnPage(bool kitten)
    {
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject2.position;

        isMoving = true;
        StartCoroutine(DelayPreviousPage());
        timer = 0.0f;
    }

    IEnumerator DelayNextPage()
    {
        yield return new WaitForSeconds(0.25f);
        anim.Play("arm_AMT|VirarPag");
    }

     IEnumerator DelayPreviousPage()
    {
        yield return new WaitForSeconds(0.25f);
        anim.Play("arm_AMT|VirarPag 0");
    }
}