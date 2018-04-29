﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class TribeSplitDecisionEvent : FactionEvent {

	public const long DateSpanFactorConstant = CellGroup.GenerationSpan * 20;

	public const int SplitClanMaxAdministrativeLoad = 500000;
	public const int SplitClanMinAdministrativeLoad = 100000;

	public const int TribeMaxAdministrativeLoad = 500000;
	public const int TribeMinAdministrativeLoad = 100000;

	public const float MaxAdministrativeLoadChanceFactor = 0.05f;

	public const float MinCoreDistance = 200f;

	private Clan _splitClan;
	private Clan _dominantClan;

	private Tribe _originalTribe;

	private float _splitClanChanceOfSplitting;
	private float _tribeChanceOfSplitting;

	public TribeSplitDecisionEvent () {

		DoNotSerialize = true;
	}

	public TribeSplitDecisionEvent (Clan splitClan, FactionEventData data) : base (splitClan, data) {

		_splitClan = splitClan;
		_originalTribe = World.GetPolity (data.OriginalPolityId) as Tribe;

		DoNotSerialize = true;
	}

	public TribeSplitDecisionEvent (Clan splitClan, long triggerDate) : base (splitClan, triggerDate, TribeSplitDecisionEventId) {

		_splitClan = splitClan;
		_originalTribe = splitClan.Polity as Tribe;

		DoNotSerialize = true;
	}

	public static long CalculateTriggerDate (Clan clan) {

		float randomFactor = clan.GetNextLocalRandomFloat (RngOffsets.TRIBE_SPLITTING_EVENT_CALCULATE_TRIGGER_DATE);
		randomFactor = Mathf.Pow (randomFactor, 2);

		Clan dominantClan = clan.Polity.DominantFaction as Clan;

		float administrativeLoad = dominantClan.CalculateAdministrativeLoad ();

		float loadFactor = 1;

		if (administrativeLoad != Mathf.Infinity) {

			float modAdminLoad = Mathf.Max (0, administrativeLoad - SplitClanMinAdministrativeLoad);
			float modHalfFactorAdminLoad = SplitClanMaxAdministrativeLoad - SplitClanMinAdministrativeLoad;

			loadFactor = modHalfFactorAdminLoad / (modAdminLoad + modHalfFactorAdminLoad);
		}

		float cohesionPreferenceValue = clan.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float dateSpan = (1 - randomFactor) *  DateSpanFactorConstant * loadFactor * cohesionPrefFactor;

		long triggerDateSpan = (long)dateSpan + CellGroup.GenerationSpan;

		if (triggerDateSpan < 0) {
			#if DEBUG
			Debug.LogWarning ("updateSpan less than 0: " + triggerDateSpan);
			#endif

			triggerDateSpan = CellGroup.MaxUpdateSpan;
		}

		return clan.World.CurrentDate + triggerDateSpan;
	}

	public override bool CanTrigger () {

		if (!base.CanTrigger ())
			return false;

		if (_splitClan.Polity != OriginalPolity)
			return false;

		if (_splitClan.IsDominant)
			return false;

		_dominantClan = _originalTribe.DominantFaction as Clan;

		// We should use the latest cultural attribute values before calculating chances
		_splitClan.PreUpdate ();
		_dominantClan.PreUpdate ();

		CellGroup clanCoreGroup = _splitClan.CoreGroup;

		PolityProminence polityProminence = clanCoreGroup.GetPolityProminence (OriginalPolity);

		if (clanCoreGroup.HighestPolityProminence != polityProminence)
			return false;

		float influence = _splitClan.Influence;

//		if (influence < MinInfluenceTrigger) {
//
//			return false;
//		}

		float polityCoreDistance = (polityProminence.PolityCoreDistance * influence) - MinCoreDistance;

		if (polityCoreDistance <= 0)
			return false;

		_tribeChanceOfSplitting = CalculateChanceOfSplittingForTribe ();

//		if (_tribeChanceOfSplitting <= 0) {
//
//			return false;
//		}

		_splitClanChanceOfSplitting = CalculateChanceOfSplittingForSplitClan ();

		if (_splitClan.IsUnderPlayerGuidance && _splitClanChanceOfSplitting < 0.5f) {
		
			return false;
		}

		if (_splitClanChanceOfSplitting <= 0) {

			return false;
		}

		return true;
	}

	public float CalculateChanceOfSplittingForSplitClan () {

		float administrativeLoad = _dominantClan.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float cohesionPreferenceValue = _splitClan.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);

		if (cohesionPreferenceValue <= 0)
			return 1;

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float authorityPreferenceValue = _splitClan.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreferenceValue <= 0)
			return 1;

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float relationshipFactor = 2 * _splitClan.GetRelationshipValue (_dominantClan);
		relationshipFactor = Mathf.Pow (relationshipFactor, 4);

		float diffLimitsAdministrativeLoad = SplitClanMaxAdministrativeLoad - SplitClanMinAdministrativeLoad;

		float modMinAdministrativeLoad = SplitClanMinAdministrativeLoad * cohesionPrefFactor * relationshipFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(diffLimitsAdministrativeLoad * _dominantClan.CurrentLeader.Wisdom * _dominantClan.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float chance = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

		return Mathf.Clamp01 (chance);
	}

	public float CalculateChanceOfSplittingForTribe () {

		float administrativeLoad = _dominantClan.CalculateAdministrativeLoad ();

		if (administrativeLoad == Mathf.Infinity)
			return 1;

		float cohesionPreferenceValue = _originalTribe.GetPreferenceValue (CulturalPreference.CohesionPreferenceId);

		if (cohesionPreferenceValue <= 0)
			return 1;

		float cohesionPrefFactor = 2 * cohesionPreferenceValue;
		cohesionPrefFactor = Mathf.Pow (cohesionPrefFactor, 4);

		float authorityPreferenceValue = _originalTribe.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		if (authorityPreferenceValue <= 0)
			return 1;

		float authorityPrefFactor = 2 * authorityPreferenceValue;
		authorityPrefFactor = Mathf.Pow (authorityPrefFactor, 4);

		float relationshipFactor = 2 * _dominantClan.GetRelationshipValue (_splitClan);
		relationshipFactor = Mathf.Pow (relationshipFactor, 4);

		float diffLimitsAdministrativeLoad = TribeMaxAdministrativeLoad - TribeMinAdministrativeLoad;

		float modMinAdministrativeLoad = TribeMinAdministrativeLoad * cohesionPrefFactor * relationshipFactor;
		float modMaxAdministrativeLoad = modMinAdministrativeLoad + 
			(diffLimitsAdministrativeLoad * _dominantClan.CurrentLeader.Wisdom * _dominantClan.CurrentLeader.Charisma * authorityPrefFactor * MaxAdministrativeLoadChanceFactor);

		float chance = (administrativeLoad - modMinAdministrativeLoad) / (modMaxAdministrativeLoad - modMinAdministrativeLoad);

		return Mathf.Clamp01 (chance);
	}

	public override void Trigger () {

		bool splitClanPreferSplit = _splitClan.GetNextLocalRandomFloat (RngOffsets.TRIBE_SPLITTING_EVENT_SPLITCLAN_PREFER_SPLIT) < _splitClanChanceOfSplitting;

		if (_originalTribe.IsUnderPlayerFocus || _splitClan.IsUnderPlayerGuidance) {

			Decision splitClanDecision;

			if (_splitClanChanceOfSplitting >= 1) {
				splitClanDecision = new ClanTribeSplitDecision (_originalTribe, _splitClan, _dominantClan, _tribeChanceOfSplitting); // Player that controls split clan can't prevent splitting from happening
			} else {
				splitClanDecision = new ClanTribeSplitDecision (_originalTribe, _splitClan, _dominantClan, splitClanPreferSplit, _tribeChanceOfSplitting); // Give player options
			}

			if (_splitClan.IsUnderPlayerGuidance) {

				World.AddDecisionToResolve (splitClanDecision);

			} else {

				splitClanDecision.ExecutePreferredOption ();
			}

		} else if (splitClanPreferSplit) {

			ClanTribeSplitDecision.LeaderAllowsSplit (_splitClan, _dominantClan, _originalTribe, _tribeChanceOfSplitting);

		} else {

			ClanTribeSplitDecision.LeaderPreventsSplit (_splitClan, _dominantClan, _originalTribe);
		}
	}

	protected override void DestroyInternal () {

		base.DestroyInternal ();

		if ((Faction != null) && (Faction.StillPresent)) {

			_splitClan.ResetEvent (WorldEvent.TribeSplitDecisionEventId, CalculateTriggerDate (_splitClan));
		}
	}

	public override void FinalizeLoad () {

		base.FinalizeLoad ();

		_splitClan = Faction as Clan;

		_splitClan.AddEvent (this);
	}

	public override void Reset (long newTriggerDate)
	{
		base.Reset (newTriggerDate);

		_originalTribe = OriginalPolity as Tribe;
	}
}
