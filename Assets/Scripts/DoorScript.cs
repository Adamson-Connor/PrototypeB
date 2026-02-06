using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DoorScript : MonoBehaviour

{
    public bool IsOpen = false;
    [SerializeField] private bool IsRotatingDoor = true;
    [SerializeField] private float openSpeed = 1f;
    [Header("Rotation Settings")]
    [SerializeField] private float RotationAmount = 90f;
    [SerializeField] private float ForwardDirection = 0;
    [Header("Sliding Settings")]
    [SerializeField] private Vector3 SlideDirection = Vector3.back;
    [SerializeField] private float SlideAmount = 1.9f;


    private Vector3 StartRotation;
    private Vector3 StartPosition;
    private Vector3 Forward;

    private Coroutine AnimationCoroutine;
    public UnityEvent onOpen, onClose;

    private void Awake()
    {
        StartRotation = transform.rotation.eulerAngles;
        Forward = transform.right;
        StartPosition = transform.position;
    }

   
    public void Open(Vector3 UserPosition)
    {
        if (!IsOpen)
        {
            onOpen.Invoke();
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }

            if (IsRotatingDoor)
            {

                float dot = Vector3.Dot(Forward, (UserPosition - transform.position).normalized);
                Debug.Log($"Dot:{dot.ToString("N3")}");
                AnimationCoroutine = StartCoroutine(DoRotationOpen(dot));
            }
            else
            {
                AnimationCoroutine = StartCoroutine(DoSlidingOpen());
            }
        }
    }

    private IEnumerator DoRotationOpen(float ForwardAmount)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation;

        if (ForwardAmount >= ForwardDirection)
        {
            endRotation = Quaternion.Euler(new Vector3(0, StartRotation.y - RotationAmount, 0));
        }
        else
        {
            endRotation = Quaternion.Euler(new Vector3(0, StartRotation.y + RotationAmount, 0));
        }

        IsOpen = true;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * openSpeed;
        }
    }

        private IEnumerator DoSlidingOpen()
    {
        Vector3 endPosition = StartPosition + SlideAmount *SlideDirection;
        Vector3 startPosition = transform.position;
        float time = 0;
        IsOpen = true;
        while (time < 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, time);
            yield return null;
            time += Time.deltaTime * openSpeed;
        }
    }
    

    public void Close()
    {
        if (IsOpen)
        {
            onClose.Invoke();
            if (AnimationCoroutine !=null)
            {
                StopCoroutine(AnimationCoroutine);
            }

            if (IsRotatingDoor)
            {
                AnimationCoroutine = StartCoroutine(DoRotationClose());
            }
            else
            {
                AnimationCoroutine = StartCoroutine(DoSlidingClose());
            }
        }
    }

    private IEnumerator DoRotationClose()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(StartRotation);

        IsOpen = false;

        float time = 0;
        while (time <1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * openSpeed;
        }
    }
    private IEnumerator DoSlidingClose() {
        Vector3 endPosition = StartPosition;
        Vector3 startPosition = transform.position;
        float time = 0;
        IsOpen = false;
        while (time < 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, time);
            yield return null;
            time += Time.deltaTime * openSpeed;
        } }

    }
