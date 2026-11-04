using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Helper that reveals a <see cref="TMP_Text"/> line character by character (typewriter effect), animated with DOTween,
/// with configurable pauses after punctuation.
/// </summary>
/// <remarks>
/// <para>
/// Reveals characters through <see cref="TMP_Text.maxVisibleCharacters"/> instead of growing the string, so the
/// full line is laid out from the first frame (no word jumping between lines) and rich-text tags are never cut in half.
/// </para>
/// <para>
/// <b>Timeline design (narration-ready):</b> before animating, the reveal time of every character is precomputed
/// (base per-character delay + punctuation pauses). A single linear tween then drives "elapsed time" over that
/// timeline, so the whole line has a known <see cref="CurrentDuration"/>. When a narration clip exists, pass its
/// length as <c>targetDuration</c> to <see cref="Play"/>: the timeline is stretched/compressed uniformly to
/// <see cref="TypewriterSettings.narrationDurationScale"/> of that length
/// (punctuation pauses included, so the rhythm is preserved), so the text finishes just before the audio does.
/// </para>
/// </remarks>
public class TypewriterEffect
{
    /// <summary>Value TMP uses for "no visible-character limit".</summary>
    private const int ALL_CHARACTERS_VISIBLE = 99999;

    /// <summary>GameObject the tween is linked to, so it is killed automatically when that object is destroyed.</summary>
    private readonly GameObject _linkTarget;

    /// <summary>Text component being typed into.</summary>
    private readonly TMP_Text _text;

    /// <summary>Timings; held by reference, so Inspector tweaks during Play Mode apply to the next line.</summary>
    private readonly TypewriterSettings _settings;

    /// <summary>Time (seconds since the line started) at which each parsed character becomes visible.</summary>
    private float[] _revealTimes = Array.Empty<float>();

    /// <summary>Tween driving <see cref="_elapsed"/> along the timeline, or null when idle.</summary>
    private Tween _typingTween;

    /// <summary>Current position on the timeline, written by <see cref="_typingTween"/>.</summary>
    private float _elapsed;

    /// <summary>True while a line is still being revealed.</summary>
    public bool IsTyping => _typingTween != null && _typingTween.IsActive();

    /// <summary>Total duration (seconds) of the line being shown, after any <c>targetDuration</c> scaling. 0 when shown instantly.</summary>
    public float CurrentDuration { get; private set; }

    /// <param name="linkTarget">GameObject whose lifetime bounds the tween (usually the dialogue's own GameObject).</param>
    /// <param name="text">Text component to type into.</param>
    /// <param name="settings">Timings of the effect.</param>
    public TypewriterEffect(GameObject linkTarget, TMP_Text text, TypewriterSettings settings)
    {
        _linkTarget = linkTarget;
        _text = text;
        _settings = settings ?? new TypewriterSettings();
    }

    /// <summary>
    /// Replaces the current text with <paramref name="content"/> and starts revealing it, cancelling any line still being typed.
    /// </summary>
    /// <param name="content">Line to show (TMP rich text allowed).</param>
    /// <param name="targetDuration">
    /// Optional total duration in seconds (e.g. the narration clip length). When &gt; 0, the timeline is scaled so the
    /// reveal ends exactly at that time; otherwise the duration comes from <see cref="TypewriterSettings"/>.
    /// </param>
    /// <param name="onCompleted">Invoked once the whole line is visible (also when it is skipped or shown instantly).</param>
    public void Play(string content, float targetDuration = -1f, Action onCompleted = null)
    {
        Kill();

        _text.text = content;
        _text.maxVisibleCharacters = 0;

        // Parse the new text now so characterInfo / characterCount exclude rich-text tags.
        _text.ForceMeshUpdate();

        float naturalDuration = BuildTimeline();

        if (naturalDuration <= 0f || (_settings.charactersPerSecond <= 0f && targetDuration <= 0f))
        {
            CurrentDuration = 0f;
            ShowAll();
            onCompleted?.Invoke();
            return;
        }

        if (targetDuration > 0f)
        {
            // Fitted to a fraction of the clip so the text stays slightly ahead of the voice (see narrationDurationScale).
            float fittedDuration = targetDuration * _settings.narrationDurationScale;
            ScaleTimeline(fittedDuration / naturalDuration);
            CurrentDuration = fittedDuration;
        }
        else
        {
            CurrentDuration = naturalDuration;
        }

        _elapsed = 0f;
        _typingTween = DOTween.To(() => _elapsed, value => { _elapsed = value; ApplyElapsed(); }, CurrentDuration, CurrentDuration)
            .SetEase(Ease.Linear)
            .SetTarget(_text)
            .SetLink(_linkTarget)
            .OnComplete(() =>
            {
                _typingTween = null;
                ShowAll();
                onCompleted?.Invoke();
            });
    }

