using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 룰을 구현한 클래스.
/// </summary>
public class PlayEngine : MonoBehaviour {

    /*
     * 
     * 게임 진행(플레이어 6명 기준)
     * 01. 랜덤으로 플레이어 순번 매기기
     * 02. 랜덤으로 왕관을 가질 사람 뽑기
     * 03. 왕관을 가진 사람이 8개의 카드(이때 카드는 모두 뒷면 표시) 중 
     *     하나를 골라 그 영웅 카드는 못 쓰게 한다.
     *     * 이때 토큰 표시는 한 라운드가 모두 끝나면 표시한다.
     *     * 한 라운드가 끝나면 사용한 영웅 번호 순서대로 새로운 영웅을 뽑는다.
     *     
     * 04. 행동하기
     *    - 특수 능력 사용(선택, 경우에 따라 필수)
     *    - 자기 자원 얻기 행동
     *    - 건설 행동(선택)
     *    - 턴 넘기기
     *    
     *    행동하기 버튼을 누른다 => 동전 2개 or 카드 얻기 택1
     * 
    */

    // 게임 변수
    [SerializeField] private Player[] players;
    [SerializeField] private CardManager theCardManager;
    [SerializeField] private ButtonManager theButtonManager;
    [SerializeField] private TokenManager theTokenManager;
    [SerializeField] private HeroManager theHeroManager;

    [SerializeField] private AI _AI;

    [SerializeField] private int round; // 라운드
    [SerializeField] private int state; // 상태 (0: 영웅 제외, 1: 영웅 고르기, 2: 게임 플레이)

    private int currentPlayerIndex; // 현재 플레이어 인덱스
    private int currentPlayerSequence; // 현재 플레이어 순번(영웅 뽑는 순번)
    private int currentPlayerRoundSequence; // 현재 플레이어 순번(라운드 진행 순번)
    private int currentHeroNumber; // 현재 영웅 번호
    private int crownPlayerIndex; // <왕관>을 가지고 있는 플레이어 인덱스

    private List<int> roundHeros = new List<int>(); // 라운드마다 살아남은 영웅 번호 리스트

    // 게임 시작.
    void Start()
    {
        _init_();
    }

    // 초기화 함수: 6명의 플레이어 정보 불러오기
    public void _init_()
    {
        round = 1; // 라운드 초기화 
        state = 0; // 영웅 고르기 상태
        crownPlayerIndex = -1; // <왕관>을 가지고 있는 플레이어 인덱스 초기화
        currentPlayerRoundSequence = 0; // 현재 플레이어 순번 초기화
        currentHeroNumber = 1; // 현재 영웅 번호 초기화
        theCardManager.InitCardDeck(); // 카드 덱 초기화
        SetPlayerSequence(); // 플레이어 순번 배치
        InitPlayer(); // 플레이어 6명의 손패, 필드 카드 비활성화
        theCardManager.ResetHero(); // 영웅 카드 뒷면 표시
        theCardManager.InitExplainCard(); // 도움 카드 초기화
        theButtonManager.InitHelpCanvas(); // 도움 창 초기화
        theButtonManager.InitPlayerPlayLog(); // 플레이어 플레이 로그 메세지 초기화
        theTokenManager.InitToken(); // 토큰 버튼 초기화
        theButtonManager.UpdateHeplCanvas();
        theCardManager.InitCurrentCardIndex(); // 카드 맨 윗 번호 초기화
        theCardManager.ShuffleBuildingCard(); // 건물 카드 섞기

        theCardManager.SetHeroCanvasLoacate(false); // 영웅 카드 캔버스 위치 조정

        PlayFirstPlayer();
    }

    // 라운드 리셋
    public void ResetRound()
    {
        ResetPlayerSequence(); // 플레이어 순서 갱신

        currentPlayerRoundSequence = 0; // 현재 플레이어 순번 초기화
        currentHeroNumber = 1; // 현재 영웅 번호 초기화
        currentPlayerSequence = 0; // 플레이어 순서 초기화

        round++; // 라운드 증가
        UpdateState(0); // 영웅 고르기 상태

        currentPlayerRoundSequence = 0; // 현재 플레이어 순번 초기화
        currentHeroNumber = 1; // 현재 영웅 번호 초기화

        theButtonManager.InitHelpCanvas(); // 도움 창 초기화
        theButtonManager.InitPlayerPlayLog(); // 플레이어 플레이 로그 메세지 초기화
        theTokenManager.ResetCurrentPlayToken(); // 현재 플레이 중인 토큰 리셋
        theTokenManager.InitToken(); // 토큰 버튼 초기화
        theButtonManager.UpdateHeplCanvas(); // 도움 캔버스 초기화

        theCardManager.SetHeroCanvasLoacate(true); // 영웅 카드 캔버스 위치 조정

        Debug.Log(round + "라운드 리셋!");
    }

