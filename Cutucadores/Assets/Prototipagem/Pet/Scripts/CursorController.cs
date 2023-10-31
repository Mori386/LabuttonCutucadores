using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public GameObject cursorObject,config, inicialMenu, roles, roleClientHost; 
    public Camera mainCamera;
    public Transform targetObject, targetObject2; // O objeto 3D para onde o cursor 3D ser� movido
    public Animator animHand, animBook; // anima��es da m�o e caderno
    public Image Blueprint; // blueprint sele��o
   

    private bool isMoving = false;
    private float moveTime = 1.1f; // Tempo total (tempo que o cursor fica na posi��o de destino + tempo de deslocamento)
    private float pauseTime = 0.9f; // Tempo que o cursor fica na posi��o de destino
    private float moveDuration; // Tempo de deslocamento
    private float timer = 0.0f; // verifica��o de tempo max

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        mainCamera = Camera.main;

        //Cursor.visible = false; // Esconde o cursor do mouse
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
            cursorObject.transform.position = mainCamera.ScreenToWorldPoint(mousePosition);

        }
        else
        {
            // O objeto 3D est� se movendo para a posi��o de destino para realizar a anima��o
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
                // O tempo de espera acabou, anima��o executada, retornar o objeto 3D para a posi��o do mouse --- calculo realizado no LockMousePosition
                //cursorObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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

    public void BlueprintSelect() // anim para blueprint de sele��o
    {
        Cursor.visible = true;
        cursorObject.SetActive(false);

        // Movimentar blueprint para perto da camera.
        Vector3 posicaoAtual = Blueprint.transform.localPosition;
        posicaoAtual.z = -183f; // posi��o do obj

        Blueprint.transform.localPosition = posicaoAtual;
    }



    // Delay anima��es
    IEnumerator DelayNextPage()
    {
        yield return new WaitForSeconds(0.25f);
        animHand.Play("arm_AMT|VirarPag");

        yield return new WaitForSeconds(0.50f);
        inicialMenu.SetActive(false);

        yield return new WaitForSeconds(0.10f);
        config.SetActive(true);

        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
    }

     IEnumerator DelayPreviousPage()
    {
        yield return new WaitForSeconds(0.25f);
        animHand.Play("arm_AMT|VirarPag 0");

        yield return new WaitForSeconds(0.50f);
        config.SetActive(false);

        yield return new WaitForSeconds(0.10f);
        inicialMenu.SetActive(true);
        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
    }

    IEnumerator DelayCloseBook()
    {

        animBook.Play("Armature|ViraPag");

        yield return new WaitForSeconds(0.25f);
        roles.SetActive(false);
        inicialMenu.SetActive(false);

        yield return new WaitForSeconds(1.10f);
        animHand.Play("arm_AMT|FecharCaderno");

        yield return new WaitForSeconds(0.65f);
        roleClientHost.SetActive(true);

        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));

    }

    IEnumerator DelayOpenBook()
    {

        animBook.Play("Armature|ViraPag 0");

        yield return new WaitForSeconds(0.50f);
        roleClientHost.SetActive(false);
        animHand.Play("arm_AMT|FecharCaderno 0");


        yield return new WaitForSeconds(0.75f);
        roles.SetActive(true);
        inicialMenu.SetActive(true);
        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));

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