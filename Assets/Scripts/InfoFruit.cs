using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InfoFruit : MonoBehaviour
{
    private int level;
    [SerializeField] private Rigidbody2D rb;
    private bool isColider;

    public bool IsColider
    {
        get => isColider;
    }
    
    private Action<InfoFruit, InfoFruit, int> onMerge;
    private Action endGame;
    
    [SerializeField] private CircleCollider2D _collider2D;
    public void Init(int level, Action<InfoFruit, InfoFruit, int> actionMerge, Action endGame, bool isFall = false)
    {
        this.level = level;
        onMerge = actionMerge;
        this.endGame = endGame;
        isColider = false;
        rb.bodyType = !isFall ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
    }
    public void OnFall()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out InfoFruit otherfruit))
        {
            if(level > GameController.Instance.Model.DataFruit.Count) return;
            if (otherfruit.level == level)
            {
                onMerge?.Invoke(this, otherfruit, level + 1);
                isColider = true;
                otherfruit.isColider = true;
            }
        }
    }
}
