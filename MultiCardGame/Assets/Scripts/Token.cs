using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Token", menuName = "New Token/token")]
public class Token : ScriptableObject
{

    public int number; // 토큰 번호
    public StateType stateType; // 토큰 상태
    public Sprite[] stateImages; // 토큰 상태 이미지(0: 활성화, 1: 밴, 2: 암살당함, 3: 현재 화면, 4: 현재 플레이, 5: 선택)
    public Player player; // 현재 내포 된 플레이어

    public enum StateType
    {
        // 토큰
        Active,
        Ban,
        Death,
        Pick,
        Play,
        Select,

        // 플레이어 행동 버튼
        Hide,
        Show,
        Act,
        End,
        Return,
        SelectHero,
        Skip,

        // 플레이어 영웅 버튼
        PowerActive,
        PowerOff
    }
}
