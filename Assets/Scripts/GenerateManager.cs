using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateManager : MonoBehaviour
{
	[Header("生成物集合")]
	public Texture2D[] textures;
	[Header("图片的格数：横/竖")]
	public Vector2 numbers;
	[Header("图片与图片之间的距离：左/上")]
	public Vector2 margin;
	[Header("图片的缩放值")]
	public float scale;

	[Header("基础Sprite")]
	public GameObject sprite;

	[Header("所有图片的父节点")]
	public Transform parent;

	/// <summary>
	/// 坐标集合
	/// </summary>
	private Vector2[][] axis;

	/// <summary>
	/// 窗口宽高-基于世界坐标
	/// </summary>
	private float width = 1280;
	private float height = 720;

	// Use this for initialization
	void Start()
	{
		//GetBasicValues();
		GetImageValue();
	}

	// Update is called once per frame
	void Update()
	{
		/// Test
		if (Input.GetKeyUp(KeyCode.A))
		{
			GetImageValue();
		}
		if (Input.GetKeyUp(KeyCode.C))
		{
			CleanImage();
		}
		///
	}

	/// <summary>
	/// 获取窗口宽高-基于世界坐标
	/// </summary>
	void GetBasicValues()
	{
		float leftBorder;
		float rightBorder;
		float topBorder;
		float downBorder;
		//the up right corner
		Vector3 cornerPos = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, Mathf.Abs(Camera.main.transform.position.z)));

		leftBorder = Camera.main.transform.position.x - (cornerPos.x - Camera.main.transform.position.x);
		rightBorder = cornerPos.x;
		topBorder = cornerPos.y;
		downBorder = Camera.main.transform.position.y - (cornerPos.y - Camera.main.transform.position.y);

		width = rightBorder - leftBorder;
		height = topBorder - downBorder;
		Debug.Log("width:" + width + ",height:" + height);
	}

	/// <summary>
	/// 获取图片数据-每张图片大小一样
	/// </summary>
	void GetImageValue()
	{
		axis = new Vector2[(int)numbers.x][];
		for (int i = 0; i < axis.Length; i++)
		{
			axis[i] = new Vector2[(int)numbers.y];
		}
		if (textures.Length != 0)
		{
			// 实际图片与世界坐标倍数为100
			float imageWidth = textures[0].width / 10.0f * scale;
			float imageHeight = textures[0].height / 10.0f * scale;
			float boxWidth = imageWidth + 10;
			float boxHeight = imageHeight + 10;
			 Vector2 org = new Vector2(-boxWidth * numbers.x / 2, boxHeight * numbers.y / 2);
			Vector2 pos = org;
			for (int i = 0; i < axis.Length; i++)
			{
				for (int j = 0; j < axis[i].Length; j++)
				{
					axis[i][j] = pos;
					int index = Random.Range(0, textures.Length);
					GameObject gameObject = Instantiate(sprite);
					gameObject.SetActive(true);

					Texture2D texture2d = Instantiate(textures[index]) as Texture2D;
					UITexture texture = gameObject.GetComponent<UITexture>();
					texture.width = (int)imageWidth;
					texture.height = (int)imageHeight;

					texture.mainTexture = texture2d;
					gameObject.tag = "Player";
					gameObject.layer = LayerMask.NameToLayer("Image");
					gameObject.name = "" + i + "-" + j;
					//gameObject.transform.position = axis[i][j];
					//gameObject.transform.position = new Vector3(axis[i][j].x - 22, axis[i][j].y, 0);
					gameObject.transform.parent = parent;
					//gameObject.transform.position = new Vector3(-13.2f, -4, 0);
					gameObject.transform.localScale = Vector3.one;
					gameObject.transform.localPosition  = axis[i][j];
					AddImageComponent(gameObject.transform);
					ImageMoveAction imageMoveAction = gameObject.AddComponent<ImageMoveAction>();
					//imageMoveAction.MoveTo(axis[i][j], new Vector2(i, j), numbers);

					var image = gameObject.GetComponent<ImageObject>();
					ImageManager.Instance.Add(image);
					image.Init();

					pos.x += boxWidth;
				}

				pos.x = org.x;
				pos.y -= boxHeight;
			}
		}
	}

	/// <summary>
	/// 对每个图片增加
	/// </summary>
	/// <param name="transform"></param>
	public void AddImageComponent(Transform transform)
	{
	}

	/// <summary>
	/// 清除非激活Image
	/// </summary>
	public void CleanImage()
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child.gameObject.layer != LayerMask.NameToLayer("Activation"))
			{
				Destroy(child.gameObject);
			}
		}
	}
}
