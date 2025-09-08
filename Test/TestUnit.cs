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

    // 添加属性访问器
    public int TestHealth
    {
        get => testHealth;
        set => testHealth = Mathf.Clamp(value, 0, testMaxHealth);
    }

    public int TestAttackPower
    {
        get => testAttackPower;
        set => testAttackPower = Mathf.Max(0, value);
    }

    void Start()
    {
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

    void OnDestroy()
    {
        if (BuffManager != null)
        {
            BuffManager.OnBuffAdded -= OnBuffAdded;
            BuffManager.OnBuffRemoved -= OnBuffRemoved;
        }
    }
}