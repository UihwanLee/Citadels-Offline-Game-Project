using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour {

    [SerializeField] private ButtonManager theButtoneManager;
    [SerializeField] private TokenManager theTokenManager;
    [SerializeField] private PlayEngine thePlayEngine;

    [SerializeField] private GameObject objHerosCanvas;
    [SerializeField] private GameObject[] objHeros; // 영웅 카드 선택 패
    [SerializeField] private int currentCountHeros=8; // 영웅 카드 선택 패 개수
    [SerializeField] private GameObject[] explainCardObj; // 도움 카드 오브젝트

    [SerializeField] private Card[] heros; // 영웅 카드
    [SerializeField] private Card[] cards; // 건물 카드
    [SerializeField] private Card[] cardsBackground; // 카드 뒷면
    [SerializeField] private Card[] explainCard; // 도움 카드

    private int currentCardIndex; // 현재 맨 위에 있는 카드 인덱스

    /* 
     * * 초기 (0 라운드) 방식
     * 01. 랜덤 카드 부여
     * 02. 랜덤 카드 순서대로 영웅카드 선택 순서 부여
     * 03. 선택 순서대로 영웅을 뽑는 방식 진행
     * 04. 1라운드 이후부터는 왕(영웅 4번 카드)을 가지고 있는 순번 다음대로 진행
     * 
    */

    // 덱 카드 초기화
    public void InitCardDeck()
    {
        for (int i = 0; i < cards.Length; i++) cards[i].cardLocate = Card.CardLocate.Deck;
    }

    // 도움 카드 초기화
    public void InitExplainCard()
    {
        for (int i = 0; i < explainCardObj.Length; i++) UpdateCard(explainCardObj[i], explainCard[i], Card.CardType.Explain);
    }

    // 영웅 카드 뒷면 초기화
    public void ResetHero()
    {
        objHerosCanvas.SetActive(true);
        for (int i=0; i<objHeros.Length; i++)
        {
            objHeros[i].SetActive(true);
            UpdateCard(objHeros[i], cardsBackground[0], Card.CardType.Pick);
        }
        theButtoneManager.UpdateHeplCanvas();

        currentCountHeros = 8;
    }

    // 영웅 카드 캔버스 비활성화
    public void OffHeroCanvas()
    {
        objHerosCanvas.SetActive(false);
    }

    // 영웅 카드 캔버스 위치 조정
    public void SetHeroCanvasLoacate(bool _check)
    {
        /*
        if (_check) { objHerosCanvas.GetComponent<Transform>().position = new Vector3(0f, 0f, 0f); Debug.Log(objHerosCanvas.GetComponent<Transform>().position); } 
        else objHerosCanvas.GetComponent<Transform>().position = new Vector3(-2000f, 0f, 0f);
        */
    }

    // 카드 인덱스 초기화
    public void InitCurrentCardIndex()
    {
        currentCardIndex = cards.Length-1;
    }

    // 카드 갱신
    public void UpdateCard(GameObject _card, Card _newCard, Card.CardType _cardType)
    {
        _card.GetComponent<CardGet>().card = _newCard;
        _card.GetComponent<CardGet>().card.cardType = _cardType; // 영웅 카드 타입 변경
        _card.GetComponent<Image>().sprite = _newCard.cardImage;
    }

    // 영웅 카드 갱신
    public void UpdateHero()
    {
        for (int i = 0; i < objHeros.Length; i++) UpdateCard(objHeros[i], heros[i], Card.CardType.Pick);

    }

    // 영웅 카드 제외
    public void RemoveHero(int _index)
    {
        objHeros[_index].GetComponent<CardGet>().card = null;
        currentCountHeros--;
        SortingCard(0);
    }

    // 카드 정렬: _type (0: 영웅 선택 패, 1: 손패, 2: 필드), _state(0: 추가, 1: 제외)
    public void  SortingCard(int _type)
    {
        if(_type==0)
        {
            // objHeros = objHeros.OrderBy< gameObject => gameObject.GetComponent<CardGet>().card.cardNumber>
            int num = 0;
            foreach (GameObject _hero in objHeros)
            {
                if(_hero.GetComponent<CardGet>().card!=null)
                {
                    objHeros[num].GetComponent<CardGet>().card = _hero.GetComponent<CardGet>().card; 
                    objHeros[num].GetComponent<Image>().sprite = _hero.GetComponent<CardGet>().card.cardImage;
                    num++;
                }
            }
            UpdateCardActive(objHeros, currentCountHeros, 1);
        }
    }

    // 카드 활성화/비활성화 갱신: 0: 추가, 1: 제외
    public void UpdateCardActive(GameObject[] obj, int currentCountObj, int _state)
    {
        for (int i = currentCountObj; i < obj.Length; i++)
        {
            if (_state == 0) obj[i].SetActive(true);
            else if (_state == 1) obj[i].SetActive(false);
        }
    }

    // 카드 섞기
    public void ShuffleBuildingCard()
    {
        for(int i=0; i<currentCardIndex; i++)
        {
            Card temp = cards[i];
            int k = Random.Range(i, cards.Length);
            cards[i] = cards[k];
            cards[k] = temp;
        }
    }

    // 플레이어 건물 카드 건설 (0: 정상 구매, 1: 특수 능력 버튼 유무, 2: 건설 가능한 건물 수 부족, 3: 금화 부족)
    public int PurchaseBuildingCard(Player _player, Card _card, int _index)
    {
        if (_player == thePlayEngine.GetUserPlayer() && theTokenManager.GetHeroButtonStateType()==Token.StateType.PowerActive) return 1;
        else if (_player.GetAvailableBuildCount() == 0) return 2;
        else if (_player.GetPlayerCurrentCoinCount() < _card.price) return 3;
        else
        {
            // 핸드 건물 삭제 및 필드 건물 추가
            _player.RemovePlayerCard(0, _index, _player.playerType);
            _player.AddPlayerCard(1, _card, _player.playerType);

            // 플레이어 코인 업데이트
            _player.UpdatePlayerCoin(_card.price, false);
            _player.UpdatePlayerCoinObj();

            // 플레이어 화면 업데이트 (유저 -> 바로 업데이트, 컴퓨터 -> 유저가 그 컴퓨터의 화면을 보고 있는 상황 시 업데이트)
            if (_player.playerType == thePlayEngine.GetUserPlayer().playerType) theTokenManager.UpdatePlayerStateCanvas(_player);

            return 0;
        }
    }

    // 건물 카드 뽑기
    public Card DrawBuildingCard() { return cards[currentCardIndex--]; }

    // 건물 카드 덱에 다시 넣고 섞기
    public void ReturnBuildingCard(Card _card) {
        if (currentCardIndex + 2 <= cards.Length){
            // cardLoacate Deck 위치로 갱신하기
            cards[++currentCardIndex] = _card; ShuffleBuildingCard();
        }
    }

    // 영웅 선택 화면 숨기기/보이기
    public void SetHeroCanvas(bool _bool) { objHerosCanvas.SetActive(_bool); }

    // 영웅 카드 반환
    public Card[] GetHeros() { return heros; }

    // 남아있는 영웅 카드 오브젝트 개수 반환
    public int GetHeroObjCount() { return currentCountHeros; }

    // 남아있는 영웅 카드 오브젝트 반환
    public GameObject GetLeftHeroObj(int _index) { return objHeros[_index]; }

    public int GetDeckCardCount() { return currentCardIndex+1; } // 현재 덱에 남아있는 카드 개수 반환
}
