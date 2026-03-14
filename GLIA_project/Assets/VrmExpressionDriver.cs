using System.Collections;
using UnityEngine;
using UniVRM10;

public class VrmExpressionDriver : MonoBehaviour
{
    private Vrm10Instance _instance;
    private Vrm10RuntimeExpression _expression;

    [Header("Blink")]
    [SerializeField] private bool autoBlink = true;
    [SerializeField] private float blinkIntervalMin = 2.5f;
    [SerializeField] private float blinkIntervalMax = 5.0f;
    [SerializeField] private float blinkCloseTime = 0.06f;
    [SerializeField] private float blinkOpenTime = 0.08f;
    [SerializeField] private float blinkHoldTime = 0.02f;

    [Header("Lip Sync (fake demo)")]
    [SerializeField] private bool autoTalk = false;
    [SerializeField] private float talkSpeed = 8.0f;

    [Header("Emotion")]
    [Range(0f, 1f)]
    [SerializeField] private float happyWeight = 0f;

    private Coroutine _blinkRoutine;
    private Coroutine _talkRoutine;

    private void Awake()
    {
        _instance = GetComponent<Vrm10Instance>();
        if (_instance == null)
        {
            Debug.LogError("Vrm10Instance が見つかりません。VRMルートにこのスクリプトを付けてください。");
            enabled = false;
            return;
        }

        _expression = _instance.Runtime.Expression;
        if (_expression == null)
        {
            Debug.LogError("Runtime.Expression が取得できませんでした。");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_expression == null) return;

        if (autoBlink)
        {
            _blinkRoutine = StartCoroutine(BlinkLoop());
        }

        if (autoTalk)
        {
            _talkRoutine = StartCoroutine(FakeTalkLoop());
        }
    }

    private void OnDisable()
    {
        if (_blinkRoutine != null) StopCoroutine(_blinkRoutine);
        if (_talkRoutine != null) StopCoroutine(_talkRoutine);

        ResetMouth();
        SetHappy(0f);
        SetBlink(0f);
    }

    private void Update()
    {
        // Inspector から happyWeight をいじるだけで確認できる
        SetHappy(happyWeight);

        // テスト用ショートカット
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(BlinkOnce());
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResetMouth();
            SetMouth(ExpressionKey.Aa, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ResetMouth();
            SetMouth(ExpressionKey.Ih, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ResetMouth();
            SetMouth(ExpressionKey.Ou, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ResetMouth();
            SetMouth(ExpressionKey.Ee, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ResetMouth();
            SetMouth(ExpressionKey.Oh, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetMouth();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            happyWeight = happyWeight > 0.5f ? 0f : 1f;
        }
    }

    public void SetHappy(float weight)
    {
        _expression.SetWeight(ExpressionKey.Happy, Mathf.Clamp01(weight));
    }

    public void SetBlink(float weight)
    {
        _expression.SetWeight(ExpressionKey.Blink, Mathf.Clamp01(weight));
    }

    public void SetMouth(ExpressionKey key, float weight)
    {
        _expression.SetWeight(key, Mathf.Clamp01(weight));
    }

    public void ResetMouth()
    {
        _expression.SetWeight(ExpressionKey.Aa, 0f);
        _expression.SetWeight(ExpressionKey.Ih, 0f);
        _expression.SetWeight(ExpressionKey.Ou, 0f);
        _expression.SetWeight(ExpressionKey.Ee, 0f);
        _expression.SetWeight(ExpressionKey.Oh, 0f);
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            float wait = Random.Range(blinkIntervalMin, blinkIntervalMax);
            yield return new WaitForSeconds(wait);
            yield return BlinkOnce();
        }
    }

    private IEnumerator BlinkOnce()
    {
        // close
        float t = 0f;
        while (t < blinkCloseTime)
        {
            t += Time.deltaTime;
            float w = Mathf.Clamp01(t / blinkCloseTime);
            SetBlink(w);
            yield return null;
        }
        SetBlink(1f);

        yield return new WaitForSeconds(blinkHoldTime);

        // open
        t = 0f;
        while (t < blinkOpenTime)
        {
            t += Time.deltaTime;
            float w = 1f - Mathf.Clamp01(t / blinkOpenTime);
            SetBlink(w);
            yield return null;
        }
        SetBlink(0f);
    }

    private IEnumerator FakeTalkLoop()
    {
        ExpressionKey[] mouthKeys =
        {
            ExpressionKey.Aa,
            ExpressionKey.Ih,
            ExpressionKey.Ou,
            ExpressionKey.Ee,
            ExpressionKey.Oh
        };

        while (true)
        {
            ResetMouth();

            int index = Random.Range(0, mouthKeys.Length);
            float weight = 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(Time.time * talkSpeed));
            SetMouth(mouthKeys[index], weight);

            yield return new WaitForSeconds(0.08f);
        }
    }
}