using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialCardManager : MonoBehaviour {

    /* 특수 카드 능력 작동 스크립트 */

    // 게임 변수
    [SerializeField] PlayEngine thePlayEngine;
    [SerializeField] CardManager theCardManager;
    [SerializeField] ButtonManager theButtonManager;

    // 특수 건물 카드 이름 명시
    public string DRAGONGATE = "드래곤 게이트";
    public string COLLEGE = "대학";
    public string CHECKPOINT = "초소";
    public string LABORATORY = "실험실"; // 효과 발동
    public string HOUSEOFGHOST = "유령의 집";
    public string SMITHY = "대장간"; // 효과 발동
    public string LIBRARY = "도서관";
    public string LARGECASTLE = "거대 성곽";
    public string CEMETRY = "묘지";
    public string MAGICSCHOOL = "마법 학교";
    public string OBSERVATORY = "천문대";

    private bool isUseLabortory = false;
    private bool isUseCemetry = false;

    // 특수 카드 이름과 일치하면 RETURN TRUE
    public bool IsSpecialCard(string _name, Player player)
    {
        GameObject[] fieldCards = player.GetPlayerFieldCard();

        for(int i=0; i<fieldCards.Length; i++)
        {
            if (fieldCards[i].GetComponent<CardGet>().card.cardName == _name) return true;
        }

        return false;
    }

    // 드래곤 게이트: 건설 비용은 금화 6개이지만, 게임이 끝났을 때 얻는 점수는 8점입니다. ( 패 시 브 )

    // 대학: 건설 비용은 금화 6개이지만, 게임이 끝났을 때 얻는 점수는 8점입니다. ( 패 시 브 )

    // 초소: <장군>에게 파괴되지 않습니다. ( 패 시 브 ) [완료]

    // 실험실: 차례 중에 한 번, 손에 있는 건물카드 1장을 골라 버리고 은행에서 금화 1개를 가져옵니다. ( 효과 발동 ) [완료]
    public void UseLabortoryPower(){
        Player player = thePlayEngine.GetCurrentPlayer();

        if(player == thePlayEngine.GetUserPlayer()){
            UpdateLabortoryPower(true);
        }
        else
        {
            int _index = Random.Range(0, player.GetPlayerHandCardCount());
            player.RemovePlayerCard(0, _index, player.playerType);

            player.UpdatePlayerCoin(1, true);
            player.UpdatePlayerCoinObj();
        }
    }

    public void UpdateLabortoryPower(bool _check)
    {
        if (_check) isUseLabortory = true;
        else isUseLabortory = false;
    }

    public bool IsUseLabortoryPower() { return isUseLabortory; }

    // 유령의 집: 게임이 끝나 점수를 계산할 때, 원하는 색깔로 간주합니다. 하지만 마지막 라운드에 지었다면 그렇게 사용할 수 없습니다. ( 패 시 브 )

    // 대장간: 차례 중에 한 번, 금화 2개를 지불하고 건물카드 3장을 가져올 수 있습니다. ( 효과 발동 ) [완료]
    public void UseSmithyPower()
    {
        Player player = thePlayEngine.GetCurrentPlayer();

        player.UpdatePlayerCoin(2, false);
        player.UpdatePlayerCoinObj();

        player.AddPlayerCard(0, theCardManager.DrawBuildingCard(), player.playerType);
        player.AddPlayerCard(0, theCardManager.DrawBuildingCard(), player.playerType);
        player.AddPlayerCard(0, theCardManager.DrawBuildingCard(), player.playerType);
    }

    // 도서관: 일반 행동을 할 때 카드를 가져오기로 결정했다면, 카드 2장을 모두 손에 들 수 있습니다. ( 패 시 브 ) [완료]
    public void UseLibrary()
    {
        Player player = thePlayEngine.GetCurrentPlayer();

        player.AddPlayerCard(0, theCardManager.DrawBuildingCard(), player.playerType);
        player.AddPlayerCard(0, theCardManager.DrawBuildingCard(), player.playerType);
    }

    // 거대 성곽: <장군>이 이 카드가 있는 도시의 다른 건물을 파괴하려면 금화 1개를 더 지불해야 합니다. ( 패 시 브 ) [완료]

    // 묘지: <장군>이 건물을 파괴할 때, 금화 1개를 내고 파괴된 건물을 손에 들 수 있습니다. 이 카드의 주인이 <장군>일 때에는 이런 능력을 사용할 수 없습니다. ( 패 시 브 ) [완료]
    public void UseCemetry(GameObject card)
    {
        Player player = FindCemetryOwnPlayer();
        if(player != null)
        {
            UpdateCemetryPower(true);
            // <묘지> 카드를 가지고 있는 플레이어가 유저일때
            if (thePlayEngine.GetUserPlayer() == player)
            {
                theButtonManager.OnClickCard(card);
            }
            // <묘지> 카드를 가지고 있는 플레이어가 컴퓨터일때
            else
            {
                PurchaseBuildingByCemetry(card.GetComponent<CardGet>().card, player);
            }
        }
    }

    // <묘지> 카드를 가지고 있는 플레이어 찾기
    private Player FindCemetryOwnPlayer()
    {
        for(int i=0; i<6; i++){
            Player player = thePlayEngine.GetPlayerByIndex(i);

            if (IsSpecialCard(CEMETRY, player)) return player;
        }
        return null;
    }

    public int PurchaseBuildingByCemetry(Card card, Player player)
    {
        if (player.GetPlayerCurrentCoinCount() < card.price - 1) return 1;
        else if (player.GetPlayerHandCardCount() < 8) return 2;
        else
        {
            player.UpdatePlayerCoin(card.price - 1, false);
            player.UpdatePlayerCoinObj();

            player.AddPlayerCard(0, card, player.playerType);

            UpdateCemetryPower(false);

            return 0;
        }
    }

    public void UpdateCemetryPower(bool _check)
    {
        if (_check) isUseCemetry = true;
        else isUseCemetry = false;
    }

    public bool IsUseCemetryPower() { return isUseCemetry; }

    // 마법 학교: 차례 중에 원하는 색깔로 간주하여 수입을 얻을 수 있습니다. 예를 들어 <왕>이라면 귀족(노란색) 건물로 간주하여 추가로 수입을 얻을 수 있습니다. 
    //            게임이 끝나 점수를 계산할 때에는 그렇게 사용할 수 없습니다. ( 패 시 브 ) << 자동 처리 [완료]

    // 천문대: 일반 행동을 할 때, '금화 얻기'와 '카드 얻기' 모두 사용할 수 있습니다. ( 패 시 브 ) [완료]

}
