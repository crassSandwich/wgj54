﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallController : MonoBehaviour {

	public HallSection HallObject;
	[Tooltip("Must be a positive odd number.")]
	public int MaxSections;

	Dictionary<int, HallSection> halls; // increasing index -> increasing z position
	int frontierPos = 0, frontierNeg = 0;

	int midpoint { get {
		return (frontierNeg + frontierPos) / 2;
	}}
	HallSection posTrigger { get {
		return halls[midpoint + 1];
	}}
	HallSection negTrigger { get {
		return halls[midpoint - 1];
	}}

	bool infinitized, initialized;

	void Update () {
		if (initialized && !infinitized) {
			scaleFog();
		}
	}

	public void Initialize (ZDir direction) {
		initialized = true;

		Vector3 pos = transform.position + (direction.IsPositive() ? Vector3.forward : Vector3.back) * HallSection.ZLength / 2;

		halls = new Dictionary<int, HallSection>(MaxSections);
		halls[0] = Instantiate(HallObject, pos, Quaternion.identity);
		halls[0].TriggerHandler += HallTriggerHandler;
		halls[0].transform.parent = transform;
		
		for (int i = 1; i < MaxSections; i++) {
			SpawnHall(direction);
		}
	}

	HallSection SpawnHall (ZDir direction) {
		Vector3 loc;
		if (direction.IsPositive()) {
			loc = halls[frontierPos].transform.position + Vector3.forward * HallSection.ZLength;
		}
		else {
			loc = halls[frontierNeg].transform.position + Vector3.back * HallSection.ZLength;
		}
		var hall = Instantiate(HallObject, loc, Quaternion.identity);
		hall.TriggerHandler += HallTriggerHandler;
		hall.transform.parent = transform;

		if (direction.IsPositive()) {
			halls[++frontierPos] = hall;
		}
		else {
			halls[--frontierNeg] = hall;
		}

		return hall;
	}
	
	void HallTriggerHandler (object o, System.EventArgs e) {
		HallSection h = (HallSection) o;

		if (!infinitized) {
			if (h == halls[midpoint])
				infinitize();
			else
				return;
		}

		if (h == posTrigger) {
			popLeft();
			SpawnHall(ZDir.Positive);
		}
		else if (h == negTrigger) {
			popRight();
			SpawnHall(ZDir.Negative);
		}
	}

	void infinitize () {
		infinitized = true;

		var newLandOffset = new Vector3(137, 137, 137);
		transform.position += newLandOffset;
		PlayerMove.Instance.transform.position += newLandOffset;
	}
	
	void scaleFog () {
		HallSection farPoint = halls[midpoint * 2];
		float distanceAlong = Mathf.Abs(farPoint.transform.position.z - PlayerMove.Instance.transform.position.z);

		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.ExponentialSquared;
		RenderSettings.fogDensity = fogDensity(distanceAlong, true);
	}

	// finds the density for the exponential fog equation so that its value at distance is arbitrarily close to zero
	float fogDensity (float distance, bool squared) {
		float alpha = 1000; // this is arbitrarily large
		float ln = Mathf.Log(alpha);

		if (squared) {
			return Mathf.Sqrt(ln) / distance;
		}
		else {
			return ln / distance;
		}
	}

	void popLeft () {
		Destroy(halls[frontierNeg].gameObject);
		halls[frontierNeg++] = null;
	}

	void popRight () {
		Destroy(halls[frontierPos].gameObject);
		halls[frontierPos--] = null;
	}

}
