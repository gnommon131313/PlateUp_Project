using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private SignalBus _signalBus;

    private CinemachineVirtualCamera _virtualCamera;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;

    private float _shakeTimer;
    private float _shakeTimerBase;
    private float _intensityBase;

    private CompositeDisposable _disposable = new CompositeDisposable();

    [Inject]
    public void Construct(SignalBus signalBus) => _signalBus = signalBus;

    private void Awake()
    {
        Instance = this;
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        cinemachineBasicMultiChannelPerlin = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;

            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
                Mathf.Lerp(0, _intensityBase, _shakeTimer / _shakeTimerBase);
        }
    }

    private void OnEnable()
    {
        SubscribeOnSignals();
    }

    private void OnDisable()
    {
        _disposable.Clear();
    }

    public void Do(float intensity, float time)
    {
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

        _intensityBase = intensity;
        _shakeTimer = time;
        _shakeTimerBase = time;
    }

    private void SubscribeOnSignals()
    {
       
    }
}
