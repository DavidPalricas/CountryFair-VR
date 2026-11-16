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

public partial class TentParams : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public TentParams() { }
	[Type(0, "number")]
	public float repetitions = default(float);

	[Type(1, "number")]
	public float sessionTimeMinutes = default(float);

	[Type(2, "string")]
	public string difficultyChange = default(string);
}

