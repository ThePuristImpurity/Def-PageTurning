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

    void Start()
    {
        // 将测试值赋给基类的属性
        maxHealth = testMaxHealth;
        health = testHealth;
        attackPower = testAttackPower;
        defensePower = testDefensePower;
        speed = testSpeed;
        
        // 检查基类是否已经初始化了 BuffManager
        if (BuffManager == null)
        {
            // 如果基类没有初始化，我们自己初始化
            var buffManagerComponent = gameObject.AddComponent<BuffManager>();
            buffManagerComponent.Initialize(this);
        }
        else
        {
            // 如果基类已经初始化，我们可以直接使用
            Debug.Log("使用基类的 BuffManager");
        }
        
        // 订阅事件用于调试
        BuffManager.OnBuffAdded += OnBuffAdded;
        BuffManager.OnBuffRemoved += OnBuffRemoved;
        
        Debug.Log($"{unitName} 初始化完成，生命值: {health}");
    }

    private void OnBuffAdded(BuffInstance buff)
    {
        Debug.Log($"🟢 {unitName} 获得: {buff.BuffData.buffName}");
    }

    private void OnBuffRemoved(BuffInstance buff)
    {
        Debug.Log($"🔴 {unitName} 移除: {buff.BuffData.buffName}");
    }

    void OnDestroy()
    {
        if (BuffManager != null)
        {
            BuffManager.OnBuffAdded -= OnBuffAdded;
            BuffManager.OnBuffRemoved -= OnBuffRemoved;
        }
    }
}