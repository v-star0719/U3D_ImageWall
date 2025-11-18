using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageAnimationManager : MonoBehaviour
{

	public static ImageAnimationManager instance = null;

	public AnimationCurve SelectEnlargeAC;

	[Header("按钮碰撞放大倍数:")]
	public float enlarge = 2.0f;

	[Header("按钮碰撞放大速度:")]
	public float deltaSpeed = 10.0f;

	/// <summary>
	/// 是否开始进行放大变化
	/// </summary>
	private bool isBeginPlay = false;

	/// <summary>
	/// 需要放大的碰撞体
	/// </summary>
	private CircleCollider2D circleCollider2D = null;

	/// <summary>
	/// 需要放大的按钮
	/// </summary>
	private GameObject curObject = null;

	/// <summary>
	/// 键值对儿的形式保存iTween所用到的参数  
	/// </summary>
	Hashtable args = new Hashtable();

	[Space(10)]
	[Header("iTween放大倍数:")]
	public Vector3 iTweenScale = new Vector3(0.8f, 0.8f, 0.8f);
	[Header("iTween速度:")]
	public float iTweenSpeed = 10.0f;
	[Header("iTween放大时间:")]
	public float iTweenTime = 1.0f;
	[Header("iTween延时时间:")]
	public float iTweenDelay = 0.0f;

	void Awake()
	{
		instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		//放大的倍数  
		args.Add("scale", iTweenScale);

		//动画的速度  
		args.Add("speed", iTweenSpeed);
		//动画的时间  
		args.Add("time", iTweenTime);
		//延迟执行时间  
		args.Add("delay", iTweenDelay);

		//这里是设置类型，iTween的类型又很多种，在源码中的枚举EaseType中  
		args.Add("easeType", iTween.EaseType.linear);
		//三个循环类型 none loop pingPong (一般 循环 来回)     
		args.Add("loopType", "none");
		//args.Add("loopType", "loop");    
		//args.Add("loopType", iTween.LoopType.pingPong);

	}
	
	// Update is called once per frame
	void Update ()
	{
		if (isBeginPlay && circleCollider2D != null)
		{
			if (circleCollider2D.radius < enlarge)
			{
				circleCollider2D.radius += Time.deltaTime * deltaSpeed;
			}
			else
			{
				isBeginPlay = false;
				circleCollider2D = null;
			}
		}
	}

	/// <summary>
	/// 开始播放点击动画
	/// </summary>
	/// <param name="gameObject"></param>
	public void PlayImageAnim(GameObject gameObject)
	{
		curObject = gameObject;
		Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
		circleCollider2D = gameObject.GetComponent<CircleCollider2D>();
		SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		if (rigidbody2D && circleCollider2D && spriteRenderer)
		{
			gameObject.layer = LayerMask.NameToLayer("Activation");
			gameObject.AddComponent<ImageColliderAction>();
			spriteRenderer.sortingOrder = ImageUtil.IMAGE_ORDER_MAX;
			//rigidbody2D.AddForce(gameObject.transform.up * 10);
			rigidbody2D.constraints = RigidbodyConstraints2D.FreezePosition;
			isBeginPlay = true;
			iTween.ScaleTo(gameObject, args);
		}
	}

}