    // 게임 상태 업데이트
    public void UpdateState(int _state)
    {
        state = _state;
    }

    // 플레이어 세팅 초기화
    public void InitPlayer()
    {
        for(int i=0; i<players.Length; i++)
        {
            players[i].InitPlayer();
            if (i == 0) players[i].playerType = Player.PlayerType.user;
            else players[i].playerType = Player.PlayerType.computer;
        }
    }

    // 플레이어 순번 배치
    public void SetPlayerSequence()
    {
        for(int i=0; i<players.Length; i++)
        {
            int num = players[i].playerSequence;
            int p = Random.Range(i, players.Length);
            players[i].playerSequence = players[p].playerSequence;
            players[p].playerSequence = num;
            players[i].playerName = (i<=0) ? "Player":("Computer" + i.ToString());

            if (players[i].playerSequence == 0){
                crownPlayerIndex = i;
                players[crownPlayerIndex].SetPlayerCrown(true);
                currentPlayerIndex = i; // 첫번째 순서인 플레이어 인덱스 저장
            }
        }
        currentPlayerSequence = 0;
    }

    // 초반 Player 플레이
    public void PlayFirstPlayer()
    {
        ShowPlayerSequence();
        NextTurn();
    }

    // 다음턴 : 현재 플레이어 인덱스 갱신(한 플레이어의 턴이 끝나면 갱신)
    public void NextTurn()
    {
        if (state==0) // 영웅 밴 상태
        {
            if (players[currentPlayerIndex].playerType != Player.PlayerType.user) { _AI.PlayBanSelectHero(players[currentPlayerIndex]); theCardManager.SetHeroCanvasLoacate(false);  }
            else { theTokenManager.SetPlayerButtonHide(); theCardManager.SetHeroCanvasLoacate(true); }
        }
        else if (state==1) // 영웅 고르기 상태
        {
            // 모두 영웅을 고르고 난 후
            if (currentPlayerSequence == 6) // 가장 마지막에 고른 플레이어 순서가 모두 끝난 후
            {
                /* 모든 영웅을 고르고 난 후 각 플레이어 세팅 시 작동하는 페이지 */

                UpdateState(2);
                theCardManager.OffHeroCanvas();
                theTokenManager.SetHeroToken(players);
                if (GetUserPlayer().GetPlayerHero().cardNumber >= 3) theTokenManager.SetHeroTokenBan(); // User가 '암살자', '도둑' 영웅이 아니라면 Ban 기능 활성화
                roundHeros = GetRoundHeros(); // 라운드 영웅 번호 리스트 갱신

                UpdateHeroNumber();
                theTokenManager.UpdatePlayerStateCanvas(players[0]); // 초반 유저 플레이 상태 캔버스 초기화
                theTokenManager.SetCurrentPlayToken(roundHeros[currentPlayerRoundSequence] - 1); // 가장 낮은 영웅 번호를 가지고 있는 플레이어 인덱스 반환
                theTokenManager.UpdateCurrentPickToken(null); // 초반 유저 플레이 화면으로 배치
                theTokenManager.SetplayerHeroButtonObj(GetUserPlayer().playerHero); // User 영웅 버튼 세팅

                // 라운드 첫 번째 Player 인덱스 찾기
                for (int i = 0; i < players.Length; i++) if (players[i].GetPlayerHero().cardNumber == roundHeros[currentPlayerRoundSequence]) currentPlayerIndex = i;

                // 해당 플레이어 영웅 카드 공개
                players[currentPlayerIndex].UpdateHeroCard(state);

                // Player가 유저인지 컴퓨터인지 판별하여 적절한 함수 실행
                if (players[currentPlayerIndex].playerType == Player.PlayerType.computer) { _AI.PlayAct(players[currentPlayerIndex]); }
                else
                {
                    if (players[currentPlayerIndex].GetPlayerHero().cardNumber == 1) { theHeroManager.Assassin(players[currentPlayerIndex]); theButtonManager.UpdateHeplCanvas(); }
                    else theTokenManager.SetPlayerButtonAct(); // Player 행동하기 버튼으로 변환
                }

            }
            else
            {
                if(round==1) SetPlayerCard(); // 1 라운드일 때만 플레이어에게 건물 카드 배포
                currentPlayerSequence++;
                for (int i = 0; i < players.Length; i++) if (players[i].playerSequence == currentPlayerSequence % players.Length) currentPlayerIndex = i;

                if (players[currentPlayerIndex].playerType == Player.PlayerType.user) {
                    if (currentPlayerSequence == 6) NextTurn();
                    else { theTokenManager.SetPlayerButtonHide(); theCardManager.SetHeroCanvasLoacate(true); }
                }
                else
                {
                    _AI.PlayBanSelectHero(players[currentPlayerIndex]);
                    theCardManager.SetHeroCanvasLoacate(false);
                }

                theButtonManager.UpdateHeplCanvas();
            }
        }
        else if(state==2) // 라운드 플레이 상태
        {
            /* 매 턴 플레이어가 턴을 종료할 시 작동하는 페이지 */

            if(currentPlayerRoundSequence == 5)
            {
                Debug.Log(round+"라운드 끝!");

                ResetRound(); // 라운드 리셋(증가)
                for (int i = 0; i < players.Length; i++) players[i].ResetHeroCard(); // 영웅 카드 리셋
                theCardManager.ResetHero(); // 영웅 카드 뒷면 표시

                Debug.Log(round + "라운드 시작!");
                Debug.Log("게임 상태: " + state);
                PlayFirstPlayer(); // 라운드 시작
            }
            else
            {
                // 다음 영웅 번호를 확인하여 Player 인덱스 찾기 및 갱신
                currentPlayerRoundSequence++;
                for (int i = 0; i < players.Length; i++) if (players[i].GetPlayerHero().cardNumber == roundHeros[currentPlayerRoundSequence % players.Length]) currentPlayerIndex = i;

                // 다음 Player Hero 토큰 갱신
                theTokenManager.SetCurrentPlayToken(roundHeros[currentPlayerRoundSequence] - 1);

                // 해당 플레이어 영웅 카드 공개
                players[currentPlayerIndex].UpdateHeroCard(state);

                // Player가 유저인지 컴퓨터인지 판별하여 적절한 함수 실행
                if (players[currentPlayerIndex].playerType == Player.PlayerType.computer) {

                    // 현재 Player가 암살자에게 암살 당했을 경우 턴을 넘긴다.
                    if (theTokenManager.GetDeathPlayerHeroNumber() == players[currentPlayerIndex].GetPlayerHero().cardNumber) NextTurn();

                    // 현재 Player가 도둑에게 도둑 당했을 경우 가지고 있는 돈을 모두 도둑에게 주고 행동한다.
                    else if(theTokenManager.GetStolenPlayerHeroNumber() == players[currentPlayerIndex].GetPlayerHero().cardNumber){
                        int coin = players[currentPlayerIndex].GetPlayerCurrentCoinCount();

                        theTokenManager.GetHeroTokenPlayer(1).UpdatePlayerCoin(coin, true); theTokenManager.GetHeroTokenPlayer(1).UpdatePlayerCoinObj();
                        players[currentPlayerIndex].UpdatePlayerCoin(coin, false); players[currentPlayerIndex].UpdatePlayerCoinObj();

                        _AI.PlayAct(players[currentPlayerIndex]);
                    }
                    else
                    {
                        _AI.PlayAct(players[currentPlayerIndex]);
                    }
                } 
                else
                {
                    if (currentPlayerRoundSequence == 6) NextTurn();
                    else
                    {
                        if (players[currentPlayerIndex].GetPlayerHero().cardNumber==2)
                        {
                            // 현재 Player가 암살자에게 암살 당했을 경우 턴을 넘긴다.
                            if (theTokenManager.GetDeathPlayerHeroNumber() == 2)
                            {
                                // Debug.Log("Stolen User!");
                                NextTurn();
                            }
                            else
                            {
                                theHeroManager.Thief(players[currentPlayerIndex]);
                                theButtonManager.UpdateHeplCanvas();
                            }
                        }
                        else
                        {
                            // 현재 Player가 암살자에게 암살 당했을 경우 턴을 넘긴다.
                            if (theTokenManager.GetDeathPlayerHeroNumber() == players[currentPlayerIndex].GetPlayerHero().cardNumber) NextTurn();

                            // 현재 Player가 도둑에게 도둑 당했을 경우 가지고 있는 돈을 모두 도둑에게 주고 행동한다.
                            else if (theTokenManager.GetStolenPlayerHeroNumber() == players[currentPlayerIndex].GetPlayerHero().cardNumber)
                            {
                                int coin = players[currentPlayerIndex].GetPlayerCurrentCoinCount();

                                theTokenManager.GetHeroTokenPlayer(1).UpdatePlayerCoin(coin, true); theTokenManager.GetHeroTokenPlayer(1).UpdatePlayerCoinObj();
                                players[currentPlayerIndex].UpdatePlayerCoin(coin, false); players[currentPlayerIndex].UpdatePlayerCoinObj();

                                theTokenManager.UpdateplayerHeroButtonObj(true); // Player 영웅 버튼 활성화 
                                theTokenManager.SetPlayerButtonAct(); // Player 행동하기 버튼으로 변환
                            }
                            else
                            {
                                theTokenManager.UpdateplayerHeroButtonObj(true); // Player 영웅 버튼 활성화 
                                theTokenManager.SetPlayerButtonAct(); // Player 행동하기 버튼으로 변환
                            }
                        }

                        // 다른 Computer 화면을 보고 있었다면 User 화면으로 돌아가기
                        theTokenManager.ReturnUserWindow();
                    }
                }

                theButtonManager.UpdateHeplCanvas();
            }
        }
    }

