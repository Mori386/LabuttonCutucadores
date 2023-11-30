using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    [HideInInspector] public Camera mainCamera;

    readonly private float animSpeedUpMultiplier = 2f; // Valor de multiplicacao de velocidade quando o jogador acelerar as animacoes
    [Header("Mão")]
    [Header("|----- Main Menu -----|")]
    public Transform mainMenuHand;
    [Header("Livro")]
    public Transform bookOnTableTransform;
    public Animator animBook;
    [Header("Pagina")]
    public Transform folhaDoCaderno;
    public Animator animFolhaDoCaderno;
    [Header("Canvas")]
    public CanvasGroup logoCanvasLayer;
    public CanvasGroup playCanvasLayer;
    [Header("Cursor")]
    public Transform mira;
    public GameObject cursorObject2;
    [Header("Host and client page")]
    [Header("|----- Host Client Menu-----|")]
    public TMP_InputField nicknameInputField;
    public CanvasGroup clientHostCanvas; // continuar daki mexer ele 
    public CanvasGroup clientHostPaper;
    public RectTransform returnToMainMenuFromClientHost;
    [Header("Create/Join page")]
    private bool selectedHost;
    public TMP_InputField sessionNameInputfield;
    public CanvasGroup mapSelection;
    public Image mapPreview;
    public Sprite[] mapPreviewImages;
    private int mapInPreviewID = 0;
    public CanvasGroup createJoinPaper;
    public CanvasGroup createJoinPaperDefaultGroup;
    public CanvasGroup createJoinPaperLoadingGroup;

    [Header("OBJETOS DO LIVRO")]
    public GameObject[] config;
    public GameObject[] credits;
    public Transform animHandStartingPoint, targetObject2, book; // O objeto 3D para onde o cursor 3D será movido
    public Animator animHand; // animações da mão e caderno

    [Header("OBJETOS DO BLUEPRINT")]
    public GameObject Blueprint;
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

    public RectTransform canvasRect;
    public RectTransform blueprintRect;

    private float moveDuration; // Tempo de deslocamento
    private float timer = 0.0f; // verificação de tempo max

    [Header("BOOLEANOS")]
    private bool isMoving = false;
    private bool isChange = false;
    private bool isReturn = false;

    [Header("VETORES")]
    private Vector3 startPosition;
    private Vector3 targetPosition;
    void Start()
    {
        mainCamera = Camera.main;

        Cursor.visible = false; // Esconde o cursor do mouse
        Cursor.lockState = CursorLockMode.Confined; // Mantém o cursor dentro da janela do jogo.

        startPosition = mainMenuHand.transform.position; //para retorno da posição inicial
        moveDuration = moveTime - pauseTime; // valor do tempo de deslocamento
        cursorObject2.SetActive(false);
        StartCoroutine(MoveBookSmoothly());

    }
    public void StartHandFollowCursor()
    {
        if (CalculateMousePosInWorldCoroutine == null) CalculateMousePosInWorldCoroutine = StartCoroutine(CalculateMousePosInWorld());
        if (HandFollowCursorCoroutine == null) HandFollowCursorCoroutine = StartCoroutine(HandFollowCursor());
        if (AimFollowCursorCoroutine == null) AimFollowCursorCoroutine = StartCoroutine(AimFollowCursor());
    }
    public void StopHandFollowCursor()
    {
        if (CalculateMousePosInWorldCoroutine != null)
        {
            StopCoroutine(CalculateMousePosInWorldCoroutine);
            CalculateMousePosInWorldCoroutine = null;
        }
        if (HandFollowCursorCoroutine != null)
        {
            StopCoroutine(HandFollowCursorCoroutine);
            HandFollowCursorCoroutine = null;
        }
        if (AimFollowCursorCoroutine != null)
        {
            mira.gameObject.SetActive(false);
            StopCoroutine(AimFollowCursorCoroutine);
            AimFollowCursorCoroutine = null;
        }
    }

    //Get MousePosition in World Coordinates
    private Vector3 mousePosInWorld;
    public Coroutine CalculateMousePosInWorldCoroutine;
    public IEnumerator CalculateMousePosInWorld()
    {
        Vector3 mousePosition;
        while (true)
        {
            mousePosition = Input.mousePosition;
            mousePosition.z = 2.8f;
            mousePosInWorld = mainCamera.ScreenToWorldPoint(mousePosition);
            yield return null;
        }
    }
    public bool IsMouseOutOffApp()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector2 mousePosNormalizedByScreenSize = new Vector2(mousePosition.x, mousePosition.y);
        mousePosNormalizedByScreenSize.x /= Screen.width;
        mousePosNormalizedByScreenSize.y /= Screen.height;
        return !(mousePosNormalizedByScreenSize.x >= 0 && mousePosNormalizedByScreenSize.x <= 1 &&
                mousePosNormalizedByScreenSize.y >= 0 && mousePosNormalizedByScreenSize.y <= 1);
    }

    //Hand follow cursor
    public Coroutine HandFollowCursorCoroutine;
    public IEnumerator HandFollowCursor()
    {
        while (true)
        {
            if (!IsMouseOutOffApp())
            {
                mainMenuHand.position = Vector3.Lerp(mainMenuHand.position, mousePosInWorld, 50f / 10f * Time.fixedDeltaTime);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    //Aim follow cursor
    public Coroutine AimFollowCursorCoroutine;
    public IEnumerator AimFollowCursor()
    {
        mira.gameObject.SetActive(true);
        while (true)
        {
            if (!IsMouseOutOffApp()) mira.transform.position = mousePosInWorld;
            yield return null;
        }
    }


    //Move objet to target position
    public void StartMoveCursorObject(Transform objectTransform, float duration, Vector3 targetPosition)
    {
        if (MoveCursorObjectCoroutine != null) StopCoroutine(MoveCursorObjectCoroutine);
        else
        {
            MoveCursorObjectCoroutine = StartCoroutine(MoveCursorObject(objectTransform, duration, targetPosition));
        }
    }
    public Coroutine MoveCursorObjectCoroutine;
    public IEnumerator MoveCursorObject(Transform objectTransform, float duration, Vector3 targetPosition)
    {
        Vector3 startPos = objectTransform.position;
        float timer = 0f;
        while (timer < duration)
        {
            objectTransform.position = Vector3.Lerp(startPos, targetPosition, timer / duration);
            timer += Time.deltaTime;
            if (Input.GetMouseButton(0)) timer += Time.deltaTime * (animSpeedUpMultiplier - 1);
            yield return null;
        }
        objectTransform.position = targetPosition;
        MoveCursorObjectCoroutine = null;
    }

    // todos os voids abaixo são chamados através de botões na cena, de acordo com os seus respectivos nomes
    public void NextPage(bool kitten)  // passar pag
    {
        // Inicializa a posição de início e destino
        startPosition = mainMenuHand.position;
        targetPosition = animHandStartingPoint.position;

        isMoving = true;
        StartCoroutine(DelayNextPage());
        timer = 0.0f;
    }

    public void Credits(bool kitten) // menu creditos
    {
        startPosition = mainMenuHand.position;
        targetPosition = animHandStartingPoint.position;

        isMoving = true;
        StartCoroutine(DelayCredits());
        timer = 0.0f;
    }

    public void ReturnCredits(bool kitten) // sair do menu de creditos
    {
        startPosition = mainMenuHand.position;
        targetPosition = animHandStartingPoint.position;

        isMoving = true;
        StartCoroutine(DelayReturnCredits());
        timer = 0.0f;
    }

    public void ReturnPage(bool kitten) // anim para retorno
    {
        startPosition = mainMenuHand.position;
        targetPosition = animHandStartingPoint.position;

        isMoving = true;
        StartCoroutine(DelayPreviousPage());
        timer = 0.0f;
    }

    //Quando pressiona o play
    public void CloseBookOnPlay() // anim para fechar livro
    {
        StopHandFollowCursor();
        StartMoveCursorObject(mainMenuHand, moveDuration, animHandStartingPoint.position);
        StartCoroutine(DelayCloseBook());

    }

    //Quando retorna ao menu
    public void OpenBookReturnToPlay() // anim para abrir livro
    {
        StopHandFollowCursor();
        StartMoveCursorObject(mainMenuHand, moveDuration, animHandStartingPoint.position);
        StartCoroutine(DelayOpenBook());
    }

    public void ChangeMapSelected(int idChange)
    {
        int nextID;
        if (mapInPreviewID + idChange >= mapPreviewImages.Length) nextID = 0;
        else if (mapInPreviewID + idChange < 0) nextID = mapPreviewImages.Length - 1;
        else nextID = mapInPreviewID + idChange;
        mapInPreviewID = nextID;
        mapPreview.sprite = mapPreviewImages[mapInPreviewID];
    }

    public void BlueprintSelect() // anim para blueprint de seleção
    {
        Blueprint.SetActive(true);
        tanques.SetActive(true);
        clientHostCanvas.gameObject.SetActive(false);
        Luz.SetActive(true);
        Lampada.SetActive(false);
        StartCoroutine(MoveBlue());


        isChange = true;
        isReturn = false;
    }

    public void ChangeToCreateJoinPage(bool isHosting)
    {
        returnToMainMenuFromClientHost.gameObject.SetActive(false);
        selectedHost = isHosting;
        StartCoroutine(ChangeToCreateJoinPageAnimation(clientHostPaper, createJoinPaper, isHosting));
    }
    public void ChangeToHostClientPage()
    {
        returnToMainMenuFromClientHost.gameObject.SetActive(true);
        StartCoroutine(ChangeToCreateJoinPageAnimation(createJoinPaper, clientHostPaper, selectedHost));
        selectedHost = false;
    }

    public void HostOrCreateSession()
    {
        createJoinPaperDefaultGroup.gameObject.SetActive(false);
        createJoinPaperLoadingGroup.gameObject.SetActive(true);
        if (selectedHost) StartHost();
        else StartClient();
    }
    private void StartHost()
    {
        Task task = NetworkRunnerHandler.Instance.StartNetworkRunner(sessionNameInputfield.name, Fusion.GameMode.Host);
        StartCoroutine(WaitForHostToConnectToServer(task));
    }
    public IEnumerator WaitForHostToConnectToServer(Task task)
    {
        while(task.Status != TaskStatus.RanToCompletion)
        {
            Debug.Log(task.Status);
            if (task.Status == TaskStatus.Canceled ||  task.Status == TaskStatus.Faulted)
            {
                Debug.LogError("Error type"+ task.Status);
            }
            yield return null;
        }
        Debug.Log("Server Created");
    }
    private void StartClient()
    {
        NetworkRunnerHandler.Instance.StartNetworkRunner(sessionNameInputfield.name, Fusion.GameMode.Client);
    }
    public void ReturnBlueprintSelect() // sair do blue de seleção
    {
        tanques.SetActive(false);
        Luz.SetActive(false);
        Lampada.SetActive(true);
        clientHostCanvas.gameObject.SetActive(true);
        StartCoroutine(ReturnBlue());

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

    IEnumerator ChangeToCreateJoinPageAnimation(CanvasGroup pageLeaving, CanvasGroup pageEntering, bool isHost)
    {
        pageLeaving.blocksRaycasts = false;
        pageEntering.blocksRaycasts = false;
        if (isHost) mapSelection.blocksRaycasts = false;
        pageEntering.gameObject.SetActive(true);

        Vector3 pageLeavingStartPos = pageLeaving.transform.localPosition;
        Vector3 pageLeavingFinalPos = new Vector3(0, -1000, 0);

        Vector3 pageEnteringStartPos = pageEntering.transform.localPosition;
        Vector3 pageEnteringFinalPos = new Vector3(0, 0, -5);
        float timer = 0f;
        float duration = 0.25f;
        bool isHidingMapSelection = mapSelection.gameObject.activeInHierarchy;
        if (isHost && isHidingMapSelection)
        {
            timer = 0f;
            Vector3 MapSelectStartPos = mapSelection.transform.localPosition;
            Vector3 MapSelectFinalPos = mapSelection.transform.localPosition;
            MapSelectFinalPos.x = -550;

            while (timer < duration)
            {
                mapSelection.transform.localPosition = Vector3.Lerp(MapSelectStartPos, MapSelectFinalPos, timer / duration);
                timer += Time.deltaTime;
                if (Input.GetMouseButton(0)) timer += Time.deltaTime * (animSpeedUpMultiplier - 1);
                yield return null;
            }
            mapSelection.blocksRaycasts = false;
            mapSelection.gameObject.SetActive(false);
        }

        timer = 0f;
        while (timer < duration)
        {
            pageLeaving.transform.localPosition = Vector3.Lerp(pageLeavingStartPos, pageLeavingFinalPos, timer / duration);
            pageEntering.transform.localPosition = Vector3.Lerp(pageEnteringStartPos, pageEnteringFinalPos, timer / duration);
            timer += Time.deltaTime;
            if (Input.GetMouseButton(0)) timer += Time.deltaTime * (animSpeedUpMultiplier - 1);
            yield return null;
        }
        pageLeaving.transform.localPosition = pageLeavingFinalPos;
        pageEntering.transform.localPosition = pageEnteringFinalPos;

        timer = 0f;
        pageLeavingStartPos = pageLeaving.transform.localPosition;
        pageLeavingFinalPos = new Vector3(0, 0, 0);
        while (timer < duration)
        {
            pageLeaving.transform.localPosition = Vector3.Lerp(pageLeavingStartPos, pageLeavingFinalPos, timer / duration);
            timer += Time.deltaTime;
            if (Input.GetMouseButton(0)) timer += Time.deltaTime * (animSpeedUpMultiplier - 1);
            yield return null;
        }
        pageLeaving.transform.localPosition = pageLeaving.transform.localPosition;
        if (!isHidingMapSelection && isHost)
        {
            timer = 0f;
            Vector3 MapSelectStartPos = mapSelection.transform.localPosition;
            Vector3 MapSelectFinalPos = mapSelection.transform.localPosition;
            MapSelectFinalPos.x = -1075;
            mapSelection.gameObject.SetActive(true);

            while (timer < duration)
            {
                mapSelection.transform.localPosition = Vector3.Lerp(MapSelectStartPos, MapSelectFinalPos, timer / duration);
                timer += Time.deltaTime;
                if (Input.GetMouseButton(0)) timer += Time.deltaTime * (animSpeedUpMultiplier - 1);
                yield return null;
            }
            mapSelection.blocksRaycasts = true;
        }
        pageLeaving.blocksRaycasts = true;
        pageEntering.blocksRaycasts = true;
        pageLeaving.gameObject.SetActive(false);
    }

    IEnumerator DelayNextPage()
    {
        yield return new WaitForSeconds(0.25f);
        animHand.Play("arm_AMT|VirarPag");

        yield return new WaitForSeconds(0.5f);
        animFolhaDoCaderno.Play("FolhaVirando");

        yield return new WaitForSeconds(0.06f);
        playCanvasLayer.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.3f);
        logoCanvasLayer.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.10f);
        config[0].SetActive(true);

        yield return new WaitForSeconds(0.08f);
        config[1].SetActive(true);


        StartCoroutine(LockMousePosition(mainMenuHand, moveTime));
    }
    IEnumerator DelayCredits()
    {
        yield return new WaitForSeconds(0.25f);
        animHand.Play("arm_AMT|VirarPag");

        yield return new WaitForSeconds(0.5f);
        animFolhaDoCaderno.Play("FolhaVirando");

        yield return new WaitForSeconds(0.06f);
        config[0].SetActive(false);

        yield return new WaitForSeconds(0.03f);
        config[1].SetActive(false);

        yield return new WaitForSeconds(0.10f);
        credits[1].SetActive(true);

        yield return new WaitForSeconds(0.15f);
        credits[0].SetActive(true);

        StartCoroutine(LockMousePosition(mainMenuHand, moveTime));
    }

    IEnumerator DelayReturnCredits()
    {
        animHand.Play("arm_AMT|VirarPag 0");

        yield return new WaitForSeconds(0.15f);
        animFolhaDoCaderno.Play("FolhaVirando 0 0");

        yield return new WaitForSeconds(0.06f);
        credits[0].SetActive(false);

        yield return new WaitForSeconds(0.05f);
        credits[1].SetActive(false);

        yield return new WaitForSeconds(0.15f);
        config[1].SetActive(true);

        yield return new WaitForSeconds(0.5f);
        config[0].SetActive(true);

        StartCoroutine(LockMousePosition(mainMenuHand, moveTime));
    }

    IEnumerator DelayPreviousPage()
    {
        animHand.Play("arm_AMT|VirarPag 0");

        yield return new WaitForSeconds(0.15f);
        animFolhaDoCaderno.Play("FolhaVirando 0");

        yield return new WaitForSeconds(0.06f);
        config[0].SetActive(false);

        yield return new WaitForSeconds(0.05f);
        config[1].SetActive(false);

        yield return new WaitForSeconds(0.15f);
        logoCanvasLayer.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        playCanvasLayer.gameObject.SetActive(true);

        StartCoroutine(LockMousePosition(mainMenuHand, moveTime));
    }

    IEnumerator DelayCloseBook()
    {
        StartAccelerateAnimatorSpeedByMouseInput(animHand, animSpeedUpMultiplier);
        StartAccelerateAnimatorSpeedByMouseInput(animBook, animSpeedUpMultiplier);

        animHand.Play("VirarPaginaDireita");
        float timerDelay = 0;
        while (timerDelay < 0.15f)
        {
            timerDelay += Time.deltaTime * animHand.speed;
            yield return null;
        }
        clientHostPaper.gameObject.SetActive(false);
        returnToMainMenuFromClientHost.gameObject.SetActive(false);
        clientHostCanvas.blocksRaycasts = false;
        clientHostCanvas.gameObject.SetActive(true);
        animBook.Play("CadernoFechando");
        timerDelay = 0;
        while (timerDelay < 0.25f)
        {
            timerDelay += Time.deltaTime * animBook.speed;
            yield return null;
        }
        clientHostPaper.gameObject.SetActive(true);
        folhaDoCaderno.gameObject.SetActive(false);
        logoCanvasLayer.gameObject.SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.25f)
        {
            timerDelay += Time.deltaTime * animBook.speed;
            yield return null;
        }
        playCanvasLayer.gameObject.SetActive(false);

        timerDelay = 0;
        while (timerDelay < 0.25f)
        {
            timerDelay += Time.deltaTime * animBook.speed;
            yield return null;
        }
        returnToMainMenuFromClientHost.gameObject.SetActive(true);
        clientHostCanvas.blocksRaycasts = true;
        StopAccelerateAnimatorSpeedByMouseInput(animHand);
        StopAccelerateAnimatorSpeedByMouseInput(animBook);
        StartHandFollowCursor();
    }

    IEnumerator DelayOpenBook()
    {

        StartAccelerateAnimatorSpeedByMouseInput(animHand, animSpeedUpMultiplier);
        StartAccelerateAnimatorSpeedByMouseInput(animBook, animSpeedUpMultiplier);

        animHand.Play("VirarPaginaEsquerda");
        float timerDelay = 0;
        while (timerDelay < 0.35f)
        {
            timerDelay += Time.deltaTime * animHand.speed;
            yield return null;
        }
        animBook.Play("AbrirCaderno");
        returnToMainMenuFromClientHost.gameObject.SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.25f)
        {
            timerDelay += Time.deltaTime * animBook.speed;
            yield return null;
        }
        playCanvasLayer.blocksRaycasts = false;
        logoCanvasLayer.blocksRaycasts = false;
        playCanvasLayer.gameObject.SetActive(true);
        timerDelay = 0;
        while (timerDelay < 0.25f)
        {
            timerDelay += Time.deltaTime * animBook.speed;
            yield return null;
        }
        clientHostPaper.gameObject.SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.25f)
        {
            timerDelay += Time.deltaTime * animBook.speed;
            yield return null;
        }
        folhaDoCaderno.gameObject.SetActive(true);
        logoCanvasLayer.gameObject.SetActive(true);
        playCanvasLayer.blocksRaycasts = true;
        logoCanvasLayer.blocksRaycasts = true;

        clientHostCanvas.gameObject.SetActive(false);
        returnToMainMenuFromClientHost.gameObject.SetActive(true);
        clientHostPaper.gameObject.SetActive(true);

        StopAccelerateAnimatorSpeedByMouseInput(animHand);
        StopAccelerateAnimatorSpeedByMouseInput(animBook);
        StartHandFollowCursor();
    }

    IEnumerator MoveBookSmoothly()
    {
        Vector3 pontoOrigem = book.position;
        float duration = 0.75f; // Tempo total da transição em segundos

        Vector3 pontoAtual;
        for (float t = 0; t < duration;)
        {
            pontoAtual = Vector3.Lerp(pontoOrigem, bookOnTableTransform.position, t / duration);
            // Movimenta o objeto para o ponto atual
            book.position = pontoAtual;

            t += Time.deltaTime;
            //Se o jogador estiver apertar o botao esquerdo do mouse ele aumentara a velocidade da animacao de queda do livro
            if (Input.GetMouseButton(0))
            {
                t += Time.deltaTime * (animSpeedUpMultiplier - 1);
            }
            yield return null; // Aguarda até o próximo quadro
        }
        book.position = bookOnTableTransform.position;
        //Cria uma corotina q checa se o mouse esta pressionado caso tal esteja aumenta a velocidade do animator
        StartAccelerateAnimatorSpeedByMouseInput(animBook, animSpeedUpMultiplier);
        StartAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno, animSpeedUpMultiplier);
        //Animacao de abrir o caderno 
        animBook.Play("AbrirCaderno");
        folhaDoCaderno.gameObject.SetActive(true);
        float timerDelay = 0;
        while (timerDelay < 0.5f)
        {
            timerDelay += Time.deltaTime * animBook.speed;
            yield return null;
        }
        timerDelay = 0;
        //Animacao da pagina do caderno virando 
        animFolhaDoCaderno.Play("FolhaVirando");
        while (timerDelay < 0.45f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        timerDelay = 0;
        //Disabilita a interacao com o menu durante a aparicao do menu
        playCanvasLayer.blocksRaycasts = false;
        logoCanvasLayer.blocksRaycasts = false;

        playCanvasLayer.gameObject.SetActive(true);
        while (timerDelay < 0.20f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        logoCanvasLayer.gameObject.SetActive(true);
        StopAccelerateAnimatorSpeedByMouseInput(animBook);
        StopAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno);

        StartHandFollowCursor();
        //Habilita a interacao com o menu apos a aparicao do menu
        playCanvasLayer.blocksRaycasts = true;
        logoCanvasLayer.blocksRaycasts = true;
    }
    public void StartAccelerateAnimatorSpeedByMouseInput(Animator animator, float speedMultiplier)
    {

        if (AccelerateAnimatorSpeedByMouseCoroutineDictionary.ContainsKey(animator)) StopAccelerateAnimatorSpeedByMouseInput(animator);
        //Se caso ele nao achar uma corotina para o animator
        Coroutine coroutine = StartCoroutine(AccelerateAnimatorSpeedByMouseInput(animator, speedMultiplier, animator.speed));
        //cria um struct para armazenas tanta a corotina quanto o valor de velocidade inicial para caso a corotina seja parada ele retorne ela a seu valor normal
        AnimatorAcceleratorCoroutine AACStruct = new AnimatorAcceleratorCoroutine()
        {
            coroutine = coroutine,
            originalSpeed = animator.speed
        };
        AccelerateAnimatorSpeedByMouseCoroutineDictionary.Add(animator, AACStruct);

    }
    public void StopAccelerateAnimatorSpeedByMouseInput(Animator animator)
    {
        if (AccelerateAnimatorSpeedByMouseCoroutineDictionary.TryGetValue(animator, out AnimatorAcceleratorCoroutine AACStruct))
        {
            StopCoroutine(AACStruct.coroutine);
            AACStruct.coroutine = null;
            animator.speed = 1;
            AccelerateAnimatorSpeedByMouseCoroutineDictionary.Remove(animator);
        }
        else Debug.Log("Error in finding dictionary of" + animator + " to stop");
    }
    [HideInInspector] private Dictionary<Animator, AnimatorAcceleratorCoroutine> AccelerateAnimatorSpeedByMouseCoroutineDictionary = new Dictionary<Animator, AnimatorAcceleratorCoroutine>();
    public IEnumerator AccelerateAnimatorSpeedByMouseInput(Animator animator, float speedMultiplier, float defaultAnimatorSpeed)
    {
        while (true)
        {
            if (Input.GetMouseButton(0))
            {
                animator.speed = defaultAnimatorSpeed * speedMultiplier;
            }
            else
            {
                animator.speed = defaultAnimatorSpeed;
            }
            yield return null;
        }
    }

    IEnumerator MoveBlue()
    {

        Vector3 posicaoCentroCanvas = new Vector3(0f, blueprintRect.localPosition.y, blueprintRect.localPosition.z);

        while (Vector2.Distance(blueprintRect.localPosition, new Vector2(posicaoCentroCanvas.x, posicaoCentroCanvas.y)) > 0.1f)
        {
            // Move suavemente o objeto em direção ao centro do canvas usando a interpolação linear
            blueprintRect.localPosition = Vector3.Lerp(blueprintRect.localPosition, posicaoCentroCanvas, Time.deltaTime * 10);

            yield return null;
        }

        blueprintRect.localPosition = posicaoCentroCanvas;
    }

    IEnumerator ReturnBlue()
    {
        Vector3 posicaoCentroCanvas = new Vector3(1656f, blueprintRect.localPosition.y, blueprintRect.localPosition.z);

        while (Vector2.Distance(blueprintRect.localPosition, new Vector2(posicaoCentroCanvas.x, posicaoCentroCanvas.y)) > 0.1f)
        {
            // Move suavemente o objeto em direção ao centro do canvas usando a interpolação linear
            blueprintRect.localPosition = Vector3.Lerp(blueprintRect.localPosition, posicaoCentroCanvas, Time.deltaTime * 10);

            yield return null;
        }

        blueprintRect.localPosition = posicaoCentroCanvas;
    }

    IEnumerator LockMousePosition(Transform armPosition, float duration) // retornar o mouse suavimente para a posição
    {
        float timer = 0;
        Vector2 armScreenPosition;
        while (timer < duration)
        {
            armScreenPosition = mainCamera.WorldToScreenPoint(armPosition.position);
            Mouse.current.WarpCursorPosition(armScreenPosition);
            timer += Time.deltaTime;
            if (Input.GetMouseButton(0)) timer += Time.deltaTime * (animSpeedUpMultiplier - 1);
            yield return null;
        }
    }
}
public struct AnimatorAcceleratorCoroutine
{
    public Coroutine coroutine;
    public float originalSpeed;
}