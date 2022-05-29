using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TokenManager : MonoBehaviour {

    /* 관리하는 토큰 오브젝트
     * 
     * 01. 영웅 토큰(버튼 실행 함수, 토큰 이미지 갱신 등)
     * 02. 플레이어 영웅 특수 능력, 행동하기 버튼(영웅 토큰을 눌러 다른 플레이어 화면으로 갔을 때 사라져야 하는 오브젝트)
     * 03. 플레이어 관리 창 (현재 플레이어의 핸드, 필드, 코인 상태 관리)
     * 
    */

    // 게임 변수
    [SerializeField] private GameObject[] heroTokenObj; // 영웅 토큰 오브젝트
    [SerializeField] private Token[] heroToken; // 영웅 토큰
    [SerializeField] private GameObject[] playerStateCanvas; // 플레이어 토큰 캔버스 (0: 윈도우, 1: 플레이어 이름, 2: 카드 개수, 3: 코인 개수 4~8: 건물 종류)
    [SerializeField] private GameObject[] playerButton; // 플레이어 선택 버튼

    [SerializeField] private GameObject heroSelectTokenCanvas; // 영웅 특수 능력 대상자에 사용되는 오브젝트 캔버스
    [SerializeField] private GameObject[] heroSelectTokenObj; // 영웅 특수 능력 대상자에 사용되는 오브젝트

    [SerializeField] private GameObject playerActButtonObj; // 플레이어 행동 버튼 오브젝트
    [SerializeField] private GameObject playerHeroButtonObj; // 플레이어 영웅 버튼 오브젝트
    [SerializeField] private Token playerActButtonToken; // 플레이어 행동 버튼 토큰
    [SerializeField] private Token playerHeroButtonToken; // 플레이어 영웅 버튼 토큰 

    [SerializeField] private Sprite[] windowActImage; // 행동하기 창 이미지 (0: 왼쪽 코인 Off, 1: 왼쪽 코인 On, 2: 오른쪽 카드 Off, 3: 오른쪽 카드 On)
    // 행동하기 창 (0: 윈도우 창, 1: 왼쪽 버튼, 2: 오른쪽 버튼, 3: 왼쪽 선택 버튼, 4: 오른쪽 선택 버튼 5: 왼쪽 텍스트, 6: 오른쪽 텍스트. 7: 오른쪽 경고 텍스트)
    [SerializeField] private GameObject[] windowAct;
    [SerializeField] private GameObject[] windowCard; // 카드뽑기 창 (0: 윈도우 창, 1: 왼쪽 카드, 2: 오른쪽 카드)
    private bool windowActBool; // 행동하기 창 참값
    private bool selectCoinBool; // 코인 선택 참값
    private bool selectCardBool; // 카드 선택 참값

    [SerializeField] private PlayEngine thePlayEngine;
    [SerializeField] private CardManager theCardManager;
    [SerializeField] private ButtonManager theButtonManager;
    [SerializeField] private HeroManager theHeroManager;
    [SerializeField] private SpecialCardManager theSpecialCardManager;

    [SerializeField] private GameObject theCamera;

    private GameObject currentHeroToken; // User가 선택하는 Hero Token
    private GameObject currentRoundPlayHeroToken; // 현재 플레이 중인 Hero Token
    private GameObject currentSelectHeroToken; // '암살자', '도둑', '마법사' 영웅 능력에 필요한 대상자

    private int deathPlayerHeroNumber; // 암살 당할 PLayer 영웅 번호
    private int stolenPlayerHeroNumber; // 도둑 당할 Player 영웅 번호
    private int exchangePlayerHeroNumber; // 교환 당할 Player 영웅 번호

    private int currentPlayerPlayState; // 현재 User의 '행동하기' 상태 (0: 없음, 1: 행동하기, 2: 행동하기 누른 상태, 3: 카드 선택한 경우, 4: 턴 넘기기, 5: 영웅 토큰 선택, 6: 마법시 능력 사용)

    // 영웅 토큰 초기화
    public void InitToken()
    {
        if (thePlayEngine.GetCurrentRound() == 1) currentHeroToken = null;
        currentSelectHeroToken = null;
        currentRoundPlayHeroToken = null;

        deathPlayerHeroNumber = 0;
        stolenPlayerHeroNumber = 0;
        exchangePlayerHeroNumber = -1;

        // 플레이어 상태창 초기화
        for(int i=1; i<playerButton.Length; i++) playerButton[i].SetActive(false);
        if (thePlayEngine.GetCurrentRound() == 1) playerStateCanvas[0].SetActive(false);

        // 영웅 토큰 초기화
        for (int i=0; i < heroTokenObj.Length; i++) {
            heroTokenObj[i].GetComponent<TokenGet>().index = i;

            if(thePlayEngine.GetCurrentRound() == 1){
                // 토큰 초기화
                heroTokenObj[i].GetComponent<TokenGet>().token = heroToken[i];
                heroTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                heroTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[0];

                // 선택 토큰 초기화
                heroSelectTokenObj[i].GetComponent<TokenGet>().index = i;
                heroSelectTokenObj[i].GetComponent<TokenGet>().token = heroToken[i];
                heroTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                heroTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[0];
            }

            // 토큰 아이콘 초기화
            heroTokenObj[i].GetComponent<TokenGet>().icons[0].SetActive(false);
            heroTokenObj[i].GetComponent<TokenGet>().icons[1].SetActive(false);
        }

        // 영웅 선택 캔버스 비활성화
        heroSelectTokenCanvas.SetActive(false);

        // 플레이어 버튼 초기화
        playerActButtonObj.GetComponent<TokenGet>().token = playerActButtonToken;
        playerActButtonObj.GetComponent<TokenGet>().token.player = thePlayEngine.GetUserPlayer(); // 기본 유저 플레이어 버튼 초기화
        playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Hide;
        playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[0];
        playerActButtonObj.SetActive(false);

        playerHeroButtonObj.GetComponent<TokenGet>().token = playerHeroButtonToken;
        playerHeroButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.PowerOff;
        playerHeroButtonObj.GetComponent<Image>().sprite = playerHeroButtonToken.stateImages[1];
        playerHeroButtonObj.GetComponent<CardGet>().card = null;

        // 행동하기 창 초기화 
        windowActBool = false;
        selectCoinBool = false;
        selectCardBool = false;
        windowAct[0].SetActive(false);
        windowAct[1].GetComponent<Image>().sprite = windowActImage[0];
        windowAct[2].GetComponent<Image>().sprite = windowActImage[2];
        windowAct[3].SetActive(false);
        windowAct[4].SetActive(false);
        windowAct[5].SetActive(true);
        windowAct[6].SetActive(true);
        windowAct[7].SetActive(false);
        windowCard[0].SetActive(false);

        // User Player 플레이 상태 초기화
        currentPlayerPlayState = 0;
    }

    // 영웅 토큰 세팅
    public void SetHeroToken(Player[] _players)
    {
        heroSelectTokenCanvas.SetActive(true);
        for (int i=0; i<heroTokenObj.Length; i++){
            int _index = -1;
            for (int j=0; j< _players.Length; j++){
                if(_players[j].GetPlayerHero().cardNumber==i+1) _index=j; 
            }
            if (_index != -1) { // 영웅 번호가 플레이어 정보 안에 존재한다면
                heroTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                heroTokenObj[i].GetComponent<TokenGet>().position = _players[_index].GetPlayerPosition();
                heroTokenObj[i].GetComponent<TokenGet>().token.player = _players[_index];

                heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                heroSelectTokenObj[i].GetComponent<TokenGet>().token.player = _players[_index];
            }
            else // 선택되지 않은 영웅이라면
            {
                heroTokenObj[i].GetComponent<TokenGet>().token.player = null;
                heroTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Ban;

                heroSelectTokenObj[i].GetComponent<TokenGet>().token.player = null;
                heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Ban;
            }
        }
        heroSelectTokenCanvas.SetActive(false);
    }

    // 해당 라운드 영웅 토큰 초기화
    public void ResetRoundHeroToken()
    {
        for (int i = 0; i < heroTokenObj.Length; i++)
        {
            if (heroTokenObj[i].GetComponent<TokenGet>().token.player == null)
            {
                heroTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Ban;
                heroTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[0];
            }
            else if (heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType == Token.StateType.Death)
            {
                heroTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                heroTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[0];
            }
        }

        // 현재 플레이 중인 영웅 토큰 갱신
        heroTokenObj[thePlayEngine.GetCurrentPlayer().GetPlayerHero().cardNumber - 1].GetComponent<TokenGet>().token.stateType = Token.StateType.Play;
        currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateType = Token.StateType.Play;
        currentRoundPlayHeroToken.GetComponent<Image>().sprite = heroTokenObj[thePlayEngine.GetCurrentPlayer().GetPlayerHero().cardNumber - 1].GetComponent<TokenGet>().token.stateImages[4];
    }


    // 해당 라운드 영웅 토큰 리셋(밴 된 상태 유지)
    public void ResetHeroToken()
    {
        for (int i = 0; i < heroTokenObj.Length; i++)
        {
            if (heroTokenObj[i].GetComponent<TokenGet>().token.player == null)
            {
                heroTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Ban;
                heroTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[1];
            }
            else if (heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType == Token.StateType.Death)
            {
                heroTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                heroTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[0];
            }
        }

        // 현재 플레이 중인 영웅 토큰 갱신
        heroTokenObj[thePlayEngine.GetCurrentPlayer().GetPlayerHero().cardNumber - 1].GetComponent<TokenGet>().token.stateType = Token.StateType.Play;
        currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateType = Token.StateType.Play;
        currentRoundPlayHeroToken.GetComponent<Image>().sprite = heroTokenObj[thePlayEngine.GetCurrentPlayer().GetPlayerHero().cardNumber - 1].GetComponent<TokenGet>().token.stateImages[4];
    }

    // 영웅 토큰 밴 
    public void SetHeroTokenBan()
    {
        for(int i=0; i<heroTokenObj.Length; i++)
        {
            heroTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[0];
            if (heroTokenObj[i].GetComponent<TokenGet>().token.stateType == Token.StateType.Ban) heroTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[1];
        }
    }

    // 영웅 토큰 암살
    public void SetHeroTokenDeath(int _index)
    {
        deathPlayerHeroNumber = _index + 1;
        heroTokenObj[_index].GetComponent<TokenGet>().icons[0].SetActive(true);
        if (heroTokenObj[_index].GetComponent<TokenGet>().token.stateType != Token.StateType.Ban)
        {
            heroTokenObj[_index].GetComponent<TokenGet>().token.stateType = Token.StateType.Death;
        }

        // 영웅 토큰 상태 갱신
        ResetHeroToken();

        playerHeroButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.PowerOff;
    }

    // 영웅 토큰 도둑
    public void SetHeroTokenStolen(int _index)
    {
        stolenPlayerHeroNumber = _index + 1;
        heroTokenObj[_index].GetComponent<TokenGet>().icons[1].SetActive(true);

        // 영웅 토큰 상태 갱신
        ResetHeroToken();

        playerHeroButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.PowerOff;
    }

    // 영웅 토큰 교환
    public void ExchangeHeroToken(int _index)
    {
        exchangePlayerHeroNumber = _index + 1;

        Player player1 = thePlayEngine.GetCurrentPlayer(); // 교환 할 플레이어
        Player player2 = heroTokenObj[exchangePlayerHeroNumber-1].GetComponent<TokenGet>().token.player; // 교환 당할 플레이어

        int handCardsCountPlayer1 = player1.GetPlayerHandCardCount();
        int handCardsCountPlayer2 = player2.GetPlayerHandCardCount();

        GameObject[] handCardsPlayer1 = player1.GetPlayerHandCard(); Card[] newHandCardsPlayer1 = new Card[handCardsCountPlayer1];
        GameObject[] handCardsPlayer2 = player2.GetPlayerHandCard(); Card[] newHandCardsPlayer2 = new Card[handCardsCountPlayer2];

        // player1 손패 카드 저장하기
        for (int i = 0; i < handCardsCountPlayer1; i++) newHandCardsPlayer1[i] = handCardsPlayer1[i].GetComponent<CardGet>().card;

        // player2 손패 카드 저장하기 
        for (int i=0; i<handCardsCountPlayer2; i++) newHandCardsPlayer2[i] = handCardsPlayer2[i].GetComponent<CardGet>().card;

        // player1 손패 카드 삭제
        for (int i = 0; i < handCardsCountPlayer1; i++) player1.RemovePlayerCard(0, i, player1.playerType);
        // player2 손패 카드 삭제
        for (int i = 0; i < handCardsCountPlayer2; i++) player2.RemovePlayerCard(0, i, player2.playerType);

        // player1 손패에 player2 손패 집어넣기
        for (int i = 0; i < newHandCardsPlayer2.Length; i++) player1.AddPlayerCard(0, newHandCardsPlayer2[i], player1.playerType);
        // player2 손패에 player1 손패 집어넣기
        for (int i = 0; i < newHandCardsPlayer1.Length; i++) player2.AddPlayerCard(0, newHandCardsPlayer1[i], player2.playerType);
    }

    // 현재 플레이 중인 토큰 세팅
    public void SetCurrentPlayToken(int _index)
    {
        // 이전 토큰 상태 갱신
        if (currentRoundPlayHeroToken == null) currentRoundPlayHeroToken = heroTokenObj[_index];
        else
        {
            if(currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateType == Token.StateType.Play)
            {
                currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                currentRoundPlayHeroToken.GetComponent<Image>().sprite = currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateImages[0];
                currentRoundPlayHeroToken.GetComponent<RectTransform>().position -= new Vector3(0.2f, 0, 0);
            }
        }

        currentRoundPlayHeroToken = heroTokenObj[_index];

        // 플레이 중인 토큰으로 갱신
        heroTokenObj[_index].GetComponent<TokenGet>().token.stateType = Token.StateType.Play;
        currentRoundPlayHeroToken.GetComponent<RectTransform>().position += new Vector3(0.2f, 0, 0);
        currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateType = Token.StateType.Play;
        currentRoundPlayHeroToken.GetComponent<Image>().sprite = heroTokenObj[_index].GetComponent<TokenGet>().token.stateImages[4];
    }

    // 현재 플레이 중인 토큰 리셋
    public void ResetCurrentPlayToken()
    {
        if(currentRoundPlayHeroToken != null){
            if (currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateType == Token.StateType.Play){
                currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                currentRoundPlayHeroToken.GetComponent<Image>().sprite = currentRoundPlayHeroToken.GetComponent<TokenGet>().token.stateImages[0];
                currentRoundPlayHeroToken.GetComponent<RectTransform>().position -= new Vector3(0.2f, 0, 0);
            }
        }

        currentRoundPlayHeroToken = null;
    }

    // 플레이어 상태 창 업데이트
    public void UpdatePlayerStateCanvas(Player _player)
    {
        playerStateCanvas[0].SetActive(true);
        playerStateCanvas[1].GetComponent<Text>().text = _player.playerName;
        playerStateCanvas[2].GetComponent<Text>().text = "x" + _player.currentCountPlayerHandCard.ToString();
        playerStateCanvas[3].GetComponent<Text>().text = "x" + _player.currentCountPlayerCoin.ToString();
        playerStateCanvas[4].GetComponent<Text>().text = "x" + _player.GetBuildTypeCount(1).ToString();
        playerStateCanvas[5].GetComponent<Text>().text = "x" + _player.GetBuildTypeCount(2).ToString();
        playerStateCanvas[6].GetComponent<Text>().text = "x" + _player.GetBuildTypeCount(3).ToString();
        playerStateCanvas[7].GetComponent<Text>().text = "x" + _player.GetBuildTypeCount(4).ToString();
        playerStateCanvas[8].GetComponent<Text>().text = "x" + _player.GetBuildTypeCount(5).ToString();
    }

    // 픽한 플레이어 화면 보여주기
    public void UpdateCurrentPickToken(GameObject _heroToken)
    {
        // 이전의 토큰 초기화
        if (currentHeroToken != null)
        {
            // 현재 플레이 중인지 체크
            if (currentHeroToken.GetComponent<TokenGet>().token.stateType == Token.StateType.Play)
            {
                currentHeroToken.GetComponent<Image>().sprite = currentHeroToken.GetComponent<TokenGet>().token.stateImages[4];
            }
            else
            {
                currentHeroToken.GetComponent<Image>().sprite = currentHeroToken.GetComponent<TokenGet>().token.stateImages[0];
                currentHeroToken.GetComponent<TokenGet>().token.stateType = Token.StateType.Active; 
            }
        }

        currentHeroToken = _heroToken;
        if (currentHeroToken == null) currentHeroToken = heroTokenObj[thePlayEngine.GetUserPlayer().playerHero.cardNumber-1]; // 컴퓨터가 아닌 유저 세팅 화면 초기화
        currentHeroToken.GetComponent<Image>().sprite = currentHeroToken.GetComponent<TokenGet>().token.stateImages[3];
        if(currentHeroToken.GetComponent<TokenGet>().token.stateType != Token.StateType.Play) currentHeroToken.GetComponent<TokenGet>().token.stateType = Token.StateType.Pick;
        theCamera.GetComponent<Transform>().position = currentHeroToken.GetComponent<TokenGet>().token.player.GetPlayerPosition(); // 카메라 이동시키기

        if(thePlayEngine.GetCurrentState() == 2 )
        {
            // Computer 화면일 시 '돌아가기' 버튼 추가
            if (currentHeroToken != heroTokenObj[thePlayEngine.GetUserPlayer().playerHero.cardNumber - 1])
            {
                if (theHeroManager.IsUsingWarlordPower())
                {
                    playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Skip;
                    SetPlayerButtonSkip();
                }
                else if (currentPlayerPlayState != 2 && currentPlayerPlayState != 3)
                {
                    playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Return;
                    SetPlayerButtonReturn();
                }
            }
            // Player 화면일 시 '돌아가기' 버튼 삭제 후 UserPlayState에 따라 버튼 세팅
            else
            {
                if (currentPlayerPlayState == 0) { playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Act; playerActButtonObj.SetActive(false); }
                else if (theHeroManager.IsUsingWarlordPower()) { playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Skip; SetPlayerButtonSkip(); }
                else if (currentPlayerPlayState == 1) { SetPlayerButtonAct(); }
                else if (currentPlayerPlayState == 4) { SetPlayerButtonEnd(); }
            }
        }
    }

    // 현재 화면의 플레이어가 User인지 판별하는 함수
    public bool CheckCurrentTokenPlayer()
    {
        if (thePlayEngine.GetCurrentState() == 2)
        {
            if (currentHeroToken.GetComponent<TokenGet>().token.player == thePlayEngine.GetUserPlayer()) return true;
            else return false;
        }
        return true;
    }

    // User 화면으로 돌아가기
    public void ReturnUserWindow()
    {
        UpdatePlayerStateCanvas(thePlayEngine.GetUserPlayer());
        UpdateCurrentPickToken(heroTokenObj[thePlayEngine.GetUserPlayer().playerHero.cardNumber - 1]);
    }

    // 영웅 선택 화면 세팅(1: 암살자, 2: 도둑, 3: 마법사)
    public void SetHeroSelectCanvas(int _hero)
    {
        heroSelectTokenCanvas.SetActive(true);
        theButtonManager.UpdateHeplCanvas();

        // 암살자
        if (_hero == 1)
        {
            playerHeroButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.PowerActive;

            heroSelectTokenObj[0].GetComponent<TokenGet>().token.stateType = Token.StateType.Death;
            heroSelectTokenObj[0].GetComponent<Image>().sprite = heroToken[0].stateImages[1];
        }
        // 도둑
        else if (_hero == 2)
        {
            playerHeroButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.PowerActive;

            heroSelectTokenObj[0].GetComponent<TokenGet>().token.stateType = Token.StateType.Death;
            heroSelectTokenObj[1].GetComponent<TokenGet>().token.stateType = Token.StateType.Death;
            heroSelectTokenObj[0].GetComponent<Image>().sprite = heroToken[0].stateImages[1];
            heroSelectTokenObj[1].GetComponent<Image>().sprite = heroToken[1].stateImages[1];

            // 암살자에게 지목당한 영웅 토큰 빼기
            heroSelectTokenObj[deathPlayerHeroNumber - 1].GetComponent<TokenGet>().token.stateType = Token.StateType.Death;
            heroSelectTokenObj[deathPlayerHeroNumber - 1].GetComponent<Image>().sprite = heroToken[deathPlayerHeroNumber - 1].stateImages[1];
        }
        // 마법사
        else if (_hero == 3)
        {
            for(int i=0; i<heroSelectTokenObj.Length; i++){

                // 이전 선택 영웅 토큰 초기화
                if(heroSelectTokenObj[i].GetComponent<TokenGet>().token.player == null)
                {
                    heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Ban;
                }
                else if (heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType == Token.StateType.Death){
                    heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Active;
                    heroSelectTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[0];
                }

                // 자신의 직업 마법사 영웅 토큰 설정
                heroSelectTokenObj[2].GetComponent<TokenGet>().token.stateType = Token.StateType.Death;
                heroSelectTokenObj[2].GetComponent<Image>().sprite = heroToken[2].stateImages[1];

                // 해당 라운드에서 밴 당한 영웅 토큰 설정
                if (heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType == Token.StateType.Ban){
                    heroSelectTokenObj[i].GetComponent<TokenGet>().token.stateType = Token.StateType.Death;
                    heroSelectTokenObj[i].GetComponent<Image>().sprite = heroToken[i].stateImages[1];
                }
            }
        }
        SetPlayerButtonHide();
    }

    // 영웅 선택 버튼 클릭 시
    public void OnClickHeroSelectButton(GameObject hero)
    {
        if(hero.GetComponent<TokenGet>().token.stateType != Token.StateType.Death)
        {
            if (currentSelectHeroToken == null)
            {
                currentSelectHeroToken = hero;
            }
            else
            {
                currentSelectHeroToken.GetComponent<Image>().sprite = currentSelectHeroToken.GetComponent<TokenGet>().token.stateImages[0];

                // 선택 된 영웅 토큰을 다시 클릭하면 '선택' 버튼이 사라짐
                if (currentSelectHeroToken.GetComponent<TokenGet>().token == hero.GetComponent<TokenGet>().token)
                {
                    SetPlayerButtonHide(); currentSelectHeroToken = null;
                    return;
                }
            }

            currentSelectHeroToken.GetComponent<Image>().sprite = currentSelectHeroToken.GetComponent<TokenGet>().token.stateImages[5];
            SetPlayerButtonSelect();
        }
    }

    // 버튼

    // 영웅 토큰 누를 시
    public void OnClickHeroToken(GameObject _heroToken)
    {
        // 현재 라운드 진행 중이고 User의 영웅이 '암살자' 혹은 '도둑'의 능력을 시전 중이 아닐 시 (영웅 능력을 사용 체크하는 변수 이용)
        if(!(theHeroManager.IsUsingAssassinPower() || theHeroManager.IsUsingThiefPower()))
        {
            // 0 라운드 처음 영웅 고르기 할 시 버튼 못 누르게 하기
            if (!(thePlayEngine.GetCurrentRound() == 1 && thePlayEngine.GetCurrentState() != 2))
            {
                // 밴된 영웅 토큰을 클릭하지 못한다.
                if (_heroToken.GetComponent<TokenGet>().token.stateType != Token.StateType.Ban && thePlayEngine.GetCurrentState() != 0)
                {
                    UpdateCurrentPickToken(_heroToken);
                    UpdatePlayerStateCanvas(_heroToken.GetComponent<TokenGet>().token.player);
                }
            }
        }
    }

    // '행동하기' 버튼 초기화
    public void InitPlayerButtonState()
    {
        currentPlayerPlayState = 0;
    }

    // 숨기기 버튼으로 세팅
    public void SetPlayerButtonHide()
    {
        // '행동하기' 창을 숨기는 것인지 체크하여 State 갱신
        if (thePlayEngine.GetCurrentState() == 2)
        {
            if(!theHeroManager.GetMegicianPowerBool() && !theHeroManager.IsUsingPlayerExchange()) currentPlayerPlayState = 2;
        }

        playerActButtonObj.SetActive(true);
        playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Hide;
        playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[0];
    }

    // 선택하기 버튼으로 세팅
    public void SetPlayerButtonSelect()
    {
        // currentPlayerPlayState = 5;
        playerActButtonObj.SetActive(true);
        playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.SelectHero;
        playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[5];
    }

    // 행동하기 버튼으로 세팅
    public void SetPlayerButtonAct()
    {
        currentPlayerPlayState = 1;
        playerActButtonObj.SetActive(true);
        playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Act;
        playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[2];
    }

    // 턴 넘기기 버튼으로 세팅
    public void SetPlayerButtonEnd()
    {
        currentPlayerPlayState = 4;
        playerActButtonObj.SetActive(true);
        playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.End;
        playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[3];
    }

    // 돌아가기 버튼으로 세팅
    public void SetPlayerButtonReturn()
    {
        playerActButtonObj.SetActive(true);
        playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Return;
        playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[4];
    }

    // 넘기기 버튼으로 세팅 (8번 영웅 장군 능력 사용 시 작동)
    public void SetPlayerButtonSkip()
    {
        playerActButtonObj.SetActive(true);
        playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Skip;
        playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[6];
    }

    // 행동 버튼 누를 시
    public void OnClickPlayerAct()
    {
        Token.StateType _type = playerActButtonObj.GetComponent<TokenGet>().token.stateType;

        // 숨기기 및 보이기(행동하기 창)
        if (_type == Token.StateType.Hide || _type == Token.StateType.Show){
            // '암살자', '도둑', '마법사' 영웅 토큰 선택 창
            if (!(theHeroManager.IsUsingAssassinPower() || theHeroManager.IsUsingThiefPower() || (theHeroManager.GetMegicianPowerBool() || theHeroManager.IsUsingPlayerExchange()))) 
            {
                // 숨기기
                if (_type == Token.StateType.Hide)
                {
                    theCardManager.SetHeroCanvas(false); playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Show;
                    playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[1];
                }
                // 보이기
                else if (_type == Token.StateType.Show)
                {
                    theCardManager.SetHeroCanvas(true); playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Hide;
                    playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[0];
                }
            }
            // '마법사' 교환 선택 창
            else if(theHeroManager.GetMegicianPowerBool() == true)
            {
                // 숨기기
                if (_type == Token.StateType.Hide)
                {
                    theHeroManager.SetWindowMegicianActive(false); playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Show;
                    playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[1];
                }
                // 보이기
                else if (_type == Token.StateType.Show)
                {
                    theHeroManager.SetWindowMegicianActive(true); playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Hide;
                    playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[0];
                }
            }
            else // 영웅 선택 창
            {
                // 숨기기
                if (_type == Token.StateType.Hide)
                {
                    heroSelectTokenCanvas.SetActive(false); playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Show;
                    playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[1];
                }
                // 보이기
                else if (_type == Token.StateType.Show)
                {
                    heroSelectTokenCanvas.SetActive(true); playerActButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.Hide;
                    playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[0];
                }
            }
        }

        // '암살자', '도둑' 능력 사용 대상자 선택
        else if(_type == Token.StateType.SelectHero){

            // 암살자 영웅
            if (theHeroManager.IsUsingAssassinPower())
            {
                SetHeroTokenDeath(currentSelectHeroToken.GetComponent<TokenGet>().index);
                currentSelectHeroToken.GetComponent<Image>().sprite = currentSelectHeroToken.GetComponent<TokenGet>().token.stateImages[0];

                currentSelectHeroToken = null;
                heroSelectTokenCanvas.SetActive(false);
                theHeroManager.UpdateAssassinPower(false);
                theButtonManager.UpdateHeplCanvas();

                SetHeroTokenBan();
                SetPlayerButtonAct();
            }

            // 도둑 영웅
            if (theHeroManager.IsUsingThiefPower())
            {
                SetHeroTokenStolen(currentSelectHeroToken.GetComponent<TokenGet>().index);
                currentSelectHeroToken.GetComponent<Image>().sprite = currentSelectHeroToken.GetComponent<TokenGet>().token.stateImages[0];

                currentSelectHeroToken = null;
                heroSelectTokenCanvas.SetActive(false);
                theHeroManager.UpdateThiefPower(false);
                theButtonManager.UpdateHeplCanvas();

                SetHeroTokenBan();
                SetPlayerButtonAct();
            }

            // 마법사 영웅
            if(theHeroManager.IsUsingPlayerExchange())
            {
                ExchangeHeroToken(currentSelectHeroToken.GetComponent<TokenGet>().index);
                currentSelectHeroToken.GetComponent<Image>().sprite = currentSelectHeroToken.GetComponent<TokenGet>().token.stateImages[0];

                currentSelectHeroToken = null;
                heroSelectTokenCanvas.SetActive(false);
                theHeroManager.UpdatePlayerExchange(false);
                theButtonManager.UpdateHeplCanvas();

                SetHeroTokenBan();
                if (GetCurrentPlayerPlayState() == 1) SetPlayerButtonAct();
                else if (GetCurrentPlayerPlayState() == 4) SetPlayerButtonEnd();
            }
        }

        // 행동하기
        else if(_type == Token.StateType.Act){
            // [천문대: 일반 행동을 할 때, '금화 얻기'와 '카드 얻기' 모두 사용할 수 있습니다]
            if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.OBSERVATORY, thePlayEngine.GetCurrentPlayer()))
            {
                // Player 금화 2개 추가
                thePlayEngine.GetUserPlayer().UpdatePlayerCoin(2, true);
                thePlayEngine.GetUserPlayer().UpdatePlayerCoinObj();

                // 카드 2장 중 1장 선택
                SelectCard();
            }
            else
            {
                // '행동하기' 버튼 누른 상태로 갱신
                if (currentPlayerPlayState == 1) currentPlayerPlayState = 2;

                // 행동하기 창 띄운 뒤 보이기 숨기기 버튼 작동
                windowActBool = !windowActBool;
                if (windowActBool)
                {
                    playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[0];
                    if (currentPlayerPlayState == 2) windowAct[0].SetActive(true);
                    else if (currentPlayerPlayState == 3) windowCard[0].SetActive(true);
                }
                else
                {
                    playerActButtonObj.GetComponent<Image>().sprite = playerActButtonToken.stateImages[1];
                    if (currentPlayerPlayState == 2) windowAct[0].SetActive(false);
                    else if (currentPlayerPlayState == 3) windowCard[0].SetActive(false);
                }
            }
        }

        // 돌아가기
        else if(_type == Token.StateType.Return){
            ReturnUserWindow();

            if (currentPlayerPlayState == 0) playerActButtonObj.SetActive(false);
            else if (currentPlayerPlayState == 1) SetPlayerButtonAct();
            else if (currentPlayerPlayState == 3) SetPlayerButtonEnd();
        }

        // 턴 넘기기
        else if(_type == Token.StateType.End){
            InitPlayerButtonState();
            playerActButtonObj.SetActive(false);
            thePlayEngine.NextTurn();
        }

        // 실험실 특수 카드 능력 스킵 or 8번 영웅 장군 능력 스킵
        else if(_type == Token.StateType.Skip){
            ReturnUserWindow();

            // 장군 능력 스킵
            if (theHeroManager.IsUsingWarlordPower())
            {
                theHeroManager.UpdateWarlordPower(false);
                theButtonManager.UpdateHeplCanvas();

                theHeroManager.SetPlayerFieldCardDestory(false);
            }
            // 실험실 능력 스킵
            else if (theSpecialCardManager.IsUseLabortoryPower())
            {
                theSpecialCardManager.UpdateLabortoryPower(false);
            }

            if (GetCurrentPlayerPlayState() == 1) SetPlayerButtonAct();
            else if (GetCurrentPlayerPlayState() == 4) SetPlayerButtonEnd();
        }
    }

    // 행동하기 : 금화 2개 혹은 카드 2장 중 선택

    public void OnClickCoinWindow()
    {
        if (selectCardBool){
            windowAct[2].GetComponent<Image>().sprite = windowActImage[2];
            windowAct[4].SetActive(false);
            windowAct[6].SetActive(true);
            selectCardBool = false;
        }

        selectCoinBool = !selectCoinBool;
        if (selectCoinBool){
            windowAct[1].GetComponent<Image>().sprite = windowActImage[1];
            windowAct[3].SetActive(true);
            windowAct[5].SetActive(false);
        }
        else{
            windowAct[1].GetComponent<Image>().sprite = windowActImage[0];
            windowAct[3].SetActive(false);
            windowAct[5].SetActive(true);
        }
    }

    public void SelectCoin()
    {
        windowActBool = false;
        selectCoinBool = false;
        selectCardBool = false;
        windowAct[0].SetActive(false);

        // Player 금화 2개 추가
        thePlayEngine.GetUserPlayer().UpdatePlayerCoin(2, true);
        thePlayEngine.GetUserPlayer().UpdatePlayerCoinObj();

        // '턴 넘기기' 상태로 이동
        SetPlayerButtonEnd();

        // 다른 Computer 화면을 보고 있었다면 User 화면으로 돌아가기
        ReturnUserWindow();
    }

    public void OnClickCardWindow()
    {
        if (selectCoinBool){
            windowAct[1].GetComponent<Image>().sprite = windowActImage[0];
            windowAct[3].SetActive(false);
            windowAct[5].SetActive(true);
            selectCoinBool = false;
        }

        selectCardBool = !selectCardBool;
        if (selectCardBool){
            windowAct[2].GetComponent<Image>().sprite = windowActImage[3];
            windowAct[4].SetActive(true);
            windowAct[6].SetActive(false);
        }
        else{
            windowAct[2].GetComponent<Image>().sprite = windowActImage[2];
            windowAct[4].SetActive(false);
            windowAct[6].SetActive(true);
        }
    }

    public void SelectCard()
    {

        // [도서관: 일반 행동을 할 때 카드를 가져오기로 결정했다면, 카드 2장을 모두 손에 들 수 있습니다] 
        if (theSpecialCardManager.IsSpecialCard(theSpecialCardManager.LIBRARY, thePlayEngine.GetCurrentPlayer())){
            if (thePlayEngine.GetUserPlayer().GetPlayerHandCardCount() < 6)
            {
                windowActBool = false;
                selectCoinBool = false;
                selectCardBool = false;
                windowAct[0].SetActive(false);
                currentPlayerPlayState = 3;

                // 카드 2장 뽑기
                theSpecialCardManager.UseLibrary();

                SetPlayerButtonEnd();
                CloseWindowCard();

                // 다른 Computer 화면을 보고 있었다면 User 화면으로 돌아가기
                ReturnUserWindow();
            }
            else
            {
                windowAct[7].SetActive(true); 
            }
        }
        else
        {
            if (thePlayEngine.GetUserPlayer().GetPlayerHandCardCount() < 7)
            {
                windowActBool = false;
                selectCoinBool = false;
                selectCardBool = false;
                windowAct[0].SetActive(false);
                currentPlayerPlayState = 3;
                windowCard[0].SetActive(true);

                // 덱에서 2장 뽑아 오브젝트 배치하기
                Card _card1 = theCardManager.DrawBuildingCard(); Card _card2 = theCardManager.DrawBuildingCard();
                windowCard[1].GetComponent<CardGet>().card = _card1;
                windowCard[1].GetComponent<Image>().sprite = _card1.cardImage;
                windowCard[2].GetComponent<CardGet>().card = _card2;
                windowCard[2].GetComponent<Image>().sprite = _card2.cardImage;
            }
            else
            {
                windowAct[7].SetActive(true);
            }
        }
    }

    // 카드 2장 중 뽑지 못한 카드 다시 덱으로 넣기
    public void ReturnLeftCard(Card _card)
    {
        if (windowCard[1].GetComponent<CardGet>().card == _card) theCardManager.ReturnBuildingCard(windowCard[1].GetComponent<CardGet>().card);
        else theCardManager.ReturnBuildingCard(windowCard[2].GetComponent<CardGet>().card);
    }

    // 영웅 버튼 갱신
    public void SetplayerHeroButtonObj(Card _hero)
    {
        _hero.cardType = Card.CardType.Hero;
        playerHeroButtonObj.GetComponent<CardGet>().card = _hero;
    }

    // 영웅 버튼 활성화/비활성화
    public void UpdateplayerHeroButtonObj(bool _bool)
    {
        // '효과 발동' 비활성화
        if(playerHeroButtonObj.GetComponent<CardGet>().card != null) playerHeroButtonObj.GetComponent<CardGet>().card.cardType = Card.CardType.Explain;

        if (_bool)
        {
            playerHeroButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.PowerActive;
            playerHeroButtonObj.GetComponent<Image>().sprite = playerHeroButtonToken.stateImages[0];
        }
        else
        {
            playerHeroButtonObj.GetComponent<TokenGet>().token.stateType = Token.StateType.PowerOff;
            playerHeroButtonObj.GetComponent<Image>().sprite = playerHeroButtonToken.stateImages[1];
        }
    }

    // 영웅 버튼 누를 시
    public void OnClickPlayerHero()
    {
        if (playerHeroButtonObj.GetComponent<TokenGet>().token.stateType == Token.StateType.PowerActive 
            && (thePlayEngine.GetUserPlayer().GetPlayerHero().cardNumber!=1 && thePlayEngine.GetUserPlayer().GetPlayerHero().cardNumber != 2))
        {
            playerHeroButtonObj.GetComponent<CardGet>().card.cardType = Card.CardType.Hero;
            theButtonManager.OnClickCard(playerHeroButtonObj); 
        }
    }

    public Player GetHeroTokenPlayer(int _index) { return heroTokenObj[_index].GetComponent<TokenGet>().token.player; } // 영웅 토큰 안에 내재된 플레이어 반환
    public Player GetPickHeroTokenPlayer() { return currentHeroToken.GetComponent<TokenGet>().token.player; } // 현재 선택하고 있는 영웅 토큰 안의 플레이어 반환

    public void CloseWindowCard() { windowCard[0].SetActive(false); }
    public int GetCurrentPlayerPlayState() { return currentPlayerPlayState; } // 플레이 버튼 상태 반환
    public Token.StateType GetHeroButtonStateType() { return playerHeroButtonObj.GetComponent<TokenGet>().token.stateType; }

    public int GetDeathPlayerHeroNumber() { return deathPlayerHeroNumber; } // 암살 당한 Plyer 영웅 번호 반환
    public int GetStolenPlayerHeroNumber() { return stolenPlayerHeroNumber; } // 도둑 당한 Player 영웅 번호 반환
}
