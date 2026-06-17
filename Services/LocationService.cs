using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace FitnessActivityApp.Services;

public class LocationService
{
    private bool _isReadingLocation;

    public async Task<bool> RequestLocationPermissionAsync()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return true;

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        return status == PermissionStatus.Granted;
    }

    public async Task<Location?> GetCurrentLocationAsync()
    {
        if (_isReadingLocation)
            return null;

        try
        {
            _isReadingLocation = true;

            GeolocationRequest request = new(
                GeolocationAccuracy.Best,
                TimeSpan.FromSeconds(10));

            using CancellationTokenSource cancellationTokenSource = new(
                TimeSpan.FromSeconds(12));

            Location? location = await Geolocation.Default.GetLocationAsync(
                request,
                cancellationTokenSource.Token);

            return location;
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
        catch (FeatureNotEnabledException)
        {
            return null;
        }
        catch (PermissionException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            _isReadingLocation = false;
        }
    }
}