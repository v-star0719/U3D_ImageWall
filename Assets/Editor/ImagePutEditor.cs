using UnityEngine;
using UnityEditor;


public class ImagePutEditor : Editor
{
    /// <summary>
    /// 测试,增加指定照片
    /// </summary>
    [MenuItem("ImageFlow/PutImageForTest")]
    public static void PutImage()
    {
        GameObject sprite = GameObject.Find("Sprite");
        Object[] objects = Selection.objects;
        foreach (var item in objects)
        {
            GameObject gameObject = Instantiate(sprite);
            Texture2D texture2d = Instantiate(item) as Texture2D;
            Sprite sp = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));//注意居中显示采用0.5f值  //创建一个精灵(图片，纹理，二维浮点型坐标)
            SpriteRenderer spr = gameObject.GetComponent<SpriteRenderer>();
            spr.sprite = sp;
            gameObject.name = texture2d.name;
        }
    }

    /// <summary>
    /// 给一个物体下的所有图片新增该效果的物理属性
    /// </summary>
    [MenuItem("ImageFlow/AddImagePhysics")]
    public static void AddImagePhysics()
    {
        Transform[] transforms = Selection.transforms;
        foreach (var item in transforms)
        {
            for (int i = 0; i < item.childCount; i++)
            {
                Transform transform = item.GetChild(i);
                SpriteRenderer spriteRenderer = transform.GetComponent<SpriteRenderer>();
                if (spriteRenderer)
                {
                    Vector3 size = spriteRenderer.bounds.size;
                    float radius = size.x > size.y ? size.y : size.x;
                    Rigidbody2D rigidbody2D = transform.gameObject.AddComponent<Rigidbody2D>();
                    rigidbody2D.useAutoMass = true;
                    rigidbody2D.gravityScale = 0;
                    CircleCollider2D circleCollider2D = transform.gameObject.AddComponent<CircleCollider2D>();
                    circleCollider2D.radius = radius;
                }
                
            }
        }
    }
}
