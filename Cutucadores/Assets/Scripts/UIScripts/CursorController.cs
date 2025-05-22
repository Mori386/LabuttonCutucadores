using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CharacterData;

public class CursorController : MonoBehaviour
{
    public static CursorController Instance;
    [HideInInspector] public Camera mainCamera;

    readonly private float animSpeedUpMultiplier = 2f; // Valor de multiplicacao de velocidade quando o jogador acelerar as animacoes
    [Header("|----- Main Menu -----|")]
    [Header("Mão")]
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
    public GameObject carimbo;

    [Header("|----- Host Client Menu-----|")]
    [Header("Host and client page")]
    public TMP_InputField nicknameInputField;
    public CanvasGroup clientHostCanvas; // continuar daki mexer ele 
    public CanvasGroup clientHostPaper;
    public GameObject insertPasswordPaper;
    public RectTransform returnToMainMenuFromClientHost;
    [Header("Create/Join page")]
    private bool selectedHost;
    public TMP_InputField sessionNameInputfield;
    public TMP_InputField passwordInputField;
    public TMP_InputField enterRoomPasswordInputField;
    public TextMeshProUGUI placeholderPasswordText;
    public Toggle isRoomPrivateToggle;
    public TextMeshProUGUI placeholderSessionNameText;
    public CanvasGroup mapSelection;
    public Image mapPreview;
    public Sprite[] mapPreviewImages;
    private int mapInPreviewID = 0;
    public CanvasGroup createJoinPaper;
    public CanvasGroup createJoinPaperDefaultGroup;
    public CanvasGroup createJoinPaperLoadingGroup;
    public TextMeshProUGUI createJoinPaperLoadingText;
    public GameObject roomListParent;
    public GameObject roomPrefab;
    public TextMeshProUGUI lobbyStatusText;

    [Header("|----- Blueprint -----|")]
    public Button hostStartGameButton;
    public BPCharacter escavadorCharBP;
    [Space] public BPCharacter mineradorCharBP;
    [Space] public BPCharacter paiEFilhaCharBP;
    [Space] public BPCharacter vovoCharBP;

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

