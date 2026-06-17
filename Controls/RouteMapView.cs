using FitnessActivityApp.Models;
using Microsoft.Maui.Graphics;
using System.Collections.Specialized;

namespace FitnessActivityApp.Controls;

public class RouteMapView : GraphicsView
{
    private readonly RouteDrawable _routeDrawable;
    private INotifyCollectionChanged? _collectionChanged;

    public static readonly BindableProperty SamplesProperty =
        BindableProperty.Create(
            nameof(Samples),
            typeof(IList<ActivitySample>),
            typeof(RouteMapView),
            default(IList<ActivitySample>),
            propertyChanged: OnSamplesChanged);

    public RouteMapView()
    {
        _routeDrawable = new RouteDrawable();
        Drawable = _routeDrawable;
        HeightRequest = 260;
    }

    public IList<ActivitySample>? Samples
    {
        get => (IList<ActivitySample>?)GetValue(SamplesProperty);
        set => SetValue(SamplesProperty, value);
    }

    private static void OnSamplesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        RouteMapView view = (RouteMapView)bindable;

        if (view._collectionChanged != null)
        {
            view._collectionChanged.CollectionChanged -= view.OnCollectionChanged;
        }

        view._collectionChanged = newValue as INotifyCollectionChanged;

        if (view._collectionChanged != null)
        {
            view._collectionChanged.CollectionChanged += view.OnCollectionChanged;
        }

        view._routeDrawable.Samples = newValue as IList<ActivitySample>;
        view.Invalidate();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _routeDrawable.Samples = Samples;
        Invalidate();
    }
}

public class RouteDrawable : IDrawable
{
    public IList<ActivitySample>? Samples { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromArgb("#1E1E1E");
        canvas.FillRoundedRectangle(dirtyRect, 20);

        List<ActivitySample> points = Samples?
            .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
            .ToList() ?? new List<ActivitySample>();

        canvas.FontColor = Colors.White;
        canvas.FontSize = 14;

        if (points.Count < 2)
        {
            canvas.DrawString(
                "Nema dovoljno GPS tačaka za crtanje rute.",
                dirtyRect,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);

            return;
        }

        float padding = 24;

        double minLat = points.Min(x => x.Latitude!.Value);
        double maxLat = points.Max(x => x.Latitude!.Value);
        double minLon = points.Min(x => x.Longitude!.Value);
        double maxLon = points.Max(x => x.Longitude!.Value);

        double latRange = maxLat - minLat;
        double lonRange = maxLon - minLon;

        if (latRange == 0)
            latRange = 0.000001;

        if (lonRange == 0)
            lonRange = 0.000001;

        float width = dirtyRect.Width - padding * 2;
        float height = dirtyRect.Height - padding * 2;

        List<PointF> drawPoints = new();

        foreach (ActivitySample sample in points)
        {
            double lat = sample.Latitude!.Value;
            double lon = sample.Longitude!.Value;

            float x = padding + (float)((lon - minLon) / lonRange * width);
            float y = padding + (float)((maxLat - lat) / latRange * height);

            drawPoints.Add(new PointF(x, y));
        }

        PathF path = new();
        path.MoveTo(drawPoints[0].X, drawPoints[0].Y);

        for (int i = 1; i < drawPoints.Count; i++)
        {
            path.LineTo(drawPoints[i].X, drawPoints[i].Y);
        }

        canvas.StrokeColor = Color.FromArgb("#4CAF50");
        canvas.StrokeSize = 5;
        canvas.DrawPath(path);

        PointF start = drawPoints.First();
        PointF end = drawPoints.Last();

        canvas.FillColor = Color.FromArgb("#2196F3");
        canvas.FillCircle(start.X, start.Y, 8);

        canvas.FillColor = Color.FromArgb("#F44336");
        canvas.FillCircle(end.X, end.Y, 8);

        canvas.FontColor = Colors.White;
        canvas.FontSize = 12;

        canvas.DrawString(
            "Start",
            start.X + 10,
            start.Y - 10,
            60,
            20,
            HorizontalAlignment.Left,
            VerticalAlignment.Center);

        canvas.DrawString(
            "End",
            end.X + 10,
            end.Y - 10,
            60,
            20,
            HorizontalAlignment.Left,
            VerticalAlignment.Center);
    }
}