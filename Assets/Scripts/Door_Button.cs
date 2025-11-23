using System.Collections;
using UnityEngine;

public class Door_Button : MonoBehaviour
{
    private bool IsPlayerThere;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool isOpen = false;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;

    private void Start()
    {
        IsPlayerThere = false;
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));

    }

    private void OnTriggerEnter(Collider other)
    {
        IsPlayerThere = true;
    }

    private void OnTriggerExit(Collider other)
    {
        IsPlayerThere = false;
    }
    private void Update()
    {
        if (IsPlayerThere = true)
        {

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(ToggleDoor());

            }
        }
}

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? _closedRotation : _openRotation;
        isOpen = !isOpen;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.rotation = targetRotation;
    }

}
