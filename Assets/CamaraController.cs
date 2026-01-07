using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraController : MonoBehaviour
{
    [SerializeField] Transform _PlayerTrget;
    public float smoothSpeed = 0.1f;
    public Vector3 offset;

    Vector3 velocity = Vector3.zero;
    [SerializeField] float minPosX,maxPosX;
    [SerializeField] float minPosY,maxPosY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector3 desiredPosition = _PlayerTrget.position + offset;
        Vector3 smoothPositiion = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothPositiion;

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minPosX, maxPosX),
            Mathf.Clamp(transform.position.y, minPosY, maxPosY),
            transform.position.z
            );
    }
}
