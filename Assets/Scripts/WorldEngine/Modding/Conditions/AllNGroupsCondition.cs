﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AllNGroupsCondition : GroupCondition
{
    public Condition Condition;

    public AllNGroupsCondition(string conditionStr)
    {
        Condition = BuildCondition(conditionStr);
    }

    public override bool Evaluate(CellGroup group)
    {
        foreach (CellGroup nGroup in group.NeighborGroups)
        {
            if (!Condition.Evaluate(nGroup))
                return false;
        }

        return true;
    }

    public override string ToString()
    {
        return "ALL_N_GROUPS (" + Condition.ToString() + ")";
    }
}
