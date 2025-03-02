using System;
using UnityEngine;

public class UIObjectSpinner : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        transform.Rotate(new Vector3(0,0,-speed * Time.deltaTime));
    }
}
