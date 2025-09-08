// TestUnit.cs
using UnityEngine;
using BuffSystem;

public class TestUnit : Unit
{
    [Header("基础属性")]
    public int testHealth = 100;
    public int testMaxHealth = 100;
    public int testAttackPower = 20;
    public int testDefensePower = 10;
    public int testSpeed = 15;

    protected override void Start()
    {
        base.Start();
        
        // 直接使用基类的 BuffManager，订阅事件用于调试
        BuffManager.OnBuffAdded += OnBuffAdded;
        BuffManager.OnBuffRemoved += OnBuffRemoved;
        
        Debug.Log($"{unitName} 初始化完成，生命值: {testHealth}");
    }

    private void OnBuffAdded(BuffInstance buff)
    {
        Debug.Log($"🟢 {unitName} 获得: {buff.BuffData.buffName}");
    }

    private void OnBuffRemoved(BuffInstance buff)
    {
        Debug.Log($"🔴 {unitName} 移除: {buff.BuffData.buffName}");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (BuffManager != null)
        {
            BuffManager.OnBuffAdded -= OnBuffAdded;
            BuffManager.OnBuffRemoved -= OnBuffRemoved;
        }
    }
}