    // 플레이어 영웅 배치
    public void SetPlayerHero(Card _hero)
    {
        players[currentPlayerIndex].playerHero = _hero;
        players[currentPlayerIndex].UpdateHeroCard(state);
        if (players[currentPlayerIndex].GetPlayerHero().cardNumber == 4){
            Debug.Log("<왕관> 플레이어 갱신: " + players[currentPlayerIndex].playerName + "[" + players[currentPlayerIndex].playerSequence + "번]");
            players[crownPlayerIndex].SetPlayerCrown(false);
            crownPlayerIndex = currentPlayerIndex;
            players[crownPlayerIndex].SetPlayerCrown(true);
        }
    }

    // 플레이어 코인, 건물 카드 배치
    public void SetPlayerCard()
    {
        // 코인 추가
        players[currentPlayerIndex].UpdatePlayerCoin(2, true);
        players[currentPlayerIndex].UpdatePlayerCoinObj();

        // 플레이에의 핸드 카드에 무작위 카드 4장 추가
        players[currentPlayerIndex].AddPlayerCard(0, theCardManager.DrawBuildingCard(), players[currentPlayerIndex].playerType);
        players[currentPlayerIndex].AddPlayerCard(0, theCardManager.DrawBuildingCard(), players[currentPlayerIndex].playerType);
        players[currentPlayerIndex].AddPlayerCard(0, theCardManager.DrawBuildingCard(), players[currentPlayerIndex].playerType);
        players[currentPlayerIndex].AddPlayerCard(0, theCardManager.DrawBuildingCard(), players[currentPlayerIndex].playerType);
    }

