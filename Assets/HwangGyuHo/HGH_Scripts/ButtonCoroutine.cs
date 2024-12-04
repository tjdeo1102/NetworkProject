using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonCoroutine : MonoBehaviour
{
    [SerializeField] private UnityEvent OnClick;
    [SerializeField] private Animator animator;
    public void ButtonWrapper()
    {
        StartCoroutine(ButtonRoutine());
    }

    private IEnumerator ButtonRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        OnClick?.Invoke();
    }

    private void OnEnable()
    {
        animator.writeDefaultValuesOnDisable = true;
    }
}