    /// <summary>
    /// If a line is still being typed, reveals it entirely (firing its <c>onCompleted</c>) and returns true; otherwise returns false.
    /// Lets the "next" button first complete the current line and only advance on the following press.
    /// </summary>
    public bool Skip()
    {
        if (!IsTyping)
        {
            return false;
        }

        _typingTween.Complete(true);
        return true;
    }

    /// <summary>Stops the current animation (without firing <c>onCompleted</c>) and leaves the text as it is.</summary>
    public void Kill()
    {
        _typingTween?.Kill();
        _typingTween = null;
    }

    /// <summary>
    /// Fills <see cref="_revealTimes"/> for the text just parsed: each character appears one base step after the
    /// previous one, plus a pause when the previous character ends a clause/sentence.
    /// </summary>
    /// <returns>Natural duration of the line (reveal time of its last character).</returns>
    private float BuildTimeline()
    {
        int count = _text.textInfo.characterCount;

        if (_revealTimes.Length < count)
        {
            _revealTimes = new float[count];
        }

        float step = _settings.charactersPerSecond > 0f ? 1f / _settings.charactersPerSecond : 0f;
        float time = 0f;

        for (int i = 0; i < count; i++)
        {
            time += step;

            if (i > 0)
            {
                time += GetPauseAfter(i - 1, count);
            }

            _revealTimes[i] = time;
        }

        // When charactersPerSecond <= 0 but a narration duration is given, fall back to a uniform reveal.
        if (time <= 0f && count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                _revealTimes[i] = i + 1;
            }

            time = count;
        }

        return count > 0 ? time : 0f;
    }

    /// <summary>
    /// Pause to insert after the character at <paramref name="index"/>. Only applies when that character is followed by
    /// whitespace, so decimals ("3.5"), the inner dots of "..." and abbreviations glued to text don't pause.
    /// </summary>
    private float GetPauseAfter(int index, int count)
    {
        if (index + 1 >= count)
        {
            return 0f;
        }

        TMP_CharacterInfo[] info = _text.textInfo.characterInfo;

        if (!char.IsWhiteSpace(info[index + 1].character))
        {
            return 0f;
        }

        char c = info[index].character;

        if (_settings.longPauseCharacters.IndexOf(c) >= 0)
        {
            return _settings.longPauseSeconds;
        }

        if (_settings.shortPauseCharacters.IndexOf(c) >= 0)
        {
            return _settings.shortPauseSeconds;
        }

        return 0f;
    }

    /// <summary>Multiplies every reveal time by <paramref name="factor"/> (used to fit the line to a narration clip).</summary>
    private void ScaleTimeline(float factor)
    {
        int count = _text.textInfo.characterCount;

        for (int i = 0; i < count; i++)
        {
            _revealTimes[i] *= factor;
        }
    }

    /// <summary>Shows every character whose reveal time has been reached by <see cref="_elapsed"/>.</summary>
    private void ApplyElapsed()
    {
        int count = _text.textInfo.characterCount;

        // Number of characters with revealTime <= elapsed (timeline is sorted ascending).
        int index = Array.BinarySearch(_revealTimes, 0, count, _elapsed);
        int visible = index >= 0 ? index + 1 : ~index;

        // Equal reveal times (scaled-to-zero steps) would make BinarySearch return any of them; include the whole run.
        while (visible < count && _revealTimes[visible] <= _elapsed)
        {
            visible++;
        }

        _text.maxVisibleCharacters = visible;
    }

    /// <summary>Removes the visible-character limit.</summary>
    private void ShowAll()
    {
        _text.maxVisibleCharacters = ALL_CHARACTERS_VISIBLE;
    }
}
