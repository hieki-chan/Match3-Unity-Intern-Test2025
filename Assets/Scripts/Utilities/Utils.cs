using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

public class Utils
{
    public static NormalItem.eNormalType GetRandomNormalType()
    {
        Array values = Enum.GetValues(typeof(NormalItem.eNormalType));
        NormalItem.eNormalType result = (NormalItem.eNormalType)values.GetValue(URandom.Range(0, values.Length));

        return result;
    }

    public static NormalItem.eNormalType GetRandomNormalTypeExcept(NormalItem.eNormalType[] types)
    {
        List<NormalItem.eNormalType> list = Enum.GetValues(typeof(NormalItem.eNormalType)).Cast<NormalItem.eNormalType>().Except(types).ToList();

        int rnd = URandom.Range(0, list.Count);
        NormalItem.eNormalType result = list[rnd];

        return result;
    }

    public static List<(NormalItem.eNormalType, int)> GetItemList(int boardSize)
    {
        if(boardSize % 3 != 0)
        {
            Debug.LogError($"NOT DIVISIBLE BY 3!!: {boardSize}");
        }

        List<(NormalItem.eNormalType, int)> result = Enum.GetValues(typeof(NormalItem.eNormalType))
            .Cast<NormalItem.eNormalType>()
            .Select(t => (t, 3))
            .ToList();

        int notFilled = boardSize - result.Count * 3;

        while (notFilled > 0)
        {
            int randItemIndex = URandom.Range(0, result.Count);
            notFilled -= 3;

            var item = result[randItemIndex];
            item.Item2 += 3;
            result[randItemIndex] = item;
        }

        return result;
    }
}
