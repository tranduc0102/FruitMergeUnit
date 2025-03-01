using System.Collections.Generic;
using UnityEngine;

public class ModelFruit : MonoBehaviour
{
    [SerializeField] private int limitFruit; // số lượng level của quả spawn ban đầu
    [SerializeField] private List<InfoFruit> dataFruit;

    public int LimitFruit
    {
        get => limitFruit;
        set => limitFruit = value;
    }

    public List<InfoFruit> DataFruit
    {
        get => dataFruit;
        set => dataFruit = value;
    }
}
