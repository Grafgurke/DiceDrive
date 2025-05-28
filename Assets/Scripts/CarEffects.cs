using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections.Generic;

public class CarEffects : MonoBehaviour
{
    [SerializeField] private bool isPlayer1 = true;
    private PlayerInput playerInput;
    public TrailRenderer[] tireMarks;
    public ParticleSystem[] smoke;
    public InputAction brakeAction;
    private bool tireMarksFlag = false;
    private Rigidbody rb;
    [Header("Audio")]
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private AudioSource skidSound;
    [SerializeField][Range(0, 2)] private float minPitch = 1f;
    [SerializeField][Range(1, 8)] private float maxPitch = 5f;
    [SerializeField] private AudioSource crashSound;
    [SerializeField] private List<AudioClip> crashSounds;
    private void Start()
    {
        skidSound.mute = true;
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        string actionMapName = isPlayer1 ? "Player1" : "Player2";
        playerInput.SwitchCurrentActionMap(actionMapName);
        brakeAction = playerInput.actions.FindAction($"{actionMapName}/Brake");
    }
    private void FixedUpdate()
    {
        checkDrift();
        EngineSound();
    }
    private void checkDrift()
    {
        if (brakeAction.IsPressed())
        {
            StartEmitter();
            ToggleSkidSound(true);
        }
        else
        {
            StopEmitter();
            ToggleSkidSound(false);
        }

    }
    private void StartEmitter()
    {
        if (tireMarksFlag)
            return;
        foreach (TrailRenderer trailRenderer in tireMarks)
        {
            trailRenderer.emitting = true;
        }
        foreach (ParticleSystem particleSystem in smoke)
        {
            if (!particleSystem.isPlaying)
                particleSystem.Play();
        }
        tireMarksFlag = true;
    }
    private void StopEmitter()
    {
        if (!tireMarksFlag)
            return;
        foreach (TrailRenderer trailRenderer in tireMarks)
        {
            trailRenderer.emitting = false;
        }
        foreach (ParticleSystem particleSystem in smoke)
        {
            if (particleSystem.isPlaying)
                particleSystem.Stop();
        }
        tireMarksFlag = false;
    }
    #region Audio
    private void EngineSound()
    {
        engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(rb.linearVelocity.magnitude / 100f));
    }
    private void ToggleSkidSound(bool isSkidding)
    {
        skidSound.mute = !isSkidding;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 5f)
        {
            crashSound.PlayOneShot(crashSounds[Random.Range(0, crashSounds.Count)]);
        }
    }
    #endregion

}
