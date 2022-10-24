using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    [SerializeField] float _swipeResist;

    Drop _drop;
    Vector2 _firstTouchPos;
    Vector2 _finalTouchPos;
    float _swipeAngle;

    void Awake()
    {
        _drop = GetComponent<Drop>();
    }

    void OnMouseDown()
    {
        _firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void OnMouseUp()
    {
        _finalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        SendAngle();
    }

    void SendAngle()
    {
        if (Mathf.Abs(_finalTouchPos.y - _firstTouchPos.y) > _swipeResist ||
           Mathf.Abs(_finalTouchPos.x - _firstTouchPos.x) > _swipeResist)
        {
            _swipeAngle = Mathf.Atan2(_finalTouchPos.y - _firstTouchPos.y,
                                      _finalTouchPos.x - _firstTouchPos.x) * Mathf.Rad2Deg;

            _drop.ProcessSwipe(_swipeAngle);
        }
    }
}
