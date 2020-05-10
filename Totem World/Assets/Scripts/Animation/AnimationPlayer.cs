using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    SpriteAnim _clip;

    float _timer;
    float _timePerFrame => _clip ? 1 / (float) _clip.framerate : 0;
    int _frame;
    
    // Start is called before the first frame update
    void Start()
    {
        ApplyAnimationFrame(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_clip) return;

        _timer += Time.deltaTime;
        if (_timer >= _timePerFrame)
        {
            _timer = 0;
            _frame++;
            if (_frame >= _clip.sprites.Count) _frame = 0;
            ApplyAnimationFrame(_frame);
        }
    }

    public void ApplyAnimationClip(SpriteAnim newClip)
    {
        _clip = newClip;
        if (_frame >= _clip.sprites.Count) _frame = 0;
    }

    void ApplyAnimationFrame(int frameNumber)
    {
        if (!spriteRenderer || !_clip) return;
        spriteRenderer.sprite = _clip.sprites[frameNumber];
    }
}
