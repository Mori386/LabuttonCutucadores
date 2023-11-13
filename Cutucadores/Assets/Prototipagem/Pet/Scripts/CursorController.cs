using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public GameObject cursorObject, cursorObject2, config, inicialMenu, roles, roleClientHost, Blueprint, credits;
    public GameObject[] Okays;
    public Camera mainCamera;
    public Transform targetObject, targetObject2, targetObject3, Book; // O objeto 3D para onde o cursor 3D ser� movido
    public Animator animHand, animBook, animPage; // anima��es da m�o e caderno
    //public Image  // blueprint sele��o


    private bool isMoving = false;
    private bool isChange = false;

    public float moveTime = 2f; // Tempo total (tempo que o cursor fica na posi��o de destino + tempo de deslocamento)
    public float pauseTime = 1.5f; // Tempo que o cursor fica na posi��o de destino
    public float smoothTime = 0.5f; 

    private float moveDuration; // Tempo de deslocamento
    private float timer = 0.0f; // verifica��o de tempo max

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public Vector3 mousePosition;

    void Start()
    {
        mainCamera = Camera.main;

        Cursor.visible = false; // Esconde o cursor do mouse
        Cursor.lockState = CursorLockMode.Confined; // Mant�m o cursor dentro da janela do jogo.

        startPosition = cursorObject.transform.position; //para retorno da posi��o inicial
        moveDuration = moveTime - pauseTime; // valor do tempo de deslocamento
                                            
        StartCoroutine(MoveBookSmoothly());
    }

    void Update()
    {
        if (!isMoving)
        {
            // Atualiza a posi��o do cursor 3D para seguir o mouse
            mousePosition = Input.mousePosition;
            mousePosition.z = 3f; // Dist�ncia do cursor em rela��o � c�mera
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

        if (isChange == true)
        {
            ChangeCursorObject(cursorObject2, 10f);
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

    public void Credits(bool kitten)
    {
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayCredits());
        timer = 0.0f;
    }

    public void ReturnPage(bool kitten) // anim para retorno
    {
       startPosition = targetObject.transform.position;
       targetPosition = targetObject.position;

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
        timer = 0.0f;

    }

    public void OpenBook(bool kitten) // anim para abrir livro
    {
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayOpenBook());
        timer = 0.0f;
    }

    public void BlueprintSelect() // anim para blueprint de sele��o
    {
        Blueprint.SetActive(true);

        // Movimentar blueprint para perto da camera.
        Vector3 posicaoAtual = Blueprint.transform.localPosition;
        posicaoAtual.z = -183f; // posi��o do obj

        Blueprint.transform.localPosition = posicaoAtual;

        isChange = true;
    }

    void ChangeCursorObject(GameObject newCursorObject, float newMousePositionZ)
    {
        Debug.Log("change");
        // Desativa o cursor anterior (se houver um)
        if (cursorObject != null)
        {
            cursorObject.SetActive(false);
        }

        // Atualiza a posi��o z do mouse
        mousePosition.z = newMousePositionZ;

        // Ativa o novo cursor
        newCursorObject.SetActive(true);

        // Atualiza a refer�ncia para o novo objeto de cursor
        cursorObject = newCursorObject;
    }

    public void OkPlayers(int playerNumber) // confirma��o dos players
    {

        switch (playerNumber) // vinculado a cada highligthed
        {
            case 0:
                startPosition = cursorObject2.transform.position;
                targetPosition = Okays[0].transform.position;
                isMoving = true;
                timer = 0.4f;
                SetActiveWithDelay(Okays[0], true, 0.8f);
                break;

            case 1:
                startPosition = cursorObject2.transform.position;
                targetPosition = Okays[1].transform.position;
                isMoving = true;
                timer = 0.4f;
                SetActiveWithDelay(Okays[1], true, 0.8f);
                break;

            case 2:
                startPosition = cursorObject2.transform.position;
                targetPosition = Okays[2].transform.position;
                isMoving = true;
                timer = 0.4f;
                SetActiveWithDelay(Okays[2], true, 0.8f);
                break;

            case 3:
                startPosition = cursorObject2.transform.position;
                targetPosition = Okays[3].transform.position;
                isMoving = true;
                timer = 0.4f;
                SetActiveWithDelay(Okays[3], true, 0.8f);
                break;

            default:
                // Caso nenhum jogador v�lido seja selecionado
                Debug.LogError("Jogador inv�lido: " + playerNumber);
                break;
        }

    }

    public void SetActiveWithDelay(GameObject go, bool state, float delay)
    {
        StartCoroutine(SetActiveDelayCoroutine(go, state, delay));
    }
    IEnumerator SetActiveDelayCoroutine(GameObject go, bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(state);
    }
    // Delay anima��es
    IEnumerator DelayNextPage()
    {
        yield return new WaitForSeconds(0.25f);
        animHand.Play("arm_AMT|VirarPag");

        yield return new WaitForSeconds(0.5f);
        animPage.Play("FolhaVirando");

        yield return new WaitForSeconds(0.15f);
        inicialMenu.SetActive(false);

        yield return new WaitForSeconds(0.10f);
        config.SetActive(true);

        StartCoroutine(LockMousePosition(targetObject, moveTime));
    }
    IEnumerator DelayCredits()
    {
        yield return new WaitForSeconds(0.25f);
        animHand.Play("arm_AMT|VirarPag");

        yield return new WaitForSeconds(0.5f);
        animPage.Play("FolhaVirando");

        yield return new WaitForSeconds(0.15f);
        config.SetActive(false);

        yield return new WaitForSeconds(0.10f);
        credits.SetActive(true);

        StartCoroutine(LockMousePosition(targetObject, moveTime));
    }


    IEnumerator DelayPreviousPage()
    {
        
        animHand.Play("arm_AMT|VirarPag 0");

        yield return new WaitForSeconds(0.15f);
        animPage.Play("FolhaVirando 0");

        yield return new WaitForSeconds(0.25f);
        config.SetActive(false);

        yield return new WaitForSeconds(0.10f);
        inicialMenu.SetActive(true);
        StartCoroutine(LockMousePosition(targetObject, moveTime));
    }

    IEnumerator DelayCloseBook()
    {

        animHand.Play("arm_AMT|VirarPag 0");

        yield return new WaitForSeconds(0.15f);
        animBook.Play("CadernoFechando");

        yield return new WaitForSeconds(0.25f);
        roles.SetActive(false);
        inicialMenu.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        roleClientHost.SetActive(true);

        StartCoroutine(LockMousePosition(targetObject, moveTime));

    }

    IEnumerator DelayOpenBook()
    {
        animHand.Play("arm_AMT|VirarPag");

        yield return new WaitForSeconds(0.35f);
        animBook.Play("CadernoFechando 0");
        

        yield return new WaitForSeconds(0.50f);
        roleClientHost.SetActive(false);
        //animHand.Play("CadernoFechando 0");


        yield return new WaitForSeconds(0.25f);
        roles.SetActive(true);
        inicialMenu.SetActive(true);
        StartCoroutine(LockMousePosition(targetObject, moveTime));

    }

    IEnumerator MoveBookSmoothly()
    {
        Vector3 pontoOrigem = Book.position;
        float duration = 1.0f; // Tempo total da transi��o em segundos

        for (float t = 0; t < 1.0f; t += Time.deltaTime / duration)
        {
            Vector3 pontoAtual = Vector3.Lerp(pontoOrigem, targetObject3.position, t);
            pontoAtual.y = pontoOrigem.y + t * t * (targetObject3.position.y - pontoOrigem.y);

            Debug.Log("Ponto atual: " + pontoAtual);

            // Movimenta o objeto para o ponto atual
            Book.position = pontoAtual;

            yield return null; // Aguarda at� o pr�ximo quadro
        }

        animBook.Play("CadernoFechando 0");
        roles.SetActive(true);

        yield return new WaitForSeconds(0.50f);
        animPage.Play("FolhaVirando");

        yield return new WaitForSeconds(0.45f);
        inicialMenu.SetActive(true);


    }

    IEnumerator LockMousePosition(Transform armPosition, float duration) // retornar o mouse suavimente para a posi��o
    {
        float timer = 0;
        Vector2 armScreenPosition = mainCamera.WorldToScreenPoint(armPosition.position);
        while (timer < duration)
        {
            armScreenPosition = mainCamera.WorldToScreenPoint(armPosition.position);
            Mouse.current.WarpCursorPosition(armScreenPosition);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}