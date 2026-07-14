using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum ScreenTransitionType
{
    Fade,
    ZoomInOut,
    CircleReveal
}
public class TweenManager : MonoBehaviour
{
    public static TweenManager Instance;

    private Dictionary<RectTransform, Coroutine> activeTweens = new Dictionary<RectTransform, Coroutine>();
    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();
    private Dictionary<RectTransform, Quaternion> originalRotations = new Dictionary<RectTransform, Quaternion>();
    private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();


    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        PlayCutucadaLoop();
        Screen.SetResolution(1280, 720, false);
    }


    #region Screen Transitions

    public void SwitchScreens(CanvasGroup from, CanvasGroup to, ScreenTransitionType type)
    {
        StopAllCoroutines();
        StartCoroutine(SwitchRoutine(from, to, type));
    }

    private IEnumerator SwitchRoutine(CanvasGroup from, CanvasGroup to, ScreenTransitionType type)
    {

        if (from != null)
        {
            switch (type)
            {
                case ScreenTransitionType.Fade:
                    yield return StartCoroutine(FadeCanvas(from, 0.4f, 1f, 0f, false));
                    break;
                case ScreenTransitionType.ZoomInOut:
                    yield return StartCoroutine(ZoomOut(from.transform, 0.4f));
                    break;
                case ScreenTransitionType.CircleReveal:
                    yield return StartCoroutine(CircleWipe(from, false, 0.6f));
                    break;
            }

            from.gameObject.SetActive(false);
        }

        if (to != null)
        {
            to.gameObject.SetActive(true);

            switch (type)
            {
                case ScreenTransitionType.Fade:
                    yield return StartCoroutine(FadeCanvas(to, 0.4f, 0f, 1f, true));
                    break;
                case ScreenTransitionType.ZoomInOut:
                    yield return StartCoroutine(ZoomIn(to.transform, 0.4f));
                    break;
                case ScreenTransitionType.CircleReveal:
                    yield return StartCoroutine(CircleWipe(to, true, 0.6f));
                    break;
            }
        }
    }

    IEnumerator FadeCanvas(CanvasGroup canvasGroup, float duration, float start, float end, bool enableOnEnd)
    {
        float elapsed = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = end;
        canvasGroup.interactable = enableOnEnd;
        canvasGroup.blocksRaycasts = enableOnEnd;
    }

    IEnumerator ZoomIn(Transform target, float duration)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            target.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localScale = endScale;
    }

    IEnumerator ZoomOut(Transform target, float duration)
    {
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            target.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localScale = endScale;
    }

    IEnumerator CircleWipe(CanvasGroup canvasGroup, bool reveal, float duration)
    {

        RectTransform mask = canvasGroup.GetComponentInChildren<Mask>()?.rectTransform;

        if (mask == null)
            yield break;

        float elapsed = 0f;
        float start = reveal ? 0f : 1f;
        float end = reveal ? 1f : 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Lerp(start, end, elapsed / duration);
            mask.localScale = Vector3.one * t * 2f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        mask.localScale = Vector3.one * end * 2f;
    }

    #endregion

    #region === CONTROL ===
    public void StopTween(RectTransform target)
    {
        if (activeTweens.ContainsKey(target) && activeTweens[target] != null)
        {
            StopCoroutine(activeTweens[target]);
            activeTweens[target] = null;
        }

        // Reset
        if (originalScales.ContainsKey(target)) target.localScale = originalScales[target];
        if (originalRotations.ContainsKey(target)) target.localRotation = originalRotations[target];
        if (originalPositions.ContainsKey(target)) target.anchoredPosition = originalPositions[target];
    }

    private void SaveOriginal(RectTransform target)
    {
        if (!originalScales.ContainsKey(target)) originalScales[target] = target.localScale;
        if (!originalRotations.ContainsKey(target)) originalRotations[target] = target.localRotation;
        if (!originalPositions.ContainsKey(target)) originalPositions[target] = target.anchoredPosition;
    }
    #endregion

    #region === PULSE ===
    public void PlayPulse(RectTransform target, float strength = 1.1f, float duration = 0.6f)
    {
        StopTween(target);
        SaveOriginal(target);
        activeTweens[target] = StartCoroutine(PulseRoutine(target, strength, duration));
    }

    private IEnumerator PulseRoutine(RectTransform target, float strength, float duration)
    {
        Vector3 original = originalScales[target];
        Vector3 bigger = original * strength;

        while (true)
        {
            yield return ScaleRoutine(target, original, bigger, duration * 0.5f);
            yield return ScaleRoutine(target, bigger, original, duration * 0.5f);
        }
    }
    #endregion

    #region === POPUP ===
    public void PlayPopUp(RectTransform asset, float duration = 0.4f)
    {
        if (asset == null) return;
        StartCoroutine(PopUpRoutine(asset, duration));
    }

    private IEnumerator PopUpRoutine(RectTransform asset, float duration)
    {
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.one;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            asset.localScale = Vector3.Lerp(start, end, Mathf.Sin((elapsed / duration) * Mathf.PI * 0.5f));
            elapsed += Time.deltaTime;
            yield return null;
        }
        asset.localScale = end;
    }
    #endregion

    #region === SWING ===
    public void PlaySwing(RectTransform target, float angle = 15f, float duration = 0.5f)
    {
        StopTween(target);
        SaveOriginal(target);
        activeTweens[target] = StartCoroutine(SwingRoutine(target, angle, duration));
    }

    private IEnumerator SwingRoutine(RectTransform target, float angle, float duration)
    {
        Quaternion left = Quaternion.Euler(0, 0, -angle);
        Quaternion right = Quaternion.Euler(0, 0, angle);
        Quaternion original = originalRotations[target];

        while (true)
        {
            yield return RotationRoutine(target, original, left, duration * 0.5f);
            yield return RotationRoutine(target, left, right, duration);
            yield return RotationRoutine(target, right, original, duration * 0.5f);
        }
    }
    #endregion

    #region === THUMB ===
    public void PlayThumb(RectTransform target, float moveY = 50f, float duration = 1f)
    {
        StopTween(target);
        SaveOriginal(target);
        activeTweens[target] = StartCoroutine(ThumbRoutine(target, moveY, duration));
    }

    private IEnumerator ThumbRoutine(RectTransform target, float moveY, float duration)
    {
        Vector2 start = originalPositions[target];
        Vector2 up = start + new Vector2(0, moveY);

        while (true)
        {
            yield return PositionRoutine(target, start, up, duration * 0.5f);
            yield return PositionRoutine(target, up, start, duration * 0.5f);
        }
    }
    #endregion

    #region === ARROW NUDGE ===
    public void PlayArrowNudge(RectTransform target, float distance = 15f, float duration = 0.4f)
    {
        StopTween(target);
        SaveOriginal(target);
        activeTweens[target] = StartCoroutine(ArrowRoutine(target, distance, duration));
    }

    private IEnumerator ArrowRoutine(RectTransform target, float distance, float duration)
    {
        Vector2 start = originalPositions[target];
        Vector2 end = start + new Vector2(distance, 0);

        while (true)
        {
            yield return PositionRoutine(target, start, end, duration * 0.5f);
            yield return PositionRoutine(target, end, start, duration * 0.5f);
        }
    }
    #endregion

    #region === ZOOM TRANSITION ===
    public void PlayZoomInOut(RectTransform target, float duration = 0.6f)
    {
        StopTween(target);
        SaveOriginal(target);
        activeTweens[target] = StartCoroutine(ZoomRoutine(target, duration));
    }

    private IEnumerator ZoomRoutine(RectTransform target, float duration)
    {
        Vector3 original = originalScales[target];

        // zoom in
        yield return ScaleRoutine(target, Vector3.zero, original, duration * 0.5f);
        // zoom out
        yield return ScaleRoutine(target, original, Vector3.zero, duration * 0.5f);

        target.localScale = original; // reset
    }
    #endregion

    #region === CIRCLE REVEAL ===
    public void PlayCircleReveal(Image mask, float duration = 1f)
    {
        StopTween(mask.rectTransform);
        SaveOriginal(mask.rectTransform);
        activeTweens[mask.rectTransform] = StartCoroutine(CircleRoutine(mask, duration));
    }

    private IEnumerator CircleRoutine(Image mask, float duration)
    {
        float elapsed = 0f;
        mask.material.SetFloat("_Cutoff", 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            mask.material.SetFloat("_Cutoff", 1f - t);
            yield return null;
        }
        mask.material.SetFloat("_Cutoff", 0f);
    }
    #endregion

    #region === STRETCH ===
    public void PlayStretchY(RectTransform asset, float scaleY = 1.2f, float duration = 0.5f)
    {
        if (asset == null) return;
        StartCoroutine(StretchYRoutine(asset, scaleY, duration));
    }

    private IEnumerator StretchYRoutine(RectTransform asset, float scaleY, float duration)
    {
        Vector3 start = Vector3.one;
        Vector3 end = new Vector3(1f, scaleY, 1f);
        float elapsed = 0f;

        while (true)
        {
            while (elapsed < duration)
            {
                asset.localScale = Vector3.Lerp(start, end, Mathf.Sin((elapsed / duration) * Mathf.PI));
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
        }
    }
    #endregion

    #region === SHAKE ===
    public void PlayShake(RectTransform asset, float intensity = 5f, float speed = 50f)
    {
        if (asset == null) return;
        StartCoroutine(ShakeRoutine(asset, intensity, speed));
    }

    private IEnumerator ShakeRoutine(RectTransform asset, float intensity, float speed)
    {
        Vector3 originalPos = asset.localPosition;

        while (true)
        {
            float offsetX = Mathf.Sin(Time.time * speed) * intensity;
            float offsetY = Mathf.Cos(Time.time * speed * 1.2f) * intensity * 0.5f;

            asset.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            yield return null;
        }
    }
    #endregion

    #region === HELPERS ===
    private IEnumerator ScaleRoutine(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        target.localScale = to;
    }

    private IEnumerator RotationRoutine(RectTransform target, Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localRotation = Quaternion.Lerp(from, to, t);
            yield return null;
        }
        target.localRotation = to;
    }

    private IEnumerator PositionRoutine(RectTransform target, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        target.anchoredPosition = to;
    }
    #endregion

    #region === TANKS & THUMBS CUTUCADA (2 Tanks) ===
    [SerializeField] private RectTransform tankLeft;
    [SerializeField] private RectTransform tankRight;

    [SerializeField] private RectTransform thumbLeft;
    [SerializeField] private RectTransform thumbRight;

    //[SerializeField] private float tankDistance = 50f;
    //[SerializeField] private float tankDuration = 0.5f;
    //[SerializeField] private float tankReturnDuration = 0.5f;

    [SerializeField] private float thumbUpDistance = 100f;
    [SerializeField] private float thumbUpDuration = 0.5f;
    [SerializeField] private float thumbStayUpDuration = 1f;
    [SerializeField] private float thumbDownDistance = 200f;
    [SerializeField] private float thumbDownDuration = 0.5f;
    [SerializeField] private float thumbStayDownDuration = 1f;

    private Coroutine cutucadaLoop;

    public void PlayCutucadaLoop()
    {
        if (cutucadaLoop != null) StopCoroutine(cutucadaLoop);
        cutucadaLoop = StartCoroutine(CutucadaRoutine());
    }

    public void StopCutucadaLoop()
    {
        if (cutucadaLoop != null)
        {
            StopCoroutine(cutucadaLoop);
            cutucadaLoop = null;
        }

        
        if (tankLeft != null) tankLeft.anchoredPosition = tankLeftStart;
        if (tankRight != null) tankRight.anchoredPosition = tankRightStart;
        if (thumbLeft != null) thumbLeft.anchoredPosition = thumbLeftStart;
        if (thumbRight != null) thumbRight.anchoredPosition = thumbRightStart;
    }

    private Vector2 tankLeftStart, tankRightStart;
    private Vector2 thumbLeftStart, thumbRightStart;

    private IEnumerator CutucadaRoutine()
    {
        if (tankLeft == null || tankRight == null || thumbLeft == null || thumbRight == null)
            yield break;

        // Salva as posições iniciais
        tankLeftStart = tankLeft.anchoredPosition;
        tankRightStart = tankRight.anchoredPosition;
        thumbLeftStart = thumbLeft.anchoredPosition;
        thumbRightStart = thumbRight.anchoredPosition;

        while (true)
        {
            yield return StartCoroutine(CutucadaSingle(tankLeft, tankLeftStart, thumbLeft, thumbLeftStart, +1));
            yield return StartCoroutine(CutucadaSingle(tankRight, tankRightStart, thumbRight, thumbRightStart, -1));
        }
    }

    private IEnumerator CutucadaSingle(RectTransform tank, Vector3 tankStart, RectTransform thumb, Vector2 thumbStart, int dir)
    {

//yield return PositionRoutine(tank, tankStart, tankStart + Vector3.right * (tankDistance * dir), tankDuration);

        yield return PositionRoutine(thumb, thumbStart, thumbStart + Vector2.up * thumbUpDistance, thumbUpDuration);
        yield return new WaitForSeconds(thumbStayUpDuration);

        yield return PositionRoutine(thumb, thumbStart + Vector2.up * thumbUpDistance, thumbStart - Vector2.up * thumbDownDistance, thumbDownDuration);
        yield return new WaitForSeconds(thumbStayDownDuration);


        //yield return PositionRoutine(tank, tankStart + Vector3.right * (tankDistance * dir), tankStart, tankReturnDuration);

        thumb.anchoredPosition = thumbStart;
    }
    #endregion
}
