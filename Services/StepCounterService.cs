using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.Content;
using Android.Hardware;
#endif

namespace FitnessActivityApp.Services;

// Ovaj servis koristi pravi Android step counter senzor.
// Ako senzor ne postoji ili nema dozvole, aplikacija koristi formulu kao fallback.
public class StepCounterService
#if ANDROID
    : Java.Lang.Object, ISensorEventListener
#endif
{
#if ANDROID
    private readonly SensorManager? _sensorManager;
    private readonly Sensor? _stepCounterSensor;

    private float? _startTotalSteps;
    private int _currentActivitySteps;
    private bool _isStarted;

    public StepCounterService()
    {
        // Uzimamo Android servis koji radi sa senzorima telefona.
        _sensorManager = Android.App.Application.Context.GetSystemService(Context.SensorService) as SensorManager;

        // Uzimamo step counter senzor.
        // Ovaj senzor vraća ukupan broj koraka od poslednjeg restartovanja telefona.
        _stepCounterSensor = _sensorManager?.GetDefaultSensor(SensorType.StepCounter);
    }

    // Da li telefon ima step counter senzor.
    public bool IsAvailable => _stepCounterSensor != null;

    // Broj koraka samo za trenutnu aktivnost.
    public int CurrentActivitySteps => _currentActivitySteps;

    // Traži dozvolu za prepoznavanje fizičke aktivnosti.
    public async Task<bool> RequestPermissionAsync()
    {
        try
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(29))
                return true;

            PermissionStatus status = await Permissions.CheckStatusAsync<ActivityRecognitionPermission>();

            if (status == PermissionStatus.Granted)
                return true;

            status = await Permissions.RequestAsync<ActivityRecognitionPermission>();

            return status == PermissionStatus.Granted;
        }
        catch
        {
            return false;
        }
    }

    // Pokreće slušanje step counter senzora.
    public bool Start()
    {
        if (_sensorManager == null || _stepCounterSensor == null)
            return false;

        if (_isStarted)
            return true;

        try
        {
            _startTotalSteps = null;
            _currentActivitySteps = 0;

            _isStarted = _sensorManager.RegisterListener(
                this,
                _stepCounterSensor,
                SensorDelay.Ui);

            return _isStarted;
        }
        catch
        {
            _isStarted = false;
            return false;
        }
    }

    // Zaustavlja slušanje step counter senzora.
    public void Stop()
    {
        if (!_isStarted)
            return;

        try
        {
            _sensorManager?.UnregisterListener(this);
        }
        catch
        {
            // Ako Android baci grešku, ne rušimo aplikaciju.
        }

        _isStarted = false;
    }

    // Resetuje brojač za novu aktivnost.
    public void Reset()
    {
        _startTotalSteps = null;
        _currentActivitySteps = 0;
    }

    // Android automatski poziva ovu metodu kada senzor pošalje novu vrednost.
    public void OnSensorChanged(SensorEvent? e)
    {
        if (e == null)
            return;

        if (e.Sensor?.Type != SensorType.StepCounter)
            return;

        float totalStepsFromReboot = e.Values[0];

        // Prva vrednost se pamti kao početak aktivnosti.
        _startTotalSteps ??= totalStepsFromReboot;

        int activitySteps = (int)Math.Round(totalStepsFromReboot - _startTotalSteps.Value);

        _currentActivitySteps = Math.Max(0, activitySteps);
    }

    // Ova metoda mora da postoji zbog Android interfejsa.
    public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
    {
    }

    // Custom permission za ACTIVITY_RECOGNITION.
    private class ActivityRecognitionPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new[]
            {
                ("android.permission.ACTIVITY_RECOGNITION", true)
            };
    }
#else
    // Fallback za druge platforme.
    public bool IsAvailable => false;

    public int CurrentActivitySteps => 0;

    public Task<bool> RequestPermissionAsync()
    {
        return Task.FromResult(false);
    }

    public bool Start()
    {
        return false;
    }

    public void Stop()
    {
    }

    public void Reset()
    {
    }
#endif
}