    [Header("VETORES")]
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    void Start()
    {
        mainCamera = Camera.main;

        if (PlayerPrefs.GetString("nickname") != null && PlayerPrefs.GetString("nickname") != "")
            nicknameInputField.text = PlayerPrefs.GetString("nickname");

        Cursor.visible = false; // Esconde o cursor do mouse
        Cursor.lockState = CursorLockMode.Confined; // Mantém o cursor dentro da janela do jogo.

        startPosition = mainMenuHand.transform.position; //para retorno da posição inicial
        moveDuration = moveTime - pauseTime; // valor do tempo de deslocamento
        StartCoroutine(MoveBookSmoothly());
        isRoomPrivateToggle.onValueChanged.AddListener(passwordInputField.gameObject.transform.parent.gameObject.SetActive);
    }
    public void StartHandFollowCursor()
    {
        if (CalculateMousePosInWorldCoroutine == null) CalculateMousePosInWorldCoroutine = StartCoroutine(CalculateMousePosInWorld(2.8f));
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
    public IEnumerator CalculateMousePosInWorld(float distance)
    {
        Vector3 mousePosition;
        while (true)
        {
            mousePosition = Input.mousePosition;
            mousePosition.z = distance;
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

    public void StartStampFollowCursor()
    {
        if (CalculateMousePosInWorldCoroutine == null) CalculateMousePosInWorldCoroutine = StartCoroutine(CalculateMousePosInWorld(1.3f));
        if (StampFollowCursorCoroutine == null) StampFollowCursorCoroutine = StartCoroutine(StampFollowCursor());
    }
    public void StopStampFollowCursor()
    {
        if (CalculateMousePosInWorldCoroutine != null)
        {
            StopCoroutine(CalculateMousePosInWorldCoroutine);
            CalculateMousePosInWorldCoroutine = null;
        }
        if (StampFollowCursorCoroutine != null)
        {
            StopCoroutine(StampFollowCursorCoroutine);
            StampFollowCursorCoroutine = null;
        }
    }
    public Coroutine StampFollowCursorCoroutine;
    public IEnumerator StampFollowCursor()
    {
        Vector3 newMousePos;
        float carimboYPos = carimbo.transform.position.y;
        while (true)
        {
            if (!IsMouseOutOffApp())
            {
                newMousePos = Vector3.Lerp(carimbo.transform.position, mousePosInWorld, 50f / 10f * Time.fixedDeltaTime);
                carimbo.transform.position = new Vector3(newMousePos.x, carimboYPos, newMousePos.z);
            }
            yield return new WaitForFixedUpdate();
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
    public void NextPageGoToConfigs()  // passar pag
    {
        // Inicializa a posição de início e destino
        StopHandFollowCursor();
        StartMoveCursorObject(mainMenuHand, moveDuration, animHandStartingPoint.position);
        StartCoroutine(DelayNextPage());
    }

    public void Credits(bool kitten) // menu creditos
    {
        StopHandFollowCursor();
        StartMoveCursorObject(mainMenuHand, moveDuration, animHandStartingPoint.position);
        StartCoroutine(DelayCredits());
    }
    public void Controls()
    {
        StopHandFollowCursor();
        StartMoveCursorObject(mainMenuHand, moveDuration, animHandStartingPoint.position);
        StartCoroutine(DelayCredits());
    }
    public void ReturnCredits(bool kitten) // sair do menu de creditos
    {
        StopHandFollowCursor();
        StartMoveCursorObject(mainMenuHand, moveDuration, animHandStartingPoint.position);
        StartCoroutine(DelayReturnCredits());
    }

    //Return to main menu
    public void ReturnPage(bool kitten) // anim para retorno
    {
        StopHandFollowCursor();
        StartMoveCursorObject(mainMenuHand, moveDuration, animHandStartingPoint.position);
        StartCoroutine(DelayPreviousPage());
    }

    //Quando pressiona o play
    public void CloseBookOnPlay() // anim para fechar livro
    {
        StopHandFollowCursor();
        StartMoveCursorObject(mainMenuHand, moveDuration, animHandStartingPoint.position);
        StartCoroutine(DelayCloseBook());
        NetworkRunnerHandler.Instance.StartLobby();
        foreach (Transform child in roomListParent.transform)
        {
            if (child.GetComponent<LobbyRoomPrefab>() != null)
                return;
        }
        lobbyStatusText.gameObject.SetActive(true);
        lobbyStatusText.text = "Procurando salas...";
    }

    public void ReloadRoomList(List<SessionInfo> sessionList)
    {
        Debug.Log($"Reloading room list");
        if (roomListParent == null)
            return;
        foreach (Transform child in roomListParent.transform)
        {
            if (child.GetComponent<LobbyRoomPrefab>() != null)
                Destroy(child.gameObject);
        }
        lobbyStatusText.gameObject.SetActive(true);
        lobbyStatusText.text = "Nenhuma sala ativa no momento.";
        if (sessionList != null || sessionList.Count > 0)
        {
            foreach (SessionInfo session in sessionList)
            {
                if (session.Properties[joinable] == 1)
                {
                    lobbyStatusText.gameObject.SetActive(false);
                    break;
                }
            }
        }
        foreach (SessionInfo session in sessionList)
        {
            if (session.Properties[joinable] == 1)
            {
                Debug.Log($"Instantiating room {session.Name}");
                LobbyRoomPrefab prefab = Instantiate(roomPrefab, roomListParent.transform).GetComponent<LobbyRoomPrefab>();
                prefab.Setup(session.Name, session.PlayerCount, new Dictionary<string, SessionProperty>(session.Properties));
            }
        }
    }

    string currentRoomPassword;
    string currentRoomName;
    public void ToInsertPasswordScreen(string password, string name)
    {
        clientHostPaper.gameObject.SetActive(false);
        insertPasswordPaper.SetActive(true);
        enterRoomPasswordInputField.placeholder.color = Color.black;
        //enterRoomPasswordInputField.placeholder.GetComponent<Text>().text = "Insira a senha da partida...";
        currentRoomPassword = password;
        currentRoomName = name;
    }

    public void BackFromInsertPasswordScreen()
    {
        clientHostPaper.gameObject.SetActive(true);
        insertPasswordPaper.SetActive(false);
        enterRoomPasswordInputField.placeholder.color = Color.black;
        //enterRoomPasswordInputField.placeholder.GetComponent<Text>().text = "Insira a senha da partida...";
        currentRoomPassword = null;
        currentRoomName = null;
    }

    public void TryPassword()
    {
        if (enterRoomPasswordInputField.text != currentRoomPassword)
        {
            enterRoomPasswordInputField.text = "";
            enterRoomPasswordInputField.placeholder.color = Color.red;
            //enterRoomPasswordInputField.placeholder.GetComponent<Text>().text = "Senha incorreta";
            return;
        }
        enterRoomPasswordInputField.placeholder.color = Color.black;
        //enterRoomPasswordInputField.placeholder.GetComponent<Text>().text = "Insira a senha da partida...";
        Dictionary<string, SessionProperty> properties = new()
        {
            { isRoomPrivate, 1 },
            { password, currentRoomPassword },
            { joinable, 1 }
        };
        StartClient(currentRoomName, properties);
        currentRoomPassword = null;
        currentRoomName = null;
    }

    //Quando retorna ao menu
    public void OpenBookReturnToPlay() // anim para abrir livro
    {
        if (createJoinPaperLoadingGroup.gameObject.activeInHierarchy)
            return;
        NetworkRunnerHandler.Instance.ShutdownNetworkRunner();
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

    public void BlueprintLoadInfos()
    {
        for (int i = 0; i < NetworkBetweenScenesManager.Instance.userIDList.Count; i++)
        {
            if (NetworkBetweenScenesManager.Instance.userIDToPlayerData.TryGet(NetworkBetweenScenesManager.Instance.userIDList.Get(i), out PlayerData playerData))
            {
                if (playerData.character != Character.Null)
                {
                    BPCharacter characterBpInfos;
                    switch (playerData.character)
                    {
                        default:
                        case Character.Escavador:
                            characterBpInfos = escavadorCharBP;
                            break;
                        case Character.Minerador:
                            characterBpInfos = mineradorCharBP;
                            break;
                        case Character.PaiEFilha:
                            characterBpInfos = paiEFilhaCharBP;
                            break;
                        case Character.Vovo:
                            characterBpInfos = vovoCharBP;
                            break;
                    }
                    characterBpInfos.selectButton.interactable = false;
                    characterBpInfos.drillBodyMeshRenderer.material = characterBpInfos.defaultMaterial;
                    characterBpInfos.drillHeadMeshRenderer.material = characterBpInfos.defaultMaterial;
                    characterBpInfos.usernameText.text = playerData.username.ToString();
                }
            }
        }
    }
    public void SelectCharacter(Character charSelected)
    {
        NetworkBetweenScenesManager.Instance.RPC_LockCharacter(NetworkBetweenScenesManager.Instance.selfUserID, charSelected);
        escavadorCharBP.selectButton.interactable = false;
        mineradorCharBP.selectButton.interactable = false;
        paiEFilhaCharBP.selectButton.interactable = false;
        vovoCharBP.selectButton.interactable = false;
    }
    public void BlueprintEnter() // anim para blueprint de seleção
    {
        StopHandFollowCursor();
        BlueprintLoadInfos();
        Blueprint.SetActive(true);
        StartCoroutine(MoveBlue());
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
        if (sessionNameBlankCoroutine != null)
        {
            StopCoroutine(sessionNameBlankCoroutine);
            placeholderSessionNameText.color = Color.black;
        }
        if (passwordBlankCoroutine != null)
        {
            StopCoroutine(passwordBlankCoroutine);
            placeholderPasswordText.color = Color.black;
        }
        if (sessionNameInputfield.text == "" || sessionNameInputfield.text == null)
        {
            sessionNameBlankCoroutine = StartCoroutine(SessionNameBlankFeedback(placeholderSessionNameText));
            return;
        }
        if (isRoomPrivateToggle.isOn && (passwordInputField.text == "" || passwordInputField.text == null))
        {
            passwordBlankCoroutine = StartCoroutine(SessionNameBlankFeedback(placeholderPasswordText));
            return;
        }
        //createJoinPaperDefaultGroup.gameObject.SetActive(false);
        createJoinPaperLoadingGroup.gameObject.SetActive(true);
        StartHost();
    }
    private Coroutine sessionNameBlankCoroutine;
    private Coroutine passwordBlankCoroutine;
    private IEnumerator SessionNameBlankFeedback(TextMeshProUGUI text)
    {
        for (int i = 0; i <= 3; i++)
        {
            text.color = Color.red;
            yield return new WaitForSeconds(.3f);
            text.color = Color.black;
            yield return new WaitForSeconds(.3f);
        }
    }
    private const string isRoomPrivate = "Private"; // 1 = true; 0 = false;
    private const string password = "Password";
    private const string joinable = "Joinable";
    private void StartHost()
    {
        Dictionary<string, SessionProperty> properties = new()
        {
            { isRoomPrivate, isRoomPrivateToggle.isOn ? 1 : 0 },
            { password, passwordInputField.text },
            { joinable, 0 }
        };
        Task task = NetworkRunnerHandler.Instance.StartNetworkRunner(sessionNameInputfield.text, Fusion.GameMode.Host, properties);
        StartCoroutine(WaitForHostToConnectToServer(task));
    }
    public void StartClient(string sessionName, Dictionary<string, SessionProperty> sessionProperties)
    {
        createJoinPaperLoadingGroup.gameObject.SetActive(true);
        Task task = NetworkRunnerHandler.Instance.StartNetworkRunner(sessionName, Fusion.GameMode.Client, sessionProperties);
        StartCoroutine(WaitForHostToConnectToServer(task));
    }
    public IEnumerator WaitForHostToConnectToServer(Task task)
    {
        float timeoutCount = 0;
        createJoinPaperLoadingText.text = "Conectando...";
        while (task.Status != TaskStatus.RanToCompletion)
        {
            Debug.Log($"Task status: {task.Status}, time elapsed: {timeoutCount} seconds.");
            if (task.Status == TaskStatus.Canceled || task.Status == TaskStatus.Faulted || timeoutCount > 25f)
            {
                createJoinPaperLoadingText.text = "Erro ao conectar.";
                Task shutdownTask = NetworkRunnerHandler.Instance.ShutdownNetworkRunner();
                foreach (Transform child in roomListParent.transform)
                {
                    if (child.GetComponent<LobbyRoomPrefab>() != null)
                        Destroy(child.gameObject);
                }
                while (shutdownTask.Status != TaskStatus.RanToCompletion)
                    yield return null;
                yield return new WaitForSeconds(1);
                NetworkRunnerHandler.Instance.StartLobby();
                clientHostPaper.gameObject.SetActive(true);
                lobbyStatusText.gameObject.SetActive(true);
                lobbyStatusText.text = "Procurando salas...";
                createJoinPaperLoadingGroup.gameObject.SetActive(false);
                createJoinPaper.gameObject.SetActive(false);
                insertPasswordPaper.SetActive(false);
                yield break;
            }
            yield return null;
            timeoutCount += Time.deltaTime;
        }
        while (carimbo == null)
        {
            Debug.Log($"Loading carimbo, time elapsed: {timeoutCount} seconds.");
            if (timeoutCount > 25f)
            {
                createJoinPaperLoadingText.text = "Erro ao conectar.";
                Task shutdownTask = NetworkRunnerHandler.Instance.ShutdownNetworkRunner();
                foreach (Transform child in roomListParent.transform)
                {
                    if (child.GetComponent<LobbyRoomPrefab>() != null)
                        Destroy(child.gameObject);
                }
                while (shutdownTask.Status != TaskStatus.RanToCompletion)
                    yield return null;
                yield return new WaitForSeconds(1);
                NetworkRunnerHandler.Instance.StartLobby();
                clientHostPaper.gameObject.SetActive(true);
                lobbyStatusText.gameObject.SetActive(true);
                lobbyStatusText.text = "Procurando salas...";
                createJoinPaperLoadingGroup.gameObject.SetActive(false);
                createJoinPaper.gameObject.SetActive(false);
                insertPasswordPaper.SetActive(false);
                yield break;
            }
            yield return null;
            timeoutCount += Time.deltaTime;
        }
        Debug.Log("Carimbo loaded.");
        while (NetworkBetweenScenesManager.Instance.spawned == false)
        {
            Debug.Log($"Loading NetworkBetweenScenesManager, time elapsed: {timeoutCount} seconds.");
            if (timeoutCount > 25f)
            {
                createJoinPaperLoadingText.text = "Erro ao conectar.";
                Task shutdownTask = NetworkRunnerHandler.Instance.ShutdownNetworkRunner();
                foreach (Transform child in roomListParent.transform)
                {
                    if (child.GetComponent<LobbyRoomPrefab>() != null)
                        Destroy(child.gameObject);
                }
                while (shutdownTask.Status != TaskStatus.RanToCompletion)
                    yield return null;
                yield return new WaitForSeconds(1);
                NetworkRunnerHandler.Instance.StartLobby();
                clientHostPaper.gameObject.SetActive(true);
                lobbyStatusText.gameObject.SetActive(true);
                lobbyStatusText.text = "Procurando salas...";
                createJoinPaperLoadingGroup.gameObject.SetActive(false);
                createJoinPaper.gameObject.SetActive(false);
                insertPasswordPaper.SetActive(false);
                yield break;
            }
            yield return null;
            timeoutCount += Time.deltaTime;
        }
        Debug.Log("NetworkBetweenScenesManager carregado");
        createJoinPaperLoadingText.text = "Conectado";
        yield return new WaitForSeconds(1);
        NetworkRunnerHandler.Instance.ManageRoomVisibility(1);
        BlueprintEnter();
    }
    public void StartMatch()
    {
        switch(mapInPreviewID)
        {
            case 0:
                NetworkBetweenScenesManager.Instance.LoadMapToHost("Level1",1);
                break;
            case 1:
                NetworkBetweenScenesManager.Instance.LoadMapToHost("Level2",2);
                break;
            case 2:
                NetworkBetweenScenesManager.Instance.LoadMapToHost("Level3",3);
                break;
        }
    }
    public void ReturnBlueprintSelect() // sair do blue de seleção
    {
        NetworkRunnerHandler.Instance.ShutdownNetworkRunner();
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
                SelectCharacter(Character.Escavador);
                break;

            case 1:
                SelectCharacter(Character.Minerador);
                //startPosition = carimbo.transform.position;
                //targetPosition = Okays[1].transform.position;
                //isMoving = true;
                //timer = 0.4f;
                //SetActiveWithDelay(Okays[1], true, 0.8f);
                break;

            case 2:
                SelectCharacter(Character.PaiEFilha);
                //startPosition = carimbo.transform.position;
                //targetPosition = Okays[2].transform.position;
                //isMoving = true;
                //timer = 0.4f;
                //SetActiveWithDelay(Okays[2], true, 0.8f);
                break;

            case 3:
                SelectCharacter(Character.Vovo);
                //startPosition = carimbo.transform.position;
                //targetPosition = Okays[3].transform.position;
                //Polaroids[4].SetActive(false);
                //isMoving = true;
                //timer = 0.4f;
                //SetActiveWithDelay(Okays[3], true, 0.8f);
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
            mapSelection.transform.localPosition = MapSelectFinalPos;
            mapSelection.blocksRaycasts = true;
        }
        pageLeaving.blocksRaycasts = true;
        pageEntering.blocksRaycasts = true;
        pageLeaving.gameObject.SetActive(false);
    }

    //Next page animation
    IEnumerator DelayNextPage()
    {
        StartAccelerateAnimatorSpeedByMouseInput(animHand, animSpeedUpMultiplier);
        StartAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno, animSpeedUpMultiplier);
        float timerDelay = 0;
        while (timerDelay < 0.25f)
        {
            timerDelay += Time.deltaTime * animHand.speed;
            yield return null;
        }
        animHand.Play("VirarPaginaEsquerda");
        timerDelay = 0;
        while (timerDelay < 0.5f)
        {
            timerDelay += Time.deltaTime * animHand.speed;
            yield return null;
        }
        animFolhaDoCaderno.Play("FolhaVirando");
        timerDelay = 0;
        while (timerDelay < 0.06f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        playCanvasLayer.gameObject.SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.3f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        logoCanvasLayer.gameObject.SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.1f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        config[0].SetActive(true);
        timerDelay = 0;
        while (timerDelay < 0.08f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        config[1].SetActive(true);
        StopAccelerateAnimatorSpeedByMouseInput(animHand);
        StopAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno);
        StartHandFollowCursor();
    }
    IEnumerator DelayCredits()
    {
        StartAccelerateAnimatorSpeedByMouseInput(animHand, animSpeedUpMultiplier);
        StartAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno, animSpeedUpMultiplier);
        float timerDelay = 0;
        while (timerDelay < 0.25f)
        {
            timerDelay += Time.deltaTime * animHand.speed;
            yield return null;
        }
        animHand.Play("VirarPaginaEsquerda");
        timerDelay = 0;
        while (timerDelay < 0.5f)
        {
            timerDelay += Time.deltaTime * animHand.speed;
            yield return null;
        }
        animFolhaDoCaderno.Play("FolhaVirando");
        timerDelay = 0;
        while (timerDelay < 0.06f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        config[0].SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.03f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        config[1].SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.1f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        credits[1].SetActive(true);
        timerDelay = 0;
        while (timerDelay < 0.15f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        credits[0].SetActive(true);

        StopAccelerateAnimatorSpeedByMouseInput(animHand);
        StopAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno);
        StartHandFollowCursor();
    }

    IEnumerator DelayReturnCredits()
    {
        StartAccelerateAnimatorSpeedByMouseInput(animHand, animSpeedUpMultiplier);
        StartAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno, animSpeedUpMultiplier);
        animHand.Play("VirarPaginaDireita");
        float timerDelay = 0;
        while (timerDelay < 0.15f)
        {
            timerDelay += Time.deltaTime * animHand.speed;
            yield return null;
        }
        animFolhaDoCaderno.Play("FolhaVirandoInverse");
        timerDelay = 0;
        while (timerDelay < 0.06f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        credits[0].SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.05f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        credits[1].SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.15f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        config[1].SetActive(true);
        timerDelay = 0;
        while (timerDelay < 0.5f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        config[0].SetActive(true);
        StopAccelerateAnimatorSpeedByMouseInput(animHand);
        StopAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno);
        StartHandFollowCursor();
    }

    IEnumerator DelayPreviousPage()
    {
        StartAccelerateAnimatorSpeedByMouseInput(animHand, animSpeedUpMultiplier);
        StartAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno, animSpeedUpMultiplier);
        animHand.Play("VirarPaginaDireita");
        float timerDelay = 0;
        while (timerDelay < 0.15f)
        {
            timerDelay += Time.deltaTime * animHand.speed;
            yield return null;
        }
        animFolhaDoCaderno.Play("FolhaVirandoInverse");
        timerDelay = 0;
        while (timerDelay < 0.06f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        config[0].SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.05f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        config[1].SetActive(false);
        timerDelay = 0;
        while (timerDelay < 0.15f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        logoCanvasLayer.gameObject.SetActive(true);
        timerDelay = 0;
        while (timerDelay < 0.5f)
        {
            timerDelay += Time.deltaTime * animFolhaDoCaderno.speed;
            yield return null;
        }
        playCanvasLayer.gameObject.SetActive(true);
        StopAccelerateAnimatorSpeedByMouseInput(animHand);
        StopAccelerateAnimatorSpeedByMouseInput(animFolhaDoCaderno);
        StartHandFollowCursor();
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
        Vector3 bpCenterPos = new Vector3(0f, blueprintRect.localPosition.y, blueprintRect.localPosition.z);
        float timerDelay = 0f;
        float duration = 0.5f;
        Vector3 bpStartPos = Blueprint.transform.localPosition;
        while (timerDelay < duration)
        {
            blueprintRect.localPosition = Vector3.Lerp(bpStartPos, bpCenterPos, timerDelay / duration);
            timerDelay += Time.deltaTime;
            if (Input.GetMouseButton(0)) timerDelay += Time.deltaTime * (animSpeedUpMultiplier - 1);
            yield return null;
        }
        blueprintRect.localPosition = bpCenterPos;
        carimbo.SetActive(true);
        //StartStampFollowCursor();
        Cursor.visible = true;
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
[Serializable]
public struct BPCharacter
{
    [Header("Drill")]
    public SkinnedMeshRenderer drillBodyMeshRenderer;
    public MeshRenderer drillHeadMeshRenderer;
    public RotateObject rotateObjectScript;
    public ParticleSystem onMatChangeParticle;
    [Header("Characters")]
    public Animator[] characterAnimator;
    [Header("Materials")]
    public Material defaultMaterial;
    public Material BpEffectMaterial;
    [Header("Ui Elements")]
    public Button selectButton;
    public Image OkayImage;
    public TextMeshProUGUI usernameText;
}