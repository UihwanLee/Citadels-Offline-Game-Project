using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroManager : MonoBehaviour {

    /* 영웅 특수능력 작동 스크립트 */

    // 게임 변수
    [SerializeField] private PlayEngine thePlayEngine;
    [SerializeField] private CardManager theCardManager;
    [SerializeField] private TokenManager theTokenManager;
    [SerializeField] private ButtonManager theButtonManager;
    [SerializeField] private SpecialCardManager theSpecialCardManager;

    private bool useAssassinPower = false;
    private bool useThiefPower = false;
    private bool useWarlordPower = false;

    [SerializeField] private Sprite[] windowMegicianImage; // 마법사 창 이미지 (0: 왼쪽 플레이어 Off, 1: 왼쪽 플레이어 On, 2: 오른쪽 덱 Off, 3: 오른쪽 덱 On)
    // 마법사 창 (0: 윈도우 창, 1: 왼쪽 버튼, 2: 오른쪽 버튼, 3: 왼쪽 선택 버튼, 4: 오른쪽 선택 버튼 5: 왼쪽 텍스트, 6: 오른쪽 텍스트. 7: 오른쪽 경고 텍스트)
    [SerializeField] private GameObject[] windowMegician;
    private bool megicianPowerBool; // 마법사 교환 선택 참값
    private bool selectPlayerExchangeBool; // 플레이어 교환 선택 참값
    private bool selectDeckExchangeBool; // 카드 덱 교환 선택 참값

    private bool usePlayerExchange; // 마법사 능력 사용 시 플레이어 교환 참값

    public void HeroPower(Player _player)
    {
        int _index = _player.GetPlayerHero().cardNumber;

        if (_index == 1) Assassin(_player);
        else if (_index == 2) Thief(_player);
        else if (_index == 3) Magician(_player);
        else if (_index == 4) King(_player);
        else if (_index == 5) Bishop(_player);
        else if (_index == 6) Merchant(_player);
        else if (_index == 7) Architect(_player);
        else if (_index == 8) Warlord(_player);
    }


    // 01. 암살자: 암살할 캐릭터의 이름을 선언합니다. 암살된 캐릭터는 자기 차례를 쉽니다.
    public void Assassin(Player _player)
    {
        if (_player == thePlayEngine.GetUserPlayer())
        {
            useAssassinPower = true;
            theTokenManager.SetHeroSelectCanvas(1);
        }
        else
        {
            int _index = Random.Range(1, 8);
            theTokenManager.SetHeroTokenDeath(_index);
        }
    }

    public void UpdateAssassinPower(bool _use)
    {
        if (_use) useAssassinPower = true;
        else useAssassinPower = false;
    }

    public bool IsUsingAssassinPower() { return useAssassinPower; }

    // 02. 도둑: 금화를 훔쳐올 목표 캐릭터의 이름을 선언합니다. 목표 캐릭터가 호명될 때, 목표 캐릭터의 개인 금고에서 금화 전부를 즉시 가져옵니다.
    public void Thief(Player _player)
    {
        if (_player == thePlayEngine.GetUserPlayer())
        {
            useThiefPower = true;
            theTokenManager.SetHeroSelectCanvas(2);
            theTokenManager.ResetRoundHeroToken();
        }
        else
        {
            // 암살자가 지목하지 않은 번호가 나올 때까지 뽑는다
            int _index = Random.Range(2, 8);
            while(_index == theTokenManager.GetDeathPlayerHeroNumber()-1)
            {
                _index = Random.Range(2, 8);
            }

            theTokenManager.SetHeroTokenStolen(_index);
        }
    }

    public void UpdateThiefPower(bool _use)
    {
        if (_use) useThiefPower = true;
        else useThiefPower = false;
    }

    public bool IsUsingThiefPower() { return useThiefPower; }

    // 03. 마술사: 다른 플레이어와 손에 든 카드 전부를 서로 교환하거나, 또는 손에 든 카드를 원하는 만큼 버리고 똑같은 수만큼 건물 카드 더미에서 카드를 가져옵니다.
    public void Magician(Player _player)
    {
        // User인 경우 '덱'과 '플레이어' 중 누구와 교환할지 선택하는 창을 보여준다.
        if (_player == thePlayEngine.GetUserPlayer())
        {
            megicianPowerBool = true;
            windowMegician[0].SetActive(true);
            theTokenManager.SetPlayerButtonHide();
        }
        else
        {
            /* 컴퓨터인 경우 
             * - 손패가 너무 적을 경우 손패가 많은 플레이어와 교환하도록 설정(가장 많은 플레이어 인덱스를 찾는다.)
             * - 손패가 적당히 많은 경우 덱과 교환하도록 설정한다.
             * - 단, 덱과 카드를 교환할때 손패에 '특수 카드'가 1개 이상 있을 시 바꾸지 않는다.
             */
            if (_player.GetPlayerHandCardCount() < 4)
            {
                int index = GetMaxHandCountPlayerIndex();
                theTokenManager.ExchangeHeroToken(index);
            }
            else
            {
                SelectDeckExchange();
            }
        }
    }

    // 손패를 가장 많이 가지고 있는 플레이어 인덱스 반환
    private int GetMaxHandCountPlayerIndex()
    {
        int _index = 0;
        int max = -1;

        for (int i = 0; i < 6; i++)
        {
            if (thePlayEngine.GetPlayerByIndex(i).GetPlayerHandCardCount() > max)
            {
                max = thePlayEngine.GetPlayerByIndex(i).GetPlayerHandCardCount();
                _index = i;
            }
        }

        return _index;
    }

    // 마법사 교환 선택 창 초기화
    public void InitWindowMegician()
    {
        megicianPowerBool = false;
        selectPlayerExchangeBool = false;
        selectDeckExchangeBool = false;
        windowMegician[0].SetActive(false);
        windowMegician[3].SetActive(false);
        windowMegician[4].SetActive(false);
        windowMegician[7].SetActive(false);
        windowMegician[0].SetActive(false);

        usePlayerExchange = false;
    }

    public void UpdatePlayerExchange(bool _use)
    {
        if (_use) usePlayerExchange = true;
        else usePlayerExchange = false;
    }

    public bool GetMegicianPowerBool() { return megicianPowerBool; } // 마법사 영웅 능력 참값 반환
    public void SetWindowMegicianActive(bool _bool) { windowMegician[0].SetActive(_bool); } // 마법사 교환 선택 창 활성화/비활성화
    public bool IsUsingPlayerExchange() { return usePlayerExchange; } // 플레이어와 카드 교환 참값 반환

    public void OnClickPlayerExchangeWindow()
    {
        if (selectDeckExchangeBool)
        {
            windowMegician[2].GetComponent<Image>().sprite = windowMegicianImage[2];
            windowMegician[4].SetActive(false);
            windowMegician[6].SetActive(true);
            selectDeckExchangeBool = false;
        }

        selectPlayerExchangeBool = !selectPlayerExchangeBool;
        if (selectPlayerExchangeBool)
        {
            windowMegician[1].GetComponent<Image>().sprite = windowMegicianImage[1];
            windowMegician[3].SetActive(true);
            windowMegician[5].SetActive(false);
        }
        else
        {
            windowMegician[1].GetComponent<Image>().sprite = windowMegicianImage[0];
            windowMegician[3].SetActive(false);
            windowMegician[5].SetActive(true);
        }
    }

    public void SelectPlayerExchange()
    {
        megicianPowerBool = false;
        selectPlayerExchangeBool = false;
        selectDeckExchangeBool = false;
        windowMegician[0].SetActive(false);

        // 교환할 플레이어를 선택하는 창 활성화
        UpdatePlayerExchange(true);
        theTokenManager.SetHeroSelectCanvas(3);
    }

    public void OnClickDeckExchangeWindow()
    {
        if (selectPlayerExchangeBool)
        {
            windowMegician[1].GetComponent<Image>().sprite = windowMegicianImage[0];
            windowMegician[3].SetActive(false);
            windowMegician[5].SetActive(true);
            selectPlayerExchangeBool = false;
        }

        selectDeckExchangeBool = !selectDeckExchangeBool;
        if (selectDeckExchangeBool)
        {
            windowMegician[2].GetComponent<Image>().sprite = windowMegicianImage[3];
            windowMegician[4].SetActive(true);
            windowMegician[6].SetActive(false);
        }
        else
        {
            windowMegician[2].GetComponent<Image>().sprite = windowMegicianImage[2];
            windowMegician[4].SetActive(false);
            windowMegician[6].SetActive(true);
        }
    }

    public void SelectDeckExchange()
    {
        megicianPowerBool = false;
        selectPlayerExchangeBool = false;
        selectDeckExchangeBool = false;
        windowMegician[0].SetActive(false);

        // 현재 손패에 가지고 있는 카드들을 전부 덱으로 돌려놓은 뒤 새로 뽑는다.
        Player player = thePlayEngine.GetCurrentPlayer();
        GameObject[] handCards = player.GetPlayerHandCard();

        // 덱에서 교체 가능한 수만큼 길이를 정한다.
        int length = (player.GetPlayerHandCardCount() < theCardManager.GetDeckCardCount()) ? player.GetPlayerHandCardCount() : theCardManager.GetDeckCardCount();

        Card[] newCards = new Card[length]; // 새로 뽑을 카드

        // 미리 덱에서 플레이어 손패 수 만큼 카드 뽑기 
        for (int i = 0; i < length; i++)
        {
            newCards[i] = theCardManager.DrawBuildingCard();
        }

        // Player HandCard Deck에 돌려넣기 및 HandCard 삭제
        for (int i=0; i< length; i++)
        {
            theCardManager.ReturnBuildingCard(handCards[i].GetComponent<CardGet>().card);
            player.RemovePlayerCard(0, i, player.playerType);
        }

        // HandCard에 Deck에서 뽑은 새로운 카드 집어넣기
        for (int i = 0; i < length; i++)
        {
            player.AddPlayerCard(0, newCards[i], player.playerType);
        }

        if(thePlayEngine.GetCurrentPlayer() == thePlayEngine.GetUserPlayer())
        {
            if (theTokenManager.GetCurrentPlayerPlayState() == 1) theTokenManager.SetPlayerButtonAct();
            else if (theTokenManager.GetCurrentPlayerPlayState() == 4) theTokenManager.SetPlayerButtonEnd();
        }
    }

    // 04. 왕: 왕관을 가져옵니다. 자기 도시의 귀족 건물 수만큼 금화를 받습니다.
    public void King(Player _player)
    {
        int reward = _player.GetBuildTypeCount(3);

        // [마법 학교: 차례 중에 원하는 색깔로 간주하여 수입을 얻을 수 있습니다]
        if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.MAGICSCHOOL, thePlayEngine.GetCurrentPlayer())) reward += 1;

        _player.UpdatePlayerCoin(reward, true);
        _player.UpdatePlayerCoinObj();
    }

    // 05. 주교: 자기 도시의 건물에는 8번 캐릭터가 절대로 능력을 쓸 수 없습니다. 자기 도시의 종교 건물 수만큼 금화를 받습니다.
    public void Bishop(Player _player)
    {
        int reward = _player.GetBuildTypeCount(2);

        // [마법 학교: 차례 중에 원하는 색깔로 간주하여 수입을 얻을 수 있습니다]
        if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.MAGICSCHOOL, thePlayEngine.GetCurrentPlayer())) reward += 1;

        _player.UpdatePlayerCoin(reward, true);
        _player.UpdatePlayerCoinObj();
    }

    // 06. 상인: 추가로 금화 1닢을 받습니다. 자기 도시의 상업 건물 수만큼 금화를 받습니다.
    public void Merchant(Player _player)
    {
        int reward = _player.GetBuildTypeCount(1);

        // [마법 학교: 차례 중에 원하는 색깔로 간주하여 수입을 얻을 수 있습니다]
        if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.MAGICSCHOOL, thePlayEngine.GetCurrentPlayer())) reward += 1;

        _player.UpdatePlayerCoin(reward, true);

        _player.UpdatePlayerCoin(1, true);
        _player.UpdatePlayerCoinObj();
    }

    // 07. 건축가: 추가로 카드 2장을 받습니다. 건물을 3채까지 건설할 수 있습니다.
    public void Architect(Player _player)
    {
        for(int i=0; i<2; i++){
            if(_player.GetPlayerHandCardCount() <= 7) _player.AddPlayerCard(0, theCardManager.DrawBuildingCard(), _player.playerType);
        }
        _player.UpdateAvailableBuildCount(_player.GetAvailableBuildCount() + 2);
    }

    // 08. 장군: 원하는 건물 한 채를 파괴할 수 있습니다(파괴비용은 해당 건물 건설 비용보다 금화 1닢 적습니다). 자기 도시의 군사 건물 수만큼 금화를 받습니다.
    public void Warlord(Player _player)
    {
        int reward = _player.GetBuildTypeCount(4);

        // [마법 학교: 차례 중에 원하는 색깔로 간주하여 수입을 얻을 수 있습니다]
        if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.MAGICSCHOOL, thePlayEngine.GetCurrentPlayer())) reward += 1;

        _player.UpdatePlayerCoin(reward, true);
        _player.UpdatePlayerCoinObj();

        /* 건물 파괴 */

        // 능력을 사용하는 플레이어를 빼고 다른 플레이어 필드 카드 CardLoacte Destory로 설정
        SetPlayerFieldCardDestory(true);

        if (_player == thePlayEngine.GetUserPlayer())
        {
            UpdateWarlordPower(true);
            theButtonManager.UpdateHeplCanvas();
            theTokenManager.SetPlayerButtonSkip();


        }
        else
        {
            /* 컴퓨터 
             * 01. 우선적으로 현재 필드 카드가 가장 많은 플레이어의 건물 카드를 파괴하도록 한다.(8 건물 채우기 방지)
             * 02. 가능한 높은 코스트 건물을 파괴할 수 있도록 한다,(ex) 5~6코스트 일반 건물 or특수 건물
            */

            int index = GetMaxFieldCountPlayerIndex();
            Player player2 = thePlayEngine.GetPlayerByIndex(index);
            DestoryHighCostFieldCard(_player, player2);
        }
    }

    // 능력을 사용하는 플레이어를 빼고 다른 플레이어 필드 카드 CardLoacte Destory로 설정
    public void SetPlayerFieldCardDestory(bool check)
    {
        Player[] players = thePlayEngine.GetPlayers();
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetPlayerHero().cardNumber != thePlayEngine.GetCurrentPlayer().GetPlayerHero().cardNumber) players[i].SetFieldCardLoacteDestory(check);
        }
    }

    // 필드 카드를 가장 많이 가지고 있는 플레이어 인덱스 반환
    private int GetMaxFieldCountPlayerIndex()
    {
        int _index = 0;
        int max = -1;

        for (int i = 0; i < 6; i++)
        {
            if (thePlayEngine.GetPlayerByIndex(i).GetPlayerFieldCardCount() > max)
            {
                max = thePlayEngine.GetPlayerByIndex(i).GetPlayerFieldCardCount();
                _index = i;
            }
        }

        return _index;
    }

    // 가장 높은 코스트 건물 카드 파괴: 단, 파괴하려는 플레이어가 지불할 수 있는 금액이여야 한다.
    public void DestoryHighCostFieldCard(Player player1, Player player2)
    {
        int maxCost = 0; int index = -1; 
        int availableDestoryCost = player1.GetPlayerCurrentCoinCount();
        int player2FieldCardCount = player2.GetPlayerFieldCardCount();
        GameObject[] player2FieldCard = player2.GetPlayerFieldCard();

        for(int i=0; i<player2FieldCardCount; i++){
            // [초소: <장군>에게 파괴되지 않습니다]
            if (player2FieldCard[i].GetComponent<CardGet>().card.price > maxCost && player2FieldCard[i].GetComponent<CardGet>().card.cardName != theSpecialCardManager.CHECKPOINT) {
                if (player2FieldCard[i].GetComponent<CardGet>().card.price <= availableDestoryCost)
                {
                    maxCost = player2FieldCard[i].GetComponent<CardGet>().card.price;
                    index = i;
                }
            }
        }

        if(maxCost != 0 && index != -1){
            // [묘지: <장군>이 건물을 파괴할 때, 금화 1개를 내고 파괴된 건물을 손에 들 수 있습니다, 이 카드의 주인이 <장군>일 때에는 이런 능력을 사용할 수 없습니다]
            theSpecialCardManager.UseCemetry(player2FieldCard[index]);

            // [거대 성곽: < 장군 > 이 이 카드가 있는 도시의 다른 건물을 파괴하려면 금화 1개를 더 지불해야 합니다]
            if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.LARGECASTLE, player2)) player1.UpdatePlayerCoin(maxCost, false);
            else player1.UpdatePlayerCoin(maxCost - 1, false);
            player1.UpdatePlayerCoinObj();
            player2.RemovePlayerCard(1, index, player2.playerType);

            UpdateWarlordPower(false);
        }

        SetPlayerFieldCardDestory(false);
    }

    public void UpdateWarlordPower(bool _use)
    {
        if (_use) useWarlordPower = true;
        else useWarlordPower = false;
    }

    public bool IsUsingWarlordPower() { return useWarlordPower; }

}
