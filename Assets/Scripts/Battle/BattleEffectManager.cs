using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI; // or TMPro
using UnityEngine.EventSystems; // if needed

public class BattleEffectManager : MonoBehaviour
{
    [Header("UI Canvas (World Space)")]
    [SerializeField] private Canvas battleZoneCanvas;

    [Header("UI Projectile Prefab")]
    [SerializeField] private GameObject projectileUIPrefab;  // RectTransform + Image

    [Header("Travel Settings")]
    [SerializeField] private float travelTime = 0.3f;

    private void Start()
    {
        battleZoneCanvas.worldCamera = Camera.main;
        
        BattleSystem.Instance.AttackEffect += OnAttackEffect;
        BattleSystem.Instance.SetCanvas += OnSetCanvas;
    }
    
    private void OnDestroy()
    {
        if (BattleSystem.Instance != null)
        {
            BattleSystem.Instance.AttackEffect -= OnAttackEffect;
            BattleSystem.Instance.SetCanvas   -= OnSetCanvas;
        }
    }

    public void OnAttackEffect(Card attacker, Card target)
    {
         _ = PlayProjectileUI(attacker, target);
    }

    public void OnSetCanvas(Vector3 size, Vector3 center)
    {
        var canvasRT = battleZoneCanvas.GetComponent<RectTransform>();
        
        canvasRT.sizeDelta = new Vector2(size.x, size.y);
        canvasRT.transform.position = center;
    }
    
    private async UniTask PlayProjectileUI(Card attacker, Card target)
    {
        var canvasRT = battleZoneCanvas.GetComponent<RectTransform>();

        var screenStart = Camera.main.WorldToScreenPoint(attacker.transform.position);
        var screenEnd   = Camera.main.WorldToScreenPoint(target.transform.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, screenStart, battleZoneCanvas.worldCamera, out Vector2 startPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, screenEnd,   battleZoneCanvas.worldCamera, out Vector2 endPos);

        var projGo = Instantiate(projectileUIPrefab, battleZoneCanvas.transform);
        var img    = projGo.GetComponent<Image>();
        var rt     = projGo.GetComponent<RectTransform>();

        var spriteWorldSize = new Vector2(
            img.sprite.bounds.size.x,
            img.sprite.bounds.size.y
        );

        var zoneWidth    = canvasRT.sizeDelta.x;
        var desiredPct   = 0.5f;
        var targetWorldW = zoneWidth * desiredPct;
        var scaleFactor  = targetWorldW / spriteWorldSize.x;

        rt.sizeDelta  = spriteWorldSize;
        rt.localScale = Vector3.one * scaleFactor;

        rt.anchoredPosition    = startPos;
        var dir            = (endPos - startPos).normalized;
        var   angle          = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.localEulerAngles    = new Vector3(0f, 0f, angle);

        var elapsed = 0f;
        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / travelTime);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            await UniTask.Yield();
        }

        Destroy(projGo);
    }

}
