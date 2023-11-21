using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    [Header("CURSOR")]
    public GameObject cursorObject;
    public GameObject cursorObject2;
    public GameObject mira;
    public Camera mainCamera;

    [Header("OBJETOS DO LIVRO")]
    public GameObject config;
    public GameObject inicialMenu;
    public GameObject roles;
    public GameObject roleClientHost;
    public GameObject Blueprint;
    public GameObject credits;
    public Transform targetObject, targetObject2, targetObject3, Book; // O objeto 3D para onde o cursor 3D será movido
    public Animator animHand, animBook, animPage; // animações da mão e caderno

    [Header("OBJETOS DO BLUEPRINT")]
    public GameObject[] Okays;
    public GameObject[] Polaroids;
    public Material[] tanksMaterial;
    public GameObject[] tankMesh; // acessar o renderer corretamente e trocar o material
    public GameObject[] tankDrill; // acessar o renderer corretamente e trocar o material
    public Material Bluematerial; 
    public GameObject tanques; // desativas todos, verificar se já não há a declaração no game manager
    public GameObject Lampada;
    public GameObject Luz;


    [Header("PARA ANIMAÇÃO")]
    public float moveTime = 2f; // Tempo total (tempo que o cursor fica na posição de destino + tempo de deslocamento)
    public float pauseTime = 1.5f; // Tempo que o cursor fica na posição de destino
    public float smoothTime = 0.5f;

    private float moveDuration; // Tempo de deslocamento
    private float timer = 0.0f; // verificação de tempo max

    [Header("BOOLEANOS")]
    private bool isMoving = false;
    private bool isChange = false;
    private bool isReturn = false;

    [Header("VETORES")]
    private Vector3 startPosition;
    private Vector3 targetPosition;
    public Vector3 mousePosition;

    void Start()
    {
        mainCamera = Camera.main;

        Cursor.visible = false; // Esconde o cursor do mouse
        Cursor.lockState = CursorLockMode.Confined; // Mantém o cursor dentro da janela do jogo.

        startPosition = cursorObject.transform.position; //para retorno da posição inicial
        moveDuration = moveTime - pauseTime; // valor do tempo de deslocamento

        StartCoroutine(MoveBookSmoothly());
        cursorObject2.SetActive(false);
    }

    void Update()
    {
        Cursor.lockState = CursorLockMode.Confined;

        if (!isMoving)
        {
            // Atualiza a posição do cursor 3D para seguir o mouse
            mousePosition = Input.mousePosition;
            mousePosition.z = 2.7f; // Distância do cursor em relação à câmera
            cursorObject.transform.position = mainCamera.ScreenToWorldPoint(mousePosition);
            mira.transform.position = mainCamera.ScreenToWorldPoint(mousePosition);

            if (isChange == true) // else buga a animação
            {
                // atualiza a posição de cursorObject2 para seguir o mouse
                cursorObject2.SetActive(true);
                mousePosition.z = 1f;
                cursorObject2.transform.position = mainCamera.ScreenToWorldPoint(mousePosition);
            }

            if (isReturn == true)
            {
                cursorObject.SetActive(true);
                cursorObject2.SetActive(false);
                mousePosition.z = 2.7f;
            }

        }
        else
        {
            // Lógica de animação suave
            timer += Time.deltaTime;

            // Interpola suavemente a posição do cursor 3D durante o tempo de deslocamento
            if (timer < moveDuration)
            {
                float t = timer / moveDuration;
                cursorObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                cursorObject2.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }

            // O tempo de deslocamento acabou; o cursor fica parado
            else if (timer >= moveDuration && timer < moveTime)
            {
                cursorObject.transform.position = targetPosition;
                cursorObject2.transform.position = targetPosition;
            }
            else
            {
                isMoving = false;
            }
        }

        if (isChange == true) // else buga a animação
        {
            //ChangeCursorObject(cursorObject2, 10f);
            cursorObject2.SetActive(true);
            cursorObject.SetActive(false);
        }

    }

    // todos os voids abaixo são chamados através de botões na cena, de acordo com os seus respectivos nomes
    public void NextPage(bool kitten)  // passar pag
    {
        // Inicializa a posição de início e destino
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayNextPage());
        timer = 0.0f;
    }

    public void Credits(bool kitten) // menu creditos
    {
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayCredits());
        timer = 0.0f;
    }

    public void ReturnCredits(bool kitten) // sair do menu de creditos
    {
        startPosition = cursorObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayReturnCredits());
        timer = 0.0f;
    }

    public void ReturnPage(bool kitten) // anim para retorno
    {
        startPosition = cursorObject.transform.position;
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
        startPosition = targetObject.transform.position;
        targetPosition = targetObject.position;

        isMoving = true;
        StartCoroutine(DelayOpenBook());
        timer = 0.0f;
    }

    public void BlueprintSelect() // anim para blueprint de seleção
    {
        Blueprint.SetActive(true);
        tanques.SetActive(true);
        roleClientHost.SetActive(false);
        Luz.SetActive(true);
        Lampada.SetActive(false);

        // Movimentar blueprint para perto da camera.
        Vector3 posicaoAtual = Blueprint.transform.localPosition;
        posicaoAtual.z = -179f; // posição do obj

        Blueprint.transform.localPosition = posicaoAtual;
        isChange = true;
        isReturn = false;
    }

    public void ReturnBlueprintSelect() // sair do blue de seleção
    {
        Blueprint.SetActive(false);
        tanques.SetActive(false);
        Luz.SetActive(false);
        Lampada.SetActive(true);
        roleClientHost.SetActive(true);
        isReturn = true;
        isChange = false;
    }

    public void ChangeMaterial(Material newMaterial, GameObject Tank, GameObject Drill)
    {
        Renderer renderer1 = Tank.GetComponent<Renderer>();
        Renderer renderer2 = Drill.GetComponent<Renderer>();

        if (renderer1 != null && renderer2 != null)
        {
            renderer1.material = newMaterial;
            renderer2.material = newMaterial;
        }
        else
        {
            Debug.LogWarning("O objeto não possui um componente Renderer.");
        }
    }

    public void OkPlayers(int playerNumber) // confirmação dos players
    {

        switch (playerNumber) // vinculado a cada highligthed em cena
        {
            case 0:
                Debug.Log("Anim roda");
                startPosition = cursorObject2.transform.position;
                targetPosition = Okays[0].transform.position;
                Polaroids[0].SetActive(false);
                ChangeMaterial(tanksMaterial[0], tankMesh[0], tankDrill[0]);
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
                Polaroids[4].SetActive(false);
                isMoving = true;
                timer = 0.4f;
                SetActiveWithDelay(Okays[3], true, 0.8f);
                break;

            default:
                // Caso nenhum jogador válido seja selecionado
                Debug.LogError("Jogador inválido: " + playerNumber);
                break;
        }

    }

    public void SetActiveWithDelay(GameObject go, bool state, float delay) // delay global
    {
        StartCoroutine(SetActiveDelayCoroutine(go, state, delay));
    }
    IEnumerator SetActiveDelayCoroutine(GameObject go, bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(state);
    }

    // Delay animações
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

        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
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

        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
    }

    IEnumerator DelayReturnCredits()
    {
        animHand.Play("arm_AMT|VirarPag 0");

        yield return new WaitForSeconds(0.15f);
        animPage.Play("FolhaVirando 0 0");

        yield return new WaitForSeconds(0.15f);
        config.SetActive(true);

        yield return new WaitForSeconds(0.10f);
        credits.SetActive(false);

        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
    }

    IEnumerator DelayPreviousPage()
    {
        animHand.Play("arm_AMT|VirarPag 0");

        yield return new WaitForSeconds(0.15f);
        animPage.Play("FolhaVirando 0");

        yield return new WaitForSeconds(0.15f);
        config.SetActive(false);

        yield return new WaitForSeconds(0.10f);
        inicialMenu.SetActive(true);
        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
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

        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));
    }

    IEnumerator DelayOpenBook()
    {
        animHand.Play("arm_AMT|VirarPag");

        yield return new WaitForSeconds(0.35f);
        animBook.Play("CadernoFechando 0");

        yield return new WaitForSeconds(0.50f);
        roleClientHost.SetActive(false);

        yield return new WaitForSeconds(0.25f);
        roles.SetActive(true);
        inicialMenu.SetActive(true);
        StartCoroutine(LockMousePosition(cursorObject.transform, moveTime));

    }

    IEnumerator MoveBookSmoothly() 
    {
        Vector3 pontoOrigem = Book.position;
        float duration = 1.0f; // Tempo total da transição em segundos

        for (float t = 0; t < 1.0f; t += Time.deltaTime / duration)
        {
            Vector3 pontoAtual = Vector3.Lerp(pontoOrigem, targetObject3.position, t);
            pontoAtual.y = pontoOrigem.y + t * t * (targetObject3.position.y - pontoOrigem.y);

            //Debug.Log("Ponto atual: " + pontoAtual);

            // Movimenta o objeto para o ponto atual
            Book.position = pontoAtual;

            yield return null; // Aguarda até o próximo quadro
        }

        animBook.Play("CadernoFechando 0");
        roles.SetActive(true);

        yield return new WaitForSeconds(0.50f);
        animPage.Play("FolhaVirando");

        yield return new WaitForSeconds(0.45f);
        inicialMenu.SetActive(true);

    }

    IEnumerator LockMousePosition(Transform armPosition, float duration) // retornar o mouse suavimente para a posição
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