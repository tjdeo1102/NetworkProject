using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Enums
{
    public enum SkillType
    {
        Basic,      // 기본
        Fire,       // 불
        Water,      // 물
        Thunder,    // 번개
        Wind        // 바람
    }

    public enum Grade
    {
        Basic,      // 기본
        Low,        // 하급
        Mid,        // 중급
        High        // 상급
    }

    public enum PlayerSkillSlot
    {
        Slot1,
        Slot2,
        Slot3,
        Slot4,
        Length,
    }

    public enum ESoundType
    {
        BGM,
        SFX,
        Length,
    }
}
