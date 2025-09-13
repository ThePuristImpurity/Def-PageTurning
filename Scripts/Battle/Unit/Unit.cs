using UnityEngine;
using BuffSystem;

public class Unit : MonoBehaviour
{
    //引入隔壁的Buff管理器
    protected BuffManager _buffManager;
    public BuffManager BuffManager
    {
        get => _buffManager;
        set => _buffManager=value;
    }

    // 背景属性
    public string unitName;

    //战斗属性
    public int _speed = 5;
    public int speed
    {
        get => _speed;
        set => _speed = (int)Mathf.Clamp(value,0,10);
    }//速度，介于0和10之间(包括边缘)
    public int attackPower = 0;//攻击力
    public int defensePower = 0;//防御力
    public int baseHealth = 100;//基础最大生命
    public int maxHealth
    {
        get => (int)(baseHealth * healthMultiplier);
        set => baseHealth = value;
    }
    public int _health = 100;
    public int health
    {
        get => _health;
        set => _health = (int)Mathf.Clamp(value,0,2*maxHealth);
    }//当前生命，最大值是最大生命的2倍
    public int maxSkillPoint = 60;//最大技力
    public int _skillPoint = 0;
    public int skillPoint
    {
        get => _skillPoint;
        set => _skillPoint = (int)Mathf.Clamp(value,0,maxSkillPoint);
    }//当前技力，最大值是最大技力
    public int _shield = 0;
    public int shield
    {
        get => _shield;
        set => _shield = (int)Mathf.Clamp(value,0,2*maxHealth);
    }//护甲值，最大值是最大生命的2倍
    public float attackMultiplier = 1.00f;//攻击倍率
    public float skillMultiplier = 1.00f;//技能倍率
    public float healthMultiplier = 1.00f;//生命倍率
    public float defenseCoefficient => Mathf.Max(Mathf.Min(0.95f,Mathf.Pow(2f,-defensePower*0.1f)),0f);//主动防御承伤系数
    public float damageReduction = 0f;//被动百分比减伤
    public int flatDamageReduction = 0;//被动固定值减伤

    //阵营属性
    //所有阵营属性变量名首字母大写
    public bool IsPlayer = false;
    public bool IsEnemy = false;
    public bool IsNeutral = false;

    //位置属性
    //所有位置属性变量名首字母大写
    public int StandingPosition = -1;//站位，-1代表不在场上，0代表初始玩家位置，1~4代表敌方初始位置，5代表额外站位(暂时不考虑)

    // 行动相关
    public Action plannedAction; // 计划执行的动作

    public bool IsDefending = false;//是否处于防御状态
    public bool IsDodging = false;//是否处于闪避状态

    public bool IsDefeated { get { return health == 0; } }

    void start()
    {
        BuffManager.Initialize(this);// 先初始化 BuffManager

    }
    
    // 决策方法(敌方AI使用)
    public void DecideAction()
    {
        // 根据AI逻辑决定本回合行动
        // 这只是一个示例，实际实现取决于您的游戏规则
        plannedAction = new AttackAction(this, FindNearestEnemyCampUnit());
    }
    
    // 执行行动
    public void ExecuteAction()
    {
        if (plannedAction != null && !IsDefeated)
        {
            plannedAction.Execute();
        }
    }
    
    // 回合重置
    public void ResetForNewRound()
    {
        IsDefending = false; 
        IsDodging = false;
        plannedAction = null;
    }
    
    // 辅助方法：查找最近的敌对阵营单位
    private Unit FindNearestEnemyCampUnit()
    {
        // 实现查找逻辑
        Unit[] allUnits = FindObjectsOfType<Unit>();
        Unit nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;
        
        foreach (Unit unit in allUnits)
        {
            // 根据阵营关系判断是否为敌人
            bool isEnemy = (this.IsPlayer && unit.IsEnemy) || 
                        (this.IsEnemy && unit.IsPlayer) ;
            
            if (isEnemy && !unit.IsDefeated)
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestEnemy = unit;
                }
            }
        }
        return nearestEnemy;
    }
    
    // 接受伤害
    public void TakeDamage(int damage)
    {
        if (shield!=0)
        {
            if(damage >　shield)
            {
                damage -= shield;
                shield = 0;
            }
            else
            {
                shield -= damage;
                damage = 0;
            }
        }
        health -= damage;
        if (health < 0) health = 0;
        
        Debug.Log($"{unitName} 受到 {damage} 点伤害，剩余生命值: {health}");
        
        if (IsDefeated)
        {
            Debug.Log($"{unitName} 被击败!");
            // 处理单位被击败的逻辑
        }
    }
}

// 行动基类
public abstract class Action
{
    protected Unit source;
    
    public Action(Unit source)
    {
        this.source = source;
    }
    
    public abstract void Execute();
}

// 攻击行动
public class AttackAction : Action
{
    private Unit target;
    
    public AttackAction(Unit source, Unit target) : base(source)
    {
        this.target = target;
    }
    
    public override void Execute()
    {
        if (target != null && !target.IsDefeated)
        {
            int damage = CalculateDamage();
            target.TakeDamage(damage);
            Debug.Log($"{source.unitName} 攻击 {target.unitName}，造成 {damage} 点伤害");
        }
    }
    
    private int CalculateDamage()
    {
        float lastAttackPower=source.attackPower*source.attackMultiplier*source.skillMultiplier;//最终攻击力等于基础攻击力连乘攻击倍率和技能倍率
        float speedDifferenceCoefficient=Mathf.Pow((source.speed+target.speed),2f)*0.01f;//速差伤害倍率等于功防双方速度之和的平方乘以0.01
        float damage = lastAttackPower//最终攻击力
        *speedDifferenceCoefficient//乘以速差伤害倍率
        -target.defensePower;//减掉防御得到减伤前伤害
        if (target.IsDefending)
        {
            damage*=target.defenseCoefficient;//得到防御减伤后伤害
        }
        if(target.damageReduction!=0)
        {
            damage*=1-target.damageReduction;//得到被动百分比减伤后伤害
        }
        if(target.flatDamageReduction!=0)
        {
            damage-=target.damageReduction;//得到被动固定值减伤后伤害
        }
        if(target.IsDodging)
        {
            int random=Random.Range(0, 100)+1;
            int dodgeChance=(source.speed-target.speed)*10+50;
            if(dodgeChance>=random)
            {
                Debug.Log($"闪避检定：{random}/{dodgeChance},成功");
                damage=0;
            }
            else Debug.Log($"闪避检定：{random}/{dodgeChance},失败");
        }
        return (int)Mathf.Max(0, damage); // 至少造成0点伤害
    }
}

//防御行动
public class DefendAction:Action
{
    public DefendAction(Unit source):base(source)
    {
        
    }

    public override void Execute()
    {
        source.IsDefending = true;
        Debug.Log($"{source.unitName} 进入防御状态！");
    }
}

//闪避行动
public class DodgeAction:Action
{
    public DodgeAction(Unit source):base(source)
    {
        
    }

    public override void Execute()
    {
        source.IsDodging = true;
        Debug.Log($"{source.unitName} 进入闪避状态！");
    }
}