    // 영웅 번호 업데이트 
    public void UpdateHeroNumber()
    {
        int _index = 10; 
        for(int i=0; i<players.Length; i++) { if (players[i].playerHero.cardNumber == currentHeroNumber) { currentPlayerIndex = i; _index = -1; } }
        if (_index == 10) { currentHeroNumber++; UpdateHeroNumber(); }
    }

    // 영웅 번호 array 오름차순 반환
    public List<int> GetRoundHeros()
    {
        List<int> heros = new List<int>();

        for (int i = 0; i < players.Length; i++) heros.Add(players[i].GetPlayerHero().cardNumber);
        heros.Sort();

        return heros;
    }

    // <왕관>을 가지고 있는 플레이어 기준으로 순서 갱신, Player 건설 가능한 횟수 초기화
    private void ResetPlayerSequence()
    {
        int index = players[crownPlayerIndex].playerSequence;
        for (int i = 0; i < players.Length; i++)
        {
            // Player 건설 가능한 횟수 초기화
            players[i].ResetAvailableBuildCount();

            /*
             * <왕관> 0 -> (0 - 0 + 6) % 6 = 0
             * <왕관> 1 -> (1 - 1 + 6) % 6 = 0
             *        0 -> (0 - 1 + 6) % 6 = 5
             *        2 -> (0 - 4 + 5) % 6 = 2
             */

            // <왕관>을 가지고 있는 플레이어 기준으로 순서 갱신
            int newSequence = (players[i].playerSequence - index + players.Length) % players.Length; // 새로 갱신될 순서 번호
            if (newSequence == 0) currentPlayerIndex = i; // 플레이어 인덱스 초기화
            Debug.Log(players[i].playerSequence + " -> (" + players[i].playerSequence + " - " + index + " + " + players.Length + ") % " + players.Length + " = " + newSequence); 
            players[i].playerSequence = newSequence;
        }
    }

    private void ShowPlayerSequence()
    {
        for(int i=0; i<players.Length; i++)
        {
            Debug.Log("[" + players[i].playerName + "]: " + players[i].playerSequence);
        }
    }

    public Player[] GetPlayers() { return players; } // 플레이어들 정보 반환
    public Player GetCurrentPlayer() { return players[currentPlayerIndex]; } // 현재 플레이어 정보 반환
    public int GetCurrentPlayerIndex() { return currentPlayerIndex; } // 현재 플레이어 인덱스 반환
    public int GetCurrentPlayerSequence() { return currentPlayerSequence % players.Length; } // 현재 플레이어 순번 반환
    public int GetCurentHeroNumber() { return currentHeroNumber; } // 현재 영웅 번호 반환
    public int GetCurrentState() { return state; } // 현재 게임 상태 반환
    public int GetCurrentRound() { return round; } // 현재 라운드 반환
 
    public Player GetUserPlayer() { return players[0]; }
    public Player GetPlayerByIndex(int _index) { return players[_index]; }
}
