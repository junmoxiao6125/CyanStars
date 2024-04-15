using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// StarsGenerator 用于控制选曲面板背景星星和 Staff 信息的生成、移动。
public class StarsGenerator : MonoBehaviour
{
    [Header("图像配置 Image settings")]
    [Tooltip("星星预制体")]
    public GameObject StarPrefab;
    [Tooltip("准星图像")]
    public GameObject SightPrefab;

    [Header("数值与UI配置 Value & UI settings")]
    [Tooltip("用作基准缩放长宽的 Panel")]
    public GameObject Panel;
    [Tooltip("至少生成几个星星")]
    public int MinStarNum;
    [Tooltip("至多生成几个星星")]
    public int MaxStarNum;
    [Tooltip("星星的透明度至少为")]
    public float MinStarAlpha;
    [Tooltip("星星的透明度至多为")]
    public float MaxStarAlpha;
    [Tooltip("星星的大小至少为")]
    public float MinStarSize;
    [Tooltip("星星的大小至多为")]
    public float MaxStarSize;
    [Tooltip("由视差引起的位移至少为（以1倍屏幕宽度为单位）")]
    public float MinStarParallax;
    [Tooltip("由视差引起的位移至多为（以1倍屏幕宽度为单位）")]
    public float MaxStarParallax;


    [Header("Debug, readonly")]
    [SerializeField]
    [Tooltip("实际生成的星星数量")]
    private int starNum;

    void Start()
    {
        // 生成 Star 预制体
        starNum = Random.Range(MinStarNum, MaxStarNum);
        for (int i = 0; i < starNum; i++)
        {
            // 随机生成位置、透明度、大小、视差灵敏度，并传递给每一个 Star 预制体
            GameObject newStarObject = Instantiate(StarPrefab, transform);
            Star newStar = newStarObject.GetComponent<Star>();
            newStar.XPos = Random.Range(0f, 2f);    // 横向范围为两倍，用于动画时从镜头外向左过渡到镜头内
            newStar.YPos = Random.Range(0f, 1f);
            newStar.Alpha = Random.Range(MinStarAlpha, MaxStarAlpha);
            newStar.Size = Random.Range(MinStarSize, MaxStarSize);
            newStar.Parallax = Random.Range(MinStarParallax, MaxStarParallax);
            newStar.Panel = Panel;
        }
    }
}
