// Scripts/Battle/State/BuffEffects.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    /// <summary>
    /// Buff效果应用器，负责将Buff效果应用到战斗单位上
    /// </summary>
    public static class BuffEffectApplier
    {
        /// <summary>
        /// 应用Buff效果到目标单位
        /// </summary>
        /// <param name="effect">Buff效果配置</param>
        /// <param name="target">目标单位</param>
        /// <param name="caster">施放者单位</param>
        /// <param name="buffStacks">Buff层数</param>
        public static void ApplyEffect(BuffEffect effect, Unit target, Unit caster, int buffStacks = 1)
        {
            // 检查触发概率
            if (UnityEngine.Random.value > effect.triggerChance)
                return;
            // 获取实际效果值（考虑层数影响）
            float actualValue = CalculateEffectValue(effect, target, caster, buffStacks);
            
            // 应用效果到目标
            ApplyEffectToTarget(effect, target, actualValue, buffStacks);
        }
        
        /// <summary>
        /// 计算实际效果值
        /// </summary>
        private static float CalculateEffectValue(BuffEffect effect, Unit target, Unit caster, int buffStacks)
        {
            float baseValue = effect.value;
            
            
            // 根据数值类型计算基础值
            switch (effect.valueType)
            {
                case EffectValueType.Flat:
                    // 使用固定值
                     break;
                     
                case EffectValueType.Percentage:
                    // 百分比值
                     baseValue = baseValue * 0.01f;
                     break;

                case EffectValueType.Reciprocal:
                    // 倒数值
                    baseValue = (baseValue == 0f) ? 0f : (1f / baseValue);
                    break;

                case EffectValueType.BasedOnStatType:
                    Unit statUnit = GetTarget(effect, target, caster);
                    if (statUnit == null) return 0f;
                    float statValue = GetStatType(statUnit, effect.dependencyStatType);
                    baseValue *= statValue;
                    break;
            }
            return baseValue;
        }

        private static Unit GetTarget(BuffEffect effect, Unit target, Unit caster)
        {
            switch (effect.dependencyTarget)
            {
                case EffectTarget.Self:
                    return target;
                    
                case EffectTarget.Caster:
                    return caster;
                    
                case EffectTarget.Target:
                    return target;
                    
                case EffectTarget.AllAllies:
                    // 这里需要实现获取所有盟友的逻辑
                    // 暂时返回目标本身
                    return target;
                    
                case EffectTarget.AllEnemies:
                    // 这里需要实现获取所有敌人的逻辑
                    // 暂时返回目标本身
                    return target;
                    
                case EffectTarget.RandomAlly:
                    // 这里需要实现随机选择盟友的逻辑
                    // 暂时返回目标本身
                    return target;
                    
                case EffectTarget.RandomEnemy:
                    // 这里需要实现随机选择敌人的逻辑
                    // 暂时返回目标本身
                    return target;
                    
                default:
                    return target;
            }
        }
        
        private static float GetStatType(Unit unit, StatType statType)
        {
            if (unit == null) return 0f;
            
            switch (statType)
            {
                case StatType.BaseHealth:
                    return unit.baseHealth;
                    
                case StatType.HealthMultiplier:
                    return unit.healthMultiplier;
                    
                case StatType.AttackPower:
                    return unit.attackPower;
                    
                case StatType.AttackMultiplier:
                    return unit.attackMultiplier;
                    
                case StatType.DefensePower:
                    return unit.defensePower;
                    
                case StatType.Speed:
                    return unit.speed;
                    
                case StatType.Shield:
                    return unit.shield;
                    
                default:
                    Debug.LogWarning($"未知的StatType: {statType}");
                    return 0f;
            }
        }
        
        /// <summary>
        /// 应用效果到目标单位
        /// </summary>
        private static void ApplyEffectToTarget(BuffEffect effect, Unit target, float value, int currentBuffStacks = 1)
        {
            if (target == null) return;
            int buffStacks = 0;
            foreach (BuffInstance ownedBuffInstance in effect.ownerBuffData.OwnedBuffInstances)
            {
                if (ownedBuffInstance.p_owner == target)
                {
                    buffStacks = currentBuffStacks - ownedBuffInstance._currentStacks;
                    break; 
                }
            }

            // 从缓存获取操作委托
            Func<float, float, int, float> operation = GetCachedOperation(effect.operation);
            
            // 应用操作到具体属性
            switch (effect.statType)
            {
                case StatType.BaseHealth:
                    target.baseHealth = (int)operation(target.baseHealth, value, buffStacks);
                    break;

                case StatType.HealthMultiplier:
                    target.healthMultiplier = operation(target.healthMultiplier, value, buffStacks);
                    break;
                    
                case StatType.AttackPower:
                    target.attackPower = (int)operation(target.attackPower, value, buffStacks);
                    break;

                case StatType.AttackMultiplier:
                    target.attackMultiplier = operation(target.attackMultiplier, value, buffStacks);
                    break;
                    
                case StatType.DefensePower:
                    target.defensePower = (int)operation(target.defensePower, value, buffStacks);
                    break;
                    
                case StatType.Speed:
                    target.speed = (int)operation(target.speed, value, buffStacks);
                    break;

                case StatType.Shield:
                    target.shield = (int)operation(target.shield, value, buffStacks);
                    break;
            }
        }

        /// <summary>
        /// 操作委托缓存字典
        /// </summary>
        private static readonly Dictionary<EffectOperation, Func<float, float, int, float>> _operationCache = 
            new Dictionary<EffectOperation, Func<float, float, int, float>>();

        /// <summary>
        /// 从缓存获取操作委托（线程安全）
        /// </summary>
        private static Func<float, float, int, float> GetCachedOperation(EffectOperation operation)
        {
            // 如果缓存未初始化，进行初始化
            if (_operationCache.Count == 0)
            {
                lock (_operationCache)
                {
                    if (_operationCache.Count == 0)
                    {
                        InitializeOperationCache();
                    }
                }
            }
            
            // 从缓存获取委托
            return _operationCache.TryGetValue(operation, out var cachedOperation) 
                ? cachedOperation 
                : (current, val, buffStacks) => current; // 默认操作
        }

        /// <summary>
        /// 初始化操作缓存
        /// </summary>
        private static void InitializeOperationCache()
        {
            _operationCache[EffectOperation.Add] = (current, val, buffStacks) => current + val * buffStacks;
            _operationCache[EffectOperation.Subtract] = (current, val, buffStacks) => current - val * buffStacks;
            _operationCache[EffectOperation.Multiply] = (current, val, buffStacks) => 
                current * SafePower(val, buffStacks);
            _operationCache[EffectOperation.Divide] = (current, val, buffStacks) => 
            {
                if (Mathf.Approximately(val, 0f))
                {
                    Debug.LogWarning("除零错误: 效果值不能为零");
                    return current;
                }
                return current / SafePower(val, buffStacks);
            };
            _operationCache[EffectOperation.Set] = (current, val, buffStacks) => val;
            _operationCache[EffectOperation.AddPercentage] = (current, val, buffStacks) => 
                current * SafePower(1 + val * 0.01f, buffStacks);
            _operationCache[EffectOperation.SubtractPercentage] = (current, val, buffStacks) => 
                current * SafePower(1 - val * 0.01f, buffStacks);
            // 这里需求一个百分比转换前的数字，例如：攻击倍率+200%，addpercentage里应当输入200，substractpercentage里应当输入-200
            // 需要注意的是，一般不建议在需要恢复数值的buff中使用百分比增加减少方法，因为这个方法跟取倒数方法各种意义上都不是很适配
            // 也就是说，如果你需要恢复该buff的影响，你需要手动计算倍率的倒数值
            // 只有当你确定这个数值增加从buff添加的那一刻直到战斗结束将一直存在，才推荐使用百分比增加减少方法
            // 即使如此，这个方法仍然有其应用领域：局外属性增加
        }
        
        /// <summary>
        /// 安全的幂运算
        /// </summary>
        private static float SafePower(float baseValue, int exponent)
        {
            if (Mathf.Approximately(baseValue, 0f) && exponent <= 0)
            {
                Debug.LogWarning("幂运算错误: 底数为零且指数非正");
                return 0f;
            }
            return Mathf.Pow(baseValue, exponent);
        }
    }
}
/*
    /// <summary>
    /// 临时效果管理器，负责管理临时效果并在回合结束时恢复
    /// </summary>
    public class TemporaryEffectManager
    {
        private Unit _owner;
        private Dictionary<StatType, float> _originalValues = new Dictionary<StatType, float>();
        
        public TemporaryEffectManager(Unit owner)
        {
            _owner = owner;
        }
        
        /// <summary>
        /// 在应用任何临时修改前，记录属性的原始值
        /// </summary>
        public void SnapshotOriginalValue(StatType statType)
        {
            // 如果还没有记录过这个属性的原始值，就记录一次
            if (!_originalValues.ContainsKey(statType))
            {
                float originalValue = GetCurrentStatValue(_owner, statType);
                _originalValues[statType] = originalValue;
            }
        }
        
        /// 获取单位当前某个属性的值
        /// </summary>
        private float GetCurrentStatValue(Unit unit, StatType statType)
        {
            // 这个函数应该和 GetStatType 逻辑一致
            switch (statType)
            {
                case StatType.BaseHealth: return unit.baseHealth;
                case StatType.HealthMultiplier: return unit.healthMultiplier;
                case StatType.AttackPower: return unit.attackPower;
                case StatType.AttackMultiplier: return unit.attackMultiplier;
                case StatType.DefensePower: return unit.defensePower;
                case StatType.Speed: return unit.speed;
                case StatType.Shield: return unit.shield;
                default: return 0f;
            }
        }

        /// <summary>
        /// 设置单位某个属性的值（用于恢复）
        /// </summary>
        private void SetStatValue(Unit unit, StatType statType, float value)
        {
            switch (statType)
            {
                case StatType.BaseHealth: unit.baseHealth = (int)value; break;
                case StatType.HealthMultiplier: unit.healthMultiplier = value; break;
                case StatType.AttackPower: unit.attackPower = (int)value; break;
                case StatType.AttackMultiplier: unit.attackMultiplier = value; break;
                case StatType.DefensePower: unit.defensePower = (int)value; break;
                case StatType.Speed: unit.speed = (int)value; break;
                case StatType.Shield: unit.shield = (int)value; break;
            }
        }

        /// <summary>
        /// 恢复所有临时效果：直接将所有被修改过的属性重置为之前记录的原始值
        /// </summary>
        public void RestoreAllTemporaryEffects()
        {
            foreach (var kvp in _originalValues)
            {
                StatType statType = kvp.Key;
                float originalValue = kvp.Value;
                SetStatValue(_owner, statType, originalValue);
            }
            _originalValues.Clear();
        }
    }

*/
//原本有个临时效果，写完发现直接用buffmanager的过期移除造个一回合持续的buff就好......代码不舍得丢，放这吧。