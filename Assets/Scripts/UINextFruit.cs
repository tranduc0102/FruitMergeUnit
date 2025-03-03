using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UINextFruit : MonoBehaviour
{
    [SerializeField] private Image _image;

    public void SetNextFruit(Sprite sprite)
    {
        _image.transform.DOScale(Vector3.zero, 0.2f).OnComplete(delegate
        {
            _image.sprite = sprite;
            _image.transform.DOScale(Vector3.one, 0.2f);
        });
    }
}
