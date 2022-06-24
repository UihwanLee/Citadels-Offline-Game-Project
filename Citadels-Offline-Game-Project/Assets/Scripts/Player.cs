using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public string playerName; // 플레이이어 이름
    public int playerSequence; // 플레이어 순번
    public PlayerType playerType; // 플레이어 유형

    public Card playerHero; // 플레이어 영웅
    public GameObject playerHeroCard; // 플레이어 영웅 카드

    public GameObject crown; // 플레이어 왕 여부

    public GameObject[] playerCoin; // 플레이어 코인 
    public int currentCountPlayerCoin; // 플레이어 코인 개수

    public GameObject[] playerHandCard; // 플레이어 핸드 카드
    public GameObject[] playerFieldCard; // 플레이어 필드 카드

    public int currentCountPlayerHandCard; // 플레이어 핸드 카드 개수
    public int currentCountPlayerFieldCard; // 플레이어 필드 카드 개수

    public Vector3 position; // 플레이어 포지션

    public int availableBuildCouunt; // 한 턴에 건설 가능한 건물 수(기본적으로 1)

    public Card heroCardBackGround;
    public Card cardBackGroundImage;

    public enum PlayerType
    {
        user,
        computer,
        ETC
    }

    // 플레이어 코인, 핸드, 필드 카드 초기화
    public void InitPlayer() {
        Card initCard = new Card();
        initCard.cardName = "init"; initCard.cardImage = null;
        currentCountPlayerCoin = 0;
        currentCountPlayerHandCard = 0;
        currentCountPlayerFieldCard = 0;
        for (int i = 0; i < playerHandCard.Length; i++) {
            if (i != playerHandCard.Length - 1) playerCoin[i].SetActive(false);
            playerHandCard[i].GetComponent<CardGet>().index = i;
            playerFieldCard[i].GetComponent<CardGet>().index = i;
            UpdateCard(playerHandCard[i], initCard, Card.CardType.HandBuilding, Player.PlayerType.ETC);
            UpdateCard(playerFieldCard[i], initCard, Card.CardType.FieldBuilding, Player.PlayerType.ETC);
            playerHandCard[i].SetActive(false);
            playerFieldCard[i].SetActive(false);
        }
        availableBuildCouunt = 1;

        ResetHeroCard();
        crown.SetActive(false);
        for (int i = 0; i < playerCoin.Length; i++) playerCoin[i].SetActive(false);
    }

    // 플레이어 영웅 카드 리셋
    public void ResetHeroCard()
    {
        playerHeroCard.SetActive(true);
        UpdateCard(playerHeroCard, heroCardBackGround, Card.CardType.Explain, Player.PlayerType.ETC);
        playerHeroCard.SetActive(false);
    }

    // 플레이어 영웅 카드 업데이트
    public void UpdateHeroCard(int currentState)
    {
        playerHeroCard.SetActive(true);
        // 영웅 선택 상태X, 컴퓨터) 현재 플레이 중이거나 지났을때 영웅 카드 정보 공개
        if (playerHero != null && (this.playerType == PlayerType.user || (this.playerType == PlayerType.computer && currentState == 2)))
        {
            UpdateCard(playerHeroCard, playerHero, Card.CardType.Explain, Player.PlayerType.ETC);
        }
        else
        {
            UpdateCard(playerHeroCard, heroCardBackGround, Card.CardType.Explain, Player.PlayerType.ETC);
        }
    }

    // 플레이어 왕 초기화
    public void ResetCrown() { crown.SetActive(false); }

    // 플레이어 건설 가능한 횟수 초기화
    public void ResetAvailableBuildCount()
    {
        availableBuildCouunt = 1;
    }

    // 플레이어 카드 갱신
    public void UpdateCard(GameObject _card, Card _newCard, Card.CardType _cardType, Player.PlayerType _playType)
    {
        _card.GetComponent<CardGet>().card = _newCard;
        if (_newCard!=null)
        {
            _card.GetComponent<CardGet>().card.cardType = _cardType; // 카드 타입 변경
            if (_newCard.cardImage != null)
            {
                if (_playType == Player.PlayerType.user) _card.GetComponent<Image>().sprite = _newCard.cardImage;
                else if (_playType == Player.PlayerType.computer && _cardType == Card.CardType.HandBuilding) _card.GetComponent<Image>().sprite = cardBackGroundImage.cardImage;
                else _card.GetComponent<Image>().sprite = _newCard.cardImage;
            }
        }
    }

    // 플레이어 카드 위치 갱신 (0: 덱, 1: 핸드, 2: 필드, 3: 파괴)
    public void UpdateCardLocate(GameObject _card, int _locate) 
    {
        if (_locate == 0) { _card.GetComponent<CardGet>().card.cardLocate = Card.CardLocate.Deck; }
        else if (_locate == 1) { _card.GetComponent<CardGet>().card.cardLocate = Card.CardLocate.Hand; }
        else if (_locate == 2) { _card.GetComponent<CardGet>().card.cardLocate = Card.CardLocate.Field; }
        else if (_locate == 3) { _card.GetComponent<CardGet>().card.cardLocate = Card.CardLocate.Destory; }
    }

    // 플레이어 카드 추가 (0: 핸드, 1: 필드)
    public void AddPlayerCard(int _type, Card _card, Player.PlayerType _playerType)
    {
        if (_type == 0) { UpdateCard(playerHandCard[currentCountPlayerHandCard], _card, Card.CardType.HandBuilding, _playerType);
                          UpdateCardLocate(playerHandCard[currentCountPlayerHandCard], 1);
                          UpdatePlayerCardActive(playerHandCard, currentCountPlayerHandCard++, 0); }
        if (_type == 1) { UpdateCard(playerFieldCard[currentCountPlayerFieldCard], _card, Card.CardType.FieldBuilding, _playerType);
                          UpdateCardLocate(playerFieldCard[currentCountPlayerFieldCard], 2);
                          UpdatePlayerCardActive(playerFieldCard, currentCountPlayerFieldCard, 0); currentCountPlayerFieldCard++; }
    }

    // 플레이어 카드 제외 (0: 핸드, 1: 필드)
    public void RemovePlayerCard(int _type, int _index, Player.PlayerType _playerType)
    {
        if (_type == 0) { UpdateCard(playerHandCard[_index], null, Card.CardType.HandBuilding, _playerType); SortingPlayerCard(playerHandCard, currentCountPlayerHandCard, _playerType); currentCountPlayerHandCard--; }
        if (_type == 1) { UpdateCardLocate(playerFieldCard[_index], 3); UpdateCard(playerFieldCard[_index], null, Card.CardType.FieldBuilding, _playerType); SortingPlayerCard(playerFieldCard, currentCountPlayerFieldCard, _playerType); currentCountPlayerFieldCard--; }
    }

    // 플레이어 핸드 필드 카드 정렬
    public void SortingPlayerCard(GameObject[] playerCard, int countPlayerCard, Player.PlayerType _playType)
    {
        int num = 0;
        foreach (GameObject _card in playerCard)
        {
            if (_card.GetComponent<CardGet>().card != null)
            {
                playerCard[num].GetComponent<CardGet>().card = _card.GetComponent<CardGet>().card;
                if (_playType == Player.PlayerType.user) playerCard[num].GetComponent<Image>().sprite = _card.GetComponent<CardGet>().card.cardImage;
                else if (_playType == Player.PlayerType.computer && playerCard[num].GetComponent<CardGet>().card.cardLocate == Card.CardLocate.Hand) playerCard[num].GetComponent<Image>().sprite = cardBackGroundImage.cardImage;
                else playerCard[num].GetComponent<Image>().sprite = _card.GetComponent<CardGet>().card.cardImage;
                num++;
            }
        }
        UpdatePlayerCardActive(playerCard, countPlayerCard-1, 1);
    }

    // 플레이어 카드 활성화/비활성화 갱신: 0: 추가, 1: 제외
    public void UpdatePlayerCardActive(GameObject[] _card, int _index, int _state)
    {
        if (_state == 0) _card[_index].SetActive(true);
        else if (_state == 1) _card[_index].SetActive(false);
    }

    // 플레이어 필드 카드 건물 종류 분별
    public int GetBuildTypeCount(int _type)
    {
        int _count=0;
        for(int i=0; i<playerFieldCard.Length; i++)
        {
            if(playerFieldCard[i].GetComponent<CardGet>().card!=null)
            {
                if (playerFieldCard[i].GetComponent<CardGet>().card.buildType == _type) _count++;
            }
        }
        return _count;
    }

    // 플레이어 왕관 비활성화/활성화
    public void SetPlayerCrown(bool _bool) { crown.SetActive(_bool); }

    // 플레이어 코인 개수 추가
    public void UpdatePlayerCoin(int _count, bool _check)
    {
        if (_check) currentCountPlayerCoin += _count;
        else currentCountPlayerCoin -= _count;
    }

    // 플레이어 현재 코인 개수대로 코인 오브젝트 갱신
    public void UpdatePlayerCoinObj()
    {
        for(int i=0; i<playerCoin.Length; i++)
        {
            if (currentCountPlayerCoin - 1 >= i) playerCoin[i].SetActive(true);
            else playerCoin[i].SetActive(false);
        }
    }

    // 플레이어 건물 가능한 건물 수 증가/감소
    public void UpdateAvailableBuildCount(int _count)
    {
        availableBuildCouunt = _count;
    }

    // 플레이어 필드 카드 Locate Destory로 설정(장군 능력 발동 시 이용)
    public void SetFieldCardLoacteDestory(bool check)
    {
        for(int i=0; i<currentCountPlayerFieldCard; i++)
        {
            if(check) playerFieldCard[i].GetComponent<CardGet>().card.cardLocate = Card.CardLocate.Destory;
            else playerFieldCard[i].GetComponent<CardGet>().card.cardLocate = Card.CardLocate.Field;
        }
    }

    public Card GetPlayerHero() { return playerHero; } // 플레이어 영웅 카드 반환
    public Vector3 GetPlayerPosition() { return position; } // 플레이어 포지션 반환
    public int GetPlayerCurrentCoinCount() { return currentCountPlayerCoin; } // 플레이어 현재 코인 개수 반환
    public int GetAvailableBuildCount() { return availableBuildCouunt; } // 건설 가능한 건물 수 반환

    public GameObject[] GetPlayerHandCard() { return playerHandCard; } // 핸드 카드 반환
    public GameObject[] GetPlayerFieldCard() { return playerFieldCard; } // 필드 카드 반환

    public int GetPlayerHandCardCount() { return currentCountPlayerHandCard; } // 핸드 카드 개수 반환
    public int GetPlayerFieldCardCount() { return currentCountPlayerFieldCard; } // 필드 카드 개수 반환
}
