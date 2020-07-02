using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOfflineData : OfflineData
{

    public Vector2[] m_AnchorMax;
    public Vector2[] m_AnchorMin;
    public Vector2[] m_Pivot;
    public Vector2[] m_SizeDelta;
    public Vector3[] m_AnchoredPos;
    public ParticleSystem[] m_Particle;

    public override void BindData()
    {
        base.BindData();
    }

    public override void ResetProp()
    {
        Transform[] allTrs = gameObject.GetComponentsInChildren<Transform>(true);
        int allTrsCount = allTrs.Length;
        for (int i = 0; i < allTrsCount; i++)
        {
            if (!(allTrs[i] is RectTransform))
            {
                allTrs[i].gameObject.AddComponent<RectTransform>();
            }
        }

        m_AllPoint = gameObject.GetComponentsInChildren<RectTransform>(true);
        m_Particle = gameObject.GetComponentsInChildren<ParticleSystem>(true);
        int allPointCount = m_AllPoint.Length;
        m_AllPointChildCount = new int[allPointCount];
        m_AllPointActive = new bool[allPointCount];
        m_Pos = new Vector3[allPointCount];
        m_Rot = new Quaternion[allPointCount];
        m_Scale = new Vector3[allPointCount];
        m_Pivot = new Vector2[allPointCount];
        m_AnchorMax = new Vector2[allPointCount];
        m_AnchorMin = new Vector2[allPointCount];
        m_SizeDelta = new Vector2[allPointCount];
        m_AnchoredPos = new Vector3[allPointCount];

        for (int i = 0; i < allPointCount; i++)
        {
            RectTransform temp = m_AllPoint[i] as RectTransform;
            m_AllPointChildCount[i] = temp.childCount;
            m_AllPointActive[i] = temp.gameObject.activeSelf;
            m_Pos[i] = temp.localPosition;
            m_Rot[i] = temp.localRotation;
            m_Scale[i] = temp.localScale;

            m_Pivot[i] = temp.pivot;
            m_AnchorMax[i] = temp.anchorMax;
            m_AnchorMin[i] = temp.anchorMin;
            m_SizeDelta[i] = temp.sizeDelta;
            m_AnchoredPos[i] = temp.anchoredPosition3D;
        }
    }
}
