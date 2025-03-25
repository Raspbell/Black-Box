using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PlayerStatus")]
public class SO_PlayerStatus : ScriptableObject
{
    [SerializeField, Header("移動速度")] public float walkSpeed = 5.0f;
    [SerializeField, Header("空中での移動速度の倍率")] public float airControlMultiplier = 0.2f;
    [SerializeField, Header("ジャンプの力")] public float jumpPower = 10.0f;
    [SerializeField, Header("最小ジャンプ力")] public float minJumpPower = 5.0f;
    [SerializeField, Header("最大ジャンプ力")] public float maxJumpPower = 15.0f;
    [SerializeField, Header("ジャンプのチャージ時間")] public float maxJumpChargeTime = 1.0f;
    [SerializeField, Header("揺れの周波数")] public float oscillationFrequency = 7.0f;
    [SerializeField, Header("揺れの振幅（角度）")] public float oscillationAmplitude = 15.0f;
    [SerializeField, Header("向き変化のアニメーション時間")] public float turnDuration = 0.2f;
    [SerializeField, Header("接地判定用レイの長さ")] public float groundCheckDistance = 0.2f;
    [SerializeField, Header("レイの発射地点の高さオフセット")] public float rayOriginHeightOffset = 0.1f;
}
