using UnityEngine;

public class ProceduralWalk : MonoBehaviour
{
    [Header("晃动设置")]
    public float wobbleSpeed = 10f; // 晃动有多快
    public float wobbleAmount = 10f; // 晃动幅度(角度)
    
    private CharacterController _parentController;
    private Quaternion _initialRotation;

    void Start()
    {
        // 1. 记住一开始的旋转角度（关键，防止模型乱转）
        _initialRotation = transform.localRotation;
        
        // 2. 找到爸爸身上的控制器（为了知道有没有在动）
        _parentController = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (_parentController == null) return;

        // 检测水平速度（忽略上下跳跃的速度）
        Vector3 horizontalVelocity = new Vector3(_parentController.velocity.x, 0, _parentController.velocity.z);

        // 如果速度大于 0.1，说明在跑
        if (horizontalVelocity.magnitude > 0.1f)
        {
            // 用 Sin 函数计算一个左右摆动的角度
            float wobbleZ = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
            
            // 应用旋转：保持 Y 轴（面朝方向）不变，只摇摆 Z 轴
            // 注意：这里是在初始旋转的基础上叠加摇摆
            transform.localRotation = _initialRotation * Quaternion.Euler(0, 0, wobbleZ);
        }
        else
        {
            // 没动的时候，平滑地恢复到直立状态
            transform.localRotation = Quaternion.Lerp(transform.localRotation, _initialRotation, Time.deltaTime * 10f);
        }
    }
}