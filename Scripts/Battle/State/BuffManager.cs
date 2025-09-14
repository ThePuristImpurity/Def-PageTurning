// Scripts/Battle/State/BuffManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    /// <summary>
    /// Buff管理器，负责管理单位身上的所有Buff实例
    /// </summary>
    public class BuffManager : MonoBehaviour
    {
        // 持有该Buff管理器的战斗单位
        private Unit _owner;
        
        // 当前单位身上的所有Buff实例
        private Dictionary<string, BuffInstance> _activeBuffs = new Dictionary<string, BuffInstance>();
        
        // 按触发时机分组的Buff列表，用于快速查找
        private Dictionary<ApplyTiming, List<BuffInstance>> _buffsByTiming = new Dictionary<ApplyTiming, List<BuffInstance>>();
        
        // 状态标志位（所有Buff状态的叠加结果）
        private BuffState _currentStateFlags = BuffState.None;
        
        // 属性修改器缓存（用于优化性能）
        private Dictionary<StatType, float> _statModifiers = new Dictionary<StatType, float>();
        
        // 事件：当Buff被添加时触发
        public event Action<BuffInstance> OnBuffAdded;
        
        // 事件：当Buff被移除时触发
        public event Action<BuffInstance> OnBuffRemoved;
        
        // 事件：当Buff状态变化时触发
        public event Action<BuffState> OnStateChanged;
        
        /// <summary>
        /// 构造函数初始化Buff管理器
        /// </summary>
        public void Initialize(Unit owner)
        {
            _owner = owner;
            
            // 初始化按时机分组的字典
            foreach (ApplyTiming timing in Enum.GetValues(typeof(ApplyTiming)))
            {
                _buffsByTiming[timing] = new List<BuffInstance>();
            }
            
            // 初始化属性修改器
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                _statModifiers[statType] = 0f;
            }
        }
        
        /// <summary>
        /// 添加Buff到单位
        /// </summary>
        public bool AddBuff(BuffData buffData, Unit caster, int stacks = 1)
        {
            if (buffData == null || !buffData.IsValid())
            {
                Debug.LogWarning("尝试添加无效的Buff数据");
                return false;
            }
            
            // 检查抵抗
            if (buffData.canBeResisted && CheckResist(buffData))
            {
                Debug.Log($"{_owner.unitName} 抵抗了 {buffData.buffName}");
                return false;
            }
            
            // 检查冲突
            if (HasConflictingBuff(buffData))
            {
                Debug.Log($"{_owner.unitName} 已有冲突的Buff，无法添加 {buffData.buffName}");
                return false;
            }
            
            // 检查依赖
            if (!CheckDependencies(buffData))
            {
                Debug.Log($"{_owner.unitName} 缺少依赖的Buff，无法添加 {buffData.buffName}");
                return false;
            }
            
            string buffId = buffData.buffId;
            
            // 处理已存在的Buff
            if (_activeBuffs.TryGetValue(buffId, out BuffInstance existingBuff))
            {
                return HandleExistingBuff(existingBuff, stacks);
            }
            
            // 创建新的Buff实例
            BuffInstance newBuff = new BuffInstance(buffData, _owner, caster, stacks);
            _activeBuffs[buffId] = newBuff;
            
            // 添加到按时机分组的列表
            foreach (var effect in buffData.effects)
            {
                _buffsByTiming[effect.timing].Add(newBuff);
            }
            
            // 应用Buff效果
            ApplyBuffEffects(newBuff, ApplyTiming.OnApply);
            
            // 更新状态标志
            UpdateStateFlags();
            
            // 触发事件
            OnBuffAdded?.Invoke(newBuff);
            
            Debug.Log($"{_owner.unitName} 获得了 {buffData.buffName} (层数: {stacks})");
            return true;
        }
        
        /// <summary>
        /// 移除单位身上的Buff
        /// </summary>
        public bool RemoveBuff(string buffId, bool triggerRemoveEffects = true)
        {
            if (_activeBuffs.TryGetValue(buffId, out BuffInstance buff))
            {
                // 触发移除效果
                if (triggerRemoveEffects)
                {
                    ApplyBuffEffects(buff, ApplyTiming.OnRemove);
                }
                
                // 从所有分组中移除
                foreach (var effect in buff.BuffData.effects)
                {
                    _buffsByTiming[effect.timing].Remove(buff);
                }
                
                // 从主字典中移除
                _activeBuffs.Remove(buffId);
                
                // 更新状态标志
                UpdateStateFlags();
                
                // 触发事件
                OnBuffRemoved?.Invoke(buff);
                
                Debug.Log($"{_owner.unitName} 移除了 {buff.BuffData.buffName}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 移除所有指定分类的Buff
        /// </summary>
        public void RemoveBuffsByCategory(BuffCategory category, bool triggerRemoveEffects = true)
        {
            List<string> buffsToRemove = new List<string>();
            
            foreach (var kvp in _activeBuffs)
            {
                if (kvp.Value.BuffData.category == category)
                {
                    buffsToRemove.Add(kvp.Key);
                }
            }
            
            foreach (string buffId in buffsToRemove)
            {
                RemoveBuff(buffId, triggerRemoveEffects);
            }
        }
        
        /// <summary>
        /// 触发指定时机的Buff效果
        /// </summary>
        public void TriggerBuffEffects(ApplyTiming timing, float damageAmount = 0f)
        {
            if (!_buffsByTiming.ContainsKey(timing))
                return;
            
            var buffsToTrigger = new List<BuffInstance>(_buffsByTiming[timing]);
            
            foreach (var buff in buffsToTrigger)
            {
                if (buff.IsActive && (buff.p_owner == this))//buff活跃且buff的持有者与自身相同时，以buff的效果的目标为目标触发效果
                {
                    buff.TriggerEffects(timing, damageAmount);
                }
            }
        }
        
        /// <summary>
        /// 每回合更新Buff状态
        /// </summary>
        public void UpdateBuffsOnTurnEnd()
        {
            List<string> expiredBuffs = new List<string>();
            
            foreach (var kvp in _activeBuffs)
            {
                var buff = kvp.Value;
                
                // 更新持续时间
                buff.UpdateDuration();
                
                // 检查是否过期
                if (buff.IsExpired)
                {
                    expiredBuffs.Add(kvp.Key);
                }
            }
            
            // 移除过期的Buff
            foreach (string buffId in expiredBuffs)
            {
                RemoveBuff(buffId);
            }

            // 触发回合结束效果
            TriggerBuffEffects(ApplyTiming.OnTurnEnd);
        }
        
        /// <summary>
        /// 获取指定Buff的层数
        /// </summary>
        public int GetBuffStacks(string buffId)
        {
            if (_activeBuffs.TryGetValue(buffId, out BuffInstance buff))
            {
                return buff.CurrentStacks;
            }
            return 0;
        }
        
        /// <summary>
        /// 检查是否拥有指定Buff
        /// </summary>
        public bool HasBuff(string buffId)
        {
            return _activeBuffs.ContainsKey(buffId);
        }
        
        /// <summary>
        /// 检查是否拥有指定状态的Buff
        /// </summary>
        public bool HasState(BuffState state)
        {
            return (_currentStateFlags & state) != 0;
        }
        
        /// <summary>
        /// 获取属性修改值
        /// </summary>
        public float GetStatModifier(StatType statType)
        {
            return _statModifiers.ContainsKey(statType) ? _statModifiers[statType] : 0f;
        }
        
        /// <summary>
        /// 获取所有活跃的Buff
        /// </summary>
        public IEnumerable<BuffInstance> GetAllActiveBuffs()
        {
            return _activeBuffs.Values;
        }
        
        /// <summary>
        /// 清空所有Buff
        /// </summary>
        public void ClearAllBuffs(bool triggerRemoveEffects = false)
        {
            List<string> buffIds = new List<string>(_activeBuffs.Keys);
            
            foreach (string buffId in buffIds)
            {
                RemoveBuff(buffId, triggerRemoveEffects);
            }
        }
        
        #region Private Methods
        
        /// <summary>
        /// 检查是否抵抗Buff
        /// </summary>
        private bool CheckResist(BuffData buffData)
        {
            float resistChance = buffData.baseResistChance;
            // 这里可以添加基于单位属性的抵抗计算
            return UnityEngine.Random.value < resistChance;
        }
        
        /// <summary>
        /// 检查是否有冲突的Buff
        /// </summary>
        private bool HasConflictingBuff(BuffData newBuffData)
        {
            foreach (var conflictingBuffId in newBuffData.conflictingBuffs)
            {
                if (_activeBuffs.ContainsKey(conflictingBuffId))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 检查依赖的Buff是否存在
        /// </summary>
        private bool CheckDependencies(BuffData newBuffData)
        {
            foreach (var requiredBuffId in newBuffData.requiredBuffs)
            {
                if (!_activeBuffs.ContainsKey(requiredBuffId))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 处理已存在的Buff（叠加或刷新）
        /// </summary>
        private bool HandleExistingBuff(BuffInstance existingBuff, int newStacks)
        {
            var data = existingBuff.BuffData;
            
            switch (data.stackingRule)
            {
                case StackingRule.None:
                    return false; // 不可叠加
                    
                case StackingRule.Refresh:
                    existingBuff.RefreshDuration();
                    return true;
                    
                case StackingRule.StackDuration:
                    existingBuff.AddStacks(newStacks, true);
                    return true;
                    
                case StackingRule.StackIntensity:
                    existingBuff.AddStacks(newStacks, false);
                    return true;
                    
                case StackingRule.StackBoth:
                    existingBuff.AddStacks(newStacks, true);
                    return true;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 应用Buff效果
        /// </summary>
        private void ApplyBuffEffects(BuffInstance buff, ApplyTiming timing)
        {
            foreach (var effect in buff.BuffData.effects)
            {
                if (effect.timing == timing)
                {
                    // 这里会调用BuffInstance来应用具体效果
                    buff.ApplyEffect(effect,effect.target);
                }
            }
        }
        
        /// <summary>
        /// 更新状态标志位
        /// </summary>
        private void UpdateStateFlags()
        {
            BuffState newState = BuffState.None;
            
            foreach (var buff in _activeBuffs.Values)
            {
                newState |= buff.BuffData.grantedStates;
            }
            
            if (_currentStateFlags != newState)
            {
                _currentStateFlags = newState;
                OnStateChanged?.Invoke(newState);
            }
        }
        
        #endregion
    }
}