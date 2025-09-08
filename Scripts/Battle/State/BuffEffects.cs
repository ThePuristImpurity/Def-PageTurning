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
            
            // 确定最终目标
            Unit finalTarget = DetermineEffectTarget(effect, target, caster);
            
            if (finalTarget == null)
                return;
            
            // 应用效果到目标
            ApplyEffectToTarget(effect, finalTarget, actualValue);
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
                    // 百分比值（0-1范围）
                    baseValue = baseValue / 100f;
                    break;
                    
                case EffectValueType.BasedOnAttack:
                    // 基于施放者攻击力
                    baseValue = caster != null ? baseValue * caster.attackPower : baseValue;
                    break;
                    
                case EffectValueType.BasedOnDefense:
                    // 基于施放者防御力
                    baseValue = caster != null ? baseValue * caster.defensePower : baseValue;
                    break;
                    
                case EffectValueType.BasedOnMaxHealth:
                    // 基于目标最大生命值
                    baseValue = baseValue * target.maxHealth;
                    break;
                    
                case EffectValueType.BasedOnCurrentHealth:
                    // 基于目标当前生命值
                    baseValue = baseValue * target.health;
                    break;
                    
                case EffectValueType.BasedOnMissingHealth:
                    // 基于目标已损失生命值
                    baseValue = baseValue * (target.maxHealth - target.health);
                    break;
            }
            
            // 考虑Buff层数影响
            baseValue *= buffStacks;
            
            return baseValue;
        }
        
        /// <summary>
        /// 确定效果作用的最终目标
        /// </summary>
        private static Unit DetermineEffectTarget(BuffEffect effect, Unit target, Unit caster)
        {
            switch (effect.target)
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
        
        /// <summary>
        /// 应用效果到目标单位
        /// </summary>
        private static void ApplyEffectToTarget(BuffEffect effect, Unit target, float value)
        {
            switch (effect.operation)
            {
                case EffectOperation.Add:
                    ApplyAddOperation(effect, target, value);
                    break;
                    
                case EffectOperation.Subtract:
                    ApplySubtractOperation(effect, target, value);
                    break;
                    
                case EffectOperation.Multiply:
                    ApplyMultiplyOperation(effect, target, value);
                    break;
                    
                case EffectOperation.Divide:
                    ApplyDivideOperation(effect, target, value);
                    break;
                    
                case EffectOperation.Set:
                    ApplySetOperation(effect, target, value);
                    break;
                    
                case EffectOperation.AddPercentage:
                    ApplyAddPercentageOperation(effect, target, value);
                    break;
                    
                case EffectOperation.SubtractPercentage:
                    ApplySubtractPercentageOperation(effect, target, value);
                    break;
            }
        }
        
        #region 具体操作实现
        
        /// <summary>
        /// 应用加法操作
        /// </summary>
        private static void ApplyAddOperation(BuffEffect effect, Unit target, float value)
        {
            switch (effect.statType)
            {
                case StatType.Health:
                    target.health += (int)value;
                    break;
                    
                case StatType.Attack:
                    target.attackPower += (int)value;
                    break;
                    
                case StatType.Defense:
                    target.defensePower += (int)value;
                    break;
                    
                case StatType.Speed:
                    target.speed += (int)value;
                    break;
                    
                case StatType.CriticalChance:
                    // 这里需要实现暴击率修改逻辑
                    break;
                    
                case StatType.CriticalDamage:
                    // 这里需要实现暴击伤害修改逻辑
                    break;
                    
                case StatType.Accuracy:
                    // 这里需要实现命中率修改逻辑
                    break;
                    
                case StatType.Evasion:
                    // 这里需要实现闪避率修改逻辑
                    break;
                    
                case StatType.Shield:
                    // 这里需要实现护盾值修改逻辑
                    break;
            }
        }
        
        /// <summary>
        /// 应用减法操作
        /// </summary>
        private static void ApplySubtractOperation(BuffEffect effect, Unit target, float value)
        {
            switch (effect.statType)
            {
                case StatType.Health:
                    target.health -= (int)value;
                    break;
                    
                case StatType.Attack:
                    target.attackPower -= (int)value;
                    break;
                    
                case StatType.Defense:
                    target.defensePower -= (int)value;
                    break;
                    
                case StatType.Speed:
                    target.speed -= (int)value;
                    break;
                    
                case StatType.CriticalChance:
                    // 这里需要实现暴击率修改逻辑
                    break;
                    
                case StatType.CriticalDamage:
                    // 这里需要实现暴击伤害修改逻辑
                    break;
                    
                case StatType.Accuracy:
                    // 这里需要实现命中率修改逻辑
                    break;
                    
                case StatType.Evasion:
                    // 这里需要实现闪避率修改逻辑
                    break;
                    
                case StatType.Shield:
                    // 这里需要实现护盾值修改逻辑
                    break;
            }
        }
        
        /// <summary>
        /// 应用乘法操作
        /// </summary>
        private static void ApplyMultiplyOperation(BuffEffect effect, Unit target, float value)
        {
            switch (effect.statType)
            {
                case StatType.Health:
                    target.health = (int)(target.health * value);
                    break;
                    
                case StatType.Attack:
                    target.attackPower = (int)(target.attackPower * value);
                    break;
                    
                case StatType.Defense:
                    target.defensePower = (int)(target.defensePower * value);
                    break;
                    
                case StatType.Speed:
                    target.speed = (int)(target.speed * value);
                    break;
                    
                case StatType.CriticalChance:
                    // 这里需要实现暴击率修改逻辑
                    break;
                    
                case StatType.CriticalDamage:
                    // 这里需要实现暴击伤害修改逻辑
                    break;
                    
                case StatType.Accuracy:
                    // 这里需要实现命中率修改逻辑
                    break;
                    
                case StatType.Evasion:
                    // 这里需要实现闪避率修改逻辑
                    break;
                    
                case StatType.Shield:
                    // 这里需要实现护盾值修改逻辑
                    break;
            }
        }
        
        /// <summary>
        /// 应用除法操作
        /// </summary>
        private static void ApplyDivideOperation(BuffEffect effect, Unit target, float value)
        {
            if (Mathf.Approximately(value, 0f))
            {
                Debug.LogWarning("除零错误: 效果值不能为零");
                return;
            }
            
            switch (effect.statType)
            {
                case StatType.Health:
                    target.health = (int)(target.health / value);
                    break;
                    
                case StatType.Attack:
                    target.attackPower = (int)(target.attackPower / value);
                    break;
                    
                case StatType.Defense:
                    target.defensePower = (int)(target.defensePower / value);
                    break;
                    
                case StatType.Speed:
                    target.speed = (int)(target.speed / value);
                    break;
                    
                case StatType.CriticalChance:
                    // 这里需要实现暴击率修改逻辑
                    break;
                    
                case StatType.CriticalDamage:
                    // 这里需要实现暴击伤害修改逻辑
                    break;
                    
                case StatType.Accuracy:
                    // 这里需要实现命中率修改逻辑
                    break;
                    
                case StatType.Evasion:
                    // 这里需要实现闪避率修改逻辑
                    break;
                    
                case StatType.Shield:
                    // 这里需要实现护盾值修改逻辑
                    break;
            }
        }
        
        /// <summary>
        /// 应用设置操作
        /// </summary>
        private static void ApplySetOperation(BuffEffect effect, Unit target, float value)
        {
            switch (effect.statType)
            {
                case StatType.Health:
                    target.health = (int)value;
                    break;
                    
                case StatType.Attack:
                    target.attackPower = (int)value;
                    break;
                    
                case StatType.Defense:
                    target.defensePower = (int)value;
                    break;
                    
                case StatType.Speed:
                    target.speed = (int)value;
                    break;
                    
                case StatType.CriticalChance:
                    // 这里需要实现暴击率修改逻辑
                    break;
                    
                case StatType.CriticalDamage:
                    // 这里需要实现暴击伤害修改逻辑
                    break;
                    
                case StatType.Accuracy:
                    // 这里需要实现命中率修改逻辑
                    break;
                    
                case StatType.Evasion:
                    // 这里需要实现闪避率修改逻辑
                    break;
                    
                case StatType.Shield:
                    // 这里需要实现护盾值修改逻辑
                    break;
            }
        }
        
        /// <summary>
        /// 应用增加百分比操作
        /// </summary>
        private static void ApplyAddPercentageOperation(BuffEffect effect, Unit target, float value)
        {
            switch (effect.statType)
            {
                case StatType.Health:
                    target.health += (int)(target.health * value);
                    break;
                    
                case StatType.Attack:
                    target.attackPower += (int)(target.attackPower * value);
                    break;
                    
                case StatType.Defense:
                    target.defensePower += (int)(target.defensePower * value);
                    break;
                    
                case StatType.Speed:
                    target.speed += (int)(target.speed * value);
                    break;
                    
                case StatType.CriticalChance:
                    // 这里需要实现暴击率修改逻辑
                    break;
                    
                case StatType.CriticalDamage:
                    // 这里需要实现暴击伤害修改逻辑
                    break;
                    
                case StatType.Accuracy:
                    // 这里需要实现命中率修改逻辑
                    break;
                    
                case StatType.Evasion:
                    // 这里需要实现闪避率修改逻辑
                    break;
                    
                case StatType.Shield:
                    // 这里需要实现护盾值修改逻辑
                    break;
            }
        }
        
        /// <summary>
        /// 应用减少百分比操作
        /// </summary>
        private static void ApplySubtractPercentageOperation(BuffEffect effect, Unit target, float value)
        {
            switch (effect.statType)
            {
                case StatType.Health:
                    target.health -= (int)(target.health * value);
                    break;
                    
                case StatType.Attack:
                    target.attackPower -= (int)(target.attackPower * value);
                    break;
                    
                case StatType.Defense:
                    target.defensePower -= (int)(target.defensePower * value);
                    break;
                    
                case StatType.Speed:
                    target.speed -= (int)(target.speed * value);
                    break;
                    
                case StatType.CriticalChance:
                    // 这里需要实现暴击率修改逻辑
                    break;
                    
                case StatType.CriticalDamage:
                    // 这里需要实现暴击伤害修改逻辑
                    break;
                    
                case StatType.Accuracy:
                    // 这里需要实现命中率修改逻辑
                    break;
                    
                case StatType.Evasion:
                    // 这里需要实现闪避率修改逻辑
                    break;
                    
                case StatType.Shield:
                    // 这里需要实现护盾值修改逻辑
                    break;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 临时效果管理器，负责管理临时效果并在回合结束时恢复
    /// </summary>
    public class TemporaryEffectManager
    {
        private Unit _owner;
        private Dictionary<StatType, float> _originalValues = new Dictionary<StatType, float>();
        private Dictionary<StatType, float> _temporaryModifications = new Dictionary<StatType, float>();
        
        public TemporaryEffectManager(Unit owner)
        {
            _owner = owner;
        }
        
        /// <summary>
        /// 记录临时修改
        /// </summary>
        public void RecordTemporaryModification(StatType statType, float originalValue, float modificationAmount)
        {
            if (!_originalValues.ContainsKey(statType))
            {
                _originalValues[statType] = originalValue;
            }
            
            if (!_temporaryModifications.ContainsKey(statType))
            {
                _temporaryModifications[statType] = 0f;
            }
            
            _temporaryModifications[statType] += modificationAmount;
        }
        
        /// <summary>
        /// 恢复所有临时效果
        /// </summary>
        public void RestoreAllTemporaryEffects()
        {
            foreach (var kvp in _temporaryModifications)
            {
                StatType statType = kvp.Key;
                float modification = kvp.Value;
                
                switch (statType)
                {
                    case StatType.Health:
                        _owner.health = (int)(_owner.health - modification);
                        break;
                        
                    case StatType.Attack:
                        _owner.attackPower = (int)(_owner.attackPower - modification);
                        break;
                        
                    case StatType.Defense:
                        _owner.defensePower = (int)(_owner.defensePower - modification);
                        break;
                        
                    case StatType.Speed:
                        _owner.speed = (int)(_owner.speed - modification);
                        break;
                        
                    // 其他属性类型的恢复逻辑...
                }
            }
            
            _originalValues.Clear();
            _temporaryModifications.Clear();
        }
    }
}