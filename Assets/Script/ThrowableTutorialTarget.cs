using UnityEngine;

public class ThrowableTutorialTarget : BaseEnemy
{
    public Lady master;
    private bool isClicked = false;

    private void OnMouseDown()
    {
        // 시간정지 상태일 때만 처리
        if (!TimeStopController.Instance.IsTimeStopped || isClicked) return;

        isClicked = true;

        // 연계 공격 타겟으로 등록
       Player.Instance.GetComponent<P_Attack>()?.RegisterTarget(this);
        // ② Lady 에게 클릭 카운트 전달
        master?.RegisterClick(gameObject);

        // UI로 순서표시 등의 추가 로직이 있다면 여기에
    }

    public void ResetClick()
    {
        isClicked = false;
        ClearOrderNumber();  // UI 표시 초기화
    }

    // 튜토리얼에서는 데미지로 죽지 않음. 클릭만 인식하면 됨.
   public override void TakeDamage(int dmg)
    {
        ClearOrderNumber();          // 순서 UI 제거
        Destroy(gameObject);         // 바로 제거 (코루틴 호출 없음)
    }

    protected override void Die() { /* 빈 구현 – base.Die() 막기 */ }
}
