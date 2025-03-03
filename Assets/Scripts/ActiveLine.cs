using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveLine : MonoBehaviour
{
    [SerializeField] private GameObject _line;
    private Coroutine checkLine;
    private void OnTriggerEnter2D(Collider2D other)
    {
        checkLine = StartCoroutine(CheckLine(1.5f, other));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
       StopCoroutine(checkLine);
    }

    IEnumerator CheckLine(float time, Collider2D collider2D)
    {
        yield return new WaitForSeconds(time);
        if (collider2D != null && collider2D.bounds.Intersects(GetComponent<Collider2D>().bounds))
        {
            _line.SetActive(true);
        }
        else
        {
            _line.SetActive(false);
        }
    }
}
