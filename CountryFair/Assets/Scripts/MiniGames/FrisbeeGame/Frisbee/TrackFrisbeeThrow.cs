using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class TrackFrisbeeThrow : MonoBehaviour
{
    [Header("Hand Tracking")]
    [SerializeField]
    private OVRHand leftHand;
    
    [SerializeField]
    private OVRHand rightHand;

    [Header("Tracking Settings")]
    [Tooltip("Number of position samples to store (20-30 recommended)")]
    [SerializeField]
    private int maxSamples = 25;

    [Tooltip("Minimum speed to consider a valid throw (m/s)")]
    [SerializeField]
    private float minimumThrowSpeed = 0.8f;

    [Header("Direction Calculation")]
    
    [Tooltip("Blend between finger forward and velocity direction (0=velocity, 1=finger)")]
    [SerializeField]
    [Range(0f, 1f)]
    private float fingerDirectionWeight = 0.8f; // 🎯 80% finger, 20% velocity

    [Header("Velocity Mapping")]
    [SerializeField]
    private AnimationCurve velocityToThrowSpeedCurve = AnimationCurve.EaseInOut(0f, 5f, 5f, 22f);

    [SerializeField]
    private float standardThrowVelocity = 14f;

    [SerializeField]
    private float maxThrowVelocity = 25f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugGizmos = true;

    [SerializeField]
    private bool showDebugLogs = true;

    private readonly Queue<Vector3> _handSamples = new();
    private Transform _activeHandTransform = null;
    private OVRHand _activeHand = null;
    private OVRSkeleton _activeSkeleton = null;

    private void Update()
    {
        if (_activeHand != null)
        {    
            if (!_activeHand.IsTracked)
            {
                SwitchActiveHand();
            }

            TrackHand();  
        }
    }

    private void SwitchActiveHand()
    {
        _activeHand = _activeHand == leftHand ? rightHand : leftHand;
        UpdateActiveHandTransform();
    }

    private void UpdateActiveHandTransform()
    {
        _activeSkeleton = _activeHand.GetComponent<OVRSkeleton>();
        
        OVRBone handEndBone = _activeSkeleton.Bones.ToList().Find(bone => 
            bone.Id == OVRSkeleton.BoneId.Hand_End);
         
        _activeHandTransform = handEndBone.Transform;
        
        if (showDebugLogs)
        {
            Debug.Log($"✅ Tracking {(_activeHand == leftHand ? "LEFT" : "RIGHT")} index tip bone");
        }
    }

    private void TrackHand()
    {
        Vector3 currentPosition = _activeHandTransform.position;
        
        if (showDebugLogs && _handSamples.Count > 0)
        {
            float distance = Vector3.Distance(currentPosition, _handSamples.ToArray()[_handSamples.Count - 1]);
            if (distance > 0.001f)
            {
                Debug.Log($"📍 Hand moving: {distance * 1000:F1}mm per frame");
            }
        }

        _handSamples.Enqueue(currentPosition);

        while (_handSamples.Count > maxSamples)
        {
            _handSamples.Dequeue();
        }
    }

    /// <summary>
    /// Calculates throw properties using finger direction and hand velocity
    /// Direction: Primarily from finger forward (where it points at release)
    /// Speed: From hand movement velocity mapped to realistic throw speeds
    /// </summary>
    public Tuple<Vector3, float> GetThrowProprieties()
    {
        Vector3 throwVector = Vector3.forward;
        float throwSpeed = 0f;

        if (_handSamples.Count < 2)
        {
            Debug.LogWarning("⚠️ Not enough samples to calculate throw vector!");
            return Tuple.Create(throwVector, throwSpeed);
        }

        // 1️⃣ Get DIRECTION from finger forward (where the finger points)
        Vector3 fingerForward = _activeHandTransform.forward;

        // 2️⃣ Get VELOCITY from hand movement (for speed calculation)
        Tuple<Vector3, float> velocityData = GetVelocityProprietiesFromSamples();
        Vector3 velocityDirection = velocityData.Item1.normalized;
        float rawHandSpeed = velocityData.Item2;
   
      
    // 🎯 Mainly use finger direction, slightly influenced by velocity
        throwVector = fingerForward;
            
        if (showDebugLogs)
        {
                Debug.Log($"🎯 [DIRECTION] Finger: {fingerForward}, Velocity: {velocityDirection}");
                Debug.Log($"🎯 [BLEND] Final: {throwVector} ({fingerDirectionWeight * 100:F0}% finger)");
        }
        
        // 4️⃣ Calculate THROW SPEED from hand velocity
        float mappedThrowSpeed = velocityToThrowSpeedCurve.Evaluate(rawHandSpeed);
        throwSpeed = Mathf.Clamp(mappedThrowSpeed, 5f, maxThrowVelocity);

        if (showDebugLogs)
        {
            Vector3[] samples = _handSamples.ToArray();
            float totalDistance = Vector3.Distance(samples[0], samples[^ 1]);
            
            Debug.Log($"📊 [TRACKING] Samples: {samples.Length}, Distance: {totalDistance * 100:F1}cm");
            Debug.Log($"📊 [SPEED] Hand: {rawHandSpeed:F2} m/s → Frisbee: {throwSpeed:F2} m/s");
            
            if (Mathf.Abs(throwSpeed - standardThrowVelocity) < 2f)
            {
                Debug.Log($"✅ Close to standard velocity ({standardThrowVelocity} m/s)");
            }
        }

        // 5️⃣ Validate minimum speed
        if (rawHandSpeed >= minimumThrowSpeed)
        {            
            if (showDebugLogs)
            {
                Debug.Log($"✅ [RESULT] Valid throw! Direction: {throwVector}, Speed: {throwSpeed:F2} m/s");
            }
            
            return Tuple.Create(throwVector, throwSpeed);
        }

        if (showDebugLogs)
        {
            Debug.LogWarning($"⚠️ [RESULT] Hand speed too low ({rawHandSpeed:F2} < {minimumThrowSpeed})");
        }
        
        // Se muito lento, usa só a direção do dedo
        return Tuple.Create(fingerForward, throwSpeed);
    }

    private Tuple<Vector3, float> GetVelocityProprietiesFromSamples()
    {
        Vector3[] sampleArray = _handSamples.ToArray();
        Vector3 totalVelocity = Vector3.zero;
        int validSamples = 0;

        for (int i = 1; i < sampleArray.Length; i++)
        {
            Vector3 delta = sampleArray[i] - sampleArray[i - 1];
            Vector3 velocity = delta / Time.deltaTime;
            totalVelocity += velocity;
            validSamples++;
        }

        Vector3 averageVelocity = validSamples > 0 ? totalVelocity / validSamples : Vector3.zero;
        float speed = averageVelocity.magnitude;

        return Tuple.Create(averageVelocity, speed);
    }

    public void StartTracking()
    {
        _handSamples.Clear();
        _activeHand = leftHand.IsTracked ? leftHand : rightHand;
        UpdateActiveHandTransform();
        
        if (showDebugLogs)
        {
            Debug.Log($"🎬 Started tracking {(_activeHand == leftHand ? "LEFT" : "RIGHT")} hand");
        }
    }

    public void StopTracking()
    {
        if (showDebugLogs)
        {
            Debug.Log($"🛑 Stopped tracking. Samples: {_handSamples.Count}");
        }
        
        _activeHand = null;
        _activeSkeleton = null;
        _handSamples.Clear();
    }
}
