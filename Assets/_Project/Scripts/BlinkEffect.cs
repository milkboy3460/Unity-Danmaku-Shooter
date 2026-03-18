using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択中のUI要素（ImageやText等）を点滅させる演出コンポーネント
/// </summary>
public class BlinkEffect : MonoBehaviour
{
    [SerializeField] private float blinkInterval = 0.2f; 
    private Graphic uiGraphic;
    public bool isSelected = false; 

    void Awake()
    {
        uiGraphic = GetComponent<Graphic>();
    }

    void OnEnable()
    {
        // 非アクティブ時に透明のまま終了した際、次回表示時に消えたままになるのを防ぐ
        ResetVisual();
    }

    void Update()
    {
        if (uiGraphic == null) return;

        if (isSelected)
        {
            // ポーズ画面など Time.timeScale == 0 の状態でも点滅アニメーションを維持するため、unscaledTime を使用
            bool isVisible = (Time.unscaledTime % (blinkInterval * 2)) < blinkInterval;
            
            // UIの再描画負荷（Rebuild）を抑えるため、colorではなくCanvasRendererのAlphaを直接操作する
            uiGraphic.canvasRenderer.SetAlpha(isVisible ? 1.0f : 0.0f);
        }
        else
        {
            ResetVisual();
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (!selected) ResetVisual();
    }

    private void ResetVisual()
    {
        if (uiGraphic != null)
        {
            uiGraphic.canvasRenderer.SetAlpha(1.0f);
        }
    }
}