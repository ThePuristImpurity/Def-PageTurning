// Scripts/Battle/State/BuffData.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuffSystem
{
    // 单条效果配置类（Serializable使其在Inspector中可编辑）
    [System.Serializable]
    public class BuffEffect
    {
        [NonSerialized]
        public BuffData ownerBuffData;

        [Tooltip("效果触发时机")] 
        public ApplyTiming timing = ApplyTiming.OnApply;
        
        [Tooltip("效果作用目标")] 
        public EffectTarget target = EffectTarget.Self;
        
        [Tooltip("数值操作类型")] 
        public EffectOperation operation = EffectOperation.Add;
        
        [Tooltip("数值计算方式")] 
        public EffectValueType valueType = EffectValueType.Flat;

        [Tooltip("数值计算依赖目标*")]
        public EffectTarget dependencyTarget = EffectTarget.Self;

        [Tooltip("数值计算依赖属性类型")]
        public StatType dependencyStatType = StatType.BaseHealth;
        
        [Tooltip("效果数值")] 
        public float value = 0f;
        
        [Tooltip("影响的属性类型")] 
        public StatType statType = StatType.BaseHealth;
        
        [Tooltip("效果延迟回合数（0表示立即生效）")] 
        public int delayTurns = 0;
        
        [Tooltip("效果触发概率（0-1）")] 
        [Range(0f, 1f)] 
        public float triggerChance = 1f;
    }

    // Buff可视化设置类
    [System.Serializable]
    public class BuffVisuals
    {
        [Tooltip("Buff图标")] 
        public Sprite icon;
        
        [Tooltip("Buff颜色")] 
        public Color color = Color.white;
        
        [Tooltip("Buff在角色身上的视觉特效")] 
        public GameObject visualEffect;
        
        [Tooltip("Buff叠加时的显示方式")] 
        public string stackDisplayFormat = "{0}";
        
        [Tooltip("Buff持续时间显示格式")] 
        public string durationDisplayFormat = "{0}";
    }

    // Buff数据基类（ScriptableObject用于创建数据资产文件）
    [CreateAssetMenu(fileName = "NewBuff", menuName = "Buff System/Buff Data")]
    public class BuffData : ScriptableObject
    {
        [Header("🔹 基础信息")]
        [Tooltip("Buff唯一标识符（必需）")] 
        public string buffId = "new_buff";
        
        [Tooltip("Buff显示名称")] 
        public string buffName = "新Buff";
        
        [Tooltip("Buff描述")] 
        [TextArea(2, 4)] 
        public string description = "Buff效果描述";
        
        [Tooltip("Buff分类")] 
        public BuffCategory category = BuffCategory.Neutral;
        
        [Header("🔹 持续时间设置")]
        [Tooltip("Buff效果类型")] 
        public EffectType effectType = EffectType.OverTime;
        
        [Tooltip("持续回合数（0表示永久）")] 
        public int duration = 3;
        
        [Tooltip("最大叠加层数")] 
        public int maxStacks = 1;
        
        [Tooltip("叠加规则")] 
        public StackingRule stackingRule = StackingRule.Refresh;
        
        [Header("🔹 效果配置")]
        [Tooltip("Buff效果列表")] 
        public List<BuffEffect> effects = new List<BuffEffect>();

        [NonSerialized]
        //引入Buff实例列表
        public List<BuffInstance> OwnedBuffInstances = new List<BuffInstance>();

        //安全的添加Buff实例列表成员的方式
        public bool AddBuff(BuffInstance buff)
        {
            if (buff == null || OwnedBuffInstances.Contains(buff)) return false;
            OwnedBuffInstances.Add(buff);
            return true;
        }
        //安全移除方式
        public bool RemoveBuff(BuffInstance buff)
        {
            if (!OwnedBuffInstances.Remove(buff)) return false;
            return true;
        }
        // 允许外部批量操作，但保持控制
        public void ModifyBuffs(Action<List<BuffInstance>> B_modificationAction)
        {
            B_modificationAction?.Invoke(OwnedBuffInstances);
        }
        
        [Header("🔹 状态标志")]
        [Tooltip("Buff赋予的状态标志")] 
        public BuffState grantedStates = BuffState.None;
        
        [Header("🔹 Buff交互")]
        [Tooltip("需要先有的Buff（依赖关系）")] 
        public List<string> requiredBuffs = new List<string>();
        
        [Tooltip("冲突的Buff（不能同时存在）")] 
        public List<string> conflictingBuffs = new List<string>();
        
        [Tooltip("可被清除的Buff类型")] 
        public List<BuffCategory> dispellableBy = new List<BuffCategory>();
        
        [Header("🔹 可视化设置")]
        [Tooltip("Buff可视化设置")] 
        public BuffVisuals visuals = new BuffVisuals();
        
        [Header("🔹 高级设置")]
        [Tooltip("是否隐藏Buff图标")] 
        public bool isHidden = false;
        
        [Tooltip("是否在死亡后保留")] 
        public bool persistThroughDeath = false;
        
        [Tooltip("是否可被抵抗")] 
        public bool canBeResisted = true;
        
        [Tooltip("基础抵抗概率（0-1）")] 
        [Range(0f, 1f)] 
        public float baseResistChance = 0.1f;
        
        [Tooltip("是否显示浮动文字")] 
        public bool showFloatingText = true;
        
        [Tooltip("浮动文字格式")] 
        public string floatingTextFormat = "{0}";
        
        /// <summary>
        /// 验证Buff数据是否有效
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(buffId))
            {
                Debug.LogError("Buff ID不能为空");
                return false;
            }
            
            if (duration < 0)
            {
                Debug.LogError("持续时间不能为负数");
                return false;
            }
            
            if (maxStacks < 1)
            {
                Debug.LogError("最大叠加层数不能小于1");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取Buff的完整描述（包含数值信息）
        /// </summary>
        public string GetFullDescription()
        {
            string fullDesc = description;
            
            // 替换描述中的占位符
            foreach (var effect in effects)
            {
                string valueStr = effect.value.ToString();
                if (effect.valueType == EffectValueType.Percentage)
                {
                    valueStr = (effect.value * 100).ToString() + "%";
                }
                
                fullDesc = fullDesc.Replace("{value}", valueStr);
                fullDesc = fullDesc.Replace("{stat}", effect.statType.ToString());
            }
            
            // 添加持续时间信息
            if (effectType != EffectType.Permanent && duration > 0)
            {
                fullDesc += $"\n持续{duration}回合";
            }
            
            // 添加叠加信息
            if (maxStacks > 1)
            {
                fullDesc += $"\n最多叠加{maxStacks}层";
            }
            
            return fullDesc;
        }
        
        /// <summary>
        /// 检查是否与另一个Buff冲突
        /// </summary>
        public bool ConflictsWith(BuffData otherBuff)
        {
            return conflictingBuffs.Contains(otherBuff.buffId);
        }
        
        /// <summary>
        /// 检查是否有依赖的Buff
        /// </summary>
        public bool HasDependency(BuffData otherBuff)
        {
            return requiredBuffs.Contains(otherBuff.buffId);
        }
        
        /// <summary>
        /// 检查是否可以被特定类型的Buff清除
        /// </summary>
        public bool CanBeDispelledBy(BuffCategory dispelType)
        {
            return dispellableBy.Contains(dispelType);
        }

        private void OnValidate()
        {
            InitializeEffectOwners();
        }
        
        private void InitializeEffectOwners()
        {
            foreach (var effect in effects)
            {
                effect.ownerBuffData = this;
            }
        }
    }
}