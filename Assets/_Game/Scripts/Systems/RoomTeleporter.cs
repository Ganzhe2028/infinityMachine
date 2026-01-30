using UnityEngine;
using System.Collections;
using UnityEngine.UI; // 如果你用的是新版UI Toolkit可能不需要这个，但GameJam建议用旧版UGUI最快

public class RoomTeleporter : MonoBehaviour
{
    [Header("传送目标")]
    public Transform playerSpawnPoint; // 主角传送到哪？(放一个空物体在那里)
    public Transform cameraPosition;   // 摄像机去哪？(放一个空物体代表新机位)

    [Header("过渡效果 (可选)")]
    public GameObject transitionCanvas; // 拖入一个全屏黑色的UI Panel
    public Text tipText;                // 拖入显示文字的 Text 组件
    public string tipContent = "正在进入下一层..."; 
    
    private bool isTransporting = false;

    private void OnTriggerEnter(Collider other)
    {
        // 只有主角碰到了才触发，且防止重复触发
        if (other.CompareTag("Player") && !isTransporting)
        {
            StartCoroutine(TeleportSequence(other.gameObject));
        }
    }

    IEnumerator TeleportSequence(GameObject player)
    {
        isTransporting = true;

        // 1. 显示黑屏/文字
        if (transitionCanvas != null) 
        {
            transitionCanvas.SetActive(true);
            if(tipText != null) tipText.text = tipContent;
        }

        // 禁用主角控制（防止黑屏时乱跑），需要你的PlayerController里有个enabled开关或者变量
        // 这里简单粗暴直接关掉脚本
        var controller = player.GetComponent<PlayerController>();
        if(controller != null) controller.enabled = false;

        // 2. 等待一点时间 (假装在加载/叙事)
        yield return new WaitForSeconds(1.0f); // 1秒黑屏

        // 3. 瞬移主角
        // 注意：CharacterController 瞬移需要特殊处理，直接改 transform.position 可能会无效
        CharacterController cc = player.GetComponent<CharacterController>();
        if(cc != null) cc.enabled = false; // 先关掉碰撞
        player.transform.position = playerSpawnPoint.position;
        player.transform.rotation = playerSpawnPoint.rotation; // 如果需要重置朝向
        if(cc != null) cc.enabled = true; // 再打开

        // 4. 瞬移摄像机
        if (cameraPosition != null)
        {
            Camera.main.transform.position = cameraPosition.position;
            // 如果每个房间角度不同，也可以同步旋转
            Camera.main.transform.rotation = cameraPosition.rotation;
        }

        // 5. 恢复
        if(controller != null) controller.enabled = true;
        
        yield return new WaitForSeconds(0.5f); // 再黑屏一会儿让玩家反应

        if (transitionCanvas != null) transitionCanvas.SetActive(false);
        isTransporting = false;
    }
}