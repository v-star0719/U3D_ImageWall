using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSystem : MonoBehaviour
{
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		var list = ImageManager.Instance.ImageObjects;
		var selectedList = ImageManager.Instance.SelectedImageObjects;
		for(int i = 0; i < selectedList.Count; i++)
		{
			for(int j = 0; j < list.Count; j++)
			{
				var select = selectedList[i];
				var image = list[j];
				if(image == select)
				{
					continue;
				}

				if(IsIntersected(select, image))
				{
					image.transform.localPosition = GetCrowdPos(select, image);
				}
			}
		}


	}

	private bool IsIntersected(ImageObject image1, ImageObject image2)
	{
		var dist = image1.transform.localPosition - image2.transform.localPosition;
		var d = image1.BoundingRadius + image2.BoundingRadius;
		return dist.sqrMagnitude <= d * d;
	}

	private Vector3 GetCrowdPos(ImageObject select, ImageObject image)
	{
		var dir = image.transform.localPosition - select.transform.localPosition;
		var d = select.BoundingRadius + image.BoundingRadius;
		return dir.normalized * d + select.transform.localPosition;
	}
}
