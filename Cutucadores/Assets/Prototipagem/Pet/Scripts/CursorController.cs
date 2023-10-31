using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public GameObject cursorObject,config, inicialMenu; // O objeto 3D que substituirá o cursor do mouse
    public Camera mainCamera;
    public Transform targetObject, targetObject2; // O objeto 3D para onde o cursor 3D será movido
    public Animator animHand; // animações da mão
    public Animator animBook; // animações caderno
    public Image Blueprint; // blueprint seleção
   

    private bool isMoving = false;
    private float moveTime = 2f; // Tempo total (tempo que o cursor fica na posição de destino + tempo de deslocamento)
    private float pauseTime = 1.8f; // Tempo que o cursor fica na posição de destino
    private float moveDuration; // Tempo de deslocamento
    private float timer = 0.0f; // verificação de tempo max

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        mainCamera = Camera.main;

        //Cursor.visible = false; // Esconde o cursor do mouse
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
            cursorObject.transform.position = mainCamera.ScreenToWorldPoint(mousePosition);

        }
        else
        {
            // O objeto 3D está se movendo para a posição de destino para realizar a animação -- OBS, cursor do mouse não tem a posição modificada
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
                // O tempo de espera acabou, animação executada, retornar o objeto 3D para a posição do mouse --- verificar calculo para retorno suave
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

    public void ReturnPage(bool kitten) // anim para retorno
    {
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject2.position;

        isMoving = true;
        StartCoroutine(DelayPreviousPage());
        timer = 0.0f;
    }

    public void CloseBook(bool kitten) // anim para fechar livro
    {
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayCloseBook());
        //timer = 0.0f;

    }

    public void OpenBook(bool kitten) // anim para abrir livro
    {
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayOpenBook());
        //timer = 0.0f;
    }

    public void BlueprintSelect() // anim para blueprint de seleção
    {
        Cursor.visible = true;
        cursorObject.SetActive(false);

        // Movimentar blueprint para perto da camera.
        Vector3 posicaoAtual = Blueprint.transform.localPosition;
        posicaoAtual.z = -183f; // posição do obj

        Blueprint.transform.localPosition = posicaoAtual;
    }



    // Delay animações
    IEnumerator DelayNextPage()
    {
        yield return new WaitForSeconds(0.25f);
        animHand.Play("arm_AMT|VirarPag");
        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
    }

     IEnumerator DelayPreviousPage()
    {
        yield return new WaitForSeconds(0.25f);
        animHand.Play("arm_AMT|VirarPag 0");
        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
    }

    IEnumerator DelayCloseBook()
    {

        animBook.Play("Armature|ViraPag");
        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
        yield return new WaitForSeconds(1.20f);
        animHand.Play("arm_AMT|FecharCaderno");
       
    }

    IEnumerator DelayOpenBook()
    {

        animHand.Play("arm_AMT|FecharCaderno 0");
        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
        yield return new WaitForSeconds(1.20f);
        animBook.Play("Armature|ViraPag 0");

    }
    IEnumerator LockMousePosition(Transform armPosition,float duration)
    {
        float timer = 0;
        Vector2 armScreenPosition = mainCamera.WorldToScreenPoint(armPosition.position);
        while(timer<duration)
        {
            armScreenPosition = mainCamera.WorldToScreenPoint(armPosition.position);
            Mouse.current.WarpCursorPosition(armScreenPosition);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}