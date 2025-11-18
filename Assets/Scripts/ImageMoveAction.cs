using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// http://robertpenner.com/easing/easing_demo.html
/// iTween运动轨迹
/// </summary>
public class ImageMoveAction : MonoBehaviour
{
	/// <summary>
	/// 从开始路径到目标路径之间的障碍物
	/// </summary>
	private GameObject[] circleColliders;

	/// <summary>
	/// 图片随机的缩小倍数
	/// </summary>
	private float multiple = 0;

	public enum Orientation
	{
		FromTopToBottom,
		FromBottomToTop,
		FromLeftToRight,
		FromRightToLeft
	}

	public Orientation orientation = ImageMoveAction.Orientation.FromLeftToRight;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	/// <summary>
	/// 开始移动
	/// </summary>
	/// <param name="vec"></param>
	/// <param name="location"></param>
	/// <param name="numbers"></param>
	public void MoveTo(Vector2 vec, Vector2 location, Vector2 numbers)
	{
		float delay = 0;// GetDelayTime(location, numbers);
		vec = GetFinalTargetPosition(vec);
		bool isCollider = SetCollider(vec);
		List<Vector3> paths = GetPaths(vec);
		if (paths.Count != 0)
		{
			if (isCollider)
			{
				Hashtable args = new Hashtable();
				args.Add("loopType", "none");
				args.Add("time", 1.0f);
				delay = 0.1f;
				args.Add("delay", delay);
				args.Add("easeType", iTween.EaseType.linear);
				//GameObject game0 = Instantiate(GameObject.Find("0"));
				//int i = 0;
				//foreach (var item in paths)
				//{
				//    GameObject game = Instantiate(GameObject.Find("Sphere"));
				//    game.name = "" + i;
				//    i++;
				//    game.transform.position = item;
				//    game.transform.parent = game0.transform;
				//}
				args.Add("path", paths.ToArray());
				args.Add("oncomplete", "AnimationColliderEnd");
				args.Add("oncompleteparams", vec);
				args.Add("oncompletetarget", gameObject);
				iTween.MoveTo(gameObject, args);
			}
			else
			{
				Hashtable args = new Hashtable();
				args.Add("loopType", "none");
				args.Add("time", 1.0f);
				delay = 0.1f;
				args.Add("delay", delay);
				args.Add("easeType", iTween.EaseType.linear);
				paths.Add(new Vector3(vec.x, vec.y, 0));
				args.Add("path", paths.ToArray());
				args.Add("oncomplete", "AnimationEnd");
				args.Add("oncompletetarget", gameObject);
				iTween.MoveTo(gameObject, args);
			}
		}
		else
		{
			Hashtable args = new Hashtable();
			args.Add("loopType", "none");
			args.Add("time", 1.2f);
			delay = 0;
			args.Add("position", new Vector3(vec.x, vec.y, 0));
			args.Add("easeType", iTween.EaseType.easeInOutExpo);
			args.Add("delay", delay);
			args.Add("oncomplete", "AnimationEnd");
			args.Add("oncompletetarget", gameObject);
			iTween.MoveTo(gameObject, args);
		}
	}

	/// <summary>
	/// 由位置获取delay值,中间最快两边慢
	/// </summary>
	/// <returns></returns>
	float GetDelayTime(Vector2 location, Vector2 numbers)
	{
		int middle = -1;
		int len = -1;
		float delay = 0;
		switch (orientation)
		{
			case Orientation.FromTopToBottom:
			case Orientation.FromBottomToTop:
				middle = (int)location.x;
				len = (int)numbers.x;
				break;
			case Orientation.FromLeftToRight:
			case Orientation.FromRightToLeft:
				middle = (int)location.y;
				len = (int)numbers.y;
				break;
			default:
				break;
		}
		if (middle != -1 && len != -1)
		{
			delay = System.Math.Abs(((float)middle - (float)len / 2) / 30.0f);
		}
		return delay;
	}

	/// <summary>
	/// 获取最终目标位置,如果在障碍物碰撞范围内则需要将目标位置设置为碰撞范围外
	/// </summary>
	/// <param name="vec"></param>
	Vector2 GetFinalTargetPosition(Vector2 vec)
	{
		GameObject circle = IsInTheCircle(vec);
		while (circle != null)
		{
			// 如果目标位置在碰撞内,则提前打开碰撞
			SetTrigger(false);
			vec = GetNewPosition(vec, circle);
			circle = IsInTheCircle(vec);
		}
		return vec;
	}

