using UnityEngine;

public interface IDamageable
{
    void Hit(int damage);          // 데미지 or 비활성화 처리
    Transform transform { get; }   // 위치 계산용
}