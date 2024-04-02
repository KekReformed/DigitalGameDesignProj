using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessFade : MonoBehaviour
{
    private float _fadeTo;
    public Material darkness;
    private Material _darkness;
    [Range(0, 1)] public float fadeRate;
    public float startingAlpha;


    // Start is called before the first frame update
    void Start()
    {
        startingAlpha = darkness.color.a;
        _darkness = new Material(darkness);
        gameObject.GetComponent<SpriteRenderer>().material = _darkness;
        _fadeTo = startingAlpha;
    }

    // Update is called once per frame
    void Update()
    {
        if (_fadeTo != _darkness.color.a)
        {
            if (_fadeTo > _darkness.color.a) _darkness.color = new Vector4(_darkness.color.r, _darkness.color.g, _darkness.color.b, Mathf.Round((_darkness.color.a + fadeRate) * 100) / 100);
            else _darkness.color = new Vector4(_darkness.color.r, _darkness.color.g, _darkness.color.b, Mathf.Round((_darkness.color.a - fadeRate) * 100) / 100);
        }
    }

    public void FadeTo(float value)
    {
        _fadeTo = Mathf.Round(value * 100) / 100;
    }
}
