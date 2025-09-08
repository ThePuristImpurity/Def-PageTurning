using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TurnManager : MonoBehaviour
{
    // 事件系统，用于UI和其他系统监听回合状态变化
    public static event UnityAction<int> OnRoundStart; // 回合开始事件，参数为当前回合数
    public static event UnityAction OnEnemyDecisionComplete; // 敌方决策完成事件
    public static event UnityAction OnRoundExecutionStart; // 回合执行开始事件
    public static event UnityAction OnRoundEnd; // 回合结束事件
    
    // 当前回合数
    private int currentRound = 1;
    
    // 回合状态
    private enum RoundState
    {
        EnemyDecision,   // 敌方决策阶段
        PlayerStrategy,  // 玩家策略选择阶段
        RoundExecution,  // 回合执行阶段(战斗动画)
        RoundEnd         // 回合结束阶段
    }
    
    private RoundState currentState = RoundState.EnemyDecision;//初始化回合状态为敌方决策阶段

    bool IsGameOver = false;//初始化游戏结束判定为假
    
    // 所有参战单位列表
    private List<Unit> allUnits = new List<Unit>();

    //所有参战单位的目标框列表
    private List<GameObject> allTargetNodes = new List<GameObject>();
    
    // 配置参数
    public float enemyDecisionTime = 2f; // 敌方决策时间(秒)
    public float roundExecutionTime = 3f; // 回合执行时间(秒)
    
    // UI引用
    public TextMeshProUGUI AttackButton; // 攻击UI
    public TextMeshProUGUI DefendButton; // 防御UI
    public TextMeshProUGUI DodgeButton; // 闪避UI
    public TextMeshProUGUI SkillButton; // 技能UI
    public TextMeshProUGUI startRoundButton;    // 开始回合按钮

    // 物体引用
    public GameObject AttackBlock; // 攻击方块
    public GameObject DefendBlock; // 防御方块
    public GameObject DodgeBlock; // 闪避方块
    public GameObject SkillBlock; // 技能方块

    // 玩家选择的策略
    private string selectedStrategy = "";

    // 用于目标选择
    private Unit selectedTarget = null;
    private bool isSelectingTarget = false;
    private string pendingStrategy = ""; // 等待选择目标的策略

    private Coroutine targetSelectionCoroutine;//目标选择协程
    private Camera mainCamera;//主相机
    private int unitLayerMask;//层级掩码
    
    void Start()
    {
        // 初始化缓存
        mainCamera = Camera.main;
        unitLayerMask = 1 << LayerMask.NameToLayer("Unit");

        // 收集所有参战单位及其子目标框
        CollectAllUnits();
        CollectAllTargetNodes();

        // 设置所有UI按钮
        SetupAllUIButtons();
    
        // 隐藏UI元素
        SetStrategyUIVisibility(false);
    
        // 开始游戏循环
        StartCoroutine(GameLoop());
    }

    ////////////////////////////////////////////////////////收集所有参战单位及其子目标框/////////////////////////////////////////////////
    //收集所有参战单位
    private void CollectAllUnits()
    {
        // 查找所有单位（可根据标签或其他标识）
        Unit[] foundUnits = FindObjectsOfType<Unit>();
        allUnits.AddRange(foundUnits);
        
        Debug.Log($"找到 {allUnits.Count} 个单位");
        
        // 添加详细日志
        foreach (Unit unit in allUnits)
        {
            Debug.Log($"单位: {unit.name}, 是敌人: {unit.IsEnemy},站位为{unit.StandingPosition}号位");
        }
    }

    private void CollectAllTargetNodes()
    {
        allTargetNodes.Clear();
    
        foreach (Unit unit in allUnits)
        {
            // 在单位的子物体中查找目标框
            FindNodeInChildren(unit.transform);
        }
        
        Debug.Log($"找到 {allTargetNodes.Count} 个目标选择框");
    }

    private void FindNodeInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // 根据名称识别目标框
            if (child.name.Contains("TargetNode"))
            {
                allTargetNodes.Add(child.gameObject);
            }
        }
    }
    /////////////////////////////////////////////////////////////////设置所有UI按钮//////////////////////////////////////////////////////
    // 设置所有UI按钮的点击事件
    private void SetupAllUIButtons()
    {
        SetupStrategyButton(AttackButton, "Attack");
        SetupStrategyButton(DefendButton, "Defend");
        SetupStrategyButton(DodgeButton, "Dodge");
        SetupStrategyButton(SkillButton, "Skill");
        SetupStartRoundButton();
    }

    // 设置策略按钮的通用方法
    private void SetupStrategyButton(TextMeshProUGUI button, string strategyType)
    {
        if (button != null)
        {
            button.raycastTarget = true;
            
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }
            else
            {
                trigger.triggers.Clear();
            }
            
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { 
                SelectStrategy(strategyType);
            });
            
            trigger.triggers.Add(entry);
            
            // 添加碰撞体支持3D射线
            if (button.gameObject.GetComponent<Collider>() == null)
            {
                BoxCollider collider = button.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(button.preferredWidth, button.preferredHeight, 0.1f);
            }
        }
    }

    // 设置开始回合按钮
    private void SetupStartRoundButton()
    {
        if (startRoundButton != null)
        {
            startRoundButton.raycastTarget = true;
            
            EventTrigger trigger = startRoundButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = startRoundButton.gameObject.AddComponent<EventTrigger>();
            }
            else
            {
                trigger.triggers.Clear();
            }
            
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { 
                StartRoundManually();
            });
            
            trigger.triggers.Add(entry);
            
            if (startRoundButton.gameObject.GetComponent<Collider>() == null)
            {
                BoxCollider collider = startRoundButton.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(startRoundButton.preferredWidth, startRoundButton.preferredHeight, 0.1f);
            }
        }
    }

    // 高亮选中的按钮
    private void HighlightSelectedButton(string selectedStrategy)
    {
        // 重置所有按钮颜色
        ResetButtonColors();
        
        // 高亮选中的按钮
        switch (selectedStrategy)
        {
            case "Attack":
                if (AttackButton != null) AttackButton.color = Color.yellow;
                break;
            case "Defend":
                if (DefendButton != null) DefendButton.color = Color.yellow;
                break;
            case "Dodge":
                if (DodgeButton != null) DodgeButton.color = Color.yellow;
                break;
            case "Skill":
                if (SkillButton != null) SkillButton.color = Color.yellow;
                break;
        }
    }

    // 重置所有按钮颜色
    private void ResetButtonColors()
    {
        if (AttackButton != null) AttackButton.color = Color.black;
        if (DefendButton != null) DefendButton.color = Color.black;
        if (DodgeButton != null) DodgeButton.color = Color.black;
        if (SkillButton != null) SkillButton.color = Color.black;
    }

    /////////////////////////////////////////////////////设置策略UI的可见性///////////////////////////////////////////
    // 设置策略UI的可见性
    private void SetStrategyUIVisibility(bool visible)
    {
         // 设置UI文字的可见性
        if (AttackButton != null) AttackButton.gameObject.SetActive(visible);
        if (DefendButton != null) DefendButton.gameObject.SetActive(visible);
        if (DodgeButton != null) DodgeButton.gameObject.SetActive(visible);
        if (SkillButton != null) SkillButton.gameObject.SetActive(visible);
        if (startRoundButton != null) startRoundButton.gameObject.SetActive(visible);
        
        // 设置UI方块的可见性
        if (AttackBlock != null) AttackBlock.SetActive(visible);
        if (DefendBlock != null) DefendBlock.SetActive(visible);
        if (DodgeBlock != null) DodgeBlock.SetActive(visible);
        if (SkillBlock != null) SkillBlock.SetActive(visible);
    }
    
    ///////////////////////////////////////////////////////开始游戏循环////////////////////////////////////////////////
    // 主游戏循环
    private IEnumerator GameLoop()
    {
        while(true)
        {
            switch(currentState)
            {
                case RoundState.EnemyDecision:
                    yield return StartCoroutine(EnemyDecisionPhase());// 敌方决策阶段
                    break;
                    
                case RoundState.PlayerStrategy:
                    yield return StartCoroutine(PlayerStrategyPhase());// 玩家策略阶段
                    break;
                    
                case RoundState.RoundExecution:
                    yield return StartCoroutine(RoundExecutionPhase());// 回合执行阶段
                    break;
                    
                case RoundState.RoundEnd:
                    yield return StartCoroutine(RoundEndPhase());// 回合结束阶段
                    break;
            }
            
            // 短暂延迟，避免过度占用CPU
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    ////////////////////////////////////////////////////////////敌方决策阶段///////////////////////////////////////////////////
    // 敌方决策阶段
    private IEnumerator EnemyDecisionPhase()
    {
        // 显示目标选择框
        SetTargetNodesVisibility(true);

        Debug.Log($"第 {currentRound} 回合 - 敌方决策阶段");
        
        // 通知UI和其他系统回合开始
        OnRoundStart?.Invoke(currentRound);
        
        // 敌方AI做出决策
        foreach (Unit unit in allUnits)
        {
            if (unit.IsEnemy)
            {
                unit.DecideAction(); // 敌方单位决策
            }
        }
        
        // 模拟敌方决策时间
        yield return new WaitForSeconds(enemyDecisionTime);
        
        // 敌方决策完成，进入玩家策略阶段
        currentState = RoundState.PlayerStrategy;
        OnEnemyDecisionComplete?.Invoke();
    }

    private void SetTargetNodesVisibility(bool visible)
    {
        foreach (GameObject targetNode in allTargetNodes)
        {
            if (targetNode != null)
            {
                targetNode.SetActive(visible);
            }
        }
    }
    
    //////////////////////////////////////////////////////////////玩家策略阶段//////////////////////////////////////////////////////
    // 玩家策略阶段
    private IEnumerator PlayerStrategyPhase()
    {
        Debug.Log($"第 {currentRound} 回合 - 玩家策略阶段");
        
        // 重置选择
        selectedTarget = null;
        selectedStrategy = "";
        ResetButtonColors();
        
        // 显示所有策略UI
        SetStrategyUIVisibility(true);
        
        // 等待玩家选择策略并点击开始回合或按空格键
        while (currentState == RoundState.PlayerStrategy && 
            string.IsNullOrEmpty(selectedStrategy) && 
            !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        // 如果正在选择目标，等待目标选择完成
        while (isSelectingTarget)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || currentState == RoundState.RoundExecution))
            {
                if (selectedTarget == null)
                {
                    int targetposition=99;
                    foreach(Unit unit in allUnits)
                    {
                        if(unit.IsEnemy)
                        {
                            if(unit.StandingPosition<targetposition)
                            {
                                targetposition=unit.StandingPosition;
                                selectedTarget=unit;
                            }
                        }
                    }
                    while (Input.GetKeyDown(KeyCode.Space))
                    {

                        yield return null;
                    }
                    currentState = RoundState.PlayerStrategy;
                    Debug.Log("若不主动选择目标,将优先攻击站位靠左的敌人(确认(Decided或空格键)/取消(手动选择其他策略)");
                }
                else
                {
                    StopTargetSelection();
                }
            }
            yield return null;
        }
        
        // 如果按了空格或回合开始按钮但没有选择策略，使用默认策略
        if (string.IsNullOrEmpty(selectedStrategy) && (Input.GetKeyDown(KeyCode.Space) || currentState == RoundState.RoundExecution))
        {
            selectedStrategy = "Defend"; // 默认防御
            Debug.Log("若不主动选择策略，将使用默认策略: Defend(确认(Decided或空格键)/取消(手动选择其他策略))");
            currentState = RoundState.PlayerStrategy;
            //等待松开空格键
            while (Input.GetKeyDown(KeyCode.Space))
            {

                yield return null;
            }
            //二次确认
            while (!Input.GetKeyDown(KeyCode.Space) && currentState ==RoundState.PlayerStrategy)
            {

                yield return null;
            }
        }

        
        
        // 隐藏UI元素
        SetStrategyUIVisibility(false);
        
        // 应用选择的策略
        ApplySelectedStrategy(selectedStrategy);
        
        // 隐藏目标选择框
        SetTargetNodesVisibility(false);

        // 进入回合执行阶段
        currentState = RoundState.RoundExecution;
    }

    // 选择策略
    private void SelectStrategy(string strategy)
    {
        selectedStrategy = strategy;
        Debug.Log($"选择了策略: {strategy}");
        if (strategy == "Attack" || strategy == "Skill")
        {
            pendingStrategy = strategy; 
            StartTargetSelection();
        }
        // 高亮选中的按钮
        HighlightSelectedButton(strategy);
    }

    // 开始目标选择
    private void StartTargetSelection()
    {
        isSelectingTarget = true;
        
        // 停止现有的协程（如果有）
        if (targetSelectionCoroutine != null)
        {
            StopCoroutine(targetSelectionCoroutine);
        }
        
        // 启动新的目标选择协程
        targetSelectionCoroutine = StartCoroutine(TargetSelectionRoutine());
    }

    // 停止目标选择
    private void StopTargetSelection()
    {
        isSelectingTarget = false;
        
        if (targetSelectionCoroutine != null)
        {
            StopCoroutine(targetSelectionCoroutine);
            targetSelectionCoroutine = null;
        }
    }

    // 目标选择协程
    private IEnumerator TargetSelectionRoutine()
    {
        Debug.Log("开始选择目标,点击敌人选择目标,按鼠标右键或键盘'-'键取消");
        
        while (isSelectingTarget)
        {
            // 检测鼠标点击
            if (Input.GetMouseButtonDown(0))
            {
                HandleTargetSelection();
            }
            
            // 检测取消键
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetMouseButtonDown(1))
            {
                CancelTargetSelection();
            }
            
            // 每帧检测一次
            yield return null;
        }
    }

    // 处理目标选择
    private void HandleTargetSelection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // 使用LayerMask优化检测
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitLayerMask))
        {
            // 检查是否点击了目标框
            if (hit.collider.name.Contains("TargetNode"))
            {
                // 通过目标框找到对应的Unit
                Unit targetUnit = hit.collider.GetComponentInParent<Unit>();
                if (IsValidTarget(targetUnit))
                {
                    SelectTarget(targetUnit);
                    return;
                }
            }
            else
            {
                Debug.Log("无效的目标选择");
            }
        }
    }

    // 验证目标有效性
    private bool IsValidTarget(Unit target)
    {
        return target != null && !target.IsDefeated;
    }

    // 选择目标
    private void SelectTarget(Unit target)
    {
        selectedTarget = target;
        Debug.Log($"已选择目标: {selectedTarget.unitName}");
    }

    // 取消目标选择
    private void CancelTargetSelection()
    {
        selectedTarget = null;
        pendingStrategy = "";
        Debug.Log("已取消目标选择");
    }


    // 应用选择的策略
    private void ApplySelectedStrategy(string strategy)
    {
         // 首先重置所有单位的状态
        foreach (Unit unit in allUnits)
        {
            if (unit.IsPlayer == true)
            {
                unit.IsDefending = false;
                unit.IsDodging = false;
                // 重置其他可能的状态
            }
        }
        switch (strategy)
        {
            ///////////////////////////////攻击情况/////////////////////////////
            case "Attack":
                // 应用攻击策略
                foreach (Unit unit in allUnits)
                {
                    if (unit.IsPlayer)
                    {
                        // 这里需要为每个玩家单位设置攻击目标
                        // 您可能需要修改Unit类来支持设置特定目标
                        SetUnitAttackTarget(unit, selectedTarget);
                    }
                }
                Debug.Log("执行攻击策略");
                break;
            ////////////////////////////////防御情况////////////////////////////
            case "Defend":
                // 应用防御策略
                foreach (Unit unit in allUnits)
                {
                    if (unit.IsPlayer == true)
                    {
                        unit.IsDefending = true;
                    }
                }
                Debug.Log("执行防御策略");
                break;
            /////////////////////////////////闪避情况////////////////////////////
            case "Dodge":
                // 应用闪避策略
                foreach (Unit unit in allUnits)
                {
                    if (unit.IsPlayer == true)
                    {
                        unit.IsDodging = true;
                    }
                }
                Debug.Log("执行闪避策略");
                break;
            ////////////////////////////////技能情况/////////////////////////////
            case "Skill":
                // 应用技能策略
                foreach (Unit unit in allUnits)
                {
                    if (unit.IsPlayer)
                    {
                        SetUnitSkillTarget(unit, selectedTarget);
                    }
                }
                Debug.Log("执行技能策略");
                break;
        }
    }

    // 设置单位攻击目标（需要在Unit类中添加对应方法）
    private void SetUnitAttackTarget(Unit attacker, Unit target)
    {
        // 这里需要您修改Unit类来支持设置攻击目标
        attacker.plannedAction = new AttackAction(attacker, target);
        Debug.Log($"{attacker.unitName} 将攻击 {target.unitName}");
    }

    // 设置单位技能目标
    private void SetUnitSkillTarget(Unit caster, Unit target)
    {
        // 设置技能目标
        Debug.Log($"{caster.unitName} 将对 {target.unitName} 使用技能");
    }

    //////////////////////////////////////////////////////////回合执行阶段/////////////////////////////////////////////
    // 回合执行阶段
    private IEnumerator RoundExecutionPhase()
    {
        Debug.Log($"第 {currentRound} 回合 - 执行阶段");
        
        // 通知UI和其他系统回合执行开始
        OnRoundExecutionStart?.Invoke();
        
        // 执行所有单位的行动
        foreach (Unit unit in allUnits)
        {
            // 根据单位类型和决策执行行动
            unit.ExecuteAction();
            
            // 短暂延迟，使行动有顺序感
            yield return new WaitForSeconds(0.5f);
        }
        
        // 等待回合执行完成(播放动画等)
        yield return new WaitForSeconds(roundExecutionTime);
        
        // 进入回合结束阶段
        currentState = RoundState.RoundEnd;
    }
    
    /////////////////////////////////////////////////////////回合结束阶段//////////////////////////////////////////////////
    // 回合结束阶段
    private IEnumerator RoundEndPhase()
    {
        Debug.Log($"第 {currentRound} 回合 - 结束阶段");
        
        // 处理回合结束逻辑(状态重置、效果结算等)
        foreach (Unit unit in allUnits)
        {
            unit.ResetForNewRound();
        }
        
        // 检查游戏结束条件
        bool GameVictor = CheckGameVictor();
        if (IsGameOver)
        {
            // 游戏结束逻辑
            Debug.Log("游戏结束");
            ShowGameOverScreen(GameVictor);
            yield break; // 结束协程
        }
        
        // 通知UI和其他系统回合结束
        OnRoundEnd?.Invoke();
        
        // 增加回合数，准备下一回合
        currentRound++;
        
        // 回到敌方决策阶段
        currentState = RoundState.EnemyDecision;
        
        yield return null;
    }
    
    // 检查游戏结束条件
    private bool CheckGameVictor()
    {
        bool allPlayersIsDefeated = true;
        bool allEnemiesIsDefeated = true;
        // 检查是否所有玩家单位或所有敌方单位被击败
        foreach (Unit unit in allUnits)
        {
            if (unit.IsPlayer && !unit.IsDefeated)
            {
                allPlayersIsDefeated = false;
            }
            
            if (unit.IsEnemy && !unit.IsDefeated)
            {
                allEnemiesIsDefeated = false;
            }
        }
        if(allEnemiesIsDefeated)
        {
            IsGameOver=true;
            return true;
        }
        else if(allPlayersIsDefeated)
        {
            IsGameOver=true;
            return false;
        }
        else return true;
    }

    // 显示游戏结束界面
    private void ShowGameOverScreen(bool GameVictor)
    {
        // 这里可以实例化或显示游戏结束UI
        Debug.Log(GameVictor ? "玩家胜利" : "玩家失败");
        
        // 暂停游戏时间
        Time.timeScale = 0f;
        
        // 禁用所有UI交互
        SetStrategyUIVisibility(false);
        SetTargetNodesVisibility(false);
    }
    
    // 外部调用方法：手动开始回合(用于UI按钮)
    public void StartRoundManually()
    {
        if (currentState == RoundState.PlayerStrategy)
        {
            currentState = RoundState.RoundExecution;
        }
    }
    
    void Update()
    {
        //每帧检测一次
    }

    
}