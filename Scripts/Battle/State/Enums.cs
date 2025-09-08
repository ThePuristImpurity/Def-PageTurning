// Scripts/Battle/State/Enums.cs
using System;

namespace BuffSystem
{
    // Buff效果类型枚举
    public enum EffectType
    {
        Instant,        // 立即生效型：施加后立即触发一次效果
        OverTime,       // 持续生效型：每回合或特定时机触发效果
        Permanent       // 永久生效型：持续生效直到被主动移除
    }

    // Buff效果应用时机枚举
    public enum ApplyTiming
    {
        OnApply,               // 施加时：Buff被施加到单位时触发
        OnRemove,              // 移除时：Buff被移除时触发
        OnTurnStart,           // 回合开始：单位回合开始时触发
        OnTurnEnd,             // 回合结束：单位回合结束时触发
        OnAttack,              // 攻击时：单位发起攻击时触发
        OnBeingAttacked,       // 受击时：单位被攻击时触发
        OnDefend,              // 防御时：单位使用防御时触发
        OnDodge,               // 闪避时：单位使用闪避时触发
        OnSkill,               // 技能时：单位使用任意技能时触发
        OnHit,                 // 被击中时：单位受到伤害时触发
        OnHeal,                // 治疗时：单位受到治疗时触发
        OnDamage,              // 造成伤害时：单位造成伤害时触发
        OnKill,                // 击杀时：单位击杀目标时触发
        OnDeath                // 死亡时：单位死亡时触发
    }

    // Buff效果目标枚举
    public enum EffectTarget
    {
        Self,           // 对自己生效：Buff持有者本身
        Caster,         // 对施放者生效：施加Buff的单位
        Target,         // 对目标生效：Buff的目标单位
        AllAllies,      // 对所有友方生效：目标的所有盟友
        AllEnemies,     // 对所有敌方生效：目标的所有敌人
        RandomAlly,     // 随机友方：随机一个盟友
        RandomEnemy     // 随机敌方：随机一个敌人
    }

    // 效果操作类型枚举
    public enum EffectOperation
    {
        Add,            // 增加值：直接增加数值
        Subtract,       // 减少值：直接减少数值
        Multiply,       // 乘以值：乘以系数
        Divide,         // 除以值：除以系数
        Set,            // 设置值：直接设置数值
        AddPercentage,  // 增加百分比：按百分比增加
        SubtractPercentage // 减少百分比：按百分比减少
    }

    // 效果数值类型枚举
    public enum EffectValueType
    {
        Flat,                   // 固定值：使用固定的数值
        Percentage,             // 百分比：使用百分比数值（0-100）
        BasedOnAttack,          // 基于攻击力：基于单位的攻击力计算
        BasedOnDefense,         // 基于防御力：基于单位的防御力计算
        BasedOnMaxHealth,       // 基于最大生命值：基于单位的最大生命值计算
        BasedOnCurrentHealth,   // 基于当前生命值：基于单位的当前生命值计算
        BasedOnMissingHealth    // 基于已损失生命值：基于单位的已损失生命值计算
    }

    // 属性类型枚举
    public enum StatType
    {
        Health,         // 生命值
        Attack,         // 攻击力
        Defense,        // 防御力
        Speed,          // 速度
        CriticalChance, // 暴击率
        CriticalDamage, // 暴击伤害
        Accuracy,       // 命中率
        Evasion,        // 闪避率
        Shield          // 护盾值
    }

    // 状态标志枚举（使用Flags特性支持多状态组合）
    [Flags]
    public enum BuffState
    {
        None = 0,
        Stunned = 1 << 0,       // 眩晕状态：无法行动
        Silenced = 1 << 1,      // 沉默状态：无法使用技能
        Invincible = 1 << 2,    // 无敌状态：免疫所有伤害
        Taunted = 1 << 3,       // 嘲讽状态：强制攻击特定目标
        Frozen = 1 << 4,        // 冰冻状态：无法行动且受到额外伤害
        Blinded = 1 << 5,       // 致盲状态：命中率降低
        Poisoned = 1 << 6,      // 中毒状态：每回合持续伤害
        Burning = 1 << 7,       // 燃烧状态：每回合持续伤害且可扩散
        Bleeding = 1 << 8       // 流血状态：每回合持续伤害且治疗效果降低
    }

    // Buff分类枚举
    public enum BuffCategory
    {
        Buff,           // 增益效果：正面效果
        Debuff,         // 减益效果：负面效果
        Neutral         // 中性效果：特殊效果
    }

    // Buff叠加规则枚举
    public enum StackingRule
    {
        None,           // 不可叠加
        Refresh,        // 刷新持续时间
        StackDuration,  // 叠加持续时间
        StackIntensity, // 叠加效果强度
        StackBoth       // 同时叠加持续时间和强度
    }
}