	/// <summary>
	/// 判断是否在某个障碍物碰撞范围内,如在,则返回障碍物
	/// </summary>
	/// <param name="vec"></param>
	/// <returns></returns>
	GameObject IsInTheCircle(Vector2 vec)
	{
		GameObject circle = null;
		// 自身半径
		//float radius = transform.localScale.x * gameObject.GetComponent<CircleCollider2D>().radius;
		float radius = 10;
		// 获取路径内有多少个障碍
		RaycastHit2D[] ray = Physics2D.LinecastAll(transform.position, vec, 1 << LayerMask.NameToLayer("Activation"));
		foreach (var item in ray)
		{
			var obj = item.collider.gameObject;
			// 障碍半径
			float radiusItem = obj.transform.localScale.x * obj.GetComponent<CircleCollider2D>().radius;
			// 根据障碍中心到目标位置的距离判定目标位置是否位于某个障碍物碰撞内
			if (Vector2.Distance(obj.transform.position, vec) <= radius + radiusItem)
			{
				circle = obj;
				break;
			}
		}
		return circle;
	}

	/// <summary>
	/// 如果目标位置在障碍物碰撞范围内,则获取新的位于碰撞外的位置
	/// </summary>
	/// <param name="vec"></param>
	/// <param name="circle"></param>
	/// <returns></returns>
	Vector2 GetNewPosition(Vector2 vec, GameObject circle)
	{
		Vector2 center = circle.transform.position;

		// 求距离等于自身碰撞半径+障碍物碰撞半径+0.1)的距离,并且在直线ax+b=y上的位置
		float radius = transform.localScale.x * gameObject.GetComponent<CircleCollider2D>().radius +
			 circle.transform.localScale.x * circle.GetComponent<CircleCollider2D>().radius + 0.1f;

		// 求目标点和障碍物圆心的直线方程:kx+b=y
		// (x1 - x2) * a = y1 - y2;
		// 注意:这里需要考虑水平运动还是垂直运动,垂直运动则根据y判断直线是否过圆心
		if (vec.x == center.x)
		{
			Vector2 vector2 = new Vector2(vec.x, 0);
			if (vec.y < center.y)
			{
				vector2.y = center.y - radius;
			}
			else
			{
				vector2.y = center.y + radius;
			}
			return vector2;
		}
		else
		{
			float k = (vec.y - center.y) / (vec.x - center.x);
			// a * x1 + b = y1;
			float b = vec.y - k * vec.x;

			// 根据((x-c) * (x-c) + (y-d) * (y-d)=r*r)和k * x + b=y求交点
			Vector2[] vecs = ImageUtil.GetIntersectionOfCircleAndLine(radius, center.x, center.y, k, b);

			// 得出直线和圆的交点为2个,判断原目标点在障碍物左侧还是右侧,则取x小的还是大的点----注意:这里需要考虑水平运动还是垂直运动,垂直运动则根据y判断
			if (vec.x < center.x)
			{
				return vecs[0].x < vecs[1].x ? vecs[0] : vecs[1];
			}
			else
			{
				return vecs[0].x > vecs[1].x ? vecs[0] : vecs[1];
			}
		}
	}

	/// <summary>
	/// 如果路径中有障碍物,则计算绕行路线
	/// </summary>
	/// <param name="vec"></param>
	/// <returns></returns>
	List<Vector3> GetPaths(Vector2 vec)
	{
		List<Vector3> vector3s = new List<Vector3>();

		// 获取路径内有多少个障碍
		RaycastHit2D[] ray = Physics2D.LinecastAll(transform.position, vec, 1 << LayerMask.NameToLayer("Activation"));
		foreach (var item in ray)
		{
			var obj = item.collider.gameObject;
			// 障碍半径
			float radiusItem = obj.transform.localScale.x * obj.GetComponent<CircleCollider2D>().radius;

			// 提前计算碰撞后应该缩小的倍数
			multiple = Random.Range(0.2f, 0.4f);
			// 碰撞后自身半径
			float radiusSelf = multiple * gameObject.GetComponent<CircleCollider2D>().radius;

			// 设置图片绕行障碍物半径,略小于自身半径+障碍物半径,保证运动过程中产生碰撞使图片进行随机倍数缩小
			float radius = radiusItem + radiusSelf - 0.1f;
			vector3s.AddRange(GetPathsForRoundAndLine(gameObject.transform.position, vec, obj.transform.position.x, obj.transform.position.y, radius));
		}
		return vector3s;
	}

