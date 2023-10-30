using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public GameObject cursorObject; // O objeto 3D que substituirá o cursor do mouse
    public Transform targetObject, targetObject2; // O objeto 3D para onde o cursor 3D será movido
    public Animator anim; // animações da mão

    private bool isMoving = false;
    private float moveTime = 2f; // Tempo total (tempo que o cursor fica na posição de destino + tempo de deslocamento)
    private float pauseTime = 1.8f; // Tempo que o cursor fica na posição de destino
    private float moveDuration; // Tempo de deslocamento
    private float timer = 0.0f; // verificação de tempo max

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        Cursor.visible = false; // Esconde o cursor do mouse
        Cursor.lockState = CursorLockMode.Confined; // Mantém o cursor dentro da janela do jogo.

        startPosition = cursorObject.transform.position; //para retorno da posição inicial
        moveDuration = moveTime - pauseTime; // valor do tempo de deslocamento
        //anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isMoving)
        {
            // Atualiza a posição do cursor 3D para seguir o mouse
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 2.3f; // Distância do cursor em relação à câmera
            cursorObject.transform.position = Camera.main.ScreenToWorldPoint(mousePosition);

        }
        else
        {
            // O cursor 3D está se movendo para a posição de destino para realizar a animação
            timer += Time.deltaTime;

            if (timer < moveDuration)
            {
                // Interpole suavemente a posição do cursor 3D durante o tempo de deslocamento
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
                // O tempo de espera acabou, animação executada, retornar à posição do mouse --- verificar calculo para retorno suave
                cursorObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                isMoving = false;
            }
        }
    }

    public void NextPage(bool kitten) // chamando esse void através dos botões em cena, caso o parametro do metodo seja verdadeiro, a sequência do update é executada
    {
        // Inicializa a posição de início e destino
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