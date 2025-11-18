using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageColliderAction : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject gameObject = collision.gameObject;
        if (gameObject.tag.Equals("Player") && gameObject.GetComponent<ImageColliderAction>() == null)
        {
            collision.gameObject.GetComponent<ImageMoveAction>().AddScaleAnim();
        }
    }
}
