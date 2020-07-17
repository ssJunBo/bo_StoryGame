using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TipsItem : BaseItem
{
    public Text content;
    private void OnEnable()
    {
        ToolsManager.TimeCallback(this, 15f, () =>
        {
            ObjectManager.Instance.ReleaseObject(gameObject);
        });
        transform.DOLocalMoveY(transform.localPosition.y + 400, 0.5f);
    }
}
