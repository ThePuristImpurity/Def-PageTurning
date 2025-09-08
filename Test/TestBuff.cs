// TestBuffSystem.cs
using UnityEngine;
using BuffSystem;
using System.Collections;

public class TestBuffSystem : MonoBehaviour
{
    [Header("测试用Buff数据")]
    public BuffData testAttackBuff;      // 攻击力加成Buff
    public BuffData testPoisonDebuff;    // 中毒Debuff
    public BuffData testStunDebuff;      // 眩晕Debuff
    public BuffData testStackingBuff;    // 叠加测试Buff（添加这个字段）

    [Header("测试单位")]
    public Unit testUnit;
    public Unit casterUnit;

    void Start()
    {
        if (testUnit != null && casterUnit != null)
        {
            // 延迟开始测试，确保所有组件初始化完成
            Invoke("RunTests", 0.5f);
        }
    }

    void RunTests()
    {
        Debug.Log("=== 开始Buff系统测试 ===");
        
        StartCoroutine(RunAllTests());
    }

    IEnumerator RunAllTests()
    {
        TestBuffApplication();
        yield return new WaitForSeconds(1f);
        
        TestBuffEffects();
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(TestBuffStacking());
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(TestBuffRemoval());
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(TestStateEffects());
    }

    void TestBuffApplication()
    {
        Debug.Log("🧪 测试1: Buff施加");
        
        bool success = testUnit.BuffManager.AddBuff(testAttackBuff, casterUnit);
        Debug.Log($"攻击Buff施加结果: {success}");
        
        success = testUnit.BuffManager.AddBuff(testPoisonDebuff, casterUnit);
        Debug.Log($"中毒Debuff施加结果: {success}");
    }

    void TestBuffEffects()
    {
        Debug.Log("🧪 测试2: Buff效果触发");
        
        // 模拟回合开始
        testUnit.BuffManager.TriggerBuffEffects(ApplyTiming.OnTurnStart);
        
        // 模拟攻击
        testUnit.BuffManager.TriggerBuffEffects(ApplyTiming.OnAttack);
        
        // 模拟回合结束（中毒效果应该在这里触发）
        testUnit.BuffManager.UpdateBuffsOnTurnEnd();
    }

    IEnumerator TestBuffStacking()
    {
        Debug.Log("🧪 测试3: Buff叠加");
        
        if (testStackingBuff == null)
        {
            Debug.LogWarning("未分配叠加测试Buff，跳过此测试");
            yield break;
        }
        
        // 第一次施加
        bool success1 = testUnit.BuffManager.AddBuff(testStackingBuff, casterUnit, 1);
        Debug.Log($"第一次叠加施加: {success1}");
        
        // 第二次施加
        bool success2 = testUnit.BuffManager.AddBuff(testStackingBuff, casterUnit, 1);
        Debug.Log($"第二次叠加施加: {success2}");
        
        // 检查层数
        int stacks = testUnit.BuffManager.GetBuffStacks(testStackingBuff.buffId);
        Debug.Log($"当前层数: {stacks}");
        
        yield return new WaitForSeconds(1f);
    }

    IEnumerator TestBuffRemoval()
    {
        Debug.Log("🧪 测试4: Buff移除");
        
        // 先添加一个Buff
        testUnit.BuffManager.AddBuff(testAttackBuff, casterUnit);
        
        // 等待一下然后移除
        yield return new WaitForSeconds(1f);
        
        bool removed = testUnit.BuffManager.RemoveBuff(testAttackBuff.buffId);
        Debug.Log($"Buff移除结果: {removed} - 预期: True");
        
        yield return new WaitForSeconds(1f);
    }

    IEnumerator TestStateEffects()
    {
        Debug.Log("🧪 测试5: 状态效果");
        
        if (testStunDebuff == null)
        {
            Debug.LogWarning("未分配状态测试Buff，跳过此测试");
            yield break;
        }
        
        // 施加眩晕Buff
        testUnit.BuffManager.AddBuff(testStunDebuff, casterUnit);
        
        // 检查状态
        bool isStunned = testUnit.BuffManager.HasState(BuffState.Stunned);
        Debug.Log($"是否被眩晕: {isStunned} - 预期: True");
        
        yield return new WaitForSeconds(2f);
        
        // 模拟回合结束，眩晕应该消失
        testUnit.BuffManager.UpdateBuffsOnTurnEnd();
        
        isStunned = testUnit.BuffManager.HasState(BuffState.Stunned);
        Debug.Log($"眩晕状态是否消失: {!isStunned} - 预期: True");
        
        yield return new WaitForSeconds(1f);
    }

    // 在Inspector中显示测试按钮
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 250, 300));
        
        GUILayout.Label("🧪 Buff系统测试");
        
        if (GUILayout.Button("运行所有测试"))
        {
            StartCoroutine(RunAllTests());
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("测试Buff施加"))
        {
            TestBuffApplication();
        }
        
        if (GUILayout.Button("测试效果触发"))
        {
            TestBuffEffects();
        }
        
        if (GUILayout.Button("测试Buff叠加"))
        {
            StartCoroutine(TestBuffStacking());
        }
        
        if (GUILayout.Button("测试Buff移除"))
        {
            StartCoroutine(TestBuffRemoval());
        }
        
        if (GUILayout.Button("测试状态效果"))
        {
            StartCoroutine(TestStateEffects());
        }
        
        if (GUILayout.Button("清除所有Buff"))
        {
            testUnit.BuffManager.ClearAllBuffs();
            Debug.Log("已清除所有Buff");
        }
        
        GUILayout.EndArea();
    }
}