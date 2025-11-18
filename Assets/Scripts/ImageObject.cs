using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImageObject : MonoBehaviour
{
	//public static float BoundingRadius = 200;
	public static Vector3 DefaultMoveSpeed = new Vector3(3f, 0, 0); //默认状态下的移动速度，进行漂流
	public static float MinScale = 0.3f; //默认状态下的移动速度，进行漂流

	public bool IsSelected = false;//是否完全静止

	public TweenScale SelecTweenScale;

	private float t = 2f;//等刷出动画执行完成后，开始漂流

	private Vector2 dragDelta;
	private Vector2 lastDragPos;
	private bool isDragging;
	private Vector3 curSpeed = Vector3.zero;
	private UITexture texture;
	private Vector3 bornPos;

	public float Width
	{
		get
		{
			return transform.localScale.x * texture.width;
		}
	}

	public float Height
	{
		get
		{
			return transform.localScale.y * texture.height;
		}
	}

	public float BoundingRadius
	{
		get
		{
			float w = Width;
			float h = Height;
			var r = Mathf.Sqrt(w * w + h * h) * 0.5f;
			return IsSelected ? r * 1.2f : r;
		}
	}

	public void Init()
	{
		curSpeed = DefaultMoveSpeed;
		texture = gameObject.GetComponent<UITexture>();
		bornPos = transform.localPosition;
	}

	// Update is called once per frame
	void Update ()
	{
		if(IsSelected)
		{
			return;
		}

		if(t > 0)
		{
			t -= Time.deltaTime;
			return;
		}

		if(isDragging)
		{
			return;
		}

		UdpateFlow();
		UpdateScale();
		UdpateBackToBirthPoint();
	}

	//离出生点越远，缩放越小。可以使用曲线定义这个变化。这里直接是线性的
	private void UpdateScale()
	{
		var dist = Vector3.Distance(bornPos, transform.localPosition);
		var scale = Mathf.Lerp(1f, 0.3f, dist / 60);
		transform.localScale = new Vector3(scale, scale, scale);
	}

	//漂浮效果
	private void UdpateFlow()
	{
		var delta = DefaultMoveSpeed * Time.deltaTime;
		transform.localPosition += DefaultMoveSpeed * Time.deltaTime;
		bornPos += delta;//出生点也在漂浮
	}

	//图片总是想要回到出生点
	private void UdpateBackToBirthPoint()
	{
		float speed = 30f;
		var dir = bornPos - transform.localPosition;
		transform.localPosition += dir.normalized * speed * Time.deltaTime;
	}

	private void UpdateSelectAnima()
	{
		
	}

	public void OnClick()
	{
		if(!IsSelected)
		{
			SelecTweenScale.ResetToBeginning();
			SelecTweenScale.PlayForward();
		}

		//transform.localScale = new Vector3(2, 2, 2);
		curSpeed = Vector3.zero;
		ImageManager.Instance.AddSelect(this);
		IsSelected = true;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		isDragging = true;
		lastDragPos = eventData.position;
		dragDelta = eventData.delta;
		curSpeed = Vector3.zero;
	}


	public void OnDrag(Vector2 delta)
	{
		transform.localPosition += new Vector3(delta.x, delta.y, transform.localPosition.z);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;
	}
}
