using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistantPlayersIndicator : MonoBehaviour
{
    public static DistantPlayersIndicator Instance { get; private set; }

    //[SerializeField] private List<Transform> playersTransforms = new();
    [SerializeField, SerializedDictionary("Character", "Indicator")] private SerializedDictionary<CharacterData.Character, RectTransform> indicators = new();
    [SerializeField, SerializedDictionary("Character", "Transform")] private SerializedDictionary<CharacterData.Character, Transform> playersTransforms = new();
    [SerializeField] private RectTransform canvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
    }

    public void SetupIndicator(CharacterData.Character character, Transform playerTransform)
    {
        playersTransforms.Add(character, playerTransform);
    }

    public void StartIndicators()
    {
        StartCoroutine(UpdateIndicators());
    }

    public void StopIndicators()
    {
        StopAllCoroutines();
        foreach (KeyValuePair<CharacterData.Character, RectTransform> kvp in indicators)
        {
            kvp.Value.gameObject.SetActive(false);
        }
        playersTransforms.Clear();
    }

    private IEnumerator UpdateIndicators()
    {
        while (true)
        {
            foreach (KeyValuePair<CharacterData.Character, Transform> kvp in playersTransforms)
            {
                indicators[kvp.Key].gameObject.SetActive(!CheckIfVisible(Camera.main, kvp.Value.position));
                if (!CheckIfVisible(Camera.main, kvp.Value.position))
                {
                    Vector3 viewport = Camera.main.WorldToViewportPoint(kvp.Value.position);
                    Vector2 fromCenter = new(viewport.x - 0.5f, viewport.y - 0.5f);

                    float scale = .5f / Mathf.Max(Mathf.Abs(fromCenter.x), Mathf.Abs(fromCenter.y)); //testar se funciona

                    Vector2 edgePoint = fromCenter * scale;

                    Vector2 canvasSize = canvas.rect.size;
                    Vector2 pos = new(edgePoint.x * canvasSize.x, edgePoint.y * canvasSize.y);
                    pos.x = Mathf.Clamp(pos.x, -canvasSize.x / 2 + indicators[kvp.Key].sizeDelta.x / 2, canvasSize.x / 2 - indicators[kvp.Key].sizeDelta.x / 2);
                    pos.y = Mathf.Clamp(pos.y, -canvasSize.y / 2 + indicators[kvp.Key].sizeDelta.x / 2, canvasSize.y / 2 - indicators[kvp.Key].sizeDelta.x / 2);

                    indicators[kvp.Key].anchoredPosition = pos;
                    indicators[kvp.Key].eulerAngles = new(0, 0, Mathf.Rad2Deg * Mathf.Atan2(fromCenter.y, fromCenter.x));
                    indicators[kvp.Key].GetChild(0).localEulerAngles = new(0, 0, Mathf.Rad2Deg * Mathf.Atan2(fromCenter.y, fromCenter.x) * -1);
                }
            }
            yield return null;
        }
    }

    private bool CheckIfVisible(Camera camera, Vector3 worldPosition)
    {
        Vector3 viewportPos = camera.WorldToViewportPoint(worldPosition);
        return viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1 && viewportPos.z >= 0;
    }
}
