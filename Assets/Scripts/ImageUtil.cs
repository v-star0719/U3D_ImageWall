using UnityEngine;
using System;

public class ImageUtil
{
    public const int IMAGE_ORDER_DEFAULT = 1;
    public const int IMAGE_ORDER_MIN = 0;
    public const int IMAGE_ORDER_MAX = 999;

    /// <summary>
    /// 获取直线和圆的交点
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <param name="k"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Vector2[] GetIntersectionOfCircleAndLine(float r, float c, float d, float k, float b)
    {
        // 其中((x-c) * (x-c) + (y-d) * (y-d)=r*r)和k * x + b=y
        // 已知有两个交点
        Vector2[] vecs = new Vector2[2];

        // 根据以下网页得出交点方程表达式
        // https://www.zybang.com/question/e47d6dd9876192ae16e1f1cf223b58cf.html
        // 网页中表达式为:((x+c) * (x+c) + (y+d) * (y+d)=r*r)
        c = -c;
        d = -d;

        double temp0 = Math.Sqrt((k * k + 1) * r * r - c * c * k * k + (2 * c * d + 2 * b * c) * k - d * d - 2 * b * d - b * b);
        vecs[0].x = (float)(-1 * (temp0 + (d + b) * k + c) / (k * k + 1));
        vecs[1].x = (float)((temp0 - (d + b) * k - c) / (k * k + 1));

        double temp1 = Math.Sqrt(k * k * r * r + r * r - c * c * k * k + 2 * c * d * k + 2 * b * c * k - d * d - 2 * b * d - b * b);
        vecs[0].y = (float)(-1 * (k * (temp1 + c) + d * k * k - b) / (k * k + 1));
        vecs[1].y = (float)(-1 * (k * (c - temp1) + d * k * k - b) / (k * k + 1));

        return vecs;
    }

    /// <summary>
    /// 获取指定x值的圆周上的点,有2个
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Vector2[] GetRoundPoints(float r, float c, float d, float x)
    {
        // 其中(x-c) * (x-c) + (y-d) * (y-d)=r*r
        Vector2[] vecs = new Vector2[2];
        vecs[0].x = vecs[1].x = x;
        double temp = Math.Sqrt(r * r - (x - c) * (x - c));
        vecs[0].y = (float)(d + temp);
        vecs[1].y = (float)(d - temp);
        return vecs;
    }
}
