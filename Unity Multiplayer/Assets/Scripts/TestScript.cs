using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    void Start()
    {
        inputReader.MoveEvent += HandleMove;
    }

    private void OnDestroy() {
        inputReader.MoveEvent -= HandleMove;
    }

    private void HandleMove(Vector2 vector2)
    {
        Debug.Log(vector2);
    }
    void Update()
    {
        
    }
}