	/// <summary>
	/// 获取路径中遇到单个碰撞体的变化路径
	/// </summary>
	/// <param name="point1"></param>
	/// <param name="point2"></param>
	/// <param name="c"></param>
	/// <param name="d"></param>
	/// <param name="r"></param>
	/// <returns></returns>
	List<Vector3> GetPathsForRoundAndLine(Vector2 point1, Vector2 point2, float c, float d, float r)
	{
		List<Vector3> vector3s = new List<Vector3>();

		// 求目标点和障碍物圆心的直线方程:kx+b=y
		float k = (point2.y - point1.y) / (point2.x - point1.x);
		float b = point1.y - k * point1.x;

		// 根据((x-c) * (x-c) + (y-d) * (y-d)=r*r)和k * x + b=y求交点
		Vector2[] vecs = ImageUtil.GetIntersectionOfCircleAndLine(r, c, d, k, b);

		// 保证x小的点在数组前面
		if (vecs[0].x > vecs[1].x)
		{
			Vector2 temp = new Vector2(vecs[1].x, vecs[1].y);
			vecs[1] = new Vector2(vecs[0].x, vecs[0].y);
			vecs[0] = new Vector2(temp.x, temp.y);
		}
		//vector3s.Add(vecs[0]);
		// 每个一个0.1的单位取一个碰撞圆上的一点-------注意:如果是垂直运动则需要使用y计算
		float min = vecs[0].x;
		float max = vecs[1].x;
		for (float i = min + 0.1f; i < max; i++)
		{
			Vector2 vector = vecs[0];
			Vector2[] vector2s = ImageUtil.GetRoundPoints(r, c, d, i);
			//判断交点在碰撞圆心上还是碰撞圆心下,以此取值路径在碰撞圆上方或下方
			if (vecs[0].y > d)
			{
				vector = vector2s[0].y > vector2s[1].y ? vector2s[0] : vector2s[1];
			}
			else
			{
				vector = vector2s[0].y < vector2s[1].y ? vector2s[0] : vector2s[1];
			}
			vector3s.Add(vector);
		}
		vector3s.Add(vecs[1]);

		return vector3s;
	}

	/// <summary>
	/// 判断最终目标位置是否在碰撞范围内
	/// </summary>
	/// <param name="vec"></param>
	/// <returns></returns>
	bool SetCollider(Vector2 vec)
	{
		bool isCollider = false;
		foreach (var item in ImageClickManager.instance.activatObjs)
		{
			float dis = Vector3.Distance(item.transform.position, vec);
			float radius = gameObject.transform.localScale.x * gameObject.GetComponent<CircleCollider2D>().radius +
				item.transform.localScale.x * item.GetComponent<CircleCollider2D>().radius;
			if (dis <= radius)
			{
				isCollider = true;
				SetTrigger(false);
				break;
			}
		}
		return isCollider;
	}

	/// <summary>
	/// 播放图片随机缩小动画
	/// </summary>
	public void AddScaleAnim()
	{
		if (multiple == 0)
		{
			multiple = Random.Range(0.2f, 0.4f);
		}
		//键值对儿的形式保存iTween所用到的参数
		Hashtable args = new Hashtable();

		//放大的倍数
		args.Add("scale", new Vector3(multiple, multiple, multiple));

		//动画的时间
		args.Add("time", 0.3f);
		//延迟执行时间
		args.Add("delay", 0);

		//这里是设置类型，iTween的类型又很多种，在源码中的枚举EaseType中
		args.Add("easeType", iTween.EaseType.easeInOutExpo);
		//三个循环类型 none loop pingPong (一般 循环 来回)	
		args.Add("loopType", iTween.LoopType.none);

		iTween.ScaleTo(gameObject, args);
	}

	/// <summary>
	/// 对象移动结束时时调用
	/// </summary>
	void AnimationEnd()
	{
		SetTrigger(false);
	}

	/// <summary>
	/// 对象移动结束时时调用
	/// </summary>
	void AnimationColliderEnd(Vector2 vector2)
	{
		SetTrigger(false);
		if (!vector2.Equals(Vector2.negativeInfinity))
		{
			Hashtable args = new Hashtable();
			args.Add("loopType", "none");
			args.Add("time", 0.3f);
			args.Add("easeType", iTween.EaseType.linear);
			args.Add("x", vector2.x - gameObject.transform.position.x);
			args.Add("y", vector2.y - gameObject.transform.position.y);
			iTween.MoveBy(gameObject, args);
		}
	}

	/// <summary>
	/// 设置Trigger
	/// </summary>
	/// <param name="isTrigger"></param>
	void SetTrigger(bool isTrigger)
	{
		if (gameObject.GetComponent<CircleCollider2D>().isTrigger != isTrigger)
		{
			gameObject.GetComponent<CircleCollider2D>().isTrigger = isTrigger;
		}
	}
}
