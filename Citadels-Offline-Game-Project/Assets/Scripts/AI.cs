using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    /* Computer AI가 플레이 할 알고르즘입니다. 
     
       01. 영웅 밴/선택 플레이 상태
         1) Computer의 순번이 1이라면 영웅을 밴하고 선택한다.
         2) 정해진 순번대로 Computer AI들이 '무작위'로 영웅을 뽑고 턴을 넘긴다.

       02. 라운드 플레이 상태
         1) 행동하기
            - 기본적으로'금화 2개 얻기', '카드 1장 얻기' 중에 '무작위' 선택한다.
            - 핸드 카드가 모두 찰 시 무조건 '금화 2개 얻기' 행동
            - 2장 중 카드 선택 또한 '무작위' 선택

         2) 영웅 특수능력 발동
            - 기본적으로 왠만하면 영웅 특수능력을 무조건 사용하는 식으로 진행이 된다.
            - <암살자>, <도둑>의 경우 선반영하여 처리 할 수 있도록 한다.
            - <장군>의 경우 자울 수 있는 건물을 선별한 뒤 '무작위' 선택하여 지울 수 있도록 한다.
        
         3) 건설하기
            - 기본적으로 건물 유형이 겹치지 않도록 조정
            - 유형 4가지 패턴 만들기(1. 무조건 건물 빨리 만들기, 2. 3~4개의 중간 벨류 건물 건설, 3. 4개 이상의 고급 벨류 건물 건설, 4. 특수 건물 건설 우선)
            - 유형 4가지 중 '무작위'로 선별하여 진행

         4) 특수 건물 능력 발동

         5) 턴 넘기기         
         
    */

    // 게임 변수
    [SerializeField] CardManager theCardManager;
    [SerializeField] ButtonManager theButtonManager;
    [SerializeField] PlayEngine thePlayEngine;
    [SerializeField] HeroManager theHeroManager;
    [SerializeField] SpecialCardManager theSpecialCardManager;

    // 영웅 밴/선택 플레이 
    public void PlayBanSelectHero(Player _AI)
    {
        // Debug.Log("[" + _AI.playerSequence + ": " + _AI.playerName + "]: 영웅 선택");
        if (thePlayEngine.GetCurrentState() == 0 ){
            if (_AI.playerSequence == 0){
                theButtonManager.BanHero();
                int _index = Random.Range(0, theCardManager.GetHeroObjCount());
                theButtonManager.SelectHero(theCardManager.GetLeftHeroObj(_index));
            }
        }
        else if(thePlayEngine.GetCurrentState() == 1)
        {
            int _index = Random.Range(0, theCardManager.GetHeroObjCount());
            theButtonManager.SelectHero(theCardManager.GetLeftHeroObj(_index));
        }
        // StartCoroutine(PlaySelectHeroCoroutine(_AI));
    }

    IEnumerator PlaySelectHeroCoroutine(Player _AI)
    {
        float time = Random.Range(2, 5);
        yield return new WaitForSeconds(time);
        if (thePlayEngine.GetCurrentState() == 0)
        {
            if (_AI.playerSequence == 0)
            {
                theButtonManager.BanHero();
                int _index = Random.Range(0, theCardManager.GetHeroObjCount());
                theButtonManager.SelectHero(theCardManager.GetLeftHeroObj(_index));
            }
        }
        else if (thePlayEngine.GetCurrentState() == 1)
        {
            int _index = Random.Range(0, theCardManager.GetHeroObjCount());
            theButtonManager.SelectHero(theCardManager.GetLeftHeroObj(_index));
        }
    }

    // 행동하기
    public void PlayAct(Player _AI)
    {
        if(thePlayEngine.GetCurrentState()==2 && thePlayEngine.GetCurrentPlayer() != thePlayEngine.GetUserPlayer())
        {
            PlayGain(_AI);
            PlayHeroPower(_AI);
            PlayBuild(_AI);
            PlayEnd(_AI);

   
            // StartCoroutine(PlayActCoroutine(_AI));
        }
    }

    IEnumerator PlayActCoroutine(Player _AI)
    {
        float time = Random.Range(2, 5);
        yield return new WaitForSeconds(time);
        PlayGain(_AI);
        time = Random.Range(2, 5);
        yield return new WaitForSeconds(time);
        PlayHeroPower(_AI);
        time = Random.Range(2, 5);
        yield return new WaitForSeconds(time);
        PlayBuild(_AI);
        time = Random.Range(2, 5);
        yield return new WaitForSeconds(time);
        PlayEnd(_AI);
    }

    // 행동하기: 자원 얻기
    private void PlayGain(Player _computer)
    {
        // '무작위'로 '금화 2개 얻기', '카드 2장 중 1장 선택'을 선택
        int _act = Random.Range(0, 2);

        // Player 핸드 카드가 꽉 차 있을 시 금화 2개 얻기
        if (_computer.GetPlayerHandCardCount() + 1 > 7) _act = 0;

        // [천문대: 일반 행동을 할 때, '금화 얻기'와 '카드 얻기' 모두 사용할 수 있습니다]
        if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.OBSERVATORY, thePlayEngine.GetCurrentPlayer()))
        {
            // 금화 2개 얻기
            _computer.UpdatePlayerCoin(2, true);
            _computer.UpdatePlayerCoinObj();

            // 카드 2개 뽑아 1개 얻기
            // [도서관: 일반 행동을 할 때 카드를 가져오기로 결정했다면, 카드 2장을 모두 손에 들 수 있습니다] 
            if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.LIBRARY, thePlayEngine.GetCurrentPlayer()) && _computer.GetPlayerHandCardCount() + 2 <= 7)
            {
                _computer.AddPlayerCard(0, theCardManager.DrawBuildingCard(), _computer.playerType);
                _computer.AddPlayerCard(0, theCardManager.DrawBuildingCard(), _computer.playerType);
            }
            else
            {
                _computer.AddPlayerCard(0, theCardManager.DrawBuildingCard(), _computer.playerType);
            }
        }
        else
        {
            // 금화 2개 얻기
            if (_act == 0)
            {
                theButtonManager.UpdatePlayerPlayLog(_computer.playerName + "플레이어<" + _computer.GetPlayerHero().cardNumber + ">가 [자원 얻기: 금화 2개 얻기]를 하였습니다.");
                _computer.UpdatePlayerCoin(2, true);
                _computer.UpdatePlayerCoinObj();
            }
            // 카드 2개 뽑아 1개 얻기
            else
            {
                // [도서관: 일반 행동을 할 때 카드를 가져오기로 결정했다면, 카드 2장을 모두 손에 들 수 있습니다] 
                if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.LIBRARY, thePlayEngine.GetCurrentPlayer()) && _computer.GetPlayerHandCardCount() + 2 <= 7)
                {
                    _computer.AddPlayerCard(0, theCardManager.DrawBuildingCard(), _computer.playerType);
                    _computer.AddPlayerCard(0, theCardManager.DrawBuildingCard(), _computer.playerType);
                }
                else
                {
                    theButtonManager.UpdatePlayerPlayLog(_computer.playerName + "플레이어<" + _computer.GetPlayerHero().cardNumber + ">가 [자원 얻기: 카드 2장 뽑아 1개 얻기]를 하였습니다.");
                    _computer.AddPlayerCard(0, theCardManager.DrawBuildingCard(), _computer.playerType);
                }
            }
        }
    }

    // 행동하기: 영웅 특수능력 발동
    private void PlayHeroPower(Player _computer)
    {
        theButtonManager.UpdatePlayerPlayLog(_computer.playerName + "플레이어<" + _computer.GetPlayerHero().cardNumber + "> : 능력 발동");
        theHeroManager.HeroPower(_computer);
    }

    // 행동하기: 건설하기
    private void PlayBuild(Player _computer)
    {
        theButtonManager.UpdatePlayerPlayLog(_computer.playerName + "플레이어<" + _computer.GetPlayerHero().cardNumber + "> : 건설하기");
        int algorithm = Random.Range(1, 5);

        // Player 건설 가능 횟수만큼 건설
        for (int i = 0; i < _computer.GetAvailableBuildCount(); i++){
            GameObject buildCardObj = BuildAlgorithm(_computer, algorithm);

            if (buildCardObj != null){
                Card buildCard = buildCardObj.GetComponent<CardGet>().card; int index = buildCardObj.GetComponent<CardGet>().index;
                if (_computer.GetPlayerCurrentCoinCount() >= buildCard.price){
                    int success = theCardManager.PurchaseBuildingCard(_computer, buildCard, index);
                }
            }
        }
    }

    // 건설 알고리즘((1. 무조건 건물 빨리 만들기, 2. 3~4개의 중간 벨류 건물 건설, 3. 4개 이상의 고급 벨류 건물 건설, 4. 특수 건물 건설 우선)
    private GameObject BuildAlgorithm(Player _computer, int _algorithm)
    {
        GameObject[] handCard = _computer.GetPlayerHandCard();
        int length = _computer.GetPlayerHandCardCount();

        // 1. 무조건 건물 빨리 만들기(1~2성 건물 만들기)
        if (_algorithm == 1){
            for (int i = 0; i < length; i++){
                if (handCard[i].GetComponent<CardGet>().card.price == 1 || handCard[i].GetComponent<CardGet>().card.price == 2)
                {
                    return handCard[i];
                }
            }
        }
        // 2. 3~4개의 중간 벨류 건물 건설
        else if (_algorithm == 2){
            for (int i = 0; i < length; i++){
                if (handCard[i].GetComponent<CardGet>().card.price == 3 || handCard[i].GetComponent<CardGet>().card.price == 4)
                {
                    return handCard[i];
                }
            }
        }
        // 3. 5개 이상의 고급 벨류 건물 건설
        else if (_algorithm == 3){
            for (int i = 0; i < length; i++){
                if (handCard[i].GetComponent<CardGet>().card.price == 5 || handCard[i].GetComponent<CardGet>().card.price == 6)
                {
                    return handCard[i];
                }
            }
        }
        // 4. 특수 건물 건설 우선
        else if (_algorithm == 4){
            for (int i = 0; i < length; i++){
                if (handCard[i].GetComponent<CardGet>().card.buildType == 5)
                {
                    return handCard[i];
                }
            }
        }
        return null;
    }

    // 행동하기: 턴 넘기기     
    private void PlayEnd(Player _computer)
    {
        theButtonManager.UpdatePlayerPlayLog(_computer.playerName + "플레이어<" + _computer.GetPlayerHero().cardNumber + ">가 턴을 마쳤습니다.");
        thePlayEngine.NextTurn();
    }
}
