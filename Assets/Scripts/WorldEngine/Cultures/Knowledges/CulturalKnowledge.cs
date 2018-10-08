using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

[XmlInclude(typeof(ShipbuildingKnowledge))]
[XmlInclude(typeof(AgricultureKnowledge))]
[XmlInclude(typeof(SocialOrganizationKnowledge))]
public class CulturalKnowledge : CulturalKnowledgeInfo, IFilterableValue
{
    public const float ValueScaleFactor = 0.01f;

    [XmlAttribute("V")]
    public int Value;

    [XmlIgnore]
    public bool IsPresent
    {
        get
        {
            return Value > 0;
        }
    }

    [XmlIgnore]
    public bool WasPresent { get; private set; }

    public CulturalKnowledge()
    {
    }

    public CulturalKnowledge(string id, string name, int value) : base(id, name)
    {
        Value = value;

        WasPresent = false;
    }

    public CulturalKnowledge(CulturalKnowledge baseKnowledge) : base(baseKnowledge)
    {
        Value = baseKnowledge.Value;

        WasPresent = false;
    }

    public float ScaledValue
    {
        get { return Value * ValueScaleFactor; }
    }

    public void Reset()
    {
        Value = 0;

        WasPresent = true;
    }

    public void Set(int value)
    {
        Value = value;

        WasPresent = false;
    }

    public void Set()
    {
        WasPresent = false;
    }

    public bool ShouldFilter()
    {
        return IsPresent;
    }
}
