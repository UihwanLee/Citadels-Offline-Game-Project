using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour {

    [SerializeField] private PlayEngine thePlayEngine;
    [SerializeField] private CardManager theCardManager;
    [SerializeField] private TokenManager theTokenManager;
    [SerializeField] private HeroManager theHeroManager;
    [SerializeField] private SpecialCardManager theSpecialCardManager;
    [SerializeField] private GameObject[] windowCard; // 0: 윈도우 창, 1: 카드, 2: 카드 이름, 3: 카드 정보, 4: 건물 구매 메세지
    [SerializeField] private GameObject[] windowButtonType; // 0: 선택(영웅), 1 효과발동(영웅), 2: 건설, 3: 선택(카드), 4: 효과발동(카드), 5: 넘기기(영웅), 6: 파괴, 7: 선택(카드 효과), 8: 구매
    [SerializeField] private GameObject[] windowHelpMessage; // 도움창(0: 창, 1: 도움 메세지1, 2: 도움 메시지2, 3: 플레이어 순서)
    [SerializeField] private GameObject windowPlayerPlayLog; // 플레이어 플레이 로그

    private GameObject currentObj;

    // 카드 정보 보여주기
    public void OnClickCard(GameObject _obj)
    {
        // User의 카드인지 체크 (Computer의 핸드 카드는 정보를 알 수 없다.)
        // # '행동하기' 버튼을 누른 상태에서 카드 선택 가능
        // theTokenManager.CheckCurrentTokenPlayer() || _obj.GetComponent<CardGet>().card.cardType!=Card.CardType.HandBuilding
        if (!(!theTokenManager.CheckCurrentTokenPlayer() && _obj.GetComponent<CardGet>().card.cardLocate==Card.CardLocate.Hand))
        {
            currentObj = _obj;
            Card _card = currentObj.GetComponent<CardGet>().card;
            windowCard[0].SetActive(true);
            windowCard[1].GetComponent<Image>().sprite = _card.cardImage;
            windowCard[2].GetComponent<Text>().text = _card.cardName;
            windowCard[3].GetComponent<Text>().text = _card.cardInfo;

            // 버튼 판별
            if (Card.CardType.Pick == _card.cardType) windowButtonType[0].SetActive(true);
            else if (Card.CardType.Hero == _card.cardType)
            {
                windowButtonType[1].SetActive(true);

                // '암살자', '도둑' 영웅이 아니라면 영웅 능력을 스킵할 수 있는 버튼 활성화
                if (_card.cardNumber != 1 && _card.cardNumber != 2) windowButtonType[5].SetActive(true);
            }
            else if (Card.CardType.HandBuilding == _card.cardType)
            {
                // 건물 카드일때 (행동하기:건물1장선택/건설/효과발동) 구분하기
                if (Card.CardLocate.Deck == _card.cardLocate) windowButtonType[3].SetActive(true);
                else if (Card.CardLocate.Hand == _card.cardLocate && theSpecialCardManager.IsUseLabortoryPower()) windowButtonType[7].SetActive(true);
                else if (Card.CardLocate.Hand == _card.cardLocate && !theSpecialCardManager.IsUseLabortoryPower()) windowButtonType[2].SetActive(true);
                else if (Card.CardLocate.Field == _card.cardLocate) windowButtonType[4].SetActive(true);
            }
            else if (Card.CardType.FieldBuilding == _card.cardType)
            {
                // 8번 영웅 장군 능력이 발휘되고 User 플레이어의 필드 카드가 아닐 시
                if (Card.CardLocate.Destory == _card.cardLocate && !theTokenManager.CheckCurrentTokenPlayer()) windowButtonType[6].SetActive(true);
                // 특수 건물 일때 -> '실험실', '대장간' 건물 카드는 효과 발동 유형의 카드이므로 '효과 발동' 버튼 활성화
                else if (_card.cardName == theSpecialCardManager.LABORATORY || _card.cardName == theSpecialCardManager.SMITHY) windowButtonType[4].SetActive(true);
                // <장군> 영웅이 카드 파괴 시, <묘지> 카드 효과 발동
                else if(theSpecialCardManager.IsUseCemetryPower()) { windowButtonType[8].SetActive(true); windowButtonType[5].SetActive(true); }
            }
            else Debug.Log("No Type");
        }
    }

    // 카드 정보 창 닫기
    public void close()
    {
        windowCard[0].SetActive(false); windowCard[4].SetActive(false);
        for (int i = 0; i < windowButtonType.Length; i++) windowButtonType[i].SetActive(false);
    }

    // 버튼 비활성화 함수
    private void OffButton(GameObject[] obj)
    {
        for (int i = 0; i < obj.Length; i++) obj[i].SetActive(false);
    }

    // 선택 버튼 함수: 1: 뒷면 카드, 2: 영웅 카드, 3: 건물 카드
    public void OnClickPick()
    {
        // 카드 유형
        if(currentObj.GetComponent<CardGet>())
        {
            // 게임에서 영웅 카드 제외 할 상황일때
            if (currentObj.GetComponent<CardGet>().card.cardNumber == -1)
            {
                BanHero();
            }
            else // 자신의 영웅 카드를 선택해야 할때 
            {
                SelectHero(currentObj);
            }
        }
        OffButton(windowButtonType);
        close();
    }

    public void BanHero()
    {
        int random = Random.Range(0, 8);
        theCardManager.UpdateHero();
        theCardManager.RemoveHero(random);
        thePlayEngine.UpdateState(1);
        UpdateHeplCanvas();
    }

    public void SelectHero(GameObject _currentObj)
    {
        thePlayEngine.SetPlayerHero(_currentObj.GetComponent<CardGet>().card);
        theCardManager.RemoveHero(_currentObj.GetComponent<CardGet>().index);
        thePlayEngine.NextTurn();
        UpdateHeplCanvas();
    }

    // 특수능력 버튼 함수(영웅)
    public void OnClickHero()
    {
        theHeroManager.HeroPower(thePlayEngine.GetUserPlayer());
        theTokenManager.UpdateplayerHeroButtonObj(false);
        theTokenManager.UpdatePlayerStateCanvas(thePlayEngine.GetUserPlayer());
        OffButton(windowButtonType);
        close();
    }

    // 스킵(0: 특수 능력, 1: <묘지> 능력
    public void OnClickSkip()
    {
        // <묘지> 능력 스킵
        if (theSpecialCardManager.IsUseCemetryPower()){
            theSpecialCardManager.UpdateCemetryPower(false);
        }
        // 영웅 능력 스킵
        else{
            theTokenManager.UpdateplayerHeroButtonObj(false);
        }
        OffButton(windowButtonType);
        close();
    }

    // 건설 버튼 함수
    public void OnClickBuild()
    {
        Card _card = currentObj.GetComponent<CardGet>().card; int _index = currentObj.GetComponent<CardGet>().index;
        int _success = theCardManager.PurchaseBuildingCard(thePlayEngine.GetUserPlayer(), _card, _index);
        if (_success == 1) { windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "영웅 특수 능력을 먼저 사용하세요."; }
        else if (_success == 2) { windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "이번 턴에 더 이상 건설 할 수 없습니다!"; }
        else if (_success == 3) { windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "금화가 부족하여 건설할 수 없습니다!"; }
        else
        {
            thePlayEngine.GetUserPlayer().UpdateAvailableBuildCount(thePlayEngine.GetUserPlayer().GetAvailableBuildCount()-1);
            OffButton(windowButtonType);
            close();
        }
    }
    
    // 카드 선택 함수: 기본적으로 일반행동에서 행해지는 카드 2장 중 1장을 선택하는 버튼에서 작동한다.
    public void OnClickSelect()
    {
        theTokenManager.ReturnLeftCard(currentObj.GetComponent<CardGet>().card);
        theTokenManager.SetPlayerButtonEnd();
        thePlayEngine.GetUserPlayer().AddPlayerCard(0, currentObj.GetComponent<CardGet>().card, thePlayEngine.GetUserPlayer().playerType);
        theTokenManager.CloseWindowCard();
        close();

        // 다른 Computer 화면을 보고 있었다면 User 화면으로 돌아가기
        theTokenManager.ReturnUserWindow();
    }

    // 효과 발동 함수(카드) : 실험실, 대장간
    public void OnClickCardPower()
    {
        // 실험실
        if(currentObj.GetComponent<CardGet>().card.cardName == theSpecialCardManager.LABORATORY){
            // 현재 플레이어의 손패가 1장 이상일 때만 발휘 가능
            if(thePlayEngine.GetCurrentPlayer().GetPlayerHandCardCount() > 0)
            {
                theTokenManager.SetPlayerButtonSkip();
                theSpecialCardManager.UseLabortoryPower();
                OffButton(windowButtonType);
                close();
            }
            else
            {
                windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "현재 지불할 건물 카드를 가지고 있지 않습니다.";
            }
        }

        // 대장간
        if (currentObj.GetComponent<CardGet>().card.cardName == theSpecialCardManager.SMITHY)
        {
            // 금화가 2개 이상이고 손패가 5장 이하일 시
            if (thePlayEngine.GetCurrentPlayer().GetPlayerCurrentCoinCount() < 2)
            {
                windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "지불할 금화가 부족합니다!";
            }
            else if(thePlayEngine.GetCurrentPlayer().GetPlayerHandCardCount() > 5)
            {
                windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "손패에 카드를 수용할 공간이 부족합니다!";
            }
            else
            {
                theSpecialCardManager.UseSmithyPower();
                OffButton(windowButtonType);
                close();
            }
        }
    }

    // 실험실: 버릴 핸드 카드 선택 함수
    public void OnClickSelectHandCard()
    {
        thePlayEngine.GetUserPlayer().RemovePlayerCard(0, currentObj.GetComponent<CardGet>().index, Player.PlayerType.user);

        thePlayEngine.GetUserPlayer().UpdatePlayerCoin(1, true);
        thePlayEngine.GetUserPlayer().UpdatePlayerCoinObj();

        theSpecialCardManager.UpdateLabortoryPower(false);

        OffButton(windowButtonType);
        close();
    }

    // 필드 카드 파괴 (8번 영웅 장군 능력)
    public void OnClickDestory()
    {
        Player currentPlayer = thePlayEngine.GetCurrentPlayer();
        Player player = theTokenManager.GetPickHeroTokenPlayer();
        Card card = currentObj.GetComponent<CardGet>().card;

        if (currentPlayer.GetPlayerCurrentCoinCount() < card.price - 1)
        {
            windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "파괴하는데 금화가 부족합니다!";
        }
        else
        {
            // [묘지: <장군>이 건물을 파괴할 때, 금화 1개를 내고 파괴된 건물을 손에 들 수 있습니다, 이 카드의 주인이 <장군>일 때에는 이런 능력을 사용할 수 없습니다]
            theSpecialCardManager.UseCemetry(currentObj);

            currentPlayer.UpdatePlayerCoin(card.price - 1, false);
            currentPlayer.UpdatePlayerCoinObj();
            player.RemovePlayerCard(1, currentObj.GetComponent<CardGet>().index, player.playerType);

            theHeroManager.UpdateWarlordPower(false);
            UpdateHeplCanvas();

            if (theTokenManager.GetCurrentPlayerPlayState() == 1) theTokenManager.SetPlayerButtonAct();
            else if (theTokenManager.GetCurrentPlayerPlayState() == 4) theTokenManager.SetPlayerButtonEnd();

            theTokenManager.ReturnUserWindow();
            theHeroManager.SetPlayerFieldCardDestory(false);

            OffButton(windowButtonType);
            close();
        }
    }

    // 구매 함수
    public void OnClickPurchase()
    {
        int success = theSpecialCardManager.PurchaseBuildingByCemetry(currentObj.GetComponent<CardGet>().card, thePlayEngine.GetUserPlayer());
        if(success == 1) { windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "금화가 부족하여 구매할 수 없습니다!"; }
        else if (success == 2) { windowCard[4].SetActive(true); windowCard[4].GetComponent<Text>().text = "손패가 가득 차서 구매할 수 없습니다!"; }
        else
        {
            OffButton(windowButtonType);
            close();
        }
    }

    // 플레이어 로그 매세지 초기화
    public void InitPlayerPlayLog()
    {
        windowPlayerPlayLog.GetComponent<Text>().text = "";
    }

    // 플레이어 로그 매세지 갱신
    public void UpdatePlayerPlayLog(string message)
    {
        windowPlayerPlayLog.GetComponent<Text>().text = message;
    }

    // 도움 캔버스 초기화
    public void InitHelpCanvas()
    {
        windowHelpMessage[0].SetActive(true);
        windowHelpMessage[1].GetComponent<Text>().text = "";
        windowHelpMessage[3].GetComponent<Text>().text = "";
    }

    // 도움 캔버스 업데이트
    public void UpdateHeplCanvas()
    {
        if (thePlayEngine.GetCurrentState() == 0)
        {
            if(thePlayEngine.GetCurrentPlayer().playerName=="Player") windowHelpMessage[1].GetComponent<Text>().text = "제외할 영웅을 선택해 주세요.";
            windowHelpMessage[3].GetComponent<Text>().text = thePlayEngine.GetCurrentPlayer().playerName + "가 제외할 영웅을 선택 중입니다.";
        }
        if(thePlayEngine.GetCurrentState() == 1)
        {
            if (thePlayEngine.GetCurrentPlayer().playerName == "Player") windowHelpMessage[1].GetComponent<Text>().text = "플레이 할 영웅을 선택해 주세요.";
            windowHelpMessage[3].GetComponent<Text>().text = thePlayEngine.GetCurrentPlayer().playerName + "가 영웅을 선택 중입니다.";
        }
        if(thePlayEngine.GetCurrentPlayer().playerName != "Player") windowHelpMessage[1].GetComponent<Text>().text = "";
        if (thePlayEngine.GetCurrentPlayer() == thePlayEngine.GetUserPlayer() && theHeroManager.IsUsingAssassinPower()) { windowHelpMessage[1].GetComponent<Text>().text = "암살 할 영웅을 선택해 주세요."; windowHelpMessage[3].GetComponent<Text>().text = ""; }
        else if (thePlayEngine.GetCurrentPlayer() == thePlayEngine.GetUserPlayer() && theHeroManager.IsUsingThiefPower()) { windowHelpMessage[1].GetComponent<Text>().text = "도둑질 할 영웅을 선택해 주세요."; windowHelpMessage[3].GetComponent<Text>().text = ""; }
        else if (thePlayEngine.GetCurrentPlayer() == thePlayEngine.GetUserPlayer() && theHeroManager.IsUsingPlayerExchange()) { windowHelpMessage[1].GetComponent<Text>().text = "카드를 교환 할 플레이어를 선택해 주세요."; windowHelpMessage[3].GetComponent<Text>().text = ""; }
        else if (thePlayEngine.GetCurrentPlayer() == thePlayEngine.GetUserPlayer() && theHeroManager.IsUsingWarlordPower()) { windowHelpMessage[1].GetComponent<Text>().text = "다른 플레이어의 카드를 파괴해 주세요."; windowHelpMessage[3].GetComponent<Text>().text = ""; }
        else if (thePlayEngine.GetCurrentState() == 2)
        {
            windowHelpMessage[1].GetComponent<Text>().text = "";
            if(thePlayEngine.GetCurrentPlayer() == thePlayEngine.GetUserPlayer()) windowHelpMessage[3].GetComponent<Text>().text = "";
            else windowHelpMessage[3].GetComponent<Text>().text = thePlayEngine.GetCurrentPlayer().playerName + "가 플레이 중입니다.";
        }
    }
}
