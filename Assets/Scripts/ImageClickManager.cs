using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageClickManager : MonoBehaviour
{

	public static ImageClickManager instance = null;

	public List<GameObject> activatObjs = new List<GameObject>();

	void Awake()
	{
		instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		#region 检测鼠标是否点击到某张图片
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
			RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
			if (hit.collider != null)
			{
				if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Activation"))
				{
					GameObject obj = hit.collider.gameObject;
					activatObjs.Add(obj);
					ImageAnimationManager.instance.PlayImageAnim(obj);
				}
			}
		}
		#endregion
	}
}
