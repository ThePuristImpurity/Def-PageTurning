// Scripts/Battle/State/BuffInstance.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    /// <summary>
    /// Buff实例类，表示一个Buff在单位身上的具体实例
    /// </summary>
    public class BuffInstance
    {
        // Buff数据引用
        private BuffData _buffData;
        
        // Buff持有者和施放者
        private Unit _owner;
        private Unit _caster;
        
        // Buff状态信息
        private int _currentStacks;
        private int _remainingDuration;
        private bool _isActive;
        
        // 效果延迟计时器（键为效果索引，值为剩余延迟回合数）
        private Dictionary<int, int> _effectDelayCounters = new Dictionary<int, int>();
        
        // 临时效果管理器
        private TemporaryEffectManager _tempEffectManager;
        
        /// <summary>
        /// Buff数据
        /// </summary>
        public BuffData BuffData => _buffData;
        
        /// <summary>
        /// 当前层数
        /// </summary>
        public int CurrentStacks => _currentStacks;
        
        /// <summary>
        /// 剩余持续时间
        /// </summary>
        public int RemainingDuration => _remainingDuration;
        
        /// <summary>
        /// 是否活跃
        /// </summary>
        public bool IsActive => _isActive;
        
        /// <summary>
        /// 是否已过期
        /// </summary>
        public bool IsExpired => _buffData.effectType != EffectType.Permanent && _remainingDuration <= 0;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BuffInstance(BuffData buffData, Unit owner, Unit caster, int initialStacks = 1)
        {
            _buffData = buffData;
            _owner = owner;
            _caster = caster;
            _currentStacks = Mathf.Clamp(initialStacks, 1, buffData.maxStacks);
            _remainingDuration = buffData.duration;
            _isActive = true;
            
            // 初始化临时效果管理器
            _tempEffectManager = new TemporaryEffectManager(owner);
            
            // 初始化效果延迟计数器
            for (int i = 0; i < buffData.effects.Count; i++)
            {
                _effectDelayCounters[i] = buffData.effects[i].delayTurns;
            }
            
            Debug.Log($"创建Buff实例: {buffData.buffName} (层数: {_currentStacks}, 持续时间: {_remainingDuration})");
        }
        
        /// <summary>
        /// 触发指定时机的Buff效果
        /// </summary>
        public void TriggerEffects(ApplyTiming timing, Unit target = null, float damageAmount = 0f)
        {
            if (!_isActive) return;
            
            for (int i = 0; i < _buffData.effects.Count; i++)
            {
                var effect = _buffData.effects[i];
                
                if (effect.timing == timing)
                {
                    // 检查延迟
                    if (_effectDelayCounters[i] > 0)
                    {
                        _effectDelayCounters[i]--;
                        continue;
                    }
                    
                    // 应用效果
                    ApplyEffect(effect, target, damageAmount);
                }
            }
        }
        
        /// <summary>
        /// 应用单个效果
        /// </summary>
        public void ApplyEffect(BuffEffect effect, Unit target = null, float damageAmount = 0f)
        {
            if (!_isActive) return;
            
            // 使用默认目标（Buff持有者）
            Unit effectTarget = target ?? _owner;
            
            // 应用效果
            BuffEffectApplier.ApplyEffect(effect, effectTarget, _caster, _currentStacks);
            
            Debug.Log($"{_owner.unitName} 的 {_buffData.buffName} 触发效果: {effect.timing}");
        }
        
        /// <summary>
        /// 更新Buff持续时间
        /// </summary>
        public void UpdateDuration()
        {
            if (_buffData.effectType == EffectType.Permanent) return;
            
            _remainingDuration--;
            
            if (_remainingDuration <= 0)
            {
                _isActive = false;
                Debug.Log($"{_buffData.buffName} 已过期");
            }
        }
        
        /// <summary>
        /// 刷新Buff持续时间
        /// </summary>
        public void RefreshDuration()
        {
            if (_buffData.effectType == EffectType.Permanent) return;
            
            _remainingDuration = _buffData.duration;
            Debug.Log($"{_buffData.buffName} 持续时间已刷新");
        }
        
        /// <summary>
        /// 添加Buff层数
        /// </summary>
        public void AddStacks(int stacksToAdd, bool addToDuration = false)
        {
            int newStacks = _currentStacks + stacksToAdd;
            _currentStacks = Mathf.Clamp(newStacks, 1, _buffData.maxStacks);
            
            if (addToDuration)
            {
                _remainingDuration += _buffData.duration;
            }
            
            Debug.Log($"{_buffData.buffName} 层数增加至 {_currentStacks}");
        }
        
        /// <summary>
        /// 减少Buff层数
        /// </summary>
        public void RemoveStacks(int stacksToRemove)
        {
            _currentStacks = Mathf.Max(1, _currentStacks - stacksToRemove);
            
            if (_currentStacks <= 0)
            {
                _isActive = false;
            }
            
            Debug.Log($"{_buffData.buffName} 层数减少至 {_currentStacks}");
        }
        
        /// <summary>
        /// 强制移除Buff（触发移除效果）
        /// </summary>
        public void ForceRemove()
        {
            // 触发移除效果
            TriggerEffects(ApplyTiming.OnRemove);
            
            // 恢复所有临时效果
            _tempEffectManager.RestoreAllTemporaryEffects();
            
            _isActive = false;
            Debug.Log($"{_buffData.buffName} 被强制移除");
        }
        
        /// <summary>
        /// 获取Buff的显示信息
        /// </summary>
        public string GetDisplayInfo()
        {
            string info = _buffData.buffName;
            
            // 添加层数信息
            if (_buffData.maxStacks > 1)
            {
                info += $" ({_currentStacks})";
            }
            
            // 添加持续时间信息
            if (_buffData.effectType != EffectType.Permanent && _remainingDuration > 0)
            {
                info += $"\n{_remainingDuration}回合";
            }
            
            return info;
        }
        
        /// <summary>
        /// 检查是否拥有指定状态
        /// </summary>
        public bool HasState(BuffState state)
        {
            return (_buffData.grantedStates & state) != 0;
        }
        
        /// <summary>
        /// 重置所有效果延迟计数器
        /// </summary>
        public void ResetAllEffectDelays()
        {
            for (int i = 0; i < _buffData.effects.Count; i++)
            {
                _effectDelayCounters[i] = _buffData.effects[i].delayTurns;
            }
        }
        
        /// <summary>
        /// 重置指定效果的延迟计数器
        /// </summary>
        public void ResetEffectDelay(int effectIndex)
        {
            if (effectIndex >= 0 && effectIndex < _buffData.effects.Count)
            {
                _effectDelayCounters[effectIndex] = _buffData.effects[effectIndex].delayTurns;
            }
        }
    }
}