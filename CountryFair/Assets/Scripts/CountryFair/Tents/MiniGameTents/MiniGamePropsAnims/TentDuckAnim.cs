using UnityEngine;

/// <summary>
/// Port of the webapp's HoppingProp (CountryFairWebApp/ClientSide/src/GameProps/MiniGamesProps/HoppingProp.tsx):
/// a simple cyclic hop for a decorative prop that has no animation clips of its own (e.g. the ducks
/// shown in front of their tent). Animates <see cref="_model"/> directly - both the hop height and the
/// pitch/sway rotation - layered on top of whatever rest position and base orientation it already had,
/// so no extra pivot GameObject is needed in the hierarchy. This script's own transform is left untouched;
/// only <see cref="_model"/> moves.
/// </summary>
public class TentDuckAnim : MonoBehaviour
{
    /// <summary>The visual model to animate. Its own rest local position and base rotation are preserved - the hop/tilt is layered on top of them, not replacing them.</summary>
    [SerializeField]
    private Transform _model;

    [Header("Cycle")]
    /// <summary>Hops per second.</summary>
    [SerializeField]
    private float _speed = 0.8f;

    /// <summary>Initial offset (0..1) so several props with the same speed don't hop in unison.</summary>
    [SerializeField]
    private float _phase = 0f;

    [Header("Motion")]
    /// <summary>Peak hop height, added on top of the rest local Y.</summary>
    [SerializeField]
    private float _height = 0.05f;

    /// <summary>Pitch amplitude in radians: nose up on the way up, down on the way down.</summary>
    [SerializeField]
    private float _tilt = 0.25f;

    /// <summary>Side-to-side sway amplitude in radians; runs for the whole cycle, including the pause.</summary>
    [SerializeField]
    private float _sway = 0.08f;

    /// <summary>The model's local position before any hop is applied; the hop only ever offsets its Y on top of this.</summary>
    private Vector3 _restLocalPosition;

    /// <summary>The model's base local rotation before any tilt is applied; pitch/sway is composed on top of it.</summary>
    private Quaternion _baseLocalRotation;

    /// <summary>
    /// Validates <see cref="_model"/> and snapshots its current local position/rotation as the rest
    /// pose, so <see cref="Update"/> can layer the hop height and tilt on top of them every frame
    /// without ever needing to know the model's original placement inside its prefab.
    /// </summary>
    private void Awake()
    {
        if (_model == null)
        {
            Debug.LogError("Model is not assigned in TentDuckAnim.");
            return;
        }

        _restLocalPosition = _model.localPosition;
        _baseLocalRotation = _model.localRotation;
    }

    /// <summary>
    /// Advances the shared hop/pitch/sway cycle from <see cref="_speed"/> and <see cref="_phase"/>,
    /// and writes the resulting local position and rotation onto <see cref="_model"/> for this frame.
    /// </summary>
    private void Update()
    {
        if (_model == null)
        {
            return;
        }

        // Position within the cycle, always in [0, 1). Both curves below are built from a single
        // full turn of sin/cos over this range, so the value and its slope at cycle == 0 exactly
        // match the value and slope wrapping back from cycle == 1 - no seam where the loop repeats.
        float cycle = Mathf.Repeat(Time.time * _speed + _phase, 1f);
        float turn = cycle * Mathf.PI * 2f;

        // Raised cosine: 0 at the bottom (cycle 0 and 1), _height at the top (cycle 0.5), and flat
        // (zero slope) at both the bottom and the peak, so it never looks like it's snapping into place.
        float hopHeight = _height * 0.5f * (1f - Mathf.Cos(turn));

        _model.localPosition = _restLocalPosition + Vector3.up * hopHeight;

        // Nose up on the way up, level at the peak, nose down on the way down, level again at the
        // bottom - crossing zero exactly where the hop's slope also crosses zero.
        float pitchDegrees = _tilt * Mathf.Sin(turn) * Mathf.Rad2Deg;

        // The sway runs the whole cycle so the model is never completely still between hops.
        // Composed on top of the base rotation (rather than replacing it) so it tilts in the model's
        // own frame instead of erasing whatever base orientation it already had.
        float swayDegrees = _sway * Mathf.Sin(turn) * Mathf.Rad2Deg;

        _model.localRotation = _baseLocalRotation * Quaternion.Euler(pitchDegrees, 0f, swayDegrees);
    }
}
