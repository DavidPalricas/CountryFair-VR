// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 4.0.28
// 

using Colyseus.Schema;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

public partial class FairState : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public FairState() { }
	[Type(0, "map", typeof(MapSchema<string>), "string")]
	public MapSchema<string> tentOrders = null;

	[Type(1, "number")]
	public float score = default(float);

	[Type(2, "number")]
	public float streak = default(float);

	[Type(3, "number")]
	public float sessionGoal = default(float);

	[Type(4, "string")]
	public string sceneName = default(string);

	[Type(5, "boolean")]
	public bool dialogueActive = default(bool);

	[Type(6, "boolean")]
	public bool tutorialActive = default(bool);

	[Type(7, "map", typeof(MapSchema<TentParams>))]
	public MapSchema<TentParams> tentParams = null;

	[Type(8, "boolean")]
	public bool isLeftArmAffected = default(bool);

	[Type(9, "string")]
	public string survivorFile = default(string);

	[Type(10, "boolean")]
	public bool configCompleted = default(bool);

	[Type(11, "boolean")]
	public bool isLastConfig = default(bool);

	[Type(12, "boolean")]
	public bool configFinished = default(bool);
}

