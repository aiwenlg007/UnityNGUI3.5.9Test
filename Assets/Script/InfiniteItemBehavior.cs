
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 记录prefab序号和实际数据序号
/// 判断item是否可见
/// </summary>
public class InfiniteItemBehavior : MonoBehaviour {


    [HideInInspector]
	public UIPanel panel;

     [HideInInspector]
	public InfiniteListPopulator listPopulator;

    ///itemprefab序号
    //[HideInInspector]
	public int itemNumber;
    ///对应真实数据的序号
     //[HideInInspector]
	public int itemDataIndex;

	private bool isVisible = true;
	public BoxCollider thisCollider;

    /// <summary>
    /// 记录当前item的高度，以及绝对位置，为了解决item的高度不固定的情况
    /// 比如应用在房间聊天系统，每句话高度不一致
    /// </summary>
    public float m_ItemHeight;
    public float m_ItemWorldY;
    ///Item最顶层的widgt，决定当前的item是否显示
    UIWidget m_ItemWidgtVisible; 
		
	// Use this for initialization

    protected UIWidget GetMaxSizeWidget(GameObject go)
    {
        int max = 0;
        UIWidget ret = null;
        UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>();
        for (int i = 0; i < widgets.Length; i++)
        {
            UIWidget w = widgets[i];
            int size = w.width * w.height;
            if (size > max)
            {
                max = size;
                ret = w;
            }
        }
        return ret;
      
    }

	void Start() 
	{
		thisCollider = GetComponent<BoxCollider>();
		transform.localScale = new Vector3(1,1,1); // some weird scaling issues with NGUI
    
        m_ItemWidgtVisible = GetComponent<UIWidget>();

        if (m_ItemWidgtVisible == null)
        {
            m_ItemWidgtVisible = GetMaxSizeWidget(gameObject);
        }
	}
	void Update()
	{
        if (listPopulator)
        {
            if (listPopulator.m_Movement == UIScrollView.Movement.Horizontal)
            {
                if (Mathf.Abs(listPopulator.m_ScrollView.currentMomentum.x) > 0)
                {
                    CheckVisibilty();
                }
            }
            else if (listPopulator.m_Movement == UIScrollView.Movement.Vertical)
            {
                if (Mathf.Abs(listPopulator.m_ScrollView.currentMomentum.y) > 0)
                {
                    CheckVisibilty();
                }
            }
        }
	}
	public bool verifyVisibility()
	{
        if (m_ItemWidgtVisible)
        {
            return (panel.IsVisible(m_ItemWidgtVisible));
        }
        else
        {
            return false;
        }
	}	
	void OnClick()
	{
		listPopulator.itemClicked(itemDataIndex);
	}
	void OnDrag(Vector2 delta)
	{
       
	}
	void OnPress (bool isDown)
	{
        
        listPopulator.itemIsPressed(itemDataIndex, isDown);
		
	}
	void CheckVisibilty() 
	{
        if (m_ItemWidgtVisible)
        {
            bool currentVisibilty = panel.IsVisible(m_ItemWidgtVisible);
		    if(currentVisibilty != isVisible)
		    {
			    isVisible = currentVisibilty;
                if (thisCollider)
                {
                    thisCollider.enabled = isVisible;
                }
			
			    if(!isVisible)
			    {
			        StartCoroutine(listPopulator.ItemIsInvisible(itemNumber));
                 // listPopulator.TestItemIsInvisible(itemNumber);
			    }
		}
        }

	}

    public void DeleteItem()
    {
        listPopulator.DeleteItemByIdx(itemDataIndex);
    }
}
