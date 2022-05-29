using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "New Card/card")]
public class Card : ScriptableObject
{
    public string cardName; // 카드 이름
    public string cardName_English; // 카드 이름(영어)
    public int cardNumber; // 카드 번호
    public int price; // 카드 가격
    public CardType cardType; // 카드 유형
    public CardLocate cardLocate; // 카드 위치
    public int buildType; // 건물 유형(0: 건물X, 1: 상업, 2: 종교, 3: 귀족, 4: 군사, 5: 특수)

    public Sprite cardImage; // 카드 이미지
    public string cardInfo; // 카드 정보

    public enum CardType
    {
        Hero,
        HandBuilding,
        FieldBuilding,
        Explain,
        Pick
    }
    
    public enum CardLocate
    {
        Deck,
        Hand,
        Field,
        Destory
    }

    public void SetCardName(string _newName) { cardName = _newName; }
    public void SetCardImage(Sprite _newImage) { cardImage = _newImage; }
}
