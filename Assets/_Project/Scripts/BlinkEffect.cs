using UnityEngine;
using UnityEngine.UI;

// UI（ImageやText）を点滅させるコンポーネント。選択中のカーソル等で使う。
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
        // 透明なタイミングで非アクティブにされると、次に表示した時に消えたままになるのでリセットしておく
        ResetVisual();
    }

    void Update()
    {
        if (uiGraphic == null) return;

        if (isSelected)
        {
            // ポーズ中（Time.timeScale == 0）でも点滅させたいので unscaledTime を使う
            bool isVisible = (Time.unscaledTime % (blinkInterval * 2)) < blinkInterval;
            
            // colorを変更するとUIの再描画(Rebuild)が走って重くなるため、CanvasRendererのAlphaを直接いじる
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