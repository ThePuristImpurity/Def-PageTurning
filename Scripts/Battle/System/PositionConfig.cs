using UnityEngine;

// 先定义枚举类型
public enum FormationType
{
    Single,     // 单人阵型
    Double,     // 双人阵型  
    Triangle,       // 三人阵型
    Custom          // 自定义阵型
}

[CreateAssetMenu(menuName = "Battle/PositionConfig")]
public class PositionConfig : ScriptableObject
{
    [Header("位置参数")]
    public float positionSpacing = 2f;          // 位置间距
    public float minUnitDistance = 1.5f;        // 最小单位距离
    public Vector2 battlefieldSize = new Vector2(10f, 8f); // 战场尺寸,占位
    
    [Header("阵型配置")]
    public FormationType Single;      // 默认单人
    public Vector2[] formationOffsets;          // 阵型偏移量
}

///////////////////////////////////////////////////////////////////占位